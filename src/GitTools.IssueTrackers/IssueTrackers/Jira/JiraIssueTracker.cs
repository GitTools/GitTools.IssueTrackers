namespace GitTools.IssueTrackers.Jira
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Jira = Atlassian.Jira;
    using Logging;
    using Version = Version;

    public class JiraIssueTracker : IIssueTracker
    {
        private static readonly HashSet<string> KnownClosedStatuses = new HashSet<string>(new[] { "resolved", "closed", "done" });
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        private readonly string _project;
        private readonly string _server;
        private readonly AuthSettings _authenticationInfo;

        public JiraIssueTracker(string server, string project, AuthSettings authenticationInfo)
        {
            _server = server;
            _project = project;
            _authenticationInfo = authenticationInfo ?? new AuthSettings();
        }

        public async Task<IEnumerable<Issue>> GetIssuesAsync(IssueTrackerFilter filter)
        {
            Log.DebugFormat("Connecting to Jira server '{0}'", _server);

            var jira = Atlassian.Jira.Jira.CreateRestClient(_server, _authenticationInfo.Username, 
                _authenticationInfo.Password, new Jira.JiraRestClientSettings());

            var jiraRestClient = jira.RestClient;

            Log.Debug("Retrieving statuses");

            var statuses = (await jiraRestClient.GetIssueStatusesAsync(CancellationToken.None)).ToList();

            var openedStatuses = GetOpenedStatuses(statuses);
            var closedStatuses = GetClosedStatuses(statuses);

            var finalFilter = PrepareFilter(filter, openedStatuses, closedStatuses);

            var issues = new List<Issue>();

            Log.DebugFormat("Searching for issues using filter '{0}'", filter);

            // TODO: Once the Atlassian.Sdk issue type contains all info, remove custom JiraIssue
            var retrievedIssues = jiraRestClient.GetIssues(finalFilter, 0, 200);
            //var retrievedIssues = await jiraRestClient.GetIssuesFromJqlAsync(finalFilter, 200, 0, CancellationToken.None);
            foreach (var issue in retrievedIssues)
            {
                var gitIssue = new Issue(issue.key)
                {
                    IssueType = IssueType.Issue,
                    Title = issue.summary,
                    Description = issue.description,
                    DateCreated = issue.created,
                    Labels = issue.labels.ToArray()
                };

                if (closedStatuses.Any(x => string.Equals(x.Id, issue.status)))
                {
                    gitIssue.DateClosed = issue.resolutionDate;
                }

                foreach (var fixVersion in issue.fixVersions)
                {
                    gitIssue.FixVersions.Add(new Version
                    {
                        Name = fixVersion.name,
                        ReleaseDate = fixVersion.releaseDate,
                        IsReleased = fixVersion.released
                    });
                }

                var uri = new Uri(new Uri(_server), string.Format("/browse/{0}", issue.key));
                gitIssue.Url = uri.ToString();

                issues.Add(gitIssue);
            }

            Log.DebugFormat("Found '{0}' issues using filter '{1}'", issues.Count, filter);

            return issues.AsEnumerable();
        }

        private string PrepareFilter(IssueTrackerFilter filter, IEnumerable<Jira.IssueStatus> openedStatuses, IEnumerable<Jira.IssueStatus> closedStatuses)
        {
            var finalFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(filter.Filter))
            {
                finalFilter = filter.Filter;
            }

            if (filter.IncludeOpen && filter.IncludeClosed)
            {
                // no need to filter anything
            }
            else if (!filter.IncludeOpen && !filter.IncludeClosed)
            {
                throw new Exception("Cannot exclude both open and closed issues, nothing will be returned");
            }
            else if (filter.IncludeOpen)
            {
                finalFilter = finalFilter.AddJiraFilter(string.Format("status in ({0})", string.Join(", ", openedStatuses.Select(x => string.Format("\"{0}\"", x)))));
            }
            else if (filter.IncludeClosed)
            {
                finalFilter = finalFilter.AddJiraFilter(string.Format("status in ({0})", string.Join(", ", closedStatuses.Select(x => string.Format("\"{0}\"", x)))));
            }

            if (filter.Since.HasValue)
            {
                finalFilter = finalFilter.AddJiraFilter(string.Format("resolutiondate >= '{0}'", filter.Since.Value.ToString("yyyy-MM-dd")));
            }

            finalFilter = finalFilter.AddJiraFilter(string.Format("project = {0}", _project));

            return finalFilter;
        }

        private List<Jira.IssueStatus> GetOpenedStatuses(List<Jira.IssueStatus> statuses)
        {
            return (from issueStatus in statuses
                    where !KnownClosedStatuses.Contains(issueStatus.Name.ToLower())
                    select issueStatus).ToList();
        }

        private List<Jira.IssueStatus> GetClosedStatuses(List<Jira.IssueStatus> statuses)
        {
            return (from issueStatus in statuses
                    where KnownClosedStatuses.Contains(issueStatus.Name.ToLower())
                    select issueStatus).ToList();
        }

        public static IIssueTracker Factory(string url, string project, AuthSettings authentication)
        {
            return new JiraIssueTracker(url, project, authentication);
        }

        public static bool TryCreate(string url, string project, AuthSettings authentication, out IIssueTracker issueTracker)
        {
            // For now just check if it contains atlassian.com
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (url.ToLower().Contains(".atlassian.com"))
                {
                    issueTracker = new JiraIssueTracker(url, project, authentication);
                    return true;
                }
            }

            issueTracker = null;
            return false;
        }
    }
}