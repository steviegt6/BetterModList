using System;
using System.Collections.Generic;
using BetterModList.Content.UI.Container;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using TomatoLib;
using TomatoLib.Common.Utilities.Extensions;

namespace BetterModList.Content.Patches
{
    public class UIModsOnInitializePatch : PatchSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();

            Edit(Terraria.GetCachedType("Terraria.ModLoader.UI.UIMods").GetCachedMethod("OnInitialize"),
                nameof(OnInitializePatch));
        }

        private static void OnInitializePatch(ILContext il)
        {
            ILCursor c = new(il);
            TomatoMod mod = ModContent.GetInstance<BetterModList>();

            AddTagRemovalButton(c, mod);
        }

        private static void AddTagRemovalButton(ILCursor c, TomatoMod mod)
        {
            /* GOAL: Add our Tag Removal button to the menu.
             * STEPS:
             *  1. Match: Terraria.ModLoader.UI.UIMods::_categoryButtons
             *  2. Match: ldc.i4.3 (loop variable)
             *  3. Forward the index (3) to jump out of the loop
             *  4. Push our code (explained below).
             */

            if (!c.TryGotoNext(x => x.MatchLdfld("Terraria.ModLoader.UI.UIMods", "_categoryButtons")))
            {
                mod.ModLogger.PatchFailure("Terraria.ModLoader.UI.UIMods", "OnInitialize", "ldfld", "Terraria.ModLoader.UI.UIMods::_categoryButtons");
                return;
            }

            if (!c.TryGotoNext(x => x.MatchLdcI4(3)))
            {
                mod.ModLogger.PatchFailure("Terraria.ModLoader.UI.UIMods", "OnInitialize", "ldc.i4.3");
                return;
            }

            c.Index += 3;

            c.Emit(OpCodes.Ldarg_0); // push "this" to the stack, allows us to access an instanced field
            c.Emit(OpCodes.Ldfld, Terraria.GetCachedType("Terraria.ModLoader.UI.UIMods").GetCachedField("_categoryButtons")); // push "_categoryButtons" to the stack
            c.Emit(OpCodes.Ldloc_3); // push the main UI element to the stack

            // actually insert our code + the variables we pushed to the stack
            c.EmitDelegate<Action<List<UICycleImage>, UIElement>>((categoryButtons, element) =>
            {
                Asset<Texture2D> tagTexture = ModContent.Request<Texture2D>("BetterModList/Assets/UI/ChatTagIndicator");
                UICycleImage tagButton = new(tagTexture, 2, 32, 32, 0, 0);

                tagButton.OnClick += (_, _) =>
                    UIModsFieldContainer.DisableChatTags = !UIModsFieldContainer.DisableChatTags;

                tagButton.OnRightClick += (_, _) =>
                    UIModsFieldContainer.DisableChatTags = !UIModsFieldContainer.DisableChatTags;

                // 36f -> width of each button
                // 3f -> amount of other buttons
                // 8f -> extra padding
                tagButton.Left.Pixels = 36f * 3f + 8f;

                categoryButtons.Add(tagButton);
                element.Append(tagButton);
            });
        }
    }
}