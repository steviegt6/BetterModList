using BetterModList.Content.UI.Container;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;

namespace BetterModList.Content.UI.Elements
{
    public class UITextModName : UIText
    {
        public (string, string) Names { get; }

        public UITextModName((string, string) names, float textScale = 1, bool large = false) : base(
            UIModsFieldContainer.DisableChatTags ? names.Item2 : names.Item1, textScale, large)
        {
            Names = names;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SetText(UIModsFieldContainer.DisableChatTags ? Names.Item2 : Names.Item1);
        }
    }
}