using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.DLOAS
{
    internal class DLOASSnake2Body : ModProjectile, ILocalizedModType
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

            // 获取前一节（蛇头或身体）的弹幕
            int prevIndex = Projectile.GetByUUID(Projectile.owner, (int)Projectile.ai[0]);
            if (prevIndex < 0 || !Main.projectile[prevIndex].active)
            {
                SpawnDustEffect();
                Projectile.Kill();
                return;
            }

            // 蛇身本身不主动移动
            Projectile.velocity = Vector2.Zero;

            Projectile prev = Main.projectile[prevIndex];
            Vector2 offset = prev.Center - Projectile.Center;

            // 平滑角度过渡：使蛇身缓慢追上前段角度
            float desiredRotation = offset.ToRotation();
            float angleDiff = MathHelper.WrapAngle(desiredRotation - Projectile.rotation);
            Projectile.rotation += angleDiff + MathHelper.PiOver2;

            // 跟随缩放（如不需要可以删掉）
            float scale = MathHelper.Clamp(prev.scale, 0.5f, 3f);
            Projectile.scale = scale;
            Projectile.width = Projectile.height = (int)(10f * scale);

            // 设置位置：沿前一段方向延伸 16 像素（形成连接）
            float followDistance = 11f * scale; // 调整这个xf来控制身体和头的距离
            if (offset != Vector2.Zero)
                Projectile.Center = prev.Center - Vector2.Normalize(offset) * followDistance;

            // 设置贴图方向
            Projectile.spriteDirection = (offset.X > 0f) ? 1 : -1;

            // Alpha 逐步显现
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
