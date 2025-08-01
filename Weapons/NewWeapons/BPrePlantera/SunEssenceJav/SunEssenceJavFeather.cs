using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    public class SunEssenceJavFeather : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取羽毛贴图
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;
            SpriteEffects effects = SpriteEffects.None;

            // === 🚩 1️⃣ 绘制拖尾（原 Calamity 拖尾）===
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, (int)1.5f);

            // === 🚩 2️⃣ 外层光脉动绘制 ===
            // 计算呼吸脉动缩放（1.6x ~ 2.0x）
            float pulsate = 1.6f + 0.4f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f); // 呼吸周期

            // 设置蓝绿色淡光色
            Color glowColor = Color.Cyan * 0.3f;

            // 多层叠加轻微位移绘制，形成柔和蓝光晕
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f;
                Main.spriteBatch.Draw(
                    texture,
                    drawPosition + offset,
                    null,
                    glowColor,
                    rotation,
                    origin,
                    pulsate,
                    effects,
                    0f
                );
            }

            // === 🚩 3️⃣ 绘制本体（1.5x 大小）===
            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                null,
                lightColor,
                rotation,
                origin,
                1.5f,
                effects,
                0f
            );

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 10;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = ProjAIStyleID.Nail;
            AIType = ProjectileID.NailFriendly;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.PiOver4;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);



            // 若仍在飞行状态（未碰地形），保留所有特效
            if (Projectile.tileCollide)
            {
                // === 🚩 1️⃣ 高密度 Dust 蓝绿色流动 ===
                int dustCount = 2;
                for (int i = 0; i < dustCount; i++)
                {
                    float spread = MathHelper.ToRadians(45f);
                    float angle = Main.rand.NextFloat(-spread, spread);
                    Vector2 velocity = forward.RotatedBy(angle) * Main.rand.NextFloat(4f, 10f);
                    int type = Main.rand.NextFloat() < 0.7f ? DustID.BlueTorch : DustID.GreenTorch;

                    Dust d = Dust.NewDustDirect(
                        Projectile.Center,
                        0, 0,
                        type,
                        velocity.X,
                        velocity.Y,
                        0,
                        default, // 使用默认色以匹配 Dust 自身蓝绿
                        Main.rand.NextFloat(1.2f, 2.5f)
                    );
                    d.noGravity = true;
                }

                // === 🚩 2️⃣ DirectionalPulseRing 蓝绿色脉冲波 ===
                if (Main.GameUpdateCount % 3 == 0)
                {
                    int pulseLayers = 1;
                    for (int i = 0; i < pulseLayers; i++)
                    {
                        Particle pulse = new DirectionalPulseRing(
                            Projectile.Center + forward * (10f + i * 6f),
                            forward * (2f + i * 0.8f),
                            Color.Lerp(Color.DeepSkyBlue, Color.Cyan, i / (float)pulseLayers),
                            new Vector2(0.8f, 1.8f + i * 0.3f),
                            Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver4,
                            0.15f + i * 0.03f,
                            0.025f,
                            8
                        );
                        GeneralParticleHandler.SpawnParticle(pulse);
                    }
                }

                // === 🚩 3️⃣ SparkParticle（偏外侧，随机发散）===
                if (Main.rand.NextBool(6))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(10f, 10f); // ⬅️ 比原来更远
                    Vector2 velocity = forward.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(4f, 10f); // 速度范围拉大

                    Particle spark = new SparkParticle(
                        Projectile.Center + offset,
                        velocity,
                        false,
                        10,
                        Main.rand.NextFloat(0.8f, 1.0f),
                        Color.Lerp(Color.Aqua, Color.Cyan, Main.rand.NextFloat(0.3f, 0.7f))
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // === 🚩 4️⃣ AltSparkParticle（更靠近，集中）===
                if (Main.rand.NextBool(5))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(2f, 2f); // ⬅️ 比原来更小，贴近轨迹
                    Vector2 velocity = forward * 0.02f + Main.rand.NextVector2Circular(0.1f, 0.1f); // 更低速、轻扰动

                    AltSparkParticle altSpark = new AltSparkParticle(
                        Projectile.Center + offset,
                        velocity,
                        false,
                        16,
                        1.2f,
                        Color.Cyan * 0.25f
                    );
                    GeneralParticleHandler.SpawnParticle(altSpark);
                }

            }


        }


        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            for (int i = 0; i < 15; i++)
            {
                int greenDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.2f);
                Main.dust[greenDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[greenDust].scale = 0.5f;
                    Main.dust[greenDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 30; j++)
            {
                int greenDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.7f);
                Main.dust[greenDust2].noGravity = true;
                Main.dust[greenDust2].velocity *= 5f;
                greenDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1f);
                Main.dust[greenDust2].velocity *= 2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300); // 原版的破晓效果
        }

    }
}
