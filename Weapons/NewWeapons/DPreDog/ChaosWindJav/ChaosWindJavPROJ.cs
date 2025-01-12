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
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using Terraria.Audio;
using CalamityMod.Sounds;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ChaosWindJav
{
    public class ChaosWindJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/ChaosWindJav/ChaosWindJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
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
            Projectile.timeLeft = 95;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            Projectile.localAI[0]++;

            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 弹幕逐渐加速
            Projectile.velocity.X *= 1.005f;
            Projectile.velocity.Y -= 0.06f;


            // 生成左右漂移的轻型白色烟雾特效
            int dustCount = 3; // 每次生成的烟雾数量
            float radians = MathHelper.TwoPi / dustCount;
            Vector2 smokePoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustVelocity = smokePoint.RotatedBy(radians * i).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2.6f);
                Color smokeColor = Color.White;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.WhiteSmoke, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 特效尾迹
            Vector2 trailPos = Projectile.Center;
            float trailScale = Main.rand.NextFloat(0.8f, 1.2f);
            Color trailColor = Color.White; // 白色特效
            Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
            GeneralParticleHandler.SpawnParticle(trail);

        }


        public override void OnKill(int timeLeft)
        {
            bool isStrongLogic = Projectile.localAI[0] >= 90;

            if (isStrongLogic)
            {
                // 强化逻辑
                SoundEngine.PlaySound(CommonCalamitySounds.LightningSound, Projectile.Center);

                // 屏幕震动效果
                float shakePower = 5f;
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                // 强化爆炸效果
                Vector2 spawnPosition = Projectile.Center;
                Color whiteColor = Color.White;
                float smallerScale = 1.5f;
                float rotationSpeed = Main.rand.NextFloat(-10f, 10f);
                Particle whiteExplosion = new CustomPulse(spawnPosition, Vector2.Zero, whiteColor, "CalamityMod/Particles/LargeBloom", new Vector2(0.8f, 0.8f), rotationSpeed, smallerScale, smallerScale - 0.5f, 15);
                GeneralParticleHandler.SpawnParticle(whiteExplosion);

                // 生成强逻辑的电磁球
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ChaosWindJavElectromagneticBall>(), (int)(Projectile.damage * 2f), 0, Projectile.owner, 1);
            }
            else
            {
                // 弱化逻辑
                SoundEngine.PlaySound(CommonCalamitySounds.LightningSound, Projectile.Center);

                // 生成弱逻辑的电磁球
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ChaosWindJavElectromagneticBall>(), (int)(Projectile.damage * 0.5f), 0, Projectile.owner, 0);
            }
        }


        //private void SpawnLightningBolts(int count)
        //{
        //    // 获取 ChaosWindJavPROJ 弹幕初始速度的 2.5 倍
        //    Vector2 lightningSpeed = Projectile.velocity * 2.5f;

        //    for (int i = 0; i < count; i++)
        //    {
        //        // 每条闪电在 360 度内随机角度生成
        //        Vector2 velocity = lightningSpeed.RotatedByRandom(MathHelper.TwoPi);

        //        // 生成闪电弹幕
        //        int lightningProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ProjectileID.CultistBossLightningOrbArc, Projectile.damage, 0f, Projectile.owner);

        //        // 设置闪电属性
        //        Projectile proj = Main.projectile[lightningProjectile];
        //        proj.friendly = true;
        //        proj.hostile = false;
        //        proj.penetrate = 10;
        //        proj.localNPCHitCooldown = 50;
        //        proj.usesLocalNPCImmunity = true;
        //    }
        //}
    }
}

