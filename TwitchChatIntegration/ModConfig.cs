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

        public bool IsValid()
        {
            Func<string, bool> FieldValid = (string field) =>
            {
                return !string.IsNullOrWhiteSpace(field);
            };

            return FieldValid.Invoke(this.Username) && 
                FieldValid.Invoke(this.Password) && 
                FieldValid.Invoke(this.TargetChannel) &&
                this.Password.Contains("oauth:");
        }
    }
}
