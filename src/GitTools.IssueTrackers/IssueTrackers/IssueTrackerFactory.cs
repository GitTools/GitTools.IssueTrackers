namespace GitTools.IssueTrackers
{
    using System;
    using System.Collections.Generic;
    using GitHub;
    using JetBrains.Annotations;
    using Jira;

    /// <summary>
    /// Entry point into GitTools.IssueTracker
    /// </summary>
    public static class IssueTrackerFactory
    {
        delegate IIssueTracker IssueTrackerFactoryDelegate(string url, string project, AuthenticationContext authentication);
        delegate bool TryCreateIssueTrackerDelegate(string url, AuthenticationContext authentication, out IIssueTracker issueTracker);
        static readonly Dictionary<IssueTrackerType, IssueTrackerFactoryDelegate> Factories = new Dictionary<IssueTrackerType, IssueTrackerFactoryDelegate>
        {
            { IssueTrackerType.GitHub, GitHubIssueTracker.Factory },
            { IssueTrackerType.Jira, JiraIssueTracker.Factory }
        };
        static readonly TryCreateIssueTrackerDelegate[] TryCreateFactories = { GitHubIssueTracker.TryCreate };

        /// <summary>
        /// Creates an issue tracker
        /// </summary>
        /// <param name="url">Can be base server url if identifier is parsed</param>
        /// <param name="project">If url is a base/server url, this can be the project id/repository/other identifier</param>
        /// <param name="issueTrackerType">The type of the issue tracker</param>
        /// <param name="authentication">Authentication information</param>
        /// <returns>The issue tracker API</returns>
        [Pure]
        public static IIssueTracker CreateIssueTracker(string url, [CanBeNull] string project, IssueTrackerType issueTrackerType, AuthenticationContext authentication = null)
        {
            if (!Factories.ContainsKey(issueTrackerType))
                throw new ArgumentOutOfRangeException("issueTrackerType", issueTrackerType, "This issue tracker is not supported yet!");

            return Factories[issueTrackerType](url, project, authentication);
        }

        /// <summary>
        /// Useful for hosted services with well known urls, for instance TryCreateIssueTrackerFromUrl("github.com/gittools/gittools.issuetracker") will work fine
        /// </summary>
        /// <param name="url">Url for project, can be git url or http url</param>
        /// <param name="authentication">Authentication information</param>
        /// <param name="issueTracker">The issue tracker if successful</param>
        /// <returns>True on success</returns>
        public static bool TryCreateIssueTrackerFromUrl(string url, [CanBeNull] AuthenticationContext authentication, out IIssueTracker issueTracker)
        {
            foreach (var tryCreateFactory in TryCreateFactories)
            {
                IIssueTracker createdIssueTracker;
                if (!tryCreateFactory(url, authentication, out createdIssueTracker)) continue;

                issueTracker = createdIssueTracker;
                return true;
            }
            issueTracker = null;
            return false;
        }
    }
}