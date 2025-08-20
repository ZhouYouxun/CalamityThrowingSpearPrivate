using CalamityMod;
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
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC
{
    public class GoldplumeJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/GoldplumeSpearC/GoldplumeJav";

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
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
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 500;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为14帧
            //Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }
        private bool collided = false; // 标记是否发生碰撞
        private List<SparkParticle> ownedSparkParticles = new();

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加白色光源
            Lighting.AddLight(Projectile.Center, Color.WhiteSmoke.ToVector3() * 0.55f);

            // 每帧将速度乘以1.001
            Projectile.velocity *= 1.0f;

            // 粒子特效（Cloud、34、57 混合使用）
            int particleCount = Main.rand.Next(4, 10); // 随机生成 4 到 9 个粒子
            for (int i = 0; i < particleCount; i++)
            {
                int dustType = Main.rand.Next(new int[] { DustID.Cloud, 34, 57 }); // 随机选择粒子类型
                Vector2 dustPosition = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f); // 稍微扩散的粒子生成位置
                Dust dust = Dust.NewDustPerfect(
                    dustPosition,       // 粒子生成位置
                    dustType,           // 粒子类型
                    null,               // 初始速度为空
                    100,                // 不透明度
                    default,            // 粒子颜色
                    Main.rand.NextFloat(1.25f, 1.75f) // 粒子大小随机化
                );

                dust.velocity = Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f); // 粒子速度：基于弹幕速度和随机扩散
                dust.noGravity = true; // 禁用重力
            }


            {
                // 如果未碰撞，检测与 GoldplumeJavWind 的碰撞
                if (!collided)
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile otherProj = Main.projectile[i];
                        if (otherProj.active && otherProj.type == ModContent.ProjectileType<GoldplumeJavWind>())
                        {
                            float distance = Vector2.Distance(Projectile.Center, otherProj.Center);

                            // 如果距离小于弹幕宽度，则视为碰撞
                            if (distance < (Projectile.width + otherProj.width) / 2f)
                            {
                                collided = true; // 标记为已碰撞
                                Projectile.timeLeft = 90; // 设置持续时间
                                Projectile.penetrate = -1;
                                Projectile.usesLocalNPCImmunity = true;
                                Projectile.localNPCHitCooldown = 18; //让它在公转期间也能造成多次伤害
                                Projectile.velocity = Vector2.Zero; // 初始速度归零

                                // 开始围绕 GoldplumeJavWind 做圆周运动
                                Projectile.ai[0] = distance; // 初始半径
                                Projectile.ai[1] = 0f; // 重置角度
                                break; // 防止多次判定
                            }
                        }
                    }
                }

                // 处理圆周运动逻辑
                if (collided)
                {
                    // 找到当前场景中的 GoldplumeJavWind 实例
                    Projectile targetWind = null;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile otherProj = Main.projectile[i];
                        if (otherProj.active && otherProj.type == ModContent.ProjectileType<GoldplumeJavWind>())
                        {
                            targetWind = otherProj;
                            break;
                        }
                    }

                    // 如果未找到目标实例，直接返回以避免错误
                    if (targetWind == null)
                    {
                        Projectile.Kill(); // 如果目标已经消失，则销毁主弹幕
                        return;
                    }

                    // 处理圆周运动
                    float speedIncrease = 0.1f; // 半径缩小速度
                    float angularSpeed = 0.3f; // 初始角速度

                    Projectile.ai[0] -= speedIncrease; // 半径逐渐缩小
                    float angle = Projectile.ai[1] += angularSpeed; // 累加角度

                    // 计算新的位置
                    Projectile.Center = targetWind.Center + new Vector2(Projectile.ai[0], 0f).RotatedBy(angle);
                    Projectile.rotation = (Projectile.Center - targetWind.Center).ToRotation() + MathHelper.PiOver2;

                    //{
                    //    // 计算当前正前方方向（白色 Spark 初始方向）
                    //    Vector2 forward = Projectile.rotation.ToRotationVector2();

                    //    // 创建 SparkParticle
                    //    Particle spark = new SparkParticle(
                    //        Projectile.Center,
                    //        forward * 3f, // 初速度，适中即可
                    //        false, // 不受重力
                    //        10,    // 生命周期
                    //        1.2f,  // 缩放
                    //        Color.White // 颜色
                    //    );
                    //    GeneralParticleHandler.SpawnParticle(spark);
                    //    ownedSparkParticles.Add((SparkParticle)spark);

                    //}
                }


                //// 获取场景中最近的 GoldplumeJavWind
                //Projectile targetWind = null;
                //float closestDist = float.MaxValue;
                //for (int i = 0; i < Main.maxProjectiles; i++)
                //{
                //    Projectile otherProj = Main.projectile[i];
                //    if (otherProj.active && otherProj.type == ModContent.ProjectileType<GoldplumeJavWind>())
                //    {
                //        float dist = Vector2.Distance(Projectile.Center, otherProj.Center);
                //        if (dist < closestDist)
                //        {
                //            closestDist = dist;
                //            targetWind = otherProj;
                //        }
                //    }
                //}

                //// 检测是否需要切换到公转状态（距离小于 256）
                //if (!collided && targetWind != null && closestDist < 256f)
                //{
                //    collided = true;
                //    Projectile.timeLeft = 80;
                //    Projectile.penetrate = -1;
                //    Projectile.usesLocalNPCImmunity = true;
                //    Projectile.localNPCHitCooldown = 18;
                //    Projectile.velocity = Vector2.Zero;

                //    Projectile.ai[0] = closestDist; // 初始半径
                //    Projectile.ai[1] = 0f; // 初始角度
                //}

                //// 公转逻辑
                //if (collided && targetWind != null)
                //{
                //    // 平滑缩圈
                //    float targetRadius = 48f; // 最终缩到 48px
                //    float currentRadius = Projectile.ai[0];
                //    float shrinkSpeed = 0.05f; // 缓慢缩圈速度
                //    currentRadius = MathHelper.Lerp(currentRadius, targetRadius, shrinkSpeed);
                //    Projectile.ai[0] = currentRadius;

                //    // 平滑旋转（圆润 iOS 风格缓动）
                //    float angularSpeed = 0.15f; // 缓慢旋转
                //    Projectile.ai[1] += angularSpeed;

                //    Vector2 desiredPosition = targetWind.Center + new Vector2(currentRadius, 0).RotatedBy(Projectile.ai[1]);
                //    Projectile.Center = Vector2.Lerp(Projectile.Center, desiredPosition, 0.08f); // 圆润移动到位

                //    Projectile.rotation = (Projectile.Center - targetWind.Center).ToRotation() + MathHelper.PiOver2 + MathHelper.PiOver2;

                //    // 保留 Spark 特效
                //    Vector2 forward = Projectile.rotation.ToRotationVector2();
                //    Particle spark = new SparkParticle(
                //        Projectile.Center,
                //        forward * 3f,
                //        false,
                //        60,
                //        1.2f,
                //        Color.White
                //    );
                //    GeneralParticleHandler.SpawnParticle(spark);
                //    ownedSparkParticles.Add((SparkParticle)spark);
                //}



                //// === Spark 右拐轨迹控制 ===
                //for (int i = ownedSparkParticles.Count - 1; i >= 0; i--)
                //{
                //    SparkParticle p = ownedSparkParticles[i];

                //    // 超时销毁时移除引用
                //    if (p.Time >= p.Lifetime)
                //    {
                //        ownedSparkParticles.RemoveAt(i);
                //        continue;
                //    }

                //    // 每帧持续右转（3°）
                //    float rotateAmount = MathHelper.ToRadians(3f);
                //    p.Velocity = p.Velocity.RotatedBy(rotateAmount);

                //    // （可选）微弱加速
                //    // p.Velocity *= 1.01f;
                //}


            }


          


        }


        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item39, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/New/金羽音效") with { Volume = 1.0f, Pitch = 0.0f }, Projectile.Center);


            if (collided)
            {
                // 已碰撞：360度随机发射10发羽毛（普通逻辑）
                for (int i = 0; i < 10; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f) * 10f; // 随机方向
                    int proj = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<GoldplumeJavFeather>(),
                        (int)(Projectile.damage * 0.1f),
                        Projectile.knockBack,
                        Projectile.owner,
                        1f // ai[0] = 1 → 追踪逻辑
                    );
                    if (proj.WithinBounds(Main.maxProjectiles))
                        Main.projectile[proj].ai[0] = 0f; // 明确标记普通逻辑
                }

                // 释放破旧的特效（仅使用 DustID.YellowStarfish）
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center,
                        DustID.YellowStarfish,
                        Main.rand.NextVector2Circular(3f, 3f),
                        100,
                        default,
                        1.5f
                    );
                    dust.noGravity = true; // 禁用重力
                }
            }
            else
            {
                // 未碰撞（被旋风吸入）：正前方、左右各5度发射羽毛（追踪逻辑）
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(5f) * i) * 10f;
                    int proj = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<GoldplumeJavFeather>(),
                        (int)(Projectile.damage * 0.4f),
                        Projectile.knockBack,
                        Projectile.owner,
                        0f // ai[0] = 0 → 普通直线逻辑
                    );
                    if (proj.WithinBounds(Main.maxProjectiles))
                        Main.projectile[proj].ai[0] = 1f; // 明确标记追踪逻辑
                }

                // 前方散射大量特效（Cloud、34、57 混合）
                for (int i = 0; i < 30; i++)
                {
                    int dustType = Main.rand.Next(new int[] { DustID.Cloud, 34, 57 });
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center,
                        dustType,
                        Main.rand.NextVector2Circular(5f, 5f),
                        100,
                        default,
                        Main.rand.NextFloat(1.25f, 1.75f)
                    );
                    dust.velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.8f, 1.2f);
                    dust.noGravity = true; // 禁用重力
                }
            }






        }




        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!collided)
            {
                // 未碰撞时触发：释放反方向左右扩散 15 度的羽毛
                for (int i = -1; i <= 1; i += 2) // 左右两侧（-1 和 1）
                {
                    // 反方向为当前速度反转
                    Vector2 reverseDirection = -Projectile.velocity.SafeNormalize(Vector2.Zero);
                    Vector2 featherVelocity = reverseDirection.RotatedBy(MathHelper.ToRadians(15f) * i) * 10f; // 左右扩散 15 度

                    // 生成羽毛弹幕
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        featherVelocity,
                        ModContent.ProjectileType<GoldplumeJavFeather>(),
                        (int)(Projectile.damage * 0.25f), // 羽毛伤害
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }
            }
            // 已碰撞时，不触发任何逻辑
        }

    }
}
