using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class RevelationSpark : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public float Time
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public const int DustType = 226;
        public const float MaxHomingDistance = 1200f;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            //Projectile.arrow = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // 确保速度固定为 10f
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 22f;

            if (!Main.dedServ && Time > 5f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 spawnPosition = Vector2.Lerp(Projectile.oldPosition, Projectile.position, i / 3f);
                    Dust dust = Dust.NewDustPerfect(spawnPosition, DustType);
                    dust.color = Main.hslToRgb((Main.rand.NextFloat(-0.04f, 0.04f) + Time / 80f) % 1f, 0.8f, 0.6f);
                    dust.scale = 1.3f;
                    dust.fadeIn = 1f;
                    dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    dust.velocity = Vector2.Zero;
                    dust.noGravity = true;
                }
            }



            {
                //NPC potentialTarget = Projectile.Center.ClosestNPCAt(MaxHomingDistance);
                //if (potentialTarget != null)
                //{
                //    Projectile.velocity = (Projectile.velocity * 8f + Projectile.SafeDirectionTo(potentialTarget.Center) * 18f) / 9f;
                //    return;
                //}

                if (Time > 60f) // 超过60帧后直接直线飞行
                {
                    NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = direction * 18f; // 直接设置为直线飞行速度18f
                    }
                }
                else
                {
                    Projectile.ai[1]++;
                }


                if (Time > 30f)
                {
                    float updatedTime = Time - 30f;
                    // Make a complete 90 degree turn in 30 frames.
                    if (updatedTime % 120f > 90f)
                    {
                        Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2 / 20f);
                    }
                    // Arc around quickly for 60 frames.
                    else if (updatedTime % 120f > 30f)
                    {
                        Projectile.velocity = Projectile.velocity.RotatedBy((float)Math.Sin((updatedTime - 30f) % 60f / 60f * MathHelper.TwoPi) * MathHelper.ToRadians(15f));
                    }
                }
            }

            Time++;
        }

        public override void OnKill(int timeLeft)
        {
            //Projectile.ExpandHitboxBy(60, 60);
            //Projectile.Damage();
            if (!Main.dedServ)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustType);
                    dust.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.7f);
                    dust.scale = Main.rand.NextFloat(0.9f, 1.25f);
                    dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
                    dust.noGravity = true;
                }
            }
        }
    }
}
