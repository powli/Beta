using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Beta.JSONConfig
{
    public class Configuration
    {

        public string Token { get; set; }
        public string BotID { get; set; }
        public CommandPrefixesModel CommandPrefixes { get; set; } = new CommandPrefixesModel();        
        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterAccessToken { get; set; }
        public string TwitterAccessSecret { get; set; }
        public string GithubAccessToken { get; set; }
        public string LastGithubCommit { get; set; }
        public List<ulong> _TwitterInstalledChannels { get; set; } = new List<ulong>();
        public List<ulong> _TrustedUsers { get; set; } = new List<ulong> { 94545463906144256 };

        public class CommandPrefixesModel
        {
            public string Standard { get; set; } = "$";
            public string Quote { get; set; } = "$";
            public string Twitter { get; set; } = "$";
        }



        public static class ConfigHandler
        {
            private static readonly object configLock = new object();
            public static void SaveConfig()
            {
                lock (configLock)
                {
                    File.WriteAllText("data/config.json", JsonConvert.SerializeObject(Beta.Config, Formatting.Indented));
                }
            }

        }

    }
}
