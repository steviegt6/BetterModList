using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace BetterModList.Content.UI.Elements
{
    public class UIModKeyImage : UIElement
    {
        public enum KeyType
        {
            ModBrowser,
            Beta,
            Unknown
        }

        public KeyType ImageKeyType { get; protected set; }

        public Texture2D KeyTexture { get; protected set; }

        public string HoverText { get; protected set; }

        public Action<string> DelayedDrawStorage { get; protected set; }

        public UIModKeyImage(string hoverText, KeyType imageKeyType = KeyType.Unknown)
        {
            InternalSetText(hoverText);
            InternalSetKeyType(imageKeyType);
        }

        protected void InternalSetKeyType(KeyType imageKeyType)
        {
            ImageKeyType = imageKeyType;
            KeyTexture = GetImageFromKeyType(ImageKeyType);
            Width.Set(KeyTexture.Width, 0f);
            Height.Set(KeyTexture.Height, 0f);
        }

        public virtual void SetKeyType(KeyType imageKeyType) => InternalSetKeyType(imageKeyType);

        protected void InternalSetText(string hoverText) => HoverText = hoverText;

        public virtual void SetText(string hoverText) => HoverText = hoverText;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            spriteBatch.Draw(position: GetDimensions().Position() + KeyTexture.Size() * (1f - 1f) / 2f, texture: KeyTexture,
                sourceRectangle: null, color: Color.White, rotation: 0f, origin: Vector2.Zero, scale: 1f,
                effects: SpriteEffects.None, layerDepth: 0f);

            if (!IsMouseHovering)
                return;

            DelayedDrawStorage?.Invoke(HoverText);
            UICommon.DrawHoverStringInBounds(spriteBatch, HoverText, Parent.GetDimensions().ToRectangle());
        }

        public static Texture2D GetImageFromKeyType(KeyType keyType)
        {
            switch (keyType)
            {
                case KeyType.ModBrowser:
                    return ModContent.GetTexture("BetterModList/Assets/UI/Key_ModBrowser");

                case KeyType.Beta:
                    return ModContent.GetTexture("BetterModList/Assets/UI/Key_Beta");

                case KeyType.Unknown:
                    return ModContent.GetTexture("BetterModList/Assets/UI/Key_Unknown");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}