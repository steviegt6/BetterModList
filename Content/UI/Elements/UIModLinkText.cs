using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace BetterModList.Content.UI.Elements
{
    public class UIModLinkText : UIElement
    {
        public string Link { get; }

        public UIModLinkText(string link)
        {
            Link = link;
        }

        public override void OnInitialize()
        {
            OnClick += (_, _) => Process.Start(Link);
            PaddingLeft = PaddingRight = 5f;
            PaddingBottom = PaddingTop = 10f;
        }

        public override void Recalculate()
        {
            Vector2 vector = new(FontAssets.MouseText.Value.MeasureString(Link).X, 16f);
            Width.Set(GetInnerDimensions().Width, 1f);
            Height.Set(vector.Y + PaddingTop + PaddingBottom, 0f);
            base.Recalculate();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            float extended = FontAssets.MouseText.Value.MeasureString("A").Y;
            Vector2 position = new(Parent.GetInnerDimensions().X + 5f, Parent.GetInnerDimensions().Y + 85f + extended);

            spriteBatch.Draw(
                position: position,
                texture: UICommon.DividerTexture.Value,
                sourceRectangle: null, 
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero, 
                scale: new Vector2((Parent.GetInnerDimensions().Width - 10f) / 8f, 1f),
                effects: SpriteEffects.None, 
                layerDepth: 0f);

            base.DrawSelf(spriteBatch);
            DrawPanel(spriteBatch);
            DrawEnabledText(spriteBatch);
        }

        private void DrawPanel(SpriteBatch spriteBatch)
        {
            Vector2 position = GetDimensions().Position();
            float pixels = Width.Pixels;
            Texture2D texture = UICommon.InnerPanelTexture.Value;

            spriteBatch.Draw(texture, position, new Rectangle(0, 0, 8, UICommon.InnerPanelTexture.Height()),
                Color.White);

            spriteBatch.Draw(texture, new Vector2(position.X + 8f, position.Y),
                new Rectangle(8, 0, 8, UICommon.InnerPanelTexture.Height()), Color.White, 0f, Vector2.Zero,
                new Vector2((pixels - 16f) / 8f, 1f), SpriteEffects.None, 0f);

            spriteBatch.Draw(texture, new Vector2(position.X + pixels - 8f, position.Y),
                new Rectangle(16, 0, 8, UICommon.InnerPanelTexture.Height()), Color.White);
        }

        private void DrawEnabledText(SpriteBatch spriteBatch)
        {
            const float padding = 26f;
            Vector2 pos = GetDimensions().Position() + new Vector2(PaddingLeft, PaddingTop * 0.5f);
            float buffer = FontAssets.MouseText.Value.MeasureString("[c/ffffff:]").X;
            string linkDraw = $"[c/ffffff:{GetDomainText()}:] {Link}";
            int length = linkDraw.Length;

            while (FontAssets.MouseText.Value.MeasureString(linkDraw).X + padding > GetInnerDimensions().Width + buffer)
                linkDraw = linkDraw.Remove(linkDraw.Length - 1);

            if (linkDraw.Length < length)
                linkDraw += "...";

            Utils.DrawBorderString(spriteBatch, linkDraw, pos, Colors.RarityBlue);
        }

        private string GetDomainText()
        {
            static string GetPageName(string name) => Language.GetTextValue($"Mods.BetterModsList.UI.{name}");

            string lowercaseLink = Link.ToLower();

            if (lowercaseLink.Contains("discord.gg") || lowercaseLink.Contains("discordapp.com") || lowercaseLink.Contains("discord.com"))
                return GetPageName("Discord");

            if (lowercaseLink.Contains("forums.terraria.org"))
                return GetPageName("Forums");

            if (lowercaseLink.Contains("github.com"))
                return GetPageName("GitHub");

            if (lowercaseLink.Contains("youtube.com") || lowercaseLink.Contains("youtu.be"))
                return GetPageName("YouTube");

            if (lowercaseLink.Contains("twitter.com"))
                return GetPageName("Twitter");

            return GetPageName("Default");
        }
	}
}