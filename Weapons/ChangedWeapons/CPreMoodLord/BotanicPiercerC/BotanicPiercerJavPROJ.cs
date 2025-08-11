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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC
{
    public class BotanicPiercerJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/BotanicPiercerC/BotanicPiercerJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 击中后关闭绘制
            if (hasHitNPC)
                return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15; 
            Projectile.extraUpdates = 8;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Lighting.AddLight(Projectile.Center, Color.LightGreen.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 添加绿色能量光效
            LineParticle energy = new LineParticle(Projectile.Center + Projectile.velocity * 4, Projectile.velocity * 1.95f, false, 9, 2.4f, Color.LimeGreen);
            GeneralParticleHandler.SpawnParticle(energy);

            // 🌿 飞行期间持续释放 Spark
            if (Projectile.timeLeft % 1 == 0) // 每 X 帧释放一次（根据 extraUpdates 可适度调整）
            {
                float angle = Main.GlobalTimeWrappedHourly * 2f + Projectile.whoAmI; // 依据时间和whoAmI制造自然分散角度
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f); // 自然放射

                Particle spark = new SparkParticle(
                    Projectile.Center,
                    velocity,
                    false,
                    60 + Main.rand.Next(0, 20),
                    Main.rand.NextFloat(0.8f, 1.2f),
                    Main.rand.NextBool() ? Color.LimeGreen : Color.LightGreen
                );
                GeneralParticleHandler.SpawnParticle(spark);
                ownedSparkParticles.Add((SparkParticle)spark);
            }


            for (int i = ownedSparkParticles.Count - 1; i >= 0; i--)
            {
                SparkParticle p = ownedSparkParticles[i];

                if (p.Time >= p.Lifetime)
                {
                    ownedSparkParticles.RemoveAt(i);
                    continue;
                }

                // === 🌿 轨迹复杂化：自然离谱草木灵息飞行 ===
                // 判断主弹幕朝哪边飞行（左右）
                float rightTurn = MathHelper.ToRadians(2f);
                if (Projectile.velocity.X < 0) // 如果往左飞，就改成左转
                    rightTurn = -rightTurn;

                p.Velocity = p.Velocity.RotatedBy(rightTurn);


                // 呼吸式加速减速，每 30 帧一个周期
                float cycle = 30f;
                float scaleFactor = 1f + 0.05f * (float)Math.Sin(MathHelper.TwoPi * p.Time / cycle);
                p.Velocity *= scaleFactor;

                // 每 15 帧轻微“抽风”加速脉冲
                if (p.Time % 15 == 0)
                {
                    p.Velocity *= 1.2f;
                }

                // 每 10 帧左右 ±1° 微摆扰动
                if (p.Time % 10 < 5)
                {
                    p.Velocity = p.Velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-1f, 1f)));
                }

                // 可选：让粒子缓慢漂移，类似“自然气流”
                // p.Position += p.Velocity.RotatedBy(Main.GlobalTimeWrappedHourly * 0.5f) * 0.02f;
            }

        }
        private bool hasHitNPC = false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //if (!hasHitNPC)
            //{
            //    hasHitNPC = true;
            //    Projectile.friendly = false; // 禁止后续对 NPC 造成伤害
            //    Projectile.hide = true;      // 内置隐藏标志，防止绘制
            //    Projectile.alpha = 255;      // 完全透明
            //    Projectile.timeLeft = 255;
            //}
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 直接命中敌人的话，造成两倍伤害
            modifiers.FinalDamage *= 2f;
        }
        private List<SparkParticle> ownedSparkParticles = new();
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/翠芒") with { Volume = 1.4f, Pitch = -0.0f }, Projectile.Center);

            // 发射两个 BotanicPiercerJavPROJSPLIT 分裂弹幕
            Vector2 vel1 = Projectile.velocity.RotatedBy(MathHelper.ToRadians(5)) * 0.9f;
            Vector2 vel2 = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-5)) * 0.9f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel1, ModContent.ProjectileType<BotanicPiercerJavPROJSPLIT>(), (int)(Projectile.damage * 0.75f), 0f, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel2, ModContent.ProjectileType<BotanicPiercerJavPROJSPLIT>(), (int)(Projectile.damage * 0.75f), 0f, Projectile.owner);


            {
                // ======================== 🌿 追加大自然美感特效 ========================

                // 🌿 高复杂度 Spark 调用
                int sparkCount = 20;
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = MathHelper.TwoPi / sparkCount * i;
                    Vector2 initialVelocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);

                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        initialVelocity,
                        false,
                        60 + Main.rand.Next(0, 20), // 寿命随机波动
                        Main.rand.NextFloat(0.8f, 1.4f),
                        Color.LimeGreen
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                    ownedSparkParticles.Add((SparkParticle)spark);
                }

                // 🌿 猛烈的森林灵息 Dust 爆发
                int dustAmount = 100;
                float maxRadius = 48f;
                for (int i = 0; i < dustAmount; i++)
                {
                    Vector2 spawnOffset = Main.rand.NextVector2CircularEdge(maxRadius, maxRadius); // 保证分布在圆环上，避免中心堆积
                    Vector2 spawnPos = Projectile.Center + spawnOffset;

                    Vector2 velocity = spawnOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 6f);
                    Dust brightDust = Dust.NewDustPerfect(
                        spawnPos,
                        Main.rand.NextBool() ? 107 : 110, // 绿叶系 Dust 混合
                        velocity,
                        120,
                        Main.rand.NextBool() ? Color.LightGreen : Color.LimeGreen,
                        Main.rand.NextFloat(1.0f, 2.2f)
                    );

                    brightDust.noGravity = true;
                    brightDust.fadeIn = Main.rand.NextFloat(0.8f, 1.3f);
                }


                // 3) 小而精致的图形 - 使用 PointParticle 拼出【自然之花】五角星花瓣
                int petals = 5;
                float petalRadius = 18f;
                for (int j = 0; j < petals; j++)
                {
                    float angle = MathHelper.TwoPi / petals * j;
                    Vector2 offset = angle.ToRotationVector2() * petalRadius;
                    Particle point = new PointParticle(
                        Projectile.Center + offset,
                        offset.SafeNormalize(Vector2.Zero) * 0.5f,
                        false,
                        16,
                        1.1f,
                        Color.LawnGreen
                    );
                    GeneralParticleHandler.SpawnParticle(point);
                }



                // 生成3个方向不同的翠绿色圆圈粒子特效
                for (int i = -1; i <= 1; i++) // 三个不同方向，i = -1, 0, 1
                {
                    // 设置每个粒子的方向，偏移角度根据 i 的值 (-15度, 0度, +15度)
                    Vector2 scatterDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(15 * i)) * 0.55f; // 沿着前方偏移 -15, 0, +15 度

                    // 定义一个逐渐扩散的圆圈粒子，调整旋转方向使圆圈摆正
                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center,
                        scatterDirection,
                        Color.LimeGreen,
                        new Vector2(1f, 2.5f), // 取消旋转比例，使用默认形状
                        Projectile.rotation - MathHelper.PiOver4, // 调整旋转角度，使粒子摆正
                        0.2f, // 粒子透明度衰减
                        0.1f, // 粒子每帧的扩展速度
                        30); // 粒子的存活时间 (帧数)

                    // 生成粒子
                    GeneralParticleHandler.SpawnParticle(pulse);
                }












            }





        }




    }
}
