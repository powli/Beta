using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System.IO;
using Beta.Modules;
using Beta.JSONConfig;
using Beta.Utils;
using Newtonsoft.Json;
using Discord.API.Client.Rest;
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
        public static QuoteRepository QuoteRepository
        {
            get;
            private set;
        }
        public Dictionary<ulong, MessageRepository> ChannelRepository
        {
            get;
            set;
        }

        



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
                LogToFile(e.Server, e.Channel, e.User, e.Message.Text);
                if (e.Message.IsAuthor)
                    _client.Log.Info("<<Message", $"[{((e.Server != null) ? e.Server.Name : "Private")}{((!e.Channel.IsPrivate) ? $"/#{e.Channel.Name}" : "")}] <@{e.User.Name},{e.User.Id}> {e.Message.Text}");
                else
                    _client.Log.Info(">>Message", $"[{((e.Server != null) ? e.Server.Name : "Private")}{((!e.Channel.IsPrivate) ? $"/#{e.Channel.Name}" : "")}] <@{e.User.Name},{e.User.Id}> {e.Message.Text}");
            };

            _client.AddModule<StandardModule>("Standard", ModuleFilter.None);
            _client.AddModule<QuoteModule>("Quote", ModuleFilter.None);
            _client.AddModule<TwitterModule>("Twitter", ModuleFilter.None);
            _client.AddModule<ComicModule>("Comics", ModuleFilter.None);

            _client.ExecuteAndWait(async () =>
            {
                await _client.Connect(Config.Token);
                QuoteRepository = QuoteRepository.LoadFromDisk();
                ChangeExpression("resting");
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

        public void LogToFile(Server server, Channel channel, Discord.User usr,string msg)
        {
            string ServerDir = Path.Combine(Environment.CurrentDirectory, server.Name);
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
                    if (fs != null) fs.Dispose();
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
            if (u.Id == 94545463906144256) // Replace this with your own UserId
                return PermissionLevel.BotOwner;

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