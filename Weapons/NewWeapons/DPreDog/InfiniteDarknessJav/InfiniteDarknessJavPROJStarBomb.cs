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
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav
{
    public class InfiniteDarknessJavPROJStarBomb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        private int time = 0;
        private int scaleCounter = 0;
        private bool isApproaching = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor) // 确保贴图的中心点为绘制的中心点
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 穿透次数为 1
            Projectile.timeLeft = 900; // 持续时间为 15 秒
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            time++;
            Projectile.rotation += 0.2f * Projectile.direction;

            // 12 帧之内不造成伤害
            if (time < 12)
                Projectile.friendly = false;
            else
                Projectile.friendly = true;

            // 周期性膨胀和收缩
            scaleCounter++;
            float scaleFactor = 1.0f + 0.2f * (float)Math.Sin(scaleCounter * 0.1f); // 周期性膨胀与收缩
            Projectile.scale = scaleFactor;
            Projectile.velocity *= scaleFactor > 1.15f || scaleFactor < 0.85f ? 0.97f : 1f; // 在最大和最小时减速

            // 漂浮一段时间后开始寻找敌人
            if (time > 60 && !isApproaching) // 漂浮 60 帧后才锁定敌人
            {
                NPC target = Projectile.Center.ClosestNPCAt(1000); // 检查 1000 范围内的敌人
                if (target != null)
                {
                    isApproaching = true;
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 10f; // 设置初始冲刺速度
                }
            }

            // 当开始冲刺时逐渐加速
            if (isApproaching)
            {
                NPC target = Projectile.Center.ClosestNPCAt(200); // 确保追踪的敌人存在且在范围内
                if (target != null)
                {
                    // 追踪敌人并加速
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 8f, 0.05f); // 使用Lerp逐渐增加速度
                }
                else
                {
                    isApproaching = false; // 如果目标丢失，停止追踪
                }
            }

            // 到达最大时间或速度减小到一定值时爆炸
            if (Projectile.timeLeft <= 1 || (isApproaching && Projectile.velocity.Length() < 0.5f))
            {
                Explode();
            }
        }

        private void Explode()
        {
            Projectile.Kill();
            int particleAmount = 15;
            for (int i = 0; i < particleAmount; i++)
            {
                float radians = MathHelper.TwoPi / particleAmount;
                Vector2 spinningPoint = new Vector2(1f, 0f).RotatedBy(radians * i);
                Vector2 velocity = spinningPoint.RotatedByRandom(0.15f) * (15f / 7f); // 将速度减少到原来的 1/7
                LineParticle subTrail = new LineParticle(Projectile.Center + velocity * 20.5f, velocity, false, 30, 0.75f, Color.White);
                GeneralParticleHandler.SpawnParticle(subTrail);
            }
        }


        public override bool? CanDamage() => time >= 12;

    }
}

