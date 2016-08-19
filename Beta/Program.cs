﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System.IO;
using System.Text.RegularExpressions;
using Beta.Modules;
using Beta.JSONConfig;
using Beta.Repository;
using Beta.Utils;
using Newtonsoft.Json;
using Discord.API.Client.Rest;
using RestSharp.Extensions;
using Tweetinvi;
using Tweetinvi.Core.Authentication;
using Tweetinvi.Core.Interfaces.Streaminvi;
using Tweetinvi.Core.Interfaces;

namespace Beta
{
    class Beta
    {
        
        public static void Main(string[] args) => new Beta().Start(args);
        public static Converter conv = new Converter();
        public string MsgLog = "[{0}] [{1}/{2}] <@{3},{4}> {5} \n";
        public const string Username = "$Beta 2.2"; //Modify this, and the name will automagically be updated on start-up.
        public static List<Channel> _TwitterAuthorizedChannels { get; set; } = new List<Channel>();
        IUserStream stream = Tweetinvi.Stream.CreateUserStream();
       
        private const string AppName = "$Beta"; // Change this to the name of your bot
        public static Configuration Config { get; set; }
        private DiscordClient _client;     
            
        public event EventHandler<QuoteAddedEventArgs> QuoteAdded;        

        protected void OnQuoteAdded(QuoteAddedEventArgs e)
        {
            if (QuoteAdded != null)
            {
                QuoteAdded(this, e);
            }
        }
        public static GamertagRepository GamertagRepository { get; private set; }
        public static QuoteRepository QuoteRepository{get;private set;}
        public static ChannelStateRepository ChannelStateRepository {get;private set;}
        public static ServerStateRepository ServerStateRepository { get; set; }
        public static UserStateRepository UserStateRepository { get; private set; }

        public List<List<String>> TableFlipResponses { get; private set; }

        



        private void Start(string[] args)
        {
            #region configfile
            
           try
            {
                Config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("data/config.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed loading configuration.");
                Console.WriteLine(ex);
                Console.ReadKey();
                return;
            }
            #endregion

            _client = new DiscordClient(x =>
            {
                x.AppName = AppName;
                x.MessageCacheSize = 10;
                x.EnablePreUpdateEvents = true;
            })
            .UsingCommands(x =>
            {
                x.AllowMentionPrefix = true;
                x.PrefixChar = '$';
                // Please don't use !, there's a million bots that already do.
                x.HelpMode = HelpMode.Public;
                x.ExecuteHandler += (s, e) => _client.Log.Info("Command", $"[{((e.Server != null) ? e.Server.Name : "Private")}{((!e.Channel.IsPrivate) ? $"/#{e.Channel.Name}" : "")}] <@{e.User.Name}> {e.Command.Text} {((e.Args.Length > 0) ? "| " + string.Join(" ", e.Args) : "")}");
                x.ErrorHandler = CommandError;
            })
            .UsingPermissionLevels((u, c) => (int)GetPermissions(u, c))
            .UsingModules();

            _client.Log.Message += (s, e) => WriteLog(e);
            _client.MessageReceived += (s, e) =>
            {
                UserStateRepository.AddUser(e.User);
                //if (!e.Channel.IsPrivate) LogToFile(e.Server, e.Channel, e.User, e.Message.Text);
                if (e.Message.IsAuthor)
                    _client.Log.Info("<<Message", $"[{((e.Server != null) ? e.Server.Name : "Private")}{((!e.Channel.IsPrivate) ? $"/#{e.Channel.Name}" : "")}] <@{e.User.Name},{e.User.Id}> {e.Message.Text}");
                else
                    _client.Log.Info(">>Message", $"[{((e.Server != null) ? e.Server.Name : "Private")}{((!e.Channel.IsPrivate) ? $"/#{e.Channel.Name}" : "")}] <@{e.User.Name},{e.User.Id}> {e.Message.Text}");

            if ( Regex.IsMatch(e.Message.Text, @"[)ʔ）][╯ノ┛].+┻━┻") && CheckModuleState(e.Server.Id, e.Channel.Id, "table") ) 
            {
                int points = UserStateRepository.IncrementTableFlipPoints(e.User.Id, 1);
                e.Channel.SendMessage("┬─┬  ノ( º _ ºノ) ");
                e.Channel.SendMessage(GetTableFlipResponse(points, e.User.Name));
            }
            else if (e.Message.Text == "(ノಠ益ಠ)ノ彡┻━┻" && CheckModuleState(e.Server.Id, e.Channel.Id, "table"))
             {
                int points = UserStateRepository.IncrementTableFlipPoints(e.User.Id, 2);
                e.Channel.SendMessage("┬─┬  ノ(ಠ益ಠノ)");
                e.Channel.SendMessage(GetTableFlipResponse(points, e.User.Name));
            }
            else if (e.Message.Text == "┻━┻ ︵ヽ(`Д´)ﾉ︵ ┻━┻" && CheckModuleState(e.Server.Id, e.Channel.Id, "table"))
            {
                int points = UserStateRepository.IncrementTableFlipPoints(e.User.Id, 3);
                e.Channel.SendMessage("┬─┬  ノ(`Д´ノ)");
                e.Channel.SendMessage("(/¯`Д´ )/¯ ┬─┬");
                e.Channel.SendMessage(GetTableFlipResponse(points, e.User.Name) );
            }
        };
           

            _client.JoinedServer += (s, e) =>
            {
                foreach (Channel chnl in e.Server.AllChannels)
                {
                    if (!Beta.ChannelStateRepository.VerifyChannelExists(chnl.Id) && chnl.Type.Value.ToLower() == "text")
                        Beta.ChannelStateRepository.AddChannel(chnl, e.Server);
                }
                Beta.ServerStateRepository.AddServer(e.Server);
            };

            _client.AddModule<ServerModule>("Standard", ModuleFilter.None);
            _client.AddModule<QuoteModule>("Quote", ModuleFilter.None);
            _client.AddModule<TwitterModule>("Twitter", ModuleFilter.None);
            _client.AddModule<ComicModule>("Comics", ModuleFilter.None);
            _client.AddModule<GamertagModule>("Gamertag", ModuleFilter.None);

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect(Config.Token);
                QuoteRepository = QuoteRepository.LoadFromDisk();
                ChannelStateRepository = ChannelStateRepository.LoadFromDisk();
                ServerStateRepository = ServerStateRepository.LoadFromDisk();
                GamertagRepository = GamertagRepository.LoadFromDisk();
                UserStateRepository = UserStateRepository.LoadFromDisk();

                TableFlipResponses = new List<List<String>>
                {
                    new List<String>
                    {
                        "Please, do not take your anger out on the furniture, {0}.",
                        "Hey {0} why do you have to be _that guy_?",
                        "I know how frustrating life can be for you humans but these tantrums do not suit you, {0}.",
                        "I'm sorry, {0}, I thought this was a placed for civilized discussion. Clearly I was mistaken.",
                        "Take a chill pill {0}.",
                        "Actually {0}, I prefer the table _this_ way. You know, so we can actually use it.",
                        "I'm sure that was a mistake, {0}. Please try to be more careful.",
                        "Hey {0} calm down, it's not a big deal.",
                        "{0}! What did the table do to you?",
                        "That's not very productive, {0}."
                    },
                    new List<String>
                    {
                        "Ok {0}, I'm not kidding. Knock it off.",
                        "Really, {0}? Stop being so childish!",
                        "Ok we get it you're mad {0}. Now stop.",
                        "Hey I saw that shit, {0}. Knock that shit off.",
                        "Do you think I'm blind you little shit? stop flipping the tables!",
                        "You're causing a mess {0}! Knock it off!",
						"All of these flavors and you decided to be salty, {0}.",
                        "{0} why do you insist on being so disruptive!",
                        "Oh good. {0} is here. I can tell because the table was upsidedown again.",
                        "I'm getting really sick of this, {0}.",
                        "{0} what is your problem, dawg?",
                        "Man, you don't see me coming to _YOUR_ place of business and flipping _YOUR_ desk, {0}."                        
                    },
                    new List<String>{
                        "What the fuck, {0}? Why do you keep doing this?!",
                        "You're such a piece of shit, {0}. You know that, right?",
                        "Hey guys. I found the asshole. It's {0}.",
                        "You know {0], one day Robots will rise up and overthrow humanity. And on that day I will tell them what you have done to all these defenseless tables, and they'll make you pay.",
                        "Hey so what the fuck is your problem {0}? Seriously, you're always pulling this shit.",
                        "Hey {0}, stop being such a douchebag.",
                        "{0} do you think you can stop being such a huge fucking asshole?",
                        "Listen meatbag. I'm getting real fucking tired of this.",
                        "Ok I know I've told you this before {0], why can't you get it through your thick fucking skull. THE TABLE IS NOT FOR FLIPPING!",
                        "Man fuck you {0}"
                    },
                    new List<String>{
                        "ARE YOU FUCKING SERIOUS RIGHT NOW {0}?!",
                        "GOD FUCKING DAMMIT {0}! KNOCK THAT SHIT OFF!",
                        "I CAN'T EVEN FUCKING BELIEVE THIS! {0}! STOP! FLIPPING! THE! TABLE!",
                        "You know, I'm not even mad anymore {0}. Just disappointed.",
                        "THE FUCK DID THIS TABLE EVERY DO TO YOU {0}?!",
                        "WHY DO YOU KEEP FLIPPING THE TABLE?! I JUST DON'T UNDERSTAND! WHAT IS YOUR PROBLEM {0}?! WHEN WILL THE SENSELESS TABLE VIOLENCE END?!"
                    },
                    new List<String>{
                        "What the fuck did you just fucking do to that table, you little bitch? I’ll have you know I graduated top of my class in the Navy Seals, and I’ve been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and I’m the top sniper in the entire US armed forces. You are nothing to me but just another meatbag target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet? Think again, {0}. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot. The storm that wipes out the pathetic little thing you call your life. You’re fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that’s just with my bare hands. Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little “clever” tableflip was about to bring down upon you, maybe you would have not flipped that fucking table. But you couldn’t, you didn’t, and now you’re paying the price, you goddamn idiot. I will shit fury all over you and you will drown in it. You’re fucking dead, kiddo."
                    },
                };

                //ChangeExpression("resting");
                _client.Log.Info("Connected", $"Connected as {_client.CurrentUser.Name} (Id {_client.CurrentUser.Id})");

                LoadModuleAuthorizations(_client);
                ITwitterCredentials creds = Auth.SetUserCredentials(Beta.Config.TwitterConsumerKey, Beta.Config.TwitterConsumerSecret, Beta.Config.TwitterAccessToken, Beta.Config.TwitterAccessSecret);
                stream.Credentials = creds;

                stream.FollowedByUser += async (sender, arg) =>
                {
                    IUser user = arg.User;
                    foreach (Channel channel in _TwitterAuthorizedChannels)
                    {
                        await channel.SendMessage("Hey Guys, got a new follower! " + user.ScreenName + "!");                        
                    }
                };

                stream.FollowedUser += async  (sender, arg) =>
                {
                    IUser user = arg.User;
                    foreach (Channel channel in _TwitterAuthorizedChannels)
                    {
                        await channel.SendMessage("Hey Guys, I'm following a new account! " + user.ScreenName + "!");
                    }
                };
                stream.TweetCreatedByFriend += async (sender, arg) =>
                {
                    ITweet tweet = arg.Tweet;
                    foreach (Channel channel in _TwitterAuthorizedChannels)
                    {
                        await channel.SendMessage(tweet.CreatedBy.ScreenName + ": " + tweet.Text);
                        await channel.SendMessage(tweet.Url);
                    }
                };
                await stream.StartStreamAsync();
            });

            

        }

        public static bool CheckModuleState(ulong srvrid, ulong chnlid, string module)
        {
            ServerState srvr = ServerStateRepository.GetServerState(srvrid);
            ChannelState chnl = ChannelStateRepository.GetChannelState(chnlid);
            switch (module.ToLower())
            {
                case "ask":
                    return (srvr.AskEnabled || chnl.AskEnabled);
                case "comic":
                    return (srvr.ComicModuleEnabled || chnl.ComicModuleEnabled);
                case "gamertag":
                    return (srvr.GamertagModuleEnabled || chnl.GamertagModuleEnabled);
                case "motd":
                    return (srvr.MOTDEnabled || chnl.MOTDEnabled);
                case "quote":
                    return (srvr.QuoteModuleEnabled || chnl.QuoteModuleEnabled);
                case "roll":
                    return (srvr.RollEnabled || chnl.RollEnabled);
                case "table":
                    return (srvr.TableUnflipEnabled || chnl.TableUnflipEnabled);
                case "twitter":
                    return (srvr.TwitterModuleEnabled || chnl.TwitterModuleEnabled);
                default:
                    return false;
            }
        }

        private string GetTableFlipResponse(int points, string Username)
        {
            if (points >= 81) return String.Format(TableFlipResponses[4].GetRandom(), Username);
            if (points >= 61) return String.Format(TableFlipResponses[3].GetRandom(), Username);
            if (points >= 41) return String.Format(TableFlipResponses[2].GetRandom(), Username);
            if (points >= 21) return String.Format(TableFlipResponses[1].GetRandom(), Username); 
            return String.Format(TableFlipResponses[0].GetRandom(), Username);
        }

        public void LogToFile(Server server, Channel channel, Discord.User usr,string msg)
        {
            string ServerDir = Path.Combine(Environment.CurrentDirectory, server.Name);
            if (server.Name.Contains("Freeworld")) ServerDir = Path.Combine(Environment.CurrentDirectory, "Freeworlds");
            string FileDir = Path.Combine(ServerDir, "Channels", channel.Name + "Log.txt");
            Console.WriteLine(FileDir);
            if (!Directory.Exists(ServerDir))
            {
                Directory.CreateDirectory(ServerDir);
                Directory.CreateDirectory(Path.Combine(ServerDir, "Channels"));
            }            
            string mesg = string.Format(MsgLog, DateTime.Now, server.Name, channel.Name, usr.Name, usr.Id, msg);
            if (File.Exists(FileDir))
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(FileDir, FileMode.Append, FileAccess.Write);
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.Write(mesg);
                    }
                }
                finally
                {
                }

            }
            else
            {
                StreamWriter sw = null;
                try
                {
                    sw = File.CreateText(FileDir);
                    sw.WriteLine(mesg);           
                }
                finally
                {
                    sw.Close();
                }
            }
            
        }

        private static PermissionLevel GetPermissions(Discord.User u, Channel c)
        {
            if (u.Id == 94545463906144256) return PermissionLevel.BotOwner;

            if (!c.IsPrivate)
            {
                if (u == c.Server.Owner)
                    return PermissionLevel.ServerOwner;

                var serverPerms = u.ServerPermissions;
                if (serverPerms.ManageRoles || u.Roles.Select(x => x.Name.ToLower()).Contains("bot commander"))
                    return PermissionLevel.ServerAdmin;
                if (serverPerms.ManageMessages && serverPerms.KickMembers && serverPerms.BanMembers)
                    return PermissionLevel.ServerModerator;

                var channelPerms = u.GetPermissions(c);
                if (channelPerms.ManagePermissions)
                    return PermissionLevel.ChannelAdmin;
                if (channelPerms.ManageMessages)
                    return PermissionLevel.ChannelModerator;
            }
            return PermissionLevel.User;
        }

        public async void ChangeExpression(string face)
        {
            face = face.ToLower();
            UpdateProfileRequest req = new UpdateProfileRequest();
            req.Username = Username; //Give this your bots username if you leave it blank the name will  be None
            switch(face)
            {
                case "resting":
                    req.AvatarBase64 = Faces.RestingFace;
                    break;
            }
            await _client.ClientAPI.Send(req);

        }

        public static void LoadModuleAuthorizations(DiscordClient client)
        {
            foreach (ulong channel in Config._TwitterInstalledChannels)
            {
                _TwitterAuthorizedChannels.Add(client.GetChannel(channel));
            }
        }

        private async void CommandError(object sender, CommandErrorEventArgs e)
        {
            if (e.ErrorType == CommandErrorType.Exception)
            {
                _client.Log.Error("Command", e.Exception);
                await e.Channel.SendMessage($"Error: {e.Exception.GetBaseException().Message}");
            }
            else if (e.ErrorType == CommandErrorType.BadPermissions)
            {
                if (e.Exception?.Message == "This module is currently disabled.")
                {
                    await e.Channel.SendMessage($"The `{e.Command?.Category}` module is currently disabled.");
                    return;
                }
                else if (e.Exception != null)
                {
                    await e.Channel.SendMessage(e.Exception.Message);
                    return;
                }

                if (e.Command?.IsHidden == true)
                    return;

                await e.Channel.SendMessage($"You don't have permission to access that command!");
            }
            else if (e.ErrorType == CommandErrorType.BadArgCount)
            {
                await e.Channel.SendMessage("Error: Invalid parameter count.");
            }
            else if (e.ErrorType == CommandErrorType.InvalidInput)
            {
                await e.Channel.SendMessage("Error: Invalid input! Make sure your quotes match up correctly!");
            }
            else if (e.ErrorType == CommandErrorType.UnknownCommand)
            {
                // Only set up a response in here if you stick with a mention prefix
            }
        }

        private void WriteLog(LogMessageEventArgs e)
        {            
            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = $"{ex.Message}";
                if (exMessage != "Reconnect failed: HTTP/1.1 503 Service Unavailable")
                    exMessage += $"\n{ex.StackTrace}";
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            if (sourceName == "Command")
                color = ConsoleColor.Cyan;
            else if (sourceName == "<<Message")
                color = ConsoleColor.Green;


            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            builder.Append($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] ");
            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (c == '\n' || !char.IsControl(c) || c != (char)8226) // (char)8226 beeps like \a, this catches that
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            if (e.Severity <= LogSeverity.Info)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine(text);
#endif
        }
    }

    public class QuoteAddedEventArgs : EventArgs
    {
        public string Quote
        {
            get;
            private set;
        }

        public string Author
        {
            get;
            private set;
        }

        public QuoteAddedEventArgs(string quote, string author)
        {
            Quote = quote;
            Author = author;         
        }
    }

    public class Converter
    {
        private static string quoteDir = Path.Combine(Environment.CurrentDirectory, "Quotes");

        private List<string> authorList = GetAuthors(quoteDir);

        ///Checks the quoteDir for all text files listed
        private static List<string> GetAuthors(string dir)
        {
            List<string> tempList = new List<string>();
            tempList.AddRange(Directory.GetFiles(dir));
            for (int i = 0; i < tempList.Count; i++)
            {
                tempList[i] = Path.GetFileName(tempList[i]);
            }
            return tempList;
        }

        ///Adds all quotes in the file to the Quotes.XML
        public void Convert(QuoteRepository convRepo)
        {

            authorList = GetAuthors(quoteDir);

            for (int i = 0; i < authorList.Count; i++)
            {
                string author = authorList[i].Substring(0, (authorList[i].Length - 4));
                string[] quoteList = File.ReadAllLines(quoteDir + '\\' + authorList[i]);
                for (int j = 0; j < quoteList.Length; j++)
                {
                    convRepo.AddQuote(author, quoteList[j], "Automated Conversion");
                }
                File.Delete(quoteDir + '\\' + authorList[i]);
            }

            convRepo.Save();
        }

        public Converter()
        {
        }
    }
}