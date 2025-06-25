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
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;
using CalamityMod.Sounds;
using CalamityRangerExpansion.LightingBolts;
using Terraria.GameContent.Drawing;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.BansheeHookC
{
    public class BansheeHookJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/BansheeHookC/BansheeHookJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
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
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 不允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        private float bansheeAuraRotation = 0f;

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加粉红色光效，强度保持不变
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.5f, 0.8f) * 0.55f);


            //// 定义一个偏移距离，用来增加粒子之间的间隔
            //float offsetDistance = 20f;

            //// 计算特效生成位置，始终在弹幕的正左方和正右方
            //Vector2 leftTrailPos = Projectile.Center - new Vector2(offsetDistance, 0);
            //Vector2 rightTrailPos = Projectile.Center + new Vector2(offsetDistance, 0);

            //// 生成粒子特效
            //Particle pinkTrail = new SparkParticle(leftTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, Color.Pink);
            //Particle blueTrail = new SparkParticle(rightTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, Color.LightBlue);
            //GeneralParticleHandler.SpawnParticle(pinkTrail);
            //GeneralParticleHandler.SpawnParticle(blueTrail);

            //// 特效部分
            //{
            //    // 定义一个偏移距离，用来增加粒子之间的间隔
            //    float offsetDistance = 20f;

            //    // 计算特效生成位置，始终在弹幕的正左方和正右方（基于弹幕当前方向）
            //    Vector2 leftOffset = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * offsetDistance;
            //    Vector2 rightOffset = Projectile.velocity.RotatedBy(-MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * offsetDistance;

            //    Vector2 leftTrailPos = Projectile.Center + leftOffset;
            //    Vector2 rightTrailPos = Projectile.Center + rightOffset;

            //    // 定义更为明显的粒子颜色
            //    Color brightPink = new Color(255, 105, 180); // 非常粉红的粉红
            //    Color deepPurple = new Color(75, 0, 130); // 非常深的紫色

            //    // 生成粉红色和深紫色粒子特效
            //    Particle pinkTrail = new SparkParticle(leftTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, brightPink);
            //    Particle purpleTrail = new SparkParticle(rightTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, deepPurple);
            //    GeneralParticleHandler.SpawnParticle(pinkTrail);
            //    GeneralParticleHandler.SpawnParticle(purpleTrail);

            //}


            // 让光点旋转，每帧增加一定角度
            bansheeAuraRotation += 0.1f; // 控制旋转速度，可调整

            // 在当前位置生成旋转光点
            CTSLightingBoltsSystem.Spawn_BansheeSoulOrbs(Projectile.Center, bansheeAuraRotation);







            // 弹幕加速
            Projectile.velocity *= 1.003f;

            // 每10帧留下一个BansheeHookScythe，伤害倍率为0.9
            if (Projectile.timeLeft % 15 == 0)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                         Projectile.Center,
                                         Vector2.Zero,
                                         ModContent.ProjectileType<BansheeHookScythe>(),
                                         (int)(Projectile.damage * 0.85f),
                                         Projectile.knockBack,
                                         Projectile.owner);
            }

            // 如果已经击中敌人一次，则开始追踪
            if (Projectile.ai[0] > 0)
            {
                // 前60帧不追踪
                if (Projectile.ai[1] > 60)
                {
                    NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 24f, 0.08f); // 追踪速度为12f
                    }
                }
                else
                {
                    // 增加计数器，直到达到30帧再开始追踪
                    Projectile.ai[1]++;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 center = Projectile.Center;

                if (Projectile.ai[1] == 0)
                {
                    {
                        // 💥 第一次命中：粉蓝穿刺，附带粒子特效
                        // 核心特效：TrueExcalibur（黄色剑气），粉蓝染色Dust，自定义粒子
                        for (int i = 0; i < 2; i++)
                        {
                            ParticleOrchestrator.RequestParticleSpawn(
                                false,
                                ParticleOrchestraType.TrueExcalibur,
                                new ParticleOrchestraSettings
                                {
                                    PositionInWorld = center + Main.rand.NextVector2Circular(8f, 8f)
                                }
                            );
                        }

                        // 添加 Dust：蓝粉相间
                        for (int i = 0; i < 16; i++)
                        {
                            int dustID = Main.rand.NextBool() ? DustID.PinkTorch : DustID.BlueTorch;
                            Dust dust = Dust.NewDustPerfect(center, dustID, Main.rand.NextVector2Circular(4f, 4f));
                            dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
                            dust.velocity *= 1.2f;
                            dust.noGravity = true;
                        }

                        // 添加自定义穿刺特效
                        for (int i = 0; i < 2; i++)
                        {
                            Particle slash = new DirectionalPulseRing(
                                center,
                                Projectile.velocity * 0.5f,
                                new Color(255, 100, 180), // 粉红
                                new Vector2(1.1f, 2.5f),
                                Projectile.rotation,
                                0.1f,
                                0.02f,
                                24
                            );
                            GeneralParticleHandler.SpawnParticle(slash);
                        }
                    }
                  

                    float randomAngle = MathHelper.ToRadians(Main.rand.Next(-60, 61)); // 随机角度在-60到60度之间
                    Projectile.velocity = Projectile.velocity.RotatedBy(randomAngle);  // 调整弹幕方向


                    Projectile.ai[1] = 1; // 开始追踪的前置标记
                }
                else
                {
                    {
                        // 🌈 1. 高密度粒子剑气：十字星风暴 + 多个发散粒子轨迹
                        for (int i = 0; i < 5; i++)
                        {
                            ParticleOrchestrator.RequestParticleSpawn(
                                false,
                                ParticleOrchestraType.TerraBlade,
                                new ParticleOrchestraSettings
                                {
                                    PositionInWorld = center + Main.rand.NextVector2Circular(12f, 12f)
                                }
                            );
                        }

                        // 💥 2. 大量粉蓝水晶碎片 Dust
                        for (int i = 0; i < 40; i++)
                        {
                            int dustID = Main.rand.NextBool() ? DustID.PinkCrystalShard : DustID.BlueCrystalShard;
                            Dust dust = Dust.NewDustPerfect(center, dustID, Main.rand.NextVector2Circular(8f, 8f));
                            dust.scale = Main.rand.NextFloat(1.8f, 2.4f);
                            dust.velocity *= 2.2f;
                            dust.noGravity = true;
                        }

                        // 🔥 3. 加入多个高对比颜色的烟雾粒子
                        for (int i = 0; i < 3; i++)
                        {
                            Particle smokeH = new HeavySmokeParticle(
                                center + Main.rand.NextVector2Circular(6f, 6f),
                                Main.rand.NextVector2Circular(2f, 2f),
                                Main.rand.NextBool() ? Color.DeepPink : Color.SkyBlue,
                                36,
                                Main.rand.NextFloat(1.5f, 2.2f),
                                0.9f,
                                Main.rand.NextFloat(-1f, 1f),
                                true
                            );
                            GeneralParticleHandler.SpawnParticle(smokeH);
                        }

                        // 💫 4. 爆裂星芒特效：GenericSparkle（粉蓝混色）
                        for (int i = 0; i < 5; i++)
                        {
                            GenericSparkle sparkle = new GenericSparkle(
                                center,
                                Main.rand.NextVector2Circular(1.5f, 1.5f),
                                Color.Fuchsia,
                                Color.LightBlue,
                                Main.rand.NextFloat(2f, 2.8f),
                                8,
                                Main.rand.NextFloat(-0.02f, 0.02f),
                                1.6f
                            );
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }

                        // 🌊 5. 彩光爆发：高频冲击波 × 2（不改变大小，仅强化亮度与频率）
                        for (int i = 0; i < 2; i++)
                        {
                            Particle blast = new DirectionalPulseRing(
                                center,
                                Vector2.Zero,
                                i == 0 ? Color.HotPink : Color.Cyan,
                                new Vector2(1.1f, 1.1f),
                                0f,
                                0.1f,
                                0.6f,
                                20
                            );
                            GeneralParticleHandler.SpawnParticle(blast);
                        }

                        // 🔷 6. 自定义冲击光波 + 音爆感（高频扰动）
                        for (int i = 0; i < 2; i++)
                        {
                            Particle bolt = new CustomPulse(
                                center,
                                Vector2.Zero,
                                Color.Lerp(Color.DeepPink, Color.LightBlue, Main.rand.NextFloat()),
                                "CalamityMod/Particles/HighResFoggyCircleHardEdge",
                                Vector2.One,
                                Main.rand.NextFloat(-10f, 10f),
                                0.02f,
                                0.18f,
                                18
                            );
                            GeneralParticleHandler.SpawnParticle(bolt);
                        }

                        // ✴️ 7. 闪光火花拖尾：CritSpark
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                            CritSpark spark = new CritSpark(
                                center,
                                sparkVel,
                                Color.White,
                                Color.HotPink,
                                1.3f,
                                18
                            );
                            GeneralParticleHandler.SpawnParticle(spark);
                        }

                        // 🌀 8. 四方粒子：加入科技感粒子
                        for (int i = 0; i < 4; i++)
                        {
                            SquareParticle square = new SquareParticle(
                                center + Main.rand.NextVector2Circular(6f, 6f),
                                Main.rand.NextVector2Circular(1.5f, 1.5f),
                                false,
                                24,
                                2f + Main.rand.NextFloat(0.8f),
                                Color.Lerp(Color.Cyan, Color.Pink, Main.rand.NextFloat()) * 1.2f
                            );
                            GeneralParticleHandler.SpawnParticle(square);
                        }
                    }
                   

                    // 📈 提升伤害倍率
                    int increasedDamage = (int)(hit.Damage * 0.75f);
                    NPC.HitInfo newHitInfo = target.CalculateHitInfo(increasedDamage, hit.HitDirection, hit.Crit, hit.Knockback, hit.DamageType);
                    target.StrikeNPC(newHitInfo);
                }


                // 标记已击中敌人
                Projectile.ai[0] = 1;
            }
        }





        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                // 获取当前弹幕的面朝方向（正前方）
                Vector2 forwardDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);

                // 计算三个发射方向：正后方、左后方、右后方
                Vector2 backDirection = -forwardDirection; // 正后方
                Vector2 leftBackDirection = backDirection.RotatedBy(MathHelper.ToRadians(120)); // 左后方（相对于正后方旋转30度）
                Vector2 rightBackDirection = backDirection.RotatedBy(MathHelper.ToRadians(-120)); // 右后方（相对于正后方旋转-30度）

                // 使用与 BansheeHookJavPROJ 相同的速度发射镰刀弹幕
                float scytheSpeed = Projectile.velocity.Length() * 1f;

                // 生成镰刀弹幕
                CreateBansheeHookScythe(Projectile.Center, backDirection * scytheSpeed);
                CreateBansheeHookScythe(Projectile.Center, leftBackDirection * scytheSpeed);
                CreateBansheeHookScythe(Projectile.Center, rightBackDirection * scytheSpeed);
            }

            // 播放消失音效
            SoundEngine.PlaySound(VoidEdge.ProjectileDeathSound with { Pitch = 0.3f }, Projectile.Center);
        }


        private void CreateBansheeHookScythe(Vector2 position, Vector2 velocity)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                     position,
                                     velocity = velocity * 1.5f,
                                     ModContent.ProjectileType<BansheeHookJavScythe>(),
                                     (int)(Projectile.damage * 1f),
                                     Projectile.knockBack,
                                     Projectile.owner);
        }





    }
}
