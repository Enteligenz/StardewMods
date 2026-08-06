using System;
using System.Collections.Generic;

namespace FoodCravings
{
    public sealed class ModConfig
    {
        public bool skipIfNoKitchen { get; set; } = true;
        public bool useSeededRandom { get; set; } = false;
        public int attackBuff { get; set; } = 2;
        public int defenseBuff { get; set; } = 2;
        public int speedBuff { get; set; } = 1;
        public bool useHangryMode { get; set; } = false;
        public int attackDebuff { get; set; } = -2;
        public int defenseDebuff { get; set; } = 0;
        public int speedDebuff { get; set; } = 0;
        public int buffDuration { get; set; } = 540000;
        public List<string> recipeBlacklist { get; set; } = new() { };
    }
}
