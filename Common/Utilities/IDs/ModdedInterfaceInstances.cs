using System;
using System.Reflection;
using Terraria.ModLoader;

namespace BetterModList.Common.Utilities.IDs
{
    /// <summary>
    ///     UI-state instances.
    /// </summary>
    public static class ModdedInterfaceInstances
    {
        private static readonly Assembly ModAssembly = typeof(ModLoader).Assembly;
        private static readonly Type InterfaceType = ModAssembly.GetCachedType("Terraria.ModLoader.UI.Interface");

        public static FieldInfo ModsMenu => InterfaceType.GetCachedField("modsMenu");

        public static FieldInfo LoadMods => InterfaceType.GetCachedField("loadMods");

        public static FieldInfo ModSources => InterfaceType.GetCachedField("modSources");

        public static FieldInfo BuildMod => InterfaceType.GetCachedField("buildMod");

        public static FieldInfo ErrorMessage => InterfaceType.GetCachedField("errorMessage");

        public static FieldInfo ModBrowser => InterfaceType.GetCachedField("modBrowser");

        public static FieldInfo ModInfo => InterfaceType.GetCachedField("modInfo");

        public static FieldInfo ManagePublished => InterfaceType.GetCachedField("managePublished");

        public static FieldInfo UpdateMessage => InterfaceType.GetCachedField("updateMessage");

        public static FieldInfo InfoMessage => InterfaceType.GetCachedField("infoMessage");

        public static FieldInfo EnterPassPhraseMenu => InterfaceType.GetCachedField("enterPassphraseMenu");

        public static FieldInfo ModPacksMenu => InterfaceType.GetCachedField("modPacksMenu");

        public static FieldInfo EnterSteamIDMenu => InterfaceType.GetCachedField("enterSteamIDMenu");

        public static FieldInfo ExtractMod => InterfaceType.GetCachedField("extractMod");

        public static FieldInfo DeveloperModeHelp => InterfaceType.GetCachedField("developerModeHelp");

        public static FieldInfo ModConfig => InterfaceType.GetCachedField("modConfig");

        public static FieldInfo ModConfigList => InterfaceType.GetCachedField("modConfigList");

        public static FieldInfo CreateMod => InterfaceType.GetCachedField("createMod");

        public static FieldInfo Progress => InterfaceType.GetCachedField("progress");

        public static FieldInfo DownloadProgress => InterfaceType.GetCachedField("downloadProgress");
    }
}