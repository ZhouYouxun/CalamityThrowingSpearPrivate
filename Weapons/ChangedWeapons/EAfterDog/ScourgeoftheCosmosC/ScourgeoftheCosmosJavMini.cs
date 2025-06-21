using System;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC
{
    public class ScourgeoftheCosmosJavMini : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }
        public ref float Time => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 375;
            Projectile.extraUpdates = 1;
        }
        private bool prioritizingX = true; // 初始时随机决定优先对齐哪个方向
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity *= 0.85f;
            // 生成时决定追踪延迟时间（45~80 帧）
            Projectile.localAI[1] = Main.rand.Next(45, 81);

        }
        public override void AI()
        {
            // 透明度逐渐减小直到完全可见
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 15;
                if (Projectile.alpha < 0)
                {
                    Projectile.alpha = 0;
                }
            }

            // 动画切换逻辑
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }

            // 生成紫色和浅紫色的粒子特效
            //if (Main.rand.NextBool(1)) // 控制粒子的生成频率，可以调整数字改变概率
            {
                Color particleColor = Main.rand.NextBool() ? Color.Purple : Color.Violet; // 随机选择紫色或浅紫色
                Dust dust = Dust.NewDustPerfect(Projectile.position, DustID.FireworkFountain_Pink); // 使用与紫色和浅紫色类似的粒子ID
                dust.color = particleColor;
                dust.noGravity = true; // 不受重力影响
                dust.scale = 0.65f; // 控制粒子的大小，可以根据需要调整
                dust.velocity *= 0.5f; // 控制粒子的速度
            }

   


            Time++;
            // 旋转朝向与速度一致
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // **前 X[45~80 帧] 帧进行非追踪模式**
            if (Projectile.ai[1] < Projectile.localAI[1])
            {
                // **每 15 帧进行一次 90° 转弯**
                if (Projectile.ai[1] % 15 == 0)
                {
                    bool turnLeft = Main.rand.NextBool(); // 随机决定左转或右转
                    Projectile.velocity = Projectile.velocity.RotatedBy(turnLeft ? -MathHelper.PiOver2 : MathHelper.PiOver2);
                }

                Projectile.ai[1]++;
            }
            else
            {
                // **60 帧后进入四向追踪**
                FollowEnemyInFourDirections();
            }

            // 更新旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        // **贪吃蛇风格的四向追踪**
        private void FollowEnemyInFourDirections()
        {
            NPC target = Projectile.Center.ClosestNPCAt(1800);
            if (target == null) return;

            float distanceX = Math.Abs(target.Center.X - Projectile.Center.X);
            float distanceY = Math.Abs(target.Center.Y - Projectile.Center.Y);

            // 只有在远距离时随机选择先对齐 X 轴还是 Y 轴
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = Main.rand.NextBool() ? 1f : 2f; // 1 = 先对齐 X 轴, 2 = 先对齐 Y 轴
            }

            Vector2 direction = Vector2.Zero;
            if (Projectile.localAI[0] == 1f) // **优先对齐 X 轴**
            {
                if (distanceX > 8)
                {
                    direction = target.Center.X > Projectile.Center.X ? Vector2.UnitX : -Vector2.UnitX;
                }
                else
                {
                    Projectile.localAI[0] = 2f; // **X 轴对齐完成，切换 Y 轴**
                }
            }

            if (Projectile.localAI[0] == 2f) // **优先对齐 Y 轴**
            {
                if (distanceY > 8)
                {
                    direction = target.Center.Y > Projectile.Center.Y ? Vector2.UnitY : -Vector2.UnitY;
                }
                else
                {
                    Projectile.localAI[0] = 1f; // **Y 轴对齐完成，切换 X 轴**
                }
            }

            // **确保弹幕的移动方向是严格的上下左右**
            if (direction != Vector2.Zero)
            {
                Projectile.velocity = direction * 12f; // 调整移动速度
            }
        }

        public override bool? CanDamage() => Time >= 12f; // 初始的时候不会造成伤害，直到12为止

        public override void PostDraw(Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int framing = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y6 = framing * Projectile.frame;
            Vector2 origin = new Vector2(9f, 10f);
            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/ScourgeoftheCosmosMiniGlow").Value, Projectile.Center - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)), Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

    }
}
