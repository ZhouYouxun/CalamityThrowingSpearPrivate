using System;
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



        // —— 相对坐标适配需要的列表（每种粒子：引用 + 方向 + 半径 + 径向步长） ——
        // Spark
        private readonly List<SparkParticle> ownedSparks = new();
        private readonly List<Vector2> sparkDirs = new();
        private readonly List<float> sparkDists = new();
        private readonly List<float> sparkSteps = new();

        // SquishyLight
        private readonly List<CalamityMod.Particles.SquishyLightParticle> ownedSquishies = new();
        private readonly List<Vector2> squishyDirs = new();
        private readonly List<float> squishyDists = new();
        private readonly List<float> squishySteps = new();

        // GlowOrb
        private readonly List<GlowOrbParticle> ownedOrbs = new();
        private readonly List<Vector2> orbDirs = new();
        private readonly List<float> orbDists = new();
        private readonly List<float> orbSteps = new();

        // WaterFlavored（雾）
        private readonly List<CalamityMod.Particles.WaterFlavoredParticle> ownedMists = new();
        private readonly List<Vector2> mistDirs = new();
        private readonly List<float> mistDists = new();
        private readonly List<float> mistSteps = new();

        // GenericSparkle
        private readonly List<GenericSparkle> ownedSparkles = new();
        private readonly List<Vector2> sparkleDirs = new();
        private readonly List<float> sparkleDists = new();
        private readonly List<float> sparkleSteps = new();

        // 其它你已有的计时器
        // int lifeTimer;  //（保持你现有的）

        public override void AI()
        {
            lifeTimer++;

            // === 淡入淡出（原样保留） ===
            int fadeInTime = 30;
            int fadeOutTime = 30;
            if (lifeTimer <= fadeInTime)
                Projectile.Opacity = lifeTimer / (float)fadeInTime;
            else if (Projectile.timeLeft < fadeOutTime)
                Projectile.Opacity = Projectile.timeLeft / (float)fadeOutTime;
            else
                Projectile.Opacity = 1f;

            // 基础缩放 0.6f（原样保留）
            float pulsate = 1f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f);
            Projectile.scale = 0.6f * pulsate;

            // === 缓慢旋转（原样保留） ===
            Projectile.rotation += 0.03f;

            // === 父弹幕锚定（原样保留） ===
            int parentID = (int)Projectile.ai[0];
            if (parentID >= 0 && Main.projectile[parentID].active &&
                Main.projectile[parentID].type == ModContent.ProjectileType<SunsetCConceptLeftListener>())
            {
                Projectile parent = Main.projectile[parentID];
                Player owner = Main.player[parent.owner];
                SunsetCConceptLeftListener parentProj = parent.ModProjectile as SunsetCConceptLeftListener;

                Vector2 gunHeadPosition = owner.Center + parentProj.InitialDirection.ToRotationVector2() * 150f; // 220是最尖端，150是尖端中心点
                Projectile.Center = gunHeadPosition;
                Projectile.timeLeft = 60;
            }
            else
            {
                Projectile.Kill();
                return;
            }

            // ===================== 粒子效果·黄金能量几何扩散（你的原块 — 完整保留） =====================
            {
                // —— 时间与中心 ——（原样）
                float t = (float)Main.GameUpdateCount * (1f / 60f);
                Vector2 C = Projectile.Center;

                // —— 金黄系调色板（原样）
                Color[] golds = {
            Color.Gold,
            new Color(218,165,32),          // Goldenrod
            new Color(255,238,170),         // Pale gold
            new Color(255,250,205)          // LemonChiffon
        };
                Color GoldBlend(float phase)
                {
                    float x = MathHelper.Clamp(phase - (float)Math.Floor(phase), 0f, 1f);
                    int i0 = (int)(x * (golds.Length));
                    int i1 = (i0 + 1) % golds.Length;
                    float a = x * golds.Length - i0;
                    return Color.Lerp(golds[i0], golds[i1], a);
                }

                // =============== A) 黄金角 Phyllotaxis（SparkParticle）【保留释放】+ 适配登记 ===============
                if ((lifeTimer % 2) == 0)
                {
                    int seeds = 10;
                    float golden = MathHelper.ToRadians(137.50776f);
                    float swirl = 0.8f + 0.4f * (float)Math.Sin(t * 1.7f);
                    float baseSpeed = 6.0f;

                    for (int i = 0; i < seeds; i++)
                    {
                        float k = i + (lifeTimer * 0.25f);
                        float theta = k * golden + t * 1.2f;
                        float r = 6f + 0.9f * k;
                        Vector2 dir = theta.ToRotationVector2();

                        Vector2 spawnPos = C + dir * (r * 0.22f);
                        Vector2 vel = dir * (baseSpeed + 0.03f * r)
                                      + dir.RotatedBy(MathHelper.PiOver2) * (swirl * 0.6f);

                        Color c = GoldBlend((k * 0.25f) + t * 0.35f);

                        //var spark = new SparkParticle(
                        //    spawnPos,
                        //    vel,            // 🔒 保留原始速度（不改）
                        //    false,
                        //    34,
                        //    0.95f,
                        //    c * Projectile.Opacity
                        //);
                        //CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(spark);

                        //// —— 相对坐标适配登记 ——（基于“生成方向”与“当前半径”）
                        //ownedSparks.Add(spark);
                        //sparkDirs.Add(dir);                                // 单位方向（生成时）
                        //sparkDists.Add((spawnPos - C).Length());           // 初始半径 = 与中心距离
                        //                                                   // 径向步长 ≈ vel 在 dir 上的投影；最小步长兜底，防止 0
                        //float step = Vector2.Dot(vel, dir);
                        //sparkSteps.Add(Math.Max(Math.Abs(step), 0.8f));
                    }
                }

                // =============== B) 对数螺线“光丝”（SquishyLightParticle）【保留】+ 适配登记 ===============
                if ((lifeTimer % 3) == 0)
                {
                    int beams = 6;
                    float baseA = t * 0.8f;
                    for (int j = 0; j < beams; j++)
                    {
                        float ang = baseA + MathHelper.TwoPi * j / beams;
                        float pulse = 1f + 0.18f * (float)Math.Sin(3f * baseA + j * 0.7f);
                        Vector2 v = ang.ToRotationVector2() * (6.3f * pulse);

                        var squishy = new CalamityMod.Particles.SquishyLightParticle(
                            C,
                            Vector2.Zero,  // 保留：由内部逻辑挤压
                            0.40f * pulse,
                            GoldBlend(j * 0.17f + t * 0.15f),
                            28,
                            opacity: Projectile.Opacity,
                            squishStrenght: 1.1f + 0.2f * pulse,
                            maxSquish: 3.2f
                        );
                        CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(squishy);
                        squishy.Velocity = v.RotatedBy(MathHelper.ToRadians(2.0f)) * 0.98f; // 原样

                        // —— 适配登记 ——（方向取 ang）
                        ownedSquishies.Add(squishy);
                        Vector2 dir = ang.ToRotationVector2();
                        squishyDirs.Add(dir);
                        squishyDists.Add(0f);                             // 从中心起步
                        float step = Vector2.Dot(squishy.Velocity, dir);
                        squishySteps.Add(Math.Max(Math.Abs(step), 0.9f));
                    }
                }

                // =============== C) 内摆线星轨（GlowOrbParticle）【保留】+ 适配登记 ===============
                if ((lifeTimer % 4) == 0)
                {
                    float R = 12f, r = 5f, d = 16f;
                    int stars = 12;
                    float omega = 1.4f;
                    for (int s = 0; s < stars; s++)
                    {
                        float theta = (s / (float)stars) * MathHelper.TwoPi + t * omega;
                        float k = (R - r) / r;

                        Vector2 p = new Vector2(
                            (R - r) * (float)Math.Cos(theta) + d * (float)Math.Cos(k * theta),
                            (R - r) * (float)Math.Sin(theta) - d * (float)Math.Sin(k * theta)
                        );
                        Vector2 dir = p.SafeNormalize(Vector2.UnitX);

                        Vector2 jitter = Main.rand.NextVector2CircularEdge(0.6f, 0.6f);
                        Vector2 vel = dir * 5.2f + jitter;

                        var orb = new GlowOrbParticle(
                            C,
                            vel,                  // 原样
                            false,
                            18,
                            0.8f,
                            GoldBlend(s * 0.08f + t * 0.22f) * Projectile.Opacity,
                            true, false, true
                        );
                        CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(orb);

                        // —— 适配登记 ——
                        ownedOrbs.Add(orb);
                        orbDirs.Add(dir);
                        orbDists.Add(0f);
                        float step = Vector2.Dot(vel, dir);
                        orbSteps.Add(Math.Max(Math.Abs(step), 0.8f));
                    }
                }

                // =============== D) 金雾“呼吸环”（WaterFlavoredParticle）【保留】+ 适配登记 ===============
                if ((lifeTimer % 6) == 0)
                {
                    int ringCount = 14;
                    float breath = 1f + 0.12f * (float)Math.Sin(t * 1.25f);
                    float r0 = 10f * breath;

                    for (int i = 0; i < ringCount; i++)
                    {
                        float ang = MathHelper.TwoPi * i / ringCount + t * 0.3f;
                        Vector2 dir = ang.ToRotationVector2();
                        Vector2 spawn = C + dir * (r0 * 0.15f);
                        Vector2 vel = dir * 1.2f;

                        var mist = new CalamityMod.Particles.WaterFlavoredParticle(
                            spawn,
                            vel, // 原样
                            false,
                            Main.rand.Next(18, 26),
                            0.85f + Main.rand.NextFloat(0.25f),
                            GoldBlend(i * 0.11f + t * 0.1f) * 0.85f
                        );
                        CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(mist);

                        // —— 适配登记 ——
                        ownedMists.Add(mist);
                        mistDirs.Add(dir);
                        mistDists.Add((spawn - C).Length()); // 初始就有半径
                        float step = Vector2.Dot(vel, dir);
                        mistSteps.Add(Math.Max(Math.Abs(step), 0.6f));
                    }
                }

                // =============== E) “击打脉冲”星芒（GenericSparkle）【保留】+ 适配登记 ===============
                if ((lifeTimer % 18) == 0)
                {
                    int spokes = 8;
                    for (int i = 0; i < spokes; i++)
                    {
                        float ang = MathHelper.TwoPi * i / spokes + t * 0.5f;
                        Vector2 v = ang.ToRotationVector2() * 7.5f;

                        var sp = new GenericSparkle(
                            C,
                            v,  // 原样
                            GoldBlend(i * 0.13f + t * 0.3f),
                            Color.White,
                            2.1f,
                            22,
                            Main.rand.NextFloat(-0.02f, 0.02f),
                            1.65f
                        );
                        CalamityMod.Particles.GeneralParticleHandler.SpawnParticle(sp);

                        // —— 适配登记 ——
                        ownedSparkles.Add(sp);
                        Vector2 dir = ang.ToRotationVector2();
                        sparkleDirs.Add(dir);
                        sparkleDists.Add(0f);
                        float step = Vector2.Dot(v, dir);
                        sparkleSteps.Add(Math.Max(Math.Abs(step), 0.9f));
                    }
                }
            }
            // ===================== 粒子效果·黄金能量几何扩散（你的原块 — 完整保留） =====================

            // ===================== 相对坐标更新（核心适配：始终以本体为原点推进） =====================
            // Spark
            for (int i = ownedSparks.Count - 1; i >= 0; i--)
            {
                var p = ownedSparks[i];
                if (p.Time >= p.Lifetime)
                {
                    ownedSparks.RemoveAt(i);
                    sparkDirs.RemoveAt(i);
                    sparkDists.RemoveAt(i);
                    sparkSteps.RemoveAt(i);
                    continue;
                }
                sparkDists[i] += sparkSteps[i];
                p.Position = Projectile.Center + sparkDirs[i] * sparkDists[i];
                p.Velocity = Vector2.Zero; // 避免世界速度叠加
            }

            // SquishyLight
            for (int i = ownedSquishies.Count - 1; i >= 0; i--)
            {
                var p = ownedSquishies[i];
                if (p.Time >= p.Lifetime)
                {
                    ownedSquishies.RemoveAt(i);
                    squishyDirs.RemoveAt(i);
                    squishyDists.RemoveAt(i);
                    squishySteps.RemoveAt(i);
                    continue;
                }
                squishyDists[i] += squishySteps[i];
                p.Position = Projectile.Center + squishyDirs[i] * squishyDists[i];
                p.Velocity = Vector2.Zero;
            }

            // GlowOrb
            for (int i = ownedOrbs.Count - 1; i >= 0; i--)
            {
                var p = ownedOrbs[i];
                if (p.Time >= p.Lifetime)
                {
                    ownedOrbs.RemoveAt(i);
                    orbDirs.RemoveAt(i);
                    orbDists.RemoveAt(i);
                    orbSteps.RemoveAt(i);
                    continue;
                }
                orbDists[i] += orbSteps[i];
                p.Position = Projectile.Center + orbDirs[i] * orbDists[i];
                p.Velocity = Vector2.Zero;
            }

            // WaterFlavored
            for (int i = ownedMists.Count - 1; i >= 0; i--)
            {
                var p = ownedMists[i];
                if (p.Time >= p.Lifetime)
                {
                    ownedMists.RemoveAt(i);
                    mistDirs.RemoveAt(i);
                    mistDists.RemoveAt(i);
                    mistSteps.RemoveAt(i);
                    continue;
                }
                mistDists[i] += mistSteps[i];
                p.Position = Projectile.Center + mistDirs[i] * mistDists[i];
                p.Velocity = Vector2.Zero;
            }

            // GenericSparkle
            for (int i = ownedSparkles.Count - 1; i >= 0; i--)
            {
                var p = ownedSparkles[i];
                if (p.Time >= p.Lifetime)
                {
                    ownedSparkles.RemoveAt(i);
                    sparkleDirs.RemoveAt(i);
                    sparkleDists.RemoveAt(i);
                    sparkleSteps.RemoveAt(i);
                    continue;
                }
                sparkleDists[i] += sparkleSteps[i];
                p.Position = Projectile.Center + sparkleDirs[i] * sparkleDists[i];
                p.Velocity = Vector2.Zero;
            }
            // ===================== 相对坐标更新 =====================
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
