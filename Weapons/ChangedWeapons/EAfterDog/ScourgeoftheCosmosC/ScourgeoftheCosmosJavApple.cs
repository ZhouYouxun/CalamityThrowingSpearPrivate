using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC
{
    public class ScourgeoftheCosmosJavApple : ModProjectile
    {
        //public override string Texture => "Terraria/Images/Item_4009"; // 使用原版的苹果贴图
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 60;
            Projectile.friendly = false; // 不造成伤害
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600; // X 秒后自动消失
            Projectile.MaxUpdates = 1;
            Projectile.tileCollide = false; // 不与地形碰撞
            Projectile.ignoreWater = true;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
        }

        public override void AI()
        {
            // 使苹果缓慢下降
            //Projectile.velocity.Y = 1.5f;

            // 苹果旋转
            Projectile.rotation += 1f;

            // 添加轻微的左右摆动
            Projectile.velocity.X = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.2f;

            // 增加苹果的光效
            Lighting.AddLight(Projectile.Center, Color.Gold.ToVector3() * 0.4f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center; // 粒子生成的中心点
            int totalParticles = 60; // 总粒子数，用于各部分粒子的分布密度
            float radius = 30f; // 苹果主体的半径

            // **1. 左侧半圆（从顶部到左下角）**
            for (int i = 0; i < totalParticles / 3; i++)
            {
                float angle = MathHelper.Pi + MathHelper.PiOver2 * (i / (float)(totalParticles / 3));
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                int dustType = Main.rand.NextBool() ? DustID.RedTorch : DustID.HealingPlus;
                Dust dust = Dust.NewDustPerfect(center + offset, dustType, offset.SafeNormalize(Vector2.Zero) * 0.2f, 100, Color.White, 1.95f);
                dust.noGravity = true;
            }

            // **2. 右侧半圆（从顶部到右下角）**
            for (int i = 0; i < totalParticles / 3; i++)
            {
                float angle = -MathHelper.PiOver2 * (i / (float)(totalParticles / 3));
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                int dustType = Main.rand.NextBool() ? DustID.RedTorch : DustID.HealingPlus;
                Dust dust = Dust.NewDustPerfect(center + offset, dustType, offset.SafeNormalize(Vector2.Zero) * 0.2f, 100, Color.White, 1.95f);
                dust.noGravity = true;
            }

            // **3. 小半圆（底部凹陷）**
            int smallParticles = totalParticles / 6;
            float smallRadius = radius * 0.3f; // 小半圆的半径
            for (int i = 0; i < smallParticles; i++)
            {
                float angle = MathHelper.PiOver4 + MathHelper.PiOver4 * (i / (float)smallParticles); // 控制凹陷角度范围
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * smallRadius;

                int dustType = Main.rand.NextBool() ? DustID.RedTorch : DustID.HealingPlus;
                Dust dust = Dust.NewDustPerfect(center + offset, dustType, offset.SafeNormalize(Vector2.Zero) * 0.2f, 100, Color.White, 1.92f);
                dust.noGravity = true;
            }

            // **4. 顶部的茎**
            float stemHeight = radius * 0.6f; // 茎的高度
            for (int i = 0; i < smallParticles; i++)
            {
                Vector2 stemPosition = center + new Vector2(0f, -stemHeight * (i / (float)smallParticles));
                int dustType = DustID.RedTorch;
                Dust dust = Dust.NewDustPerfect(stemPosition, dustType, Vector2.UnitY * -0.2f, 100, Color.Brown, 1.90f);
                dust.noGravity = true;
            }

            // **5. 顶部的叶子**
            float leafOffset = radius * 0.3f; // 叶子延伸的距离
            for (int i = 0; i < smallParticles; i++)
            {
                Vector2 leafPosition = center + new Vector2((i % 2 == 0 ? -1 : 1) * leafOffset, -stemHeight * 1.1f);
                int dustType = DustID.HealingPlus;
                Dust dust = Dust.NewDustPerfect(leafPosition, dustType, Vector2.UnitY * -0.2f, 100, Color.Green, 1.90f);
                dust.noGravity = true;
            }
        }


    }
}
