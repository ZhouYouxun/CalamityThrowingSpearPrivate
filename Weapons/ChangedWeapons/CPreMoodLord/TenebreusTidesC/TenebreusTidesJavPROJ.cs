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
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC
{
    public class TenebreusTidesJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/TenebreusTidesC/TenebreusTidesJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
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
            Projectile.penetrate = 20; // 设置穿透次数为20
            Projectile.timeLeft = 1500; // 设置持续时间为1500帧
            Projectile.extraUpdates = 1; // 额外更新次数为1
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            // 保持弹幕旋转逻辑不变
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 设置深海的深蓝色光效
            Lighting.AddLight(Projectile.Center, Color.DarkBlue.ToVector3() * 0.6f);

            // 每帧速度乘以1.03，逐渐加速
            Projectile.velocity *= 1.03f;

            // 每7帧释放一次TenebreusTidesWaterSword弹幕
            if (Projectile.timeLeft % 7 == 0 && Main.myPlayer == Projectile.owner)
            {
                // 当前方向
                Vector2 currentVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero);

                // 正前方的TenebreusTidesWaterSword弹幕
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, currentVelocity * 8f, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);

                // 正左方和正右方的TenebreusTidesWaterSword弹幕（旋转90度和-90度）
                Vector2 leftVelocity = currentVelocity.RotatedBy(MathHelper.ToRadians(90)) * 8f;
                Vector2 rightVelocity = currentVelocity.RotatedBy(MathHelper.ToRadians(-90)) * 8f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, leftVelocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rightVelocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
            }

            // 单螺旋的粒子效果，从左下开始向右下，循环生成
            float progress = (Projectile.timeLeft % 60) / 60f; // 粒子进度控制
            float angle = MathHelper.TwoPi * progress; // 单螺旋角度

            // 计算粒子位置，螺旋从左下到右下
            Vector2 spiralOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f; // 控制螺旋的大小
            Vector2 spiralPosition = Projectile.BottomLeft + spiralOffset;

            // 创建粒子
            Color particleColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f); // 深蓝色渐变效果
            Particle spiralParticle = new HeavySmokeParticle(spiralPosition, Projectile.velocity * 0.2f, particleColor, 30, Projectile.scale * 0.8f, 1.0f, MathHelper.ToRadians(2f), true);
            GeneralParticleHandler.SpawnParticle(spiralParticle);

            // 在飞行路径上留下深蓝色的重型烟雾粒子
            if (Main.rand.NextBool(4)) // 每4帧生成一次粒子
            {
                Color smokeColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f); // 使用深蓝色和浅蓝色渐变
                Particle smoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f, smokeColor, 30, Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f), 1.0f, MathHelper.ToRadians(2f), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 弹幕消失时，在弹幕朝向的正前方释放一大坨重型烟雾粒子特效
            for (int i = 0; i < 15; i++) // 生成15个粒子
            {
                // 计算粒子的朝向和速度，增加一个随机的偏移量，使粒子效果更加自然
                Vector2 particleVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(10)) * 0.5f; // 以弹幕当前朝向为基础，略微偏移
                particleVelocity += Vector2.Normalize(Projectile.velocity) * Main.rand.NextFloat(2f, 4f); // 为粒子赋予朝向的初始加速度

                // 使用深蓝色和浅蓝色渐变
                Color smokeColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f);

                // 创建带有初始速度的粒子
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    particleVelocity, // 使用计算好的速度
                    smokeColor,
                    40,
                    Projectile.scale * Main.rand.NextFloat(0.8f, 1.4f),
                    1.2f,
                    MathHelper.ToRadians(3f),
                    required: true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }
    }
}
