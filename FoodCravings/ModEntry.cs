using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using GenericModConfigMenu;
using System.IO;

namespace FoodCravings
{
    internal sealed class ModEntry : Mod
    {
        Random rnd = new Random();
        string DailyCravingKey;
        string DailyCravingName;
        bool CravingFulfilled;
        Buff cravingBuff;
        Buff cravingDebuff;
        //Dictionary<string, string> recipeDict = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
        private ModConfig Config;
        bool isHangryMode;
        List<string> recipeBlacklist;

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.isHangryMode = this.Config.useHangryMode;
            this.recipeBlacklist = this.Config.recipeBlacklist;

            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }


        /// <summary> Handles GMCM support for modifying configs in game. </summary>
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

            //configMenu.SetTitleScreenOnlyForNextOptions(mod: this.ModManifest, true);

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.hangry-mode"),
                tooltip: () => this.Helper.Translation.Get("menu.hangry-mode-desc"),
                getValue: () => this.Config.useHangryMode,
                setValue: value => this.Config.useHangryMode = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.attack-buff"),
                tooltip: () => this.Helper.Translation.Get("menu.attack-buff-desc"),
                getValue: () => this.Config.attackBuff,
                setValue: value => this.Config.attackBuff = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.defense-buff"),
                tooltip: () => this.Helper.Translation.Get("menu.defense-buff-desc"),
                getValue: () => this.Config.defenseBuff,
                setValue: value => this.Config.defenseBuff = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.speed-buff"),
                tooltip: () => this.Helper.Translation.Get("menu.speed-buff-desc"),
                getValue: () => this.Config.speedBuff,
                setValue: value => this.Config.speedBuff = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.attack-debuff"),
                tooltip: () => this.Helper.Translation.Get("menu.attack-debuff-desc"),
                getValue: () => this.Config.attackDebuff,
                setValue: value => this.Config.attackDebuff = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.defense-debuff"),
                tooltip: () => this.Helper.Translation.Get("menu.defense-debuff-desc"),
                getValue: () => this.Config.defenseDebuff,
                setValue: value => this.Config.defenseDebuff = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.speed-debuff"),
                tooltip: () => this.Helper.Translation.Get("menu.speed-debuff-desc"),
                getValue: () => this.Config.speedDebuff,
                setValue: value => this.Config.speedDebuff = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.buff-duration"),
                tooltip: () => this.Helper.Translation.Get("menu.buff-duration-desc"),
                getValue: () => this.Config.buffDuration,
                setValue: value => this.Config.buffDuration = value
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.blacklist"),
                tooltip: () => this.Helper.Translation.Get("menu.blacklist-desc"),
                getValue: () => string.Join(", ", this.Config.recipeBlacklist),
                setValue: value => this.Config.recipeBlacklist = value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList()
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.seeded-random"),
                tooltip: () => this.Helper.Translation.Get("menu.seeded-random-desc"),
                getValue: () => this.Config.seededRandom,
                setValue: value => this.Config.seededRandom = value
            );
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Create buffs based on current config values
            this.cravingBuff = new Buff(
                id: "Hexenentendrache.FoodCravings_Buff",
                displayName: this.Helper.Translation.Get("buff.fulfilled"),
                iconTexture: this.Helper.ModContent.Load<Texture2D>("assets/food_craving_icon.png"),
                duration: this.Config.buffDuration,
                effects: new StardewValley.Buffs.BuffEffects()
                {
                    Attack = { this.Config.attackBuff },
                    Defense = { this.Config.defenseBuff },
                    Speed = { this.Config.speedBuff }
                }
            );

            this.cravingDebuff = new Buff(
                id: "Hexenentendrache.FoodCravings_Debuff",
                displayName: this.Helper.Translation.Get("buff.unfulfilled"),
                iconTexture: this.Helper.ModContent.Load<Texture2D>("assets/food_craving_debuff_icon.png"),
                duration: Buff.ENDLESS,
                effects: new StardewValley.Buffs.BuffEffects()
                {
                    Attack = { this.Config.attackDebuff },
                    Defense = { this.Config.defenseDebuff },
                    Speed = { this.Config.speedDebuff }
                }
            );

            // Update recipe blacklist
            this.recipeBlacklist = this.Config.recipeBlacklist;

            // Get list of all known recipes
            List<string> knownRecipes = Game1.player.cookingRecipes.Keys.ToList();

            // Randomize food craving until non-blacklisted food is found
            if (this.Config.seededRandom)
            {
                this.rnd = new Random(Game1.Date.ToString().GetHashCode());
            }
            while (true)
            {
                this.DailyCravingKey = knownRecipes.ElementAt(this.rnd.Next(0, knownRecipes.Count));

                // Find the proper display name of the food
                this.DailyCravingName = this.DailyCravingKey; // For vanilla food (and some older mods) the key name will be the same as the display name
                if (CraftingRecipe.cookingRecipes.TryGetValue(this.DailyCravingKey, out string recipe))
                {
                    string[] recipeParts = recipe.Split('/');
                    if (recipeParts.Length == 6) // afaik modded food will follow this format, where the last part of the recipe is the name we want
                    {
                        this.DailyCravingName = recipeParts[5]; // Modded food might use i18n format as key, so we need to replace it with sth more readable
                    }
                }

                if (!this.recipeBlacklist.Contains(this.DailyCravingName))
                {
                    break;
                }
            }
            
            foreach (string rec in this.recipeBlacklist)
            {
                this.Monitor.Log($"recipe blacklist: {rec}.", LogLevel.Debug);
            }

            // Display HUD message naming the daily craving
            Game1.addHUDMessage(new HUDMessage(this.Helper.Translation.Get("buff.hud-msg") + this.DailyCravingName, 2));

            // Reset flag (buffs seem to automatically reset on next day)
            this.CravingFulfilled = false;

            // Apply craving debuff if necessary
            if (this.isHangryMode)
            {
                // Game1.buffsDisplay.addOtherBuff(this.cravingDebuff);
                Game1.player.applyBuff(this.cravingDebuff);
            }
        }

        private void OnUpdateTicked(object sender, EventArgs e)
        {
            if (!Game1.player.isEating || this.CravingFulfilled) // Player is not eating or craving has already been fulfilled before
            {
                return;
            }

            Item CurrentFood = Game1.player.itemToEat;

            if (!this.DailyCravingKey.Equals(CurrentFood.Name)) // Player is eating food that is not craved
            {
                return;
            }

            this.CravingFulfilled = true;

            // Game1.buffsDisplay.addOtherBuff(this.cravingBuff); // Add buff for fulfilled craving
            Game1.player.applyBuff(this.cravingBuff);
            if (this.isHangryMode)
            {
                    Game1.player.buffs.Remove("Hexenentendrache.FoodCravings_Debuff"); // Remove debuff
            }
        }
    }
}