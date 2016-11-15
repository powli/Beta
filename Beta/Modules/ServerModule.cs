using System;
using System.Collections.Generic;
using System.Linq;
using Beta.JSONConfig;
using Beta.Repository;
using Beta.Utils;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;

namespace Beta.Modules
{
    internal class ServerModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;

        public override string Prefix { get; } = Beta.Config.CommandPrefixes.Standard;
        public List<string> TogglableCommands = new List<string>() { "ask", "motd", "roll", "quote", "table", "twitter", "comic", "gamertag", "note", "politics" };

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;


            _manager.CreateCommands("", cgb =>
            {
                cgb.MinPermissions((int) PermissionLevel.User);

                cgb.CreateCommand("say")
                    .MinPermissions((int) PermissionLevel.BotOwner) // An unrestricted say command is a bad idea
                    .Description("Make the bot speak!")
                    .Parameter("text", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage(e.GetArg("text"));
                        await e.Message.Delete();
                        
                    });

                cgb.CreateCommand("setmotd")
                    .MinPermissions((int) PermissionLevel.ChannelModerator) 
                    .Description("Set the Message of the Day for this channel!")
                    .Parameter("text", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        if (Beta.ChannelStateRepository.VerifyChannelExists(e.Channel.Id))
                        {
                            Beta.ChannelStateRepository.ChannelStates.FirstOrDefault(cs => cs.ChannelID == e.Channel.Id).MOTD = e.GetArg("text");
                            Beta.ChannelStateRepository.Save();
                            await e.User.SendMessage("Ok Boss, I set that message for you!");
                            await e.Message.Delete();
                            await e.Channel.SendMessage(Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).MOTD);
                        }                       
                    });

                cgb.CreateCommand("initstates")
                    .MinPermissions((int) PermissionLevel.BotOwner) 
                    .Description("Administration tool, initiate channel states for all current channels.")
                    .Do(async e =>
                    {
                        foreach (var server in _client.Servers)
                        {
                            foreach (var channel in server.AllChannels)
                            {
                                Beta.ChannelStateRepository.AddChannel(channel, server);
                            }
                            Beta.ServerStateRepository.AddServer(server);
                        }
                        await e.Channel.SendMessage("All set, Boss!");
                    });

                cgb.CreateCommand("roll")
                    .Description(
                        @"Make Beta roll the specified number of dice. For Example typing $Roll 3d12 would cause Beta to return the results of rolling 3 12-sided die. You could also role a single 12-sided die with d12.")
                    .Parameter("roll", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        if (Beta.CheckModuleState(e, "roll", e.Channel.IsPrivate))
                        {
                            var arg = e.GetArg("roll");
                            var arguments = arg.Split('d').Where(s => !string.IsNullOrEmpty(s)).ToList();
                            int sides, times, modifier;

                            if (arguments.Count == 1)
                            {
                                if (arguments[0].Contains("+"))
                                {
                                    arguments = arguments[0].Split('+').ToList();
                                    if (int.TryParse(arguments[0], out sides))
                                    {
                                        var dice = new Dice(sides);
                                        var temp = dice.Roll();
                                        if (int.TryParse(arguments[1], out modifier))
                                        {
                                            await
                                                e.Channel.SendMessage(
                                                    string.Format("Rolled one d{0}) plus {1} and got a total of {2}", sides,
                                                        modifier, temp + modifier));
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessage("Sorry, I don't recognize that number");
                                        }
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("Sorry, I don't recognize that number");
                                    }
                                }
                                else
                                {
                                    if (int.TryParse(arguments[0], out sides))
                                    {
                                        var dice = new Dice(sides);
                                        await
                                            e.Channel.SendMessage(string.Format("Rolled one d{0} and got a total of {1}",
                                                sides, dice.Roll()));
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("Sorry, I don't recognize that number");
                                    }
                                }
                            }
                            else if (arguments.Count == 2)
                            {
                                if (int.TryParse(arguments[0], out times))
                                {
                                    if (arguments[1].Contains("+"))
                                    {
                                        arguments = arguments[1].Split('+').ToList();
                                        if (int.TryParse(arguments[0], out sides))
                                        {
                                            var dice = new Dice(sides);
                                            var temp = dice.Roll(times);
                                            if (int.TryParse(arguments[1], out modifier))
                                            {
                                                await
                                                    e.Channel.SendMessage(
                                                        string.Format("Rolled {0} d{1} plus {2} and got a total of {3}",
                                                            times, sides, modifier, temp.Total + modifier));
                                                await
                                                    e.Channel.SendMessage(string.Format("Individual Rolls: {0}",
                                                        string.Join(",", temp.Rolls)));
                                            }
                                            else
                                            {
                                                await e.Channel.SendMessage("Sorry, I don't recognize that number");
                                            }
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessage("Sorry, I don't recognize that number");
                                        }
                                    }
                                    else
                                    {
                                        if (int.TryParse(arguments[1], out sides))
                                        {
                                            var dice = new Dice(sides);
                                            var temp = dice.Roll(times);
                                            await
                                                e.Channel.SendMessage(string.Format(
                                                    "Rolled {0} d{1} and got a total of {2}", times, sides, temp.Total));
                                            await
                                                e.Channel.SendMessage(string.Format("Individual Rolls: {0}",
                                                    string.Join(",", temp.Rolls)));
                                        }
                                        else
                                        {
                                            await e.Channel.SendMessage("Sorry, I don't recognize that number");
                                        }
                                    }
                                }
                            }
                        }
                        
                    });

                cgb.CreateCommand("ask")
                    .Description("Ask Beta a yes or no question!")
                    .Parameter("text", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        if (Beta.CheckModuleState(e, "ask", e.Channel.IsPrivate))
                        {
                            await e.Channel.SendMessage(Configuration._8BallResponses.GetRandom());
                        }

                    });

                cgb.CreateCommand("motd")
                    .Description("Show the MotD for the current channel")
                    .Do(async e=>
                    {
                        if (Beta.CheckModuleState(e, "motd", e.Channel.IsPrivate))
                        {
                            if (Beta.ChannelStateRepository.VerifyChannelExists(e.Channel.Id))
                            {
                                await
                                    e.Channel.SendMessage(Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).MOTD);
                                await e.Message.Delete();
                            }
                            else
                                await
                                    e.Channel.SendMessage(
                                        "Sorry dude, there is no message! harass the Admins or something.");
                        }
                    });

                cgb.CreateCommand("stoggle")
                    .Description(
                        "Toogles whether a module is enabled for the entire server. Please use one of the following options: Ask, MotD, Roll, Quote, Table, Twitter, Comic, Gamertag")
                    .MinPermissions((int)PermissionLevel.ServerAdmin)
                    .Parameter("Module", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        if(TogglableCommands.Contains(e.GetArg("Module").ToLower() ) )
                        {
                            bool changedState =
                                Beta.ServerStateRepository.GetServerState(e.Server.Id)
                                    .ToggleFeatureBool(e.GetArg("Module").ToLower());
                            await
                                e.Channel.SendMessage(
                                    String.Format("Ok, I've toggled the setting enabling {0} so now it's set to {1} for the entire server",
                                        e.GetArg("Module"), changedState.ToString()));
                        }
                        else
                        {
                            await e.Channel.SendMessage(String.Format("Sorry, guy. I don't see a module named {0} in the approved list...", e.GetArg("Module") ) )
                            ;
                        }
                        Beta.ServerStateRepository.Save();
                    });

                cgb.CreateCommand("toggle")
                        .Description(
                            "Toogles whether a module is enabled for the channel. Please use one of the following options: Ask, MotD, Roll, Quote, Table, Twitter, Comic, Gamertag")
                        .MinPermissions((int)PermissionLevel.ChannelAdmin)
                        .Parameter("Module", ParameterType.Unparsed)
                        .Do(async e =>
                        {
                            if (TogglableCommands.Contains(e.GetArg("Module").ToLower()))
                            {
                                bool changedState =
                                    Beta.ChannelStateRepository.GetChannelState(e.Channel.Id)
                                        .ToggleFeatureBool(e.GetArg("Module").ToLower());
                                await
                                    e.Channel.SendMessage(
                                        String.Format("Ok, I've toggled the setting enabling {0} so now it's set to {1}",
                                            e.GetArg("Module"), changedState.ToString()));
                            }
                            else
                            {
                                await e.Channel.SendMessage(String.Format("Sorry, guy. I don't see a module named {0} in the approved list...", e.GetArg("Module")))
                                ;
                            }
                            Beta.ChannelStateRepository.Save();
                        });

                cgb.CreateCommand("senabled")
                    .Description("Displays the state of each module for the server.")
                    .MinPermissions((int)PermissionLevel.ServerAdmin)
                    .Do(async e =>
                    {
                        if (Beta.ServerStateRepository.VerifyStateExists(e.Server.Id))
                        {
                            ServerState srvr = Beta.ServerStateRepository.GetServerState(e.Server.Id);
                            string msg = "";
                            msg += "Ask  :  " + srvr.AskEnabled + "\n";
                            msg += "Comic  :  " + srvr.ComicModuleEnabled + "\n";
                            msg += "Gamertag  :  " + srvr.GamertagModuleEnabled + "\n";
                            msg += "MotD  :  " + srvr.MOTDEnabled + "\n";
                            msg += "Note : " + srvr.NoteModuleEnabled + "\n";
                            msg += "Quote  :  " + srvr.QuoteModuleEnabled + "\n";
                            msg += "Roll  :  " + srvr.RollEnabled + "\n";
                            msg += "Table  :  " + srvr.TableUnflipEnabled + "\n";
                            msg += "Twitter  :  " + srvr.TwitterModuleEnabled + "\n";
                            msg += "Politics : " + srvr.PoliticsEnabled + "\n";
                            await e.Channel.SendMessage(msg);
                        }                        
                    });

                cgb.CreateCommand("enabled")
                    .Description("Displays the state of each module for the channel.")
                    .MinPermissions((int)PermissionLevel.ChannelAdmin)
                    .Do(async e =>
                    {
                        if (Beta.ChannelStateRepository.VerifyChannelExists(e.Channel.Id))
                        {
                            ChannelState chnl = Beta.ChannelStateRepository.GetChannelState(e.Channel.Id);
                            string msg = "";
                            msg += "Ask  :  " + chnl.AskEnabled + "\n";
                            msg += "Comic  :  " + chnl.ComicModuleEnabled + "\n";
                            msg += "Gamertag  :  " + chnl.GamertagModuleEnabled+"\n";
                            msg += "MotD  :  " + chnl.MOTDEnabled + "\n";
                            msg += "Note : " + chnl.NoteModuleEnabled + "\n";
                            msg += "Quote  :  " + chnl.QuoteModuleEnabled + "\n";
                            msg += "Roll  :  " + chnl.RollEnabled + "\n";
                            msg += "Table  :  " + chnl.TableUnflipEnabled + "\n";
                            msg += "Twitter  :  " + chnl.TwitterModuleEnabled + "\n";
                            msg += "Politics : " + chnl.PoliticsEnabled + "\n";
                            await e.Channel.SendMessage(msg);
                        }
                    });

                cgb.CreateCommand("install")
                    .Description("Returns a link that can be used to install this bot onto other servers.")
                    .Do(async e =>
                    {
                        await
                            e.Channel.SendMessage(
                                "https://discordapp.com/oauth2/authorize?&client_id=171093139052953607&scope=bot");
                    });

            });
        }
    }
}