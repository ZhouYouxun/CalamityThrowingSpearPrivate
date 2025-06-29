using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.DLOAS;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch.FTDragon
{
    public class FinishingTouchDragon1Head : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50;
            //Projectile.ArmorPenetration = 15;
        }
        float minSpeed = 6f; // 🚩 永不低于的最低速度，可自行在此处调整
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.5f);

            // 寻找最近敌人
            NPC target = FindNearestNPC(1200f);
            if (target != null && target.active)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                Vector2 desiredDir = toTarget.SafeNormalize(Vector2.UnitY);

                float distance = toTarget.Length();

                // 可调整参数
                float minSpeed = 16f;  // 🚩 永不低于最低速度（可自行调）
                float maxSpeed = 50f;  // 🚩 最大速度
                float accel = 1.5f;    // 加速度放大，响应更迅速

                // 缓动速度：先慢中快后慢
                float speedFactor = Utils.GetLerpValue(200f, 600f, distance, true) * Utils.GetLerpValue(1000f, 700f, distance, true);
                float targetSpeed = MathHelper.Clamp(maxSpeed * speedFactor, minSpeed, maxSpeed);

                Vector2 targetVelocity = desiredDir * targetSpeed;

                // 转向限制（每帧最多转动 2° ≈ 0.0349 rad，可调）
                float maxTurnRadians = MathHelper.ToRadians(5f); // 🚩 可调
                float currentSpeed = Projectile.velocity.Length();
                float currentAngle = Projectile.velocity.ToRotation();
                float desiredAngle = targetVelocity.ToRotation();
                float angleDifference = MathHelper.WrapAngle(desiredAngle - currentAngle);

                // Clamp角度差
                angleDifference = MathHelper.Clamp(angleDifference, -maxTurnRadians, maxTurnRadians);

                // 应用旋转
                float newAngle = currentAngle + angleDifference;
                Projectile.velocity = newAngle.ToRotationVector2() * MathHelper.Lerp(currentSpeed, targetSpeed, accel * 0.05f);
            }
            else
            {
                // 无目标时减速但不低于最低速度
                if (Projectile.velocity.Length() > minSpeed)
                    Projectile.velocity *= 0.97f;
                else
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * minSpeed;
            }

            // 智能 rotation
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 火焰粒子点缀
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
                Main.dust[dust].scale = 1.2f;
                Main.dust[dust].velocity = Projectile.velocity * 0.2f;
                Main.dust[dust].noGravity = true;
            }
        }

        private NPC FindNearestNPC(float maxDetectDistance)
        {
            NPC closest = null;
            float minDist = maxDetectDistance;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }




        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center); // 更炸裂的音效

            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FlameBurst);
                Main.dust[dust].scale = Main.rand.NextFloat(1.2f, 2.5f);
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(5f, 5f);
                Main.dust[dust].noGravity = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    Main.rand.Next(61, 64));
                gore.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }
        }


    }
}
