using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.Buffs.DamageOverTime;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav
{
    internal class RedtideJavRight : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/RedtideJav/RedtideJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 4 * 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 15; // 可击中次数
            Projectile.timeLeft = 150;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1; // 可调节飞行平滑度
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 弹幕生成时执行，用于初始化粒子或播放生成音效
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            {
                // 💥 从圆环区域中随机选一点作为喷射源
                if (Main.rand.NextBool(1)) // 控制频率，越小越密集
                {
                    // 随机圆环位置：内半径32，外半径48
                    float radius = Main.rand.NextFloat(32f, 48f);
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Vector2 emitPosition = Projectile.Center + offset;

                    // 💦 随机喷射水珠或蓝光特效
                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(
                            emitPosition,
                            Main.rand.NextBool(3) ? DustID.GemSapphire : DustID.Water,
                            offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.2f, 3.5f),
                            100,
                            Color.White,
                            Main.rand.NextFloat(0.8f, 1.4f)
                        );
                        d.noGravity = true;
                    }

                    // ✨ 深蓝 SparkParticle 闪烁
                    if (Main.rand.NextBool(5))
                    {
                        Particle spark = new SparkParticle(
                            emitPosition,
                            offset.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 4f),
                            false,
                            20,
                            0.7f,
                            Color.White
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    // 🌫️ 稀有烟雾点缀
                    if (Main.rand.NextBool(12))
                    {
                        Dust d = Dust.NewDustPerfect(
                            emitPosition,
                            DustID.Smoke,
                            offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f),
                            80,
                            new Color(80, 40, 40, 100),
                            Main.rand.NextFloat(1.1f, 1.6f)
                        );
                        d.noGravity = true;
                    }
                }

            }

            // === 1️⃣ 高速自转 ===
            Projectile.rotation += 0.3f; // 高速自转

            // === 2️⃣ 线性加速飞行 ===
            float maxSpeed = 10f;
            float acceleration = 0.018f;
            if (Projectile.velocity.Length() < maxSpeed)
            {
                Projectile.velocity *= 1f + acceleration;
            }




            // === 3️⃣ 角度限制自动追踪敌人 ===
            NPC target = null;
            float trackingAngleLimit = MathHelper.ToRadians(20f);
            float minDistance = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(Projectile))
                {
                    Vector2 toTarget = npc.Center - Projectile.Center;
                    float distance = toTarget.Length();
                    float angleToTarget = Math.Abs(
                        MathHelper.WrapAngle(
                            Projectile.velocity.SafeNormalize(Vector2.UnitY).ToRotation() -
                            toTarget.SafeNormalize(Vector2.UnitY).ToRotation()
                        )
                    );

                    if (angleToTarget <= trackingAngleLimit && distance < minDistance)
                    {
                        minDistance = distance;
                        target = npc;
                    }
                }
            }

            if (target != null)
            {
                // 慢速修正方向，确保不会瞬间拐弯
                float turnSpeed = 0.05f; // 追踪力度
                Vector2 desiredVelocity = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitY)) * Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, turnSpeed);
            }


            // 每帧增加 ai[x] 计数
            Projectile.ai[1]++;

            // 前x帧禁用方块碰撞（防止在平地上某些角度射不出来）
            if (Projectile.ai[1] < 5)
            {
                Projectile.tileCollide = false; // 禁用碰撞
            }
            else
            {
                Projectile.tileCollide = true; // 启用碰撞
            }



            // === 4️⃣ 持续甩出丰富水系特效 ===

            // 💦 水珠 Dust 拖尾
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    Main.rand.NextBool(3) ? DustID.GemSapphire : DustID.Water,
                    -Projectile.velocity * 0.2f,
                    80,
                    Color.White,
                    Main.rand.NextFloat(0.8f, 1.2f)
                );
                d.noGravity = true;
            }

            // ✨ 深蓝 SparkParticle 锐光拖尾
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 5f) + Projectile.velocity * 0.1f;
                Particle spark = new SparkParticle(
                    Projectile.Center,
                    sparkVel,
                    false,
                    20,
                    0.8f,
                    Color.DarkBlue
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 🌫️ 极少量深红/褐色水雾点缀
            if (Main.rand.NextBool(10))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Smoke,
                    -Projectile.velocity * 0.1f,
                    100,
                    new Color(80, 40, 40, 100),
                    Main.rand.NextFloat(1.0f, 1.4f)
                );
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            {
                Vector2 center = Projectile.Center;
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = -0.2f }, center);

                // 🌊 (A) 内层高速水珠爆破
                int coreDust = 35;
                for (int i = 0; i < coreDust; i++)
                {
                    float angle = MathHelper.TwoPi * i / coreDust + Main.rand.NextFloat(-0.05f, 0.05f);
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                    Dust d = Dust.NewDustPerfect(
                        center,
                        Main.rand.NextBool() ? DustID.Water : DustID.GemSapphire,
                        velocity,
                        60,
                        Color.White,
                        Main.rand.NextFloat(1.1f, 1.4f)
                    );
                    d.noGravity = true;
                }

                // 🌊 (B) 中层水花喷射（模拟飞行期间双螺旋水珠特效）
                int midDust = 28;
                for (int i = 0; i < midDust; i++)
                {
                    float angle = MathHelper.TwoPi * i / midDust + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    Dust d = Dust.NewDustPerfect(
                        center,
                        Main.rand.NextBool(4) ? DustID.Blood : DustID.Water, // 极少量深红点缀
                        velocity,
                        80,
                        Main.rand.NextBool(5) ? new Color(90, 40, 40) : Color.LightBlue, // 极少量褐色点缀
                        Main.rand.NextFloat(1.0f, 1.3f)
                    );
                    d.noGravity = true;
                }

                // 🌫️ (C) 外围缓慢漂浮的蓝色雾气（残留水雾）
                int outerDust = 20;
                for (int i = 0; i < outerDust; i++)
                {
                    float angle = MathHelper.TwoPi * i / outerDust + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                    Dust d = Dust.NewDustPerfect(
                        center,
                        DustID.Smoke,
                        velocity,
                        100,
                        Color.DarkSlateGray * 0.8f,
                        Main.rand.NextFloat(1.5f, 2.0f)
                    );
                    d.noGravity = true;
                }

                // ✨ (D) 深蓝 SparkParticle 锐闪
                int sparks = 25;
                for (int i = 0; i < sparks; i++)
                {
                    float angle = MathHelper.TwoPi * i / sparks + Main.rand.NextFloat(-0.05f, 0.05f);
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 16f);
                    Particle spark = new SparkParticle(
                        center,
                        velocity,
                        false,
                        30,
                        Main.rand.NextFloat(0.9f, 1.2f),
                        Color.DarkBlue
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 🫧 (E) 少量水泡 Gore 点缀
                int goreCount = 6;
                for (int i = 0; i < goreCount; i++)
                {
                    Vector2 goreVelocity = Main.rand.NextVector2Circular(3f, 3f);
                    Gore gore = Gore.NewGorePerfect(
                        Projectile.GetSource_Death(),
                        center,
                        goreVelocity,
                        Main.rand.NextBool() ? 411 : 412
                    );
                    gore.timeLeft = 12 + Main.rand.Next(8);
                    gore.scale = Main.rand.NextFloat(0.8f, 1.2f);
                }

                // 🌊 (F) 生成 RedtideJavEXP 爆炸延续区域
                Projectile.NewProjectile(
                    Projectile.GetSource_Death(),
                    center,
                    Vector2.Zero,
                    ModContent.ProjectileType<RedtideJavEXP>(),
                    (int)(Projectile.damage * 1.0f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            // 在当前弹幕的位置生成 RedtideJavEXP 弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_Death(),          // 弹幕生成来源
                Projectile.Center,                    // 弹幕生成的位置（当前弹幕的中心）
                Vector2.Zero,                         // 初始速度为零（原地爆炸效果）
                ModContent.ProjectileType<RedtideJavEXP>(), // RedtideJavEXP 的类型
                (int)(Projectile.damage * 1.0f),      // 伤害倍率为1倍
                Projectile.knockBack,                 // 使用当前弹幕的击退值
                Projectile.owner                      // 当前弹幕的所有者
            );
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 240);
        }

    }
}
