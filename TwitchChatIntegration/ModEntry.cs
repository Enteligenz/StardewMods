using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

using System.Threading.Tasks;
using AsyncAwaitBestPractices;

namespace TwitchChatIntegration
{
    internal sealed class ModEntry : Mod
    {

        /// <summary> List of all possible chat colors. </summary>
        readonly Color[] ChatColors =
        {
            Color.MediumTurquoise,
            Color.SeaGreen,
            new Color(220, 20, 20),
            Color.DodgerBlue,
            new Color(50, 230, 150),
            new Color(0, 180, 10),
            new Color(182, 214, 0),
            Color.HotPink,
            new Color(240, 200, 0),
            new Color(255, 100, 0),
            new Color(138, 43, 250),
            Color.Gray,
            new Color(255, 255, 180),
            new Color(255, 180, 120),
            new Color(160, 80, 30),
            Color.Salmon,
            new Color(190, 0, 190),
        };

        /// <summary> Mod configuration from the player containing login credentials for a Twitch account. </summary>
        private ModConfig Config;

        public override async void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            var twitchBot = new TwitchBot(this.Config.Username, this.Config.Password, this.Monitor);

            twitchBot.Start().SafeFireAndForget();
            await twitchBot.JoinChannel(this.Config.TargetChannel);

            twitchBot.OnMessage += this.OnTwitchMessage;

            await Task.Delay(-1);

        }

        /// <summary> Picks a color for a specific sender and prints the message that was sent in Twitch chat into the Stardew Valley chat. </summary>
        private void OnTwitchMessage(object sender, TwitchBot.TwitchChatMessage twitchChatMessage)
        {
            // Prevent crashing if the user doesn't have a chatbox available
            if (!Context.IsWorldReady)
                return;

            // Ignore common Twitch command prefixes
            if (Config.IgnoreCommands && twitchChatMessage.Message[0] == '!')
                return;

            // Ignore users on our ignored list
            if (Config.IgnoredAccounts.Contains(twitchChatMessage.Sender, StringComparer.InvariantCultureIgnoreCase))
                return;

            int ColorIdx = Math.Abs(twitchChatMessage.Sender.GetHashCode()) % this.ChatColors.Length;
            Color chatColor = this.ChatColors[ColorIdx];

            Game1.chatBox.addMessage(twitchChatMessage.Sender + ": " + twitchChatMessage.Message, chatColor);
        }
    }
}