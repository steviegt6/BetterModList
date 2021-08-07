using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.GameContent;
using Terraria.ModLoader;
using TomatoLib;
using TomatoLib.Core.Utilities.Extensions;

namespace BetterModList.Content.Patches
{
    public class UIModItemDrawSelfPatch : PatchSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();

            Edit(Terraria.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("DrawSelf"),
                nameof(DrawSelfPatch));
        }

        private static void DrawSelfPatch(ILContext il)
        {
            ILCursor c = new(il);
            TomatoMod mod = ModContent.GetInstance<BetterModList>();

            MoveDividerDown(c, mod);
        }

        private static void MoveDividerDown(ILCursor c, TomatoMod mod)
        {
            float extended = FontAssets.MouseText.Value.MeasureString("A").Y;

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(30f)))
            {
                mod.ModLogger.PatchFailure("Terraria.ModLoader.UI.UIModItem", "DrawSelf", "ldc.r4", "30f");
                return;
            }

            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_R4, 30f + extended);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(45f)))
            {
                mod.ModLogger.PatchFailure("Terraria.ModLoader.UI.UIModItem", "DrawSelf", "ldc.r4", "45f");
                return;
            }

            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_R4, 45f + extended);
        }
    }
}