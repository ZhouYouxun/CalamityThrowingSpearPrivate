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

            // 100% 概率召唤 GildedProboscisJavBIRD
            if (Main.rand.NextFloat() < 1f && Projectile.owner == Main.myPlayer)
            {
                // 最多只能同时存在21只小鸟
                int currentBirdCount = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<GildedProboscisJavBIRD>());
                if (phase == 1 && currentBirdCount < 50)
                {
                    Vector2 spawnPosition = Main.player[Projectile.owner].Center + Main.rand.NextVector2Circular(80f, 80f); // 随机位置
                    int birdDamage = (int)(Projectile.damage * 1); // 使用本体的伤害作为小鸟的伤害 * 0.415
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, Vector2.Zero, ModContent.ProjectileType<GildedProboscisJavBIRD>(), birdDamage, 0f, Projectile.owner);
                  
                }
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
            // 获取当前小鸟的数量
            int currentBirdCount = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<GildedProboscisJavBIRD>());

            if (currentBirdCount < 50)
            {
                // 粒子特效生成（前后方向）
                for (int i = 0; i < 6; i++) // 生成6对尖刺粒子
                {
                    // 生成反方向的粒子
                    Vector2 particleVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * -0.5f; // 反方向并稍微散开
                    Vector2 position = Projectile.Center - Projectile.velocity + particleVelocity;
                    PointParticle spark = new PointParticle(position, particleVelocity, false, 7, 1.5f, Color.OrangeRed); // 增大粒子尺寸，颜色为橙红色
                    GeneralParticleHandler.SpawnParticle(spark);

                    // 生成正方向的粒子
                    particleVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * 0.5f;
                    position = Projectile.Center + Projectile.velocity + particleVelocity;
                    spark = new PointParticle(position, particleVelocity, false, 7, 1.5f, Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            else
            {
                // 粒子特效生成（前后左右四个方向）
                for (int i = 0; i < 6; i++) // 生成6对尖刺粒子
                {
                    // 生成反方向的粒子
                    Vector2 particleVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * -0.5f; // 反方向并稍微散开
                    Vector2 position = Projectile.Center - Projectile.velocity + particleVelocity;
                    PointParticle spark = new PointParticle(position, particleVelocity, false, 7, 1.5f, Color.OrangeRed); // 增大粒子尺寸，颜色为橙红色
                    GeneralParticleHandler.SpawnParticle(spark);

                    // 生成正方向的粒子
                    particleVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * 0.5f;
                    position = Projectile.Center + Projectile.velocity + particleVelocity;
                    spark = new PointParticle(position, particleVelocity, false, 7, 1.5f, Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark);

                    // 生成左方向的粒子
                    particleVelocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2).RotatedByRandom(MathHelper.ToRadians(30)) * 0.5f; // 左侧方向
                    position = Projectile.Center + particleVelocity;
                    spark = new PointParticle(position, particleVelocity, false, 7, 1.5f, Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark);

                    // 生成右方向的粒子
                    particleVelocity = Projectile.velocity.RotatedBy(-MathHelper.PiOver2).RotatedByRandom(MathHelper.ToRadians(30)) * 0.5f; // 右侧方向
                    position = Projectile.Center + particleVelocity;
                    spark = new PointParticle(position, particleVelocity, false, 7, 1.5f, Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }


            // 获取当前小鸟的数量
            //int currentBirdCount = Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<GildedProboscisJavBIRD>());

            // 设置特效颜色和扩散范围
            Color baseColor = Color.Red;
            float sideLength = (currentBirdCount < 50) ? 10f : 25f; // 正常状态边长10，增强状态边长25
            int particlesPerSide = (currentBirdCount < 50) ? 10 : 25; // 每条边的粒子数量
            float expansionRate = (currentBirdCount < 50) ? 5f : 8f; // 正常状态和增强状态的扩散速度

            // 计算总粒子数量
            int totalParticles = (int)(particlesPerSide * 4); // 正方形四条边

            // 绘制正方形
            for (int i = 0; i < totalParticles; i++)
            {
                // 确定粒子在正方形的哪个边上
                float progress = (i / (float)particlesPerSide) % 4; // 每条边的进度
                Vector2 position;

                if (progress < 1f) // 顶边
                {
                    position = Projectile.Center + new Vector2(-sideLength / 2 + progress * sideLength, -sideLength / 2);
                }
                else if (progress < 2f) // 右边
                {
                    position = Projectile.Center + new Vector2(sideLength / 2, -sideLength / 2 + (progress - 1f) * sideLength);
                }
                else if (progress < 3f) // 底边
                {
                    position = Projectile.Center + new Vector2(sideLength / 2 - (progress - 2f) * sideLength, sideLength / 2);
                }
                else // 左边
                {
                    position = Projectile.Center + new Vector2(-sideLength / 2, sideLength / 2 - (progress - 3f) * sideLength);
                }

                // 计算粒子速度
                Vector2 velocity = (position - Projectile.Center).SafeNormalize(Vector2.Zero) * expansionRate;

                // 生成粒子
                GlowOrbParticle squareParticle = new GlowOrbParticle(
                    position, velocity, false, 5, 0.7f, baseColor, true, true
                );
                GeneralParticleHandler.SpawnParticle(squareParticle);
            }
        }
    }
}
