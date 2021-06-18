using System;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;

namespace BetterModList.Common.Utilities.IDs
{
    /// <summary>
    ///     UI-state instances.
    /// </summary>
    public static class ModdedInterfaceInstances
    {
        private static readonly Assembly ModAssembly = typeof(ModLoader).Assembly;
        private static readonly Type InterfaceType = ModAssembly.GetCachedType("Terraria.ModLoader.UI.Interface");

        public static object ModsMenu => InterfaceType.GetField("modsMenu").GetValue(null);

        public static object LoadMods => InterfaceType.GetField("loadMods").GetValue(null);

        public static object ModSources => InterfaceType.GetField("modSources").GetValue(null);

        public static object BuildMod => InterfaceType.GetField("buildMod").GetValue(null);

        public static object ErrorMessage => InterfaceType.GetField("errorMessage").GetValue(null);

        public static object ModBrowser => InterfaceType.GetField("modBrowser").GetValue(null);

        public static object ModInfo => InterfaceType.GetField("modInfo").GetValue(null);

        public static object ManagePublished => InterfaceType.GetField("managePublished").GetValue(null);

        public static object UpdateMessage => InterfaceType.GetField("updateMessage").GetValue(null);

        public static object InfoMessage => InterfaceType.GetField("infoMessage").GetValue(null);

        public static object EnterPassPhraseMenu => InterfaceType.GetField("enterPassphraseMenu").GetValue(null);

        public static object ModPacksMenu => InterfaceType.GetField("modPacksMenu").GetValue(null);

        public static object EnterSteamIDMenu => InterfaceType.GetField("enterSteamIDMenu").GetValue(null);

        public static object ExtractMod => InterfaceType.GetField("extractMod").GetValue(null);

        public static object DeveloperModeHelp => InterfaceType.GetField("developerModeHelp").GetValue(null);

        public static object ModConfig => InterfaceType.GetField("modConfig").GetValue(null);

        public static object ModConfigList => InterfaceType.GetField("modConfigList").GetValue(null);

        public static object CreateMod => InterfaceType.GetField("createMod").GetValue(null);

        public static object Progress => InterfaceType.GetField("progress").GetValue(null);

        public static object DownloadProgress => InterfaceType.GetField("downloadProgress").GetValue(null);
    }
}