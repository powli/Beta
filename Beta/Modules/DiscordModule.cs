using Discord.Modules;
using System.Collections.Generic;
using Beta.Classes;

namespace Beta.Modules {
    internal abstract class DiscordModule : IModule {
        protected readonly HashSet<DiscordCommand> commands = new HashSet<DiscordCommand>();

        public abstract string Prefix { get; }

        public abstract void Install(ModuleManager manager);
    }
}