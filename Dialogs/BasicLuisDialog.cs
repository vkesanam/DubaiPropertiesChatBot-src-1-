using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    public class BasicLuisDialog : LuisDialog<object>
    {
        string customerName;
        string email;
        string phone;
        string complaint;

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"], 
            ConfigurationManager.AppSettings["LuisAPIKey"], 
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            string message = "I'm afraid I cannot help you with that. Please try again with different keywords.";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            //await this.ShowLuisResult(context, result);
            if (customerName == null)
            {
                string message = "Glad to talk to you. Welcome to iBot - your Virtual Wasl Property Consultant.";
                await context.PostAsync(message);

                context.Call(new FeedbackDialog("qnaURL", "userQuestion"), ResumeAfterFeedback);

                //PromptDialog.Text(
                //context: context,
                //resume: CustomerNameFromGreeting,
                //prompt: "May i know your Name please?",
                //retry: "Sorry, I don't understand that.");
            }
            else
            {
                string message = "Tell me " + customerName + ". How i can help you?";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
        }
        private async Task ResumeAfterFeedback(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            if (await result != null)
            {
                //if (result.Equals("English") || result.Equals("Arabic"))
                //{
                //    PromptDialog.Text(
                //    context: context,
                //    resume: CustomerNameFromGreeting,
                //    prompt: "May i know your Name please?",
                //    retry: "Sorry, I don't understand that.");
                //}
                //await MessageReceivedAsync(context, result);
                context.Wait(MessageReceived);
            }
            else
            {
                context.Done<IMessageActivity>(null);
            }
        }
        public async Task CustomerNameFromGreeting(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            customerName = response;

            PromptDialog.Text(
               context: context,
               resume: CustomerNameFromCarring,
               prompt: "Thanks " + customerName + ". Are you looking to buy or rent your home/property?",
               retry: "Sorry, I don't understand that.");
        }
        public async Task CustomerNameFromCarring(IDialogContext context, IAwaitable<string> result)
        {
            PromptDialog.Text(
              context: context,
              resume: PropertyCity,
              prompt: "Great. I can show you active homes If you tell me a little bit, Which part of UAE are you looking in?",

              retry: "Sorry, I don't understand that.");
        }
        [LuisIntent("ENQUIRY")]
        public async Task ENQUIRY(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
                context: context,
                resume: PropertyCity,
                prompt: "Great. I can show you active homes If you tell me a little bit, Which part of UAE are you looking in?",
                retry: "Sorry, I don't understand that.");
        }
        public async Task PropertyCity(IDialogContext context, IAwaitable<string> result)
        {
            //string response = await result;
            //phone = response;

            PromptDialog.Text(
                context: context,
                resume: PropertyBathrooms,
                prompt: "That is a great market. There are currently 306 listings on the market in that area. To narrow it down a bit, what price do you require?",
                retry: "Sorry, I don't understand that.");
        }
        public async Task PropertyPrice(IDialogContext context, IAwaitable<string> result)
        {
            //string response = await result;
            //phone = response;

            PromptDialog.Text(
                context: context,
                resume: PropertyBedrooms,
                prompt: "How many bathrooms are you looking for?",
                retry: "Sorry, I don't understand that.");
        }
        public async Task PropertyBathrooms(IDialogContext context, IAwaitable<string> result)
        {
            //string response = await result;
            //phone = response;

            PromptDialog.Text(
                context: context,
                resume: PropertyPrice,
                prompt: "How many bedrooms are you looking for?",
                retry: "Sorry, I don't understand that.");
        }
        public async Task PropertyBedrooms(IDialogContext context, IAwaitable<string> result)
        {
            //string response = await result;
            //phone = response;
            PromptDialog.Choice(context, ResumePropertyOptions,
                    new List<string>()
                    {
                        "Single Family",
                        "Condos",
                        "Attached",
                        "Detached"
                    },
                    "There are 54 available. What property type are you interested in?");
        }
        public virtual async Task ResumePropertyOptions(IDialogContext context, IAwaitable<string> argument)
        {
            var selection = await argument;
            string result = selection;

            string message = "Great there are 25  " + result + " homes/properties that meet your needs. You can swipe to see each home/property.";
            await context.PostAsync(message);

            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = GetCardsAttachments();

            await context.PostAsync(reply);

            // context.Wait(this.MessageReceived);
            PromptDialog.Confirm(
                 context: context,
                 resume: CustomerLeadCreation,
                 prompt: "Would you like to get updates of new listings like these?",
                 retry: "Sorry, I don't understand that.");

        }
        public async Task CustomerLeadCreation(IDialogContext context, IAwaitable<bool> result)
        {
            var answer = await result;
            if (answer)
            {
                PromptDialog.Text(
               context: context,
               resume: CustomerLead,
               prompt: "May I have your email id? ",
               retry: "Sorry, I don't understand that.");
            }
            else
            {
                string message = $"Thanks for using I Bot. Hope you have a great day!";
                await context.PostAsync(message);
            }
        }
        public async Task CustomerLead(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            email = response;

            await context.PostAsync("Thank you for your interest. Our property consultant will get back to you shortly.");

            PromptDialog.Confirm(
                 context: context,
                 resume: AnythingElseHandler,
                 prompt: "Is there anything else that I could help?",
                 retry: "Sorry, I don't understand that.");
            //CRMConnection.CreateLeadReg(customerName, email);

        }
        private static IList<Attachment> GetCardsAttachments()
        {
            return new List<Attachment>()
            {
                GetHeroCard(
                    "Wasl Properties",
                    "AED 95000000",
                    "Wasl Properties Group is a property development and management company based in Dubai, United Arab Emirates.",
                    new CardImage(url: "https://dubaipropertieschatbot.azurewebsites.net/1.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Read more", value: "https://www.waslproperties.com/en")),
                GetHeroCard(
                     "Wasl Properties",
                    "AED 25000000",
                    "Wasl Properties is a leading real estate master developer based in Dubai. Aligned to the leadership’s vision and overall development plans.",
                    new CardImage(url: "https://dubaipropertieschatbot.azurewebsites.net/2.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Read more", value: "https://www.waslproperties.com/en")),
                GetHeroCard(
                     "Wasl Properties",
                    "AED 670000000",
                    "Wasl Properties is a major contributor to realizing the vision of Dubai. A dynamic and forward-thinking organistion.",
                    new CardImage(url: "https://dubaipropertieschatbot.azurewebsites.net/3.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Read more", value: "https://www.waslproperties.com/en")),
                GetHeroCard(
                     "Wasl Properties",
                    "AED 45000959000",
                    "Wasl Properties is committed to creating and managing renowned developments that provide distinctive and enriching lifestyles.",
                    new CardImage(url: "https://dubaipropertieschatbot.azurewebsites.net/4.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Read more", value: "https://www.waslproperties.com/en")),
                  GetHeroCard(
                     "Wasl Properties",
                    "AED 45000959000",
                    "Riverside is part of Marasi Business Bay - an exciting new development by Wasl Properties, in the heart of Business Bay.",
                    new CardImage(url: "https://dubaipropertieschatbot.azurewebsites.net/5.jpg"),
                    new CardAction(ActionTypes.OpenUrl, "Read more", value: "https://www.waslproperties.com/en")),
            };
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }

        private static Attachment GetThumbnailCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new ThumbnailCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }
        [LuisIntent("CASE")]
        public async Task CASE(IDialogContext context, LuisResult result)
        {
            PromptDialog.Text(
            context: context,
            resume: CustomerMobileNumber,
            prompt: "What is your complaint/suggestion?",
            retry: "Sorry, I don't understand that.");
        }
        public async Task CustomerMobileNumber(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            complaint = response;

            PromptDialog.Text(
                context: context,
                resume: CustomerEmail,
                prompt: "May I have your Mobile Number? ",
                retry: "Sorry, I don't understand that.");
        }
        public async Task CustomerEmail(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            phone = response;

            PromptDialog.Text(
               context: context,
               resume: CustomerEmail,
               prompt: "May I have your Email ID? ",
               retry: "Sorry, I don't understand that.");
        }
        public virtual async Task FinalResultHandler(IDialogContext context, IAwaitable<string> argument)
        {
            string response = await argument;
            email = response;

            await context.PostAsync($@"Thank you for your interest, your request has been logged. Our customer service team will get back to you shortly.
                                    {Environment.NewLine}Your service request  summary:
                                    {Environment.NewLine}Complaint Title: {complaint},
                                    {Environment.NewLine}Customer Name: {customerName},
                                    {Environment.NewLine}Phone Number: {phone},
                                    {Environment.NewLine}Email: {email}");

            PromptDialog.Confirm(
            context: context,
            resume: AnythingElseHandler,
            prompt: "Is there anything else that I could help?",
            retry: "Sorry, I don't understand that.");
            //CRMConnection.CreateCase(complaint, customerName, phone, email);
        }
        public async Task AnythingElseHandler(IDialogContext context, IAwaitable<bool> argument)
        {
            var answer = await argument;
            if (answer)
            {
                await GeneralGreeting(context, null);
            }
            else
            {
                string message = $"Thanks for using I Bot. Hope you have a great day!";
                await context.PostAsync(message);

                //var survey = context.MakeMessage();

                //var attachment = GetSurveyCard();
                //survey.Attachments.Add(attachment);

                //await context.PostAsync(survey);

                context.Done<string>("conversation ended.");
            }
        }
        public virtual async Task GeneralGreeting(IDialogContext context, IAwaitable<string> argument)
        {
            string message = $"Great! What else that can I help you?";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result) 
        {
            await context.PostAsync($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            context.Wait(MessageReceived);
        }
    }
}