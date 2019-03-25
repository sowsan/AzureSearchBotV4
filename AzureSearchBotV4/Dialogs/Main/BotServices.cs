using System;
using System;
using System.Collections.Generic;

using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Configuration;
using AzureSearchBotV4.Models;

namespace AzureSearchBotV4.Dialogs.Main
{
    public class BotServices
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotServices"/> class.
        /// </summary>
        public SearchIndexClient IndexClient { get; set; }
        public BotServices()
        {
        }

        public BotServices(BotConfiguration botConfiguration)
        {
            foreach (var service in botConfiguration.Services)
            {
                switch (service.Type)
                {                  

                    case ServiceTypes.Generic:
                        {
                            if (service.Name == "azuresearch")
                            {
                                var searchConfig = service as GenericService;

                                if (string.IsNullOrEmpty(searchConfig.Properties["SearchServiceName"].ToString()))
                                {
                                    throw new InvalidOperationException("Azure Search Service name is required to run this bot.  Please update your '.bot' file.");
                                }

                                if (string.IsNullOrEmpty(searchConfig.Properties["SearchDialogsIndexName"].ToString()))
                                {
                                    throw new InvalidOperationException("Azure Search Service Index name is required to run this bot.  Please update your '.bot' file.");
                                }

                                if (string.IsNullOrEmpty(searchConfig.Properties["SearchServiceQueryApiKey"].ToString()))
                                {
                                    throw new InvalidOperationException("Azure Search Service Admin API Key is required to run this bot.  Please update your '.bot' file.");
                                }

                                string searchServiceName = searchConfig.Properties["SearchServiceName"].ToString();
                                string indexName = searchConfig.Properties["SearchDialogsIndexName"].ToString();
                                string queryApiKey = searchConfig.Properties["SearchServiceQueryApiKey"].ToString();

                                SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));

                                IndexClient = indexClient;
                            }

                            break;
                        }
                }
            }
        }

    }
}
