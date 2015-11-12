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
        delegate IIssueTracker IssueTrackerFactoryDelegate(string url, string project, AuthSettings authentication);
        delegate bool TryCreateIssueTrackerDelegate(string url, string project, AuthSettings authentication, out IIssueTracker issueTracker);
        static readonly Dictionary<IssueTrackerType, IssueTrackerFactoryDelegate> Factories = new Dictionary<IssueTrackerType, IssueTrackerFactoryDelegate>
        {
            { IssueTrackerType.GitHub, GitHubIssueTracker.Factory },
            { IssueTrackerType.Jira, JiraIssueTracker.Factory }
        };
        static readonly TryCreateIssueTrackerDelegate[] TryCreateFactories =
        {
            GitHubIssueTracker.TryCreate,
            JiraIssueTracker.TryCreate,
        };

        /// <summary>
        /// Creates an issue tracker based off the settings
        /// </summary>
        /// <returns>The issue tracker API</returns>
        [Pure]
        public static IIssueTracker CreateIssueTracker(IssueTrackerSettings issueTrackerSettings)
        {
            if (!Factories.ContainsKey(issueTrackerSettings.IssueTrackerType))
            {
                throw new ArgumentOutOfRangeException("issueTrackerSettings.IssueTrackerType", issueTrackerSettings.IssueTrackerType, "This issue tracker is not supported yet!");
            }

            return Factories[issueTrackerSettings.IssueTrackerType](issueTrackerSettings.Url, issueTrackerSettings.Project, issueTrackerSettings.Authentication);
        }

        /// <summary>
        /// Useful for hosted services with well known urls, for instance TryCreateIssueTrackerFromUrl("github.com/gittools/gittools.issuetracker") will work fine
        /// </summary>
        /// <param name="url">Url for project, can be git url or http url</param>
        /// <param name="project">The project name.</param>
        /// <param name="authentication">Authentication information</param>
        /// <param name="issueTracker">The issue tracker if successful</param>
        /// <returns>True on success</returns>
        public static bool TryCreateIssueTrackerFromUrl(string url, [CanBeNull] string project, [CanBeNull] AuthSettings authentication, out IIssueTracker issueTracker)
        {
            foreach (var tryCreateFactory in TryCreateFactories)
            {
                IIssueTracker createdIssueTracker;
                if (!tryCreateFactory(url, project, authentication, out createdIssueTracker))
                {
                    continue;
                }

                issueTracker = createdIssueTracker;
                return true;
            }

            issueTracker = null;
            return false;
        }
    }
}