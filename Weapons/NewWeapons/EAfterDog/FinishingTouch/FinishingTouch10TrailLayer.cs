using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.ModLoader.PlayerDrawLayer;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod;


namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    internal class FinishingTouch10TrailLayer : PlayerDrawLayer
    {

        // ✅ 修复插入位置，使用推荐的 LastVanillaLayer
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);

        // 以下均可直接替换使用，用于不同渲染时机控制
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc); // 在背饰层后绘制，适合背部特效
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);    // 在头饰层后绘制，适合头部发光
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FrontAcc); // 在前饰层后绘制，适合胸前发光特效
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem); // 在持武器层后绘制，适合手持特效
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.SolarShield); // 在日耀护盾层后绘制，特殊情况使用

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.GetModPlayer<FinishingTouch10Player>().finishingTouchOrangeTrailActive;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<FinishingTouch10Player>();
            //Texture2D texture = Terraria.GameContent.TextureAssets.Player[player.body].Value;
            Texture2D texture = Terraria.GameContent.TextureAssets.ArmorBody[player.body].Value;
            // 如果要使用未穿衣服裸身材质
            // Texture2D texture = Terraria.GameContent.TextureAssets.Players[0, player.skinVariant].Value;



            for (int i = 0; i < modPlayer.oldPos.Length; i++)
            {
                Vector2 position = modPlayer.oldPos[i] + player.Size / 2f - Main.screenPosition;
                Color color = Color.Orange * (0.35f * (1f - i / (float)modPlayer.oldPos.Length));
                float rotation = modPlayer.oldRot[i];
                SpriteEffects effects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

                drawInfo.DrawDataCache.Add(new DrawData(
                    texture,
                    position,
                    null,
                    color,
                    rotation,
                    origin,
                    1f,
                    effects,
                    0
                ));
            }
        }
    }
}