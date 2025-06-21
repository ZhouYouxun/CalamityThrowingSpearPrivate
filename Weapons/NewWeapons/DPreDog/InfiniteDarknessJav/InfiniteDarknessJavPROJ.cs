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
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using CalamityMod.Sounds;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav
{
    public class InfiniteDarknessJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/InfiniteDarknessJav/InfiniteDarknessJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        private Vector2 initialVelocity;

        private int frameCounter = 0; // 计数器
        private bool hasTarget = false; // 是否找到目标

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        private bool hideProjectile = false; // 开关：是否隐藏弹幕
        public override bool PreDraw(ref Color lightColor)
        {
            if (hideProjectile)
            {
                // 如果开启了隐藏开关，不绘制任何内容
                return false;
            }
            if (phase == 1) // 第一阶段：快速消失并准备传送
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            }
            else if (phase >= 2) // 第二阶段及后续：更换拖尾效果
            {
                // 获取绘图批处理器和拖尾效果的纹理
                SpriteBatch spriteBatch = Main.spriteBatch;
                Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/InfiniteDarknessJav/InfiniteDarknessJav").Value;

                // 遍历弹幕的历史位置，绘制拖尾效果
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    // 动态计算颜色插值，用于在紫色和黑色之间渐变
                    float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;

                    // 通过插值生成颜色，主色调为紫黑色或深灰色
                    Color color = Color.Lerp(Color.DarkViolet, Color.Purple, colorInterpolation) * 0.4f;
                    color.A = 0; // 设置透明度，确保柔和的视觉效果

                    // 计算每个历史位置的绘制位置
                    Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                    // 计算拖尾的缩放比例，随着拖尾位置逐渐减弱
                    Vector2 outerScale = new Vector2(2f) * MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);

                    // 绘制拖尾效果
                    Main.EntitySpriteDraw(
                        lightTexture,       // 拖尾纹理
                        drawPosition,       // 绘制位置
                        null,               // 绘制区域（null 表示绘制整个纹理）
                        color * 1.35f,              // 拖尾颜色
                        Projectile.rotation,// 旋转角度
                        lightTexture.Size() * 0.5f, // 设置中心点
                        outerScale * 0.6f,  // 缩放比例
                        SpriteEffects.None, // 无翻转效果
                        0                   // 层级深度
                    );
                }
            }
            return false;
        }

        public override void SetDefaults()
        {
            initialVelocity = Projectile.velocity; // Record initial velocity
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            Projectile.alpha = 0;
        }

        private int phase = 1;
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            if (phase == 1)
            {
                // 第1阶段：每帧增加12点透明度
                Projectile.alpha += 30;
                if (Projectile.alpha >= 300)
                {
                    Projectile.alpha = 300;
                    frameCounter++; // 增加计数器

                    // 切换到第二阶段
                    phase = 2;
                    frameCounter = 0;
                }
            }
            else if (phase == 2)
            {

                // 搜索最近的敌人
                NPC target = FindClosestNPC(1500f);
                if (target != null)
                {
                    // 传送到目标附近
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation() + MathHelper.PiOver4;
                    Vector2 teleportPosition = target.Center + Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 16 * 16;
                    Projectile.position = teleportPosition - new Vector2(Projectile.width / 2, Projectile.height / 2);

                    {
                        // 传送后的六芒星特效
                        float radius = 10 * 16; // 六芒星的半径

                        // 绘制正三角形
                        for (int i = 0; i < 3; i++) // 正三角形的3条边
                        {
                            float angle = MathHelper.PiOver2 + i * MathHelper.TwoPi / 3f; // 当前顶点角度
                            float nextAngle = MathHelper.PiOver2 + (i + 1) * MathHelper.TwoPi / 3f; // 下一个顶点角度

                            Vector2 start = angle.ToRotationVector2() * radius; // 当前顶点位置
                            Vector2 end = nextAngle.ToRotationVector2() * radius; // 下一个顶点位置

                            for (int j = 0; j < 40; j++) // 每条边生成40个粒子
                            {
                                Vector2 position = Vector2.Lerp(start, end, j / 40f) + Projectile.Center; // 插值计算粒子位置
                                int dustType = Main.rand.Next(new int[] { DustID.DemonTorch }); // 随机选择粒子类型

                                Dust dust = Dust.NewDustPerfect(position, dustType, null, 100, default, 1.8f);
                                dust.velocity = Vector2.Zero; // 粒子无速度
                                dust.noGravity = true; // 禁用重力
                            }
                        }

                        // 绘制倒三角形
                        for (int i = 0; i < 3; i++) // 倒三角形的3条边
                        {
                            float angle = -MathHelper.PiOver2 + i * MathHelper.TwoPi / 3f; // 当前顶点角度
                            float nextAngle = -MathHelper.PiOver2 + (i + 1) * MathHelper.TwoPi / 3f; // 下一个顶点角度

                            Vector2 start = angle.ToRotationVector2() * radius; // 当前顶点位置
                            Vector2 end = nextAngle.ToRotationVector2() * radius; // 下一个顶点位置

                            for (int j = 0; j < 40; j++) // 每条边生成40个粒子
                            {
                                Vector2 position = Vector2.Lerp(start, end, j / 40f) + Projectile.Center; // 插值计算粒子位置
                                int dustType = Main.rand.Next(new int[] { DustID.DemonTorch }); // 随机选择粒子类型

                                Dust dust = Dust.NewDustPerfect(position, dustType, null, 100, default, 1.8f);
                                dust.velocity = Vector2.Zero; // 粒子无速度
                                dust.noGravity = true; // 禁用重力
                            }
                        }
                    }

                    // 在目标的另一个点生成 InfiniteDarknessJavPROJStarBomb
                    Vector2 bombPosition = target.Center + Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 16 * 16;

                    // 计算与目标相反的方向速度
                    Vector2 reverseVelocity = -(Projectile.velocity.SafeNormalize(Vector2.Zero)) * Projectile.velocity.Length();

                    // 生成炸弹，并赋予其初始反向速度
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        bombPosition,
                        reverseVelocity, // 初始速度为反向速度
                        ModContent.ProjectileType<InfiniteDarknessJavPROJStarBomb>(),
                        (int)(Projectile.damage * 0.27f),
                        0,
                        Projectile.owner
                    );

                    // 设定透明度为完全透明，并进入下一阶段
                    Projectile.alpha = 255; // 传送后设为完全透明
                    Projectile.velocity = Vector2.Normalize(target.Center - Projectile.Center) * 10f; // 向目标冲刺
                    phase = 3;
                }
                else
                {
                    // 未找到目标敌人，开启隐藏开关
                    hideProjectile = true;
                }

                //{
                //    // 生成轻型烟雾粒子效果，左右偏离10度
                //    int smokeCount = 25;
                //    for (int i = 0; i < smokeCount; i++)
                //    {
                //        // 左右偏离 10 度
                //        float angleOffset = MathHelper.ToRadians(10) * (i % 2 == 0 ? 1 : -1);
                //        Vector2 dustVelocity = Projectile.velocity.RotatedBy(angleOffset);

                //        Particle smoke = new HeavySmokeParticle(
                //            Projectile.Center,
                //            dustVelocity * Main.rand.NextFloat(1f, 2.6f),
                //            Color.Black, // 使用黑色
                //            18,
                //            Main.rand.NextFloat(0.9f, 1.6f),
                //            0.35f,
                //            Main.rand.NextFloat(-1, 1),
                //            true
                //        );
                //        GeneralParticleHandler.SpawnParticle(smoke);
                //    }
                //}
            }
            else if (phase == 3)
            {
                // 第三阶段：逐渐降低透明度，使弹幕变得可见
                Projectile.alpha -= 20;
                Projectile.velocity = Projectile.velocity * 1.07f;
                if (Projectile.alpha < 0)
                {
                    Projectile.alpha = 0; // 保持完全可见状态
                }

                //// 单螺旋粒子特效
                //float spiralAmplitude = 16f; // 螺旋的振幅（左右滑动范围）
                //float spiralFrequency = 0.2f; // 螺旋的频率（滑动速度）
                //float offset = (float)Math.Sin(Main.GameUpdateCount * spiralFrequency) * spiralAmplitude; // 计算偏移量

                //// 粒子生成位置
                //Vector2 spiralPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * offset;

                //// 生成粒子
                //Dust dust = Dust.NewDustPerfect(spiralPosition, 191);
                //dust.scale = 1.5f; // 粒子大小
                //dust.velocity = Projectile.velocity * 0.3f; // 粒子初速度
                //dust.noGravity = true; // 禁用重力
            }



        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int slashDamage = Projectile.damage;
            int slashCount = 3; // 每次击中敌人都会释放X道斩杀

            if (phase == 1) // 第一阶段：传送前，造成 更高的 伤害
            {
                slashDamage = (int)(Projectile.damage * 3.25f);
            }
            else if (phase == 3) // 第三阶段：传送后，造成 较低的 伤害
            {
                slashDamage = (int)(Projectile.damage * 0.75f);
            }


            // 生成斩杀弹幕
            for (int i = 0; i < slashCount; i++)
            {
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, randomDirection, ModContent.ProjectileType<BlackSLASH>(), slashDamage, Projectile.knockBack, Projectile.owner);
            }

            // 播放斩杀音效
            SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = 0.5f }, Projectile.Center);
        }



        public override void OnKill(int timeLeft)
        {
            // 释放两道斩杀BlackSLASH
            for (int i = 0; i < 2; i++)
            {
                Vector2 slashPosition = Projectile.Center + Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 16;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashPosition, Vector2.Zero, ModContent.ProjectileType<BlackSLASH>(), (int)(Projectile.damage * 0.27f), 0, Projectile.owner);
            }



            //if (hasTarget && attackPhase == 3)
            //{
            //    int dustAmount = 400; // 增加粒子数量
            //    float angleOffset = MathHelper.ToRadians(15);

            //    for (int i = 0; i < dustAmount; i++)
            //    {
            //        // 使用两个椭圆的分布，一个向上，一个向下
            //        float ellipseRadiusX = Main.rand.NextFloat(6f, 18f);
            //        float ellipseRadiusY = Main.rand.NextFloat(3f, 9f);

            //        // 随机选择椭圆方向
            //        Vector2 baseDirection = i % 2 == 0 ? new Vector2(ellipseRadiusX, ellipseRadiusY) : new Vector2(ellipseRadiusX, -ellipseRadiusY);

            //        // 设置方向并添加偏移
            //        Vector2 direction = baseDirection.RotatedBy(i % 2 == 0 ? angleOffset : -angleOffset) * Main.rand.NextFloat(0.8f, 1.2f);
            //        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, direction * Main.rand.NextFloat(1f, 2f), 150, Color.Black, 1.2f); // 设为小粒子
            //        dust.noGravity = true;
            //    }
            //}
            //else
            //{
            //    // 未找到敌人而自毁时生成均匀散布的黑色 Dust 粒子
            //    int dustAmount = 150;
            //    for (int i = 0; i < dustAmount; i++)
            //    {
            //        Vector2 direction = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 4f);
            //        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, direction, 150, Color.Black, 1.5f);
            //        dust.noGravity = true;
            //    }
            //}


            {
                // 粒子箭头的特效
                int arrowSegments = 40; // 每条边生成的粒子数量
                float arrowLength = 16 * 6; // 箭头的总长度
                float arrowWidth = 16 * 2; // 箭头的宽度

                // 计算箭头的关键点
                Vector2 arrowTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * arrowLength; // 箭头尖端
                Vector2 leftBase = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * arrowWidth; // 左后端点
                Vector2 rightBase = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2) * arrowWidth; // 右后端点

                // 1. 绘制主箭头部分（从左后方和右后方延伸到尖端）
                for (int i = 0; i < arrowSegments; i++)
                {
                    // 左侧主线
                    Vector2 leftLinePos = Vector2.Lerp(leftBase, arrowTip, i / (float)arrowSegments);
                    int dustType = Main.rand.Next(new int[] {173});
                    Dust leftDust = Dust.NewDustPerfect(leftLinePos, dustType, Vector2.Zero, 150, default, 1.5f);
                    leftDust.noGravity = true;

                    // 右侧主线
                    Vector2 rightLinePos = Vector2.Lerp(rightBase, arrowTip, i / (float)arrowSegments);
                    Dust rightDust = Dust.NewDustPerfect(rightLinePos, dustType, Vector2.Zero, 150, default, 1.5f);
                    rightDust.noGravity = true;
                }

                // 2. 绘制箭头基部（从左后方和右后方延伸到弹幕中心）
                for (int i = 0; i < arrowSegments; i++)
                {
                    // 左侧基线
                    Vector2 leftBaseLinePos = Vector2.Lerp(leftBase, Projectile.Center, i / (float)arrowSegments);
                    int dustType = Main.rand.Next(new int[] {173});
                    Dust leftBaseDust = Dust.NewDustPerfect(leftBaseLinePos, dustType, Vector2.Zero, 150, default, 1.5f);
                    leftBaseDust.noGravity = true;

                    // 右侧基线
                    Vector2 rightBaseLinePos = Vector2.Lerp(rightBase, Projectile.Center, i / (float)arrowSegments);
                    Dust rightBaseDust = Dust.NewDustPerfect(rightBaseLinePos, dustType, Vector2.Zero, 150, default, 1.5f);
                    rightBaseDust.noGravity = true;
                }
            }
           
        }

        // 寻找最近的敌人
        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float minDistance = maxDetectDistance;
            foreach (NPC npc in Main.npc)
            {
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (npc.CanBeChasedBy(this) && distance < minDistance)
                {
                    minDistance = distance;
                    closestNPC = npc;
                }
            }
            return closestNPC;
        }
    }
}

