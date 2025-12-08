using System;
using System.Collections.Generic;
using CalamityMod.Graphics.Metaballs;
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

        private float metaballOrbitAngle; // 熔岩球绕弹幕旋转的相位
        private int bloomTimer;           // BloomRing 计时器
        private int sparkTimer;           // CritSpark 计时器
        private readonly List<BloomRing> ownedBloomRings = new();
        private readonly List<CritSpark> ownedCritSparks = new();

        private Vector2 lastCenter;


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

            // 缩放轻微呼吸
            float pulsate = 1f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Projectile.scale = 0.6f * pulsate;

            // 缓慢自转
            Projectile.rotation += 0.03f;

            int parentID = (int)Projectile.ai[0];
            if (parentID >= 0 && Main.projectile[parentID].active &&
                Main.projectile[parentID].type == ModContent.ProjectileType<SunsetASunsetRight>())
            {
                Projectile parent = Main.projectile[parentID];
                Player owner = Main.player[parent.owner];

                // 绑定在枪口
                Vector2 gunHeadPosition = parent.Center + parent.velocity.SafeNormalize(Vector2.UnitX) * 80f;
                Projectile.Center = gunHeadPosition;
                Projectile.timeLeft = 60;

                if (!Main.dedServ)
                {
                    //// ===========================
                    //// 1）熔岩 Metaball：绕弹幕圆周旋转，每帧一个
                    //// ===========================
                    //metaballOrbitAngle += 0.18f;
                    //float orbitRadius = 32f;
                    //Vector2 orbitOffset = new Vector2(
                    //    (float)Math.Cos(metaballOrbitAngle),
                    //    (float)Math.Sin(metaballOrbitAngle)
                    //) * orbitRadius;

                    //RancorLavaMetaball.SpawnParticle(
                    //    Projectile.Center + orbitOffset,
                    //    Main.rand.NextFloat(60f, 100f)
                    //);

                    // ===========================
                    // 2）BloomRing：原地光晕，每 3 帧一个（橙黄系）
                    // ===========================
                    bloomTimer++;
                    if (bloomTimer % 3 == 0)
                    {
                        Color ringColor = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat(0.2f, 0.9f));
                        BloomRing bloomRing = new BloomRing(
                            Projectile.Center,
                            Vector2.Zero,
                            ringColor,
                            1.0f,
                            45
                        );
                        GeneralParticleHandler.SpawnParticle(bloomRing);
                        ownedBloomRings.Add(bloomRing);
                    }
                    // ===========================
                    // 3）CritSpark：宏伟金色魔法阵辐射，每 5 帧一轮
                    // ===========================
                    sparkTimer++;
                    if (sparkTimer % 5 == 0)
                    {
                        int sparkCount = 30; // 数量×5，密度大幅提升
                        float baseAngle = Main.GlobalTimeWrappedHourly * 2.3f; // 整体相位旋转（稍快一点）

                        for (int i = 0; i < sparkCount; i++)
                        {
                            float progress = i / (float)sparkCount; // 0~1，用于数学分布

                            // 基础角度 + 叠加三叶玫瑰形扰动，形成花瓣状辐射
                            float angle = baseAngle
                                          + MathHelper.TwoPi * progress
                                          + 0.5f * (float)Math.Sin(3f * baseAngle + progress * MathHelper.TwoPi);

                            // 半径速度随 progress 非线性变化，让中段更强、两端略弱
                            float radialSpeed = MathHelper.Lerp(4f, 8f,
                                (float)Math.Sin(progress * MathHelper.Pi) * 0.5f + 0.5f);

                            Vector2 dir = angle.ToRotationVector2();
                            Vector2 sparkVelocity = dir * radialSpeed;

                            // 金色系渐变：橙金 -> 亮金黄
                            Color startColor = Color.Lerp(Color.Orange, Color.Gold,
                                0.5f + 0.5f * (float)Math.Sin(baseAngle + progress * 6f));
                            Color endColor = Color.Lerp(startColor, Color.Yellow, 0.6f);

                            //CritSpark spark = new CritSpark(
                            //    Projectile.Center,
                            //    sparkVelocity + owner.velocity, // 仍略带玩家速度
                            //    startColor,
                            //    endColor,
                            //    1.05f,  // 稍微放大一点
                            //    18      // 略长一点寿命，配合宏伟感
                            //);
                            //GeneralParticleHandler.SpawnParticle(spark);
                            //ownedCritSparks.Add(spark);
                        }
                    }

                }

                // ===========================
                // 4）相对坐标跟随模块（放在 AI 最底部）
                // ===========================
                if (!Main.dedServ)
                {
                    // 计算本帧弹幕位移
                    if (lastCenter == Vector2.Zero)
                        lastCenter = Projectile.Center;

                    Vector2 delta = Projectile.Center - lastCenter;
                    lastCenter = Projectile.Center;

                    // BloomRing 跟随弹幕移动
                    for (int i = ownedBloomRings.Count - 1; i >= 0; i--)
                    {
                        BloomRing p = ownedBloomRings[i];

                        if (p.Time >= p.Lifetime)
                        {
                            ownedBloomRings.RemoveAt(i);
                            continue;
                        }

                        // 让粒子整体随着弹幕平移
                        p.Position += delta;
                    }

                    // CritSpark 跟随弹幕移动
                    for (int i = ownedCritSparks.Count - 1; i >= 0; i--)
                    {
                        CritSpark p = ownedCritSparks[i];

                        if (p.Time >= p.Lifetime)
                        {
                            ownedCritSparks.RemoveAt(i);
                            continue;
                        }

                        // 保留自身辐射运动 + 再叠加弹幕位移
                        p.Position += delta;
                    }
                }
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
        //ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_04").Value,
        //ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_05").Value,
        //ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_06").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/Sun/fbmnoise2_003").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/Sun/fbmnoise2_004").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/Sun/fbmnoise2_005").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/Sun/fbmnoise2_006").Value,
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
