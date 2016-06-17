using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Beta.Utils;
using Beta.JSONConfig;

namespace Beta.Modules
{
    class StandardModule : DiscordModule
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

                cgb.CreateCommand("say")
                .MinPermissions((int)PermissionLevel.BotOwner) // An unrestricted say command is a bad idea
                .Description("Make the bot speak!")
                .Parameter("text", ParameterType.Unparsed)
                .Do(async e =>
                {
                    await e.Channel.SendMessage(e.GetArg("text"));
                });

                cgb.CreateCommand("roll")
                .Description(@"Make Beta roll the specified number of dice. For Example typing $Roll 3d12 would cause Beta to return the results of rolling 3 12-sided die. You could also role a single 12-sided die with d12.")
                .Parameter("roll", ParameterType.Required)
                .Do(async e =>
                {
                    Console.WriteLine(string.Format("This is a test, yo: {0}", e.GetArg("roll")));
                    string arg = e.GetArg("roll");
                    List<string> arguments = arg.Split('d').Where<string>(s => !string.IsNullOrEmpty(s)).ToList();
                    Console.WriteLine(string.Format("This is test 2, showing the split argument: {0}", string.Join(",", arguments)));
                    int sides, times, modifier;

                    if (arguments.Count == 1)
                    {
                        if (arguments[0].Contains("+"))
                        {
                            arguments = arguments[0].Split('+').ToList();
                            if (Int32.TryParse(arguments[0], out sides))
                            {
                                Dice dice = new Dice(sides);
                                int temp = dice.Roll();
                                if (Int32.TryParse(arguments[1], out modifier))
                                {
                                    await e.Channel.SendMessage(string.Format("Rolled one d{0}) plus {1} and got a total of {2}", sides, modifier, temp + modifier));
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
                            if (Int32.TryParse(arguments[0], out sides))
                            {
                                Dice dice = new Dice(sides);
                                await e.Channel.SendMessage(string.Format("Rolled one d{0} and got a total of {1}", sides, dice.Roll()));
                            }
                            else
                            {
                                await e.Channel.SendMessage("Sorry, I don't recognize that number");
                            }
                        }

                    }
                    else if (arguments.Count == 2)
                    {
                        if (Int32.TryParse(arguments[0], out times))
                        {
                            if (arguments[1].Contains("+"))
                            {
                                arguments = arguments[1].Split('+').ToList();
                                if (Int32.TryParse(arguments[0], out sides))
                                {
                                    Dice dice = new Dice(sides);
                                    Result temp = dice.Roll(times);
                                    if (Int32.TryParse(arguments[1], out modifier))
                                    {
                                        await e.Channel.SendMessage(string.Format("Rolled {0} d{1} plus {2} and got a total of {3}", times, sides, modifier, temp.Total + modifier));
                                        await e.Channel.SendMessage(string.Format("Individual Rolls: {0}", string.Join(",", temp.Rolls)));
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
                                if (Int32.TryParse(arguments[1], out sides))
                                {
                                    Dice dice = new Dice(sides);
                                    Result temp = dice.Roll(times);
                                    await e.Channel.SendMessage(string.Format("Rolled {0} d{1} and got a total of {2}", times, sides, temp.Total));
                                    await e.Channel.SendMessage(string.Format("Individual Rolls: {0}", string.Join(",", temp.Rolls)));

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
                    await e.Channel.SendMessage(Configuration._8BallResponses.GetRandom());
                });

                cgb.CreateCommand("install")
                .Description("Install a specific module to this channel")
                .MinPermissions((int)PermissionLevel.BotOwner)
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


                    await e.Channel.SendMessage("Successfully installed the " + e.GetArg("module") + " module to this channel.");
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