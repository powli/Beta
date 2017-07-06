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

                cgb.CreateCommand("reg")
                    .Description("Register your Gamertag.")
                    .Parameter("text", ParameterType.Multiple)
                    .Do(async e =>
                    {
                        if (Beta.CheckModuleState(e, "CRAM", e.Channel.IsPrivate))
                        {
                            
                        }
                    });


            });
        }
    }
}
