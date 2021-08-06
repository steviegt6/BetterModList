using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
            Workshop,
            Unknown
        }

        public KeyType ImageKeyType { get; protected set; }

        public Asset<Texture2D> KeyTexture { get; protected set; }

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
            Width.Set(KeyTexture.Width(), 0f);
            Height.Set(KeyTexture.Height(), 0f);
        }

        public virtual void SetKeyType(KeyType imageKeyType) => InternalSetKeyType(imageKeyType);

        protected void InternalSetText(string hoverText) => HoverText = hoverText;

        public virtual void SetText(string hoverText) => InternalSetText(hoverText);

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            spriteBatch.Draw(position: GetDimensions().Position() + KeyTexture.Size() * (1f - 1f) / 2f,
                texture: KeyTexture.Value,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 1f,
                effects: SpriteEffects.None,
                layerDepth: 0f);

            if (!IsMouseHovering)
                return;

            DelayedDrawStorage?.Invoke(HoverText);
            UICommon.DrawHoverStringInBounds(spriteBatch, HoverText, Parent.GetDimensions().ToRectangle());
        }

        public static Asset<Texture2D> GetImageFromKeyType(KeyType keyType)
        {
            return keyType switch
            {
                KeyType.Workshop => ModContent.Request<Texture2D>("BetterModList/Assets/UI/Key_ModBrowser",
                    AssetRequestMode.ImmediateLoad),
                KeyType.Unknown => ModContent.Request<Texture2D>("BetterModList/Assets/UI/Key_Unknown",
                    AssetRequestMode.ImmediateLoad),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}