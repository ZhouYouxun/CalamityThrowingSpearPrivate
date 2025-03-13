using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.PolarEssenceSpear
{
    internal class PolarEssenceSpearExtra : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 75;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.96f; // 每帧减速
            Projectile.rotation += MathHelper.ToRadians(5f); // 缓慢自转

            // 释放雪花粒子
            if (Main.rand.NextBool(3)) // 33% 概率
            {
                Dust snow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SnowBlock,
                                               0f, 0f, 100, Color.White, 1.2f);
                snow.noGravity = true;
            }
        }

        // 绘制函数，进行最基础的单体绘制
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            Main.EntitySpriteDraw(texture, drawPosition, null, lightColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            return false;
        }
    }
}
