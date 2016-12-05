using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Beta.Repository;

namespace Beta.Modules
{
    class ChatBattleModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;
        public override string Prefix { get; } = "$";
        public List<string> WeaponPrefixList = new List<string>()
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

        public List<string> WeaponList = new List<string>()
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

        public List<string> WeaponSuffixList = new List<string>()
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

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            manager.CreateCommands("", cgb =>
            {
                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("stats")
                .Description("Check your stats for Chat Battle, sent via on PM")
                .Do(async e =>
                {
                    Beta.UserStateRepository.AddUser(e.User);
                    UserState usr = Beta.UserStateRepository.GetUserState(e.User.Id);
                    if (usr.RPGHitpoints == -1) usr.RPGHitpoints = usr.RPGMaxHP;
                    await e.User.SendMessage(String.Format("Level: {0} HP: {1}/{2} Gold: {3} XP: {4} Kills: {5} Deaths: {6} ", usr.RPGLevel, usr.RPGHitpoints, usr.RPGMaxHP, usr.RPGGold, usr.RPGXP, usr.RPGWins, usr.RPGLosses) );
                });

                cgb.CreateCommand("attack")
                .Description("Attack your target")
                .Parameter("target", ParameterType.Required)
                .Do(async e =>
                {
                    Random r = new Random();
                    UserState attacker = Beta.UserStateRepository.GetUserState(e.User.Id);
                    UserState target = null;
                    if (e.Channel.Users.FirstOrDefault(u => u.Name == e.GetArg("target")) != null)
                    {
                        User usr = e.Channel.Users.FirstOrDefault(u => u.Name == e.GetArg("target"));
                        Beta.UserStateRepository.AddUser(usr);
                        target = Beta.UserStateRepository.GetUserState(usr.Id);
                        if (target.RPGHitpoints == -1) target.RPGHitpoints = target.RPGMaxHP;
                    }
                    int damage = (int) ((attacker.RPGLevel*.25)*r.Next(4, 50));
                    await
                        e.Channel.SendMessage(
                            String.Format("{0} attacked {1} with the {2} {3} of {4} for {5} points of damage!",
                            e.User.Name, e.GetArg("target"), WeaponPrefixList.GetRandom(), WeaponList.GetRandom(),
                            WeaponSuffixList.GetRandom(), damage ));
                    if (target != null)
                    {
                        if (target.RPGHitpoints == 0)
                        {
                            await
                                e.Channel.SendMessage(
                                    "Oh. Sorry, it looks like that target is currently dead. They'll have to res themselves before you can attack them for gold and xp!");
                        }
                        else
                        {
                            target.RPGHitpoints -= damage;
                            if (target.RPGHitpoints < 1)
                            {
                                target.RPGHitpoints = 0;
                                int xp = 1;
                                int gold = (target.RPGLevel) * r.Next(1, 25);
                                if (target.RPGLevel > attacker.RPGLevel)
                                    xp = 1 + target.RPGLevel - attacker.RPGLevel;
                                else if (target.RPGLevel - attacker.RPGLevel < -3)
                                    xp = 0;
                                await
                                    e.Channel.SendMessage(
                                        String.Format("That hit killed {0}! {1} found {2} gold on their corpse! Gained {3} XP!",
                                            target.UserName, e.User.Name, gold, xp));
                                attacker.RPGGold += gold;
                                attacker.RPGXP += xp;
                                attacker.CheckLevelUp(e);
                            }
                        }
                    }
                    
                });
                
            });
        }
    }
}
