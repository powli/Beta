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
        private List<string> Memes;

        Font font = new Font("Impact", 200);

        string memeplateFolder = "Memeplates/";
        string memeFolder = "Memes/";

        public override string Prefix { get; } = Beta.Config.CommandPrefixes.Standard;

        public override void Install(ModuleManager manager)
        {
            if (!Directory.Exists(memeplateFolder))
            {
                Directory.CreateDirectory(memeplateFolder);
            }
            if (!Directory.Exists(memeFolder))
            {
                Directory.CreateDirectory(memeFolder);
            }

            _manager = manager;
            _client = manager.Client;
            Memes = GetMemeList();

            stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            

            _manager.CreateCommands("", cgb =>
            {
                cgb.MinPermissions((int)PermissionLevel.User);

                cgb.CreateCommand("meme")
                .Description("Generate your own meme on the fly! Examples:\n\n$meme \"Random|TopText|BottomText\n$meme YUNo|TopTextOnly\n\nSee $memelist for available Memes are just use Random!\"")
                .Parameter("memeArgs", ParameterType.Required)
                .Do(async e =>
                {
                    List<string> args = e.GetArg("memeArgs").Split('|').ToList<string>();
                    if(args.Count >= 2)
                    {
                        string memeName = args[0];
                        if (memeName.ToLower() == "random")
                        {
                            memeName = Memes.GetRandom();
                            Console.WriteLine(memeName);
                        }
                        string fileName = memeFolder + memeName + DateTime.Now.ToString("hhmmss")+".png";
                        Console.WriteLine(fileName);
                        args.RemoveAt(0);
                        Image imageWithMemeText = PlaceImageText(args, memeName, e);
                        if (imageWithMemeText != null)
                        {
                            imageWithMemeText.Save(fileName);
                            await e.Channel.SendFile(fileName);
                        }
                    }
                });

                cgb.CreateCommand("memelist")
                .Alias("listmemes")
                .Description("Have a list of available Memes PM'd to you")
                .Do(async e =>
                {
                    Memes = GetMemeList();
                    string message = "Here are the memes I have available: \n";
                    foreach (string meme in Memes)
                    {
                        message += meme + "\n";
                    }
                    await e.User.SendMessage(message);
                });

                cgb.CreateCommand("addmeme")
                .Description("Add the specified link to an image as a meme, with the given name. Example:\n\n$addmeme \"OverlyAttachedGirlfriend|https://imgflip.com/s/meme/Overly-Attached-Girlfriend.jpg\"")
                .Parameter("text", ParameterType.Required)
                .MinPermissions((int)PermissionLevel.ChannelModerator)
                .Do(async e =>
                {
                    List<string> args = e.GetArg("text").Split('|').ToList<string>();
                    string memeName = args[0];
                    string link = args[1];
                    using(WebClient client = new WebClient())
                    {
                        try
                        {
                            client.DownloadFile(new Uri(link), memeplateFolder + memeName + ".png");
                        }
                        catch
                        {
                            await e.Channel.SendMessage("Sorry, I wasn't able to download that image. Check your link out, my dude.");
                        }
                    }
                    await e.Channel.SendMessage("Ok, I've added that Memeplate for you!");
                });
            });
        }

        private List<string> GetMemeList()
        {
            return Directory.GetFiles(memeplateFolder).Select(fileName => Path.GetFileNameWithoutExtension(fileName)).ToList<string>();            
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

        //Adjust Font and return one of proper size
        public Font GetAdjustedFont(Graphics GraphicRef, string GraphicString, Font OriginalFont, int ContainerWidth, int MaxFontSize, int MinFontSize, bool SmallestOnFail)
        {
            // We utilize MeasureString which we get via a control instance           
            for (int AdjustedSize = MaxFontSize; AdjustedSize >= MinFontSize; AdjustedSize--)
            {
                Font TestFont = new Font(OriginalFont.Name, AdjustedSize, OriginalFont.Style);

                // Test the string with the new size
                SizeF AdjustedSizeNew = GraphicRef.MeasureString(GraphicString, TestFont);

                if (ContainerWidth > Convert.ToInt32(AdjustedSizeNew.Width))
                {
                    // Good font, return it
                    return TestFont;
                }
            }

            // If you get here there was no fontsize that worked
            // return MinimumSize or Original?
            if (SmallestOnFail)
            {
                return new Font(OriginalFont.Name, MinFontSize, OriginalFont.Style);
            }
            else
            {
                return OriginalFont;
            }
        }

        public Image PlaceImageText(List<string> Lines, string memeName, CommandEventArgs e)
        {
            Image img = OpenImageFile(memeName,e);
            if (img != null)
            {
                int width = img.Width;
                Graphics graph = Graphics.FromImage(img);
                GraphicsPath p = new GraphicsPath();

                if (Lines.Count == 1)
                {
                    font = GetAdjustedFont(graph, "DD" + Lines[0] + "DD", font, width, Convert.ToInt32(font.SizeInPoints), 10, true);
                    p.AddString(
                    Lines[0].ToUpper(),
                    new FontFamily("Impact"),
                    (int)FontStyle.Regular,
                    graph.DpiY * font.SizeInPoints / 72,
                    new Point((int)(0.5 * img.Width), (int)(0.1 * img.Height)),
                    stringFormat);
                }
                else if (Lines.Count == 2)
                {
                    if (Lines[0].Length > Lines[1].Length)
                    {
                        font = GetAdjustedFont(graph, "DD"+Lines[0]+"DD", font, width, Convert.ToInt32(font.Size), 10, true);
                    }
                    else font = GetAdjustedFont(graph, "DD" + Lines[1] + "DD", font, width, Convert.ToInt32(font.Size), 10, true);
                    //FirstLine

                    p.AddString(
                    Lines[0].ToUpper(),
                    new FontFamily("Impact"),
                    (int)FontStyle.Regular,
                    graph.DpiY * font.SizeInPoints / 72,
                    new Point((int)(0.5 * img.Width), (int)(0.1 * img.Height)),
                    stringFormat);

                    //SecondLine
                    
                    p.AddString(
                   Lines[1].ToUpper(),
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

