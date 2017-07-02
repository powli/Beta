using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using System.Drawing;
using Beta;
using Beta.Utils;
using Beta.JSONConfig;
using Newtonsoft.Json;
using System.Net;
using System.Drawing.Drawing2D;

namespace Beta.Modules
{
    class MemeGeneratingModule : DiscordModule
    {
        private DiscordClient _client;
        private ModuleManager _manager;
        private StringFormat stringFormat;

        Font font = new Font("Impact", 200);

        string memeplateFolder = "./Memeplates/";
        string memeFolder = "./Memes/";

        public override string Prefix { get; } = Beta.Config.CommandPrefixes.Standard;

        public override void Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

            stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            if (!Directory.Exists(memeplateFolder))
            {
                Directory.CreateDirectory(memeplateFolder);
            }
            if (!Directory.Exists(memeFolder))
            {
                Directory.CreateDirectory(memeFolder);
            }

            _manager.CreateCommands("", cgb =>
            {
                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("meme")
                .Description("Memes are coming.")
                .Parameter("memeArgs", ParameterType.Optional)
                .Do(async e =>
                {
                    List<string> args = e.GetArg("memeArgs").Split('|').ToList<string>();
                    if(args.Count == 2)
                    {
                        string memeName = args[0];
                        string fileName = memeFolder + memeName + DateTime.Now.ToString();
                        args.RemoveAt(0);
                        Image imageWithMemeText = PlaceImageText(args, memeName, e);
                        if (imageWithMemeText != null)
                        {
                            imageWithMemeText.Save(fileName);
                            await e.Channel.SendFile(fileName);
                        }
                    }
                    //todo
                    await e.Channel.SendMessage("http://puu.sh/wxLKb/a33cdf1422.jpg");
                });

            });
        }

        public Image OpenImageFile(string memeName, CommandEventArgs e)
        {
            string memeFile = memeplateFolder + memeName + ".png";
            Image img = null;
            if (File.Exists(memeFile))
            {
                img = Bitmap.FromFile(memeplateFolder + memeName + ".png");
                font = new Font(font.Name, (int)(0.0625 * img.Height + 12.5)); // Changes the font to have the appropriate size based on image.
            }
            else
            {
                e.Channel.SendMessage("Sorry, I didn't see that meme. Please check the spelling!");                
            }

            return img;
        }

        //Overload if user provides four lines, separated by |'s
        public Image PlaceImageText(List<string> Lines, string memeName, CommandEventArgs e)
        {
            Image img = OpenImageFile(memeName,e);
            if (img != null)
            {
                Graphics graph = Graphics.FromImage(img);
                GraphicsPath p = new GraphicsPath();

                if (Lines.Count == 1)
                {
                    p.AddString(
                    Lines[0],
                    new FontFamily("Impact"),
                    (int)FontStyle.Regular,
                    graph.DpiY * font.SizeInPoints / 72,
                    new Point((int)(0.5 * img.Width), (int)(0.1 * img.Height)),
                    stringFormat);
                }
                else if (Lines.Count == 2)
                {
                    //FirstLine
                    p.AddString(
                    Lines[0],
                    new FontFamily("Impact"),
                    (int)FontStyle.Regular,
                    graph.DpiY * font.SizeInPoints / 72,
                    new Point((int)(0.5 * img.Width), (int)(0.1 * img.Height)),
                    stringFormat);

                    //SecondLine
                    p.AddString(
                   Lines[1],
                   new FontFamily("Impact"),
                   (int)FontStyle.Regular,
                   graph.DpiY * font.SizeInPoints / 72,
                   new Point((int)(0.5 * img.Width), (int)(0.9 * img.Height)),
                   stringFormat);
                }

                Pen pen = new Pen(Brushes.Black, font.SizeInPoints / 10); // pen for the text outline
                pen.Alignment = PenAlignment.Center;
                pen.LineJoin = LineJoin.Round; // Prevents spikes on some letters
                graph.DrawPath(pen, p); // makes the outline
                graph.FillPath(Brushes.White, p); // fills the path
            }
            
            return img;
            //todo
        }                
    }
}

