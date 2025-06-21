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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    internal class SurfeiterStonePillars : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        private int moveDirection = 0;
        private int frameCounter = 0;
        private float acceleration = 0.175f; // 逐渐加速值
        private float maxSpeed = 8f; // 最大速度

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 35;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }


        public void SetDirection(int direction)
        {
            moveDirection = direction;
        }

        public override void AI()
        {
            // 保持弹幕旋转
            //Projectile.rotation = Projectile.velocity.ToRotation();

            frameCounter++;

            // 前X帧静止
            if (frameCounter <= 55)
                return;

            // 逐渐加速
            if (Math.Abs(Projectile.velocity.X) < maxSpeed)
                Projectile.velocity.X += moveDirection * acceleration;

            // 释放粒子（频率 ×3，范围扩大到整个宽高）
            for (int i = 0; i < 6; i++) // 数量翻三倍
            {
                int dustType = Main.rand.Next(new int[] { 0, 8, 22, 28 });

                // 让粒子在整个弹幕宽度和高度范围内随机生成
                Vector2 dustPosition = Projectile.Center + new Vector2(
                    Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2), // X 方向
                    Main.rand.NextFloat(-Projectile.height / 2, Projectile.height / 2) // Y 方向
                );

                Dust dust = Dust.NewDustPerfect(dustPosition, dustType);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.2f, 1.6f); // 让粒子大小更明显
            }

            // 记录初始位置（仅记录一次）
            if (Projectile.localAI[0] == 0 && Projectile.localAI[1] == 0)
            {
                Projectile.localAI[0] = Projectile.Center.X; // 初始 X 坐标
                Projectile.localAI[1] = Projectile.Center.Y; // 初始 Y 坐标
            }

            // 只有在移动时计算距离
            if (frameCounter > 12 && Projectile.velocity.Length() > 0)
            {
                float distanceX = Projectile.Center.X - Projectile.localAI[0];
                float distanceY = Projectile.Center.Y - Projectile.localAI[1];
                float totalDistance = (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

                // 当飞行距离超过 30 * 16 时，销毁弹幕
                if (totalDistance >= 30 * 16f)
                {
                    Projectile.Kill();
                    return;
                }
            }

        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }
    }
}