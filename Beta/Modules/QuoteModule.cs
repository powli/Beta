using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;

namespace Beta.Modules
{
    class QuoteModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;
        private string _QuoteFormat = "\"{0}\"\r\n\r\n     ~{1}";

        public override string Prefix { get; } = "$";

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", cgb =>
            {
                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("quote")                
                .Description("Pull up a random quote from the specified speaker.")
                .Parameter("speaker", ParameterType.Unparsed)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "quote", e.Channel.IsPrivate))
                    {
                        string name = e.GetArg("speaker").Trim();
                        Author author = Beta.QuoteRepository.GetAuthor(name);
                        if (name.ToLower() == "random")
                        {
                            author = Beta.QuoteRepository.Authors.GetRandom();
                        }
                        if (author == null)
                        {
                            await e.Channel.SendMessage("Could not find author: '" + e.GetArg("Speaker") + "'");
                        }
                        
                        else
                        {
                            string reply = string.Format(_QuoteFormat,
                                ReplaceVariables(author.Quotes.GetRandom().Text, e.Message.User.Name), author.Name);
                            await e.Channel.SendMessage(reply);
                        }

                    }

                });

                cgb.CreateCommand("list")
                .Description("Return a numbered list of all quotes. Will be sent as a PM.")
                .Parameter("speaker", ParameterType.Unparsed)                
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "quote", e.Channel.IsPrivate))
                    {
                        string name = e.GetArg("speaker").Trim();
                        Author author = Beta.QuoteRepository.GetAuthor(name);
                        if (author == null)
                        {
                            await e.Channel.SendMessage("Could not find author: '" + e.GetArg("Speaker") + "'");
                        }
                        else
                        {
                            string reply = "";
                            int index = 1;
                            Console.WriteLine(author.Quotes[0].Text);
                            foreach (Quote quote in author.Quotes)
                            {
                                reply += index++ + ". " + quote.Text + "\r\n";
                            }
                            await e.User.SendMessage(reply);
                        }
                    }
                });

                cgb.CreateCommand("delete")
                .Description("Delete the specified quote, by number, for the specified author. EX) '$delete Beta|1' would delete the first quote by Beta. Please use the 'List' command to retrieve quote numbers.")
                .Parameter("text", ParameterType.Unparsed)
                .MinPermissions((int)PermissionLevel.ChannelModerator)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "quote", e.Channel.IsPrivate))
                    {
                        var args = e.GetArg("text").Split('|');
                        Author existingAuthor = Beta.QuoteRepository.GetAuthor(args[0]);
                        int index;
                        if (Int32.TryParse(args[1], out index))
                        {

                            if (existingAuthor == null)
                            {
                                await e.Channel.SendMessage("Sorry, I don't recognize that author.");
                            }
                            else
                            {
                                await e.Channel.SendMessage(Beta.QuoteRepository.DeleteQuote(existingAuthor, index));
                                if (existingAuthor.Quotes.Count == 0)
                                {
                                    Beta.QuoteRepository.RemoveAuthor(existingAuthor);
                                    await
                                        e.Channel.SendMessage("Looks like we're actually out of quotes for " +
                                                              existingAuthor +
                                                              " so I'll remove it from the list as well.");
                                }
                            }
                        }
                        else await e.Channel.SendMessage("Sorry, but that doesn't look like a number to me.");
                    }
                });

                cgb.CreateCommand("quotable")
                .Description("Return a list of all speakers we have stored quotes for. Will be sent as a PM.")
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "quote", e.Channel.IsPrivate))
                    {
                        string msg = "";
                        await e.User.SendMessage("Oh yeah sure one sec... I got quotes for all these guys: ");
                        Beta.QuoteRepository.Authors = Beta.QuoteRepository.Authors.OrderBy(item => item.Name).ToList();
                        foreach (Author author in Beta.QuoteRepository.Authors)
                        {
                            //Before adding the author name to the message, make sure we're not exceeding 2000 characters
                            if ((author.Name.Length + msg.Length) > 2000)
                            {//If we are, send the message, blank it out, and start building a new one.
                                await e.User.SendMessage(msg);
                                msg = "";
                                msg += author.Name+"\n";
                            } 
                            else
                            {//Otherwise, just add the author name to the list and move on.
                                msg += author.Name+"\n";
                            }
                        }
                        if (msg != "") await e.User.SendMessage(msg);
                    }
                });

                cgb.CreateCommand("convert")
                .MinPermissions((int)PermissionLevel.BotOwner)                
                .Do(e =>
                {
                    if (Beta.CheckModuleState(e, "quote", e.Channel.IsPrivate))
                    {
                        Beta.conv.Convert(Beta.QuoteRepository);
                    }
                });

                cgb.CreateCommand("add")
                .Description("Add a quote for the specified Author. $add Author|Whatever that author said.")
                .Parameter("text", ParameterType.Unparsed)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "quote", e.Channel.IsPrivate))
                    {
                        var args = e.GetArg("text").Split('|');
                        Author existingAuthor = Beta.QuoteRepository.GetAuthor(args[0]);
                        Beta.QuoteRepository.AddQuote(args[0], args[1], e.User.Name);

                        if (existingAuthor != null)
                        {
                            await
                                e.Channel.SendMessage(String.Format(
                                    "Successfully added another quote from '{0}', dawg.", existingAuthor.Name));
                        }
                        else
                        {
                            await
                                e.Channel.SendMessage(String.Format("Successfully added quote from '{0}', dawg.",
                                    args[0]));
                        }

                        // Save after every add
                        Beta.QuoteRepository.Save();
                    }
                });
            });
       }

      private string ReplaceVariables(string msg, string name)
        {
            string[] temparr = msg.Split(' ');
            for (int i = 0; i < temparr.Length; i++)
            {
                temparr[i] = temparr[i].Replace("%N", name);
            }
            return String.Join(" ", temparr);            
        }
       
    }
}
