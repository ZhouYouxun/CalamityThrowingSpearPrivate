using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using System;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC
{
    public class VulcaniteLanceJavTinyFlare : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private float rotationAngle = 0f; // 旋转角度
        private const float rotationSpeed = 0.05f; // 旋转速度

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 255;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft < 150 && target.CanBeChasedBy(Projectile);

        public override void AI()
        {
            // 更新旋转角度
            rotationAngle += rotationSpeed;
            if (rotationAngle > MathHelper.TwoPi)
            {
                rotationAngle -= MathHelper.TwoPi;
            }

            // 计算三个粒子的位置（上左下和右下），互为120度夹角
            float[] angles = { 0f, MathHelper.TwoPi / 3f, MathHelper.TwoPi * 2f / 3f }; // 粒子间隔120度
            float radius = 2 * 16f; // 公转半径为2格（2*16像素）

            foreach (float initialAngle in angles)
            {
                // 计算粒子的当前角度和位置
                float angle = initialAngle + rotationAngle;
                Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                // 创建粒子特效
                int fiery = Dust.NewDust(position, 0, 0, DustID.InfernoFork, 0f, 0f, 100, default, Main.rand.NextFloat(1.5f, 2.5f));
                Main.dust[fiery].noGravity = true;
                Main.dust[fiery].velocity = Vector2.Zero; // 粒子初始速度为零
            }

            if (Projectile.timeLeft < 150)
                CalamityUtils.HomeInOnNPC(Projectile, true, 1800f, 10f, 20f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 90); 
        }
    }
}
