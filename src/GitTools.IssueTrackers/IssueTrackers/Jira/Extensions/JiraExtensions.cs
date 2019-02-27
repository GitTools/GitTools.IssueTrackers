namespace GitTools.IssueTrackers.Jira
{
    using System.Net;
    using System.Threading.Tasks;
    using Atlassian.Jira;
    using Atlassian.Jira.Remote;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using RestSharp;

    public static partial class JiraExtensions
    {
        private static JsonSerializerSettings GetJsonSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-dd"
            };
        }

        public static async Task<JToken> ExecuteRequestRaw(this IJiraRestClient jiraRestClient, Method method, string resource, string jsonRequestBody)
        {
            var restRequest = new RestRequest
            {
                Resource = resource,
                Method = method,
                RequestFormat = DataFormat.Json,
            };

            restRequest.AddParameter(new Parameter
            {
                Name = "application/json",
                Type = ParameterType.RequestBody,
                Value = jsonRequestBody
            });

            var response = await jiraRestClient.ExecuteRequestAsync(restRequest);
            return response.StatusCode != HttpStatusCode.NoContent ? JToken.Parse(response.Content) : new JObject();
        }
    }
}