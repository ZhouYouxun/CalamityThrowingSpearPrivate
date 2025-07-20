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
            Projectile.timeLeft = 40;  // 存活时间120
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



                // === AltSpark 作为电流中心核心 ===
                AltSparkParticle centerSpark = new AltSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), // 轻微漂移
                    Projectile.velocity.SafeNormalize(Vector2.UnitX) * 0.3f + Main.rand.NextVector2Circular(0.1f, 0.1f), // 缓慢向前
                    false,
                    Main.rand.Next(14, 20), // 生命周期较短
                    Main.rand.NextFloat(1.0f, 1.3f),
                    Color.LightGoldenrodYellow * 0.45f // 柔金色调
                );
                GeneralParticleHandler.SpawnParticle(centerSpark);


                // === 干涉波轨迹粒子 ===
                float waveAmplitude1 = 24f;
                float waveAmplitude2 = 12f;
                float waveFreq1 = 0.52f;
                float waveFreq2 = 1.15f;

                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 normal = forward.RotatedBy(MathHelper.PiOver2);

                int stepsPerFrame = 2;
                for (int i = 0; i < stepsPerFrame; i++)
                {
                    float t = Time + i / (float)stepsPerFrame;

                    // ✅ 双波干涉轨迹偏移（模拟示波器）
                    float wave1 = MathF.Sin(t * waveFreq1) * waveAmplitude1;
                    float wave2 = MathF.Sin(t * waveFreq2 + MathF.Cos(t * 0.3f) * 0.5f) * waveAmplitude2;
                    float offsetY = wave1 + wave2;

                    Vector2 spawnPos = Projectile.Center + normal * offsetY;

                    int dustType = Main.rand.NextBool(2)
                        ? yellowDust[Main.rand.Next(yellowDust.Length)]
                        : blueDust[Main.rand.Next(blueDust.Length)];

                    int idx = Dust.NewDust(spawnPos, 0, 0, dustType, 0f, 0f, 100, Color.White, 0.8f);
                    Main.dust[idx].noGravity = true;
                    Main.dust[idx].fadeIn = 1.2f;
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