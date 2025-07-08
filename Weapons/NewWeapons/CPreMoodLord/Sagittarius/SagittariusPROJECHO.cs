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
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.Audio;
using CalamityRangerExpansion.LightingBolts;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius
{
    public class SagittariusPROJECHO : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Sagittarius/Sagittarius";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public ref float Time => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制拖尾
            // CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            // 包裹自身的亮黄色光晕效果
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 设置透明度
            float alpha = isAttached ? 0f : 0.6f; // 如果 isAttached 为 true，则完全透明，否则为不透明
            Color wrapColor = Color.LightGoldenrodYellow * alpha; // 设置透明度为 alpha 值

            // 绘制光晕效果
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 3f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, wrapColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 允许1次伤害
            Projectile.timeLeft = 960;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 能够穿透方块
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为25帧
            Projectile.alpha = 235;
            //// 设置充能长枪的伤害为原始伤害的5倍
            //Projectile.damage *= 5;
        }
        private bool isAttached = false; // 标记是否进入粘附状态
        private bool hasTriggeredBackSparkEffect = false; // 标记是否已触发反方向特效

        public override void AI()
        {

            if (!isAttached)// 蓄力阶段
            {

                // 在 AI 中蓄力阶段循环调用
                if (!isAttached && Main.rand.NextBool(3))
                {
                    CTSLightingBoltsSystem.Spawn_SagittariusEchoCharging(Projectile.Center);
                }

                if (Main.GameUpdateCount % 12 == 0)
                {
                    int dustCount = 12;
                    float radius = 32f;
                    for (int i = 0; i < dustCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / dustCount + Main.GameUpdateCount * 0.05f;
                        Vector2 offset = angle.ToRotationVector2() * radius;

                        Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, 267, offset.SafeNormalize(Vector2.Zero) * 0.5f, 0, Color.White, 1.2f);
                        dust.noGravity = true;
                    }
                }



                // 减速并逐渐增大和旋转
                Projectile.velocity *= 0.98f;
                Projectile.rotation += 0.075f * Projectile.scale;
                Projectile.scale += 0.0035f;
                // 持续生成向中心吸引的亮黄色闪光粒子
                if (Main.rand.NextBool(2))
                {
                    Vector2 sparkleVelocity = (Projectile.Center - Main.rand.NextVector2Circular(80f, 80f)).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.5f, 1.5f);
                    Color startColor = Color.Gold * 0.4f;
                    Color endColor = Color.LightGoldenrodYellow * 0.8f;
                    SparkleParticle spark = new SparkleParticle(Projectile.Center + sparkleVelocity * 10f, -sparkleVelocity, startColor, endColor, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(6, 12), Main.rand.NextFloat(-8, 8), 0.15f, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // 亮黄色冲击波效果
                if (Projectile.timeLeft % 20 == 0)
                {
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Yellow, new Vector2(1.5f), Projectile.rotation, 1f, 0.1f, 30);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }
            else // 粘附状态
            {
                Projectile.velocity = Vector2.Zero; // 速度归零
            }


            // 2秒后锁定最近敌人并释放特效
            if (Projectile.ai[0] > 210)
            {
                NPC target = FindClosestNPC(3000);
                if (target != null)
                {
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 24f;

                    // 仅触发一次反方向生成的扇形闪光粒子特效
                    if (!hasTriggeredBackSparkEffect)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            float angleOffset = MathHelper.ToRadians(15) * (i % 2 == 0 ? 1 : -1);
                            Vector2 particleVelocity = Projectile.velocity.RotatedBy(MathHelper.Pi + angleOffset) * Main.rand.NextFloat(1.5f, 3f);
                            Color startColor = Color.Gold * 0.6f;
                            Color endColor = Color.LightGoldenrodYellow * 0.3f;
                            SparkleParticle backSpark = new SparkleParticle(Projectile.Center, particleVelocity, startColor, endColor, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(10, 20), Main.rand.NextFloat(-8, 8), 0.15f, false);
                            GeneralParticleHandler.SpawnParticle(backSpark);
                        }

                        hasTriggeredBackSparkEffect = true; // 标记为已触发
                    }
                }
            }

            Projectile.ai[0]++;
            Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 0.55f);
            Time++;
        }

        public override bool? CanDamage() => Time >= 100f; // 初始的时候不会造成伤害，直到100为止

        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float minDistance = maxDetectDistance;

            foreach (NPC npc in Main.npc)
            {
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < minDistance && npc.CanBeChasedBy(this))
                {
                    minDistance = distance;
                    closestNPC = npc;
                }
            }

            return closestNPC;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 如果进入了粘附状态，每次攻击都会召唤两把分裂长枪
            if (isAttached)
            {
                SoundEngine.PlaySound(SoundID.Item108, Projectile.Center);

                // 根据是否启用 Main.zenithWorld 决定召唤数量
                int splitCount = Main.zenithWorld ? 10 : 3;

                for (int i = 0; i < splitCount; i++)
                {
                    float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                    Vector2 spawnPosition = target.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 70f * 16f;

                    Vector2 velocitySPIT = Vector2.Normalize(target.Center - spawnPosition) * 16;

                    // 生成分裂长枪，伤害为充能长枪的1/5
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocitySPIT, ModContent.ProjectileType<SagittariusSPIT>(), Projectile.damage / 13, Projectile.knockBack, Projectile.owner);
                }


                {
                    Vector2 sparkleVelocity = (Projectile.Center - Main.rand.NextVector2Circular(40f, 40f)) // 缩小随机偏移范围
                   .SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.25f, 0.75f); // 降低初始速度范围

                    Color startColor = Color.Gold * 0.4f;
                    Color endColor = Color.LightGoldenrodYellow * 0.8f;

                    // 减小粒子的大小和寿命，使吸收效果更轻微
                    SparkleParticle spark = new SparkleParticle(
                        Projectile.Center + sparkleVelocity * 5f, // 调整生成位置偏移
                        -sparkleVelocity,
                        startColor,
                        endColor,
                        Main.rand.NextFloat(0.15f, 0.3f), // 更小的粒子尺寸
                        Main.rand.Next(4, 8), // 更短的粒子寿命
                        Main.rand.NextFloat(-8, 8),
                        0.1f, // 调整消失速度
                        false
                    );

                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            else // 仅在第一次击中时触发特效
            {
                // 标记为进入粘附状态，确保后续不会再次触发
                isAttached = true;

                // 添加Debuff
                target.AddBuff(ModContent.BuffType<SagittariusEDebuff>(), 1200);
                // 生成20个旋转着逐渐消失的粒子效果
                for (int i = 0; i < 20; i++)
                {
                    // 随机生成偏移量
                    Vector2 sparkOffset = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(0.5f, 3f); // 扩大随机范围

                    // 设定颜色，透明度降低到原来的 25%
                    Color startColor = new Color(Color.LightGoldenrodYellow.R, Color.LightGoldenrodYellow.G, Color.LightGoldenrodYellow.B, (int)(Color.LightGoldenrodYellow.A * 0.25f));
                    Color endColor = new Color(Color.LightYellow.R, Color.LightYellow.G, Color.LightYellow.B, (int)(Color.LightYellow.A * 0.25f));

                    // 创建并生成粒子
                    GenericSparkle sparker = new GenericSparkle(
                        Projectile.Center + sparkOffset,
                        Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(0.5f, 2f), // 随机初始速度
                        startColor,
                        endColor,
                        Main.rand.NextFloat(2.5f, 2.9f), // 尺寸
                        14, // 粒子寿命
                        Main.rand.NextFloat(-0.05f, 0.05f), // 旋转速度
                        2.5f // 消失时间
                    );
                    GeneralParticleHandler.SpawnParticle(sparker);
                }

                // 扇形粒子效果（前后两侧）
                int particleCount = 30;
                float spreadAngle = MathHelper.ToRadians(5);
                Color particleColor = Color.Purple;

                // 前方扇形
                for (int i = 0; i < particleCount; i++)
                {
                    float angleOffset = Main.rand.NextFloat(-spreadAngle, spreadAngle);
                    Vector2 velocity = Projectile.velocity.RotatedBy(angleOffset) * Main.rand.NextFloat(2f, 4f);
                    Dust particle = Dust.NewDustPerfect(Projectile.Center, 173, velocity, 0, particleColor, 1.5f);
                    particle.noGravity = true;
                }

                // 后方扇形
                for (int i = 0; i < particleCount; i++)
                {
                    float angleOffset = MathHelper.Pi + Main.rand.NextFloat(-spreadAngle, spreadAngle);
                    Vector2 velocity = Projectile.velocity.RotatedBy(angleOffset) * Main.rand.NextFloat(2f, 4f);
                    Dust particle = Dust.NewDustPerfect(Projectile.Center, 173, velocity, 0, particleColor, 1.5f);
                    particle.noGravity = true;
                }

                // 亮黄色闪光粒子效果
                for (int i = 0; i < 20; i++)
                {
                    Vector2 sparkleVelocity = Main.rand.NextVector2Circular(4f, 4f);
                    Color startColor = Color.GhostWhite * 0.6f;
                    Color endColor = Color.LightYellow * 0.3f;

                    SparkleParticle spark = new SparkleParticle(target.Center, sparkleVelocity, startColor, endColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(10, 20), Main.rand.NextFloat(-8, 8), 0.2f, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }


        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 如果未进入粘附状态，则设置为粘附状态
            if (!isAttached)
            {
                isAttached = true; // 打开开关，标记为已粘附
                // 屏幕震动效果
                float shakePower = 1f; // 设置震动强度
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                // 保持原来的旋转角度
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                // 停止弹幕移动，模拟粘附在目标上的效果
                Projectile.velocity = Vector2.Zero;
            }
        }
        public override void OnKill(int timeLeft)
        {
            // 在死亡时触发屏幕震动
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = 10f;

            // 在弹幕消失时释放大量烟雾粒子
            int smokeCount = 150; // 数量为原有逻辑的两倍
            for (int i = 0; i < smokeCount; i++)
            {
                // 随机生成速度
                Vector2 dustVelocity = Main.rand.NextVector2Circular(3f, 3f) * Main.rand.NextFloat(4.5f, 7f); // 提高速度范围
                Color smokeColor = new Color(255, 200, 100); // 固定为浅橙色

                // 创建烟雾粒子
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    dustVelocity,
                    smokeColor,
                    18, // 粒子寿命
                    Main.rand.NextFloat(1.2f, 2.2f), // 粒子大小
                    0.4f, // 粒子淡出速度
                    Main.rand.NextFloat(-1, 1), // 随机旋转速度
                    true // 确保粒子启用特殊效果
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
    }
}