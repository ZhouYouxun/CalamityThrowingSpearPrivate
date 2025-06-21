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

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.HellionFlowerC
{
    internal class HellionFlowerJavAbsorb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";


        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1;
        }



        public override void AI()
        {
            // 查找场上所有 HellionFlowerJavPROJ 弹幕
            int targetIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile otherProj = Main.projectile[i];
                if (otherProj.active && otherProj.type == ModContent.ProjectileType<HellionFlowerJavPROJ>())
                {
                    float distance = Vector2.Distance(Projectile.Center, otherProj.Center);

                    // 如果与 HellionFlowerJavPROJ 的距离小于弹幕的宽度，视为碰撞
                    if (distance < (Projectile.width + otherProj.width) / 2f)
                    {
                        // 碰撞逻辑：取消追踪，设置剩余时间为 15 帧
                        Projectile.ai[0] = 1f; // 标记为已碰撞
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f; // 固定向前飞行速度
                        Projectile.timeLeft = 15; // 设置剩余时间
                        return;
                    }

                    // 寻找最近的目标
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetIndex = i;
                    }
                }
            }

            // 如果未发生碰撞并找到最近的目标，则追踪
            if (targetIndex != -1 && closestDistance < 800f && Projectile.ai[0] == 0f) // 仅在未标记为碰撞时追踪
            {
                Projectile targetProj = Main.projectile[targetIndex];
                Vector2 direction = targetProj.Center - Projectile.Center;
                direction.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 8f, 0.1f);
            }

            // 飞行期间生成粒子效果
            //if (Main.rand.NextFloat() < 0.5f) // 控制粒子生成概率
            {
                int dustType = Main.rand.Next(new int[] { 39, 40, DustID.JungleSpore, DustID.GemEmerald });
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, Projectile.velocity * 0.5f, 150, default, Main.rand.NextFloat(1.5f, 2.2f));
                dust.noGravity = true;
                dust.velocity += Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 自定义弹幕碰撞检测，检查是否与 HellionFlowerJavPROJ 碰撞
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile otherProj = Main.projectile[i];
                if (otherProj.active && otherProj.type == ModContent.ProjectileType<HellionFlowerJavPROJ>())
                {
                    if (projHitbox.Intersects(otherProj.Hitbox))
                    {
                        return true; // 检测到碰撞
                    }
                }
            }
            return base.Colliding(projHitbox, targetHitbox);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false; // 不与方块碰撞
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
        }



    }
}
