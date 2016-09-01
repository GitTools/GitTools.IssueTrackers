namespace GitTools.IssueTrackers.Tests.IntegrationTests.Bitbucket
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Shouldly;
    using Xunit;
    using IssueTrackers.Bitbucket;

    public class BitbucketIssueTrackerTests
    {
        [Fact]
        public async Task ReturnsIssuesUsingRest()
        {
            var issueTracker = new BitbucketIssueTracker("MatteoLocher", "gittools.testrepo", null);
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