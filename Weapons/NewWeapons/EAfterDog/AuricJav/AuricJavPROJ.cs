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

            // 如果弹幕时间小于 12 帧，不执行螺旋线段绘制
            if (Projectile.timeLeft > 588) // 600 - 12 = 588，确保弹幕出生后 12 帧才绘制
                return false;

            // 计算绘制起点
            Vector2 startPosition = Projectile.oldPos[0] + Projectile.Size * 0.5f; // 旧位置
            Vector2 previousEnd = startPosition; // 记录上一条线段的终点（初始为第一帧的位置）

            // 线段颜色变化参数
            float colorShift = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f;
            Color lineColor = Color.Lerp(Color.Gold, Color.White, colorShift); // 在金色和白色之间变化

            // 螺旋偏移参数
            float spiralOffset = 10f + Main.rand.NextFloat(-2f, 2f); // 让偏移量稍微随机
            bool spiralRight = true; // 控制左右交替
            float segmentLength = 16f + Main.rand.NextFloat(-2f, 2f); // 让线段长度有小幅随机变化

            // 遍历弹幕的旧位置，生成螺旋线段
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 currentPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f;

                // 计算螺旋偏移（在 -spiralOffset 和 +spiralOffset 之间波动）
                float offsetAmount = spiralOffset * (spiralRight ? 1f : -1f) + Main.rand.NextFloat(-2f, 2f); // 让每次偏移稍微不一样
                Vector2 offset = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * offsetAmount;

                Vector2 newEnd = currentPosition + offset; // 计算新的终点

                // 绘制线段
                Main.spriteBatch.DrawLineBetter(previousEnd, newEnd, lineColor, 2f);

                // 更新状态
                previousEnd = newEnd;
                spiralRight = !spiralRight; // 每次切换方向
            }

            return false;
        }
        private float spiralOffset = 12f; // 初始偏移量
        private bool spiralRight = true; // 方向控制
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



            //// 生成螺旋粒子
            //spiralOffset += (spiralRight ? 3.5f : -3.5f); // 每帧偏移
            //if (spiralOffset > 12f || spiralOffset < -12f)
            //    spiralRight = !spiralRight; // 当偏移到最大值时反向

            //Vector2 spiralOffsetVector = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * spiralOffset;

            //// 创建并生成粒子
            //AltSparkParticle spark2 = new AltSparkParticle(
            //    tipPosition + spiralOffsetVector, // 偏移位置
            //    Projectile.velocity * 0.5f, // 粒子速度
            //    false,
            //    15,
            //    1f,
            //    new Color(255, 215, 0)
            //);
            //GeneralParticleHandler.SpawnParticle(spark2);



            // 在弹幕顶端生成粒子特效
            tipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);
            int dustType = Main.rand.Next(new int[] { DustID.YellowTorch, 19, 10 }); // 随机粒子类型
            for (int j = 0; j < Main.rand.Next(1, 5); j++) // 每帧向左右各发射 1~X 个粒子
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
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/QuasarExploded"));

            //SoundStyle boom = new SoundStyle("CalamityThrowingSpear/Sound/QuasarExploded")
            //{
            //    Volume = 5.5f,
            //    Pitch = -0.05f
            //};

            //SoundEngine.PlaySound(boom, Projectile.Center);

            
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

            Particle pulse = new DirectionalPulseRing(
                Projectile.Center,
                Vector2.Zero,
                Color.White,
                new Vector2(1f, 1f),
                Main.rand.NextFloat(4f, 6f), // 可调
                0.1f,
                2.5f,
                12
            );
            GeneralParticleHandler.SpawnParticle(pulse);



            // 粒子设定（蓝色电感类全部统一替换）
            int[] electricDust = new int[] { 64, 138, 159, 204, 228 };

            // 1. 主视觉电光爆裂
            for (int i = 0; i < 36; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float distance = Main.rand.NextFloat(20f, 40f);
                Vector2 offset = angle.ToRotationVector2() * distance;
                Vector2 pos = spawnPosition + offset;

                int dustType = electricDust[Main.rand.Next(electricDust.Length)];
                Dust d = Dust.NewDustPerfect(pos, dustType, offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f), 100, Color.White, 1.9f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // 2. 十字方向电弧线条（集中选亮感较强的几种）
            Vector2[] directions2 = new Vector2[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
            foreach (Vector2 dir in directions2)
            {
                for (int i = 0; i < 6; i++)
                {
                    float length = Main.rand.NextFloat(20f, 40f);
                    Vector2 pos = spawnPosition + dir * length;
                    int dustType = Main.rand.NextBool() ? 228 : 204; // 选偏亮的Dust用于“射线”
                    Dust d = Dust.NewDustPerfect(pos, dustType, dir * Main.rand.NextFloat(4f, 7f), 120, Color.White, 1.6f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
            }

            // 3. 垂直电冲击柱（统一使用 Dust 64 为主体）
            for (int i = 0; i < 14; i++)
            {
                Vector2 vel = Vector2.UnitY * (Main.rand.NextBool() ? 1 : -1) * Main.rand.NextFloat(4f, 8f);
                Dust d = Dust.NewDustPerfect(spawnPosition, 64, vel, 100, Color.White, 1.7f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }

            // 4. 环状脉冲光圈（用 Dust 10 代替金色环）
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 pos = spawnPosition + angle.ToRotationVector2() * 18f;
                Dust d = Dust.NewDustPerfect(pos, 10, -angle.ToRotationVector2() * 1f, 80, Color.White, 1.4f);
                d.noGravity = true;
            }

            //// 5. 悬浮余辉粒子（使用 Dust 138/159 混合）
            //for (int i = 0; i < 12; i++)
            //{
            //    Vector2 pos = spawnPosition + Main.rand.NextVector2Circular(14f, 14f);
            //    int dustType = Main.rand.NextBool() ? 138 : 159;
            //    Dust d = Dust.NewDustPerfect(pos, dustType, Vector2.UnitY * -Main.rand.NextFloat(0.5f, 1.2f), 120, Color.White, 2f);
            //    d.noGravity = true;
            //    d.fadeIn = 2.4f;
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
                            (int)(Projectile.damage * 2.75f),
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

            Player player = Main.player[Projectile.owner];
            if (player.HeldItem.ModItem is AuricJav weapon)
            {
                weapon.AngerMeter += Main.rand.NextFloat(3f, 5f);
                if (weapon.AngerMeter > weapon.MaxAnger) weapon.AngerMeter = weapon.MaxAnger;
            }
        }
    }
}
