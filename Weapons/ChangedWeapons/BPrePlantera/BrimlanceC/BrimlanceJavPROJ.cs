using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.BrimlanceC
{
    public class BrimlanceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/BPrePlantera/BrimlanceC/BrimlanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";

        private bool hasBounced = false; // 记录是否已经击中过敌人

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
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        // 添加字段
        private float rotationTotal = 0f; // 累积旋转量
        private int rotationDir = 0;      // 旋转方向：+1 顺，-1 逆

        public override void AI()
        {
            // 保持旋转（基础旋转角度）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加橙色光
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // === DNA 粒子（红色） ===
            float frequency = 30f;
            float amplitude = 20f;
            Vector2 leftOffset = new Vector2(-amplitude * (float)Math.Sin(Projectile.ai[0] * MathHelper.TwoPi / frequency), 0);
            Vector2 rightOffset = new Vector2(amplitude * (float)Math.Sin(Projectile.ai[0] * MathHelper.TwoPi / frequency), 0);

            if (Projectile.ai[0] % 2 == 0)
            {
                Dust.NewDustPerfect(Projectile.Center + leftOffset, DustID.RedTorch, Vector2.Zero, 0, Color.Red, 1.2f).noGravity = true;
                Dust.NewDustPerfect(Projectile.Center + rightOffset, DustID.RedTorch, Vector2.Zero, 0, Color.Red, 1.2f).noGravity = true;
            }
            Projectile.ai[0] += 2f;

            // === 尾迹火花 ===
            if (Main.rand.NextBool(5))
            {
                Particle trail = new SparkParticle(Projectile.Center, Projectile.velocity * 0.2f, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.OrangeRed);
                GeneralParticleHandler.SpawnParticle(trail);
            }

            // 前5帧禁用地形碰撞
            Projectile.ai[1]++;
            Projectile.tileCollide = Projectile.ai[1] >= 5;

            // === 减速旋转阶段 ===
            if (Projectile.ai[2] == 1f)
            {
                float progress = Projectile.localAI[0] / 60f; // 进度 0 → 1
                float eased = (float)(0.5f - 0.5f * Math.Cos(progress * Math.PI)); // cos 曲线缓动

                // ✳️ 完整旋转 360°，带 easing
                float angle = MathHelper.TwoPi * eased;
                float rotationTotal = angle * rotationDir;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + rotationTotal;

                // ✳️ 柔和减速（每帧 ×0.995）
                Projectile.velocity *= 0.995f;

                // === 第5帧之后才触发追踪 ===
                if (Projectile.localAI[0] > 5f)
                {
                    int targetIndex = Projectile.FindTargetWithLineOfSight(1800f); // 合理点，范围不至于全屏
                    if (targetIndex != -1)
                    {
                        NPC target = Main.npc[targetIndex];
                        Vector2 toTarget = target.Center - Projectile.Center;

                        // 🌀 中强度追踪
                        float homingStrength = 0.52f; // 介于0.12f和0.35f之间
                        Vector2 desiredVel = toTarget.SafeNormalize(Vector2.UnitY) * Projectile.velocity.Length();

                        Projectile.velocity = Vector2.Lerp(
                            Projectile.velocity,
                            desiredVel,
                            homingStrength
                        );
                    }
                }


                // 🔥 原有粒子逻辑不动
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Torch, Vector2.Zero, 0, Color.OrangeRed, 1.5f);
                    dust.noGravity = true;
                }

                // 雨刮式喷射……
                float sprayCenterRot = Projectile.rotation - MathHelper.PiOver4;
                float sweepAmplitude = MathHelper.ToRadians(25f);
                float sweepSpeed = 0.15f;
                float sweepOffset = (float)Math.Sin(Projectile.localAI[0] * sweepSpeed) * sweepAmplitude;

                Vector2 sprayDir = (sprayCenterRot + sweepOffset).ToRotationVector2() * 6f;
                Vector2 sprayPos = Projectile.Center + sprayDir.SafeNormalize(Vector2.UnitX) * 16f;

                Dust d = Dust.NewDustPerfect(sprayPos, DustID.RedTorch, sprayDir * 0.3f, 0, Color.Red, 1.4f);
                d.noGravity = true;

                Particle spark = new SparkParticle(
                    sprayPos,
                    sprayDir * 2.4f,
                    false,
                    20,
                    Main.rand.NextFloat(0.9f, 1.3f),
                    Color.OrangeRed
                );
                GeneralParticleHandler.SpawnParticle(spark);

                Projectile.localAI[0]++;

                // 旋转结束 → 生成火圈
                if (Projectile.localAI[0] >= 60f)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<BrimlanceJavFireWall>(),
                        (int)(Projectile.damage * 0.75),
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    Projectile.Kill();
                }
                return;
            }


            // 🚀 初始飞行阶段：持续加速
            Projectile.velocity *= 1.01f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300); // 上硫磺火

            // 仅在首次命中时进入旋转阶段
            if (Projectile.ai[2] == 0f)
            {
                Projectile.ai[2] = 1f;
                Projectile.localAI[0] = 0f;
                Projectile.friendly = false;

                // ✅ 命中后立刻反向
                Projectile.velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * 12f;

                // ✅ 根据玩家位置决定旋转方向
                Player owner = Main.player[Projectile.owner];
                rotationDir = (owner.Center.X < Projectile.Center.X) ? 1 : -1;
            }
        }


        public override void OnKill(int timeLeft)
        {
            // 视觉爆炸特效弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<FuckYou>(),
                (int)(Projectile.damage * 0f), // 伤害为 0，仅视觉用途
                Projectile.knockBack,
                Projectile.owner
            );

            // 火墙：双保险，若前面没触发，这里确保生成
            if (Projectile.ai[2] == 0f)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<BrimlanceJavFireWall>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }


        // 从天上降下BrimlanceStandingFire弹幕
        private void SummonBrimlanceFire(NPC npc)
        {
            Player player = Main.player[npc.target];
            Vector2 targetPosition = npc.Center;
            float radius = 50f * 16f;
            float arrowSpeed = 10f;

            float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 spawnPosition = targetPosition + radius * new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

            Vector2 direction = targetPosition - spawnPosition;
            direction.Normalize();
            float speedX = direction.X * arrowSpeed + Main.rand.Next(-120, 121) * 0.01f;
            float speedY = direction.Y * arrowSpeed + Main.rand.Next(-120, 121) * 0.01f;

            int newDamage = (int)(Projectile.damage * 0.5f); // 伤害为本体的50%
            Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPosition, new Vector2(speedX, speedY), ModContent.ProjectileType<BrimlanceStandingFire>(), newDamage, 0, player.whoAmI);
        }



    }
}
