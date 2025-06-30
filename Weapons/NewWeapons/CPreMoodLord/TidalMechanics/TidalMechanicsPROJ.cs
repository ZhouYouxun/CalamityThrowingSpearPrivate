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
using Terraria.Audio;
using CalamityMod.Particles;
using CalamityMod.Buffs.DamageOverTime;
using CalamityRangerExpansion.LightingBolts;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class TidalMechanicsPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/TidalMechanics/TidalMechanics";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        private const int DecelerationFrames = 60;
        private const int SearchRadius = 19500; // 非常大的距离
        private const float ChargeMultiplier = 4.5f;
        private bool hasTarget = false;
        private Vector2 targetPosition;
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
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加蓝色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.01f;

            // 每个阶段释放水能粒子效果
            CreateWaterParticles();

            if (Projectile.ai[0] < DecelerationFrames)
            {
                // 第1阶段：减速
                Projectile.velocity *= 0.98f;
                Projectile.ai[0]++;
            }
            else if (!hasTarget)
            {
                // 第2阶段：寻找目标
                NPC target = FindClosestNPC(SearchRadius);
                if (target != null)
                {
                    hasTarget = true;
                    targetPosition = target.Center;
                }
                SmoothRotateToTarget(targetPosition);
               
            }
            else if (Projectile.ai[0] < DecelerationFrames + 120)
            {
                // 第3阶段：花式追踪 (120帧)
                DynamicTracking(targetPosition);
                Projectile.ai[0]++;
            }
            else
            {
                // 第4阶段：最终冲刺
                ChargeTowardsTarget(targetPosition);
            }
        }

        private void DynamicTracking(Vector2 target)
        {
            // **1. 计算目标的预测位置**
            Vector2 targetVelocity = target - targetPosition; // 目标的速度向量
            Vector2 predictedPosition = target + targetVelocity * 0.5f; // 预测目标0.5秒后的位置

            // **2. 生成螺旋运动路径**
            float angle = Projectile.ai[1] * 0.1f; // 当前螺旋角度，控制旋转速度
            float radius = 50f + (float)Math.Sin(Projectile.ai[1] * 0.05f) * 20f; // 动态半径，随时间波动
            Vector2 spiralOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

            // **3. 动态轨迹生成：通过插值逼近目标**
            Vector2 direction = (predictedPosition - Projectile.Center).SafeNormalize(Vector2.Zero);
            Vector2 nextPosition = Vector2.Lerp(Projectile.Center, predictedPosition + spiralOffset, 0.1f);

            // 更新弹幕位置与速度
            Projectile.velocity = (nextPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;

            // **4. 粒子特效生成**
            GenerateSpiralParticles();
            GenerateTrailEffect();

            // **5. 调整弹幕旋转和透明度**
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4; // 旋转调整
            Projectile.alpha = (int)(255 * (1f - (radius / 100f))); // 根据半径动态调整透明度

            Projectile.ai[1]++; // 更新AI计数器
        }

        private void GenerateSpiralParticles()
        {
            // **生成动态粒子，形成螺旋水流效果**
            for (int i = 0; i < 3; i++)
            {
                Vector2 particleOffset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 3 * i) * 10f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + particleOffset, DustID.Water, null, 0, Color.LightBlue, 1.5f);
                dust.noGravity = true; // 水流效果粒子无重力
            }
        }

        private void GenerateTrailEffect()
        {
            // **生成轨迹粒子，用于增强视觉效果**
            Vector2 trailOffset = -Projectile.velocity * 0.5f; // 轨迹偏移量
            Dust trailDust = Dust.NewDustPerfect(Projectile.Center + trailOffset, DustID.BlueTorch, null, 0, Color.Cyan, 1.2f);
            trailDust.noGravity = true; // 光效粒子无重力
        }





        // 🚩 优雅水流飞行特效重制
        private void CreateWaterParticles()
        {
            Vector2 center = Projectile.Center;

            // === 1️⃣ 保留并微调：轻型水流烟雾（HeavySmokeParticle，淡蓝）
            Vector2 smokeVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.4f, 0.8f);
            Particle waterSmoke = new HeavySmokeParticle(
                center,
                smokeVelocity,
                Color.LightBlue,
                18,          // 稍长寿命
                0.8f,        // 稍小缩放
                0.4f,        // 透明度偏淡
                0.15f,       // 缓慢旋转
                true         // Required
            );
            GeneralParticleHandler.SpawnParticle(waterSmoke);

            // === 2️⃣ 新增：水元素 Dust，轻缓流动感 ===
            int dustCount = 2; // 保持克制
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width * 0.3f, Projectile.height * 0.3f);
                Vector2 dustPos = center + offset;
                Vector2 dustVelocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.02f, 0.08f);

                int dust = Dust.NewDust(dustPos, 0, 0, DustID.BlueCrystalShard, dustVelocity.X, dustVelocity.Y, 80, Color.Cyan, Main.rand.NextFloat(0.8f, 1.2f));
                Main.dust[dust].noGravity = true;
            }

            // === 3️⃣ 新增：线性水流拖尾（SparkParticle） ===
            if (Main.rand.NextBool(2)) // 平均每两帧一次
            {
                Vector2 sparkVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f + Main.rand.NextVector2Circular(0.1f, 0.1f);
                Particle spark = new SparkParticle(
                    center,
                    sparkVelocity,
                    false,
                    40,                          // 寿命适中
                    0.9f,                         // 精致大小
                    Color.LightBlue * 0.7f       // 淡蓝透亮
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

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

            // 增加蓝色粒子的数量
            for (int j = 0; j < 4; j++) // 将圈数增至4
            {
                for (int i = 0; i < 30; i++) // 每圈生成30个粒子
                {
                    Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 30 * i) * (1.5f + j);
                    Particle waterParticle = new HeavySmokeParticle(Projectile.Center, velocity, Color.DarkBlue, 15, 0.9f, 0.5f, 0.2f, true);
                    GeneralParticleHandler.SpawnParticle(waterParticle);
                }
            }

            return closestNPC;
        }


        private void SmoothRotateToTarget(Vector2 target)
        {
            Vector2 direction = target - Projectile.Center;
            direction.Normalize();
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, direction.ToRotation(), 0.05f);
        }

        private void CreateOceanDust()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 10f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, null, 0, Color.CadetBlue, 1.5f);
                dust.noGravity = true;
            }
        }

        private void ChargeTowardsTarget(Vector2 target)
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.velocity = (target - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length() * ChargeMultiplier;
                Projectile.ai[1] = 1;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 屏幕震动效果
            float shakePower = 5f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            // 释放公转型弹幕
            SpawnTyphoon();

            // 触发基于散度的粒子炸裂
            GenerateDivergenceExplosion();

            // 播放声音
            SoundEngine.PlaySound(SoundID.Item84, Projectile.Center);
        }
        private void GenerateDivergenceExplosion()
        {
            Vector2 origin = Projectile.Center;

            // 🚩 1️⃣ 有序：指数螺旋 SparkParticle 水流矩阵
            int spiralCount = 120;
            float a = 2f;
            float b = 0.15f;

            for (int i = 0; i < spiralCount; i++)
            {
                float theta = i * MathHelper.TwoPi / 20f; // 多圈分布
                float r = a * (float)Math.Exp(b * theta);
                Vector2 pos = origin + theta.ToRotationVector2() * r;

                Vector2 velocity = theta.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 6f;

                Particle spiralSpark = new SparkParticle(
                    pos,
                    velocity,
                    false,
                    50,
                    1.1f,
                    Color.LightBlue * 0.8f
                );
                GeneralParticleHandler.SpawnParticle(spiralSpark);
            }

            // 🚩 2️⃣ 无序：Dust 海水爆散，带多元正弦扰动
            int dustAmount = 200;
            for (int i = 0; i < dustAmount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(8f, 20f);
                Vector2 direction = angle.ToRotationVector2();
                Vector2 velocity = direction * speed;

                // 多元正弦扰动
                velocity.X += (float)Math.Sin(velocity.Y * 0.1f + Main.GameUpdateCount * 0.05f) * 2f;
                velocity.Y += (float)Math.Sin(velocity.X * 0.1f + Main.GameUpdateCount * 0.05f) * 2f;

                int dust = Dust.NewDust(origin, 0, 0, DustID.Water, velocity.X, velocity.Y, 80, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
                Main.dust[dust].noGravity = true;
            }

            // 🚩 3️⃣ 中和：轻型烟雾环绕
            int smokeCount = 40;
            for (int i = 0; i < smokeCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Particle smoke = new HeavySmokeParticle(
                    origin,
                    velocity,
                    Color.LightBlue * 0.6f,
                    50,
                    Main.rand.NextFloat(0.8f, 1.4f),
                    0.3f,
                    Main.rand.NextFloat(-0.05f, 0.05f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 🚩 4️⃣ 特别强化：Square 粒子矩阵（洛伦兹吸引子投影模拟）
            // 洛伦兹参数
            double sigma = 10, rho = 28, beta = 8.0 / 3.0;
            Vector3 p = new Vector3(0.1f, 0f, 0f);

            int squareCount = 120; // 大幅提高
            float dt = 0.01f;
            for (int i = 0; i < squareCount; i++)
            {
                // 洛伦兹吸引子迭代
                double dx = sigma * (p.Y - p.X);
                double dy = p.X * (rho - p.Z) - p.Y;
                double dz = p.X * p.Y - beta * p.Z;
                p.X += (float)(dx * dt);
                p.Y += (float)(dy * dt);
                p.Z += (float)(dz * dt);

                Vector2 spawnPos = origin + new Vector2(p.X, p.Y) * 4f;
                Vector2 velocity = new Vector2(p.Y, -p.X).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(8f, 16f);

                SquareParticle square = new SquareParticle(
                    spawnPos,
                    velocity,
                    false,
                    60,
                    2.0f, // 放大至易见
                    Color.Cyan * 1.4f
                );
                GeneralParticleHandler.SpawnParticle(square);
            }

            // 🚩 5️⃣ CRE 高级光点：水光闪烁
            CTSLightingBoltsSystem.Spawn_SpectralWhispers(origin);
            CTSLightingBoltsSystem.Spawn_GaussDischargeShards(origin);

            // 🚩 6️⃣ 有序收尾：脉冲环扩散
            Particle pulse = new DirectionalPulseRing(
                origin,
                Vector2.Zero,
                Color.LightBlue,
                new Vector2(1.2f, 1.2f),
                0f,
                0.5f,
                8f,
                50
            );
            GeneralParticleHandler.SpawnParticle(pulse);
        }



     


        private void SpawnTyphoon()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 position = Projectile.Center;
                float angle = MathHelper.Pi * i; // 0度 和 180度
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<TidalMechanicsTyphoon>(), Projectile.damage / 2, 0, Projectile.owner);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }

    }
}