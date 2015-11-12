namespace GitTools.IssueTrackers.Tests
{
    using GitHub;
    using Jira;
    using Shouldly;
    using Xunit;

    public class IssueTrackerFactoryTests
    {
        [Fact]
        public void CanDetectGitHubFromUrl()
        {
            IIssueTracker issueTracker;
            var created = IssueTrackerFactory.TryCreateIssueTrackerFromUrl("https://github.com/GitTools/GitTools.IssueTrackers", null, null, out issueTracker);

            created.ShouldBeTrue();
            var githubIssueTracker = issueTracker.ShouldBeOfType<GitHubIssueTracker>();
            githubIssueTracker.Organisation.ShouldBe("GitTools");
            githubIssueTracker.Repository.ShouldBe("GitTools.IssueTrackers");
        }

        [Fact]
        public void CanDetectJiraFromUrl()
        {
            IIssueTracker issueTracker;
            var created = IssueTrackerFactory.TryCreateIssueTrackerFromUrl("https://catelproject.atlassian.com/", "CTL", null, out issueTracker);

            created.ShouldBeTrue();
            var jiraIssueTracker = issueTracker.ShouldBeOfType<JiraIssueTracker>();
            jiraIssueTracker.ShouldNotBeNull();
        }
    }
}