using Beta.Cram;
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
                    .Description("Lists all of your characters accross all games.")
                    .Alias("listchar")
                    .Do(async e =>
                    {
                        string msg = "Character List\n";
                        msg += "Character Name | PHY | MEN | VIT | LUC | Cash | Skill Points|";
                        msg += CramManager.GetCharacters(e.User.Id.ToString());
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
