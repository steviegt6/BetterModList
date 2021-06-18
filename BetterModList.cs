using System;
using System.Collections.Generic;
using System.Reflection;
using BetterModList.Common.Utilities;
using BetterModList.Common.Utilities.IDs;
using BetterModList.Content.UI.Container;
using BetterModList.Content.UI.Elements;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace BetterModList
{
    public class BetterModList : Mod
    {
        private static readonly Assembly TerrariaAssembly = typeof(Main).Assembly;

        public override void Load()
        {
            try
            {
                IntermediateLanguageHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIMods").GetCachedMethod("OnInitialize"),
                    nameof(TagRemovalInitializationApplicator));

                IntermediateLanguageHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIMods").GetCachedMethod("Draw"),
                    nameof(TagRemovalDrawApplicator));

                IntermediateLanguageHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("OnInitialize"),
                    nameof(TagRemovalModDrawApplicator));

                ModdedInterfaceInstances.ModsMenu.SetValue(null,
                    Activator.CreateInstance(TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIMods")));
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Unfortunately, an exception was thrown while attempting to apply patches to rework some aspects of tModLoader's UI." +
                    "\nThis is caused by the Better Mods List mod." +
                    "\nPlease report this issue to the developer and disable the mod for the time being." +
                    $"\n\n\n\n\n\nOriginal stack-trace: {e}");
            }
        }

        private static void TagRemovalInitializationApplicator(ILContext il)
        {
            // match field Terraria.ModLoader.UI.UIMods::_categoryButtons
            // ldc.i4.3
            // index->
            // into blt IL_jump
            // end loop
            // index->
            // exit loop
            // apply extra code

            ILCursor c = new ILCursor(il);
            c.GotoNext(x => x.MatchLdfld("Terraria.ModLoader.UI.UIMods", "_categoryButtons"));
            c.GotoNext(x => x.MatchLdcI4(3));
            c.Index += 3;

            c.Emit(OpCodes.Ldarg_0); // this

            c.Emit(OpCodes.Ldfld,
                TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIMods")
                    .GetCachedField("_categoryButtons")); // list
            c.Emit(OpCodes.Ldloc_3); // element
            c.EmitDelegate<Action<List<UICycleImage>, UIElement>>((categoryButtons, element) =>
            {
                // Type modsUI = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIMods");
                // FieldInfo updateNeeded = modsUI.GetCachedField("updateNeeded");
                UICycleImage image =
                    new UICycleImage(ModContent.GetTexture("BetterModList/Assets/UI/ChatTagIndicator"), 2, 32, 32, 0, 0)
                        {CurrentState = 0};
                image.OnClick += (_, __) =>
                {
                    UIModsFieldContainer.DisableChatTags = !UIModsFieldContainer.DisableChatTags;
                    // updateNeeded.SetValue(ModdedInterfaceInstances.ModsMenu.GetValue(null), true);
                };
                image.OnRightClick += (_, __) =>
                {
                    UIModsFieldContainer.DisableChatTags = !UIModsFieldContainer.DisableChatTags;
                    // updateNeeded.SetValue(ModdedInterfaceInstances.ModsMenu, true);
                };
                image.Left.Pixels = 36 * 3 + 8;
                categoryButtons.Add(image);
                element.Append(image);
            });
        }

        private static void TagRemovalDrawApplicator(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(x => x.MatchSwitch(out _));
            c.GotoNext(x => x.MatchLdarg(1));
            c.Index++;

            c.Emit(OpCodes.Ldarg_0); // this
            c.Emit(OpCodes.Ldloc_0); // switch case index
            c.EmitDelegate<Func<object, int, string>>((instance, index) =>
            {
                object menuInstance = ModdedInterfaceInstances.ModsMenu.GetValue(null);
                Type menuType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIMods");
                Type sortExtensions =
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.ModsMenuSortModesExtensions");
                Type enabledFilterExtensions =
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.EnabledFilterModesExtensions");
                Type sideFilterExtensions =
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.ModBrowser.ModSideFilterModesExtensions");
                Type searchFilterExtensions =
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.ModBrowser.SearchFilterModesExtensions");

                switch (index)
                {
                    case 0:
                        return sortExtensions.GetCachedMethod("ToFriendlyString").Invoke(null,
                            new[] {menuType.GetField("sortMode").GetValue(instance)}) as string;

                    case 1:
                        return enabledFilterExtensions.GetCachedMethod("ToFriendlyString").Invoke(null,
                            new[] {menuType.GetField("enabledFilterMode").GetValue(instance)}) as string;

                    case 2:
                        return sideFilterExtensions.GetCachedMethod("ToFriendlyString").Invoke(null,
                            new[] {menuType.GetField("modSideFilterMode").GetValue(instance)}) as string;

                    case 3:
                        return "Toggle chat tags in mod names";

                    case 4:
                        return searchFilterExtensions.GetCachedMethod("ToFriendlyString").Invoke(null,
                            new[] {menuType.GetField("searchFilterMode").GetValue(instance)}) as string;

                    default:
                        return "None";
                }
            });
            c.Emit(OpCodes.Stloc_1);
        }

        private static void TagRemovalModDrawApplicator(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            /*c.GotoNext(x => x.MatchCallvirt("Terraria.ModLoader.Core.LocalMod", "get_DisplayName"));
            c.Remove();

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<object, object, string>>((localMod, instance) =>
            {
                Type modItemType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem");
                Type localModType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.LocalMod");

                return UIModsFieldContainer.DisableChatTags
                    ? modItemType.GetCachedField("DisplayNameClean").GetValue(instance) as string
                    : localModType.GetCachedProperty("DisplayName").GetValue(localMod) as string;
            });*/

            c.GotoNext(x => x.MatchLdfld("Terraria.ModLoader.UI.UIModItem", "_modName"));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<object>>(modItem =>
            {
                Type modItemType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem");
                Type localModType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.LocalMod");
                FieldInfo textPanel = modItemType.GetCachedField("_modName");

                textPanel.SetValue(modItem, new UITextModName((
                    localModType.GetCachedProperty("DisplayName")
                        .GetValue(modItemType.GetCachedField("_mod").GetValue(modItem)) as string,
                    modItemType.GetCachedField("DisplayNameClean").GetValue(modItem) as string))
                {
                    Left = new StyleDimension((int) modItemType.GetCachedField("_modIconAdjust").GetValue(modItem), 0f),
                    Top = {Pixels = 5f}
                });
            });
        }

        public void IntermediateLanguageHook(MethodInfo method, string modifyingName) =>
            HookEndpointManager.Modify(method,
                Delegate.CreateDelegate(typeof(ILContext.Manipulator), GetType(), modifyingName));
    }
}