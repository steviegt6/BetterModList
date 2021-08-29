using System;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using TomatoLib.Common.Utilities.Extensions;

namespace BetterModList.Content.Patches
{
    public class UIModStateTextRecalculatePatch : PatchSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();

            Detour(Terraria.GetCachedType("Terraria.ModLoader.UI.UIModStateText").GetCachedMethod("Recalculate"), 
                GetType().GetCachedMethod(nameof(PatchRecalculate)));
        }

        private static void PatchRecalculate(Action<UIElement> orig, UIElement self)
        {
            orig(self);

            static Vector2 GetSize(string key) =>
                new(FontAssets.MouseText.Value.MeasureString(Language.GetTextValue(key)).X, 16f);

            Vector2 enabledSize = GetSize("GameUI.Enabled");
            Vector2 disabledSize = GetSize("GameUI.Disabled");
            Vector2 balancedSize = new(Math.Max(enabledSize.X, disabledSize.X), 16f);

            self.Width.Set(balancedSize.X + self.PaddingLeft + self.PaddingRight, 0f);
            self.Height.Set(balancedSize.Y + self.PaddingTop + self.PaddingBottom, 0f);
        }
    }
}