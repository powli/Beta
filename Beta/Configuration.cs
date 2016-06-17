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
        public static List<string> _8BallResponses { get; set; } = new List<string>
            {
                "Most definitely yes",
                "For sure",
                "As I see it, yes",
                "My sources say yes",
                "Yes",
                "Most likely",
                "Perhaps",
                "Maybe",
                "Not sure",
                "It is uncertain",
                "Ask me again later",
                "Don't count on it",
                "Probably not",
                "Very doubtful",
                "Most likely no",
                "Nope",
                "No",
                "My sources say no",
                "Dont even think about it",
                "Definitely no",
                "NO - It may cause disease contraction"
            };
        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterAccessToken { get; set; }
        public string TwitterAccessSecret { get; set; }
        public List<ulong> _TwitterInstalledChannels { get; set; } = new List<ulong>();

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
