using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System;
using System.Linq;
using Tweetinvi;
using Tweetinvi.Models;

namespace Beta.Modules
{
    class TwitterModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;
        

        public override string Prefix { get; } = "$";

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;
            _manager.CreateCommands("", cgb =>
            {
                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("Tweet")
                .Description("Send a tweet on behalf of Beta!")
                .Parameter("text", ParameterType.Unparsed)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "twitter", e.Channel.IsPrivate))
                    {
                        string tweet = e.GetArg("text");
                        if (System.Text.RegularExpressions.Regex.IsMatch(tweet, "<@d{16}>"))
                        {
                            string[] arg = tweet.Split(' ');
                            for (int i = 0; i < arg.Length; i++)
                            {
                                if (System.Text.RegularExpressions.Regex.IsMatch(arg[i], "<@d{16}>"))
                                {
                                    string username = arg[i];
                                    username = username.Replace("<@", null);
                                    username = username.Replace(">", null);
                                    Console.WriteLine(username);
                                    ulong id;
                                    ulong.TryParse(username, out id);
                                    arg[i] = e.Message.MentionedUsers.FirstOrDefault(x => x.Id == id).Name;
                                }
                            }
                            tweet = String.Join(" ", arg);
                        }
                        Auth.SetUserCredentials(Beta.Config.TwitterConsumerKey, Beta.Config.TwitterConsumerSecret, Beta.Config.TwitterAccessToken, Beta.Config.TwitterAccessSecret);
                        if (Tweet.PublishTweet(e.GetArg("text")) == null) await e.Channel.SendMessage("Something went wrong... I didn't send that tweet for you.");
                        else await e.Channel.SendMessage("I sent that tweet for you! Hopefully people like it!");
                    }                     
                });

                cgb.CreateCommand("Follow")
                .Description("Follow the specified user on Twitter")
                .Parameter("user", ParameterType.Required)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "twitter", e.Channel.IsPrivate))
                    {
                        var User = new UserIdentifier(e.GetArg("user"));
                        if (User == null)
                            await e.Channel.SendMessage("Sorry, I didn't see that user out in the Twitosphere...");
                        else Friendship.FriendshipController.CreateFriendshipWith(User);
                    }
                });
            
            });
        }

    }
}

