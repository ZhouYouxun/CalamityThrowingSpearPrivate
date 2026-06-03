using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord
{
    internal class BossChallengeBarLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            BossChallengeBarPlayer challengePlayer = drawInfo.drawPlayer.GetModPlayer<BossChallengeBarPlayer>();
            return challengePlayer.CurrentBarOpacity > 0f;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            BossChallengeBarPlayer challengePlayer = player.GetModPlayer<BossChallengeBarPlayer>();
            float opacity = challengePlayer.CurrentBarOpacity;
            if (opacity <= 0f)
            {
                return;
            }

            Texture2D barBackground = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            Texture2D barForeground = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            const float drawScale = 1.3f;
            Vector2 drawPos = player.Bottom - Main.screenPosition + new Vector2(-barBackground.Width * drawScale * 0.5f, player.gfxOffY + 18f);
            Rectangle frameCrop = new Rectangle(0, 0, (int)(barForeground.Width * challengePlayer.CurrentBarProgress), barForeground.Height);
            Color barColor = challengePlayer.CurrentBarColor;

            DrawData backgroundDraw = new DrawData(
                barBackground,
                drawPos,
                null,
                barColor * (opacity * 0.65f),
                0f,
                Vector2.Zero,
                drawScale,
                SpriteEffects.None,
                0);
            drawInfo.DrawDataCache.Add(backgroundDraw);

            if (frameCrop.Width <= 0)
            {
                return;
            }

            DrawData foregroundDraw = new DrawData(
                barForeground,
                drawPos,
                frameCrop,
                barColor * opacity,
                0f,
                Vector2.Zero,
                drawScale,
                SpriteEffects.None,
                0);
            drawInfo.DrawDataCache.Add(foregroundDraw);
        }
    }
}
