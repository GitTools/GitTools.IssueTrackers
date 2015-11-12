namespace GitTools.IssueTrackers.Jira
{
    using System;
    using System.Collections.Generic;

    public class JiraIssue : JiraObjectBase
    {
        public JiraIssue()
        {
            fixVersions = new List<JiraFixVersion>();
            labels = new List<string>();
        }

        public string key { get; set; }

        public string summary { get; set; }

        public string description { get; set; }

        public DateTime? resolutionDate { get; set; }

        public DateTime? created { get; set; }

        public string status { get; set; }

        public List<JiraFixVersion>  fixVersions { get; set; }

        public List<string> labels { get; set; } 
    }
}