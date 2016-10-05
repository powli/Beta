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

public class XKCDComic
{
    public string month { get; set; }
    public int num { get; set; }
    public string link { get; set; }
    public string year { get; set; }
    public string news { get; set; }
    public string safe_title { get; set; }
    public string transcript { get; set; }
    public string alt { get; set; }
    public string img { get; set; }
    public string title { get; set; }
    public string day { get; set; }
}

namespace Beta.Modules
{
    class ComicModule : DiscordModule
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

                cgb.CreateCommand("xkcd")
                .Description("Post either the latest comic from XKCD, or the specified comic number. rnd will provide a random comic.")
                .Parameter("comic", ParameterType.Optional)
                .Do(async e =>
                {
                    if (Beta.CheckModuleState(e, "comic", e.Channel.IsPrivate))
                    {
                        if (e.GetArg("comic") == "")
                        {
                            XKCDComic comic = _download_serialized_json_data<XKCDComic>("http://xkcd.com/info.0.json");
                            await e.Channel.SendMessage(comic.img);
                            await e.Channel.SendMessage(comic.alt);
                        }
                        else if (e.GetArg("comic") == "rnd")
                        {
                            XKCDComic comic = _download_serialized_json_data<XKCDComic>("http://xkcd.com/info.0.json");
                            int latestComic = comic.num;
                            int random = new Random().Next(1, latestComic + 1);
                            string url = String.Format("http://xkcd.com/{0}/info.0.json", random);
                            comic = _download_serialized_json_data<XKCDComic>(url);
                            await e.Channel.SendMessage(comic.img);
                            await e.Channel.SendMessage(comic.alt);
                        }
                        else
                        {
                            int comicnum;
                            if (Int32.TryParse(e.GetArg("comic"), out comicnum))
                            {
                                string url = String.Format("http://xkcd.com/{0}/info.0.json", e.GetArg("comic").Trim());
                                XKCDComic comic = _download_serialized_json_data<XKCDComic>(url);
                                await e.Channel.SendMessage(comic.img);
                                await e.Channel.SendMessage(comic.alt);
                            }
                            else
                            {
                                await e.Channel.SendMessage("uh... sorry, but that doesn't look like a number dude.");
                            }

                        }
                    }
                });
                
            });
        }

        private static XKCDComic _download_serialized_json_data<T>(string url)
        {
            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    json_data = w.DownloadString(url);
                }
                catch (Exception)
                {
                }
                var comic = new XKCDComic();
                if (string.IsNullOrEmpty(json_data))
                {
                    comic.img = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/52/Bundesstra%C3%9Fe_404_number.svg/2000px-Bundesstra%C3%9Fe_404_number.svg.png";
                    comic.alt = "Sorry, I couldn't find that comic number.";
                    return comic;
                }
                else
                {
                    return JsonConvert.DeserializeObject<XKCDComic>(json_data);
                }                
            }
        }
    }
}
