using System.Collections.Generic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    public class SunsetASunsetRightEXPMagic2 : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // 不绘制自身贴图

        public static Asset<Texture2D> screamTex;
        private int lifeTimer;

        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                screamTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/ScreamyFace", AssetRequestMode.AsyncLoad);
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 200;   // 原320 → 减少约40%
            Projectile.height = 200;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600; // 给个短寿命，方便测试淡出
            Projectile.Opacity = 0f;   // 从透明开始淡入
        }

        public override void AI()
        {
            lifeTimer++;

            // === 淡入淡出 ===
            int fadeInTime = 30;
            int fadeOutTime = 30;
            if (lifeTimer <= fadeInTime)
                Projectile.Opacity = lifeTimer / (float)fadeInTime;
            else if (Projectile.timeLeft < fadeOutTime)
                Projectile.Opacity = Projectile.timeLeft / (float)fadeOutTime;
            else
                Projectile.Opacity = 1f;

            // 基础缩放 0.6f（整体削减40%），在 ±10% 范围内浮动
            float pulsate = 1f + 0.1f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Projectile.scale = 0.6f * pulsate;

            // === 缓慢旋转 ===
            Projectile.rotation += 0.03f;



            int parentID = (int)Projectile.ai[0];
            if (parentID >= 0 && Main.projectile[parentID].active &&
                Main.projectile[parentID].type == ModContent.ProjectileType<SunsetASunsetRight>())
            {
                Projectile parent = Main.projectile[parentID];
                Player owner = Main.player[parent.owner];

                Vector2 gunHeadPosition = parent.Center + parent.velocity.SafeNormalize(Vector2.Zero) * 80f;
                Projectile.Center = gunHeadPosition;
                Projectile.timeLeft = 60;
            }
            else
            {
                Projectile.Kill();
            }




          


        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            Main.spriteBatch.End();
            Effect shieldEffect = Filters.Scene["CalamityMod:HellBall"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect,
                Main.GameViewMatrix.TransformationMatrix);

            // === Shader参数 ===
            shieldEffect.Parameters["time"].SetValue(Projectile.timeLeft / 60f * 0.24f);
            shieldEffect.Parameters["blowUpPower"].SetValue(3.2f);
            shieldEffect.Parameters["blowUpSize"].SetValue(0.4f);
            shieldEffect.Parameters["noiseScale"].SetValue(0.6f);
            shieldEffect.Parameters["shieldOpacity"].SetValue(Projectile.Opacity);
            shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

            // 改为紫色与金色渐变
            Color edgeColor = Color.Black * Projectile.Opacity;
            Color shieldColor = Color.Lerp(new Color(120, 90, 160), new Color(255, 215, 0), 0.5f) * Projectile.Opacity;
            shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
            shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Main.spriteBatch.Draw(screamTex.Value, pos, null, Color.White * Projectile.Opacity, 0,
                screamTex.Size() * 0.5f, 0.715f * Projectile.scale, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);

            // === 外圈漩涡 + 中心光斑 ===
            Texture2D[] vortexTextures = new Texture2D[]
            {
        //ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SunsetChange").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_04").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_05").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_06").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_08").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/window_04").Value
            };
            Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi;
                // 金色与紫色的交替渐变
                Color outerColor = (i % 2 == 0) ? new Color(255, 215, 0) : new Color(120, 90, 160);
                Color drawColor = Color.Lerp(outerColor, Color.Black, i * 0.15f) * 0.6f;
                drawColor.A = 0;

                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                drawPosition += (angle + Main.GlobalTimeWrappedHourly * i / 16f).ToRotationVector2() * 6f;

                foreach (var texLayer in vortexTextures)
                {
                    Main.EntitySpriteDraw(texLayer, drawPosition, null, drawColor * Projectile.Opacity,
                        -angle + MathHelper.PiOver2, texLayer.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
                }
            }

            // 中心光斑保留
            Main.EntitySpriteDraw(centerTexture, Projectile.Center - Main.screenPosition, null,
                Color.Black * Projectile.Opacity, Projectile.rotation,
                centerTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles,
            List<int> behindNPCs, List<int> behindProjectiles,
            List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
    }
}
