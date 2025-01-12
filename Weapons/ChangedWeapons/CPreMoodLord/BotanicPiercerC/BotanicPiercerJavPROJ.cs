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
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
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
            LineParticle energy = new LineParticle(Projectile.Center + Projectile.velocity * 4, Projectile.velocity * 4.95f, false, 9, 2.4f, Color.LimeGreen);
            GeneralParticleHandler.SpawnParticle(energy);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item53 with { Pitch = 0.9f, Volume = 0.4f }, Projectile.Center);

            // 命中时生成深绿色和浅绿色渐变的重型烟雾粒子
            //for (int i = 0; i < 8; i++)
            //{
            //    Color smokeColor = Color.Lerp(Color.DarkGreen, Color.LightGreen, 0.5f);
            //    Particle smoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f, smokeColor, 20, Projectile.scale * Main.rand.NextFloat(0.6f, 1.2f), 0.8f, MathHelper.ToRadians(3f), required: true);
            //    GeneralParticleHandler.SpawnParticle(smoke);
            //}

            // 造成两倍伤害并手动删除弹幕
            //Projectile.damage *= 2;
            //Projectile.Kill();
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 直接命中敌人的话，造成两倍伤害
            modifiers.FinalDamage *= 2f;
        }
        public override void OnKill(int timeLeft)
        {
            // 未命中敌人时，生成6个绿色的椎型粒子并发射分裂弹幕
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.ToRadians(5) * i;
                Vector2 velocity = Projectile.velocity.RotatedBy(angle);
                PointParticle particle = new PointParticle(Projectile.Center, velocity * 0.5f, false, 5, 1.1f, Color.LimeGreen);
                GeneralParticleHandler.SpawnParticle(particle);
            }

            // 发射两个 BotanicPiercerJavPROJSPLIT 分裂弹幕
            Vector2 vel1 = Projectile.velocity.RotatedBy(MathHelper.ToRadians(5)) * 0.9f;
            Vector2 vel2 = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-5)) * 0.9f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel1, ModContent.ProjectileType<BotanicPiercerJavPROJSPLIT>(), (int)(Projectile.damage * 0.70f), 0f, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel2, ModContent.ProjectileType<BotanicPiercerJavPROJSPLIT>(), (int)(Projectile.damage * 0.70f), 0f, Projectile.owner);

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
