using Beta.Repository;
using Beta.Scrum;
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

                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("scrum")
                .Description("Allows a Channel Moderator to schedule a weekly Scrum reminder for the specified DateTime - e.g., $scrum \"Sat, 15 July 2017 05:00:00 ET\"")
                .Parameter("datetime", ParameterType.Unparsed)
                .MinPermissions((int)PermissionLevel.ChannelModerator)
                .Do(async e =>
                {
                    DateTime dateTime;
                    if (DateTime.TryParse(e.GetArg("datetime"), out dateTime))
                    {
                        ChannelState channel = Beta.ChannelStateRepository.GetChannelState(e.Channel.Id);
                        channel.ScrumEnabled = true;
                        channel.ScrumReminderDateTime = dateTime;
                        await e.Channel.SendMessage("Ok, I've set that date for your weekly Scrum reminders!");
                    }
                    else await e.Channel.SendMessage("Sorry, " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ". I couldn't parse that DateTime.");
                });

                cgb.CreateCommand("addscrumer")
                .Description("Add the user with the given ID to the list of scrummers to remind.")
                .Parameter("uid", ParameterType.Unparsed)
                .Alias("addscrum")
                .Alias("addscrummer")
                .MinPermissions((int)PermissionLevel.ChannelModerator)
                .Do(async e =>
                {
                    ulong id;
                    if (ulong.TryParse(e.GetArg("uid"), out id))
                    {
                        ChannelState channel = Beta.ChannelStateRepository.GetChannelState(e.Channel.Id);
                        if (channel.ScrumEnabled)
                        {
                            if (e.Channel.GetUser(id) != null)
                            {
                                channel.ScrumerIds.Add(id);
                            }
                            else await e.Channel.SendMessage("Sorry " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ", looks like that user isn't in this Channel.");
                        }
                        else await e.Channel.SendMessage("Sorry " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ", looks like that Scrum isn't enabled for this Channel.");
                    }
                    else await e.Channel.SendMessage("Doesn't look like that's a number, " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ".");
                });

                cgb.CreateCommand("scrumers")
                .Description("List the scrumers for the current channel.")
                .Alias("scrummers")
                .Do(async e =>
                {
                    ChannelState channel = Beta.ChannelStateRepository.GetChannelState(e.Channel.Id);
                    if (channel.ScrumEnabled)
                    {
                        string msg = "Here's the list of scrumers:\n\n";
                        msg += channel.GetScrumerNames(e.Channel);
                        await e.Channel.SendMessage(msg);
                    }
                    else await e.Channel.SendMessage("Sorry " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ", scrum hasn't been setup for this channel!");
                });

                cgb.CreateCommand("update")
                .Description("Submit an update for this weeks scrum. Removes you from the weekly blast from Beta.")
                .Parameter("update", ParameterType.Unparsed)
                .Do(async e =>
                {
                    ChannelState chnl = Beta.ChannelStateRepository.GetChannelState(e.Channel.Id);
                    if (chnl.ScrumEnabled)
                    {
                        if (chnl.ScrumerIds.Contains(e.User.Id))
                        {
                            ScrumManager.AddNewUpdate(e.Args[0], e.User.Name, e.Channel.Id);
                            chnl.UpdatedScrumerIds.Add(e.User.Id);
                            await e.Channel.SendMessage("Logged that update, " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ".");
                        }
                        else await e.Channel.SendMessage("Sorry, looks like you're not configured to be a scrumer, " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + "!");
                    }
                    else await e.Channel.SendMessage("Sorry, Scrum is not configured for this channel, "+ Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ".");
                    
                });
            });
        }
    }
}
