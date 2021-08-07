using System;
using System.Reflection;
using BetterModList.Content.UI.Elements;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ModLoader;
using Terraria.UI;
using TomatoLib;
using TomatoLib.Core.Utilities.Extensions;

namespace BetterModList.Content.Patches
{
    public class UIModItemOnInitializePatch : PatchSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();

            Edit(Terraria.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("OnInitialize"),
                nameof(OnInitializePatch));
        }

        private static void OnInitializePatch(ILContext il)
        {
            ILCursor c = new(il);
            TomatoMod mod = ModContent.GetInstance<BetterModList>();

            ReplaceModName(c, mod);
        }

        private static void ReplaceModName(ILCursor c, TomatoMod mod)
        {
            /* GOAL: Replace the category sorting text with our own.
             * STEPS:
             *  1. Match before: ldfld Terraria.ModLoader.UI.UIModItem::_modName
             *  2. Push our code (explained below).
             */

            if (!c.TryGotoNext(MoveType.Before, x => x.MatchLdfld("Terraria.ModLoader.UI.UIModItem", "_modName")))
            {
                mod.ModLogger.PatchFailure("Terraria.ModLoader.UI", "UIModItem", "ldfld", "_modName");
                return;
            }

            c.Emit(OpCodes.Ldarg_0); // push "this"

            // TODO: include other stuff from original patches
            c.EmitDelegate<Action<UIElement>>(modItem =>
            {
                Type modItemType = Terraria.GetCachedType("Terraria.ModLoader.UI.UIModItem");
                Type localModType = Terraria.GetCachedType("Terraria.ModLoader.Core.LocalMod");
                Type buildPropertiesType = Terraria.GetCachedType("Terraria.ModLoader.Core.BuildProperties");
                StyleDimension iconAdjust = new(modItemType.GetCachedField("_modIconAdjust").GetValue<int>(modItem), 0f);
                FieldInfo textPanel = modItemType.GetCachedField("_modName");
                object modInstance = modItemType.GetCachedField("_mod").GetValue(modItem);
                object properties = localModType.GetCachedField("properties").GetValue(modInstance);

                UITextModName newModName = new((localModType.GetCachedProperty("DisplayName").GetValue<string>(modInstance),
                    modItemType.GetCachedField("DisplayNameClean").GetValue<string>(modItem))) 
                {
                    Left = iconAdjust,
                    Top = { Pixels = 5f }
                };

                textPanel.SetValue(modItem, newModName);
            });
        }
    }
}