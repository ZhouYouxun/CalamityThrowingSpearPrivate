//using CalamityMod;
//using CalamityMod.Buffs.DamageOverTime;
//using CalamityMod.Buffs.StatDebuffs;
//using CalamityMod.Graphics.Primitives;
//using CalamityMod.Particles;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria;
//using Terraria.DataStructures;
//using Terraria.Graphics.Shaders;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK
//{
//    public class SHPCKFast2 : ModProjectile, ILocalizedModType
//    {
//        // 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键 这是临时的左键
//        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SHPCK/SHPCK";
//        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";

//        public override void SetStaticDefaults()
//        {
//            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
//            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
//        }

//        // 使用 Shader: ExobladePierceShader ("CalamityMod:ExobladePierce")
//        public override bool PreDraw(ref Color lightColor)
//        {
//            GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(
//                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak")
//            );

//            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f;
//            int numPoints = 44;

//            PrimitiveRenderer.RenderTrail(
//                Projectile.oldPos,
//                new(
//                    ExoPierceWidthFunction,
//                    ExoPierceColorFunction,
//                    (_) => overallOffset,
//                    shader: GameShaders.Misc["CalamityMod:ExobladePierce"]
//                ),
//                numPoints
//            );

//            return false;
//        }

//        private float ExoPierceWidthFunction(float completionRatio)
//        {
//            return 20f * (1f - completionRatio);
//        }

//        private Color ExoPierceColorFunction(float completionRatio)
//        {
//            return Color.Lerp(Color.Cyan, Color.Transparent, completionRatio);
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = Projectile.height = 32;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = 300;
//            Projectile.light = 0.5f;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = true; // 允许与方块碰撞
//            Projectile.extraUpdates = 2; // 额外更新次数
//            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
//            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
//            Projectile.alpha = 1;
//            //Projectile.scale = 0.7f; // 大小小一点
//        }
//        public override void OnSpawn(IEntitySource source)
//        {
//            // 传送至最近 Boss 或非 Boss 敌人
//            NPC target = FindClosestTarget(4000f); // 查找范围 4000
//            if (target != null)
//            {
//                Projectile.Center = target.Center;
//            }

//            // 透明化
//            Projectile.alpha = 255;

//            // 随机初始旋转方向
//            Projectile.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);

//            // 初速度降低
//            Projectile.velocity *= 0.95f;

//            // 生成吸引向心 Dust
//            //for (int i = 0; i < 45; i++)
//            //{
//            //    Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2Circular(3 * 16, 6 * 16);
//            //    Vector2 velocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 4f);

//            //    Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.Electric, velocity, 100, Color.Cyan, 1.5f);
//            //    dust.noGravity = true;
//            //}

//            // 生成吸引向心 Dust 特效，呈现闪电形状
//            {
//                float maxDustVelSpread = 1.2f;
//                int dustPerSegment = 32; // 每个闪电分段的粒子数
//                float lightningRotation = Main.rand.NextFloat(0, MathHelper.TwoPi); // 整体闪电的随机初始旋转

//                // 定义闪电的三个主要段
//                Vector2 segmentOneStart = new Vector2(0f, -120f);
//                Vector2 segmentOneEnd = new Vector2(-48f, 24f);
//                Vector2 segmentOneIncrement = (segmentOneEnd - segmentOneStart) / dustPerSegment;

//                Vector2 segmentTwoStart = segmentOneEnd;
//                Vector2 segmentTwoEnd = new Vector2(48f, -24f);
//                Vector2 segmentTwoIncrement = (segmentTwoEnd - segmentTwoStart) / dustPerSegment;

//                Vector2 segmentThreeStart = segmentTwoEnd;
//                Vector2 segmentThreeEnd = new Vector2(0f, 120f);
//                Vector2 segmentThreeIncrement = (segmentThreeEnd - segmentThreeStart) / dustPerSegment;

//                // 对每个分段生成 Dust
//                for (int i = 0; i < dustPerSegment; ++i)
//                {
//                    // 每段的线性插值计算粒子位置
//                    float interpolant = i + 0.5f;
//                    Vector2 segmentOnePos = segmentOneStart + segmentOneIncrement * interpolant;
//                    Vector2 segmentTwoPos = segmentTwoStart + segmentTwoIncrement * interpolant;
//                    Vector2 segmentThreePos = segmentThreeStart + segmentThreeIncrement * interpolant;

//                    // 将闪电形状整体旋转
//                    segmentOnePos = segmentOnePos.RotatedBy(lightningRotation);
//                    segmentTwoPos = segmentTwoPos.RotatedBy(lightningRotation);
//                    segmentThreePos = segmentThreePos.RotatedBy(lightningRotation);

//                    // 转换到弹幕中心坐标
//                    segmentOnePos += Projectile.Center;
//                    segmentTwoPos += Projectile.Center;
//                    segmentThreePos += Projectile.Center;

//                    // 随机加速度，吸向中心
//                    float spreadSpeed = Main.rand.NextFloat(0.5f, maxDustVelSpread);
//                    Vector2 velocityOne = (Projectile.Center - segmentOnePos).SafeNormalize(Vector2.Zero) * spreadSpeed;
//                    Vector2 velocityTwo = (Projectile.Center - segmentTwoPos).SafeNormalize(Vector2.Zero) * spreadSpeed;
//                    Vector2 velocityThree = (Projectile.Center - segmentThreePos).SafeNormalize(Vector2.Zero) * spreadSpeed;

//                    // 创建 Dust
//                    Dust d = Dust.NewDustPerfect(segmentOnePos, DustID.Electric, velocityOne, 100, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
//                    d.noGravity = true;

//                    d = Dust.NewDustPerfect(segmentTwoPos, DustID.Electric, velocityTwo, 100, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
//                    d.noGravity = true;

//                    d = Dust.NewDustPerfect(segmentThreePos, DustID.Electric, velocityThree, 100, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
//                    d.noGravity = true;
//                }
//            }
//        }

//        public override void AI()
//        {
//            Player player = Main.player[Projectile.owner];

//            if (Projectile.ai[0] == 0) // 第一阶段：旋转调整
//            {
//                // 速度逐渐降低
//                Projectile.velocity *= 0.88f;

//                // 计算朝向玩家的角度
//                float targetRotation = (player.Center - Projectile.Center).ToRotation() + MathHelper.PiOver4;

//                // 平滑调整旋转
//                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.1f);

//                // 透明度减少
//                Projectile.alpha -= 16;
//                if (Projectile.alpha < 0)
//                    Projectile.alpha = 0;

//                // 如果旋转接近目标，进入第二阶段
//                if (Math.Abs(Projectile.rotation - targetRotation) < 0.05f)
//                {
//                    Projectile.ai[0] = 1;
//                }
//            }
//            else if (Projectile.ai[0] == 1) // 第二阶段：高速冲刺
//            {
//                // 固定速度朝向玩家
//                float speed = 28f;
//                Vector2 direction = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;
//                Projectile.velocity = direction;

//                // 生成电流粒子
//                //for (int i = 0; i < 4; i++)
//                //{
//                //    Vector2 spawnPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);
//                //    Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.Electric, Main.rand.NextVector2Circular(1f, 3f), 100, Color.Cyan, 1.8f);
//                //    dust.noGravity = true;
//                //}

//                // 碰撞玩家后消失
//                if (Projectile.Hitbox.Intersects(player.Hitbox))
//                {
//                    Projectile.Kill();
//                }
//            }
//        }

//        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
//        {
//            if (Projectile.ai[1] == 0) // 第一次命中
//            {
//                Projectile.ai[1] = 1;
//            }
//            else // 后续命中
//            {
//                Projectile.damage = (int)(Projectile.damage * 0.8f); // 伤害衰减
//                if (Projectile.damage < 10)
//                    Projectile.damage = 10; // 保底伤害
//            }
//        }

//        /// <summary>
//        /// 查找最近的 Boss 或者普通敌人
//        /// </summary>
//        private NPC FindClosestTarget(float maxRange)
//        {
//            NPC closestTarget = null;
//            float closestDistance = maxRange;

//            foreach (NPC npc in Main.npc)
//            {
//                if (npc.active && !npc.friendly && npc.lifeMax > 5) // 过滤友方单位 & 生命值低的单位
//                {
//                    float distance = Vector2.Distance(Main.player[Projectile.owner].Center, npc.Center);
//                    if (distance < closestDistance)
//                    {
//                        closestDistance = distance;
//                        closestTarget = npc;
//                    }
//                }
//            }
//            return closestTarget;
//        }
//        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
//        {
//            // 施加带电 Debuff
//            target.AddBuff(BuffID.Electrified, 300);

//            // 计算粒子逆向生成位置
//            int particleCount = 10; // 生成 10 个粒子
//            float minAngle = -MathHelper.ToRadians(20); // -20° 左侧边界
//            float maxAngle = MathHelper.ToRadians(20); // +20° 右侧边界
//            float initialDistance = 80f; // 远离弹幕多少距离生成

//            for (int i = 0; i < particleCount; i++)
//            {
//                // 随机角度 (在 -20° ~ 20° 之间)
//                float angleOffset = Main.rand.NextFloat(minAngle, maxAngle);

//                // 计算初始生成点（远离弹幕）
//                Vector2 initialSpawnPosition = Projectile.Center +
//                    (Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(angleOffset) * initialDistance);

//                // 计算回收方向（确保回到弹幕中心）
//                Vector2 reverseDirection = (Projectile.Center - initialSpawnPosition).SafeNormalize(Vector2.Zero);
//                float speed = initialDistance / 60f; // 速度确保在 60 帧内回收

//                // 颜色随机化
//                Color electricColor = Main.rand.NextBool() ? Color.White : Color.Blue;

//                // 生成粒子
//                Particle electricParticle = new SparkParticle(
//                    initialSpawnPosition,    // 初始位置（远离弹幕）
//                    reverseDirection * speed, // 速度朝向弹幕
//                    false,                   // 非受重力影响
//                    60,                      // 存活时间，确保 60 帧后回到弹幕
//                    Main.rand.NextFloat(0.8f, 1.2f), // 大小随机化
//                    electricColor              // 颜色
//                );

//                // 释放粒子
//                GeneralParticleHandler.SpawnParticle(electricParticle);
//            }
//        }
//    }
//}
