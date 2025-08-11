using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    internal class FinishingTouchEchoCtrl : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 50;
            Projectile.extraUpdates = 0;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 60;
            Projectile.alpha = 255; // 完全透明
        }

        private int shootTimer = 0;
        private int shootCount = 0;
        private const int MaxShoot = 5;
        private NPC target;

        public override void AI()
        {
            // 第一次获取目标
            if (target == null && Projectile.ai[0] >= 0 && Projectile.ai[0] < Main.maxNPCs)
            {
                NPC npc = Main.npc[(int)Projectile.ai[0]];
                if (npc.active && npc.CanBeChasedBy())
                    target = npc;
            }


            shootTimer++;
            if (shootTimer % 5 == 0 && shootCount < MaxShoot && target != null && target.active)
            {
                // 🌀随机一个方向
                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 spawnPosition = target.Center + angle.ToRotationVector2() * 70f * 16f;

                // 🚀飞向目标中心
                Vector2 velocity = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * 16f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<FinishingTouchEcho>(),
                    (int)(Projectile.damage * 0.75f),
                    Projectile.knockBack,
                    Projectile.owner
                );

                shootCount++;
            }


            // 控制弹幕存在时间应稍长于总发射时间
            if (shootCount >= MaxShoot)
                Projectile.Kill();
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }






        public override void OnKill(int timeLeft)
        {
          
        }









    }
}