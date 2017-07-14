using Beta.Repository;
using Beta.Utils;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta.Modules
{
    class ScrumModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;
        public override string Prefix { get; } = Beta.Config.CommandPrefixes.Standard;

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;


            _manager.CreateCommands("", cgb =>
            {

                cgb.MinPermissions((int)PermissionLevel.ChannelModerator);

                cgb.CreateCommand("scrum")
                .Description("Allows a Channel Moderator to schedule a weekly Scrum reminder for the specified DateTime - e.g., $scrum \"Sat, 15 July 2017 05:00:00 ET\"")
                .Parameter("datetime", ParameterType.Unparsed)
                .Do(async e =>
                {
                    DateTime dateTime;
                    if (DateTime.TryParse(e.GetArg("dateime"), out dateTime))
                    {
                        ChannelState channel = Beta.ChannelStateRepository.GetChannelState(e.Channel.Id);
                        channel.ScrumEnabled = true;
                        channel.ScrumReminderDateTIme = dateTime;
                        await e.Channel.SendMessage("Ok, I've set that date for your weekly Scrum reminders!");
                    }
                    else await e.Channel.SendMessage("Sorry, " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ". I couldn't parse that DateTime.");
                });


            });
        }
    }
}
