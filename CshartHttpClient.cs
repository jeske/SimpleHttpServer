using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Samples.VisualStudioOnline
{
    class Program
    {
        static String _baseUrl = "https://{0}.visualstudio.com/DefaultCollection/_apis/";

        // Get the alternate credentials that you'll use to access the Visual Studio Online account.
        static String _altUsername = "";//PromptForUsername();
        static String _altPassword = "";//PromptForPassword();

        //Your visual studio account name
        static String _account = "{account}";

        //Api version query parameter
        static String _apiVersion = "?api-version=1.0-preview.1";

        static void Main(string[] args)
        {
            RunSample();
        }

        static async void RunSample()
        {
            var responseBody = String.Empty;

            ApiCollection<BuildDefinition> buildDefinitions;
            BuildRequest queueResult;
            Build buildResult;

            _baseUrl = String.Format(_baseUrl, _account);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //Set alternate credentials
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", _altUsername, _altPassword))));

                Console.WriteLine("==============Getting a list Build Definitions=================");

                //Get a list of build definitions
                responseBody = await GetAsync(client, _baseUrl + "build/definitions");

                buildDefinitions = JsonConvert.DeserializeObject<ApiCollection<BuildDefinition>>(responseBody);

                if (buildDefinitions.Value.Count() > 0)
                {
                    foreach (var buildDefinition in buildDefinitions.Value)
                    {
                        Console.WriteLine(String.Format(
                            "{0} - {1}",
                            buildDefinition.Id,
                            buildDefinition.Name
                            ));
                    }

                    Console.WriteLine("==============Queueing a Build=================");

                    //Use the first build definition to queue a build
                    var firstBuildDefinition = buildDefinitions.Value.First<BuildDefinition>();

                    Console.WriteLine(String.Format(
                        "Queuing a build using build definition {0}...",
                        firstBuildDefinition.Id
                        ));

                    //Queue a build
                    var buildRequestPOSTData =
                        new BuildRequest()
                        {
                            Definition = new Definition()
                            {
                                Id = firstBuildDefinition.Id
                            },
                            Priority = Priority.Normal,
                            Reason = Reason.Manual

                        };

                    responseBody = await QueueBuildAsync(client, buildRequestPOSTData, _baseUrl + "build/requests");

                    queueResult = JsonConvert.DeserializeObject<BuildRequest>(responseBody);

                    Console.WriteLine(String.Format(
                        "Build request submitted successfully - ID: {0}",
                        queueResult.Id
                        ));

                    Console.WriteLine("==============Waiting for Build to Start Running=================");

                    //poll the server till the build is out of the queue
                    while (!String.IsNullOrEmpty(queueResult.Status) &&
                          queueResult.Status == "queued")
                    {
                        //Wait a while before polling the server
                        Thread.Sleep(8000);

                        Console.WriteLine("Polling the server...");

                        responseBody = await GetAsync(client,
                            String.Format(queueResult.Url,
                            queueResult.Id
                            ));

                        queueResult = JsonConvert.DeserializeObject<BuildRequest>(responseBody);

                        Console.WriteLine(String.Format("Build queue status: {0}", queueResult.Status));
                    }

                    Console.WriteLine("==============Waiting for Build to Complete=================");

                    //Get the status of the build we queued
                    responseBody = await GetAsync(client,
                            String.Format(queueResult.Builds.First<Build>().Url,
                            queueResult.Builds.First<Build>().Id
                            ));

                    buildResult = JsonConvert.DeserializeObject<Build>(responseBody);

                    //Poll until the build either fails or succeeds
                    while (buildResult.Status == BuildStatus.NotStarted
                           || buildResult.Status == BuildStatus.InProgress)
                    {
                        //Wait a while before polling the server
                        Thread.Sleep(8000);

                        //Get a Build 
                        responseBody = await GetAsync(client,
                                String.Format(buildResult.Url,
                                queueResult.Builds.First<Build>().Id
                                ));

                        buildResult = JsonConvert.DeserializeObject<Build>(responseBody);

                        Console.WriteLine(String.Format(
                            "Build {0}: {1}",
                            buildResult.Id,
                            buildResult.Status
                            ));
                    }

                    if (buildResult.Status == BuildStatus.Succeeded)
                    {
                        Console.WriteLine(String.Format(
                                "Build {0} drop: {1}  ",
                                buildResult.Id,
                                buildResult.BuildDrop.Url
                                ));
                    }
                }
                else
                {
                    Console.WriteLine("No build definitions found....");
                }

            }

            Console.WriteLine("Done...");

            Console.ReadLine();

        }

        static async Task<String> GetAsync(HttpClient client, String apiUrl)
        {
            var responseBody = String.Empty;

            try
            {
                using (HttpResponseMessage response = client.GetAsync(apiUrl + _apiVersion).Result)
                {
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return responseBody;
        }

        static async Task<String> QueueBuildAsync(HttpClient client,
                                          BuildRequest data,
                                          String apiUrl)
        {
            var responseBody = String.Empty;

            var temp = JsonConvert.SerializeObject(data);

            var content = new StringContent(
                JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json");

            try
            {
                using (HttpResponseMessage response = client.PostAsync(apiUrl + _apiVersion, content).Result)
                {
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return responseBody;
        }

    }

    public class Build
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public String Status { get; set; }

        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "drop")]
        public Drop BuildDrop { get; set; }

        [JsonProperty(PropertyName = "url")]
        public String Url { get; set; }
    }

    public class Drop
    {
        [JsonProperty(PropertyName = "location")]
        public String Location { get; set; }

        [JsonProperty(PropertyName = "type")]
        public String type { get; set; }

        [JsonProperty(PropertyName = "url")]
        public String Url { get; set; }
    }

    public class BuildDefinition
    {

        [JsonProperty(PropertyName = "uri")]
        public String Uri { get; set; }

        [JsonProperty(PropertyName = "queue")]
        public Queue Queue { get; set; }

        [JsonProperty(PropertyName = "triggerType")]
        public String TriggerType { get; set; }

        [JsonProperty(PropertyName = "defaultDropLocation")]
        public String DefaultDropLocation { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "definitionType")]
        public String DefinitionType { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public String Url { get; set; }
    }

    public class BuildRequest
    {
        [JsonProperty(PropertyName = "definition")]
        public Definition Definition { get; set; }

        [JsonProperty(PropertyName = "reason")]
        public String Reason { get; set; }

        [JsonProperty(PropertyName = "priority")]
        public String Priority { get; set; }

        [JsonProperty(PropertyName = "queuePosition")]
        public int QueuePosition { get; set; }

        [JsonProperty(PropertyName = "queueTime")]
        public DateTime QueueTime { get; set; }

        [JsonProperty(PropertyName = "requestedBy")]
        public RequestedBy RequestedBy { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public String Status { get; set; }

        [JsonProperty(PropertyName = "url")]
        public String Url { get; set; }

        [JsonProperty(PropertyName = "builds")]
        public IEnumerable<Build> Builds { get; set; }

    }

    public class RequestedBy
    {
        [JsonProperty(PropertyName = "displayName")]
        public String DisplayName { get; set; }

        [JsonProperty(PropertyName = "uniqueName")]
        public String UniqueName { get; set; }

    }

    public class Definition
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }

    public class ApiCollection<T>
    {
        [JsonProperty(PropertyName = "value")]
        public IEnumerable<T> Value { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }
    }

    public class Queue
    {
        [JsonProperty(PropertyName = "queueType")]
        public String QueueType { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "url")]
        public String Url;

    }

    public static class Priority
    {
        public const String Normal = "Normal";
        public const String AboveNormal = "AboveNormal";
        public const String BelowNormal = "BelowNormal";
        public const String High = "High";
        public const String Low = "Low";
    }

    public static class Reason
    {
        public const String BatchedCI = "BatchedCI";
        public const String CheckInShelveset = "CheckInShelveset";
        public const String IndividualCI = "IndividualCI";
        public const String Manual = "Manual";
        public const String None = "None";
        public const String Schedule = "Schedule";
        public const String ScheduleForced = "ScheduleForced";
        public const String Triggered = "Triggered";
        public const String UserCreated = "UserCreated";
        public const String ValidateShelveset = "ValidateShelveset";
        public const String HasFlag = "HasFlag";
    }

    public static class BuildStatus
    {
        public const String NotStarted = "notStarted";
        public const String InProgress = "inProgress";
        public const String Succeeded = "succeeded";
    }
}
