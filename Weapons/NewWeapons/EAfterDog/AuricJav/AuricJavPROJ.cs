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
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using Terraria.Audio;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav
{
    public class AuricJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/AuricJav/AuricJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 允许1次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;



            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(255, 215, 0);
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            //// 在飞行路径上留下白色的重型烟雾粒子
            //if (Main.rand.NextBool(3)) // 每3帧生成一次粒子
            //{
            //    Color smokeColor = Color.Lerp(Color.Yellow, Color.Gold, 0.5f);
            //    Particle smoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f, smokeColor, 30, Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f), 1.0f, MathHelper.ToRadians(2f), required: true);
            //    GeneralParticleHandler.SpawnParticle(smoke);
            //}

        }

        


        // 生成六边形法阵和尖锥特效的函数
        private void ProduceHexagonEffect(Vector2 position)
        {
            int particleCount = 3; // 只生成三发粒子

            for (int i = 0; i < particleCount; i++)
            {
                // 随机角度，约为四面八方范围内
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 particleVelocity = Vector2.UnitX.RotatedBy(randomAngle) * 4f;

                // 创建并生成淡黄色的 CrackParticle 闪电特效
                float randomScale = Main.rand.NextFloat(0.7f, 1.2f); // 随机缩放
                Particle bolt = new CrackParticle(
                    position,
                    particleVelocity,
                    Color.LightYellow * 0.65f, // 淡黄色
                    Vector2.One * randomScale,
                    0,
                    0,
                    randomScale,
                    11
                );
                GeneralParticleHandler.SpawnParticle(bolt);
            }
        }


        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // 获取弹幕中心位置
            Vector2 spawnPosition = Projectile.Center;
            ProduceHexagonEffect(spawnPosition); // 释放小阵法
            
            
            // 生成金黄色 Dust 粒子特效
            for (int i = 0; i < 30; i++)
            {
                // 生成弯折扩散的粒子效果
                float angle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机角度
                float distance = Main.rand.NextFloat(10f, 50f); // 随机半径，用于生成环状扩散效果
                Vector2 dustPosition = spawnPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;

                Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Electric, Vector2.Zero, 150, Color.Goldenrod, 1.5f);
                dust.velocity = Vector2.UnitY.RotatedBy(angle) * Main.rand.NextFloat(1f, 3f); // 设置粒子的速度和方向
                dust.noGravity = true; // 粒子无重力效果
                dust.fadeIn = 1.2f; // 粒子淡入效果
            }

            //// 1. 生成较小的橙黄色和淡黄色爆炸特效（超新星的那个光圈逐渐缩小的特效）
            //Color orangeColor = Color.Orange;
            //Color lightYellowColor = Color.LightYellow;
            //float smallerScale = 1.5f; // 较小的扩散大小
            //float rotationSpeed = Main.rand.NextFloat(-10f, 10f); // 随机旋转速度

            //// 创建两个爆炸粒子，颜色为橙黄色和淡黄色
            //Particle orangeExplosion = new CustomPulse(spawnPosition, Vector2.Zero, orangeColor, "CalamityMod/Particles/LargeBloom", new Vector2(0.8f, 0.8f), rotationSpeed, smallerScale, smallerScale - 0.5f, 15);
            //Particle yellowExplosion = new CustomPulse(spawnPosition, Vector2.Zero, lightYellowColor, "CalamityMod/Particles/LargeBloom", new Vector2(0.8f, 0.8f), -rotationSpeed, smallerScale, smallerScale - 0.5f, 15);

            //GeneralParticleHandler.SpawnParticle(orangeExplosion);
            //GeneralParticleHandler.SpawnParticle(yellowExplosion);




            //// 定义左右方向的速度
            //Vector2 leftDirection = new Vector2(-1, 0);  // 绝对左方
            //Vector2 rightDirection = new Vector2(1, 0);  // 绝对右方

            //// 定义慢速和快速的速度倍率
            //float slowSpeed = 5f;  // 慢速初始速度
            //float fastSpeed = slowSpeed * 1.3f;  // 快速弹幕的初始速度是慢速的1.3倍

            //// 生成两个向左的弹幕（一个快，一个慢）
            //Projectile.NewProjectile(
            //    Projectile.GetSource_FromThis(),
            //    spawnPosition + new Vector2(0, -6 * 16),  // 向上6*16像素
            //    leftDirection * fastSpeed,  // 快速向左
            //    ModContent.ProjectileType<AuricJavBallPROJ>(),
            //    (int)(Projectile.damage * 2.1f),
            //    Projectile.knockBack,
            //    Projectile.owner
            //);

            //Projectile.NewProjectile(
            //    Projectile.GetSource_FromThis(),
            //    spawnPosition + new Vector2(0, 6),  // 向下6像素
            //    leftDirection * slowSpeed,  // 慢速向左
            //    ModContent.ProjectileType<AuricJavBallPROJ>(),
            //    (int)(Projectile.damage * 2.1f),
            //    Projectile.knockBack,
            //    Projectile.owner
            //);

            //// 生成两个向右的弹幕（一个快，一个慢）
            //Projectile.NewProjectile(
            //    Projectile.GetSource_FromThis(),
            //    spawnPosition + new Vector2(0, -6 * 16),  // 向上6*16像素
            //    rightDirection * fastSpeed,  // 快速向右
            //    ModContent.ProjectileType<AuricJavBallPROJ>(),
            //    (int)(Projectile.damage * 2.1f),
            //    Projectile.knockBack,
            //    Projectile.owner
            //);

            //Projectile.NewProjectile(
            //    Projectile.GetSource_FromThis(),
            //    spawnPosition + new Vector2(0, 6),  // 向下6像素
            //    rightDirection * slowSpeed,  // 慢速向右
            //    ModContent.ProjectileType<AuricJavBallPROJ>(),
            //    (int)(Projectile.damage * 2.1f),
            //    Projectile.knockBack,
            //    Projectile.owner
            //);



            // 定义左右方向的速度
            Vector2[] directions = { new Vector2(-1, 0), new Vector2(1, 0) }; // 左方和右方方向
            float[] speeds = { 5f, 5f * 1.3f }; // 慢速和快速的速度倍率

            // 定义生成位置偏移：原位置、下方 6 × 16 像素、上方 6 × 16 像素
            Vector2[] positions = { Vector2.Zero, new Vector2(0, 6 * 16), new Vector2(0, -6 * 16) };

            // 生成弹幕
            foreach (var positionOffset in positions)
            {
                foreach (var direction in directions)
                {
                    foreach (var speed in speeds)
                    {
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            spawnPosition + positionOffset, // 根据位置偏移生成弹幕
                            direction * speed,              // 速度和方向
                            ModContent.ProjectileType<AuricJavBallPROJ>(),
                            (int)(Projectile.damage * 3.25f),
                            Projectile.knockBack,
                            Projectile.owner
                        );
                    }
                }
            }




        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
            target.AddBuff(ModContent.BuffType<GalvanicCorrosion>(), 300); // 电偶腐蚀
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300); // 神圣之火
        }
    }
}
