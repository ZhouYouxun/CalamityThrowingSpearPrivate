using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav
{
    public class EndlessDevourJavOrb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            //ProjectileID.Sets.MinionShot[Projectile.type] = true;
            //ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 在飞行期间绘制黑色残影
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.Black, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 50;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.penetrate = 1;

            Projectile.tileCollide = false;
            Projectile.scale = 0.01f;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.scale < 1f)
                return false;
            return null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int d = 0; d < 6; d++)
            {
                int shadow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, new Color(0, 0, 0), 2f);
                Main.dust[shadow].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[shadow].scale = 0.5f;
                    Main.dust[shadow].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int d = 0; d < 10; d++)
            {
                int shadow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, new Color(0, 0, 0), 3f);
                Main.dust[shadow].noGravity = true;
                Main.dust[shadow].velocity *= 5f;
                shadow = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, new Color(0, 0, 0), 2f);
                Main.dust[shadow].velocity *= 2f;
            }
        }
        private bool weaponLost = false;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // === 查找是否存在处于 Aim 状态的 EndlessDevourJavPROJ ===
            Projectile targetProj = null;
            if (!weaponLost) // 🚩 只要武器未丢失，才检测
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<EndlessDevourJavPROJ>())
                    {
                        if (p.ModProjectile is EndlessDevourJavPROJ ej && ej.CurrentState == EndlessDevourJavPROJ.BehaviorState.Aim)
                        {
                            targetProj = p;
                            break;
                        }
                    }
                }

                if (targetProj == null)
                {
                    weaponLost = true; // 🚩 武器第一次消失后永久标记，之后不再追踪武器
                }
            }


            NPC targetNPC = null;
            if (targetProj == null)
            {
                // 若无 Aim 状态武器，使用原先敌人追踪逻辑（带奇特弹性）
                targetNPC = CalamityUtils.MinionHoming(Projectile.Center, 1800f, owner);
            }

            // === 初始化爆发特效 ===
            if (Projectile.timeLeft == 300)
            {
                Color particleColor = Color.Black;
                float particleScale = 1.5f;
                GeneralParticleHandler.SpawnParticle(new GenericBloom(Projectile.Center, Vector2.Zero, particleColor, particleScale, 30));
                GeneralParticleHandler.SpawnParticle(new GenericBloom(Projectile.Center, new Vector2(0, 1).RotatedBy(MathHelper.PiOver2), particleColor, particleScale, 30));
            }

            if (Projectile.scale >= 1f)
            {
                Vector2 targetCenter = Projectile.Center;
                if (targetProj != null)
                {
                    targetCenter = targetProj.Center;
                }
                else if (targetNPC != null)
                {
                    targetCenter = targetNPC.Center;
                }
                else
                {
                    // 无目标时保持当前速度
                    Projectile.velocity *= 0.99f; // 可选平滑减速【可调整】
                }

                if (targetProj != null || targetNPC != null)
                {
                    float projSpeed = 40f;
                    Vector2 fireDirection = Projectile.Center;
                    Vector2 fireVel = targetCenter - fireDirection;
                    float fireVelocity = fireVel.Length();
                    if (fireVelocity < 100f)
                    {
                        projSpeed = 28f;
                    }
                    fireVelocity = projSpeed / fireVelocity;
                    fireVel *= fireVelocity;

                    Projectile.velocity.X = (Projectile.velocity.X * 25f + fireVel.X) / 26f;
                    Projectile.velocity.Y = (Projectile.velocity.Y * 25f + fireVel.Y) / 26f;

                    if (Main.rand.NextBool(5))
                        Projectile.velocity *= 1.1f;
                }
            }
            else
            {
                Projectile.scale += 0.025f;
                Projectile.velocity *= 1.03f;
            }

            // 动画更新
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Projectile.type] - 1)
            {
                Projectile.frame = 0;
            }
            Projectile.frameCounter++;

            if (Projectile.timeLeft <= 60)
            {
                Projectile.alpha += 4;
            }
        }





    }
}
