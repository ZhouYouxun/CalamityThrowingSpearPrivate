using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLanceWater : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 240; // 存在时间
            Projectile.extraUpdates = 2;
            Projectile.aiStyle = 0; // 不使用默认的 AI 风格
        }

        public override void AI()
        {
            // 重力影响
            Projectile.velocity.Y += 0.1f;

            // 弧线飞行，通过轻微的速度调整来模拟波动
            Projectile.velocity.X += (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f;

            // 生成海蓝色粒子特效
            if (Main.rand.NextBool(1))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.position, DustID.Water);
                dust.color = Color.AliceBlue;
                dust.noGravity = true;
                dust.scale = 1.25f;
                dust.velocity *= 0.5f;
            }

            // 旋转方向与速度保持一致
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void OnKill(int timeLeft)
        {         
            // 生成轻型烟雾特效
            int Dusts = 15;
            float radians = MathHelper.TwoPi / Dusts;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < Dusts; i++)
            {
                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.Blue, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 生成爆炸效果
            //for (int i = 0; i < 20; i++)
            //{
            //    Dust dust = Dust.NewDustPerfect(Projectile.position, DustID.Smoke);
            //    dust.scale = 1.5f;
            //    dust.velocity *= 1.4f;
            //}

            // 生成天蓝色粒子特效
            for (int i = 0; i < 40; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.position, DustID.Water);
                dust.color = Color.LightSkyBlue;
                dust.noGravity = true;
                dust.scale = 1.2f;
                dust.velocity = velocity;
            }
        }
    }
}
