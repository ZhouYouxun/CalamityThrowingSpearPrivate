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
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Sounds;
using Terraria.Audio;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    public class SunEssenceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        private bool isSpinning = false; // 标记是否进入高速旋转模式
        private int spinDuration = 90; // 高速旋转持续时间

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
            Projectile.penetrate = 7;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // 进入旋转模式前正常飞行
            if (!isSpinning)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

                // 生成后方的火花特效
                if (Main.rand.NextFloat() < 0.2f) // 控制生成概率
                {
                    Vector2 sparkOffset = Projectile.velocity * -0.3f + Main.rand.NextVector2Circular(1f, 1f);

                    // 调整颜色的亮度，将透明度降低到原来的 25%
                    Color startColor = new Color(Color.LightGoldenrodYellow.R, Color.LightGoldenrodYellow.G, Color.LightGoldenrodYellow.B, (int)(Color.LightGoldenrodYellow.A * 0.25f));
                    Color endColor = new Color(Color.LightYellow.R, Color.LightYellow.G, Color.LightYellow.B, (int)(Color.LightYellow.A * 0.25f));

                    GenericSparkle sparker = new GenericSparkle(Projectile.Center + sparkOffset, Vector2.Zero, startColor, endColor, Main.rand.NextFloat(2.5f, 2.9f), 14, Main.rand.NextFloat(-0.01f, 0.01f), 2.5f);
                    GeneralParticleHandler.SpawnParticle(sparker);
                }

            }
            else
            {
                // 高速旋转逻辑
                Projectile.rotation += 0.6f;

                // 获得追踪能力
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 24f, 0.08f); // 追踪速度为12f，逐渐调整方向
                }

                // 生成缩小且扩散的粒子特效
                Color particleColor = Color.LightYellow;
                float particleScale = 0.35f; 
                Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f); // 扩散到周围随机位置

                // 增加扩散速度
                Vector2 particleVelocity = Main.rand.NextVector2Circular(9f, 9f); // 扩散速度提高到原来的三倍
                GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, particleVelocity, particleColor, particleScale, Main.rand.Next(20) + 10));

                // 旋转攻击结束判断
                if (--spinDuration <= 0)
                {
                    Projectile.Kill(); // 在结束时触发 OnKill
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300); // 原版的破晓效果

            if (!isSpinning)
            {
                isSpinning = true;
                Projectile.velocity = Vector2.Zero; // 停止移动
                Projectile.timeLeft = spinDuration; // 保证旋转期间不消失
            }
            else
            {
                // 旋转期间每次造成伤害时召唤羽毛
                //int featherCount = Main.dayTime ? 3 : 1; // 白天时获得强化
                int featherCount = 3; // 不再变得强化，而是固定三个
                for (int i = 0; i < featherCount; i++)
                {
                    // 在主弹幕正上方50个方块的位置，以该点为圆心，半径15个方块的范围内随机生成
                    Vector2 featherSpawnPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f) * 16f, -50f * 16f);

                    // 计算羽毛向主弹幕位置的速度向量
                    Vector2 featherVelocity = (Projectile.Center - featherSpawnPosition).SafeNormalize(Vector2.UnitY) * 45f;

                    // 生成羽毛弹幕
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), featherSpawnPosition, featherVelocity, ModContent.ProjectileType<SunEssenceJavFeather>(), (int)(Projectile.damage * 0.6f), 0, Projectile.owner);
                }

            }
        }


        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item90, Projectile.Center);

            CreateSunParticleEffect();

            //// 在头顶15格左右的随机位置生成1条激光
            //for (int i = 0; i < 1; i++)
            //{
            //    Vector2 beamPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f), -Main.rand.NextFloat(10f, 15f) * 16f);
            //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), beamPosition, Vector2.UnitY, ModContent.ProjectileType<SunEssenceJavBEAM>(), (int)(Projectile.damage * 1.0f), 0, Projectile.owner);
            //}
        }






        // 生成太阳形状的粒子特效
        private void CreateSunParticleEffect()
        {
            int particleCount = 20; // 粒子数量，形成太阳形状
            float radius = 30f; // 粒子半径

            for (int i = 0; i < particleCount; i++)
            {
                // 每个粒子的角度间隔
                float angle = MathHelper.TwoPi * i / particleCount;

                // 计算粒子的方向和位置
                Vector2 particleDirection = angle.ToRotationVector2();
                Vector2 particlePosition = Projectile.Center + particleDirection * radius;

                // 生成粒子特效
                Dust dust = Dust.NewDustPerfect(particlePosition, DustID.SolarFlare, particleDirection * 2f, Scale: 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
                dust.color = Color.Yellow;
            }
        }

        // 创建粒子特效
        private void CreateParticleEffect()
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, velocity, Scale: 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
                dust.color = Color.Orange;
            }
        }

   


    }
}

