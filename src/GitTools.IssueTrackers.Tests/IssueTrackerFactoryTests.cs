namespace GitTools.IssueTrackers.Tests
{
    using GitHub;
    using Shouldly;
    using Xunit;

    public class IssueTrackerFactoryTests
    {
        [Fact]
        public void CanDetectGitHubFromUrl()
        {
            IIssueTracker issueTracker;
            var created = IssueTrackerFactory.TryCreateIssueTrackerFromUrl("https://github.com/GitTools/GitTools.IssueTrackers", null, out issueTracker);

            created.ShouldBeTrue();
            var githubIssueTracker = issueTracker.ShouldBeOfType<GitHubIssueTracker>();
            githubIssueTracker.Organisation.ShouldBe("GitTools");
            githubIssueTracker.Repository.ShouldBe("GitTools.IssueTrackers");
        }
        [Fact]
        public void CanDetectGitHubFromRemote()
        {
            IIssueTracker issueTracker;
            var created = IssueTrackerFactory.TryCreateIssueTrackerFromUrl("git@github.com:GitTools/GitTools.IssueTrackers.git", null, out issueTracker);

            created.ShouldBeTrue();
            var githubIssueTracker = issueTracker.ShouldBeOfType<GitHubIssueTracker>();
            githubIssueTracker.Organisation.ShouldBe("GitTools");
            githubIssueTracker.Repository.ShouldBe("GitTools.IssueTrackers");
        }
    }
}