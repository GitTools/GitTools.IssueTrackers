namespace GitTools.IssueTrackers.Jira
{
    public class JiraObjectBase
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string Self { get; set; }
    }
}