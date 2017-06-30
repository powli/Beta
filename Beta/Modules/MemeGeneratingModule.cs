using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Beta;
using Beta.Utils;
using Beta.JSONConfig;
using Newtonsoft.Json;
using System.Net;

namespace Beta.Modules
{
    class MemeGeneratingModule : DiscordModule
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

                cgb.CreateCommand("meme")
                .Description("Memes are coming.")
                .Parameter("meme", ParameterType.Optional)
                .Do(async e =>
                {
                    //todo
                    await e.Channel.SendMessage("http://puu.sh/wxLKb/a33cdf1422.jpg");
                });

            });
        }


    }
}

