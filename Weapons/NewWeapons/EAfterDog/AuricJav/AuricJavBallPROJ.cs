using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Projectiles.DraedonsArsenal;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav
{
    public class AuricJavBallPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public ref float Time => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 3;  // 穿透次数为-1
            Projectile.tileCollide = false;
            Projectile.timeLeft = 45;  // 存活时间120
            Projectile.light = 0.5f;
            Projectile.extraUpdates = 2;  // 更多帧更新
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = -1; // 无敌帧冷却时间为14帧
        }
        float waveAmplitude = 8f;     // 粒子偏移的最大高度
        float waveFrequency = 0.2f;   // 波动频率（越大波动越快）

        public override void AI()
        {
            if (Time > 10)
            {
                // 飞行中持续生成金色 & 蓝色 Dust
                int[] yellowDust = { 244, 246, 228, 269 }; // 黄色电感类
                int[] blueDust = { 230, 226, 187 };        // 蓝色电能类

                // 在飞行过程中逐渐变透明和加速
                Projectile.alpha += 5;
                Projectile.velocity *= 1.05f;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;






                // === 金色推进火花（GlowSparkParticle 推进尾焰） ===
                // 每 2 帧释放一次，形成连续直线尾迹
                if (Time % 2 == 0) // 控制粒子生成频率：数值越小粒子越密集
                {
                    Particle spark = new GlowSparkParticle(
                        Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-1.5f, -0.5f),
                        // 生成位置：在弹幕后方一点点位置生成
                        // 数值越小越靠近弹幕尾部

                        -Projectile.velocity * 0.35f,
                        // 粒子飞行速度：沿着“反方向速度”
                        // 数值越大尾焰越长

                        false,
                        // 是否受重力（false = 不受重力）

                        8,
                        // 粒子生命周期（帧数）
                        // 越大尾焰越长

                        0.09f,
                        // 粒子基础大小
                        // 越大火花越粗

                        new Color(255, 210, 80) * 0.75f,
                        // 粒子颜色（金黄色）
                        // 可以改为：
                        // Color.Gold
                        // Color.Goldenrod
                        // Color.Yellow

                        new Vector2(1.2f, 0.35f),
                        // 粒子形状比例
                        // X = 长度
                        // Y = 厚度
                        // X大 → 细长推进尾焰

                        true,
                        // 是否有发光效果

                        false,
                        // 是否旋转

                        1
                    // 图层深度
                    );

                    GeneralParticleHandler.SpawnParticle(spark);
                }


                // === 示波器双轨迹粒子（Oscilloscope Dual Trace） ===

                // 波形最大振幅（控制线条离中心多远）
                // 建议 4~10 之间，越大越散
                float waveAmplitude = 6f;

                // 波形频率（控制波形密度）
                // 越大波越密
                float waveFrequency = 0.35f;

                // 沿飞行方向的单位向量
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

                // 垂直方向（示波器波形上下摆动的方向）
                Vector2 normal = forward.RotatedBy(MathHelper.Pi / 2f);

                // 每帧生成几个点（决定线条密度）
                int pointsPerFrame = 3;

                for (int i = 0; i < pointsPerFrame; i++)
                {
                    // 时间偏移，用于制造连续扫描线效果
                    float t = Time + i * 0.35f;

                    // === 黄色示波器线 ===
                    float yellowWave = MathF.Sin(t * waveFrequency) * waveAmplitude;

                    Vector2 yellowPos =
                        Projectile.Center
                        + normal * yellowWave          // 上下波动
                        - forward * (i * 4f);          // 向后拖尾形成连续扫描

                    Dust yellow = Dust.NewDustPerfect(
                        yellowPos,
                        269,                           // 金色电能Dust
                        Vector2.Zero,
                        100,
                        Color.Gold,
                        0.9f                           // 粒子大小
                    );

                    yellow.noGravity = true;
                    yellow.fadeIn = 0.5f;



                    // === 蓝色示波器线 ===
                    float blueWave = MathF.Sin(t * waveFrequency + MathHelper.Pi) * waveAmplitude;

                    Vector2 bluePos =
                        Projectile.Center
                        + normal * blueWave
                        - forward * (i * 4f);

                    Dust blue = Dust.NewDustPerfect(
                        bluePos,
                        226,                           // 蓝色能量Dust
                        Vector2.Zero,
                        100,
                        Color.Cyan,
                        0.9f
                    );

                    blue.noGravity = true;
                    blue.fadeIn = 0.5f;
                }




            }




            Time ++;
        }


        public override bool? CanDamage() => Time >= 13f; // 初始的时候不会造成伤害，直到x为止

        private bool ProjectileWithinScreen()
        {
            Vector2 screenPosition = Main.screenPosition;
            return Projectile.position.X > screenPosition.X && Projectile.position.X < screenPosition.X + Main.screenWidth
                   && Projectile.position.Y > screenPosition.Y && Projectile.position.Y < screenPosition.Y + Main.screenHeight;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() * 0.5f;
            Color color = new Color(255, 215, 0); // 金黄色
            Main.EntitySpriteDraw(texture, drawPosition, null, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            return false;  
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
            target.AddBuff(ModContent.BuffType<GalvanicCorrosion>(), 300); // 电偶腐蚀
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300); // 神圣之火
        }


        public override void OnKill(int timeLeft)
        {
            // 生成金色旋转粒子特效
            int particleCount = 10;
            for (int i = 0; i < particleCount; i++)
            {
                // 粒子的扩散角度为正前方左右随机范围内
                float angle = Projectile.velocity.ToRotation() + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                Vector2 particleVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 4f);

                // 创建金色粒子
                int dustType = DustID.GoldCoin;
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, particleVelocity.X, particleVelocity.Y);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].scale = 1.2f;
                Main.dust[dustIndex].fadeIn = 1.5f;
            }



            {
                // 粒子特效（跟前面相同）
                int[] yellowDust = new int[] { 244, 246, 228, 269 };
                int[] blueDust = new int[] { 230, 226, 187 };

                int explosionDustCount = 40;
                for (int i = 0; i < explosionDustCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float speed = Main.rand.NextFloat(2f, 6f);
                    Vector2 velocity = angle.ToRotationVector2() * speed;

                    // 混合金色、蓝色电光
                    int dustType = Main.rand.NextBool() ? Main.rand.Next(yellowDust) : Main.rand.Next(blueDust);
                    Color dustColor = Main.rand.NextBool(2) ? Color.Goldenrod : Color.LightBlue;

                    Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, velocity, 150, dustColor, Main.rand.NextFloat(1.2f, 1.8f));
                    d.noGravity = true;
                    d.fadeIn = 1.4f;
                }

                //// ⚡额外生成电光弧线 + 中心爆点
                //for (int k = 0; k < 3; k++)
                //{
                //    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                //    Vector2 direction = angle.ToRotationVector2();
                //    Vector2 pos = Projectile.Center + direction * Main.rand.NextFloat(4f, 12f);
                //    Particle bolt = new CrackParticle(
                //        pos,
                //        direction * Main.rand.NextFloat(1f, 3f),
                //        Color.White * 0.8f,
                //        Vector2.One * Main.rand.NextFloat(0.8f, 1.4f),
                //        0, 0,
                //        1f,
                //        14
                //    );
                //    GeneralParticleHandler.SpawnParticle(bolt);
                //}

                // 💫生成一个白色脉冲收缩波作为中心爆炸感
                Particle pulse2 = new DirectionalPulseRing(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.White,
                    new Vector2(1f, 1f),
                    10f,
                    0.15f,
                    0.5f,
                    14
                );
                GeneralParticleHandler.SpawnParticle(pulse2);
            }
        }


    }
}