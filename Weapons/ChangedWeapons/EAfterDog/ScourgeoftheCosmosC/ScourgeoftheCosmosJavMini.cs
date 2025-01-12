using System;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC
{
    public class ScourgeoftheCosmosJavMini : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        private int bounce = 3;

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

        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft < 270 && target.CanBeChasedBy(Projectile);

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
            if (Main.rand.NextBool(2)) // 控制粒子的生成频率，可以调整数字改变概率
            {
                Color particleColor = Main.rand.NextBool() ? Color.Purple : Color.Violet; // 随机选择紫色或浅紫色
                Dust dust = Dust.NewDustPerfect(Projectile.position, DustID.FireworkFountain_Pink); // 使用与紫色和浅紫色类似的粒子ID
                dust.color = particleColor;
                dust.noGravity = true; // 不受重力影响
                dust.scale = 1.2f; // 控制粒子的大小，可以根据需要调整
                dust.velocity *= 0.5f; // 控制粒子的速度
            }

            // 前30帧以正弦波运动，之后开始追踪敌人
            if (Projectile.ai[1] > 30)
            {
                // 追踪逻辑
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 18f, 0.08f); // 追踪速度为12f
                }
            }
            else
            {
                // 正弦波运动逻辑
                float waveFrequency = 1.9f; // 控制正弦波的频率
                float waveAmplitude = 20f;  // 控制正弦波的振幅
                Projectile.velocity = Projectile.velocity.RotatedBy(Math.Sin(Projectile.ai[1] * waveFrequency) * waveAmplitude * (MathHelper.Pi / 180f));
                Projectile.ai[1]++;
            }
            Time++;
            // 旋转朝向与速度一致
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounce--;
            if (bounce <= 0)
                Projectile.Kill();
            else
            {
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
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
