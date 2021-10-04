using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dexter.Configurations;
using Dexter.Databases.EventTimers;
using Dexter.Enums;
using Dexter.Extensions;
using Dexter.Services;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Dexter.Abstractions
{

    /// <summary>
    /// The Service is an abstract class that all services extend upon.
    /// Services run when a specified event occured, as is hooked into the client through the Initialize method.
    /// </summary>

    public abstract class Service
    {

        /// <summary>
        /// The ProfileService is used to find a random profile picture for a webhook on create or get.
        /// </summary>
        public ProfilingService ProfileService { get; set; }

        /// <summary>
        /// The BotConfiguration is used to find the thumbnails for the BuildEmbed method.
        /// </summary>
        public BotConfiguration BotConfiguration { get; set; }

        /// <summary>
        /// The DiscordShardedClient is used to create the webhook for the webhook on create or get.
        /// </summary>
        public DiscordShardedClient DiscordShardedClient { get; set; }

        /// <summary>
        /// The TimerService class is used to create a timer for wait until an expiration time has been reached.
        /// </summary>
        public TimerService TimerService { get; set; }

        /// <summary>
        /// The Initialize abstract method is what is called when all dependencies are initialized.
        /// It can be used to hook into delegates to run when an event occurs.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// The Create Or Get Webhook finds the given channel and, when provided a name, attempts to find a webhook
        /// with that said name. If the webhook can not be found, it creates a new webhook in the channel with the set name.
        /// </summary>
        /// <param name="channelID">The Channel ID is the snowflake ID of the channel which you wish the webhook to be made in.</param>
        /// <param name="webhookName">The Webhook Name is the identifier of the webhook, and is what the webhook will be called.</param>
        /// <returns>The DiscordWebhookClient of the webhook that has been gotten or created.</returns>

        public async Task<DiscordWebhookClient> CreateOrGetWebhook(ulong channelID, string webhookName)
        {
            if (channelID <= 0)
                return null;

            SocketChannel channel = DiscordShardedClient.GetChannel(channelID);

            if (channel is SocketTextChannel textChannel)
            {
                foreach (RestWebhook restWebhook in await textChannel.GetWebhooksAsync())
                    if (restWebhook.Name.Equals(webhookName))
                        return new DiscordWebhookClient(restWebhook.Id, restWebhook.Token);

                RestWebhook webhook = await textChannel.CreateWebhookAsync(webhookName, ProfileService.GetRandomPFP());

                return new DiscordWebhookClient(webhook.Id, webhook.Token);
            }

            throw new Exception($"The webhook {webhookName} could not be initialized in the given channel {channel} due to it being of type {channel.GetType().Name}.");
        }

        /// <summary>
        /// The Create Event Timer method is a generic method that will await for an expiration time to be reached
        /// before continuing execution of the code set in the CallbackMethod parameter.
        /// </summary>
        /// <param name="callbackMethod">The method you wish to callback once expired.</param>
        /// <param name="callbackParameters">The parameters you wish to callback with once expired.</param>
        /// <param name="secondsTillExpiration">The count in seconds until the timer will expire.</param>
        /// <param name="timerType">The given type of the timer, specifying if it should be removed after the set time (EXPIRE) or continue in the set interval.</param>
        /// <returns>The token associated with the timed event for future reference.</returns>

        public async Task<string> CreateEventTimer(Func<Dictionary<string, string>, Task> callbackMethod,
                Dictionary<string, string> callbackParameters, int secondsTillExpiration, TimerType timerType)
        {

            return await CreateEventTimer(callbackMethod, callbackParameters, secondsTillExpiration, timerType, TimerService);
        }

        /// <summary>
        /// The Create Event Timer method is a generic method that will await for an expiration time to be reached
        /// before continuing execution of the code set in the CallbackMethod parameter.
        /// </summary>
        /// <param name="callbackMethod">The method you wish to callback once expired.</param>
        /// <param name="callbackParameters">The parameters you wish to callback with once expired.</param>
        /// <param name="secondsTillExpiration">The count in seconds until the timer will expire.</param>
        /// <param name="timerType">The given type of the timer, specifying if it should be removed after the set time (EXPIRE) or continue in the set interval.</param>
        /// <param name="timerService">The service used to access the database and related infrastructure to store the mute.</param>
        /// <returns>The token associated with the timed event for future reference.</returns>

        public static async Task<string> CreateEventTimer(Func<Dictionary<string, string>, Task> callbackMethod,
                Dictionary<string, string> callbackParameters, int secondsTillExpiration, TimerType timerType, TimerService timerService)
        {
            string json = JsonConvert.SerializeObject(callbackParameters);

            return await timerService.AddTimer(json, callbackMethod.Target.GetType().Name, callbackMethod.Method.Name, secondsTillExpiration, timerType);
        }

        /// <summary>
        /// The Build Embed method is a generic method that simply calls upon the EMBED BUILDER extension method.
        /// </summary>
        /// <param name="thumbnail">The thumbnail that you would like to be applied to the embed.</param>
        /// <returns>A new embed builder with the specified attributes applied to the embed.</returns>

        public EmbedBuilder BuildEmbed(EmojiEnum thumbnail)
        {
            return new EmbedBuilder().BuildEmbed(thumbnail, BotConfiguration, EmbedCallingType.Command);
        }

    }

}
