namespace GitTools.IssueTrackers.GitHub
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class GitHubIssueTracker : IIssueTracker
    {
        private readonly Dictionary<string, Octokit.User> _userCache = new Dictionary<string, Octokit.User>();
        private readonly Octokit.GitHubClient _gitHubClient;

        public GitHubIssueTracker(string organisation, string repository, string server, AuthSettings authenticationInfo)
        {
            Organisation = organisation;
            Repository = repository;
            var productInformation = new Octokit.ProductHeaderValue("GitReleaseNotes");
            _gitHubClient = string.IsNullOrEmpty(server) ?
                new Octokit.GitHubClient(productInformation) :
                new Octokit.GitHubClient(productInformation, new Uri(server));

            if (authenticationInfo != null)
            {
                if (authenticationInfo.IsTokenAuthentication)
                {
                    _gitHubClient.Credentials = new Octokit.Credentials(authenticationInfo.Token);
                }

                if (authenticationInfo.IsUsernameAndPasswordAuthentication)
                {
                    _gitHubClient.Credentials = new Octokit.Credentials(authenticationInfo.Username, authenticationInfo.Password);
                }
            }
        }

        public string Organisation { get; private set; }
        public string Repository { get; private set; }

        public async Task<IEnumerable<Issue>> GetIssuesAsync(IssueTrackerFilter filter)
        {
            var repositoryIssueRequest = PrepareFilter(filter);
            var forRepository = await _gitHubClient.Issue.GetAllForRepository(Organisation, Repository, repositoryIssueRequest);

            var readOnlyList = forRepository.Where(i => i.ClosedAt > filter.Since);

            return readOnlyList.Select(i => new Issue("#" + i.Number.ToString(CultureInfo.InvariantCulture))
            {
                DateClosed = i.ClosedAt,
                Url = i.HtmlUrl.ToString(),
                Title = i.Title,
                IssueType = i.PullRequest == null ? IssueType.Issue : IssueType.PullRequest,
                Labels = i.Labels.Select(l => l.Name).ToArray(),
                Contributors = i.PullRequest == null ? new Contributor[0] : new[]
                {
                    new Contributor(GetUserName(_gitHubClient, i.User), i.User.Login, i.User.HtmlUrl)
                }
            });
        }

        private string GetUserName(Octokit.GitHubClient client, Octokit.User u)
        {
            var login = u.Login;
            if (!_userCache.ContainsKey(login))
            {
                _userCache.Add(login, string.IsNullOrEmpty(u.Name) ? client.User.Get(login).Result : u);
            }

            var user = _userCache[login];
            if (user != null)
            {
                return user.Name;
            }

            return null;
        }

        private Octokit.RepositoryIssueRequest PrepareFilter(IssueTrackerFilter filter)
        {
            var repositoryIssueRequest = new Octokit.RepositoryIssueRequest
            {
                Filter = Octokit.IssueFilter.All,
                Since = filter.Since,
            };

            if (filter.IncludeOpen && filter.IncludeClosed)
            {
                repositoryIssueRequest.State = Octokit.ItemState.All;
            }
            else if (filter.IncludeOpen)
            {
                repositoryIssueRequest.State = Octokit.ItemState.Open;
            }
            else if (filter.IncludeClosed)
            {
                repositoryIssueRequest.State = Octokit.ItemState.Closed;
            }

            return repositoryIssueRequest;
        }

        public static IIssueTracker Factory(string url, string project, AuthSettings authentication)
        {
            var split = project.Split('/');
            return new GitHubIssueTracker(split[0], split[1], url, authentication);
        }

        private static readonly Regex GithubDotComRepo = new Regex("github.com[/:](?<org>.+?)/(?<repo>.+?)/?$");
        public static bool TryCreate(string url, AuthSettings authentication, out IIssueTracker issueTracker)
        {
            var urlWithoutGitExtension = url.EndsWith(".git") ? url.Substring(0, url.Length - 4) : url;
            var match = GithubDotComRepo.Match(urlWithoutGitExtension);
            if (match.Success)
            {
                issueTracker = new GitHubIssueTracker(match.Groups["org"].Value, match.Groups["repo"].Value, null, authentication);
                return true;
            }

            issueTracker = null;
            return false;
        }
    }
}