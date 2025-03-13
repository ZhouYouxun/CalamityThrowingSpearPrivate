using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.DLOASSpear
{
    internal class DLOASSpear3Tail : ModProjectile, ILocalizedModType
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
            Projectile.localNPCHitCooldown = 20;
            //Projectile.ArmorPenetration = 15;
        }
        private Queue<Vector2> positionHistory = new Queue<Vector2>(); // 存储历史位置

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.5f, 0f, 0.5f);

            int previousSegment = Projectile.GetByUUID(Projectile.owner, (int)Projectile.ai[0]);
            if (previousSegment >= 0 && Main.projectile[previousSegment].active)
            {
                Vector2 previousPosition = Main.projectile[previousSegment].Center;

                // 存储前一个部分的历史位置
                positionHistory.Enqueue(previousPosition);

                // 让尾巴跟随身体的历史位置，延迟更大
                if (positionHistory.Count > 2.5)
                    Projectile.Center = positionHistory.Dequeue();

                // 旋转朝向运动方向
                Vector2 followVector = Projectile.Center - previousPosition;
                Projectile.rotation = followVector.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;
                Projectile.spriteDirection = (followVector.X > 0f) ? 1 : -1;
            }
            else
            {
                SpawnDustEffect();
                Projectile.Kill();
            }

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
