using System;
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


                
            });
        }
    }
}
