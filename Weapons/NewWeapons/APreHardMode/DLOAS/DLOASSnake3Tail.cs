using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.DLOAS
{
    internal class DLOASSnake3Tail : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.NeedsUUID[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50;
            //Projectile.ArmorPenetration = 15;
        }
        private Queue<Vector2> positionHistory = new Queue<Vector2>(); // 存储历史位置

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.5f, 0f, 0.5f);

            // 获取前一节（最后一段身体）
            int prevIndex = Projectile.GetByUUID(Projectile.owner, (int)Projectile.ai[0]);
            if (prevIndex < 0 || !Main.projectile[prevIndex].active)
            {
                SpawnDustEffect();
                Projectile.Kill();
                return;
            }

            // 尾巴不主动移动
            Projectile.velocity = Vector2.Zero;

            Projectile prev = Main.projectile[prevIndex];
            Vector2 offset = prev.Center - Projectile.Center;

            // 平滑角度插值
            float desiredRotation = offset.ToRotation();
            float angleDiff = MathHelper.WrapAngle(desiredRotation - Projectile.rotation);
            //Projectile.rotation += angleDiff * 0.15f + MathHelper.PiOver2 + MathHelper.PiOver4;
            Projectile.rotation += angleDiff + MathHelper.PiOver2;

            // 尾巴继承缩放
            float scale = MathHelper.Clamp(prev.scale, 0.5f, 3f);
            Projectile.scale = scale;
            Projectile.width = Projectile.height = (int)(10f * scale);

            // 沿前一节方向摆尾
            float followDistance = 16f * scale;
            if (offset != Vector2.Zero)
                Projectile.Center = prev.Center - Vector2.Normalize(offset) * followDistance;

            Projectile.spriteDirection = (offset.X > 0f) ? 1 : -1;

            if (Projectile.alpha > 0)
                Projectile.alpha -= 40;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;
        }

        /// <summary>
        /// 释放紫色粒子特效
        /// </summary>
        private void SpawnDustEffect()
        {
            for (int i = 0; i < 8; i++)
            {
                int dustType = Main.rand.NextBool() ? 37 : 173;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }
        }
    }
}
