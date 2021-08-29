using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using TomatoLib.Common.Utilities.Extensions;

namespace BetterModList.Content.Patches
{
    public class UIModStateTextDrawEnabledText : PatchSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();

            Detour(Terraria.GetCachedType("Terraria.ModLoader.UI.UIModStateText").GetCachedMethod("DrawEnabledText"),
                GetType().GetCachedMethod(nameof(PatchDrawEnabledText)));
        }

        private static void PatchDrawEnabledText(UIElement self, SpriteBatch spriteBatch)
        {
            static Vector2 GetSize(string key) =>
                new(FontAssets.MouseText.Value.MeasureString(Language.GetTextValue(key)).X, 16f);

            Vector2 enabledSize = GetSize("GameUI.Enabled");
            Vector2 disabledSize = GetSize("GameUI.Disabled");
            string text = Terraria.GetCachedType("Terraria.ModLoader.UI.UIModStateText")
                .GetCachedProperty("DisplayText").GetValue<string>(self);
            Vector2 textDims = FontAssets.MouseText.Value.MeasureString(text);
            Vector2 drawPos = self.GetDimensions().Center() - textDims / 2f;

            if (!enabledSize.X.Equals(disabledSize.X))
            {
                float largeSize;
                float smallSize;
                bool enabledLarge = enabledSize.X > disabledSize.X;

                if (enabledLarge)
                {
                    largeSize = enabledSize.X;
                    smallSize = disabledSize.X;
                }
                else
                {
                    largeSize = disabledSize.X;
                    smallSize = enabledSize.X;
                }

                if (text == Language.GetTextValue("GameUI.Enabled") && !enabledLarge ||
                    text != Language.GetTextValue("GameUI.Enabled") && enabledLarge)
                    drawPos.X += (largeSize - smallSize) / 2f;
            }

            Color displayColor = Terraria.GetCachedType("Terraria.ModLoader.UI.UIModStateText")
                .GetCachedProperty("DisplayColor").GetValue<Color>(self);

            Utils.DrawBorderString(spriteBatch, text, drawPos, displayColor);
        }
    }
}