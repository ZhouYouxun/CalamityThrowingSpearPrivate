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
                CreateOceanDust();
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





        // 新增的粒子生成方法
        private void CreateWaterParticles()
        {
            Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 1.5f);
            Particle waterParticle = new HeavySmokeParticle(Projectile.Center, velocity, Color.LightBlue, 15, 0.9f, 0.5f, 0.2f, true);
            GeneralParticleHandler.SpawnParticle(waterParticle);
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

        // 生成 极 端 复 杂 的基于散度的粒子炸裂效果
        private void GenerateDivergenceExplosion()
        {
            int particleCount = Main.rand.Next(333, 666); // 随机生成 ? 个粒子
            float baseExplosionForce = 6.5f; // 基础爆炸力度
            float divergenceFactor = 1.5f; // 散度增强因子
            Vector2 explosionOrigin = Projectile.Center; // 以弹幕中心为爆炸点

            for (int i = 0; i < particleCount; i++)
            {
                // 选择随机粒子类型
                int dustType = Main.rand.NextBool() ? DustID.Water : Main.rand.Next(new int[] { 13, 34, DustID.BlueTorch });

                // 生成随机方向：基于球坐标系统 (极坐标转换)
                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi); // 0~2π 之间的角度
                float divergenceStrength = baseExplosionForce + Main.rand.NextFloat(-3f, 5f); // 爆炸力度的随机化
                float spread = Main.rand.NextFloat(0.5f, 1.5f); // 随机扩散系数

                // 计算散度方向的扰动
                Vector2 divergence = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * divergenceStrength;
                divergence += divergenceFactor * divergence.Length() * new Vector2(
                    (float)Math.Cos(angle + MathHelper.PiOver4),
                    (float)Math.Sin(angle + MathHelper.PiOver4)
                );

                // 生成粒子
                Dust dust = Dust.NewDustPerfect(explosionOrigin, dustType, divergence * spread, 0, Color.Cyan, Main.rand.NextFloat(1.72f, 2.55f));
                dust.noGravity = Main.rand.NextBool(); // 50% 机率无重力
                dust.fadeIn = Main.rand.NextFloat(0.5f, 1.5f); // 渐变增强
            }
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