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
    public class DLOASSnake1Head : ModProjectile, ILocalizedModType
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
            Projectile.localNPCHitCooldown = 50;
            //Projectile.ArmorPenetration = 15;
        }
        private int targetProjectileID = -1;
        private float baseSpeed = 14f;
        private bool isTracking = true; // 是否处于追踪状态
        private int trackingCooldown = 0; // 追踪冷却计时器

        public override void AI()
        {
            // 第一次执行：记录目标弹幕 ID
            if (Projectile.ai[1] == 0)
            {
                targetProjectileID = (int)Projectile.ai[0];
                Projectile.ai[1] = 1;
            }

            // 获取目标弹幕（DLOAS 主弹幕）
            Projectile target = Main.projectile[targetProjectileID];
            if (!target.active || target.type != ModContent.ProjectileType<DLOASPROJ>())
            {
                Projectile.Kill();
                return;
            }

            // 计算目标方向与当前角度
            Vector2 toTarget = target.Center - Projectile.Center;
            float desiredAngle = toTarget.ToRotation();
            float currentAngle = Projectile.velocity.ToRotation();
            float angleDiff = MathHelper.WrapAngle(desiredAngle - currentAngle);

            // 限制每帧最大旋转角度（更懒惰）
            float maxTurnAngle = MathHelper.ToRadians(3f);
            angleDiff = MathHelper.Clamp(angleDiff, -maxTurnAngle, maxTurnAngle);

            // 应用新速度（基于旧速度方向旋转后维持原速）
            float speed = Projectile.velocity.Length();
            float newAngle = currentAngle + angleDiff;
            Projectile.velocity = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * speed;

            // 限制最大速度（防止过冲）
            float maxSpeed = 9.5f;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * maxSpeed;
            }

            // 旋转朝向飞行方向（偏移 π 是为了 sprite 对齐）
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
