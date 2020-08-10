using ImplioTeamsSetup.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ImplioTeamsSetup.Entities;
using System.Data;
using System.Text;
using ImplioTeamsSetup.Entities.Interfaces;

namespace ImplioTeamsSetup.TwilioSpecific
{
    public sealed class TwilioClient
    {
        private const string ImplioApiBaseUrl = "https://web-services.implio.com/sites/";
        private const string AuthHeaderKey    = "auth";
        private const string RulesEndpoint    = "filters";
        private const string ListsEndpoint    = "dictionaries";
        

        private readonly HttpClient _httpClient;
        private readonly string _rulesRequestUrlTemplate = $"{ImplioApiBaseUrl}{{0}}/{RulesEndpoint}";
        private readonly string _listsRequestUrlTemplate = $"{ImplioApiBaseUrl}{{0}}/{RulesEndpoint}/{ListsEndpoint}";

        private TwilioClient(string authToken)
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Add(AuthHeaderKey, authToken);
        }

        public static TwilioClient Create(string authToken)
        {
            if (!String.IsNullOrWhiteSpace(authToken)) return new TwilioClient(authToken);

            Console.WriteLine("Authentication token for Implio account has not been provided. Exiting...");
            
            return null;
        }

        public async Task<RetrievalResult<RuleEntity>> RetrieveRulesAsync(string domainGuid)
        {
            if (String.IsNullOrWhiteSpace(domainGuid))
            {
                Console.WriteLine("GUID of the team to retrieve rules from has not been provided. Exiting...");
                return null;
            }
                
            HttpResponseMessage rulesRetriivalResponseMessage = await _httpClient.GetAsync(String.Format(_rulesRequestUrlTemplate, domainGuid));

            return await RetrieveEntitiesAsync<RuleEntity>(String.Format(_rulesRequestUrlTemplate, domainGuid));
        }

        public async Task<RetrievalResult<ListEntity>> RetrieveListsAsync(string domainGuid)
        {
            if (String.IsNullOrWhiteSpace(domainGuid))
            {
                Console.WriteLine("GUID of the team to retrieve rules from has not been provided. Exiting...");
                return null;
            }

            return await RetrieveEntitiesAsync<ListEntity>(String.Format(_listsRequestUrlTemplate, domainGuid));
        }

        public async Task<int> BulkDeleteRulesAsync(IEnumerable<RuleEntity> rules, bool onlyDisabled = true)
        {
            var rulesToDelete = onlyDisabled ? rules.Where(r => r.Enabled == false) : rules;

            return await BulkDeleteEntitiesAsync(rulesToDelete, _rulesRequestUrlTemplate);
        }

        public async Task<int> DeleteAllRulesAsync(string domainGuid, bool onlyDisabled = true)
        {
            RetrievalResult<RuleEntity> rulesToDelete = await RetrieveRulesAsync(domainGuid);

            return await BulkDeleteRulesAsync(rulesToDelete?.Results, onlyDisabled);
        }

        public async Task<int> BulkDeleteListsAsync(IEnumerable<ListEntity> lists)
        {
            return await BulkDeleteEntitiesAsync<ListEntity>(lists, _listsRequestUrlTemplate);
        }

        public async Task<int> DeleteAllListsAsync(string domainGuid)
        {
            RetrievalResult<ListEntity> listsToDelete = await RetrieveListsAsync(domainGuid);

            return await BulkDeleteListsAsync(listsToDelete?.Results);
        }

        public async Task<int> CopyListsAsync(string domainGuidFrom, string domainGuidTo)
        {
            if(String.IsNullOrWhiteSpace(domainGuidFrom) || String.IsNullOrWhiteSpace(domainGuidTo))
            {
                Console.WriteLine("GUID of the team to retrieve lists from or GUID of the team to copy lists to has not been provided. Exiting...");
                return 0;
            }

            RetrievalResult<ListEntity> listsToCopy = await RetrieveListsAsync(domainGuidFrom);

            if (listsToCopy == null || !listsToCopy.Results.Any())
            {
                Console.WriteLine($"No lists found in the Team with the GUID [{domainGuidFrom}]. Exiting...");
                return 0;
            }

            List<string> listsJsons = new List<string>();

            Console.WriteLine($"Retrieving lists to copy from the Team with the GUID [{domainGuidFrom}]...");

            foreach (ListEntity list in listsToCopy.Results)
            {
                HttpResponseMessage retrievalResponseMessage = await _httpClient.GetAsync($"{String.Format(_listsRequestUrlTemplate, domainGuidFrom)}/{list.Id}");

                if (retrievalResponseMessage.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"Failed to retrieve the list [{list.Id}] from the Implio Team.");
                    continue;
                }

                listsJsons.Add(await retrievalResponseMessage.Content.ReadAsStringAsync());

                Console.WriteLine($"List [{list.Id}] retrieved.");
            }

            if (!listsJsons.Any())
            {
                Console.WriteLine($"No lists retrieved from the Team with the GUID [{domainGuidFrom}]. Exiting...");
                return 0;
            }

            Console.WriteLine($"Copying lists to the Team with the GUID [{domainGuidTo}]...");

            int listsCopied = 0;

            foreach (string json in listsJsons)
            {
                StringContent newListContent = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage creationResponseMessage = await _httpClient.PostAsync(String.Format(_listsRequestUrlTemplate, domainGuidTo), newListContent);

                if (creationResponseMessage.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"\r\nFailed to create the list with the JSON [{json}].\r\nResponse code [{creationResponseMessage.StatusCode}].\r\nResponse message [{creationResponseMessage.ReasonPhrase}].");
                    continue;
                }

                listsCopied++;

                Console.Write(".");
            }

            Console.WriteLine($"\r\nCopying of the lists done. Copied {listsCopied} lists out of {listsJsons.Count}.");

            return listsCopied;
        }

        // TODO: remove copy-paste code and create proper methods.
        public async Task<int> CopyRulesAsync(string domainGuidFrom, string domainGuidTo, bool onlyEnabled = true)
        {
            if (String.IsNullOrWhiteSpace(domainGuidFrom) || String.IsNullOrWhiteSpace(domainGuidTo))
            {
                Console.WriteLine("GUID of the team to retrieve rules from or GUID of the team to copy rules to has not been provided. Exiting...");
                return 0;
            }

            RetrievalResult<RuleEntity> rulesRetrieved = await RetrieveRulesAsync(domainGuidFrom);

            if (rulesRetrieved == null || !rulesRetrieved.Results.Any())
            {
                Console.WriteLine($"No rules found in the Team with the GUID [{domainGuidFrom}]. Exiting...");
                return 0;
            }

            List<string> rulesJsons = new List<string>();

            Console.WriteLine($"Retrieving rules to copy from the Team with the GUID [{domainGuidFrom}]...");

            var rulesToCopy = onlyEnabled ? rulesRetrieved.Results.Where(r => r.Enabled) : rulesRetrieved.Results; 

            foreach (RuleEntity rule in rulesToCopy)
            {
                HttpResponseMessage retrievalResponseMessage = await _httpClient.GetAsync($"{String.Format(_rulesRequestUrlTemplate, domainGuidFrom)}/{rule.Id}");

                if (retrievalResponseMessage.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"Failed to retrieve the rule [{rule.Name}] from the Implio Team.");
                    continue;
                }

                string json = await retrievalResponseMessage.Content.ReadAsStringAsync();

                rulesJsons.Add(json.Replace(@"""notice"":{""enabled"":false},", ""));

                Console.WriteLine($"Rule [{rule.Name}] retrieved.");
            }

            if (!rulesJsons.Any())
            {
                Console.WriteLine($"No rules retrieved from the Team with the GUID [{domainGuidFrom}]. Exiting...");
                return 0;
            }

            Console.WriteLine($"Copying rules to the Team with the GUID [{domainGuidTo}]...");

            int rulesCopied = 0;

            foreach (string json in rulesJsons)
            {
                StringContent newRuleContent = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage creationResponseMessage = await _httpClient.PostAsync(String.Format(_rulesRequestUrlTemplate, domainGuidTo), newRuleContent);

                if (creationResponseMessage.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"\r\nFailed to create the rule with the JSON [{json}].\r\nResponse code [{creationResponseMessage.StatusCode}].\r\nResponse message [{creationResponseMessage.ReasonPhrase}].");
                    continue;
                }

                rulesCopied++;

                Console.Write(".");
            }

            Console.WriteLine($"\r\nCopying of the rules done. Copied {rulesCopied} lists out of {rulesJsons.Count}.");

            return rulesCopied;
        }

        private async Task<RetrievalResult<T>> RetrieveEntitiesAsync<T>(string url) where T : class, new()
        {

            HttpResponseMessage retrievalResponseMessage = await _httpClient.GetAsync(url);

            if (retrievalResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Failed to retrieve the entity {typeof(T)} from the Implio Team. Exiting...");
                return null;
            }

            string contentJson = await retrievalResponseMessage.Content.ReadAsStringAsync();

            if (String.IsNullOrWhiteSpace(contentJson))
            {
                Console.WriteLine("Failed to read response content while retrieving the entity {typeof(T)}. Exiting...");
                return null;
            }

            var retrievalResult = JsonConvert.DeserializeObject<RetrievalResult<T>>(contentJson);

            if (retrievalResult == null)
            {
                Console.WriteLine("Failed to deserilaise the response from Implio. Exiting...");
                return null;
            }

            if (!retrievalResult.Results.Any())
                Console.WriteLine($"No entities {typeof(T)} found in the Team.");

            return retrievalResult;
        }

        private async Task<int> BulkDeleteEntitiesAsync<T>(IEnumerable<T> entities, string entityUrl) where T : IEntity
        {
            if (entities == null)
            {
                Console.WriteLine("No entites provided for deletion. Exiting...");
                return 0;
            }

            int deletedEntitesCount    = 0;
            int processedEntitiesCount = 0;

            foreach (T entity in entities)
            {
                HttpResponseMessage deleteResponse = await _httpClient.DeleteAsync($"{String.Format(entityUrl, entity.Domain)}/{entity.Id}");

                if (deleteResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    Console.WriteLine($"Entity with ID [{entity.Id}] deleted.");
                    deletedEntitesCount++;
                }
                else
                    Console.WriteLine($"Failed to delete the entity with ID [{entity.Id}].");

                processedEntitiesCount++;
            }

            Console.WriteLine($"Deletion done. Enities deleted {deletedEntitesCount} out of {processedEntitiesCount}.");

            return deletedEntitesCount;

        }
    }
}
