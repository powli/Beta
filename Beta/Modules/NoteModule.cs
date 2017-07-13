using System;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Beta.Repository;
using Beta.Utils;

namespace Beta.Modules
{
    class NoteModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;
        public override string Prefix { get; } = "$";

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", cgb =>
            {                
                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("note")
                .Description("Return the server note with the specified name")
                .Parameter("name", ParameterType.Unparsed)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "note", e.Channel.IsPrivate))
                    {
                        NoteRepository noteRepository =
                            Beta.ServerStateRepository.GetServerState(e.Server.Id).NoteRepository;
                        Note note = noteRepository.GetNote(e.GetArg("name"));
                        if (note != null)
                        {
                            await e.Channel.SendMessage(note.Text);
                            Beta.UserStateRepository.ModifyUserFavorability(e.User.Id, 1);
                        }
                        else
                        {
                            await e.Channel.SendMessage("Sorry, I don't see a server note by the name of '" + e.GetArg("name") +
                                                  "', " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ".");
                            Beta.UserStateRepository.ModifyUserFavorability(e.User.Id, -1);
                        }


                    }
                });

                cgb.CreateCommand("notes")
                .Description("Return all server notes.")
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "note", e.Channel.IsPrivate))
                    {
                        NoteRepository noteRepository =
                            Beta.ServerStateRepository.GetServerState(e.Server.Id).NoteRepository;
                        string msg = "";

                        foreach ( Note note in noteRepository.Notes )
                        {
                            msg += note.Name + ": " + note.Text + "\n";
                        }

                        await e.Channel.SendMessage(msg);
                    }
                });

                cgb.CreateCommand("deletenote")
                .Description("Delete the named server note.")
                .Parameter("text", ParameterType.Unparsed)
                .MinPermissions((int)PermissionLevel.ChannelModerator)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "note", e.Channel.IsPrivate))
                    {
                        if (
                            Beta.ServerStateRepository.GetServerState(e.Server.Id)
                                .NoteRepository.NoteExists(e.GetArg("text")))
                        {
                            await e.Channel.SendMessage("Ok, deleted that note for you, " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + "! Bye bye '" + e.GetArg("text")+ "'.");

                            Beta.ServerStateRepository.GetServerState(e.Server.Id)
                                .NoteRepository.DeleteQuote(e.GetArg("text"));
                            Beta.ServerStateRepository.Save();
                            Beta.UserStateRepository.ModifyUserFavorability(e.User.Id, 1);
                        }
                        else
                        {
                            await
                                e.Channel.SendMessage("Sorry, I was unable to find a note named '" + e.GetArg("text") +
                                                      "', " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ".");
                            Beta.UserStateRepository.ModifyUserFavorability(e.User.Id, -1);
                        }
                            
                    }
                });
               
                cgb.CreateCommand("addnote")
                .Description("Add a server note by the specified name. $add Note Name|Whatever the note should say.")
                .Parameter("text", ParameterType.Unparsed)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "note", e.Channel.IsPrivate))
                    {
                        var args = e.GetArg("text").Split('|');
                        Console.WriteLine("Test");
                        NoteRepository temp = Beta.ServerStateRepository.GetServerState(e.Server.Id).NoteRepository;
                        temp.NoteExists(args[0]);
                        Console.WriteLine("Test2");
                        if (temp.NoteExists(args[0]))
                        {                            
                            await
                                e.Channel.SendMessage("Sorry, looks like we already have a note named '" + args[0] +
                                                      "' on this server, " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ".");
                            Beta.UserStateRepository.ModifyUserFavorability(e.User.Id, -1);
                        }
                        else
                        {
                            Beta.ServerStateRepository.GetServerState(e.Server.Id)
                                .NoteRepository.AddNote(args[0], args[1], e.User.Name);

                            await
                                e.Channel.SendMessage(String.Format("Successfully added note '{0}', " + Nicknames.GetNickname(Beta.UserStateRepository.GetUserState(e.User.Id).Favorability) + ".",
                                    args[0]));
                            Beta.UserStateRepository.ModifyUserFavorability(e.User.Id, 1);
                            Beta.ServerStateRepository.Save();

                        }                        
                    }
                });
            });
        }
    }
}
