using System;

namespace TwitchChatIntegration
{
    public sealed class ModConfig
    {
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string TargetChannel { get; set; } = String.Empty;
        public string[] IgnoredAccounts { get; set; } = Array.Empty<string>();
        public bool IgnoreCommands { get; set; } = false;
    }
}
