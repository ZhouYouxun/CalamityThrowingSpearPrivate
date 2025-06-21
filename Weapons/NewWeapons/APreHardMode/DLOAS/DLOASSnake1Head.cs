using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.DLOAS
{
    internal class DLOASSnake1Head : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 800;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            //Projectile.ArmorPenetration = 15;
        }
        private int targetProjectileID = -1;
        private float baseSpeed = 14f;
        private bool isTracking = true; // 是否处于追踪状态
        private int trackingCooldown = 0; // 追踪冷却计时器

        public override void AI()
        {
            // 仅在第一帧获取 `DLOASPROJ` ID
            if (Projectile.ai[1] == 0)
            {
                targetProjectileID = (int)Projectile.ai[0];
                Projectile.ai[1] = 1;
            }

            // 查找 `DLOASPROJ`
            Projectile target = Main.projectile[targetProjectileID];
            if (!target.active || target.type != ModContent.ProjectileType<DLOASPROJ>())
            {
                Projectile.Kill();
                return;
            }

            Vector2 targetDirection = target.Center - Projectile.Center;
            float distance = targetDirection.Length();

            // 检测是否进入 X × 16 范围的圆圈
            if (isTracking && distance <= 5 * 16f)
            {
                isTracking = false; // 停止追踪
                trackingCooldown = Main.rand.Next(15, 36); // 进入直线飞行模式 15~35 帧
            }

            if (!isTracking)
            {
                trackingCooldown--;

                // **直线飞行**
                if (trackingCooldown <= 0)
                {
                    isTracking = true; // 重新启用追踪
                }
            }
            else
            {
                // **追踪 `DLOASPROJ`，但角度变化受限制**
                targetDirection.Normalize();
                float maxTurnAngle = MathHelper.ToRadians(5); // 每帧最多旋转 X°
                Vector2 newVelocity = Vector2.Lerp(Projectile.velocity, targetDirection * baseSpeed, 0.1f);
                float angleDifference = MathHelper.WrapAngle(newVelocity.ToRotation() - Projectile.velocity.ToRotation());

                // 限制最大转向角度
                if (Math.Abs(angleDifference) > maxTurnAngle)
                {
                    angleDifference = Math.Sign(angleDifference) * maxTurnAngle;
                    newVelocity = Projectile.velocity.RotatedBy(angleDifference);
                }

                Projectile.velocity = newVelocity;
            }

            // 旋转朝向飞行方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;
        }
        

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

            for (int i = 0; i < 10; i++) // 增加粒子数量
            {
                int dustType = Main.rand.NextBool() ? 37 : 173;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
                dust.velocity *= 1.2f; // 增强粒子速度
                dust.scale = 1.5f; // 放大粒子
                dust.noGravity = true;
            }
        }
    }
}
