using System;

namespace FoodCravings
{
    public sealed class ModConfig
    {
        public bool useHangryMode { get; set; } = false;
        public int attackBuff { get; set; } = 2;
        public int defenseBuff { get; set; } = 2;
        public int speedBuff { get; set; } = 1;
        public int attackDebuff { get; set; } = -2;
        public int defenseDebuff { get; set; } = 0;
        public int speedDebuff { get; set; } = 0;
    }
}
