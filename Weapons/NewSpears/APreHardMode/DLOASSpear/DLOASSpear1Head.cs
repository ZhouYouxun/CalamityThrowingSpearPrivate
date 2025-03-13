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

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.DLOASSpear
{
    internal class DLOASSpear1Head : ModProjectile, ILocalizedModType
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

            Projectile.velocity *= 1.01f; // 每帧速度增加 1%

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
