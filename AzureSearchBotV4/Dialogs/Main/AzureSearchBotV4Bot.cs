// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using AzureSearchBotV4.Dialogs.Main;
using System;
using Microsoft.Azure.Search.Models;
using AzureSearchBotV4.Models;
using Microsoft.Azure.Search;
using System.Collections.Generic;
using AzureSearchBotV4.Helpers;

namespace AzureSearchBotV4.Dialogs.AzureSearchBotV4Bot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service. Transient lifetime services are created
    /// each time they're requested. Objects that are expensive to construct, or have a lifetime
    /// beyond a single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class AzureSearchBotV4Bot : IBot
    {
        private const string WelcomeText = "This bot will help you to get started with querying and displaying results from Azure Search. Type your query to get started.";

        private readonly BotAccessors _accessors;
                
        /// <summary>
        /// Services configured from the ".bot" file.
        /// </summary>
        private readonly BotServices _services;
       
        public AzureSearchBotV4Bot(BotAccessors accessors, BotServices services)
        {
             _services = services ?? throw new ArgumentNullException(nameof(services));           

            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

        }

        /// <summary>
        /// Every conversation turn calls this method.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {

                // Finish by sending a message to the user. Next time ContinueAsync is called it will return DialogTurnStatus.Empty.
                // await turnContext.SendActivityAsync(MessageFactory.Text($"Thank you, I have your name as '{results.Result}'."));

                SearchParameters parameters = new SearchParameters()
                {                    
                    SearchMode = SearchMode.All,
                    Top = 3,
                    Filter = null,
                    SearchFields = new[] { "Question" }                    
                    
                };

                // Get the documents through Azure search using the AzureSearchService service class
                DocumentSearchResult<TechKb> searchResults = GetSearchResults(turnContext.Activity.Text, _services.IndexClient, parameters);

                if (searchResults.Results.Count > 1)
                {
                    var suggestedQuestions = new List<string>();
                    foreach (var res in searchResults.Results)
                    {
                        suggestedQuestions.Add(res.Document.Question);
                    }

                    // Get hero card activity
                    var message = CardHelper.GetSuggestionsCard(suggestedQuestions);

                    await turnContext.SendActivityAsync(message);
                }
                else if (searchResults.Results.Count == 1)
                {
                    foreach (var res in searchResults.Results)
                    {

                        // Get adaptive card activity
                        var message = CardHelper.GetAnswerCard(res.Document);

                        await turnContext.SendActivityAsync(message);
                    }
                }
                else if (turnContext.Activity.Text == "None of the above.")
                {
                    await turnContext.SendActivityAsync("Ok, please send your refined question");
                }
                else  
                {
                    await turnContext.SendActivityAsync("Sorry, I couldn't find answers. Please send another query.");
                }               

                await _accessors.ConversationState.SaveChangesAsync(turnContext);
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    // Send a welcome message to the user and tell them what actions they may perform to use this bot
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected", cancellationToken: cancellationToken);
            }
        }

        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        $"Welcome to Knowledge Base Search. {WelcomeText}",
                        cancellationToken: cancellationToken);
                }
            }
        }

        public static DocumentSearchResult<TechKb> GetSearchResults(string searchString, SearchIndexClient indexClient, SearchParameters parameters)
        {
            try
            {
                DocumentSearchResult<TechKb> searchResults = indexClient.Documents.Search<TechKb>(searchString, parameters);
                return searchResults;
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }

            return null;
        }

    }
}
