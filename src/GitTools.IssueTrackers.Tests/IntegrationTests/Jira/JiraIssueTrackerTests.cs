namespace GitTools.IssueTrackers.Tests.IntegrationTests.Jira
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IssueTrackers.Jira;
    using Shouldly;
    using Xunit;

    public class JiraIssueTrackerTests
    {
        [Fact]
        public async Task ReturnsIssuesUsingRest()
        {
            var issueTracker = new JiraIssueTracker("https://catelproject.atlassian.net/", "CTL", null);
            var issueFilter = new IssueTrackerFilter
            {
                IncludeClosed = true,
                Since = new DateTimeOffset(DateTime.Today.AddMonths(-1))
            };

            var issues = await issueTracker.GetIssuesAsync(issueFilter);
            var issuesList = issues.ToList();

            issuesList.ShouldNotBeEmpty();
        }
    }
}