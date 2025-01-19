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

            // 使用顶端位置作为基准
            Vector2 tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1); // 使用顶端位置

            // 在弹幕顶端生成 SparkParticle 特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(255, 215, 0);
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(tipPosition, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 在弹幕顶端生成两排平行的 AltSparkParticle
            for (int i = -12; i <= 12; i += 24) // 偏移量相对于弹幕自身的朝向
            {
                // 基于弹幕旋转方向计算偏移
                Vector2 offset = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * i;

                // 粒子颜色
                Color particleColor = Color.Lerp(Color.Yellow, Color.LightYellow, Main.rand.NextFloat(0.5f, 1.0f)); // 亮黄色或金色

                // 创建并生成粒子
                AltSparkParticle spark = new AltSparkParticle(
                    tipPosition + offset,           // 偏移位置
                    Projectile.velocity * 0.5f,     // 粒子速度
                    false,
                    15,
                    1f,
                    particleColor
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }


            // 在弹幕顶端生成粒子特效
            tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);
            int dustType = Main.rand.Next(new int[] { DustID.YellowTorch, 19, 10 }); // 随机粒子类型
            for (int j = 0; j < Main.rand.Next(1, 3); j++) // 每帧向左右各发射 1~2 个粒子
            {
                Dust dust = Dust.NewDustPerfect(tipPosition, dustType, new Vector2(Main.rand.NextFloat(-2f, 2f), 0), 150, Color.Yellow, 1.5f);
                dust.noGravity = true;
            }
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

            //// 新增三条线特效
            //Vector2 center = Projectile.Center;
            //float initialAngle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机起始角度
            //for (int i = 0; i < 3; i++)
            //{
            //    float lineAngle = initialAngle + MathHelper.TwoPi / 3f * i; // 每条线间隔 120°
            //    for (int j = 0; j < 25; j++) // 每条线的粒子数
            //    {
            //        float distance = j * 5f; // 粒子之间的间距
            //        float widthOffset = Main.rand.NextFloat(-5f, 5f); // 增加宽度偏移，随机值扩展范围
            //        Vector2 position = center + new Vector2((float)Math.Cos(lineAngle), (float)Math.Sin(lineAngle)) * distance * 2.5f; // 线的长度增加 2.5 倍
            //        position += new Vector2((float)Math.Cos(lineAngle + MathHelper.PiOver2), (float)Math.Sin(lineAngle + MathHelper.PiOver2)) * widthOffset; // 宽度偏移

            //        int dustType = Main.rand.NextFloat() < 0.4f ? DustID.UltraBrightTorch : DustID.BlueFlare; // 混合粒子
            //        Dust dust = Dust.NewDustPerfect(position, dustType, Vector2.Zero, 150, Color.White, 1.5f);
            //        dust.noGravity = true;
            //        dust.velocity = Vector2.UnitY.RotatedBy(lineAngle) * Main.rand.NextFloat(1f, 3f); // 粒子向外扩散
            //    }
            //}


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
