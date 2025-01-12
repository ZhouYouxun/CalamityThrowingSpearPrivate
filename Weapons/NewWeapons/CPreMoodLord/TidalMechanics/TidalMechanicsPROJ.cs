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
                // 第2阶段：寻找目标并缓慢旋转
                NPC target = FindClosestNPC(SearchRadius);
                if (target != null)
                {
                    hasTarget = true;
                    targetPosition = target.Center;
                }
                SmoothRotateToTarget(targetPosition);
                CreateOceanDust();
                //Projectile.rotation += 4;
            }
            else
            {
                // 第3阶段：冲刺
                ChargeTowardsTarget(targetPosition);
            }

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

            SpawnTyphoon();
            SoundEngine.PlaySound(SoundID.Item84, Projectile.Center);
        }

        private void SpawnTyphoon()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 position = Projectile.Center;
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i) * 5f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<TidalMechanicsTyphoon>(), Projectile.damage / 2, 0, Projectile.owner);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }

    }
}