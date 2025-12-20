using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptRightCut3Space : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/RC/空间";

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/RC/空间"
            ).Value;

            CalamityUtils.DrawAfterimagesCentered(
                Projectile,
                ProjectileID.Sets.TrailingMode[Projectile.type],
                lightColor,
                1,
                texture
            );

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 62;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
            Projectile.scale = 0.6f;
        }
        public override void OnSpawn(IEntitySource source)
        {

            //// 生成粒子爆炸效果
            //Particle blastRing = new CustomPulse(
            //    Projectile.Center, // 以弹幕为中心
            //    Vector2.Zero,
            //    Color.White,
            //    "CalamityThrowingSpear/Texture/YingYang",
            //    Vector2.One * 0.33f,
            //    Main.rand.NextFloat(-10f, 10f),
            //    0.07f,
            //    0.15f,
            //    15
            //);
            //GeneralParticleHandler.SpawnParticle(blastRing);

            Player owner = Main.player[Projectile.owner];
            int totalCrit = (int)Math.Round(owner.GetTotalCritChance(Projectile.DamageType));
            Projectile.CritChance = totalCrit;

        }



        private int lifeTimer = 0; // 存活帧数计时器
        private bool flightInited = false; // 是否已初始化飞行方向
        private Vector2 launchDir;         // 出生时的“前进方向”，只记录一次


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中瞬间锁定当前前方向
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            // ① 计算“敌人背后”的位置（前方320像素）
            float behindDistance = 20f * 16f; // 20 tile
            Vector2 teleportPos = target.Center + forward * behindDistance;

            // ② 传送
            Projectile.Center = teleportPos;

            // ③ 清除旧速度
            Projectile.velocity = Vector2.Zero;

            // ④ 面向敌人进行二段冲刺
            Vector2 dirToEnemy = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            float dashSpeed = 45f; // 冲刺速度你可以随便调
            Projectile.velocity = dirToEnemy * dashSpeed;

            // ⑤ 播放冲刺音效（需要的话）
            SoundEngine.PlaySound(SoundID.Item68, Projectile.Center);
        }

        public override void AI()
        {
            // ① 初次记录朝向
            if (!flightInited)
            {
                flightInited = true;

                // 初次朝向 = 初速度方向（无速度则根据 spriteDirection 推测）
                if (Projectile.velocity.LengthSquared() > 0.01f)
                    launchDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                else
                    launchDir = (Projectile.spriteDirection >= 0) ? Vector2.UnitX : -Vector2.UnitX;

                lifeTimer = 0;
            }

            lifeTimer++;

            // ② 主动逐渐加速
            Projectile.velocity *= 1.01f;   // 每帧乘以 1.01
            if (Projectile.velocity.Length() > 64f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 64f;

            // ③ 旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ④ 你原本的特效区域跟在这里，不动





            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ================================
            // 空间主题调色盘（紫 / 银 / 白）
            // ================================
            Color spaceCore = new Color(190, 140, 255);      // 亮紫
            Color spaceDeep = new Color(80, 40, 130);        // 深紫
            Color spaceEdge = new Color(230, 220, 255);      // 银白
            Color spaceMist = new Color(180, 200, 255);      // 淡蓝白雾
            Color spaceSquare = new Color(160, 210, 255);    // 冷青白，偏科技

            Color[] starPalette = new Color[]
            {
                spaceCore,
                spaceEdge,
                Color.White,
                new Color(210, 180, 255)
            };

            Vector2 center = Projectile.Center;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 back = -forward;
            Vector2 perp = forward.RotatedBy(MathHelper.PiOver2);

            float t = lifeTimer / 60f;

            // ================================
            // ① 空间丝带轨迹（GlowOrb 多层螺旋带）
            // ================================
            {
                int ribbons = 3;
                int orbsPerRibbon = 2;

                for (int r = 0; r < ribbons; r++)
                {
                    float phase = t * (2.2f + r * 0.8f) + r * 1.3f;
                    float radial = 6f + r * 4f + (float)Math.Sin(t * 3f + r) * 2f;

                    for (int i = 0; i < orbsPerRibbon; i++)
                    {
                        float localPhase = phase + i * 0.9f;
                        Vector2 offset =
                            back * (4f + r * 3f) +
                            perp * (float)Math.Sin(localPhase) * radial;

                        Vector2 pos = center + offset;
                        Vector2 vel = -back * Main.rand.NextFloat(0.2f, 0.8f);

                        GlowOrbParticle orb = new GlowOrbParticle(
                            pos,
                            vel,
                            false,
                            Main.rand.Next(14, 22),
                            Main.rand.NextFloat(0.7f, 1.2f),
                            starPalette[Main.rand.Next(starPalette.Length)],
                            true,
                            false,
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
            }

            // ================================
            // ② 空间网格碎片（SquareParticle 旋转方块阵）
            // ================================
            if (lifeTimer % 2 == 0)
            {
                int squares = 4;
                float radius = 10f + (float)Math.Sin(t * 2.3f) * 3f;
                float baseRot = t * 1.7f;

                for (int i = 0; i < squares; i++)
                {
                    float k = i / (float)squares;
                    float ang = baseRot + MathHelper.TwoPi * k;

                    Vector2 dir = forward.RotatedBy(ang);
                    Vector2 pos = center + dir * radius;

                    Vector2 vel = dir * Main.rand.NextFloat(0.5f, 1.8f);

                    SquareParticle sq = new SquareParticle(
                        pos,
                        vel,
                        false,
                        Main.rand.Next(18, 26),
                        Main.rand.NextFloat(1.2f, 1.9f),
                        spaceSquare
                    );
                    GeneralParticleHandler.SpawnParticle(sq);
                }
            }

            // ================================
            // ③ 星屑风暴（PointParticle 沿轨迹抛洒）
            // ================================
            {
                int points = 3;
                for (int i = 0; i < points; i++)
                {
                    Vector2 pos =
                        center
                        - back * Main.rand.NextFloat(4f, 12f)
                        + perp * Main.rand.NextFloat(-6f, 6f);

                    Vector2 vel =
                        back * Main.rand.NextFloat(2.2f, 5.0f) +
                        perp * Main.rand.NextFloat(-1.5f, 1.5f);

                    PointParticle p = new PointParticle(
                        pos,
                        vel,
                        false,
                        Main.rand.Next(16, 26),
                        Main.rand.NextFloat(0.9f, 1.4f),
                        starPalette[Main.rand.Next(starPalette.Length)]
                    );
                    GeneralParticleHandler.SpawnParticle(p);
                }
            }

            // ================================
            // ④ 空间裂缝（CrackParticle 小幅撕裂）
            // ================================
            if (lifeTimer % 4 == 0)
            {
                int cracks = 2;
                for (int i = 0; i < cracks; i++)
                {
                    Vector2 pos =
                        center
                        + perp * Main.rand.NextFloat(-6f, 6f)
                        - back * Main.rand.NextFloat(0f, 8f);

                    Vector2 vel =
                        back * Main.rand.NextFloat(0.8f, 1.6f);

                    CrackParticle crack = new CrackParticle(
                        pos,
                        vel,
                        Color.Lerp(spaceDeep, spaceCore, 0.7f),
                        new Vector2(1.1f, 1.3f),
                        Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                        0.6f,
                        1.4f,
                        28
                    );
                    GeneralParticleHandler.SpawnParticle(crack);
                }
            }

            // ================================
            // ⑤ 空间薄雾（WaterFlavoredParticle 漂浮雾）
            // ================================
            if (lifeTimer % 3 == 0)
            {
                Vector2 mistPos =
                    center
                    + perp * Main.rand.NextFloat(-4f, 4f)
                    - back * Main.rand.NextFloat(2f, 6f);

                Vector2 mistVel =
                    -back * Main.rand.NextFloat(0.4f, 1.2f);

                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    mistPos,
                    mistVel,
                    false,
                    Main.rand.Next(20, 32),
                    0.9f + Main.rand.NextFloat(0.4f),
                    spaceMist * Main.rand.NextFloat(0.7f, 1.1f)
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }

            // ================================
            // ⑥ 小型空间脉冲（DirectionalPulseRing）
            // ================================
            if (lifeTimer % 6 == 0)
            {
                Particle pulse = new DirectionalPulseRing(
                    center,
                    forward * 0.6f,
                    Color.Lerp(spaceCore, spaceEdge, 0.5f),
                    new Vector2(1f, 2.2f),
                    Projectile.rotation,
                    0.15f,
                    0.025f,
                    18
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // ================================
            // ⑦ 星点闪烁（GenericSparkle 伪“星空噪声”）
            // ================================
            if (lifeTimer % 5 == 0)
            {
                int sparkles = 2;
                for (int i = 0; i < sparkles; i++)
                {
                    Vector2 pos =
                        center
                        + perp * Main.rand.NextFloat(-10f, 10f)
                        - back * Main.rand.NextFloat(4f, 14f);

                    GenericSparkle sparker = new GenericSparkle(
                        pos,
                        Vector2.Zero,
                        starPalette[Main.rand.Next(starPalette.Length)],
                        Color.White,
                        Main.rand.NextFloat(1.1f, 1.7f),
                        10,
                        Main.rand.NextFloat(-0.03f, 0.03f),
                        1.4f
                    );
                    GeneralParticleHandler.SpawnParticle(sparker);
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SunsetPlayerSpeed.ApplyNoArmorHypothesisHitEffect(
                Projectile,
                target,
                ref modifiers
            );
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center;

            Color spaceCore = new Color(190, 140, 255);
            Color spaceDeep = new Color(70, 30, 120);
            Color spaceEdge = new Color(235, 230, 255);
            Color spaceMist = new Color(180, 200, 255);
            Color spaceSquare = new Color(160, 210, 255);

            Color[] starPalette = new Color[]
            {
                spaceCore,
                spaceEdge,
                Color.White,
                new Color(210, 180, 255)
            };

            float baseRot = Main.rand.NextFloat(MathHelper.TwoPi);

            // ================================
            // ① 多重椭圆空间冲击波（3 层 DirectionalPulseRing）
            // ================================
            for (int i = 0; i < 3; i++)
            {
                float rotOffset = baseRot + i * 0.6f;
                Vector2 vel = rotOffset.ToRotationVector2() * (0.6f + i * 0.3f);

                Particle pulse = new DirectionalPulseRing(
                    center,
                    vel,
                    Color.Lerp(spaceCore, spaceEdge, 0.4f + 0.2f * i),
                    new Vector2(1f, 2.2f + i * 0.8f),
                    rotOffset,
                    0.15f + 0.05f * i,
                    0.04f + 0.02f * i,
                    22 + i * 4
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // ================================
            // ② 星环构型（GenericSparkle 外环星圈）
            // ================================
            {
                int seg = 24;
                float radius = 90f;
                for (int i = 0; i < seg; i++)
                {
                    float ang = baseRot + MathHelper.TwoPi * i / seg;
                    Vector2 pos = center + ang.ToRotationVector2() * radius;

                    GenericSparkle sparker = new GenericSparkle(
                        pos,
                        Vector2.Zero,
                        starPalette[i % starPalette.Length],
                        Color.White,
                        Main.rand.NextFloat(1.6f, 2.4f),
                        14,
                        Main.rand.NextFloat(-0.04f, 0.04f),
                        1.8f
                    );
                    GeneralParticleHandler.SpawnParticle(sparker);
                }
            }

            // ================================
            // ③ 内部空间碎片矩阵（SquareParticle + GlowOrb）
            // ================================
            {
                int gridRings = 3;
                int baseSeg = 10;

                for (int r = 0; r < gridRings; r++)
                {
                    float ringR = 30f + r * 14f;
                    int seg = baseSeg + r * 4;

                    for (int i = 0; i < seg; i++)
                    {
                        float ang = baseRot * (1f + 0.15f * r) + MathHelper.TwoPi * i / seg;
                        Vector2 dir = ang.ToRotationVector2();
                        Vector2 pos = center + dir * ringR;

                        Vector2 velSq = dir * Main.rand.NextFloat(1.5f, 4.0f);

                        SquareParticle sq = new SquareParticle(
                            pos,
                            velSq,
                            false,
                            Main.rand.Next(22, 32),
                            Main.rand.NextFloat(1.4f, 2.0f),
                            spaceSquare
                        );
                        GeneralParticleHandler.SpawnParticle(sq);

                        if (Main.rand.NextBool(3))
                        {
                            GlowOrbParticle orb = new GlowOrbParticle(
                                pos,
                                dir * Main.rand.NextFloat(0.4f, 1.0f),
                                false,
                                Main.rand.Next(14, 20),
                                Main.rand.NextFloat(0.8f, 1.3f),
                                starPalette[Main.rand.Next(starPalette.Length)],
                                true,
                                false,
                                true
                            );
                            GeneralParticleHandler.SpawnParticle(orb);
                        }
                    }
                }
            }

            // ================================
            // ④ 空间裂缝放射（CrackParticle 星芒裂纹）
            // ================================
            {
                int crackRays = 8;
                for (int i = 0; i < crackRays; i++)
                {
                    float ang = baseRot + MathHelper.TwoPi * i / crackRays;
                    Vector2 dir = ang.ToRotationVector2();

                    Vector2 vel = dir * Main.rand.NextFloat(4f, 9f);

                    CrackParticle crack = new CrackParticle(
                        center,
                        vel,
                        Color.Lerp(spaceDeep, spaceCore, 0.8f),
                        new Vector2(1.2f, 1.6f),
                        Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                        0.07f,
                        0.38f,
                        32
                    );
                    GeneralParticleHandler.SpawnParticle(crack);
                }
            }

            // ================================
            // ⑤ 星屑爆 rain（PointParticle 大范围星雨）
            // ================================
            {
                int starCount = 40;
                for (int i = 0; i < starCount; i++)
                {
                    float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dir = ang.ToRotationVector2();

                    Vector2 pos = center + dir * Main.rand.NextFloat(0f, 26f);
                    Vector2 vel = dir * Main.rand.NextFloat(6f, 16f);

                    PointParticle p = new PointParticle(
                        pos,
                        vel,
                        false,
                        Main.rand.Next(18, 28),
                        Main.rand.NextFloat(1.0f, 1.6f),
                        starPalette[Main.rand.Next(starPalette.Length)]
                    );
                    GeneralParticleHandler.SpawnParticle(p);
                }
            }

            // ================================
            // ⑥ 中心空间雾核（WaterFlavoredParticle）
            // ================================
            {
                int mistCount = 8;
                for (int i = 0; i < mistCount; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                    Vector2 vel = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(-1.2f, 1.2f);

                    WaterFlavoredParticle mist = new WaterFlavoredParticle(
                        center + offset,
                        vel,
                        false,
                        Main.rand.Next(26, 40),
                        1.0f + Main.rand.NextFloat(0.5f),
                        spaceMist * Main.rand.NextFloat(0.8f, 1.2f)
                    );
                    GeneralParticleHandler.SpawnParticle(mist);
                }
            }

            // ================================
            // ⑦ 中心星核闪烁（GenericSparkle 核心闪光）
            // ================================
            {
                GenericSparkle core = new GenericSparkle(
                    center,
                    Vector2.Zero,
                    spaceEdge,
                    spaceCore,
                    Main.rand.NextFloat(2.2f, 3.0f),
                    18,
                    Main.rand.NextFloat(-0.03f, 0.03f),
                    2.1f
                );
                GeneralParticleHandler.SpawnParticle(core);
            }
        }


















    }
}