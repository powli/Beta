using Beta.Cram;
using Beta.Cram.Models;
using Beta.Repository;
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
    class CramModule : DiscordModule
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

                cgb.CreateCommand("create")
                    .Description("Create a new character. Examples:\n\n $create [name] [PHY] [MEN] [VIT] [LUC]\n $create Test1 5 5 5 5\n$create \"Test 2\" 1 15 2 2")
                    .Parameter("text", ParameterType.Multiple)
                    .Do(async e =>
                    {
                        if (Beta.CheckModuleState(e, "cram", e.Channel.IsPrivate))
                        {
                            if (!(e.Args.Count() == 5))
                            {
                                await e.Channel.SendMessage("Sorry, doesn't look like you've provided the right number of arguments.");
                            }
                            else
                            {
                                string name = e.Args[0];
                                int phy = 0;
                                int men = 0;
                                int vit = 0;
                                int luc = 0;
                                bool successfulConversion = false;
                                try
                                {
                                    phy = Convert.ToInt32(e.Args[1]);
                                    men = Convert.ToInt32(e.Args[2]);
                                    vit = Convert.ToInt32(e.Args[3]);
                                    luc = Convert.ToInt32(e.Args[4]);
                                    successfulConversion = true;
                                }
                                catch
                                {
                                    await e.Channel.SendMessage("Looks like one of your stats wasn't in number format, bub.");
                                }
                                if (successfulConversion)
                                {                                    
                                    if (ValidateScores(phy, men, vit, luc))
                                    {
                                        CramManager.AddNewCharacter(name, phy, men, vit, luc, e.User.Id);
                                        await e.Channel.SendMessage("Ok, I've added that character for you!");
                                    }
                                    else
                                    {
                                        await e.Channel.SendMessage("Those scores aren't valid! Either they don't add up to 20, or you have a score above 15 or below 1.");
                                    }
                                }
                                
                            }
                            
                        }
                    });

                cgb.CreateCommand("listchars")
                    .Description("Lists all of your characters across all games.")
                    .Alias("listchar")
                    .Do(async e =>
                    {
                        string msg = "Character List\n";
                        msg += "Character ID | Character Name | PHY | MEN | VIT | LUC | Cash | Skill Points\n";
                        msg += CramManager.GetCharacters(e.User.Id.ToString());
                        await e.Channel.SendMessage(msg);

                    });

                cgb.CreateCommand("selectchar")
                .Description("Select one of your characters by ID. You can check 'listchars' to get the ID. Example: \n\n$selectchar 2.")
                .Parameter("id", ParameterType.Required)
                .Do(async e =>
                {
                    int charId;
                    string characterName = "";
                    if (Int32.TryParse(e.GetArg("id"), out charId))
                    {
                        using (CharacterContext db = new CharacterContext)
                        {
                            Character selectedChar = db.Characters.FirstOrDefault(chr => chr.CharacterID == charId);
                            if (selectedChar != null)
                            {
                                if (selectedChar.UserId == e.User.Id.ToString())
                                {
                                    characterName = selectedChar.Name;
                                    UserState usr = Beta.UserStateRepository.GetUserState(e.User.Id);
                                    usr.SelectedCharacter = charId;
                                    usr.SelectedCharacterName = characterName;
                                    await e.Channel.SendMessage("Awesome, I selected " + characterName + "!");
                                }
                                else await e.Channel.SendMessage("Hey! THAT ISN'T YOUR CHARACTER!");
                            }
                            else await e.Channel.SendMessage("Sorry, I don't see a character with that ID...");
                        }
                    }
                    else await e.Channel.SendMessage("That's not a digit, my dude.");
                    
                });

                cgb.CreateCommand("selectedchar")
                .Description("Shows the currently selected character, if any.")
                .Do(async e =>
                {
                    UserState usr = Beta.UserStateRepository.GetUserState(e.User.Id);
                    if (usr.SelectedCharacter != 0)
                    {
                        await e.Channel.SendMessage(usr.SelectedCharacter + " | "+usr.SelectedCharacterName);
                    }
                    else await e.Channel.SendMessage("Looks like you don't actually have a character selected, buddy.");
                    
                });

                cgb.CreateCommand("buy")
                    .Description("Purchase an item. Provide the iteam id found from the 'listitems' command. Example: \n\n$buy 3.")
                    .Parameter("id", ParameterType.Required)
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage("Good Deal! I've added that item to your inventory. Except I really didn't because Dart was too lazy to implment this fully yet. GG.");
                    });

                cgb.CreateCommand("inv")
                    .Description("Lists the items in your inventory.")
                    .Do(async e =>
                    {
                        UserState usrState = Beta.UserStateRepository.GetUserState(e.User.Id);
                        if (usrState.SelectedCharacter != 0)
                        {
                            string msg = usrState.SelectedCharacterName+"'s Inventory\n";
                            msg += "Item ID | Item Name | Item Description | Item Cost | Quantity";
                            msg += CramManager.GetCharacterItems(usrState.SelectedCharacter);
                            await e.Channel.SendMessage(msg);
                        }
                        
                    });

                cgb.CreateCommand("listskills")
                    .Description("Lists the generic list of skills.")
                    .Do(async e =>
                    {
                        string msg = "Skill List\n";
                        msg += "Skill ID | Skill Name | Skill Description\n";
                        msg += CramManager.GetSkills();
                        await e.Channel.SendMessage(msg);
                    });

                cgb.CreateCommand("listitems")
                    .Description("Lists the generic list of items..")
                    .Do(async e =>
                    {
                        string msg = "Item List\n";
                        msg += "Item ID | Item Name | Item Description | Item Cost\n";
                        msg += CramManager.GetItems();
                        await e.Channel.SendMessage(msg);
                    });

            });
        }

        private bool ValidateScores(int phy, int men, int vit, int luc)
        {
            return (phy + men + vit + luc == 20) && (0 < phy && phy < 16) && (0 < men && men < 16) && (0 < vit && vit < 16) && (0 < luc && luc < 16);
        }
    }
}
