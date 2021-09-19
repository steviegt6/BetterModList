using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BetterModList.Content.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.UI;
using TomatoLib;
using TomatoLib.Common.Utilities.Extensions;
using TomatoLib.Content.UI.Elements;

namespace BetterModList.Content.Patches
{
    public class UIModItemOnInitializePatch : PatchSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();


            Edit(Terraria.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("OnInitialize"),
                nameof(OnInitializePatch));

            Detour(Terraria.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("OnInitialize"),
                GetType().GetCachedMethod(nameof(OnInitializeDetour)));
        }

        private static void OnInitializePatch(ILContext il)
        {
            ILCursor c = new(il);
            TomatoMod mod = ModContent.GetInstance<BetterModList>();

            ForceModIcon(c);
            ReplaceModNameDisplay(c, mod);
            RemoveSourceKey(c, mod);
        }

        private static void ReplaceModNameDisplay(ILCursor c, TomatoMod mod)
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
                float textHeight = FontAssets.MouseText.Value.MeasureString("A").Y;

                // replace the normal mod name text field
                UITextModName newModName = new((localModType.GetCachedProperty("DisplayName").GetValue<string>(modInstance),
                    modItemType.GetCachedField("DisplayNameClean").GetValue<string>(modItem))) 
                {
                    Left = iconAdjust,
                    Top = { Pixels = 5f }
                };

                textPanel.SetValue(modItem, newModName);

                // Change the mod icon adjustment size to 85f
                // modItemType.GetCachedField("_modIconAdjust").SetValue(modItem, 85);

                string authorText = buildPropertiesType.GetCachedField("author").GetValue<string>(properties);
                string versionText = buildPropertiesType.GetCachedField("version").GetValue<Version>(properties).ToString();
                string authorStarter = Language.GetTextValue("Mods.BetterModList.UI.AuthorStarter");
                string authorAndVersionText = $"[c/{Color.DarkGray.Hex3()}:{authorStarter} {authorText}] " +
                                              $"[c/{Color.Goldenrod.Hex3()}:v{versionText}]";

                UIText authorAndVersion = new(authorAndVersionText)
                {
                    Left = iconAdjust,
                    Top = {Pixels = 5f + textHeight}
                };

                modItem.Append(authorAndVersion);

                // Insert improper loading error icon
                string modName = modItemType.GetCachedProperty("ModName").GetValue<string>(modItem);
                bool devMode = Terraria.GetCachedType("Terraria.ModLoader.Core.ModCompile").GetProperty("DeveloperMode").GetValue<bool>();
                bool improperlyUnloaded = (bool) typeof(ModLoader).GetCachedMethod("IsUnloadedModStillAlive").Invoke(null, new object[] { modName })!;

                if (!devMode || !improperlyUnloaded)
                    return;

                int modIconAdjust = modItemType.GetCachedField("_modIconAdjust").GetValue<int>(modItem);

                UIHoverImage unloadWarningImage = new(UICommon.ButtonErrorTexture,
                    Language.GetTextValue("tModLoader.ModDidNotFullyUnloadWarning"))
                {
                    Left = { Pixels = modIconAdjust + 5f },
                    Top = { Pixels = 3f }
                };

                modItem.Append(unloadWarningImage);
            });
        }

        private static void RemoveSourceKey(ILCursor c, TomatoMod mod)
        {
            /* GOAL: Remove the informative mod key source.
             * STEPS:
             *  1. Match after: stfld Terraria.ModLoader.UI.UIModItem::_keyImage
             *  2. Remove the code responsible for appending it.
             */

            for (int i = 0; i < 3; i++)
            {
                if (!c.TryGotoNext(MoveType.After, x => x.MatchStfld("Terraria.ModLoader.UI.UIModItem", "_keyImage")))
                {
                    mod.ModLogger.PatchFailure("Terraria.ModLoader.UI.UIModItem", "OnInitialize", "stfld", "_keyImage");
                    return;
                }

                // removes the four op-codes used to append our key.
                c.RemoveRange(4);
            }

            /*
             * Do something similar to this three times:
             * IL_04eb: stfld class Terraria.ModLoader.UI.UIHoverImage Terraria.ModLoader.UI.UIModItem::_keyImage
             * <--- Cursor here
             * 1. IL_04f0: ldarg.0
             * 2. IL_04f1: ldarg.0
             * 3. IL_04f2: ldfld class Terraria.ModLoader.UI.UIHoverImage Terraria.ModLoader.UI.UIModItem::_keyImage
             * 4. IL_04f7: call instance void Terraria.UI.UIElement::Append(class Terraria.UI.UIElement)
             * 1-4 ^^ all removed.
             */
        }

        private static void ForceModIcon(ILCursor c)
        {
            // TODO: Convert to detour?... idk
            /* GOAL: Force mods to use an icon.
             * Steps:
             *  1. Insert out code, no jumping needed.
             */

            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Action<UIElement>>(modItem =>
            {
                Type modItemType = Terraria.GetCachedType("Terraria.ModLoader.UI.UIModItem");
                Type localModType = Terraria.GetCachedType("Terraria.ModLoader.Core.LocalMod");

                TmodFile file = localModType.GetCachedField("modFile")
                    .GetValue<TmodFile>(modItemType.GetCachedField("_mod").GetValue(modItem));

                bool carryOutIcon = true;

                UIImage modIcon = new(ModContent.Request<Texture2D>("BetterModList/Assets/UI/NoIcon", AssetRequestMode.ImmediateLoad))
                {
                    Left = {Percent = 0f},
                    Top = {Percent = 0f},
                    ScaleToFit = true,
                    MaxWidth = {Pixels = 80f, Percent = 0f},
                    MaxHeight = {Pixels = 80f, Percent = 0f}
                };

                if (file.HasFile("icon.png"))
                {
                    using (file.Open())
                    using (Stream stream = file.GetStream("icon.png"))
                    {
                        Texture2D icon = Main.Assets.CreateUntracked<Texture2D>(stream, ".png").Value;

                        modIcon.SetImage(icon);

                        if (icon.Width == 80 && icon.Height == 80)
                            carryOutIcon = false;
                    }
                }


                if (!carryOutIcon) 
                    return;

                modItemType.GetCachedField("_modIconAdjust").SetValue(modItem,
                    modItemType.GetCachedField("_modIconAdjust").GetValue<int>(modItem) + 85);
                modItem.Append(modIcon);
            });
        }

        private static void OnInitializeDetour(Action<UIElement> orig, UIElement self)
        {
            orig(self);

            Type localMod = Terraria.GetCachedType("Terraria.ModLoader.Core.LocalMod");
            Type buildProperties = Terraria.GetCachedType("Terraria.ModLoader.Core.BuildProperties");
            Type uiModItem = Terraria.GetCachedType("Terraria.ModLoader.UI.UIModItem");

            // Push some elements down
            uiModItem.GetCachedField("_uiModStateText").GetValue<UIElement>(self).Top.Pixels += 5f;

            if (uiModItem.GetCachedField("_modReferenceIcon").GetValue(self) != null)
                uiModItem.GetCachedField("_modReferenceIcon").GetValue<UIImage>(self).Top.Pixels += 5f;

            // Get homepage and add homepage link if it exists
            string homepage = buildProperties.GetCachedField("homepage").GetValue<string>(localMod
                .GetCachedField("properties")
                .GetValue(uiModItem.GetCachedField("_mod").GetValue(self)));
            float extended = FontAssets.MouseText.Value.MeasureString("A").Y;

            self.Height.Pixels += extended;

            if (!string.IsNullOrEmpty(homepage))
            {
                self.Height.Pixels += 45f;

                self.Append(new UIModLinkText(homepage)
                {
                    Top =
                    {
                        Pixels = 95f + extended
                    },
                    Width =
                    {
                        Percent = 1f
                    }
                });
            }

            Type modItemType = Terraria.GetCachedType("Terraria.ModLoader.UI.UIModItem");
            Type localModType = Terraria.GetCachedType("Terraria.ModLoader.Core.LocalMod");
            object modInstance = modItemType.GetCachedField("_mod").GetValue(self);
            TmodFile file = localModType.GetCachedField("modFile").GetValue<TmodFile>(modInstance);

            UIModKeyImage keyImage = file.path.Contains("workshop")
                ? new UIModKeyImage(Language.GetTextValue("Mods.BetterModList.UI.OriginatedFromWorkshop"), UIModKeyImage.KeyType.Workshop)
                : new UIModKeyImage(Language.GetTextValue("Mods.BetterModList.UI.UnknownOrigin"));

            keyImage.Left.Pixels = 85f / 2f - keyImage.KeyTexture.Width() / 2f;
            keyImage.Top.Pixels = 74f + keyImage.KeyTexture.Height() / 2f;
            self.Append(keyImage);

            string[] fields =
            {
                "_moreInfoButton",
                "_keyImage",
                "_configButton",
                "_uiModStateText",
                "_modReferenceIcon",
                "_deleteModButton"
            };

            // Extend listed fields downward
            foreach (string field in fields.Where(x => uiModItem.GetCachedField(x).GetValue(self) != null))
                uiModItem.GetCachedField(field).GetValue<UIElement>(self).Top.Pixels += extended;
        }
    }
}