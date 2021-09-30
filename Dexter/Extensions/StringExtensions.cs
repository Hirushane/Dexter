using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dexter.Configurations;
using Discord;
using Discord.WebSocket;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.Generic;

namespace Dexter.Extensions
{

    /// <summary>
    /// The String Extensions class offers a variety of different extensions that can be applied to a string to modify it.
    /// These include the prettify, sanitize and markdown extensions.
    /// </summary>

    public static class StringExtensions
    {

        private static readonly string[] SensitiveCharacters = { "\\", "*", "_", "~", "`", "|", ">", "[", "(" };

        /// <summary>
        /// The Prettify method removes all the characters before the name of the class and only selects characters from A-Z.
        /// </summary>
        /// <param name="Name">The string you wish to run through the REGEX expression.</param>
        /// <returns>A sanitised string with the characters before the name of the class removed.</returns>

        public static string Prettify(this string Name) => Regex.Replace(Name, @"(?<!^)(?=[A-Z])", " ");

        /// <summary>
        /// The Sanitize method removes the "Commands" string from the name of the class.
        /// </summary>
        /// <param name="Name">The string you wish to run through the replace method.</param>
        /// <returns>The name of a module with the "Commands" string removed.</returns>

        public static string Sanitize(this string Name) => Name.Replace("Commands", string.Empty);

        /// <summary>
        /// The Sanitize Markdown method removes any sensitive characters that may otherwise change the created embed.
        /// It does this by looping through and replacing any sensitive characters that may break the embed.
        /// </summary>
        /// <param name="Text">The string you wish to be run through the command.</param>
        /// <returns>The text which has been sanitized and has had the sensitive characters removed.</returns>

        public static string SanitizeMarkdown(this string Text)
        {
            foreach (string Unsafe in SensitiveCharacters)
                Text = Text.Replace(Unsafe, $"\\{Unsafe}");
            return Text;
        }

        /// <summary>
        /// Gets the class of the last method that had been called.
        /// </summary>
        /// <param name="SearchHeight">The height backwards that you would like to see the call come from.</param>
        /// <returns>The last called class + method</returns>
        
        public static KeyValuePair<string, string> GetLastMethodCalled(int SearchHeight)
        {
            SearchHeight += 1;

            Type mBase = new StackTrace().GetFrame(SearchHeight).GetMethod().DeclaringType;

            string name;

            if (mBase.DeclaringType != null)
                name = mBase.DeclaringType.Name;
            else
                name = mBase.Name;

            string methodName = mBase.Name;

            int Index = methodName.IndexOf(">d__");

            if (Index != -1)
                methodName = methodName.Substring(0, Index).Replace("<", "");

            if (name.Contains("AsyncMethodBuilderCore") || name.Contains("AsyncTaskMethodBuilder") || name.Contains("EmbedExtensions") || name.Contains("AwaitTaskContinuation") || name.Contains("ExecutionContext")  || name.Contains("DiscordRestAPIClient"))
                return GetLastMethodCalled(SearchHeight + 1);
            else
                return new KeyValuePair<string, string> ( name, methodName );
        }

        /// <summary>
        /// Obtains a Proxied URL from a given Image URL.
        /// </summary>
        /// <param name="ImageURL">The URL of the target image.</param>
        /// <param name="ImageName">The Name to give the image once downloaded.</param>
        /// <param name="DiscordShardedClient">A Discord Socket Client service to parse the storage channel.</param>
        /// <param name="ProposalConfiguration">Configuration holding the storage channel ID.</param>
        /// <returns>A <c>Task</c> object, which can be awaited until this method completes successfully.</returns>

        public static async Task<string> GetProxiedImage(this string ImageURL, string ImageName, DiscordShardedClient DiscordShardedClient, ProposalConfiguration ProposalConfiguration)
        {
            string ImageCacheDir = Path.Combine(Directory.GetCurrentDirectory(), "ImageCache");

            if (!Directory.Exists(ImageCacheDir))
                Directory.CreateDirectory(ImageCacheDir);

            string FilePath = Path.Combine(ImageCacheDir, $"{ImageName}{Path.GetExtension(ImageURL.Split("?")[0])}");

            HttpClient client = new();

            var response = await client.GetAsync(ImageURL);

            using (var fs = new FileStream(FilePath, FileMode.CreateNew)) {
                await response.Content.CopyToAsync(fs);
            }
            
            ITextChannel Channel = DiscordShardedClient.GetChannel(ProposalConfiguration.StorageChannelID) as ITextChannel;

            IUserMessage AttachmentMSG = await Channel.SendFileAsync(FilePath);

            File.Delete(FilePath);

            return AttachmentMSG.Attachments.FirstOrDefault().ProxyUrl;
        }

        /// <summary>
        /// Hashes an object into an <c>int</c> using the MD5 algorithm.
        /// </summary>
        /// <param name="HashingString">The object to Hash.</param>
        /// <returns>The hashed value as an Int32.</returns>

        public static int GetHash(this object HashingString)
        {
            return BitConverter.ToInt32(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(HashingString.ToString())));
        }

    }

}
