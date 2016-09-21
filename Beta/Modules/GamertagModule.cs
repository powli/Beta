using System;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Modules;
using Discord.Commands.Permissions.Levels;
using Beta.Repository;

namespace Beta.Modules
{
    internal class GamertagModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;

        public override string Prefix { get; } = "!";

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;


            _manager.CreateCommands("", cgb =>
            {
                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("reg")
                    .Description("Register your Gamertag.")
                    .Parameter("text", ParameterType.Multiple)
                    .Do(async e =>
                    {
                        if (Beta.CheckModuleState(e.Server.Id, e.Channel.Id, "gamertag"))
                        {
                            switch (e.Args[0].ToLower())
                            {
                                case "xbl":
                                    Beta.GamertagRepository.NewGamertag(e.Args[1].Trim(), "Xbox Live",
                                        e.Message.User.Name, e.Message.User.Id);
                                    await
                                        e.Channel.SendMessage("Added that gamertag for you, " + e.Message.User.Name +
                                                              ".");
                                    break;
                                case "psn":
                                    Beta.GamertagRepository.NewGamertag(e.Args[1].Trim(), "Playstation Network",
                                        e.Message.User.Name, e.Message.User.Id);
                                    await
                                        e.Channel.SendMessage("Added that gamertag for you, " + e.Message.User.Name +
                                                              ".");
                                    break;
                                case "nnid":
                                    Beta.GamertagRepository.NewGamertag(e.Args[1].Trim(), "Nintendo Network",
                                        e.Message.User.Name, e.Message.User.Id);
                                    await
                                        e.Channel.SendMessage("Added that gamertag for you, " + e.Message.User.Name +
                                                              ".");
                                    break;
                                case "steam":
                                    Beta.GamertagRepository.NewGamertag(e.Args[1].Trim(), "Steam", e.Message.User.Name,
                                        e.Message.User.Id);
                                    await
                                        e.Channel.SendMessage("Added that gamertag for you, " + e.Message.User.Name +
                                                              ".");
                                    break;
                                case "b.n":
                                    Beta.GamertagRepository.NewGamertag(e.Args[1].Trim(), "Battle.Net",
                                        e.Message.User.Name, e.Message.User.Id);
                                    await
                                        e.Channel.SendMessage("Added that gamertag for you, " + e.Message.User.Name +
                                                              ".");
                                    break;
                                default:
                                    await
                                        e.Channel.SendMessage(
                                            "Sorry, I don't recognize that type of gamertag. I currently recognize the following types of Gamertags (With explination in parenthesis): ");
                                    await
                                        e.Channel.SendMessage(
                                            "XBL (Xbox Live), PSN (Playstation Network), NNID (Nintendo Network), Steam, B.N (Battle.Net)");
                                    break;
                            }
                        }
                    });

                /* LEGACY REGISTER COMMANDS */
                /*
                cgb.CreateCommand("regpsn")                    
                    .Description("Register your PSN Gamertag.")
                    .Parameter("text", ParameterType.Required)
                    .Do(async e => { 
                        Beta.GamertagRepository.NewGamertag(e.GetArg("text").Trim(),"Playstation Network", e.Message.User.Name,e.Message.User.Id);
                        await e.Channel.SendMessage("Added that gamertag for you, "+e.Message.User.Name+"."); 
                    });

                cgb.CreateCommand("regxbl")                    
                    .Description("Register your Xbox Live Gamertag.")
                    .Parameter("text", ParameterType.Required)
                    .Do(async e => { 
                        Beta.GamertagRepository.NewGamertag(e.GetArg("text").Trim(),"Xbox Live", e.Message.User.Name,e.Message.User.Id);
                        await e.Channel.SendMessage("Added that gamertag for you, "+e.Message.User.Name+"."); 
                    });
                
                cgb.CreateCommand("regb.n")                    
                    .Description("Register your Battle.Net Gamertag.")
                    .Parameter("text", ParameterType.Required)
                    .Do(async e => { 
                        Beta.GamertagRepository.NewGamertag(e.GetArg("text").Trim(),"Battle.Net", e.Message.User.Name, e.Message.User.Id);
                        await e.Channel.SendMessage("Added that gamertag for you, "+e.Message.User.Name+".");                    
                    });
                                                                                                                                                    */
                /*                                                                                                                                  */

                cgb.CreateCommand("glist")                    
                    .Description("Returns a list of every stored Gamertag.")
                    .Do(async e =>
                    {
                        if (Beta.CheckModuleState(e.Server.Id, e.Channel.Id, "gamertag"))
                        {
                            bool anyExist = false;
                            string list = "";
                            if (Beta.GamertagRepository.Gamertags.FirstOrDefault(g => g.GamertagType == "Steam") != null)
                            {
                                list += "**Steam Gamertags:**\n";
                                foreach (Gamertag tag in Beta.GamertagRepository.Gamertags)
                                {
                                    if (tag.GamertagType == "Steam")
                                    {
                                        anyExist = true;
                                        list += "Discord: **" + tag.DiscordUsername + "** Steam: **" + tag.GTag + "**\n";
                                    }
                                }
                            }
                            if (Beta.GamertagRepository.Gamertags.FirstOrDefault(g => g.GamertagType == "Battle.Net") !=
                                null)
                            {
                                list += "**Battle.Net Gamertags:**\n";
                                foreach (Gamertag tag in Beta.GamertagRepository.Gamertags)
                                {
                                    if (tag.GamertagType == "Battle.Net")
                                    {
                                        anyExist = true;
                                        list += "Discord: **" + tag.DiscordUsername + "** Battle.Net: **" + tag.GTag +
                                                "**\n";
                                    }
                                }
                            }
                            if (Beta.GamertagRepository.Gamertags.FirstOrDefault(g => g.GamertagType == "Xbox Live") !=
                                null)
                            {
                                list += "**Xbox Live Gamertags:**\n";
                                foreach (Gamertag tag in Beta.GamertagRepository.Gamertags)
                                {
                                    if (tag.GamertagType == "Xbox Live")
                                    {
                                        anyExist = true;
                                        list += "Discord: **" + tag.DiscordUsername + "** XBL: **" + tag.GTag + "**\n";
                                    }
                                }
                            }
                            if (
                                Beta.GamertagRepository.Gamertags.FirstOrDefault(
                                    g => g.GamertagType == "Playstation Network") != null)
                            {
                                list += "**Playstation Network Gamertags:**\n";
                                foreach (Gamertag tag in Beta.GamertagRepository.Gamertags)
                                {
                                    if (tag.GamertagType == "Playstation Network")
                                    {
                                        anyExist = true;
                                        list += "Discord: **" + tag.DiscordUsername + "** PSN: **" + tag.GTag + "**";
                                    }
                                }
                            }
                            if (
                                Beta.GamertagRepository.Gamertags.FirstOrDefault(
                                    g => g.GamertagType == "Nintendo Network") != null)
                            {
                                list += "**Nintendo Network Gamertags:**\n";
                                foreach (Gamertag tag in Beta.GamertagRepository.Gamertags)
                                {
                                    if (tag.GamertagType == "Nintendo Network")
                                    {
                                        anyExist = true;
                                        list += "Discord: **" + tag.DiscordUsername + "** NNID: **" + tag.GTag + "**\n";
                                    }
                                }
                            }
                            if (anyExist) await e.Channel.SendMessage(list);
                            else await e.Channel.SendMessage("Sorry, I don't have any Gamertags stored yet :-/");
                        }
                    });

                cgb.CreateCommand("retrieve")
                    .Description("Returns all of the Gamertags stored for specified Discord user.")
                    .Parameter("text", ParameterType.Required)
                    .Do(async e =>{
                                      if (Beta.CheckModuleState(e.Server.Id, e.Channel.Id, "gamertag"))
                                      {
                                          bool found = false;
                                          string retrieved = "";
                                          foreach (Gamertag tag in Beta.GamertagRepository.Gamertags)
                                          {
                                              if (tag.DiscordUsername == e.GetArg("text"))
                                              {
                                                  found = true;
                                                  retrieved += "Discord Username: **" + tag.DiscordUsername +
                                                               "** Gamertag: **" + tag.GTag + "** Network: **" +
                                                               tag.GamertagType + "** Gamertag ID: **" + tag.ID + "**\n";
                                              }

                                          }
                                          if (found) await e.Channel.SendMessage(retrieved);
                                          else
                                              await
                                                  e.Channel.SendMessage(
                                                      "Sorry, I didn't see any gamertags for that person.");
                                      }
                    });

                cgb.CreateCommand("remove")
                    .Description("Removes the specified gamertag. Please provide the GamertagID, which can be retrieved using the '$retrieve' command.")
                    .Parameter("ID", ParameterType.Required)
                    .Do(async e =>{
                                      if (Beta.CheckModuleState(e.Server.Id, e.Channel.Id, "gamertag"))
                                      {
                                          int id;
                                          if (Int32.TryParse(e.GetArg("ID"), out id))
                                          {
                                              Gamertag removeTag =
                                                  Beta.GamertagRepository.Gamertags.FirstOrDefault(g => g.ID == id);
                                              if (removeTag != null)
                                              {
                                                  if (removeTag.DiscordID == e.Message.User.Id)
                                                  {
                                                      Beta.GamertagRepository.Gamertags.Remove(removeTag);
                                                      await
                                                          e.Channel.SendMessage(
                                                              "Ok cool I've removed that gamertag for you! ");
                                                      Beta.GamertagRepository.Save();
                                                  }
                                                  else
                                                      await
                                                          e.Channel.SendMessage(
                                                              "Hey that's not yours! Leave it alone, dude.");
                                              }
                                              else
                                              {
                                                  await
                                                      e.Channel.SendMessage(
                                                          "Sorry, I didn't find that gamertag. Are you sure you have the right number?");
                                              }
                                          }
                                      }
                    });               
            });
        }
    }
}