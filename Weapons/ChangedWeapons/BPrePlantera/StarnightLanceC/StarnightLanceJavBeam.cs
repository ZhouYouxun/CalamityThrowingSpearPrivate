using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC
{
    public class StarnightLanceJavBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = ProjAIStyleID.Beam;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            AIType = ProjectileID.LightBeam;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Lighting - 添加粉色光源
            Lighting.AddLight(Projectile.Center, 0.4f, 0f, 0.4f);

            //// 生成粉色线性粒子效果
            //if (Main.rand.NextBool(5))
            //{
            //    // 直接在弹幕中心生成粒子，没有左右偏移
            //    Vector2 trailPos = Projectile.Center;

            //    float trailScale = Main.rand.NextFloat(0.8f, 1.2f); // 维持粒子的缩放效果
            //    Color trailColor = Color.LightPink; // 固定颜色为粉色

            //    // 创建粒子
            //    Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
            //    GeneralParticleHandler.SpawnParticle(trail);
            //}

            // 在前 100 帧保持直线飞行
            if (Projectile.timeLeft > 200) // Projectile 总存在时间是 300 帧，timeLeft > 200 表示前 100 帧
            {
                // 每隔 25 帧右拐 90 度
                if (Projectile.timeLeft % 25 == 0)
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2); // 向右旋转 90 度
                }
            }
            // 之后开始追踪敌人
            else
            {
                CalamityUtils.HomeInOnNPC(Projectile, true, 1200f, 24f, 30f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(byte.MaxValue, 50, 128, Projectile.alpha);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 595)
                return false;

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            for (int i = 4; i < 14; i++)
            {
                float projOldX = Projectile.oldVelocity.X * (30f / (float)i);
                float projOldY = Projectile.oldVelocity.Y * (30f / (float)i);
                int starnight = Dust.NewDust(new Vector2(Projectile.oldPosition.X - projOldX, Projectile.oldPosition.Y - projOldY), 8, 8, DustID.PinkFairy, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.8f);
                Dust dust = Main.dust[starnight];
                dust.noGravity = true;
                dust.velocity *= 0.1f;
                starnight = Dust.NewDust(new Vector2(Projectile.oldPosition.X - projOldX, Projectile.oldPosition.Y - projOldY), 8, 8, DustID.PinkFairy, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.4f);
                dust.velocity *= 0.01f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 120);
        }

        public override bool? CanDamage() => Projectile.timeLeft <= 220;
    }
}
