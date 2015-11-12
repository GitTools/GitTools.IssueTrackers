namespace GitTools.IssueTrackers.Jira
{
    using System;
    using System.Collections.Generic;
    using Atlassian.Jira;
    using Atlassian.Jira.Remote;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;

    public static partial class JiraExtensions
    {
        public static List<JiraIssue> GetIssues(this IJiraRestClient jiraRestClient, string jql, int startAt = 0, int maxResults = 200)
        {
            var issues = new List<JiraIssue>();

            var searchRequest = new JiraSearchRequest
            {
                Jql = jql,
                StartAt = startAt,
                MaxResults = maxResults,
            };

            //if (fields != null)
            //{
            //    searchRequest.Fields.AddRange(fields);
            //}

            var requestJson = JsonConvert.SerializeObject(searchRequest, GetJsonSettings());
            var responseJson = jiraRestClient.ExecuteRequestRaw(Method.POST, "rest/api/2/search", requestJson);

            var issuesJson = responseJson["issues"];
            foreach (var jsonElement in issuesJson.Children())
            {
                var issue = JsonConvert.DeserializeObject<JiraIssue>(jsonElement.ToString());

                var fieldsElement = jsonElement["fields"];
                if (fieldsElement != null)
                {
                    issue.summary = fieldsElement.Value<string>("summary");
                    issue.description = fieldsElement.Value<string>("description");
                    issue.created = fieldsElement.Value<DateTime?>("created");
                    issue.resolutionDate = fieldsElement.Value<DateTime?>("resolutiondate");

                    var statusElement = fieldsElement["status"];
                    if (statusElement != null)
                    {
                        issue.status = statusElement.Value<string>("id");
                    }

                    var labelsElement = fieldsElement["labels"];
                    if (labelsElement != null)
                    {
                        issue.labels = JsonConvert.DeserializeObject<List<string>>(labelsElement.ToString());
                    }

                    var fixVersionsElement = fieldsElement["fixVersions"];
                    if (fixVersionsElement != null)
                    {
                        foreach (var fixVersionElement in fixVersionsElement)
                        {
                            var fixVersion = JsonConvert.DeserializeObject<JiraFixVersion>(fixVersionElement.ToString());
                            issue.fixVersions.Add(fixVersion);
                        }
                    }
                }

                issues.Add(issue);
            }

            return issues;
        }
    }
}