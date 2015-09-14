namespace GitTools.IssueTrackers
{
    using GitHub;

    /// <summary>
    ///     Arguments class for creating an issue tracker
    /// </summary>
    public class IssueTrackerSettings
    {
        public IssueTrackerSettings(string url, IssueTrackerType issueTrackerType)
        {
            Url = url;
            IssueTrackerType = issueTrackerType;
        }

        /// <summary>
        /// Url for issue tracker, can be a base url and Project is specified or a fully qualified url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Optional project id, repo name or other identifier for the project. Required if the Url is just the base url for the server
        /// </summary>
        public string Project { get; set; }

        public IssueTrackerType IssueTrackerType { get; set; }

        public AuthSettings Authentication { get; set; }
    }
}