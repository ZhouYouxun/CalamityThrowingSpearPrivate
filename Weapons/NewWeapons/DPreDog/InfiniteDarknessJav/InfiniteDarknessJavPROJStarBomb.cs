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
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;

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
        internal Color ColorFunction(float completionRatio)
        {
            // 末端渐隐效果
            float fadeToEnd = MathHelper.Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

            // 不透明度随位置变化
            float fadeOpacity = Utils.GetLerpValue(1f, 0.64f, completionRatio, true) * Projectile.Opacity;

            // 拖尾颜色动态渐变（从深灰色到黑色）
            Color colorHue = Color.Lerp(Color.DarkSlateGray, Color.Black, 0.5f); // 设置主色调为深灰色和黑色的中间值
            Color endColor = Color.Lerp(colorHue, Color.DarkGray, (float)Math.Sin(completionRatio * MathHelper.Pi * 1.6f - Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);

            return Color.Lerp(Color.Black, endColor, fadeToEnd) * fadeOpacity;
        }
        internal float WidthFunction(float completionRatio)
        {
            // 拖尾宽度动态衰减
            float expansionCompletion = (float)Math.Pow(1 - completionRatio, 3);
            return MathHelper.Lerp(0f, 22 * Projectile.scale * Projectile.Opacity, expansionCompletion);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 贴图绘制
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            // 拖尾特效绘制
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]),
                30
            );

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

            // 减速逻辑
            if (time <= 60)
            {
                Projectile.velocity *= 0.98f; // 每帧减速
            }
            else if (!isApproaching) // 追踪逻辑
            {
                NPC target = Projectile.Center.ClosestNPCAt(1000); // 检查 1000 范围内的敌人
                if (target != null)
                {
                    isApproaching = true;
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 10f; // 初始冲刺速度
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

            // 生成粒子特效
            int particleCount = Main.rand.Next(20, 56); // 粒子数量随机在20~55之间
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(50f, 50f); // 在半径50的圆内随机生成
                Vector2 velocity = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f); // 随机扩散速度
                int dustType = Main.rand.Next(new int[] { 175, 191 }); // 随机粒子类型

                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, dustType, velocity, 100, default, Main.rand.NextFloat(1.5f, 1.9f));
                dust.noGravity = true; // 禁用重力
            }
        }



        public override bool? CanDamage() => time >= 12;

    }
}

