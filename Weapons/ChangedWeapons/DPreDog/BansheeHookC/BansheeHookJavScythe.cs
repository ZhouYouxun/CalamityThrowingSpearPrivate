using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using System;



namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.BansheeHookC
{
    public class BansheeHookJavScythe : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        }
        public ref float Time => ref Projectile.ai[1];
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理资源和位置
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/BansheeHookC/BansheeHookJavScythe").Value;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
                Color color = Color.Lerp(Color.DeepPink, Color.Pink, colorInterpolation) * 0.4f;
                color.A = 0;
                Vector2 drawPosition = Projectile.oldPos[i] + lightTexture.Size() * 1.0f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + new Vector2(-28f, -28f);
                Color outerColor = color;
                Color innerColor = color * 0.5f;
                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60) // 弹幕即将消失时缩小
                {
                    intensity *= Projectile.timeLeft / 60f;
                }
                Vector2 outerScale = new Vector2(1f) * intensity;
                Vector2 innerScale = new Vector2(1f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;

                // 使用 Projectile.rotation 替代固定的旋转角度，确保与自身旋转保持一致
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, Projectile.rotation, lightTexture.Size() * 0.5f, outerScale * 1.2f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, Projectile.rotation, lightTexture.Size() * 0.5f, innerScale * 1.0f, SpriteEffects.None, 0);
            }
            return false;
        }






        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;

            Projectile.aiStyle = ProjAIStyleID.Sickle;
            AIType = ProjectileID.DeathSickle;

            Projectile.alpha = 100;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 420;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.6f / 255f, 0f, 0f);
            Projectile.rotation += 0.25f; // 你可以根据需求调整旋转速度，增加或减少该值

            // 前30帧不追踪，之后开始追踪玩家
            if (Projectile.ai[1] > 30)
            {
                Player player = Main.player[Projectile.owner]; // 查找玩家
                if (player != null)
                {
                    Vector2 direction = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 48f, 0.08f); // 追踪速度为12f

                    // 如果与玩家发生重叠，清除弹幕
                    if (Projectile.Hitbox.Intersects(player.Hitbox))
                    {
                        Projectile.Kill(); // 移除弹幕
                    }
                }
            }
            else
            {
                Projectile.ai[1]++;
            }
            Time++;
        }

        public override bool? CanDamage() => Time >= 6f;

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.timeLeft < 85)
            {
                byte b2 = (byte)(Projectile.timeLeft * 3);
                byte a2 = (byte)(100f * (b2 / 255f));
                return new Color(b2, b2, b2, a2);
            }
            return new Color(255, 255, 255, 100);
        }
        public override void OnKill(int timeLeft)
        {
            //// 在死亡时向自己面向的反方向抛射出 15 个粉红色的原版粒子特效（Dust）
            //for (int i = 0; i < 15; i++)
            //{
            //    Vector2 randomOffset = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)); // 随机方向偏移
            //    Vector2 dustVelocity = -Projectile.velocity * 0.5f + randomOffset; // 反方向速度，加上一些随机性
            //    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch, dustVelocity.X, dustVelocity.Y);
            //    dust.noGravity = true;
            //    dust.scale = 1.5f;
            //}

            // 在死亡时向自己面向的反方向以扇形随机抛射出 15 个粉红色的原版粒子特效（Dust）
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.NextFloat(-45f, 45f)); // 在 -45 到 45 度之间随机生成角度
                Vector2 dustVelocity = Projectile.velocity.RotatedBy(angle) * -0.5f; // 反方向速度，加上随机旋转
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch, dustVelocity.X, dustVelocity.Y);
                dust.noGravity = true;
                dust.scale = 1.5f;
            }
        }



    }
}
