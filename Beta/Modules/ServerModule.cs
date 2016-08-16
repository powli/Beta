using System.Linq;
using Beta.JSONConfig;
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
                    .Do(async e => { await e.Channel.SendMessage(e.GetArg("text")); });

                /*cgb.CreateCommand("greet")
                    .MinPermissions((int) PermissionLevel.BotOwner) // An unrestricted say command is a bad idea
                    .Description("Toggle Beta's greet mode for this channel.!")
                    .Parameter("text", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        if (Beta.ChannelStateRepository.VerifyChannelExists(e.Channel.Id))
                        {
                            Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).GreetMode =
                                !Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).GreetMode;
                            var currentState = Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).GreetMode;
                            if (currentState) await e.Channel.SendMessage("Ok, boss. I turned on greet mode!");
                            else await e.Channel.SendMessage("Ok, boss. I turned off greet mode!");
                        }
                        else
                        {
                            Beta.ChannelStateRepository.AddChannel(e.Channel, e.Server);
                            Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).GreetMode =
                                !Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).GreetMode;
                            var currentState = Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).GreetMode;
                            if (currentState) await e.Channel.SendMessage("Ok, boss. I turned on greet mode!");
                            else await e.Channel.SendMessage("Ok, boss. I turned off greet mode!");
                        }
                    });*/

                cgb.CreateCommand("setmotd")
                    .MinPermissions((int) PermissionLevel.ChannelModerator) // An unrestricted say command is a bad idea
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
                    .MinPermissions((int) PermissionLevel.BotOwner) // An unrestricted say command is a bad idea
                    .Description("Administration tool, initiate channel states for all current channels.")
                    .Do(async e =>
                    {
                        foreach (var server in _client.Servers)
                        {
                            foreach (var channel in server.AllChannels)
                            {
                                Beta.ChannelStateRepository.AddChannel(channel, server);
                            }
                        }
                        await e.Channel.SendMessage("All set, Boss!");
                    });

                cgb.CreateCommand("roll")
                    .Description(
                        @"Make Beta roll the specified number of dice. For Example typing $Roll 3d12 would cause Beta to return the results of rolling 3 12-sided die. You could also role a single 12-sided die with d12.")
                    .Parameter("roll", ParameterType.Required)
                    .Do(async e =>
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
                    });

                cgb.CreateCommand("ask")
                    .Description("Ask Beta a yes or no question!")
                    .Parameter("text", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        if (!e.Server.Name.Contains("Freeworld"))
                        {
                            if (e.Message.Text.Contains("Hillary"))
                            await e.Channel.SendMessage(Configuration._8BallResponses.GetRandom());
                        }                        
                    });

                cgb.CreateCommand("motd")
                    .Description("Show the MotD for the current channel")
                    .Do(async e=>
                    {
                        if (Beta.ChannelStateRepository.VerifyChannelExists(e.Channel.Id))
                        {
                            await e.Channel.SendMessage(Beta.ChannelStateRepository.GetChannelState(e.Channel.Id).MOTD);
                            await e.Message.Delete();
                        }
                        else await e.Channel.SendMessage("Sorry dude, there is no message! harass the Admins or something.");
                    });

                cgb.CreateCommand("install")
                    .Description("Install a specific module to this channel")
                    .MinPermissions((int) PermissionLevel.BotOwner)
                    .Parameter("module", ParameterType.Required)
                    .Do(async e =>
                    {
                        switch (e.GetArg("module").ToLower())
                        {
                            case "twitter":
                                if (!Beta.Config._TwitterInstalledChannels.Contains(e.Channel.Id))
                                {
                                    Beta.Config._TwitterInstalledChannels.Add(e.Channel.Id);
                                    Beta._TwitterAuthorizedChannels.Add(e.Channel);
                                    Configuration.ConfigHandler.SaveConfig();
                                }
                                break;
                        }


                        await
                            e.Channel.SendMessage("Successfully installed the " + e.GetArg("module") +
                                                  " module to this channel.");
                    });

                cgb.CreateCommand("install")
                    .Description("Returns a link that can be used to install this bot onto other servers.")
                    .Do(async e =>
                    {
                        await
                            e.Channel.SendMessage(
                                "https://discordapp.com/oauth2/authorize?&client_id=171093139052953607&scope=bot");
                    });
                /* cgb.CreateCommand("params") // This command doesn't have any use, just an example for ParamenterType.Multiple

                .Description("Multiple paramter test")
                .Parameter("text", ParameterType.Multiple)
                .Do(async e =>
                {
                    StringBuilder output = new StringBuilder();

                    output.AppendLine("Parameters:");

                    for (int i = 0; i < e.Args.Length; i++)
                    {
                        output.AppendLine($"{i}: {e.Args[i]}");
                    }

                    await e.Channel.SendMessage(output.ToString());
                });*/
            });
        }
    }
}