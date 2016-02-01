using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace Coffee.VoiceCommands
{
    public sealed class CoffeeVoiceService : IBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();

            // Does a ton of like, super important stuff
            
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null && triggerDetails.Name == "CoffeeVoiceService")
            {
                try
                {
                    voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                    switch (voiceCommand.CommandName)
                    {
                        case "makeMeCoffeeBitch":
                            {
                                //await ShowProgressScreen("Working");
                                //WhatKindOfCoffee();

                                GetSassy();
                                break;
                            }

                        // As a last resort, launch the app in the foreground.
                        default:
                            LaunchAppInForeground();
                            break;
                    }
                }
                finally
                {
                    if (this.serviceDeferral != null)
                    {
                        // Complete the service deferral.
                        this.serviceDeferral.Complete();
                    }
                }
            }
        }

        private async void GetSassy()
        {
            String[] firstPart = { "lazy", "stupid", "insecure", "idiotic", "slimy", "slutty", "smelly", "pompous", "communist", "dicknose", "pie-eating", "racist", "elitist", "trashy", "drug-loving", "butterface", "tone deaf", "ugly", "creepy" };

            String[] secondPart = { "douche", "ass", "turd", "rectum", "butt", "cock", "shit", "crotch", "bitch", "prick", "slut", "taint", "fuck", "dick", "boner", "shart", "nut", "sphincter" };

            String[] thirdPart = { "pilot", "canoe", "captain", "pirate", "hammer", "knob", "box", "jockey", "nazi", "waffle", "goblin", "blossum", "biscuit", "clown", "socket", "monster", "hound", "dragon", "balloon"};

            Random rand = new Random();
            String responseText = "Make your own damn coffee you "
                + firstPart[rand.Next(0, firstPart.Length)] + " "
                + secondPart[rand.Next(0, secondPart.Length)] + " "
                + thirdPart[rand.Next(0, thirdPart.Length)] + "!";

            var userMessage = new VoiceCommandUserMessage();
            userMessage.DisplayMessage = responseText;
            userMessage.SpokenMessage = responseText;
            
            var response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(response);
        }

        private async void WhatKindOfCoffee()
        {
            // Take action and determine when the next trip to destination
            // Insert code here.

            // Replace the hardcoded strings used here with strings 
            // appropriate for your application.

            // First, create the VoiceCommandUserMessage with the strings 
            // that Cortana will show and speak.
            var userMessage = new VoiceCommandUserMessage();
            userMessage.DisplayMessage = "What kind of Coffee?";
            userMessage.SpokenMessage = "What kind of Coffee?";

            // Optionally, present visual information about the answer.
            // For this example, create a VoiceCommandContentTile with an 
            // icon and a string.
            var destinationsContentTiles = new List<VoiceCommandContentTile>();
            
            destinationsContentTiles.Add(new VoiceCommandContentTile()
            {
                ContentTileType = VoiceCommandContentTileType.TitleWithText,
                AppLaunchArgument = "espresso",
                Title = "Espresso",
                TextLine1 = "Super Strong"
            });

            destinationsContentTiles.Add(new VoiceCommandContentTile()
            {
                ContentTileType = VoiceCommandContentTileType.TitleWithText,
                AppLaunchArgument = "irish",
                Title = "Irish Coffee",
                TextLine1 = "Good coffee and better alcohol"
            });

            // Create the VoiceCommandResponse from the userMessage and list    
            // of content tiles.
            var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);

            // Cortana will present a “Go to app_name” link that the user 
            // can tap to launch the app. 
            // Pass in a launch to enable the app to deep link to a page 
            // relevant to the voice command.
            //response.AppLaunchArgument = "faster";            

            // Ask Cortana to display the user message and content tile and 
            // also speak the user message.
            await voiceServiceConnection.ReportSuccessAsync(response);
        }

        private async void LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = "Yeah I have no clue what you want.";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            // When launching the app in the foreground, pass an app 
            // specific launch parameter to indicate what page to show.
            response.AppLaunchArgument = "showAllTrips=true";

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        /// <summary>
        /// Show a progress screen. These should be posted at least every 5 seconds for a 
        /// long-running operation, such as accessing network resources over a mobile 
        /// carrier network.
        /// </summary>
        /// <param name="message">The message to display, relating to the task being performed.</param>
        /// <returns></returns>
        private async Task ShowProgressScreen(string message)
        {
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = message;

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(response);
        }

        /// <summary>
        /// Handle the completion of the voice command. Your app may be cancelled
        /// for a variety of reasons, such as user cancellation or not providing 
        /// progress to Cortana in a timely fashion. Clean up any pending long-running
        /// operations (eg, network requests).
        /// </summary>
        /// <param name="sender">The voice connection associated with the command.</param>
        /// <param name="args">Contains an Enumeration indicating why the command was terminated.</param>
        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                // Insert your code here.
                // Complete the service deferral.
                this.serviceDeferral.Complete();
            }
        }

        /// <summary>
        /// When the background task is cancelled, clean up/cancel any ongoing long-running operations.
        /// This cancellation notice may not be due to Cortana directly. The voice command connection will
        /// typically already be destroyed by this point and should not be expected to be active.
        /// </summary>
        /// <param name="sender">This background task instance</param>
        /// <param name="reason">Contains an enumeration with the reason for task cancellation</param>
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
            if (this.serviceDeferral != null)
            {
                //Complete the service deferral
                this.serviceDeferral.Complete();
            }
        }
    }
}
