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
using Terraria.DataStructures;

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
            Projectile.ArmorPenetration = 5;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧

            // 初始速度设置为充能长枪速度的x%
            Projectile.velocity *= 3.00f;
        }
        public override void OnSpawn(IEntitySource source)
        {


        }
        public override void AI()
        {
            // 在 AI 内加入一次性判断调用（只在出生时）
            if (Projectile.timeLeft == 600) // 初始时调用
            {
                CTSLightingBoltsSystem.Spawn_SagittariusSpitBirth(Projectile.Center);
            }

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
                Particle smallPulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.White, smallPulseScale, Projectile.velocity.ToRotation(), 0.3f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(smallPulse);

                // 第二个大型垂直椭圆冲击波
                Particle largePulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.White, largePulseScale, Projectile.velocity.ToRotation(), 0.2f, 1f, 30);
                GeneralParticleHandler.SpawnParticle(largePulse);
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            // 在死亡时触发屏幕震动
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = 5f;
            // 保留原版粒子效果
            for (int i = 0; i < 10; i++)
            {
                Vector2 offset = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15));
                Dust.NewDustPerfect(Projectile.Center, 57, offset * 0.5f, 150, Color.Yellow, 1.2f).noGravity = true;
            }

            //// 亮黄色闪光点效果
            //for (int i = 0; i < 20; i++)
            //{
            //    if (Main.rand.NextFloat() < 0.7f) // 70% 概率生成新特效
            //    {
            //        Color particleColor = Color.LightYellow;
            //        float particleScale = 0.35f;
            //        Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f); // 扩散到周围随机位置
            //        Vector2 particleVelocity = Main.rand.NextVector2Circular(29f, 29f); // 扩散速度（这个需要快一点）

            //        GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, particleVelocity, particleColor, particleScale, Main.rand.Next(20) + 10));
            //    }
            //    else // 30% 概率生成原有特效
            //    {
            //        Vector2 offset = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(1.5f, 3f); // 调整速度范围
            //        Color startColor = Color.Gold * 0.6f;
            //        Color endColor = Color.LightGoldenrodYellow * 1.0f;

            //        SparkleParticle spark = new SparkleParticle(Projectile.Center, offset, startColor, endColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(10, 20), Main.rand.NextFloat(-8, 8), 0.3f, false);
            //        GeneralParticleHandler.SpawnParticle(spark);
            //    }
            //}

            // ===== 前方扇形神圣扩散（替代原地释放）======
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            int count = 12;
            float spread = MathHelper.ToRadians(70f);

            // ========= ① 有序层（扇形骨架）=========
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                float angle = MathHelper.Lerp(-spread, spread, t);

                Vector2 dir = forward.RotatedBy(angle);

                // ❗初始位置略微前移（避免贴脸生成）
                Vector2 spawnPos = Projectile.Center + forward * 6f;

                // ❗主速度（扇形推进）
                Vector2 vel = dir * Main.rand.NextFloat(6f, 12f);

                // ===== 主Bloom（骨架）=====
                GeneralParticleHandler.SpawnParticle(new GenericBloom(
                    spawnPos,
                    vel,
                    Color.Lerp(Color.White, Color.LightGoldenrodYellow, 0.6f),
                    0.28f,
                    Main.rand.Next(16, 24)
                ));

                // ===== Sparkle（结构点）=====
                if (i % 2 == 0)
                {
                    SparkleParticle sparkle = new SparkleParticle(
                        spawnPos,
                        vel * 0.35f,
                        Color.White * 0.8f,
                        Color.Gold * 0.5f,
                        Main.rand.NextFloat(0.4f, 0.6f),
                        Main.rand.Next(16, 24),
                        Main.rand.NextFloat(-2f, 2f),
                        0.2f,
                        false
                    );
                    GeneralParticleHandler.SpawnParticle(sparkle);
                }
            }

            // ========= ② 无序层（逸散，但仍受方向约束）=========
            int chaoticCount = 10;

            for (int i = 0; i < chaoticCount; i++)
            {
                // ❗生成位置：略微前方范围
                Vector2 randPos = Projectile.Center + forward * Main.rand.NextFloat(4f, 12f);

                // ❗方向：仍然围绕forward，但更散
                Vector2 randVel =
                    forward.RotatedByRandom(MathHelper.ToRadians(55f)) *
                    Main.rand.NextFloat(4f, 10f);

                GeneralParticleHandler.SpawnParticle(new GenericBloom(
                    randPos,
                    randVel,
                    Color.LightYellow,
                    0.22f,
                    Main.rand.Next(12, 20)
                ));
            }


        }


        public override void OnKill(int timeLeft)
        {
            //for (int i = 0; i < 30; i++)
            //{
            //    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            //    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 6f);
            //    Color color = Main.rand.NextBool() ? Color.White : Color.LightYellow;

            //    SparkleParticle spark = new SparkleParticle(
            //        Projectile.Center,
            //        velocity,
            //        color * 0.6f,
            //        color * 0.2f,
            //        Main.rand.NextFloat(0.6f, 1.0f),
            //        Main.rand.Next(18, 30),
            //        Main.rand.NextFloat(-0.2f, 0.2f),
            //        0.2f,
            //        false
            //    );
            //    GeneralParticleHandler.SpawnParticle(spark);
            //}
            // ===== 五角星结构（替代原弧线）=====
            int starPoints = 5;           // 五角星
            int pointsPerEdge = 6;        // 每条边的点数
            float baseRadius = 60f;

            // 五角星外圈角度
            float rotationOffset = Main.GameUpdateCount * 0.03f;

            // ===== 计算五角星5个顶点 =====
            Vector2[] starVertices = new Vector2[starPoints];

            for (int i = 0; i < starPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / starPoints + rotationOffset - MathHelper.PiOver2;
                starVertices[i] = Projectile.Center + angle.ToRotationVector2() * baseRadius;
            }

            // ===== 五角星连接顺序（经典跳点连接：0→2→4→1→3）=====
            int[] order = { 0, 2, 4, 1, 3 };

            // ===== 画五角星边 =====
            for (int e = 0; e < starPoints; e++)
            {
                Vector2 start = starVertices[order[e]];
                Vector2 end = starVertices[order[(e + 1) % starPoints]];

                for (int i = 0; i < pointsPerEdge; i++)
                {
                    float t = (float)i / (pointsPerEdge - 1);

                    // 插值位置（边上）
                    Vector2 pos = Vector2.Lerp(start, end, t);

                    // 方向（用于微扰）
                    Vector2 dir = (end - start).SafeNormalize(Vector2.UnitX);

                    // ===== Dust主结构 =====
                    Dust dust = Dust.NewDustPerfect(
                        pos,
                        267,
                        Vector2.Zero,
                        0,
                        Color.White,
                        1.3f
                    );
                    dust.noGravity = true;

                    //// ===== 内层能量（轻微内缩+横向流）=====
                    //if (Main.rand.NextBool(2))
                    //{
                    //    Vector2 innerPos = Vector2.Lerp(pos, Projectile.Center, 0.15f);

                    //    SparkleParticle spark = new SparkleParticle(
                    //        innerPos,
                    //        dir.RotatedBy(MathHelper.PiOver2) * 0.4f, // 横向流动
                    //        Color.LightYellow * 0.7f,
                    //        Color.White * 0.3f,
                    //        0.6f,
                    //        22,
                    //        Main.rand.NextFloat(-0.05f, 0.05f),
                    //        0.2f,
                    //        false
                    //    );

                    //    GeneralParticleHandler.SpawnParticle(spark);
                    //}
                }
            }

            // 金色冲击波
            Particle pulse = new DirectionalPulseRing(
                Projectile.Center,
                Vector2.Zero,
                Color.Goldenrod,
                new Vector2(1.6f, 1.6f),
                0f,
                0.5f,
                1.0f,
                30
            );
            GeneralParticleHandler.SpawnParticle(pulse);

            CTSLightingBoltsSystem.Spawn_SagittariusSpitDeath(Projectile.Center, Projectile.velocity);
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