using System;
using System.Collections.Generic;
using System.Reflection;
using BetterModList.Common.Utilities;
using BetterModList.Common.Utilities.IDs;
using BetterModList.Content.UI.Container;
using BetterModList.Content.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
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
                LanguageManager.Instance.OnLanguageChanged += ReInitializeStaticModLoaderUserInterfaces;

                MonoModHooks.RequestNativeAccess();

                IntermediateLanguageHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIMods").GetCachedMethod("OnInitialize"),
                    nameof(TagRemovalInitializationApplicator));

                IntermediateLanguageHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIMods").GetCachedMethod("Draw"),
                    nameof(TagRemovalDrawApplicator));

                IntermediateLanguageHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("OnInitialize"),
                    nameof(TagRemovalModDrawApplicator));

                new Hook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModStateText")
                        .GetCachedMethod("Recalculate"),
                    GetType().GetCachedMethod(nameof(ReplaceRecalculationSizingOfEnabledText))).Apply();

                new Hook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModStateText")
                        .GetCachedMethod("DrawEnabledText"),
                    GetType().GetCachedMethod(nameof(ReplaceEnabledTextDrawing))).Apply();

                new Hook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("OnInitialize"),
                    GetType().GetCachedMethod(nameof(AppendHomepageLinkAndMessWithInitialization))).Apply();

                ModdedInterfaceInstances.ModsMenu.SetToNewInstance();
            }
            catch (Exception e)
            {
                throw new Exception(
                    "Unfortunately, an exception was thrown while attempting to apply patches to rework some aspects of tModLoader's UI." +
                    "\nThis is caused by the Better Mods List mod." +
                    "\nPlease report this issue to the developer and disable the mod for the time being." +
                    $"\n\n\nOriginal stack-trace: {e}");
            }
        }

        private static void ReInitializeStaticModLoaderUserInterfaces(LanguageManager languageManager)
        {
            ModdedInterfaceInstances.ModsMenu.SetToNewInstance();
            ModdedInterfaceInstances.LoadMods.SetToNewInstance();
            ModdedInterfaceInstances.ModSources.SetToNewInstance();
            ModdedInterfaceInstances.BuildMod.SetToNewInstance();
            ModdedInterfaceInstances.ErrorMessage.SetToNewInstance();
            ModdedInterfaceInstances.ModBrowser.SetToNewInstance();
            ModdedInterfaceInstances.ModInfo.SetToNewInstance();
            ModdedInterfaceInstances.ManagePublished.SetToNewInstance();
            ModdedInterfaceInstances.UpdateMessage.SetToNewInstance();
            ModdedInterfaceInstances.InfoMessage.SetToNewInstance();
            ModdedInterfaceInstances.EnterPassPhraseMenu.SetToNewInstance();
            ModdedInterfaceInstances.ModPacksMenu.SetToNewInstance();
            ModdedInterfaceInstances.EnterSteamIDMenu.SetToNewInstance();
            ModdedInterfaceInstances.ExtractMod.SetToNewInstance();
            ModdedInterfaceInstances.DeveloperModeHelp.SetToNewInstance();
            ModdedInterfaceInstances.ModConfig.SetToNewInstance();
            ModdedInterfaceInstances.ModConfigList.SetToNewInstance();
            ModdedInterfaceInstances.CreateMod.SetToNewInstance();
            ModdedInterfaceInstances.Progress.SetToNewInstance();
            ModdedInterfaceInstances.DownloadProgress.SetToNewInstance();
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

        private static void ReplaceRecalculationSizingOfEnabledText(Action<UIElement> orig, UIElement self)
        {
            orig(self);

            Vector2 enabledSize =
                new Vector2(Main.fontMouseText.MeasureString(Language.GetTextValue("GameUI.Enabled")).X, 16f);
            Vector2 disabledSize =
                new Vector2(Main.fontMouseText.MeasureString(Language.GetTextValue("GameUI.Disabled")).X, 16f);
            Vector2 balancedSize = new Vector2(Math.Max(enabledSize.X, disabledSize.X), 16f);

            self.Width.Set(balancedSize.X + self.PaddingLeft + self.PaddingRight, 0f);
            self.Height.Set(balancedSize.Y + self.PaddingTop + self.PaddingBottom, 0f);
        }

        private static void ReplaceEnabledTextDrawing(UIElement self, SpriteBatch spriteBatch)
        {
            Vector2 enabledSize =
                new Vector2(Main.fontMouseText.MeasureString(Language.GetTextValue("GameUI.Enabled")).X, 16f);
            Vector2 disabledSize =
                new Vector2(Main.fontMouseText.MeasureString(Language.GetTextValue("GameUI.Disabled")).X, 16f);
            string text = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModStateText")
                .GetCachedProperty("DisplayText")
                .GetValue(self) as string;
            Vector2 textDims = Main.fontMouseText.MeasureString(text);
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

            Utils.DrawBorderString(spriteBatch, text, drawPos,
                (Color) TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModStateText")
                    .GetCachedProperty("DisplayColor").GetValue(self));
        }

        private static void AppendHomepageLinkAndMessWithInitialization(Action<UIElement> orig, UIElement self)
        {
            orig(self);
            Type localMod = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.LocalMod");
            Type buildProperties = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.BuildProperties");
            Type uiModItem = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem");

            uiModItem.GetCachedField("_uiModStateText").GetValue<UIElement>(self).Top.Pixels += 5f;

            if (uiModItem.GetCachedField("_modReferenceIcon").GetValue(self) != null)
                uiModItem.GetCachedField("_modReferenceIcon").GetValue<UIImage>(self).Top.Pixels += 5f;

            string homepage = buildProperties.GetCachedField("homepage").GetValue<string>(localMod
                .GetCachedField("properties")
                .GetValue(uiModItem.GetCachedField("_mod").GetValue(self)));

            if (string.IsNullOrEmpty(homepage))
                return;

            self.Height.Pixels += 45f;

            self.Append(new UIModLinkText(homepage)
            {
                Top =
                {
                    Pixels = 95f
                },
                Width =
                {
                    Percent = 1f
                }
            });
        }

        public void IntermediateLanguageHook(MethodInfo method, string modifyingName) =>
            HookEndpointManager.Modify(method,
                Delegate.CreateDelegate(typeof(ILContext.Manipulator), GetType(), modifyingName));
    }
}