using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Projectiles.Magic;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC
{
    public class GildedProboscisJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/GildedProboscisC/GildedProboscisJav";

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
        private bool hasBounced = false; // 记录是否已经反弹过一次
        private float dustAngle = 0f; // 控制粒子生成的弯曲角度
        private bool growing = true;  // 控制 dustAngle 的增减方向
        private float variance = 1f;  // 控制角度变化的随机幅度

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 调用之前的拖尾效果绘制
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            // 添加刀刃亮光效果
            Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            Vector2 shineScale = new Vector2(1.67f, 3f) * Projectile.scale;
            shineScale *= MathHelper.Lerp(0.9f, 1.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.4f + Projectile.identity) * 0.5f + 0.5f);

            // 设置亮光的位置为弹幕的中心
            Vector2 lensFlareWorldPosition = Projectile.Center; // 移除偏移，直接使用弹幕中心

            // 亮光颜色为红色和橙色渐变
            Color lensFlareColor = Color.Lerp(Color.Red, Color.Orange, 0.23f) with { A = 0 };
            Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, 0f, shineTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
            Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, MathHelper.PiOver2, shineTex.Size() * 0.5f, shineScale, 0, 0);

            return false;
        }




        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 7; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 原有逻辑
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);
            //Projectile.velocity *= 1.005f;


            // 控制 dustAngle 的增减
            if (dustAngle <= -0.5f)
            {
                growing = true;
            }
            if (dustAngle >= 0.5f)
            {
                growing = false;
            }
            dustAngle += (growing ? 0.07f * variance : -0.07f * variance);

            // 粒子生成逻辑
            if (Projectile.localAI[0] > 12f && Projectile.Distance(Main.player[Projectile.owner].Center) < 1200)
            {
                // 创建红色的 GlowOrbParticle 粒子
                GlowOrbParticle orb = new GlowOrbParticle(
                    (Projectile.Center + Projectile.velocity.RotatedBy(dustAngle) * 4.5f) - Projectile.velocity * 5,
                    Vector2.Zero, false, 5, 0.55f + MathF.Abs(dustAngle * 0.5f),
                    Color.Red, true, true
                );
                GeneralParticleHandler.SpawnParticle(orb);

                // 创建红色的 PointParticle 粒子（火花）
                PointParticle spark = new PointParticle(
                    Projectile.Center + Projectile.velocity * 3.5f,
                    Projectile.velocity, false, 2, 0.6f, Color.Red
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            Projectile.localAI[0]++;
        }

        private int phase = 1;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果

            int currentBirdCount = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<GildedProboscisJavBIRD>());

            if (Projectile.owner == Main.myPlayer && currentBirdCount < 21)
            {
                // 使用当前伤害 * 0.415 作为小鸟伤害基准
                int birdDamage = (int)(Projectile.damage * 0.415f);

                // 在命中位置生成 GildedProboscisJavINV，承担攻击传递
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero, // INV 弹幕初速可自行在 AI 内设定
                    ModContent.ProjectileType<GildedProboscisJavINV>(),
                    birdDamage,
                    0f,
                    Projectile.owner
                );
            }
        }


        //public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        //{
        // 检查当前小鸟数量
        //int currentBirdCount = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<GildedProboscisJavBIRD>());
        //if (currentBirdCount >= 21)
        //{
        // 小鸟数量达到上限时，增加1.5倍伤害
        //modifiers.FinalDamage *= 1.5f;
        //int birdDamage = (int)(Projectile.damage * 2);
        //}
        //}

        public override void OnKill(int timeLeft)
        {

            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/拉链闪电"), Projectile.position);



            {
                // === GildedProboscisJav 命中特效（完全体整合：有序正方形 GlowOrb + 无序 Spark/Dust + 尖刺） ===

                // 获取当前小鸟的数量
                int currentBirdCount = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<GildedProboscisJavBIRD>());

                // 基础参数
                Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                float speedBase = Projectile.velocity.Length();
                Color sparkColor = Color.Gold;
                Color squareColor = Color.Lerp(Color.Yellow, Color.Gold, 0.5f);
                Color dustColor = Color.Gold;

                // === 1️⃣ 有序正方形 GlowOrbParticle ===
                Color glowColor = Color.Lerp(Color.Gold, Color.White, 0.3f);
                float sideLength = (currentBirdCount < 50) ? 14f : 28f;
                int particlesPerSide = (currentBirdCount < 50) ? 10 : 18;
                float expansionRate = (currentBirdCount < 50) ? 4f : 7f;
                int totalParticles = particlesPerSide * 4;

                for (int i = 0; i < totalParticles; i++)
                {
                    float progress = (i / (float)particlesPerSide) % 4f;
                    Vector2 position;

                    if (progress < 1f) // 顶边
                    {
                        position = Projectile.Center + new Vector2(-sideLength / 2f + progress * sideLength, -sideLength / 2f);
                    }
                    else if (progress < 2f) // 右边
                    {
                        position = Projectile.Center + new Vector2(sideLength / 2f, -sideLength / 2f + (progress - 1f) * sideLength);
                    }
                    else if (progress < 3f) // 底边
                    {
                        position = Projectile.Center + new Vector2(sideLength / 2f - (progress - 2f) * sideLength, sideLength / 2f);
                    }
                    else // 左边
                    {
                        position = Projectile.Center + new Vector2(-sideLength / 2f, sideLength / 2f - (progress - 3f) * sideLength);
                    }

                    Vector2 velocity = (position - Projectile.Center).SafeNormalize(Vector2.Zero) * expansionRate * Main.rand.NextFloat(0.8f, 1.2f);

                    GlowOrbParticle glow = new GlowOrbParticle(
                        position,
                        velocity,
                        false,
                        18,
                        Main.rand.NextFloat(0.6f, 0.9f),
                        glowColor,
                        true,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(glow);
                }

                // === 2️⃣ 尖刺 PointParticle + SparkParticle + Dust 无序流动 ===
                int scatterCount = (currentBirdCount < 50) ? 12 : 20;
                for (int i = 0; i < scatterCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dir = angle.ToRotationVector2();
                    Vector2 spawnPos = Projectile.Center + dir * Main.rand.NextFloat(4f, 12f);
                    Vector2 velocity = dir * Main.rand.NextFloat(3f, 7f);

                    // PointParticle 尖刺（电击碎片）
                    if (Main.rand.NextBool(2))
                    {
                        PointParticle spike = new PointParticle(
                            spawnPos,
                            velocity,
                            false,
                            12,
                            1.2f,
                            sparkColor
                        );
                        GeneralParticleHandler.SpawnParticle(spike);
                    }

                    // SparkParticle （电火花流动）
                    if (Main.rand.NextBool(2))
                    {
                        Particle spark = new SparkParticle(
                            spawnPos,
                            velocity,
                            false,
                            16,
                            0.9f,
                            sparkColor
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    // 金色 Dust
                    if (Main.rand.NextBool(2))
                    {
                        Dust dust = Dust.NewDustPerfect(
                            spawnPos,
                            DustID.GoldFlame,
                            velocity * 0.4f,
                            100,
                            dustColor,
                            Main.rand.NextFloat(0.8f, 1.3f)
                        );
                        dust.noGravity = true;
                    }
                }

            }










        }







    }
}
