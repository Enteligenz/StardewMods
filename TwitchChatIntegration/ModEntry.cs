using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using GenericModConfigMenu;
using StardewModdingAPI.Events;

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
            this.Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            this.Config = this.Helper.ReadConfig<ModConfig>();

            var twitchBot = new TwitchBot(this.Config.Username, this.Config.Password, this.Monitor);
            twitchBot.OnMessage += this.OnTwitchMessage;

            twitchBot.Start().SafeFireAndForget();
            await twitchBot.JoinChannel(this.Config.TargetChannel);
            await Task.Delay(-1);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            configMenu.SetTitleScreenOnlyForNextOptions(mod: this.ModManifest, true);

            // Twitch Connection Stuff
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Twitch Connection Info"
            );

            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "NOTE: Any changes to the Twitch Connection Info requires a game restart in order to apply."
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Chat Username",
                tooltip: () => "The name of the Twitch account you want to use as login",
                getValue: () => this.Config.Username,
                setValue: value => this.Config.Username = value,
                fieldId: "username"
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Target Channel",
                tooltip: () => "The name of the Twitch channel you want to read the chat of",
                getValue: () => this.Config.TargetChannel, 
                setValue: value => this.Config.TargetChannel = value,
                fieldId: "target"
            );

            configMenu.AddPageLink(
                mod: this.ModManifest,
                pageId: "twitchPassword",
                text: () => "Click to set chat password",
                tooltip: () => "DO NOT CLICK THIS ON STREAM");

            configMenu.SetTitleScreenOnlyForNextOptions(mod: this.ModManifest, false);

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Behavior Options"
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Ignore Common Chat Commands",
                tooltip: () => "This will make any message that's prefixed with a ! to not appear",
                getValue: () => this.Config.IgnoreCommands,
                setValue: value => this.Config.IgnoreCommands = value
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "User Accounts to Filter",
                tooltip: () => "Comma separated list of account names to ignore from the chat",
                getValue: () => string.Join(",", this.Config.IgnoredAccounts),
                setValue: value => {
                    // Remove any spaces the user has
                    value = value.Replace(" ", string.Empty);
                    if (string.IsNullOrWhiteSpace(value))
                        this.Config.IgnoredAccounts = Array.Empty<string>();
                    else
                        this.Config.IgnoredAccounts = value.Split(',');
                }
            );

            configMenu.SetTitleScreenOnlyForNextOptions(mod: this.ModManifest, true);

            configMenu.AddPage(
                mod: this.ModManifest,
                pageId: "twitchPassword",
                pageTitle: () => "Twitch OAuth Password"
            );

            configMenu.AddParagraph(
                mod: this.ModManifest,
                text: () => "Note that you will need to fully restart the game if you change the value below"
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Chat Password",
                tooltip: () => $"This is the account password for the twitch account set as the username {this.Config.Username}",
                getValue: () => this.Config.Password,
                setValue: value => this.Config.Password = value,
                fieldId: "password"
            );
        }

        /// <summary> Picks a color for a specific sender and prints the message that was sent in Twitch chat into the Stardew Valley chat. </summary>
        private void OnTwitchMessage(object sender, TwitchBot.TwitchChatMessage twitchChatMessage)
        {
            // Prevent crashing if the user doesn't have a chatbox available
            // Also ignore any messages that are empty
            if (!Context.IsWorldReady || string.IsNullOrEmpty(twitchChatMessage.Message))
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