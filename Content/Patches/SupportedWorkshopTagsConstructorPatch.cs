using Terraria.ModLoader;
using Terraria.Social;
using Terraria.Social.Base;
using Terraria.Social.Steam;
using TomatoLib.Common.Utilities.Extensions;
// ReSharper disable StringLiteralTypo

namespace BetterModList.Content.Patches
{
    public class SupportedWorkshopTagsConstructorPatch : ModSystem
    {
        public override void OnModLoad()
        {
            base.OnModLoad();

            On.Terraria.Social.Steam.SupportedWorkshopTags.ctor += InitializeModdedTags;

            InitializeModdedTags(SocialAPI.Workshop.SupportedTags);
        }

        private static void InitializeModdedTags(On.Terraria.Social.Steam.SupportedWorkshopTags.orig_ctor orig,
            SupportedWorkshopTags self) => InitializeModdedTags(self);

        private static void InitializeModdedTags(AWorkshopTagsCollection tags)
        {
            void Add(string tag, string name) => typeof(AWorkshopTagsCollection).GetCachedMethod("AddModTag")
                .Invoke(tags, new object[] { "Mods.BetterModList.WorkshopTags." + tag, name });

            Add("QualityOfLife", "quality of life");
            Add("Tweaks", "tweaks");
            Add("Utility", "utility");
            Add("NewContent", "new content");
            Add("TotalConversion", "total conversion");
            Add("WorldGen", "worldgen");
            Add("Interface", "interface");
            Add("Localization", "localization");
            Add("ClientSideOnly", "clientside-only");
            Add("ServerSideOnly", "serverside-only");
            Add("Library", "library");
        }
    }
}