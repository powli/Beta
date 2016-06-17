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
                    string name = e.GetArg("speaker").Trim();
                    Author author = Beta.QuoteRepository.GetAuthor(name);
                    if (author == null)
                    {
                        await e.Channel.SendMessage( "Could not find author: '"+e.GetArg("Speaker")+"'");
                    }
                    else
                    {
                        string reply = string.Format(_QuoteFormat, ReplaceVariables(author.Quotes.GetRandom().Text, e.Message.User.Name), author.Name);
                        await e.Channel.SendMessage(reply);
                    }
                });

                cgb.CreateCommand("quotable")
                .Description("Return a list of all speakers we have stored quotes for")
                .Do(async e =>
                {
                    await e.Channel.SendMessage("Oh yeah sure one sec... I got quotes for all these guys: ");
                    Beta.QuoteRepository.Authors = Beta.QuoteRepository.Authors.OrderBy(item => item.Name).ToList();                    
                    await e.Channel.SendMessage(string.Join(Environment.NewLine, Beta.QuoteRepository.Authors));
                });

                cgb.CreateCommand("convert")
                .MinPermissions((int)PermissionLevel.BotOwner)                
                .Do(e =>
                {
                    Beta.conv.Convert(Beta.QuoteRepository);
                });
                /*cgb.CreateCommand("")
                .MinPermissions((int)PermissionLevel.BotOwner) // An unrestricted say command is a bad idea
                .Description("Make the bot speak!")
                .Parameter("text", ParameterType.Unparsed)
                .Do(async e =>
                {
                    await e.Channel.SendMessage(e.GetArg("text"));
                });*/
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
