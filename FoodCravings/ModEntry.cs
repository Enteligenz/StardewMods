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
using System.Runtime.CompilerServices;

namespace FoodCravings
{
    internal sealed class ModEntry : Mod
    {
        string DailyCravingDisplayName;
        bool CravingFulfilled;
        Buff cravingBuff;
        Buff cravingDebuff;
        private ModConfig Config;
        bool isHangryMode;
        List<string> recipeBlacklist;

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.isHangryMode = this.Config.useHangryMode;
            this.recipeBlacklist = this.Config.recipeBlacklist;

            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.UpdateTicked += this.OneSecondUpdateTicked;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnCravingCommand(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Load a save first.", LogLevel.Info);
                return;
            }

            if (this.DailyCravingDisplayName is null)
            {
                Monitor.Log("No craving today (no kitchen or all recipes blacklisted).", LogLevel.Info);
                return;
            }

            Monitor.Log($"Today's craving: {this.DailyCravingDisplayName}.", LogLevel.Info);
        }

        /// <summary> Tries to find the display name for a given recipe key. </summary>
        private string GetRecipeDisplayName(string recipeKey)
        {
            try
            {
                CraftingRecipe recipe = new CraftingRecipe(recipeKey, isCookingRecipe: true);
                return recipe.DisplayName;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Couldn't resolve display name for recipe '{recipeKey}': {ex.Message}", LogLevel.Warn);
                return recipeKey;
            }
        }

        /// <summary> Handles GMCM support for modifying configs in game. </summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Get Generic Mod Config Menu's API (if it's installed)
            IGenericModConfigMenuApi configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // Register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this.Config)
            );

            // Console command for checking the daily craving
            Helper.ConsoleCommands.Add(
                name: "craving",
                documentation: this.Helper.Translation.Get("cmd.craving"),
                callback: this.OnCravingCommand
            );

            // GMCM Options
            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("menu.main-title")
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.no-kitchen"),
                tooltip: () => this.Helper.Translation.Get("menu.no-kitchen-desc"),
                getValue: () => this.Config.skipIfNoKitchen,
                setValue: value => this.Config.skipIfNoKitchen = value
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.seeded-random"),
                tooltip: () => this.Helper.Translation.Get("menu.seeded-random-desc"),
                getValue: () => this.Config.useSeededRandom,
                setValue: value => this.Config.useSeededRandom = value
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

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("menu.buff-title")
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

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("menu.debuff-title")
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("menu.hangry-mode"),
                tooltip: () => this.Helper.Translation.Get("menu.hangry-mode-desc"),
                getValue: () => this.Config.useHangryMode,
                setValue: value => this.Config.useHangryMode = value
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
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            // Update recipe blacklist
            this.recipeBlacklist = this.Config.recipeBlacklist;

            // Get list of display names of all valid recipes (must be known, not on the blacklist and be from vanilla or a mod that is still installed)
            HashSet<string> loadedRecipeKeys = CraftingRecipe.cookingRecipes.Keys.ToHashSet();
            List<string> knownRecipeKeys = Game1.player.cookingRecipes.Keys.Where(k => loadedRecipeKeys.Contains(k)).ToList(); // Drop recipes from removed mods
            List<string> knownRecipeNames = knownRecipeKeys.Select(GetRecipeDisplayName).ToList();
            List<string> validRecipes = knownRecipeNames.Where(r => !recipeBlacklist.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

            if (this.Config.skipIfNoKitchen && Game1.player.HouseUpgradeLevel < 1)
            {
                Monitor.Log("Player does not have the kitchen unlocked yet, so daily craving will be skipped.", LogLevel.Debug);
                this.DailyCravingDisplayName = null;
                return;
            }

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

            // Check if there are any blacklist items that do not match any known recipe
            foreach (string entry in Config.recipeBlacklist)
            {
                if (!knownRecipeNames.Contains(entry, StringComparer.OrdinalIgnoreCase))
                    Monitor.Log($"Recipe blacklist entry '{entry}' doesn't match any known recipe display name.", LogLevel.Warn);
            }

            // Check if there are known recipes remaining after applying the blacklist
            if (validRecipes.Count == 0)
            {
                Monitor.Log("All known recipes are blacklisted, so daily craving will be skipped.", LogLevel.Warn);
                this.DailyCravingDisplayName = null;
                return;
            }

            // Pick which random number generator to use
            if (this.Config.useSeededRandom)
            {
                Random rnd = Utility.CreateDaySaveRandom(49173);
                this.DailyCravingDisplayName = validRecipes[rnd.Next(validRecipes.Count)];
            }
            else 
            {
                Random rnd = new Random();
                this.DailyCravingDisplayName = validRecipes[rnd.Next(validRecipes.Count)];
            }

            //foreach (string rec in this.recipeBlacklist)
            //    this.Monitor.Log($"recipe blacklist: {rec}.", LogLevel.Debug);

            // Display HUD message naming the daily craving
            Game1.addHUDMessage(new HUDMessage(this.Helper.Translation.Get("buff.hud-msg") + this.DailyCravingDisplayName, 2));

            // Reset flag (buffs seem to automatically reset on next day)
            this.CravingFulfilled = false;

            // Apply craving debuff if necessary
            if (this.isHangryMode)
            {
                Game1.player.applyBuff(this.cravingDebuff);
            }
        }

        private void OneSecondUpdateTicked(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady || this.DailyCravingDisplayName is null)
                return;

            if (!Game1.player.isEating || this.CravingFulfilled) // Player is not eating or craving has already been fulfilled before
                return;

            Item CurrentFood = Game1.player.itemToEat;

            if (!this.DailyCravingDisplayName.Equals(CurrentFood.DisplayName)) // Player is eating food that is not craved
                return;

            this.CravingFulfilled = true;

            Game1.player.applyBuff(this.cravingBuff);
            if (this.isHangryMode)
                Game1.player.buffs.Remove("Hexenentendrache.FoodCravings_Debuff"); // Remove debuff
        }
    }
}