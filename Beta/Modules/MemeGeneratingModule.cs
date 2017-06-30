using System;
using System.Collections.Generic;
using System.Linq;
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

        //Overload if user provides four lines, separated by |'s
        public static void PlaceImageText(List<string> Lines, string memeName)
        {
            //Graphics graph = Graphics.FromImage(img);
            if (Lines.Count == 1)
            {

            }
            //todo
        }

        /* public void updateImg()
        {
            // reset the Bitmap and reload the image
            img = null;
            img = new Bitmap(filePath);

            Graphics graph = Graphics.FromImage(img);
            GraphicsPath p = new GraphicsPath();

            // Adds the text in textBoxUpper
            p.AddString(
                textBoxUpper.Text,
                new FontFamily("Impact"),
                (int)FontStyle.Regular,
                graph.DpiY * font.SizeInPoints / 72,
                new Point((int)(0.5 * img.Width), (int)(0.1 * img.Height)),
                stringFormat);

            // Adds the text in textBoxLower
            p.AddString(
                textBoxLower.Text,
                new FontFamily("Impact"),
                (int)FontStyle.Regular,
                graph.DpiY * font.SizeInPoints / 72,
                new Point((int)(0.5 * img.Width), (int)(0.9 * img.Height)),
                stringFormat);

            Pen pen = new Pen(Brushes.Black, font.SizeInPoints / 10); // pen for the text outline
            pen.Alignment = PenAlignment.Center;
            pen.LineJoin = LineJoin.Round; // Prevents spikes on some letters
            graph.DrawPath(pen, p); // makes the outline
            graph.FillPath(Brushes.White, p); // fills the path

            // sets the pictureBox.Image to to edited one
            pictureBox1.Image = img;
        }*/
    }
}

