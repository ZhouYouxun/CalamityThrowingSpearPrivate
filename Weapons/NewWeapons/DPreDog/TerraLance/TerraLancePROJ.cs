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
using Terraria.Audio;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityRangerExpansion.LightingBolts;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.TerraLance
{
    public class TerraLancePROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/TerraLance/TerraLance";
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        public override void SetStaticDefaults()
        {
            // 设置弹幕的历史位置长度和残影模式
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理资源
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // 计算每帧的高度
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int y = frameHeight * Projectile.frame;

            // 投射物的缩放比例和裁剪区域
            float scale = Projectile.scale;
            Rectangle rectangle = new Rectangle(0, y, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;

            // 当前弹幕的翻转状态
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // 居中偏移
            Vector2 drawOffset = Projectile.Size / 2f;
            Color alpha = Projectile.GetAlpha(lightColor);

            // 第一阶段：普通拖尾效果
            if (Projectile.ai[0] < 25)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    // 获取历史位置、旋转和翻转状态
                    Vector2 position = Projectile.oldPos[i] + drawOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    float rotation2 = Projectile.oldRot[i];
                    SpriteEffects effects2 = Projectile.oldSpriteDirection[i] == -1
                        ? SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
                        : SpriteEffects.None;

                    // 根据历史位置调整颜色透明度
                    Color color = alpha * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length);

                    // 绘制残影
                    Main.spriteBatch.Draw(texture, position, rectangle, color, rotation2, origin, scale, effects2, 0f);
                }

                // 绘制弹幕本体
                Vector2 currentPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                Main.spriteBatch.Draw(texture, currentPosition, rectangle, lightColor, Projectile.rotation, origin, scale, effects, 0f);

                return false;
            }

            // 第二阶段：背光效果
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 背光效果部分 - 亮绿色光晕
            float chargeOffset = 5f; // 控制背光效果扩散的偏移量
            Color chargeColor = Color.Lime * 0.6f; // 设置为亮绿色
            chargeColor.A = 0; // 设置透明度

            // 修复旋转逻辑，确保与速度方向同步
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 绘制背光效果 - 圆周上绘制多个光效
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + offset, rectangle, chargeColor, rotation, origin, scale, effects, 0f);
            }

            // 渲染实际的投射物本体
            Main.spriteBatch.Draw(texture, drawPosition, rectangle, Projectile.GetAlpha(lightColor), rotation, origin, scale, effects, 0f);

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        public override bool? CanDamage()
        {
            // 如果在第一阶段（加速阶段），返回 false，表示不造成伤害
            if (Projectile.ai[0] < 25)
            {
                return false;
            }

            // 其他阶段允许造成伤害
            return base.CanDamage();
        }
        private List<SparkParticle> ownedSparkParticles = new();
        private List<AltSparkParticle> ownedAltSparkParticles = new();

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加绿色光源
            Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 0.55f);



            // 阶段控制
            if (Projectile.ai[0] < 25)
            {
                Projectile.penetrate = -1; // 无限穿透
                
                // 加速期间超大型粒子特效
                if (Projectile.numUpdates % 3 == 0)
                {
                    Color brightWhite = Color.DarkOliveGreen;
                    float outerSparkScale = 2.6f; // 放大？%
                    SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, brightWhite);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            else if (Projectile.ai[0] == 25) // 阶段转换时释放特殊特效
            {
                // 吸引特效：生成两个向中心汇聚的圆圈
                for (int i = 0; i < 2; i++) // 生成两个吸引特效
                {
                    // 设置不同的颜色，每次随机选择
                    Color pulseColor = (i == 0) ? Color.LimeGreen : Color.LightGreen;

                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center,          // 特效生成位置
                        Vector2.Zero,               // 无初始速度（吸引效果）
                        pulseColor,                 // 颜色
                        new Vector2(2f + i * 0.5f), // 大小依次增加
                        Projectile.rotation,        // 旋转角度
                        1.2f,                       // 初始缩放
                        0.15f,                      // 缩放减小速率
                        40                          // 特效存活时间
                    );

                    GeneralParticleHandler.SpawnParticle(pulse); // 生成特效
                }

                // 特效：生成抽象小树图标
                {
                    Vector2 center = Projectile.Center; // 弹幕中心对齐树干正中间
                    float trunkHeight = 120f; // 树干高度扩大两倍
                    float foliageRadius = 40f; // 树冠扩大两倍
                    float expansionSpeed = 1.5f; // 粒子扩散速度增大

                    // 左右树枝的长度和高度调整
                    float leftBranchLength = 60f; // 左侧树枝长度加倍
                    float rightBranchLength = 100f; // 右侧树枝长度加倍
                    float leftBranchHeight = trunkHeight * 0.4f; // 左侧树枝偏低
                    float rightBranchHeight = trunkHeight * 0.7f; // 右侧树枝偏高

                    // 生成树干部分（竖直粒子列）
                    for (float y = 0; y <= trunkHeight; y += 10f) // 每隔10像素生成一个粒子
                    {
                        Vector2 position = center + new Vector2(0, -y); // 树干向上生长
                        Dust treeTrunkDust = Dust.NewDustPerfect(
                            position,
                            DustID.WoodFurniture,
                            Vector2.Zero,
                            0,
                            Color.SaddleBrown,
                            1.2f
                        );
                        treeTrunkDust.noGravity = true; // 禁用重力
                        treeTrunkDust.fadeIn = 0.8f;    // 添加淡入效果
                    }

                    // 生成左侧树枝
                    for (float t = 0; t <= 1; t += 0.05f) // 线性插值生成树枝，密度增加
                    {
                        Vector2 branchStart = center + new Vector2(0, -leftBranchHeight);
                        Vector2 branchEnd = branchStart + new Vector2(-leftBranchLength, -leftBranchLength * 0.5f);
                        Vector2 position = Vector2.Lerp(branchStart, branchEnd, t);

                        Dust.NewDustPerfect(position, DustID.PureSpray, Main.rand.NextVector2Circular(0.8f, 0.8f) * expansionSpeed, 0, Color.Lime, 1.5f);
                    }

                    // 生成右侧树枝
                    for (float t = 0; t <= 1; t += 0.05f)
                    {
                        Vector2 branchStart = center + new Vector2(0, -rightBranchHeight);
                        Vector2 branchEnd = branchStart + new Vector2(rightBranchLength, -rightBranchLength * 0.5f);
                        Vector2 position = Vector2.Lerp(branchStart, branchEnd, t);

                        Dust.NewDustPerfect(position, DustID.PureSpray, Main.rand.NextVector2Circular(0.8f, 0.8f) * expansionSpeed, 0, Color.Lime, 1.5f);
                    }

                    // 生成树冠（顶部圆形）
                    for (int j = 0; j < 36; j++)
                    {
                        float angle = MathHelper.TwoPi / 36 * j;
                        Vector2 position = center + new Vector2(0, -trunkHeight) + angle.ToRotationVector2() * foliageRadius;

                        Dust.NewDustPerfect(position, DustID.Terra, Main.rand.NextVector2Circular(1f, 1f) * expansionSpeed, 0, Color.LimeGreen, 1.8f);
                    }

                    // 生成树枝末端的圆形装饰
                    for (int i = -1; i <= 1; i += 2) // 左右树枝
                    {
                        float branchHeight = (i == -1) ? leftBranchHeight : rightBranchHeight;
                        float branchLength = (i == -1) ? leftBranchLength : rightBranchLength;
                        Vector2 branchEnd = center + new Vector2(0, -branchHeight) + new Vector2(branchLength * i, -branchLength * 0.5f);
                        for (int j = 0; j < 24; j++)
                        {
                            float angle = MathHelper.TwoPi / 24 * j;
                            Vector2 position = branchEnd + angle.ToRotationVector2() * foliageRadius * 0.5f;

                            Dust.NewDustPerfect(position, DustID.PureSpray, Main.rand.NextVector2Circular(0.8f, 0.8f) * expansionSpeed, 0, Color.White, 1.2f);
                        }
                    }
                }
            }
            else // 第二阶段：追踪敌人
            {
                // 切换为单体穿透并逐渐加速
                float accelerationFactor = Main.rand.NextFloat(1.01f, 1.04f); // 随机加速度
                Projectile.velocity *= accelerationFactor; // 增加速度
                Projectile.penetrate = 1;

                // 开始追踪最近的敌人
                NPC target = Projectile.Center.ClosestNPCAt(6400); // 查找范围内最近的敌人
                if (target != null)
                {
                    // 计算目标方向并追踪
                    Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero); // 目标方向
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToTarget * 18f, 0.08f); // 平滑追踪目标
                }


                {
                    // 每 ？ 帧释放 Spark，会飞出去后转向追踪自己
                    if (Projectile.ai[0] % 1 == 0)
                    {
                        Vector2 velocity = Main.rand.NextVector2CircularEdge(4f, 4f);
                        SparkParticle spark = new SparkParticle(
                            Projectile.Center,
                            velocity,
                            false,
                            60,
                            Main.rand.NextFloat(0.8f, 1.4f),
                            Color.LimeGreen
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                        ownedSparkParticles.Add(spark);
                    }

                    // 每 ？ 帧释放 AltSparkParticle，加长流体粒子并旋转
                    if (Projectile.ai[0] % 1 == 0)
                    {
                        Vector2 velocity = Main.rand.NextVector2CircularEdge(2f, 2f);
                        AltSparkParticle trail = new AltSparkParticle(
                            Projectile.Center,
                            velocity,
                            false,
                            45,
                            1.4f,
                            Color.LimeGreen * 0.2f
                        );
                        GeneralParticleHandler.SpawnParticle(trail);
                        ownedAltSparkParticles.Add(trail);
                    }

                    // 每 2 帧生成绿色 Dust 爆发
                    if (Projectile.ai[0] % 2 == 0)
                    {
                        Dust dust = Dust.NewDustPerfect(
                            Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                            107,
                            Main.rand.NextVector2Circular(2f, 4f),
                            150,
                            Color.LimeGreen,
                            Main.rand.NextFloat(1.0f, 1.6f)
                        );
                        dust.noGravity = true;
                        dust.fadeIn = Main.rand.NextFloat(0.5f, 1.0f);
                    }

                }




            }

            // 更新计数器
            Projectile.ai[0]++;

            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
            Projectile.rotation += Projectile.spriteDirection * MathHelper.ToRadians(45f);

            {
                // 管理 SparkParticle 吸引回本体
                for (int i = ownedSparkParticles.Count - 1; i >= 0; i--)
                {
                    SparkParticle p = ownedSparkParticles[i];
                    if (p.Time >= p.Lifetime)
                    {
                        ownedSparkParticles.RemoveAt(i);
                        continue;
                    }
                    // 计算朝向本体的方向
                    Vector2 toSelf = (Projectile.Center - p.Position).SafeNormalize(Vector2.Zero);
                    p.Velocity = Vector2.Lerp(p.Velocity, toSelf * 8f, 0.06f);
                    p.Velocity *= 1.02f; // 缓慢加速
                }

                // 管理 AltSparkParticle 旋转
                for (int i = ownedAltSparkParticles.Count - 1; i >= 0; i--)
                {
                    AltSparkParticle p = ownedAltSparkParticles[i];
                    if (p.Time >= p.Lifetime)
                    {
                        ownedAltSparkParticles.RemoveAt(i);
                        continue;
                    }
                    p.Velocity = p.Velocity.RotatedBy(MathHelper.ToRadians(2f));
                }

            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            {
                Vector2 center = target.Center;

                // === 0️⃣ 多次调用 Spawn_TerraLanceForestSpirals 构建六边形结构 ===
                float spiralRadius = 60f;
                int spiralPoints = 6;
                for (int i = 0; i < spiralPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / spiralPoints;
                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;
                    CTSLightingBoltsSystem.Spawn_TerraLanceForestSpirals(center + offset);
                }
                // 中心再调用一次
                CTSLightingBoltsSystem.Spawn_TerraLanceForestSpirals(center);
                

                float time = Main.GameUpdateCount * 0.05f;


                // ===================================
                // 🚩【1️⃣ 有序：多层螺旋藤蔓 GlowSparkParticle 阵（收敛版）】
                // ===================================
                int spiralLayers = 2;
                int sparksPerLayer = 16;
                float baseRadius = 20f;
                float radiusStep = 12f;

                for (int layer = 0; layer < spiralLayers; layer++)
                {
                    float radius = baseRadius + layer * radiusStep;
                    for (int i = 0; i < sparksPerLayer; i++)
                    {
                        float angle = MathHelper.TwoPi * i / sparksPerLayer + layer * 0.6f;
                        Vector2 dir = angle.ToRotationVector2();
                        Vector2 spawnPos = center + dir * radius;
                        Vector2 velocity = dir.RotatedBy(MathHelper.PiOver4) * Main.rand.NextFloat(2f, 5f);

                        GlowSparkParticle spark = new GlowSparkParticle(
                            spawnPos,
                            velocity,
                            false,
                            Main.rand.Next(40, 50),
                            Main.rand.NextFloat(0.08f, 0.12f),
                            Color.Lerp(Color.ForestGreen, Color.Green, 0.5f) * 0.5f,
                            new Vector2(0.4f, 1.2f)
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }


                // ===================================
                // 🚩【2️⃣ 有序 + 中和：森林符文十字星矩阵爆裂】
                // ===================================
                int starCount = 16;
                for (int i = 0; i < starCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / starCount;
                    Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(20f, 36f);
                    Vector2 spawnPos = center + offset;

                    GenericSparkle sparkle = new GenericSparkle(
                        spawnPos,
                        Vector2.Zero,
                        Color.White,
                        Color.LimeGreen,
                        Main.rand.NextFloat(1.8f, 2.5f),
                        10,
                        Main.rand.NextFloat(-0.06f, 0.06f),
                        1.9f
                    );
                    GeneralParticleHandler.SpawnParticle(sparkle);
                }

                // ===================================
                // 🚩【3️⃣ 无序但流动：Dust 环形森林藤蔓】
                // ===================================
                int vineDustAmount = 180;
                for (int i = 0; i < vineDustAmount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = Main.rand.NextFloat(20f, 100f);
                    Vector2 spawnPos = center + angle.ToRotationVector2() * radius;
                    Vector2 velocity = angle.ToRotationVector2().RotatedBy(Math.Sin(time + i * 0.05f) * 0.3f) * Main.rand.NextFloat(3f, 8f);

                    int dust = Dust.NewDust(spawnPos, 0, 0, DustID.Grass, velocity.X, velocity.Y, 100, Color.ForestGreen, Main.rand.NextFloat(1.0f, 1.6f));
                    Main.dust[dust].noGravity = true;
                }

                // ===================================
                // 🚩【4️⃣ 花瓣式森林藤蔓 SparkParticle 环形爆散（夸张版）】
                // ===================================
                int vineCount = 160;
                float startRadius = 16f;
                float endRadius = 96f;

                for (int i = 0; i < vineCount; i++)
                {
                    float progress = i / (float)vineCount;
                    float angle = MathHelper.TwoPi * progress * 8f + Main.GameUpdateCount * 0.08f; // 8 圈螺旋 + 更快动态

                    // 花瓣形震荡加剧
                    float flowerFactor = (float)Math.Sin(progress * MathHelper.TwoPi * 8f) * 0.4f;
                    float radius = MathHelper.Lerp(startRadius, endRadius, progress) * (1f + flowerFactor);

                    Vector2 spawnPos = center + angle.ToRotationVector2() * radius;

                    // 藤蔓式曲折速度加大
                    Vector2 dir = angle.ToRotationVector2().RotatedBy(Math.Sin(progress * MathHelper.TwoPi * 5f) * 0.4f);
                    Vector2 velocity = dir * MathHelper.Lerp(6f, 18f, progress);

                    Particle vineSpark = new SparkParticle(
                        spawnPos,
                        velocity,
                        false,
                        55,
                        MathHelper.Lerp(0.05f, 0.15f, 1f - progress), // 内细外粗
                        Color.Lerp(Color.ForestGreen, Color.LimeGreen, progress) * 0.7f
                    );
                    GeneralParticleHandler.SpawnParticle(vineSpark);
                }




                // ===================================
                // 🚩【5️⃣ 有序收束：森林脉冲环爆发】
                // ===================================
                for (int i = 0; i < 14; i++)
                {
                    Color pulseColor = Main.rand.Next(3) switch
                    {
                        0 => Color.ForestGreen,
                        1 => Color.LimeGreen,
                        _ => Color.GreenYellow,
                    };

                    Particle pulse = new CustomPulse(
                        center,
                        Vector2.Zero,
                        pulseColor * 0.5f,
                        "CalamityThrowingSpear/Texture/KsTexture/light_03", 
                        new Vector2(0.5f, 0.5f),
                        Main.rand.NextFloat(-20f, 20f),
                        0f,
                        (5f - i * 0.25f) * 0.15f,
                        50
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }

            }

            Vector2 center1 = target.Center;

            // 调用 TerraBeamStorm 方法，生成激光弹幕
            TerraBeamStorm(center1);

            // 在敌人中心生成 TerratomereExplosion 弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                center1,
                Vector2.Zero,
                ModContent.ProjectileType<TerratomereExplosion>(),
                (int)(Projectile.damage * 1.05f),
                Projectile.knockBack,
                Projectile.owner
            );
        }


        //public override void OnKill(int timeLeft)
        //{
        //    Vector2 center = Projectile.Center;

        //    // 更复杂的同心圆法阵
        //    int numCircles = 5; // 圆的数量增加
        //    int numPointsPerCircle = 18; // 每个圆的粒子数量增加
        //    float radiusIncrement = 40f; // 圆之间的半径缩小，使整体更加紧凑

        //    for (int circle = 0; circle < numCircles; circle++)
        //    {
        //        float radius = (circle + 1) * radiusIncrement;
        //        for (int i = 0; i < numPointsPerCircle; i++)
        //        {
        //            float angle = MathHelper.TwoPi * i / numPointsPerCircle;
        //            Vector2 position = center + angle.ToRotationVector2() * radius;

        //            for (int j = 0; j < 10; j++) // 每个点释放更多粒子
        //            {
        //                float speed = MathHelper.Lerp(3f, 10f, j / 10f); // 更宽范围的速度
        //                Color particleColor = Color.Lerp(Color.White, Color.LimeGreen, j / 10f); // 保持原有颜色渐变
        //                float scale = MathHelper.Lerp(1.8f, 0.6f, j / 10f); // 更强的缩放对比

        //                Dust magicDust = Dust.NewDustPerfect(position, 107);
        //                magicDust.velocity = angle.ToRotationVector2() * speed;
        //                magicDust.color = particleColor;
        //                magicDust.scale = scale;
        //                magicDust.noGravity = true;
        //            }
        //        }
        //    }

        //    // 添加更复杂的旋转法阵
        //    for (int i = 0; i < 72; i++) // 粒子数量翻倍
        //    {
        //        float angle = MathHelper.TwoPi * i / 72f;
        //        Vector2 position = center + angle.ToRotationVector2() * 25f;

        //        Dust spinningDust = Dust.NewDustPerfect(position, 107);
        //        spinningDust.velocity = angle.ToRotationVector2() * 6f; // 更高的旋转速度
        //        spinningDust.color = Color.GreenYellow;
        //        spinningDust.scale = 1.5f; // 更大粒子
        //        spinningDust.noGravity = true;
        //    }

        //    // 魔法阵式 SparkParticle 特效
        //    int numRings = 3; // 魔法阵的环数
        //    for (int ring = 0; ring < numRings; ring++)
        //    {
        //        float ringRadius = 50f + ring * 30f; // 每个环的半径增加
        //        int particlesPerRing = 24; // 每个环的粒子数量
        //        for (int i = 0; i < particlesPerRing; i++)
        //        {
        //            float angle = MathHelper.TwoPi * i / particlesPerRing;
        //            Vector2 position = center + angle.ToRotationVector2() * ringRadius;
        //            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f); // 随机速度

        //            Particle trail = new SparkParticle(position, velocity, false, 60, Main.rand.NextFloat(1.0f, 1.5f), Color.Green);
        //            GeneralParticleHandler.SpawnParticle(trail);
        //        }
        //    }


        //    // 在原地生成TerratomereExplosion弹幕，倍率为1.25
        //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TerratomereExplosion>(), (int)(Projectile.damage * 1.05f), Projectile.knockBack, Projectile.owner);


        //}

        // 在屏幕外围召唤一系列TerraBeam弹幕，类似于PetalStorm的效果
        private void TerraBeamStorm(Vector2 targetPos)
        {
            // 播放攻击音效
            SoundEngine.PlaySound(SoundID.Item105, targetPos);

            // 设置弹幕类型为TerraBeam（132号）
            int type = ModContent.ProjectileType<TerraLanceBEAM>();
            int numBeams = 12;  // 生成8个TerraBeam弹幕
            var source = Projectile.GetSource_FromThis();
            int beamDamage = (int)(Projectile.damage * 0.85f);  // 伤害调整为1.05倍
            float beamKB = Projectile.knockBack;

            for (int i = 0; i < numBeams; ++i)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);  // 随机生成方向角
                                                                          // 使用类似CalamityUtils的工具函数生成弹幕
                    Projectile beam = CalamityUtils.ProjectileBarrage(source, Projectile.Center, targetPos, Main.rand.NextBool(),
                        2000f, 2800f, 80f, 900f, Main.rand.NextFloat(DragonPow.MinPetalSpeed * 2, DragonPow.MaxPetalSpeed * 2),
                        type, beamDamage, beamKB, Projectile.owner);

                    if (beam.whoAmI.WithinBounds(Main.maxProjectiles))
                    {
                        beam.DamageType = DamageClass.Melee;
                        beam.rotation = angle;  // 设置弹幕旋转角度
                        beam.usesLocalNPCImmunity = true;
                        beam.localNPCHitCooldown = -1;  // 设置无敌帧冷却时间
                    }
                }
            }
        }




    }
}