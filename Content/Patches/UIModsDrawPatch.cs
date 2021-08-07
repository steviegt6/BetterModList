using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using TomatoLib;
using TomatoLib.Core.Utilities.Extensions;

namespace BetterModList.Content.Patches
{
    public class UIModsDrawPatch : PatchSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();

            Edit(Terraria.GetCachedType("Terraria.ModLoader.UI.UIMods").GetCachedMethod("Draw"),
                nameof(DrawPatch));
        }

        private static void DrawPatch(ILContext il)
        {
            ILCursor c = new(il);
            TomatoMod mod = ModContent.GetInstance<BetterModList>();

            ReplaceNormalButtonText(c, mod);
        }

        private static void ReplaceNormalButtonText(ILCursor c, TomatoMod mod)
        {
            /* GOAL: Replace the category sorting text with our own.
             * STEPS:
             *  1. Match: The first switch case.
             *  2. Match and move after: ldarg.1.
             *  3. Push our code (explained below).
             */

            if (!c.TryGotoNext(x => x.MatchSwitch(out _)))
            {
                mod.ModLogger.PatchFailure("Terraria.ModLoader.UI.UIMods", "Draw", "switch");
                return;
            }

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdarg(1)))
            {
                mod.ModLogger.PatchFailure("Terraria.ModLoader.UI.UIMods", "Draw", "ldarg", "1");
                return;
            }

            c.Emit(OpCodes.Ldarg_0); // this
            c.Emit(OpCodes.Ldloc_0); // switch case index

            // returns our replacement string
            c.EmitDelegate<Func<UIElement, int, string>>((instance, index) =>
            {
                Type menuType = Terraria.GetCachedType("Terraria.ModLoader.UI.UIMods");
                Type sortExtensions = Terraria.GetCachedType("Terraria.ModLoader.UI.ModsMenuSortModesExtensions");
                Type enabledExtensions = Terraria.GetCachedType("Terraria.ModLoader.UI.EnabledFilterModesExtensions");
                Type sideExtensions = Terraria.GetCachedType("Terraria.ModLoader.UI.ModBrowser.ModSideFilterModesExtensions");
                Type searchExtensions = Terraria.GetCachedType("Terraria.ModLoader.UI.ModBrowser.SearchFilterModesExtensions");
                MethodInfo sortString = sortExtensions.GetCachedMethod("ToFriendlyString");
                MethodInfo enabledString = enabledExtensions.GetCachedMethod("ToFriendlyString");
                MethodInfo sideString = sideExtensions.GetCachedMethod("ToFriendlyString");
                MethodInfo searchString = searchExtensions.GetCachedMethod("ToFriendlyString");

                switch (index)
                {
                    case 1:
                        return sortString.Invoke(null,
                            new[] {menuType.GetCachedField("sortMode").GetValue(instance)}) as string;

                    case 3:
                        return enabledString.Invoke(null,
                            new[] { menuType.GetCachedField("enabledFilterMode").GetValue(instance) }) as string;

                    case 5:
                        return sideString.Invoke(null,
                            new[] { menuType.GetCachedField("modSideFilterMode").GetValue(instance) }) as string;

                    case 6:
                        return Language.GetTextValue("Mods.BetterModList.UI.ToggleChatTags");

                    case 7:
                        return searchString.Invoke(null,
                            new[] { menuType.GetCachedField("searchFilterMode").GetValue(instance) }) as string;

                    default:
                        return Language.GetTextValue("Mods.BetterModList.UI.MissingDescription");
                }
            });

            // set the mouse text field to our returned field
            c.Emit(OpCodes.Stloc_2);
        }
    }
}