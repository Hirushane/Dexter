﻿using Dexter.Attributes.Methods;
using Dexter.Enums;
using Dexter.Extensions;
using Dexter.Databases.Warnings;
using Discord;
using Discord.Commands;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Dexter.Configurations;
using System.Linq;

namespace Dexter.Commands {

    public partial class ModeratorCommands {

        /// <summary>
        /// The WarningsCommand runs on WARNINGS and will send a DM to the author of the message if the command is run in a bot
        /// channel and no user is specified of their own warnings. If a user is specified and the author is a moderator it will
        /// proceed to print out all the warnings of that specified member into the channel the command had been sent into.
        /// </summary>
        /// <param name="User">The User field specifies the user that you wish to get the warnings of.</param>
        /// <returns>A task object, from which we can await until this method completes successfully.</returns>

        [Command("warnings")]
        [Summary("Returns a record of warnings for a set user or your own.")]
        [Alias("records", "record", "warns")]
        [BotChannel]

        public async Task WarningsCommand([Optional] IUser User) {
            bool IsUserSpecified = User != null;

            if (IsUserSpecified) {
                if ((Context.User as IGuildUser).GetPermissionLevel(BotConfiguration)
                    >= PermissionLevel.Moderator) {

                    EmbedBuilder[] Warnings = GetWarnings(User, Context.User, true);

                    if (Warnings.Length > 1)
                        await CreateReactionMenu(Warnings, Context.Channel);
                    else
                        await Warnings.FirstOrDefault().WithCurrentTimestamp().SendEmbed(Context.Channel);
                } else {
                    await BuildEmbed(EmojiEnum.Annoyed)
                        .WithTitle("Halt! Don't go there-")
                        .WithDescription("Heya! To run this command with a user specified, you will need to be a moderator. <3")
                        .SendEmbed(Context.Channel);
                }
            } else {
                EmbedBuilder[] Embeds = GetWarnings(Context.User, Context.User, false);

                try {
                    foreach (EmbedBuilder Embed in Embeds)
                        await Embed.SendEmbed(await Context.User.GetOrCreateDMChannelAsync());

                    await BuildEmbed(EmojiEnum.Love)
                        .WithTitle("Sent warnings log.")
                        .WithDescription("Heya! I've sent you a log of your warnings. " +
                            "Please note these records are not indicitive of a mute or ban, and are simply a sign of when we've had to verbally warn you in the chat.")
                        .SendEmbed(Context.Channel);
                } catch (HttpException) {
                    await BuildEmbed(EmojiEnum.Annoyed)
                        .WithTitle("Unable to send warnings log!")
                        .WithDescription("Woa, it seems as though I'm not able to send you a log of your warnings! " +
                            "This is usually indicitive of having DMs from the server blocked or me personally. " +
                            "Please note, for the sake of transparency, we often use Dexter to notify you of events that concern you - " +
                            "so it's critical that we're able to message you through Dexter. <3")
                        .SendEmbed(Context.Channel);
                }
            }
        }

        /// <summary>
        /// The GetWarnings method returns an array of embeds detailing the user's warnings, time of warning, and moderator (if enabled).
        /// </summary>
        /// <param name="User">The user of whose warnings you wish to recieve.</param>
        /// <param name="RunBy">The user who has run the given warnings command.</param>
        /// <param name="ShowIssuer">Whether or not the moderators should be shown in the log. Enabled for moderators, disabled for DMed records.</param>
        /// <returns>An array of embeds containing the given users warnings.</returns>
        
        public EmbedBuilder[] GetWarnings(IUser User, IUser RunBy, bool ShowIssuer) {
            Warning[] Warnings = WarningsDB.GetWarnings(User.Id);

            if (Warnings.Length <= 0)
                return new EmbedBuilder[1] {
                    BuildEmbed(EmojiEnum.Love)
                        .WithTitle("No issued warnings!")
                        .WithDescription($"{User.Mention} has a clean slate!\n" +
                        $"Go give {(User.Id == RunBy.Id ? "yourself" : "them")} a pat on the back. <3")
                };

            List<EmbedBuilder> Embeds = new ();

            EmbedBuilder CurrentBuilder = BuildEmbed(EmojiEnum.Love)
                .WithTitle($"{User.Username}'s Warnings - {Warnings.Length} {(Warnings.Length == 1 ? "Entry" : "Entries")}")
                .WithDescription($"All times are displayed in {TimeZoneInfo.Local.DisplayName}");

            for(int Index = 0; Index < Warnings.Length; Index++) {
                IUser Issuer = Client.GetUser(Warnings[Index].Issuer);

                long TimeOfIssue = Warnings[Index].TimeOfIssue;

                DateTimeOffset Time = DateTimeOffset.FromUnixTimeSeconds(TimeOfIssue > 253402300799 ? TimeOfIssue / 1000 : TimeOfIssue);

                EmbedFieldBuilder Field = new EmbedFieldBuilder()
                    .WithName($"Warning {Index + 1} - ID {Warnings[Index].WarningID}")
                    .WithValue($"{(ShowIssuer ? $":cop: {(Issuer != null ? Issuer.GetUserInformation() : "Unknown")}\n" : "")}" +
                    $":calendar: {Time:M/d/yyyy h:mm:ss}\n" +
                    $":notepad_spiral: {Warnings[Index].Reason}");

                try {
                    CurrentBuilder.AddField(Field);
                } catch (Exception) {
                    Embeds.Add(CurrentBuilder);
                    CurrentBuilder = new EmbedBuilder().AddField(Field).WithColor(Color.Green);
                }
            }

            Embeds.Add(CurrentBuilder);

            return Embeds.ToArray();
        }

    }

}
