using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLanceDASH : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/TheLastLance/TheLastLancePROJ";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        private const int MaxChargeTime = 30; // 蓉力时间30帧
        private const float DashSpeed = 32.5f * 0.75f; // 冲刺速度为默认速度的0.75倍
        private const int MaxDashTime = 30; // 冲刺时间30帧（0.5秒）

        private Vector2 lockedDirection; // 存储锁定的冲刺方向
        private int dashTime = 0; // 冲刺已进行的时间
        private bool isDashing = false; // 标记是否处于冲刺状态
        private int Time = 0; // 用于控制粒子效果的计时器
        private bool isSuperDash = false; // 标记是否为超级冲刺
        public void SetSuperDash()
        {
            isSuperDash = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            if (isSuperDash)
            {
                Projectile.timeLeft = 60000; // 如果是超级冲刺状态，设置为6万
                //Projectile.damage *= 6; // 超级冲刺形态造成6倍伤害
            }
            else
            {
                Projectile.timeLeft = MaxChargeTime + 30; // 维持原本的时间
                //Projectile.damage *= 2; // 正常冲刺形态仅造成2倍伤害
            }
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Player owner = Main.player[Projectile.owner];
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            //// 强制保持高速旋转，模拟孙悟空耍金箍棒的操作
            //Projectile.rotation += 0.45f; // 高速旋转

            if (Projectile.velocity == Vector2.Zero) // 蓄力阶段
            {
                Projectile.tileCollide = false;
                Projectile.friendly = false;
                isDashing = false;

                // 强制保持高速旋转，模拟孙悟空耍金箍棒的操作
                Projectile.rotation += 0.45f; // 高速旋转
                //Projectile.rotation = Projectile.AngleTo(Main.MouseWorld);
                //if (Projectile.spriteDirection == -1)
                //    Projectile.rotation += MathHelper.PiOver2;
                //else
                //    Projectile.rotation += MathHelper.PiOver4;


                Projectile.Center = owner.MountedCenter;
                owner.heldProj = Projectile.whoAmI;

                if (Time % 3 == 0)
                {
                    Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                    particleOffset.X += Main.rand.NextFloat(-3f, 3f);
                    Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;
                    Particle Smear = new CircularSmearVFX(particlePosition, Color.CadetBlue * Main.rand.NextFloat(0.9f, 1.0f), Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(1.2f, 1.3f));
                    GeneralParticleHandler.SpawnParticle(Smear);

                    // 🚩🚩🚩 新增复杂 Dust + 气泡 Gore 圆环释放
                    {
                        Vector2 origin = Projectile.Center;
                        int points = 24; // 控制密度
                        float radius = 40f;
                        float timeFactor = Main.GameUpdateCount * 0.08f;
                        for (int i = 0; i < points; i++)
                        {
                            float angle = MathHelper.TwoPi * i / points + timeFactor;
                            Vector2 offset = angle.ToRotationVector2() * radius;
                            Vector2 spawnPos = origin + offset;

                            // === Dust ===
                            if (Main.rand.NextBool(2))
                            {
                                Vector2 dustVelocity = offset.SafeNormalize(Vector2.Zero).RotatedBy(Math.Sin(timeFactor + i) * 0.2f) * Main.rand.NextFloat(2f, 6f);
                                Dust dust = Dust.NewDustPerfect(
                                    spawnPos,
                                    DustID.Water,
                                    dustVelocity,
                                    0,
                                    Color.DarkSlateGray * 0.9f,
                                    Main.rand.NextFloat(1.1f, 1.6f)
                                );
                                dust.noGravity = true;
                            }

                            // === Gore 气泡 ===
                            if (Main.rand.NextBool(3))
                            {
                                Vector2 goreVelocity = offset.SafeNormalize(Vector2.Zero).RotatedBy(Math.Sin(timeFactor + i * 0.5f) * 0.3f) * Main.rand.NextFloat(3f, 7f);
                                Gore bubble = Gore.NewGorePerfect(
                                    Projectile.GetSource_FromAI(),
                                    spawnPos,
                                    goreVelocity,
                                    411
                                );
                                bubble.timeLeft = 10 + Main.rand.Next(8);
                                bubble.scale = Main.rand.NextFloat(0.7f, 1.2f);
                                bubble.type = Main.rand.NextBool(4) ? 412 : 411;
                            }
                        }
                    }
                }

                Time++;

                if (Projectile.ai[0] >= MaxChargeTime)
                {
                    StartLunge(owner); // 开始冲刺
                }
                else
                {
                    Projectile.ai[0]++;
                }
            }
            else // 冲刺阶段
            {
                // 在冲刺阶段将长枪的旋转方向固定为开始冲刺的方向
                //Projectile.rotation = lockedDirection.ToRotation() + MathHelper.PiOver4;


                // 不断的更新弹幕的旋转方向，让它永远对准鼠标还是调整
                Vector2 currentDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.rotation = currentDirection.ToRotation() + MathHelper.PiOver4;
                // 更新弹幕的 velocity 以使其持续朝向鼠标
                Projectile.velocity = currentDirection * Projectile.velocity.Length();


                if (isSuperDash)
                {
                    Projectile.tileCollide = false; // 超级冲刺状态下会穿墙
                }
                else
                {
                    Projectile.tileCollide = true; // 普通冲刺不会穿墙
                }

                Projectile.friendly = true;
                isDashing = true;

                //Projectile.velocity = lockedDirection * Projectile.velocity.Length();
                owner.velocity = Projectile.velocity;
                owner.Center = Projectile.Center;

                dashTime++;
                if (!isSuperDash && dashTime >= MaxDashTime) // 仅在不是超级冲刺时才执行减速和停止逻辑
                {
                    Projectile.velocity *= 0.35f;
                    return;
                }

                {
                    // === 🚩🚩🚩 TheLastLanceDASH 冲刺阶段飞行特效完全重制 ===

                    {
                        // === 1️⃣ 枪口位置生成深海重型烟雾（主视觉） ===
                        Vector2 gunMuzzle = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);
                        Particle muzzleSmoke = new HeavySmokeParticle(
                            gunMuzzle,
                            Vector2.Zero,
                            Color.DarkSlateGray,
                            25,
                            Main.rand.NextFloat(1.1f, 1.7f),
                            0.4f,
                            Main.rand.NextFloat(-0.5f, 0.5f),
                            required: true
                        );
                        GeneralParticleHandler.SpawnParticle(muzzleSmoke);

                        // === 2️⃣ 枪身周围深海灰蓝 Dust 环绕喷发 ===
                        int dustPoints = 12;
                        float radius = 18f;
                        for (int i = 0; i < dustPoints; i++)
                        {
                            float angle = MathHelper.TwoPi * i / dustPoints + Main.GameUpdateCount * 0.1f;
                            Vector2 offset = angle.ToRotationVector2() * radius;
                            Vector2 spawnPos = Projectile.Center + offset;
                            Vector2 velocity = offset.SafeNormalize(Vector2.Zero).RotatedBy(Math.Sin(Main.GameUpdateCount * 0.1f + i) * 0.2f) * Main.rand.NextFloat(1f, 4f);

                            Dust dust = Dust.NewDustPerfect(
                                spawnPos,
                                DustID.Water,
                                velocity,
                                0,
                                Color.DarkSlateGray * 0.9f,
                                Main.rand.NextFloat(1.0f, 1.4f)
                            );
                            dust.noGravity = true;
                        }

                        // === 3️⃣ 深海气泡 Gore 喷射（随机位置围绕枪体） ===
                        if (Main.rand.NextBool(2))
                        {
                            Vector2 goreOffset = Main.rand.NextVector2CircularEdge(Projectile.width * 0.5f, Projectile.height * 0.5f);
                            Vector2 goreSpawnPos = Projectile.Center + goreOffset;
                            Vector2 goreVelocity = goreOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 7f) + Projectile.velocity * 0.2f;

                            Gore bubble = Gore.NewGorePerfect(
                                Projectile.GetSource_FromAI(),
                                goreSpawnPos,
                                goreVelocity,
                                411
                            );
                            bubble.timeLeft = 10 + Main.rand.Next(8);
                            bubble.scale = Main.rand.NextFloat(0.8f, 1.2f);
                            bubble.type = Main.rand.NextBool(4) ? 412 : 411;
                        }

                        // === 4️⃣ 深海线性粒子拖尾（SparkParticle） ===
                        if (Main.rand.NextBool(3))
                        {
                            Vector2 sparkVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                            Particle spark = new SparkParticle(
                                Projectile.Center,
                                sparkVelocity,
                                false,
                                35,
                                0.8f,
                                Color.DarkBlue * 0.7f
                            );
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                    }

                }

                // 如果是超级冲刺且接触到液体，则销毁投射物
                if (isSuperDash && Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
                {
                    Projectile.Kill();
                }
            }
        }

        private void StartLunge(Player owner) // 冲刺的具体逻辑
        {
            owner = Main.player[Projectile.owner];

            dashTime = 0;
            lockedDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.velocity = lockedDirection * DashSpeed;

            // 检查玩家是否处于海洋群系，以便造成额外的两倍伤害
            if (owner != null && owner.ZoneBeach)
            {
                Projectile.damage = (int)(Projectile.damage * 2.0f); // 造成两倍伤害
            }

            // 如果是超级冲刺，设置冲刺时间和生存时间无限制
            if (isSuperDash)
            {
                dashTime = 0;
                Projectile.timeLeft = MaxChargeTime + 60000;
                Projectile.tileCollide = false;
            }

            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);
            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.DarkBlue, new Vector2(1f, 1f), Main.rand.NextFloat(6f, 10f), 0.15f, 3f, 10);
            GeneralParticleHandler.SpawnParticle(pulse);

            owner.immune = true;
            owner.immuneNoBlink = true;
            owner.immuneTime = 30;

            for (int i = 0; i < owner.hurtCooldowns.Length; i++)
            {
                owner.hurtCooldowns[i] = owner.immuneTime;
            }

            Projectile.netUpdate = true;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int freezeDuration = 180; // 冻结持续时间，单位为帧
            target.AddBuff(ModContent.BuffType<GlacialState>(), freezeDuration); // 冰河时代
            target.AddBuff(BuffID.Frostburn, freezeDuration); // 原版的霜火效果
            target.AddBuff(BuffID.Chilled, freezeDuration); // 原版的寒冷效果
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor) // 确保贴图的中心点为绘制的中心点
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
