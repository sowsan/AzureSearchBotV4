using AdaptiveCards;
using AzureSearchBotV4.Models;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureSearchBotV4.Helpers
{
    public class CardHelper
    {
        /// <summary>
        /// Get Hero card
        /// </summary>
        /// <param name="suggestionsList">List of suggested questions</param>
        /// <param name="cardTitle">Title of the cards</param>
        /// <param name="cardNoMatchText">No match text</param>
        /// <returns></returns>
        public static IMessageActivity GetSuggestionsCard(List<string> suggestionsList, string cardTitle = "Did you mean:", string cardNoMatchText = "None of the above.")
        {
            var chatActivity = Activity.CreateMessageActivity();
            var buttonList = new List<CardAction>();

            // Add all suggestions
            foreach (var suggestion in suggestionsList)
            {
                buttonList.Add(
                    new CardAction()
                    {
                        Value = suggestion,
                        Type = "imBack",
                        Title = suggestion,
                    });
            }

            // Add No match text
            buttonList.Add(
                new CardAction()
                {
                    Value = cardNoMatchText,
                    Type = "imBack",
                    Title = cardNoMatchText
                });

            var plCard = new HeroCard()
            {
                Title = cardTitle,
                Subtitle = string.Empty,
                Buttons = buttonList
            };

            // Create the attachment.
            var attachment = plCard.ToAttachment();

            chatActivity.Attachments.Add(attachment);

            return chatActivity;
        }

        public static IMessageActivity GetAnswerCard(TechKb kb)
        {
            var chatActivity = Activity.CreateMessageActivity();
            
            chatActivity.Attachments = new List<Attachment>();

            AdaptiveCard card = new AdaptiveCard();

            // Specify speech for the card.
            //card.Speak = "<s>Your  meeting about \"Adaptive Card design session\"<break strength='weak'/> is starting at 12:30pm</s><s>Do you want to snooze <break strength='weak'/> or do you want to send a late notification to the attendees?</s>";

            // Add text to the card.
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = kb.Question,
                Size =AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder
            });

            // Add text to the card.
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "tap on the appropriate buttons for the answer"
            });

            var quickCard = new AdaptiveCard();
            quickCard.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Text = kb.QuickSteps
            });

            var detailCard = new AdaptiveCard();
            detailCard.Body.Add(new AdaptiveTextBlock()
            {
                Wrap = true,
                Text = kb.DetailedSteps
            });
            
            card.Actions.Add(new AdaptiveShowCardAction()
            {
                Title = "Quick Steps",
                Card = quickCard
            });

            card.Actions.Add(new AdaptiveShowCardAction()
            {
                Title = "Detail Steps",
                Card = detailCard
            });


            // Create the attachment.
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            chatActivity.Attachments.Add(attachment);

            return chatActivity;
        }
    }
}
