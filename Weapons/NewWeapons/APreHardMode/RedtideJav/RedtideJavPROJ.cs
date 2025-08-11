using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Buffs.DamageOverTime;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav
{
    public class RedtideJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/RedtideJav/RedtideJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 9; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转 (在这基础上再增加一点角度，为了适配这个特殊的贴图)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.ToRadians(15);

            // 添加红色光源
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.003f;



            {
                // 每隔一段时间生成水泡特效
                if (Main.rand.NextBool(5)) // 每5帧有20%概率生成一次水泡
                {
                    Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                    bubble.timeLeft = 8 + Main.rand.Next(6);
                    bubble.scale = Main.rand.NextFloat(0.6f, 1f) * (1 + Projectile.timeLeft / (float)Projectile.timeLeft);
                    bubble.type = Main.rand.NextBool(3) ? 412 : 411;
                }

                // 🌊 双螺旋水珠 Dust（飞行特效）
                float spiralRadius = 10f;
                float spiralSpeed = 0.15f;
                float time = Main.GameUpdateCount * spiralSpeed;

                for (int s = 0; s < 12; s++)
                {
                    float spiralOffset = s * MathHelper.Pi;
                    float angle = time + spiralOffset;
                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;

                    if (Main.rand.NextBool(2))
                    {
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            Main.rand.NextBool() ? DustID.Water : DustID.GemSapphire,
                            -Projectile.velocity * 0.1f + offset.SafeNormalize(Vector2.Zero) * 0.5f,
                            100,
                            Color.White,
                            1.1f
                        );
                        d.noGravity = true;
                    }
                }
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