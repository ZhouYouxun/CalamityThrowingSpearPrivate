using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Items.Weapons.Ranged;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd
{
    internal class ElectrocutionHalberdRIGHT : ModProjectile, ILocalizedModType
    {
        public override string Texture => "Terraria/Images/Projectile_254";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
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
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 150;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 20; // 无敌帧冷却时间为14帧
            Projectile.alpha = 1;
        }
        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation += 0.5f;

            // 控制闪电球动画
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 4)
                {
                    Projectile.frame = 0;
                }
            }

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Gray.ToVector3() * 0.55f);

            // 添加粒子特效
            GenerateParticles();
            ApplyTeslaCannonEffects();

            // 初始减速阶段
            if (Projectile.ai[1] <= 30)
            {
                Projectile.velocity *= 0.985f; // 每帧减速
                Projectile.ai[1]++;
            }
            else
            {
                // 开启追踪逻辑
                PerformTracking();
            }
        }

        private void GenerateParticles()
        {
            // 添加蓝紫色的光效
            Lighting.AddLight(Projectile.Center, 0.2f, 0.3f, 0.5f);

            // 双螺旋粒子特效
            float amplitude = 12f; // 振幅
            float speed = 0.15f;   // 旋转速度
            float time = Projectile.timeLeft * speed;

            for (int i = 0; i < 2; i++)
            {
                float direction = (i == 0) ? 1f : -1f;
                Vector2 offset = new Vector2(
                    (float)Math.Sin(time + i * MathHelper.Pi) * amplitude * direction,
                    (float)Math.Cos(time + i * MathHelper.Pi) * amplitude
                );

                int dustType = (i == 0) ? DustID.IceTorch : DustID.Electric;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, dustType, Vector2.Zero, 150, default, 1.2f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.1f, 1.6f);
            }

            // 扩散的 UltraBrightTorch 粒子
            for (int i = 0; i < 2; i++) // 每帧生成两个粒子
            {
                Vector2 randomOffset = Main.rand.NextVector2Circular(3f, 3f); // 随机偏移
                Dust ultraBrightDust = Dust.NewDustPerfect(Projectile.Center + randomOffset, DustID.UltraBrightTorch, Vector2.Zero, 150, default, 1.0f);
                ultraBrightDust.noGravity = true;
                ultraBrightDust.scale = Main.rand.NextFloat(1.0f, 1.3f);
            }
        }
        private void PerformTracking()
        {
            NPC target = Projectile.Center.ClosestNPCAt(2000); // 查找 X 范围内最近的敌人
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);

                // 固定追踪速度为 15f
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 15f, 0.08f);
            }
        }

        private void ApplyTeslaCannonEffects()
        {
            // 添加蓝绿色的光效
            Lighting.AddLight(Projectile.Center, 0f, 0.3f, 0.4f);

            // 生成蓝色 Dust
            for (int i = 0; i < 2; i++)
            {
                Vector2 position = Projectile.position - Projectile.velocity * (i * 0.25f);
                int dust = Dust.NewDust(position, 1, 1, DustID.Electric, 0f, 0f, 0, default, 1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = Main.rand.NextFloat(0.7f, 1.1f);
            }

            // 额外粒子特效
            if (Main.rand.NextBool(6))
            {
                Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, 226, 0f, 0f);
            }

            // 双螺旋粒子效果
            CreateDoubleSpiralDust();
        }

        private void CreateDoubleSpiralDust()
        {
            float amplitude = 16f; // 振幅
            float speed = 0.1f;    // 旋转速度
            float time = Projectile.timeLeft * speed;

            for (int i = 0; i < 2; i++)
            {
                float direction = (i == 0) ? 1f : -1f;
                Vector2 offset = new Vector2(
                    (float)Math.Sin(time + i * MathHelper.Pi) * amplitude * direction,
                    (float)Math.Cos(time + i * MathHelper.Pi) * amplitude
                );

                int dustType = i == 0 ? 146 : 50; // 蓝紫交替
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, dustType, Vector2.Zero, 180, default, 1.2f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.1f, 1.6f);
            }
        }



        // ---------------------------------------------------------------------------------------------------------
        private void PerformDazzlingStabberChase(NPC target)
        {
            // 如果当前攻击延迟未结束，减少延迟并跳过追踪逻辑
            if (Projectile.ai[0] > -60f)
            {
                Projectile.ai[0]--;
                if (Projectile.ai[0] > 0f)
                    return;
            }

            // 调整速度确保不会卡住
            if (Projectile.velocity.Length() < 3f)
                Projectile.velocity = Vector2.UnitY * -12f;

            // 如果接近目标，减速
            if (Projectile.WithinRange(target.Center, 90f))
                Projectile.velocity *= 0.93f;
            else if (Projectile.velocity.Length() < 40f)
                Projectile.velocity *= 1.03f; // 否则加速

            // 角度调整
            float turnSpeed = 0.35f;
            float angleToTarget = Projectile.AngleTo(target.Center);
            if (!Projectile.WithinRange(target.Center, 200f))
            {
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(angleToTarget, turnSpeed).ToRotationVector2() * Projectile.velocity.Length();
            }

            // 冲刺逻辑
            if (!Projectile.WithinRange(target.Center, 75f) && Vector2.Dot(Projectile.SafeDirectionTo(target.Center), Projectile.velocity.SafeNormalize(Vector2.Zero)) > 0.85f)
            {
                Projectile.velocity = Projectile.SafeDirectionTo(target.Center) * 36f;
                Projectile.ai[0] = 15f; // 设置冷却时间
                Projectile.netUpdate = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        private void ReturnToRestingPosition()
        {
            // 恢复到初始位置的逻辑
            Projectile.rotation = Projectile.rotation.AngleTowards(Projectile.ai[1], 0.25f);
            Vector2 destination = Owner.Center + Vector2.UnitY.RotatedBy(Projectile.ai[1]) * -120f;
            Projectile.velocity = (destination - Projectile.Center) / 10f;
        }
        // ---------------------------------------------------------------------------------------------------------



        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果

            // 在击中目标时生成光环特效
            // 随机生成 15~35 个粒子
            int particleCount = Main.rand.Next(15, 36); // 随机粒子数量
            for (int i = 0; i < particleCount; i++)
            {
                // 轮流选择红色或浅灰色
                Color sparkColor = (i % 2 == 0) ? Color.Red : Color.LightGray;

                // 设置粒子的速度，方向有一定随机偏移
                Vector2 sparkSpeed = Main.rand.NextVector2Circular(8f, 17f);

                // 创建粒子特效
                Particle spark = new CritSpark(
                    target.Center,                            // 粒子起始位置
                    sparkSpeed,                               // 粒子速度
                    Color.White,                              // 外层颜色
                    sparkColor,                               // 内层颜色
                    0.7f + Main.rand.NextFloat(0f, 0.6f),     // 粒子大小
                    30,                                       // 粒子存活时间
                    0.4f,                                     // 起始透明度
                    0.6f                                      // 结束透明度
                );

                // 生成粒子
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }


        public override void OnKill(int timeLeft)
        {
            // 检查是否存在Lighting弹幕
            bool lightningExists = Main.projectile.Any(proj => proj.active && proj.type == ModContent.ProjectileType<ElectrocutionHalberdLIGHTING>());
            SoundEngine.PlaySound(SoundID.Item92, Projectile.position);

            if (!lightningExists)
            {
                // 召唤一条闪电
                for (int i = 0; i < 1; i++)
                {
                    Vector2 lightningSpawnPosition = Projectile.Center - Vector2.UnitY * 880f; // 正上方 X 像素
                    Vector2 lightningShootVelocity = (Projectile.Center - lightningSpawnPosition).SafeNormalize(Vector2.UnitY) * 14f;

                    int lightning = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        lightningSpawnPosition,
                        lightningShootVelocity,
                        ModContent.ProjectileType<ElectrocutionHalberdLIGHTING>(),
                        (int)(Projectile.damage * 0.5), // 伤害倍率为1.0倍
                        0f,
                        Projectile.owner
                    );

                    if (Main.projectile.IndexInRange(lightning))
                    {
                        Main.projectile[lightning].ai[0] = lightningShootVelocity.ToRotation();
                        Main.projectile[lightning].ai[1] = Main.rand.Next(100);
                    }
                }
            }


            // 添加闪电特效
            CreateLightningEffect(Projectile.Center);
        }
        private void CreateLightningEffect(Vector2 center)
        {
            int lineCount = 6; // 总共射出的线条数
            float angleStep = MathHelper.TwoPi / lineCount; // 每两条线之间的夹角

            // 生成六条线
            for (int i = 0; i < lineCount; i++)
            {
                float angle = i * angleStep; // 计算当前线的角度
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)); // 根据角度计算方向向量

                // 创建线条
                CreateLine(center, 0f, 5 * 16, direction); // 从中心点向外延伸 5 * 16 像素
            }
        }

        private void CreateLine(Vector2 startPoint, float minOffset, float maxOffset, Vector2 direction)
        {
            int dustCount = 15; // 每条线的粒子数量
            float segmentLength = (maxOffset - minOffset) / dustCount;

            for (int i = 0; i <= dustCount; i++)
            {
                float offset = MathHelper.Lerp(minOffset, maxOffset, i / (float)dustCount);
                Vector2 position = startPoint + direction * offset;

                // 创建 Dust
                Dust dust = Dust.NewDustPerfect(position, DustID.Electric, Vector2.Zero, 150, default, Main.rand.NextFloat(1.2f, 1.8f));
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
        }

    }
}
