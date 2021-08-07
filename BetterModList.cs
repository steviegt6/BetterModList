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