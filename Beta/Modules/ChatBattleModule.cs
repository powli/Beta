using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Discord;
using Beta.Utils;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Beta.Repository;
using RestSharp.Validation;

namespace Beta.Modules
{    
    class ChatBattleModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;
        private BetaUserState BetaState;
        Random r = new Random();
        public override string Prefix { get; } = "$";

        #region Weapon Building Lists
        public static List<string> WeaponPrefixList = new List<string>()
        {
            "Ancient",
            "Abysmal",
            "Chaotic",
            "Lawful",
            "Infernal",
            "Angelic",
            "Celestial",
            "Demonic",
            "Holy",
            "Floppy",
            "Spongey",
            "Shitty",
            "Pointy",
            "Savage",
            "Feral",
            "Slippery",
            "Swift",
            "Skewering",
            "Rabid",
            "Ferocious",
            "Gentleman's",
            "Lady's",
            "Flaming",
            "Fragile",
            "Frozen",
            "Arcane",
            "Ashen",
            "Balanced",
            "Barbaric",
            "Barbed",
            "Brutal",
            "Extra-heavy",
            "fickle",
            "Green",
            "Hateful",
            "Lovecraftian",
            "Bizzare",
            "Rusty",
            "Rusted",
            "Pristine",
            "Legendary",
            "Hell-Forged",
            "Heavy",
            "Malign",
            "Lucky",
            "Gilded",
            "Precise",
            "Mighty",
            "Karmic",
            "Magical",
            "Experienced",
            "Hefty",
            "Light",
            "Masterwork",
            "Mild",
            "Murderous",
            "Nasty",
            "Poisonous",
            "Red",
            "Strange",
            "Ultra-heavy",
            "Unbalanced",
            "Unwieldy",
            "Weeping",
            "White",
            "Wicked",
            "Yellow",
            "Monstrous",
            "Operational",
            "Surgical",
            "Potent",
            "Vengeful"
        };

        public static List<string> WeaponList = new List<string>()
        {
            "Sword",
            "Bastard Sword",
            "Fish Hook",
            "Shotgun",
            "Sawed-off Shotgun",
            "Wrench",
            "Grenade",
            "Wet Towel",
            "Whip",
            "Scimitar",
            "Harpoon",
            "Sling",
            "Trout",
            "Das Boot",
            "Moringstar",
            "Mace",
            "Baseball Bat",
            "Hookshot",
            "Magic Rod",
            "Stiletto",
            "Dirk",
            "Bolo",
            "Balisword",
            "RPG",
            "BFG",
            "Dagger",
            "Kunai",
            "Katana",
            "Lamp Post",
            "Street Sign",
            "Flamethrower",
            "Drill",
            "Chainsaw",
            "Pickaxe",
            "Repeater",
            "Sickle",
            "Kama",
            "Polearm",
            "Spear",
            "Guandao",
            "Stick",
            "Shiv",
            "Microwave Gun",
            "Gravity Gun",
            "Portal Gun",
            "Cat",
            "Gladius",
            "Viking Sword",
            "Arming Sword",
            "Estoc",
            "Kris",
            "Balisong Knife",
            "Dagger",
            "Claymore",
            "Longsword",
            "Rapier",
            "Jintachi",
            "Wakizashi",
            "Uchigatana",
            "Cutlass",
            "Backsword",
            "Katzbalger",
            "Sabina",
            "Waraxe",
            "M16",
            "Molotov Cocktail",
            "Desert Eagle",
            "Garrote Wire",
            "Candlestick",
            "Rusty Tin Can",
            "Genie's Lamp",
            "Used Condom",
            "Double Helix Bong",
            "Vaporizer",
            "Land Mine",
            "Nuclear Football",
            "Shield",
            "Wooden Club",
            "Staff",
            "Wand",
            "Bow",
            "Crossbow",
            "Goat",
            "Boomerang",
            "Squirtgun",
            "Rifle",
            "Artifact"
        };

        public static List<string> WeaponSuffixList = new List<string>()
        {
            "Antioch",
            "Erfworld",
            "Westeros",
            "Middle Earth",
            "Might",
            "Hunting",
            "Devastation",
            "Fumbling",
            "Damnation",
            "Corruption",
            "Penetration",
            "Power",
            "Surrendering",
            "Retreat",
            "Slaughtering",
            "The Void",
            "Cthulhu",
            "Vampirism",
            "Weakness",
            "the Eagle",
            "the Sun",
            "the Moon",
            "Augmentation",
            "Transmutation",
            "Nyarlathotep",
            "Azathoth",
            "Yibb-Tsll",
            "Thor",
            "Loki",
            "Sizer",
            "Friendly Fire",
            "DarkForce",
            "Kalis",
            "Anubis",
            "Ishtar",
            "Ra",
            "Gilgamesh",
            "Nabu",
            "Sin",
            "Ninlil",
            "Marduk",
            "Tian",
            "Yu Di",
            "Cao Guojiu",
            "Han Xiangzi",
            "Han Zhongli",
            "He Xiangu",
            "Lan Caihe",
            "Lü Dongbin",
            "Tie Guaili",
            "Zhang Guolao",
            "Raijin",
            "Izanagi",
            "Inari Okami",
            "Suijin",
            "Tenjin",
            "Vishnu",
            "Shiva",
            "Brahma",
            "Ganesha",
            "Tala",
            "Hanan",
            "Bathala",
            "Mayari",
            "Diyan",
            "Jupiter",
            "Juno",
            "Neptune",
            "Mars",
            "Venus",
            "Baldr",
            "Odin",
            "Eir",
            "Frigg",
            "Freyja"
        };
        #endregion

        #region String Formats
        string KalisAirlock = "Kalis pushes {0} out of the airlock, then returns to his command chair like nothing happened";
        string DartAirlock = "Dart pushes {0} out of the airlock, blowing them a kiss \"Goodbye\".";
        string SizerAirlock = "Sizer pushes {0} out of the airlock, lights a cigar, and returns to his throne";
        string FFAirlock ="FriendlyFire materializes an airlock around {0} then pushes them out of it, grunting something about having work to do.";
        string DFAirlock = "DarkForce pushes {0} out of the airlock, feels great about himself for being such a go-getter for about five minutes, then has a panic attack for the next five hours over having just committed murder. His Google searches over this period of time include \'Can the police discover bodies in space?\', \'Will bodies, evidence, organic matter burn up upon atmospheric reentry?\', and \'Does it count as murder if your victim was really, really, super aggravating?\'";
        string DefaultAirlock = "{0} pushes {1} out of the airlock, cackaling manaically. So long, "+Nicknames.GetNickname(-500.0)+"!";
        string AdminFail = "{0} attempted to push {1} out of the airlock, however {1} sidesteps and pushes them out, instead. What a chump.";
        string AirlockFail = "{0} tried to push {1} out of the airlock, however {0} tripped and fell out instead... Better luck next time!";
        string RPGStats = "Level: {0} HP: {1}/{2} Stamina: {3}/{4}Gold: {5} XP: {6} Kills: {7} Deaths: {8} ";
        string RPGInv = "Healing Potions: {0} Stamina Potions: {1}";
#endregion

        public List<string> Admins = new List<string>()
        {
            "sizer",
            "friendlyfire",
            "dart",
            "darkforce",
            "kalis"
        };        

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;
            

            manager.CreateCommands("", cgb =>
            {
                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("stats")
                .Description("Check your stats for Chat Battle, sent via PM")
                .Do(async e =>
                {
                    Beta.UserStateRepository.AddUser(e.User);
                    UserState usr = Beta.UserStateRepository.GetUserState(e.User.Id);
                    if (usr.RPGHitpoints == -1) usr.RPGHitpoints = usr.RPGMaxHP;
                    await e.User.SendMessage(String.Format(RPGStats, usr.RPGLevel, usr.RPGHitpoints, usr.RPGMaxHP, usr.RPGStamina, usr.RPGMaxStamina, usr.RPGGold, usr.RPGXP, usr.RPGWins, usr.RPGLosses) );
                    Console.WriteLine(String.Format("[Chat Battle] {0} has requested their stats.",usr.UserName));
                    Console.WriteLine(String.Format("[Chat Battle] "+RPGStats, usr.RPGLevel, usr.RPGHitpoints, usr.RPGMaxHP, usr.RPGStamina, usr.RPGMaxStamina, usr.RPGGold, usr.RPGXP, usr.RPGWins, usr.RPGLosses));
                });

                cgb.CreateCommand("res")
                .Description("Spend a percentage of your gold to return you to the living with max health! Beta will probably take some of your gold.")
                .Do(async e =>
                {
                    UserState target = Beta.UserStateRepository.GetUserState(e.User.Id);
                    int cost = Convert.ToInt32(target.RPGGold * (.1 + (.01 * target.RPGLevel)));

                    if (!target.Alive)
                    {
                        Beta.UserStateRepository.ResUser(cost, target.UserId);
                        await e.User.SendMessage("Heroes never die!");
                        await e.User.SendMessage(String.Format("Oh. Also I took {0} gold. You know, for my trouble ;D", cost));
                    }
                    else
                    {
                        await e.User.SendMessage("Sorry, bub. Looks like you're still kicking, try dying first.");
                    }


                });


                cgb.CreateCommand("push")
                .Description("Pushes the target out of an airlock. Can only be used by Admins")
                .Parameter("target", ParameterType.Unparsed)
                .Do(async e =>
                {
                    bool nuke = false;
                    string msg = "";
                    string target = e.GetArg("target").Trim();
                    UserState pusher = Beta.UserStateRepository.GetUserState(e.User.Id);
                    UserState pushee = Beta.UserStateRepository.UserStates.FirstOrDefault(us => us.UserName == target);
                    bool isAdmin = Admins.Contains(target.ToLower());                  
                    Console.WriteLine("isAdmin: "+isAdmin+" Target: "+target);
                    switch (e.User.Name)
                    {
                        case "Dart":
                            nuke = true;
                            msg = String.Format(DartAirlock, target);
                            break;
                        case "Kalis":
                            nuke = true;
                            msg = String.Format(KalisAirlock, target);
                            break;
                        case "Sizer":
                            nuke = true;
                            msg = String.Format(SizerAirlock, target);
                            break;
                        case "FriendlyFire":
                            nuke = true;
                            msg = String.Format(FFAirlock, target);
                            break;
                        case "DarkForce":
                            nuke = true;
                            msg = String.Format(DFAirlock, target);
                            break;
                        default:
                            if (e.Channel.Id == 93924120042934272)
                            {
                                msg = String.Format(AirlockFail, e.User.Name, target);
                                if (isAdmin) msg = String.Format(AdminFail, e.User.Name, target);
                            }                            
                            break;
                    }
                    if (msg != "") await e.Channel.SendMessage(msg);
                    if (nuke && pushee != null)
                    {
                        await e.Channel.SendMessage(
                            String.Format("As {0}'s body floats through space they slowly begin to lose consciousness...",target));
                        await e.Channel.SendMessage(String.Format("{0} has slain {1}! FATALITY!",e.User.Name,target));
                        pusher.Die();
                    }
                    else if (!nuke && pusher != null)
                    {
                        await e.Channel.SendMessage(
                            String.Format(
                                "As {0}'s body floats through space they slowly begin to lose consciousness...", pusher.UserName));
                        await e.Channel.SendMessage(
                            String.Format("{0} managed to get themselves lost in the cold vacuum of space...", pusher.UserName));
                        Thread.Sleep(1500);
                        await
                            e.Channel.SendMessage(
                                String.Format("A robotic hand closes on {0}'s wrist and drags them back in. They feel exhausted, but they'll live.",e.User.Name));
                        pusher.RPGStamina = 0;
                    }
                });

                cgb.CreateCommand("attack")
                .Description("Attack your target")
                .Parameter("target", ParameterType.Unparsed)
                .Do(async e =>
                {
                    Result combatResult;

                    Result npcResult = null;
                    BetaState = (BetaUserState)Beta.UserStateRepository.NPCUserStates.FirstOrDefault(nu => nu.UserName == "Beta");
                    UserState attacker = Beta.UserStateRepository.GetUserState(e.User.Id);
                    UserState target = null;
                    UserState beta = Beta.UserStateRepository.UserStates.FirstOrDefault(us => us.UserName == "Beta");
                    if (attacker == null)
                    {
                        Beta.UserStateRepository.AddUser(e.User);
                        attacker = Beta.UserStateRepository.GetUserState(e.User.Id);
                    }
                    decimal stamCost = (decimal)(attacker.RPGLevel*.25);
                    int damage = (int)(stamCost * r.Next(4, 50));
                    if (Beta.CheckModuleState(e, "battle", e.Channel.IsPrivate) && attacker.Alive)
                    {
                        if (ValidChatBattleTarget(e))
                        {
                            target = GetTargetUserState(e);
                        }
                        if (target != null && attacker.RPGStamina >= stamCost)
                        {
                            attacker.RPGStamina -= stamCost;
                            if (attacker.RPGStamina < stamCost) await e.User.SendMessage("You feel exhausted...");
                            if (target.RPGHitpoints == 0 && target.UserName == "Beta") target.Res(1);
                            if (!target.Alive)
                            {
                                await
                                    e.Channel.SendMessage(
                                        "Oh. Sorry, it looks like that target is currently dead. They'll have to res themselves before you can attack them for gold and xp!");
                            }
                            else
                            {
                                combatResult = attacker.Attack(target, e);
                                await
                                    e.Channel.SendMessage(
                                        String.Format(
                                            "{0} attacked {1} with the {2} {3} of {4} for {5} points of damage! You drew blood!",
                                            e.User.Name, e.GetArg("target"), WeaponPrefixList.GetRandom(),
                                            WeaponList.GetRandom(),
                                            WeaponSuffixList.GetRandom(), combatResult.Damage));


                                if (!target.IsBot())
                                {
                                    #region Target Death Result

                                    if (target.UserId != attacker.UserId)
                                    {
                                        if (combatResult.TargetDead)
                                        {
                                            await
                                                e.Channel.SendMessage(
                                                    String.Format(
                                                        "That hit killed {0}! {1} found {2} gold on their corpse! Gained {3} XP!",
                                                        target.UserName, e.User.Name, combatResult.Spoils.Gold,
                                                        combatResult.Spoils.XP));
                                            attacker.CheckLevelUp(e);
                                            if (combatResult.Spoils.HealthPot > 0 || combatResult.Spoils.StamPot > 0)
                                            {
                                                await
                                                    e.Channel.SendMessage(
                                                        String.Format(
                                                            "Looks like you also managed to find some potions! Gained {0} Health Potions and {1} Stamina Potions.",
                                                            combatResult.Spoils.HealthPot,
                                                            combatResult.Spoils.StamPot));
                                            }
                                        }
                                    }
                                        #endregion

                                    #region Player Attacked Themselves

                                    else if (target.UserId == attacker.UserId)
                                    {
                                        await e.Channel.SendMessage(
                                            String.Format(
                                                "Oh. Looks like {0} managed to kill themselves. You lost {1} XP.",
                                                e.User.Name, target.RPGLevel*3));
                                        target.Die();
                                        if (r.Next(1, 5) == 3)
                                        {
                                            int lostGold = r.Next(1, 5)*target.RPGLevel;
                                            await
                                                e.Channel.SendMessage(
                                                    String.Format(
                                                        "Also, a bandit seems to have picked your corpse's pocket. Lose {0} gold.",
                                                        lostGold));
                                            if (lostGold > target.RPGGold)
                                            {
                                                UserState.BanditGold += target.RPGGold;
                                                target.RPGGold = 0;
                                            }
                                            else
                                            {
                                                UserState.BanditGold += lostGold;
                                                target.RPGGold -= lostGold;
                                            }
                                            if (3*target.RPGLevel > target.RPGXP)
                                            {
                                                target.RPGXP = 0;
                                            }
                                            else target.RPGXP -= target.RPGLevel*3;
                                        }
                                    }

                                    #endregion
                                }
                                else
                                {

                                    /*if (target.UserName == "R2-D2")
                                    {
                                        //This section is going to be a little messy until the new UserStates are in place.
                                        BetaState.DefendBot(target,attacker, e, combatResult);
                                    }*/
                                    if (target.UserName == "Beta")
                                    {
                                        BetaState.CounterAttack(attacker, e, combatResult);
                                    }
                                    else
                                    {
                                        npcResult = target.Attack(attacker,e);
                                        await
                                            e.Channel.SendMessage(
                                                String.Format(
                                                    "{0} struck back with the {1}{2} of {3} for {4} points of damage!",target.UserName,WeaponPrefixList.GetRandom(),WeaponList.GetRandom(),WeaponSuffixList.GetRandom(),npcResult.Damage));

                                        //Both died
                                        if (npcResult.TargetDead && combatResult.TargetDead)
                                        {
                                            await
                                                e.Channel.SendMessage(
                                                    String.Format(
                                                        "{0} and {1} managed to kill each other! {0} gained {2} XP and {1} gained {3} XP",
                                                        target.UserName, attacker.UserName, npcResult.Spoils.XP, combatResult.Spoils.XP));
                                            Beta.UserStateRepository.NPCUserStates.Remove((NPCUserState)target);
                                        }
                                        //NPC died, player survived
                                        else if (!npcResult.TargetDead && combatResult.TargetDead)
                                        {
                                            await e.Channel.SendMessage(String.Format("{0} managed to kill the {1}! Gained {2} XP, {3} Gold, {4} Health Potions, and {5} Stamina Potions",attacker.UserName,target.UserName, combatResult.Spoils.XP, combatResult.Spoils.Gold, combatResult.Spoils.HealthPot, combatResult.Spoils.StamPot));
                                            Beta.UserStateRepository.NPCUserStates.Remove((NPCUserState) target);
                                        }
                                        //Player died, NPC survived
                                        else if (npcResult.TargetDead && !combatResult.TargetDead)
                                        {
                                            await e.Channel.SendMessage(String.Format("{0} managed to kill the {1}! Gained {2} XP, {3} Gold, {4} Health Potions, and {5} Stamina Potions", target.UserName, attacker.UserName, combatResult.Spoils.XP, combatResult.Spoils.Gold, combatResult.Spoils.HealthPot, combatResult.Spoils.StamPot));
                                        }
                                    }
                                }
                            }

                        }
                        else if (target == null)
                        {
                            await
                                e.Channel.SendMessage(
                                    String.Format(
                                        "{0} attacked {1} with the {2} {3} of {4} for {5} points of damage!",
                                        e.User.Name, e.GetArg("target"), WeaponPrefixList.GetRandom(),
                                        WeaponList.GetRandom(),
                                        WeaponSuffixList.GetRandom(), damage));
                        }                        
                    }
                    else if (!attacker.Alive)
                    {
                        await e.User.SendMessage("YOU'RE DEAD DING DONG!");
                    }

                });

                cgb.CreateCommand("drink")                
                .Description("Drink either a Stamina or Healing potion. Stamina potions recover 2-4 Stamina and Healing potions recover 12-25 Hitpoints")
                .Parameter("potionType", ParameterType.Unparsed)
                .Do(async e =>
                {
                    UserState usr = Beta.UserStateRepository.GetUserState(e.User.Id);
                    if (usr == null)
                    {
                        Beta.UserStateRepository.AddUser(e.User);
                        usr = Beta.UserStateRepository.GetUserState(e.User.Id);
                    }                  
                    switch (e.GetArg("potionType").Trim().ToLower())
                    {
                        case "healing":
                            if (usr.RPGHealingPotions > 1)
                            {
                                await
                                    e.User.SendMessage(String.Format("You drank a healing poition! Healed for {0}!",
                                        usr.DrinkHealingPotion()));
                            }
                            else
                            {
                                await e.User.SendMessage("You don't even have any healing potions!");
                            }
                            break;
                        case "stamina":
                            if (usr.RPGStaminaPotions > 1)
                            {
                                await
                                    e.User.SendMessage(String.Format("You drank a Stamina poition! Recovered {0} stamina!",
                                        usr.DrinkStaminaPotion()));
                            }
                            else
                            {
                                await e.User.SendMessage("You don't even have any Stamina potions!");
                            }
                            break;
                        default:
                            await
                                e.User.SendMessage(
                                    "Sorry, I don't recognize that type of potion. Please use either \"$drink healing\" or \"$drink stamina\".");
                            break;
                    }
                });

                cgb.CreateCommand("npclist")
                    .Description("Returns a list of NPCs which can be attacked")
                    .Do(async e =>
                    {
                        string msg = "The following NPCs are available for attack:\n";
                        foreach (NPCUserState npc in Beta.UserStateRepository.NPCUserStates)
                        {
                            msg += npc.UserName + " Level: "+npc.RPGLevel+"\n";
                        }
                        await e.User.SendMessage(msg);
                    });
                
                cgb.CreateCommand("inventory")
                .Alias("inv")
                .Description("Check your inventory for Chat battle, sent via PM")
                .Do(async e =>
                {
                    UserState usr = Beta.UserStateRepository.GetUserState(e.User.Id);
                    await e.User.SendMessage(String.Format(RPGInv, usr.RPGHealingPotions, usr.RPGStaminaPotions));
                });
            });
        }

        public bool ValidChatBattleTarget(CommandEventArgs e)
        {
            bool userExists = e.Channel.Users.FirstOrDefault(u => u.Name == e.GetArg("target")) != null;
            bool npcExists = Beta.UserStateRepository.NPCUserStates.FirstOrDefault(u => u.UserName == e.GetArg("target")) != null;
            return userExists || npcExists;
        }

        public UserState GetTargetUserState(CommandEventArgs e)
        {
            User usr = e.Channel.Users.FirstOrDefault(u => u.Name == e.GetArg("target"));
            if (usr != null)
            {
                if (Beta.UserStateRepository.VerifyUsersExists(usr.Id))
                {
                    var target = Beta.UserStateRepository.GetUserState(usr.Id);
                    if (target.RPGHitpoints == -1) target.RPGHitpoints = target.RPGMaxHP;
                    return target;
                }
            }            
            if (Beta.UserStateRepository.VerifyNPCExists(e.GetArg("target")))
            {
                var target =
                    Beta.UserStateRepository.NPCUserStates.FirstOrDefault(nu => nu.UserName == e.GetArg("target"));
                Console.WriteLine("Target is null : "+(target == null));
                return target;
            }
            return null;
        }

        public User GetUser(ulong id)
        {
            foreach (Server srvr in _client.Servers)
            {
                foreach (User usr in srvr.Users)
                {
                    if (usr.Id == id) return usr;
                }
            }
            return null;
        }       
    }
}
