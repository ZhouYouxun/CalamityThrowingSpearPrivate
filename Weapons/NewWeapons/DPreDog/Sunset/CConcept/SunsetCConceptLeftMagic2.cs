using System.Collections.Generic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    public class SunsetCConceptLeftMagic2 : ModProjectile
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



            // === 父弹幕锚定（保持在枪口位置） ===
            int parentID = (int)Projectile.ai[0];
            if (parentID >= 0 && Main.projectile[parentID].active &&
                Main.projectile[parentID].type == ModContent.ProjectileType<SunsetCConceptLeftListener>())
            {
                Projectile parent = Main.projectile[parentID];
                Player owner = Main.player[parent.owner];
                SunsetCConceptLeftListener parentProj = parent.ModProjectile as SunsetCConceptLeftListener;

                Vector2 gunHeadPosition = owner.Center + parentProj.InitialDirection.ToRotationVector2() * 80f;
                Projectile.Center = gunHeadPosition;
                Projectile.timeLeft = 60; 
            }
            else
            {
                Projectile.Kill();
            }



            {


                // ===================== 粒子效果·黄金能量几何扩散（替换本块） =====================
                {
                    // —— 时间与中心 ——
                    float t = (float)Main.GameUpdateCount * (1f / 60f); // 连续时间（秒）
                    Vector2 C = Projectile.Center;

                    // —— 金黄系调色板（3~4色）——
                    // 纯金 / 深金 / 淡金 / 柠檬淡黄（加法混合时层次清晰）
                    Color[] golds = {
        Color.Gold,
        new Color(218,165,32),          // Goldenrod
        new Color(255,238,170),         // Pale gold
        new Color(255,250,205)          // LemonChiffon
    };

                    // 小工具：按权重在金色之间做插值（让色彩在动）
                    Color GoldBlend(float phase)
                    {
                        float x = MathHelper.Clamp(phase - (float)System.Math.Floor(phase), 0f, 1f);
                        int i0 = (int)(x * (golds.Length));
                        int i1 = (i0 + 1) % golds.Length;
                        float a = x * golds.Length - i0;
                        return Color.Lerp(golds[i0], golds[i1], a);
                    }

                    // =============== A) 黄金角 Phyllotaxis 爆发（SparkParticle） ===============
                    // 复杂度升级：每 2 帧打一小束，沿 φ=137.50776° 的种子序列，叠加轻微对数螺线切向速度
                    if ((lifeTimer % 2) == 0)
                    {
                        int seeds = 10;                                      // 每束点数（控制强度）
                        float golden = MathHelper.ToRadians(137.50776f);     // 黄金角
                        float swirl = 0.8f + 0.4f * (float)System.Math.Sin(t * 1.7f); // 切向“旋味”
                        float baseSpeed = 6.0f;

                        for (int i = 0; i < seeds; i++)
                        {
                            float k = i + (lifeTimer * 0.25f);               // 种子序位 + 时间滚动
                            float theta = k * golden + t * 1.2f;             // 旋转推进
                                                                             // 对数螺线：r = a * e^{bθ} 的离散近似（这里用线性近似避免过度发散）
                            float r = 6f + 0.9f * k;                          // 生成环半径（像“花盘”渐外）
                            Vector2 dir = theta.ToRotationVector2();

                            Vector2 spawnPos = C + dir * (r * 0.22f);         // 在核心附近生成
                            Vector2 vel = dir * (baseSpeed + 0.03f * r)       // 径向向外
                                          + dir.RotatedBy(MathHelper.PiOver2) * (swirl * 0.6f); // 加一点切向

                            Color c = GoldBlend((k * 0.25f) + t * 0.35f);

                            var spark = new SparkParticle(
                                spawnPos,
                                vel,
                                false,
                                34,                         // 寿命短促但连续喷发
                                0.95f,                      // 稍细的线性火花
                                c * Projectile.Opacity
                            );
                            CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(spark);
                        }
                    }

                    // =============== B) 对数螺线“光丝”脉冲（SquishyLightParticle） ===============
                    // 6 条光丝以 60° 分布，速度含“径向 + 微右拐阻尼”，形成层层外舒的金丝
                    if ((lifeTimer % 3) == 0)
                    {
                        int beams = 6;
                        float baseA = t * 0.8f; // 主相位
                        for (int j = 0; j < beams; j++)
                        {
                            float ang = baseA + MathHelper.TwoPi * j / beams;
                            float pulse = 1f + 0.18f * (float)System.Math.Sin(3f * baseA + j * 0.7f);
                            Vector2 v = ang.ToRotationVector2() * (6.3f * pulse);

                            var squishy = new CalamityMod.Particles.SquishyLightParticle(
                                C,
                                Vector2.Zero,                  // 我们用位置推进
                                0.40f * pulse,                 // 缩放随脉冲
                                GoldBlend(j * 0.17f + t * 0.15f),
                                28,                            // 中等寿命
                                opacity: Projectile.Opacity,
                                squishStrenght: 1.1f + 0.2f * pulse,
                                maxSquish: 3.2f
                            );
                            CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(squishy);

                            // 初速度：右拐 + 阻尼（通过后续每帧内部逻辑表现，这里先给初速）
                            squishy.Velocity = v.RotatedBy(MathHelper.ToRadians(2.0f)) * 0.98f;
                        }
                    }

                    // =============== C) 内摆线（Hypotrochoid）星轨点阵（GlowOrbParticle） ===============
                    // 参数方程：
                    // x = (R - r) cosθ + d cos((R - r)/r * θ)
                    // y = (R - r) sinθ - d sin((R - r)/r * θ)
                    // 我们只用它来指定“发射方向”，粒子依旧从中心外喷
                    if ((lifeTimer % 4) == 0)
                    {
                        float R = 12f, r = 5f, d = 16f; // 星形参数可调
                        int stars = 12;
                        float omega = 1.4f;             // 旋速
                        for (int s = 0; s < stars; s++)
                        {
                            float theta = (s / (float)stars) * MathHelper.TwoPi + t * omega;
                            float k = (R - r) / r;

                            // 取轨迹的法线方向作为“喷射角”
                            Vector2 p = new Vector2(
                                (R - r) * (float)System.Math.Cos(theta) + d * (float)System.Math.Cos(k * theta),
                                (R - r) * (float)System.Math.Sin(theta) - d * (float)System.Math.Sin(k * theta)
                            );
                            Vector2 dir = p.SafeNormalize(Vector2.UnitX);

                            var orb = new CalamityMod.Particles.GlowOrbParticle(
                                C,
                                dir * 5.2f + Main.rand.NextVector2CircularEdge(0.6f, 0.6f), // 径向 + 少量扰动
                                false,
                                18,                             // 短促高亮
                                0.8f,
                                GoldBlend(s * 0.08f + t * 0.22f) * Projectile.Opacity,
                                true, false, true
                            );
                            CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(orb);
                        }
                    }

                    // =============== D) 金雾“呼吸环”（WaterFlavoredParticle） ===============
                    // 每 6 帧生成一圈柔雾，半径做正弦呼吸式脉动，速度极低形成氤氲
                    if ((lifeTimer % 6) == 0)
                    {
                        int ringCount = 14;
                        float breath = 1f + 0.12f * (float)System.Math.Sin(t * 1.25f);
                        float r0 = 10f * breath;

                        for (int i = 0; i < ringCount; i++)
                        {
                            float ang = MathHelper.TwoPi * i / ringCount + t * 0.3f;
                            Vector2 dir = ang.ToRotationVector2();
                            var mist = new CalamityMod.Particles.WaterFlavoredParticle(
                                C + dir * (r0 * 0.15f),            // 靠近中心一点生成
                                dir * 1.2f,                        // 慢速外扩
                                false,
                                Main.rand.Next(18, 26),
                                0.85f + Main.rand.NextFloat(0.25f),
                                GoldBlend(i * 0.11f + t * 0.1f) * 0.85f
                            );
                            CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(mist);
                        }
                    }

                    // =============== E) “击打脉冲”星芒（GenericSparkle） ===============
                    // 每 18 帧触发一次轻打击（像音乐节拍），从圆周若干点向外喷星芒
                    if ((lifeTimer % 18) == 0)
                    {
                        int spokes = 8;
                        for (int i = 0; i < spokes; i++)
                        {
                            float ang = MathHelper.TwoPi * i / spokes + t * 0.5f;
                            Vector2 v = ang.ToRotationVector2() * 7.5f;

                            var sp = new CalamityMod.Particles.GenericSparkle(
                                C,
                                v,
                                GoldBlend(i * 0.13f + t * 0.3f),         // 主色
                                Color.White,                              // 辉光
                                2.1f,                                     // 尺寸
                                22,                                       // 寿命
                                Main.rand.NextFloat(-0.02f, 0.02f),       // 自转
                                1.65f                                     // 光晕扩散
                            );
                            CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(sp);
                        }
                    }
                }
                // ===================== 粒子效果·黄金能量几何扩散（替换本块） =====================



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
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_01").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_02").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_03").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_04").Value,
        ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_05").Value
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
