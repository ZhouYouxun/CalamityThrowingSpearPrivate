using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK
{
    public class SHPCKFast : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SHPCK/SHPCK";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取 SpriteBatch 和投射物纹理
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SHPCK/SHPCK").Value;

            // 颜色循环：蓝色光效（RGB 变化）
            Color[] colors = { Color.Cyan, Color.Blue, Color.LightBlue };
            float timeFactor = Main.GlobalTimeWrappedHourly * 3f;
            int colorIndex = (int)(timeFactor % colors.Length);
            Color currentColor = Color.Lerp(colors[colorIndex], colors[(colorIndex + 1) % colors.Length], timeFactor % 1f);

            // 遍历投射物的旧位置数组，绘制光学拖尾效果
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 计算颜色插值值，使颜色在旧位置之间平滑过渡
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;

                // 颜色过渡：RGB 光效切换
                Color color = Color.Lerp(currentColor, colors[(colorIndex + 2) % colors.Length], colorInterpolation) * 0.5f;
                color.A = 0;

                // 计算绘制位置，将位置调整到碰撞箱的中心
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                // 计算外部和内部的颜色
                Color outerColor = color;
                Color innerColor = color * 0.7f;

                // 确保拖尾大小不变
                Vector2 fixedScale = new Vector2(1.5f);

                // 绘制外部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, Projectile.rotation, lightTexture.Size() * 0.5f, fixedScale, SpriteEffects.None, 0);

                // 绘制内部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, Projectile.rotation, lightTexture.Size() * 0.5f, fixedScale * 0.7f, SpriteEffects.None, 0);
            }


            if (hasRecordedSpawnPosition)
            {
                Texture2D magicTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_03").Value;

                // 旋转和缩放逐渐减小
                Projectile.localAI[0] += 0.075f; // 旋转
                Projectile.localAI[1] -= 0.02f; // 大小
                if (Projectile.localAI[1] < 0f)
                    Projectile.localAI[1] = 0f;

                Vector2 drawPos = spawnEffectPosition - Main.screenPosition;
                Color technoBlue = new Color(80, 200, 255, 200) * (Projectile.localAI[1] / 2.5f);
                Color deepBlue = new Color(30, 100, 220, 240) * (Projectile.localAI[1] / 2.5f);

                Main.EntitySpriteDraw(
                    magicTexture,
                    drawPos,
                    null,
                    deepBlue,
                    Projectile.localAI[0],
                    magicTexture.Size() / 2f,
                    Projectile.localAI[1],
                    SpriteEffects.None,
                    0
                );
            }

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.alpha = 1;
            //Projectile.scale = 0.7f; // 大小小一点
        }
        public override void OnSpawn(IEntitySource source)
        {
            // 传送至最近 Boss 或非 Boss 敌人
            NPC target = FindClosestTarget(4000f);
            if (target != null)
            {
                Projectile.Center = target.Center;
                spawnEffectPosition = target.Center; // ✅ 记录出生特效位置
                hasRecordedSpawnPosition = true;
            }


            // 初始化旋转角度和缩放
            Projectile.localAI[0] = Main.rand.NextFloat(MathHelper.TwoPi); // 初始旋转
            Projectile.localAI[1] = 1.0f; // 初始缩放倍率（从X倍开始）


            // 透明化
            Projectile.alpha = 255;

            // 随机初始旋转方向
            Projectile.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);

            // 初速度降低
            Projectile.velocity *= 0.95f;

            // 生成吸引向心 Dust
            //for (int i = 0; i < 45; i++)
            //{
            //    Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2Circular(3 * 16, 6 * 16);
            //    Vector2 velocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 4f);

            //    Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.Electric, velocity, 100, Color.Cyan, 1.5f);
            //    dust.noGravity = true;
            //}

            //// 生成吸引向心 Dust 特效，呈现闪电形状
            //{
            //    float maxDustVelSpread = 1.2f;
            //    int dustPerSegment = 32; // 每个闪电分段的粒子数
            //    float lightningRotation = Main.rand.NextFloat(0, MathHelper.TwoPi); // 整体闪电的随机初始旋转

            //    // 定义闪电的三个主要段
            //    Vector2 segmentOneStart = new Vector2(0f, -120f);
            //    Vector2 segmentOneEnd = new Vector2(-48f, 24f);
            //    Vector2 segmentOneIncrement = (segmentOneEnd - segmentOneStart) / dustPerSegment;

            //    Vector2 segmentTwoStart = segmentOneEnd;
            //    Vector2 segmentTwoEnd = new Vector2(48f, -24f);
            //    Vector2 segmentTwoIncrement = (segmentTwoEnd - segmentTwoStart) / dustPerSegment;

            //    Vector2 segmentThreeStart = segmentTwoEnd;
            //    Vector2 segmentThreeEnd = new Vector2(0f, 120f);
            //    Vector2 segmentThreeIncrement = (segmentThreeEnd - segmentThreeStart) / dustPerSegment;

            //    // 对每个分段生成 Dust
            //    for (int i = 0; i < dustPerSegment; ++i)
            //    {
            //        // 每段的线性插值计算粒子位置
            //        float interpolant = i + 0.5f;
            //        Vector2 segmentOnePos = segmentOneStart + segmentOneIncrement * interpolant;
            //        Vector2 segmentTwoPos = segmentTwoStart + segmentTwoIncrement * interpolant;
            //        Vector2 segmentThreePos = segmentThreeStart + segmentThreeIncrement * interpolant;

            //        // 将闪电形状整体旋转
            //        segmentOnePos = segmentOnePos.RotatedBy(lightningRotation);
            //        segmentTwoPos = segmentTwoPos.RotatedBy(lightningRotation);
            //        segmentThreePos = segmentThreePos.RotatedBy(lightningRotation);

            //        // 转换到弹幕中心坐标
            //        segmentOnePos += Projectile.Center;
            //        segmentTwoPos += Projectile.Center;
            //        segmentThreePos += Projectile.Center;

            //        // 随机加速度，吸向中心
            //        float spreadSpeed = Main.rand.NextFloat(0.5f, maxDustVelSpread);
            //        Vector2 velocityOne = (Projectile.Center - segmentOnePos).SafeNormalize(Vector2.Zero) * spreadSpeed;
            //        Vector2 velocityTwo = (Projectile.Center - segmentTwoPos).SafeNormalize(Vector2.Zero) * spreadSpeed;
            //        Vector2 velocityThree = (Projectile.Center - segmentThreePos).SafeNormalize(Vector2.Zero) * spreadSpeed;

            //        // 创建 Dust
            //        Dust d = Dust.NewDustPerfect(segmentOnePos, DustID.Electric, velocityOne, 100, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
            //        d.noGravity = true;

            //        d = Dust.NewDustPerfect(segmentTwoPos, DustID.Electric, velocityTwo, 100, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
            //        d.noGravity = true;

            //        d = Dust.NewDustPerfect(segmentThreePos, DustID.Electric, velocityThree, 100, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
            //        d.noGravity = true;
            //    }
            //}


            {
                // 1. 记录初始位置
                Vector2 origin = Projectile.Center;

                // 2. Dust 粒子：爆发式放射 + 时间扭曲
                int dustCount = 30;
                for (int i = 0; i < dustCount; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(80f, 124f); // 半径随机
                    Vector2 pos = origin + offset;
                    Vector2 vel = -offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.8f, 3.2f); // 回吸式

                    Color dustColor = Main.rand.NextBool(2) ? Color.Cyan : Color.BlueViolet;
                    float scale = Main.rand.NextFloat(1.2f, 2.1f);

                    Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, dustColor, scale);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // 3. 方块粒子：随机旋转散落，表现科技能量核心爆发
                int squareCount = 30;
                for (int i = 0; i < squareCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f); // 缓慢漂浮感
                    Color c = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat());
                    SquareParticle square = new SquareParticle(
                        origin,
                        vel,
                        false,
                        Main.rand.Next(36, 64), // 存活时间
                        Main.rand.NextFloat(1.2f, 2.5f), // 大小
                        c * 1.5f
                    );
                    GeneralParticleHandler.SpawnParticle(square);
                }

                // 4. 线性拖尾粒子：方向感，表现“时空能量流动”
                int sparkCount = 12;
                for (int i = 0; i < sparkCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 4f);
                    Color c = Color.Lerp(Color.OrangeRed, Color.Orange, Main.rand.NextFloat());

                    Particle trail = new SparkParticle(
                        origin + Main.rand.NextVector2Circular(16f, 16f),
                        vel,
                        false,
                        60,
                        Main.rand.NextFloat(0.8f, 1.4f),
                        c
                    );
                    GeneralParticleHandler.SpawnParticle(trail);
                }

            }


        }

        private Vector2 spawnEffectPosition;
        private bool hasRecordedSpawnPosition = false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[0] == 0) // 第一阶段：旋转调整
            {
                // 速度逐渐降低
                Projectile.velocity *= 0.88f;

                // 计算朝向玩家的角度
                float targetRotation = (player.Center - Projectile.Center).ToRotation() + MathHelper.PiOver4;

                // 平滑调整旋转
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.1f);

                // 透明度减少
                Projectile.alpha -= 16;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;

                // 如果旋转接近目标，进入第二阶段
                if (Math.Abs(Projectile.rotation - targetRotation) < 0.05f)
                {
                    Projectile.ai[0] = 1;
                }
            }
            else if (Projectile.ai[0] == 1) // 第二阶段：高速冲刺
            {
                // 固定速度朝向玩家
                float speed = 28f;
                Vector2 direction = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;
                Projectile.velocity = direction;

                // 生成电流粒子
                //for (int i = 0; i < 4; i++)
                //{
                //    Vector2 spawnPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);
                //    Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.Electric, Main.rand.NextVector2Circular(1f, 3f), 100, Color.Cyan, 1.8f);
                //    dust.noGravity = true;
                //}

                // 碰撞玩家后消失
                if (Projectile.Hitbox.Intersects(player.Hitbox))
                {
                    Projectile.Kill();
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.ai[1] == 0) // 第一次命中
            {
                Projectile.ai[1] = 1;
            }
            else // 后续命中
            {
                Projectile.damage = (int)(Projectile.damage * 0.8f); // 伤害衰减
                if (Projectile.damage < 10)
                    Projectile.damage = 10; // 保底伤害
            }
        }

        /// <summary>
        /// 查找最近的 Boss 或者普通敌人
        /// </summary>
        private NPC FindClosestTarget(float maxRange)
        {
            NPC closestTarget = null;
            float closestDistance = maxRange;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.lifeMax > 5) // 过滤友方单位 & 生命值低的单位
                {
                    float distance = Vector2.Distance(Main.player[Projectile.owner].Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = npc;
                    }
                }
            }
            return closestTarget;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 施加带电 Debuff
            target.AddBuff(BuffID.Electrified, 300);

            // 计算粒子逆向生成位置
            int particleCount = 10; // 生成 10 个粒子
            float minAngle = -MathHelper.ToRadians(20); // -20° 左侧边界
            float maxAngle = MathHelper.ToRadians(20); // +20° 右侧边界
            float initialDistance = 80f; // 远离弹幕多少距离生成

            for (int i = 0; i < particleCount; i++)
            {
                // 随机角度 (在 -20° ~ 20° 之间)
                float angleOffset = Main.rand.NextFloat(minAngle, maxAngle);

                // 计算初始生成点（远离弹幕）
                Vector2 initialSpawnPosition = Projectile.Center +
                    (Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(angleOffset) * initialDistance);

                // 计算回收方向（确保回到弹幕中心）
                Vector2 reverseDirection = (Projectile.Center - initialSpawnPosition).SafeNormalize(Vector2.Zero);
                float speed = initialDistance / 60f; // 速度确保在 60 帧内回收

                // 颜色随机化
                Color electricColor = Main.rand.NextBool() ? Color.White : Color.Blue;

                // 生成粒子
                Particle electricParticle = new SparkParticle(
                    initialSpawnPosition,    // 初始位置（远离弹幕）
                    reverseDirection * speed, // 速度朝向弹幕
                    false,                   // 非受重力影响
                    60,                      // 存活时间，确保 60 帧后回到弹幕
                    Main.rand.NextFloat(0.8f, 1.2f), // 大小随机化
                    electricColor              // 颜色
                );

                // 释放粒子
                GeneralParticleHandler.SpawnParticle(electricParticle);
            }
        }
    }
}
