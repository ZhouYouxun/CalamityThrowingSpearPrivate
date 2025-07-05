using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.ElectrocoagulationTenmonJav
{
    public class ElectrocoagulationTenmonJavLight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static int MaxUpdate = 5; // 定义一个静态变量，表示弹幕每次更新的最大次数
        private int Lifetime = 110; // 定义弹幕的生命周期为110

        private static Color ShaderColorOne = Color.WhiteSmoke; // 着色器颜色1，设置为深绿色
        private static Color ShaderColorTwo = Color.White; // 着色器颜色2，设置为白色
        private static Color ShaderEndColor = Color.GhostWhite; // 着色器结束颜色，设置为浅绿色

        private Vector2 altSpawn; // 定义一个备用生成位置向量
        public ref float Time => ref Projectile.ai[1];
        public override void SetStaticDefaults() // 设置弹幕的静态默认值
        {
            // 虽然这个弹幕没有残影，但它会跟踪旧位置用于绘制代码
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // 设置拖尾模式为2
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21; // 设置拖尾缓存长度为21
        }

        public override void SetDefaults() // 设置弹幕的默认值
        {
            Projectile.width = Projectile.height = 24; // 设置弹幕宽度和高度为24
            //Projectile.arrow = true; // 标记弹幕为箭
            Projectile.friendly = true; // 弹幕为友好类型（不会伤害玩家）
            Projectile.DamageType = DamageClass.Melee; // 设置弹幕为远程伤害类型
            Projectile.tileCollide = false; // 弹幕不会与砖块碰撞
            Projectile.ignoreWater = true; // 弹幕不会受到水的影响
            Projectile.timeLeft = Lifetime; // 设置弹幕的剩余时间为Lifetime
            Projectile.MaxUpdates = MaxUpdate; // 设置最大更新次数为MaxUpdate
            Projectile.penetrate = -1; // 弹幕可以穿透inf次
            Projectile.usesLocalNPCImmunity = true; // 使用本地NPC无敌机制
            Projectile.localNPCHitCooldown = 2; // 设置NPC的无敌冷却时间为5（立即生效）
        }

        //public override bool? CanDamage() => Projectile.numHits >= 1 ? false : null; // 如果弹幕已经命中一次，不再造成伤害

        public override void AI() // 弹幕的AI逻辑
        {
            Projectile.ai[0]++; // 弹幕AI计数器递增
            if (Projectile.ai[0] == 45) // 如果AI计数器达到45
                altSpawn = Projectile.Center; // 记录当前弹幕位置为备用生成位置
            if (Projectile.timeLeft <= 5) // 如果弹幕剩余时间小于等于5
            {
                // 生成深绿色的GemDiamond粒子特效，位置随机略偏移
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(9, 9) - Projectile.velocity * 5, DustID.GemDiamond, Projectile.velocity * 30 * Main.rand.NextFloat(0.1f, 0.95f));
                dust.noGravity = true; // 粒子不受重力影响
                dust.scale = Main.rand.NextFloat(0.9f, 1.45f); // 粒子的缩放比例
                dust.alpha = 235; // 设置粒子的透明度
                dust.color = Color.White; // 强制设置为深绿色
            }

            {
                // 双螺旋白色 Dust + Spark 飞行环绕特效
                float spiralRadius = 12f;
                float spiralSpeed = 0.25f; // 控制螺旋速度
                float globalTime = Main.GameUpdateCount * spiralSpeed;

                for (int spiral = 0; spiral < 2; spiral++)
                {
                    float spiralOffset = spiral * MathHelper.Pi; // 两螺旋相差 180°
                    float angle = globalTime + spiralOffset;
                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;

                    // Dust 环绕
                    if (Main.rand.NextBool(2)) // 控制密度
                    {
                        Dust dust = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            DustID.GemDiamond,
                            Vector2.Zero,
                            150,
                            Color.White,
                            1.0f
                        );
                        dust.noGravity = true;
                        dust.fadeIn = 0.5f;
                    }

                    // SparkParticle 环绕
                    if (Main.rand.NextBool(4)) // 控制密度
                    {
                        Vector2 perpendicularVel = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(-1f, 1f);

                        Particle spark = new SparkParticle(
                            Projectile.Center + offset,
                            perpendicularVel,
                            false,
                            20, // 生命周期
                            0.5f, // 大小
                            Color.White
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

            }





            if (Projectile.timeLeft <= 80) // 如果弹幕剩余时间小于等于80
                Projectile.velocity *= 0.97f; // 缓慢减小弹幕速度

            Time++;
        }

        public override bool? CanDamage() => Time >= 6f; // 初始的时候不会造成伤害，直到x为止


        private float PrimitiveWidthFunction(float completionRatio) // 计算弹幕宽度变化
        {
            float arrowheadCutoff = 0.36f; // 箭头部分的截止点
            float width = 24f; // 设置默认宽度为24
            float minHeadWidth = 0.03f; // 设置最小宽度为0.03
            float maxHeadWidth = width; // 最大宽度为24
            if (completionRatio <= arrowheadCutoff) // 如果进度比小于截止点
                width = MathHelper.Lerp(minHeadWidth, maxHeadWidth, Utils.GetLerpValue(0f, arrowheadCutoff, completionRatio, true)); // 计算渐变宽度
            return width; // 返回计算后的宽度
        }

        private Color PrimitiveColorFunction(float completionRatio) // 计算弹幕颜色变化
        {
            float endFadeRatio = 0.41f; // 结束渐变比例
            float completionRatioFactor = 2.7f; // 完成比例因子
            float globalTimeFactor = 5.3f; // 全局时间因子
            float endFadeFactor = 3.2f; // 结束渐变因子
            float endFadeTerm = Utils.GetLerpValue(0f, endFadeRatio * 0.5f, completionRatio, true) * endFadeFactor; // 结束渐变项
            float cosArgument = completionRatio * completionRatioFactor - Main.GlobalTimeWrappedHourly * globalTimeFactor + endFadeTerm; // 计算余弦参数
            float startingInterpolant = (float)Math.Cos(cosArgument) * 0.5f + 0.5f; // 计算颜色插值

            float colorLerpFactor = 0.6f; // 颜色渐变因子
            Color startingColor = Color.Lerp(ShaderColorOne, ShaderColorTwo, startingInterpolant * colorLerpFactor); // 计算起始颜色
            return Color.Lerp(startingColor, ShaderEndColor, MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0f, endFadeRatio, completionRatio, true))); // 返回渐变后的颜色
        }

        public override bool PreDraw(ref Color lightColor) // 在绘制前的操作
        {
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak")); // 设置着色器的纹理
            Vector2 overallOffset = Projectile.Size * 0.5f; // 计算整体偏移量
            overallOffset += Projectile.velocity * 1.4f; // 调整偏移量
            int numPoints = 46; // 点的数量
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints); // 渲染拖尾效果
            return false; // 不执行默认绘制
        }

        public override void OnKill(int timeLeft) // 当弹幕被销毁时的操作
        {
            if (Projectile.ai[1] == 0) // 如果ai[1]为0
            {
                Player Owner = Main.player[Projectile.owner]; // 获取弹幕所有者
                float targetDist = Vector2.Distance(Owner.Center, Projectile.Center); // 计算弹幕与玩家的距离

                NPC target = Projectile.Center.ClosestNPCAt(2000); // 查找距离弹幕最近的NPC（2000像素内）
                Vector2 targetPosition = Projectile.numHits == 1 ? target == null ? Projectile.Center : target.Center : altSpawn; // 确定目标位置
                Vector2 spawnSpot = (Projectile.numHits == 1 ? target == null ? Projectile.Center : target.Center : altSpawn) + new Vector2(Main.rand.NextFloat(-450, 450), Main.rand.NextFloat(750, 950)); // 计算生成点

                Vector2 velocity; // 定义速度向量
                if (target == null) // 如果没有目标
                    velocity = (targetPosition - spawnSpot).SafeNormalize(Vector2.UnitX) * 20; // 计算安全归一化速度
                else
                    velocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(spawnSpot, target, 20f, MaxUpdate); // 使用预测算法计算速度

                if (targetDist < 1400f) // 如果目标距离小于1400像素
                {
                    int Dusts = 8; // 定义生成的粒子数量
                    float radians = MathHelper.TwoPi / Dusts; // 计算每个粒子的弧度
                    Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f)); // 定义旋转点
                    for (int i = 0; i < Dusts; i++) // 生成粒子
                    {
                        Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i) * 3.5f; // 计算粒子速度
                                                                                            // 生成浅绿色粒子特效
                        //Dust dust = Dust.NewDustPerfect(spawnSpot, DustID.GreenFairy, dustVelocity, 0, Color.LightGreen, 0.9f); // 生成粒子1，浅绿色
                        //dust.noGravity = true; // 粒子无重力

                        // 生成深绿色粒子特效
                        //Dust dust2 = Dust.NewDustPerfect(spawnSpot, DustID.GreenFairy, dustVelocity * 0.6f, 0, Color.DarkGreen, 1.2f); // 生成粒子2，深绿色
                        //dust2.noGravity = true; // 粒子无重力
                    }
                }
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slimed, 300); // 原版的史莱姆效果
        }



    }
}
