namespace GitTools.IssueTrackers.Jira
{
    internal static class StringExtensions
    {
        public static string AddJiraFilter(this string value, string additionalValue, string joinText = "AND")
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                value += string.Format(" {0} ", joinText);
            }
            else
            {
                value = string.Empty;
            }

            value += additionalValue;

            return value;
        }
    }
}