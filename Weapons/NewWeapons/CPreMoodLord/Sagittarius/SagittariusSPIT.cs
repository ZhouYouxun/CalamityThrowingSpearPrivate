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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius
{
    public class SagittariusSPIT : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Sagittarius/Sagittarius";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";

        // 保存光斑数据的列表
        private List<(Vector2 Position, float Opacity, float Rotation)> sparkleData = new List<(Vector2, float, float)>();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 200; // 只允许200次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 能够穿透方块
            Projectile.extraUpdates = 6; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧

            // 初始速度设置为充能长枪速度的x%
            Projectile.velocity *= 3.00f;
        }

        public override void AI()
        {
            // 加速效果，每帧速度乘以1.01
            Projectile.velocity *= 1.01f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 0.55f);

            // 造成了一次伤害之后就直接关闭伤害检测并快速降低速度，并让自己停下来
            if (Projectile.penetrate < 200)
            {
                if (Projectile.timeLeft > 60) { Projectile.timeLeft = 60; } //The projectile start shrinking and slowing down. it can still hit for a bit during this, to allow a bit of multi-target if the enemies are really close to eachother.
                Projectile.velocity *= 0.88f;
            }

            // 小型冲击波生成，两个一大一小
            if (Projectile.timeLeft == 600)
            {
                Vector2 smallPulseScale = new Vector2(0.3f, 1.2f); // 小型冲击波
                Vector2 largePulseScale = new Vector2(0.6f, 1.6f); // 大型冲击波

                // 第一个小型垂直椭圆冲击波
                Particle smallPulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.White, smallPulseScale, MathHelper.PiOver2, 0.3f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(smallPulse);

                // 第二个大型垂直椭圆冲击波
                Particle largePulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.White, largePulseScale, MathHelper.PiOver2, 0.2f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(largePulse);
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 保留原版粒子效果
            for (int i = 0; i < 10; i++)
            {
                Vector2 offset = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15));
                Dust.NewDustPerfect(Projectile.Center, 57, offset * 0.5f, 150, Color.Yellow, 1.2f).noGravity = true;
            }

            // 亮黄色闪光点效果
            for (int i = 0; i < 20; i++)
            {
                if (Main.rand.NextFloat() < 0.7f) // 70% 概率生成新特效
                {
                    Color particleColor = Color.LightYellow;
                    float particleScale = 0.35f;
                    Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f); // 扩散到周围随机位置
                    Vector2 particleVelocity = Main.rand.NextVector2Circular(29f, 29f); // 扩散速度（这个需要快一点）

                    GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, particleVelocity, particleColor, particleScale, Main.rand.Next(20) + 10));
                }
                else // 30% 概率生成原有特效
                {
                    Vector2 offset = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(1.5f, 3f); // 调整速度范围
                    Color startColor = Color.Gold * 0.6f;
                    Color endColor = Color.LightGoldenrodYellow * 1.0f;

                    SparkleParticle spark = new SparkleParticle(Projectile.Center, offset, startColor, endColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(10, 20), Main.rand.NextFloat(-8, 8), 0.3f, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

        }



        public override bool PreDraw(ref Color lightColor)
        {
            // 获取 SpriteBatch 和投射物纹理
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Sagittarius/Sagittarius").Value;

            // 遍历投射物的旧位置数组，绘制光学拖尾效果
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 计算颜色插值值，使颜色在旧位置之间平滑过渡
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;

                // 使用金黄色~浅黄色渐变
                Color color = Color.Lerp(Color.Orange, Color.OrangeRed, colorInterpolation) * 0.4f;
                color.A = 0;

                // 计算绘制位置，将位置调整到碰撞箱的中心
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                // 计算外部和内部的颜色
                Color outerColor = color;
                Color innerColor = color * 0.5f;

                // 计算强度，使拖尾逐渐变弱
                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60)
                {
                    intensity *= Projectile.timeLeft / 60f; // 如果弹幕即将消失，则拖尾也逐渐消失
                }

                // 计算外部和内部的缩放比例，使拖尾具有渐变效果
                Vector2 outerScale = new Vector2(2f) * intensity;
                Vector2 innerScale = new Vector2(2f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;

                // 绘制外部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, Projectile.rotation, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);

                // 绘制内部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, Projectile.rotation, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }

            // 如果需要绘制弹幕主体，取消注释以下代码
            //Main.EntitySpriteDraw(lightTexture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, lightColor, Projectile.rotation, lightTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}