﻿using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Luis;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis.Models;
using MyZippo.Internal;
using MyZippo.Entities;
using System.Threading;
using Microsoft.Bot.Connector;

namespace MyZippo.Dialogs
{
    [LuisModel("a0101534-3872-4263-b65d-36e29279d6f6", "fe24da2bdad14b25842661e0eb4c2add")]
    [Serializable]
    public class MyZippoLuisDialog : LuisDialog<object>
    {
        private const string BLOG_BASE_URL = "https://ankitbko.github.io";

        public MyZippoLuisDialog(params ILuisService[] services) : base(services)
        {
        }

        [LuisIntent("None")]
        [LuisIntent("")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult result)
        {
            var cts = new CancellationTokenSource();
            await context.Forward(new GreetingsDialog(), GreetingDialogDone, await message, cts.Token);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(Responses.HelpMessage);
            context.Wait(MessageReceived);
        }

        [LuisIntent("AboutMe")]
        public async Task AboutMe(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(@"Hieu is a Software Engineer currently working for AZO Controls in Osterburken.");
            await context.PostAsync(@"He is now live in Buchen.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("BlogSearch")]
        public async Task BlogSearch(IDialogContext context, LuisResult result)
        {
            string tag = string.Empty;
            string replyText = string.Empty;
            List<Post> posts = new List<Post>();

            try
            {
                if (result.Entities.Count > 0)
                {
                    tag = result.Entities.FirstOrDefault(e => e.Type == "Tag").Entity;
                }

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    var bs = new BlogSearch();
                    posts = bs.GetPostsWithTag(tag);
                }

                replyText = GenerateResponseForBlogSearch(posts, tag);
                await context.PostAsync(replyText);
            }
            catch (Exception)
            {
                await context.PostAsync("Something really bad happened. You can try again later meanwhile I'll check what went wrong.");
            }
            finally
            {
                context.Wait(MessageReceived);
            }
        }

        #region Private
        private string GenerateResponseForBlogSearch(List<Post> posts, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return "I didn't get what topic you are searching for. It might be that Ankit has not written any article so I am not able to recognize that topic. You may try to change the topic and try again.";
            if (posts.Count == 0)
                return "Ankit has not written any article regarding " + tag + ". Contact him on Twitter to let him know you are interested in ." + tag;

            string replyMessage = string.Empty;
            replyMessage += $"I got {posts.Count} articles on {tag} \n\n";
            foreach (var post in posts)
            {
                replyMessage += $"* [{post.Name}]({BLOG_BASE_URL}{post.Url})\n\n";
            }
            replyMessage += $"Have fun reading. Post a comment if you like them.";
            return replyMessage;
        }

        private async Task GreetingDialogDone(IDialogContext context, IAwaitable<bool> result)
        {
            var success = await result;
            if (!success)
                await context.PostAsync("I'm sorry. I didn't understand you.");

            context.Wait(MessageReceived);
        }
        #endregion
    }
}