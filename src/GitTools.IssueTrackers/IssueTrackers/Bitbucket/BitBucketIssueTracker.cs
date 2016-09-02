
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace GitTools.IssueTrackers.Bitbucket
{
    public class BitbucketIssueTracker : IIssueTracker
    {
        const int MaxIssues = 15; // default value for bitbucket api
        const string ApiUrl = "https://bitbucket.org/api/1.0/";
        const string openedStatuses = "&status=new&status=open";
        const string closedStatuses = "&status=closed&status=resolved";
        const string IssuesUrl = "repositories/{0}/{1}/issues?start={2}{3}"; //0: account name; 1: repository name; 2: start page (pagination); 3: filter (default is empty - gets all issues open and closed)

        AuthSettings _authenticationSettings;

        public string AccountName { get; private set; }
        public string Repository { get; private set; }

        public BitbucketIssueTracker(string accountName, string repository, AuthSettings authenticationSettings)
        {
            AccountName = accountName;
            Repository = repository;
            _authenticationSettings = authenticationSettings ?? new AuthSettings();
        }

        public async Task<IEnumerable<Issue>> GetIssuesAsync(IssueTrackerFilter filter)
        {
            return await Task.Run(() =>
            {
                var finalFilter = PrepareFilter(filter);                
                var issuesUrl = string.Format(IssuesUrl, AccountName, Repository, 0, finalFilter);

                var issues = new List<Issue>();
                issues.AddRange(GetIssues(issuesUrl));

                int lastRetrievedIssuesCount = issues.Count;

                while (lastRetrievedIssuesCount % MaxIssues == 0)
                {
                    issuesUrl = string.Format(IssuesUrl, AccountName, Repository, lastRetrievedIssuesCount, finalFilter);
                    var newlyRetrievedIssues = GetIssues(issuesUrl);

                    if (newlyRetrievedIssues.Count == 0)
                    {
                        break;
                    }

                    issues.AddRange(newlyRetrievedIssues);

                    lastRetrievedIssuesCount = issues.Count;
                }

                return issues;
            });
        }

        private List<Issue> GetIssues(string issuesUrl)
        {
            var baseUrl = new Uri(ApiUrl, UriKind.Absolute);
            var restClient = new RestClient(baseUrl.AbsoluteUri);
            var request = new RestRequest(issuesUrl);
            
            if (_authenticationSettings.IsUsernameAndPasswordAuthentication)
                GenerateClassicalRequest(request, issuesUrl);

            var response = restClient.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK) { throw new Exception("Failed to query BitBucket: " + response.StatusDescription); }

            dynamic responseObject = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return ParseIssues(responseObject);
        }

        private string PrepareFilter(IssueTrackerFilter filter)
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
                finalFilter += openedStatuses;
            }
            else if (filter.IncludeClosed)
            {
                finalFilter += closedStatuses;
            }

            return finalFilter;
        }

        private List<Issue> ParseIssues(dynamic responseObject)
        {
            var baseUrl = new Uri(ApiUrl, UriKind.Absolute);
            var issues = new List<Issue>();

            foreach (var issue in responseObject.issues)
            {
                DateTimeOffset lastChange = DateTimeOffset.Parse(issue.utc_last_updated.ToString());
                
                issues.Add(new Issue(issue.local_id.ToString())
                {
                    IssueType = IssueType.Issue,
                    Title = issue.title,
                    Description = issue.content,
                    DateClosed = lastChange,
                    Url = new Uri(baseUrl, string.Format("/repositories/{0}/{1}/issue/{2}/{3}", AccountName, Repository, issue.local_id.ToString(), issue.title)).ToString()
                });
            }

            return issues;
        }

        private string PerpareFilter(IssueTrackerFilter filter)
        {
            var finalFilter = string.Empty;

            if (!string.IsNullOrWhiteSpace(filter.Filter))
            {
                finalFilter = filter.Filter;
            }
            if (filter.IncludeOpen && filter.IncludeClosed)
            {
                finalFilter += "status=open&status=new";
            }

            return finalFilter;
        }

        private void GenerateClassicalRequest(RestRequest request, string methodLocation)
        {
            var usernameAndPass = string.Format("{0}:{1}", _authenticationSettings.Username, _authenticationSettings.Password);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(usernameAndPass));
            request.Resource = string.Format(methodLocation);
            request.AddHeader("Authorization", string.Format("Basic {0}", token));
        }

        private static readonly Regex BitbucketRepo = new Regex("bitbucket.org[/:](?<account>.+?)/(?<repository>.+?)/?$");
        internal static bool TryCreate(string url, string project, AuthSettings authentication, out IIssueTracker issueTracker)
        {
            var urlWithoutGitExtension = url.EndsWith(".git") ? url.Substring(0, url.Length - 4) : url;
            var match = BitbucketRepo.Match(urlWithoutGitExtension);
            if (match.Success)
            {
                issueTracker = new BitbucketIssueTracker(match.Groups["account"].Value, match.Groups["repository"].Value, authentication);
                return true;
            }

            issueTracker = null;
            return false;
        }
    }
}
