using UnityEngine;

namespace SurakshaAR.Application
{
    /// <summary>Captain-approved mobile palette shared by launcher, HUD, feedback, and AR targets.</summary>
    public static class MobilePalette
    {
        public static readonly Color SageGreen = Hex(0x5F725D);
        public static readonly Color ForestGreen = Hex(0x2E4B36);
        public static readonly Color DeepMossCharcoal = Hex(0x353A31);
        public static readonly Color EarthyTaupeBrown = Hex(0x4F473B);
        public static readonly Color DarkEspresso = Hex(0x312E28);

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color Hex(uint value)
        {
            return new Color(
                ((value >> 16) & 0xff) / 255f,
                ((value >> 8) & 0xff) / 255f,
                (value & 0xff) / 255f,
                1f);
        }
    }
}
