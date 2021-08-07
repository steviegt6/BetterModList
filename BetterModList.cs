using BetterModList.Common.Utilities.IDs;
using Terraria.Localization;
using Terraria.ModLoader;
using TomatoLib;
using TomatoLib.Core.Utilities.Extensions;

namespace BetterModList
{
    public class BetterModList : TomatoMod
    {
        public override void Load()
        {
            base.Load();

            MonoModHooks.RequestNativeAccess();

            // Repair menus not getting recreated during localization changes.
            LanguageManager.Instance.OnLanguageChanged += ReInitializeStaticModLoaderUserInterfaces;
        }

        public override void PostSetupContent()
        {
            base.PostSetupContent();

            // Create new instance in PostSetupContent to ensure patches are applied.
            ModdedInterfaceInstances.ModsMenu.ReplaceInfoInstance();
        }

        public override void Unload()
        {
            base.Unload();

            LanguageManager.Instance.OnLanguageChanged -= ReInitializeStaticModLoaderUserInterfaces;

            ModdedInterfaceInstances.ModsMenu.ReplaceInfoInstance();
        }

        private static void ReInitializeStaticModLoaderUserInterfaces(LanguageManager languageManager)
        {
            ModdedInterfaceInstances.ModsMenu.ReplaceInfoInstance();
            ModdedInterfaceInstances.LoadMods.ReplaceInfoInstance();
            ModdedInterfaceInstances.ModSources.ReplaceInfoInstance();
            ModdedInterfaceInstances.BuildMod.ReplaceInfoInstance();
            ModdedInterfaceInstances.ErrorMessage.ReplaceInfoInstance();
            ModdedInterfaceInstances.ModBrowser.ReplaceInfoInstance();
            ModdedInterfaceInstances.ModInfo.ReplaceInfoInstance();
            ModdedInterfaceInstances.UpdateMessage.ReplaceInfoInstance();
            ModdedInterfaceInstances.InfoMessage.ReplaceInfoInstance();
            ModdedInterfaceInstances.ModPacksMenu.ReplaceInfoInstance();
            ModdedInterfaceInstances.ExtractMod.ReplaceInfoInstance();
            ModdedInterfaceInstances.ModConfig.ReplaceInfoInstance();
            ModdedInterfaceInstances.ModConfigList.ReplaceInfoInstance();
            ModdedInterfaceInstances.CreateMod.ReplaceInfoInstance();
            ModdedInterfaceInstances.Progress.ReplaceInfoInstance();
            ModdedInterfaceInstances.DownloadProgress.ReplaceInfoInstance();
        }
        /*private static readonly Assembly TerrariaAssembly = typeof(Main).Assembly;

        public override void Load()
        {
            try
            {
                MonoModHooks.RequestNativeAccess();

                IntermediateLanguageHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("OnInitialize"),
                    nameof(TagRemovalModDrawApplicator));

                DetourHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("OnInitialize"),
                    GetType().GetCachedMethod(nameof(AppendHomepageLinkAndMessWithInitialization)));

                IntermediateLanguageHook(
                    TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem").GetCachedMethod("DrawSelf"),
                    nameof(ShoveDividerDown));

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

        private static void TagRemovalModDrawApplicator(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchLdfld("Terraria.ModLoader.UI.UIModItem", "_modName"));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<UIElement>>(modItem =>
            {
                Type modItemType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem");
                Type localModType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.LocalMod");
                Type buildPropertiesType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.BuildProperties");
                FieldInfo textPanel = modItemType.GetCachedField("_modName");
                float textHeight = Main.fontMouseText.MeasureString("A").Y;

                modItemType.GetCachedField("_modIconAdjust").SetValue(modItem, 85);

                StyleDimension iconAdjust =
                    new StyleDimension(modItemType.GetCachedField("_modIconAdjust").GetValue<int>(modItem), 0f);
                object modInstance = modItemType.GetCachedField("_mod").GetValue(modItem);
                object properties = localModType.GetCachedField("properties").GetValue(modInstance);

                UITextModName name = new UITextModName((
                    localModType.GetCachedProperty("DisplayName")
                        .GetValue<string>(modInstance),
                    modItemType.GetCachedField("DisplayNameClean").GetValue<string>(modItem)))
                {
                    Left = iconAdjust,
                    Top = {Pixels = 5f}
                };

                textPanel.SetValue(modItem, name);

                UIText authorText =
                    new UIText("by " + buildPropertiesType.GetCachedField("author")
                        .GetValue<string>(properties))
                    {
                        TextColor = Color.DarkGray,
                        Left = iconAdjust,
                        Top = {Pixels = 5f + textHeight}
                    };

                UIText versionText =
                    new UIText("v" + buildPropertiesType.GetCachedField("version")
                        .GetValue<Version>(properties))
                    {
                        TextColor = Color.Goldenrod,
                        Left = iconAdjust,
                        Top = {Pixels = 5f}
                    };

                Main.spriteBatch.Begin();
                versionText.Left.Pixels += 8f + ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch,
                    Main.fontMouseText, name.Text, new Vector2(0), Color.White, 0f, Vector2.Zero, Vector2.One).X;
                Main.spriteBatch.End();

                modItem.Append(authorText);
                modItem.Append(versionText);
            });

            c.Index = 0;
            c.GotoNext(x => x.MatchLdcI4(85));
            c.Index++;
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_0);

            // insert at beginning because LAZY lol
            c.Index = 0;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<UIElement>>(modItem =>
            {
                Type modItemType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem");
                Type localModType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.LocalMod");

                TmodFile file = localModType.GetCachedField("modFile")
                    .GetValue<TmodFile>(modItemType.GetCachedField("_mod").GetValue(modItem));

                if (file.HasFile("icon.png"))
                {
                    using (file.Open())
                    using (Stream stream = file.GetStream("icon.png"))
                    {
                        Texture2D temp = Texture2D.FromStream(Main.instance.GraphicsDevice, stream);

                        if (temp.Width == 80 && temp.Height == 80)
                            return;
                    }
                }

                UIImage icon = new UIImage(ModContent.GetTexture("BetterModList/Assets/UI/NoIcon"))
                {
                    Left = {Percent = 0f},
                    Top = {Percent = 0f}
                };

                modItem.Append(icon);
                modItemType.GetCachedField("_modIcon").SetValue(modItem, icon);
            });

            // remove browser key
            c.Index = 0;
            c.GotoNext(x => x.MatchStfld("Terraria.ModLoader.UI.UIModItem", "_keyImage"));
            c.Index++;
            c.RemoveRange(4);

            // remove improper unloaded icon
            c.GotoNext(x => x.MatchStfld("Terraria.ModLoader.UI.UIModItem", "_keyImage"));
            c.Index++;
            c.RemoveRange(18);
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
            float extended = Main.fontMouseText.MeasureString("A").Y;

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

            Type modItemType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.UI.UIModItem");
            Type localModType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.LocalMod");
            Type buildPropertiesType = TerrariaAssembly.GetCachedType("Terraria.ModLoader.Core.BuildProperties");
            object modInstance = modItemType.GetCachedField("_mod").GetValue(self);
            object properties = localModType.GetCachedField("properties").GetValue(modInstance);
            TmodFile file = localModType.GetCachedField("modFile").GetValue<TmodFile>(modInstance);
            UIModKeyImage keyImage;

            if (file.GetPropertyValue<TmodFile, bool>("ValidModBrowserSignature"))
                keyImage = new UIModKeyImage(Language.GetTextValue("tModLoader.ModsOriginatedFromModBrowser"), UIModKeyImage.KeyType.Workshop);
            else if (buildPropertiesType.GetCachedField("beta").GetValue<bool>(properties))
                keyImage = new UIModKeyImage("Built on a beta version of tModLoader", UIModKeyImage.KeyType.Beta);
            else
                keyImage = new UIModKeyImage("Unknown place of origin");

            keyImage.Left.Pixels = 85f / 2f - keyImage.KeyTexture.Width / 2f;
            keyImage.Top.Pixels = 74f + keyImage.KeyTexture.Height / 2f;
            self.Append(keyImage);

            string[] fields =
            {
                "_moreInfoButton",
                "_keyImage",
                "_configButton",
                "_uiModStateText",
                "_modReferenceIcon"
            };

            foreach (string field in fields.Where(x => uiModItem.GetCachedField(x).GetValue(self) != null))
                uiModItem.GetCachedField(field).GetValue<UIElement>(self).Top.Pixels += extended;
        }

        private static void ShoveDividerDown(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            float extended = Main.fontMouseText.MeasureString("A").Y;

            c.GotoNext(x => x.MatchLdcR4(30f));
            c.Index++;
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_R4, 30f + extended);

            c.GotoNext(x => x.MatchLdcR4(45f));
            c.Index++;
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_R4, 45f + extended);
        }*/
    }
}