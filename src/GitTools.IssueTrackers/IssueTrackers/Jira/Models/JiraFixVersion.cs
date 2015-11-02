namespace GitTools.IssueTrackers.Jira
{
    using System;

    public class JiraFixVersion : JiraObjectBase
    {
        public string name { get; set; }

        public DateTime? releaseDate { get; set; }

        public bool released { get; set; }
    }
}