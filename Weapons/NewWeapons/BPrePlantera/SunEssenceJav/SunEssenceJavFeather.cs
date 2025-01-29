using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using System;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    public class SunEssenceJavFeather : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.alpha = 255;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = ProjAIStyleID.Nail;
            AIType = ProjectileID.NailFriendly;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // 逐渐加速，每帧乘以
            //Projectile.velocity *= 1.025f;

            // 每三帧执行一次追踪逻辑
            if (Projectile.timeLeft % 3 == 0)
            {
                // 查找范围内最近的敌人
                NPC target = Projectile.Center.ClosestNPCAt(1800);
                if (target != null)
                {
                    // 计算目标方向和当前方向之间的夹角
                    Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float targetAngle = directionToTarget.ToRotation();
                    float currentAngle = Projectile.velocity.ToRotation();
                    float maxTurnAngle = MathHelper.ToRadians(1f); // 最大拐角为X度

                    // 限制转向角度
                    float newAngle = MathHelper.Lerp(currentAngle, targetAngle, maxTurnAngle / Math.Abs(targetAngle - currentAngle));
                    Projectile.velocity = newAngle.ToRotationVector2() * Projectile.velocity.Length(); // 更新速度向量
                }
            }

            if (Main.rand.NextBool(6))
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.GreenTorch, Projectile.velocity.X * 1f, Projectile.velocity.Y * 1f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            for (int i = 0; i < 15; i++)
            {
                int greenDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.2f);
                Main.dust[greenDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[greenDust].scale = 0.5f;
                    Main.dust[greenDust].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 30; j++)
            {
                int greenDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1.7f);
                Main.dust[greenDust2].noGravity = true;
                Main.dust[greenDust2].velocity *= 5f;
                greenDust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GreenTorch, 0f, 0f, 100, default, 1f);
                Main.dust[greenDust2].velocity *= 2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300); // 原版的破晓效果
        }
        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 2);
            return false;
        }
    }
}
