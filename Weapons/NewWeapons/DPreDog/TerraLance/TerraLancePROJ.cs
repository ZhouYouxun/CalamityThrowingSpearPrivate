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
            // 获取纹理
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // 计算每帧的高度
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int y = frameHeight * Projectile.frame;

            // 投射物的缩放比例和裁剪区域
            float scale = Projectile.scale;
            Rectangle rectangle = new Rectangle(0, y, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;

            // 当前弹幕的翻转状态
            SpriteEffects effects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
            {
                // 如果 spriteDirection 为 -1，则水平和垂直翻转图像
                effects = SpriteEffects.FlipHorizontally;
                //effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            }

            // 残影特效
            Vector2 drawOffset = Projectile.Size / 2f; // 居中偏移
            Color alpha = Projectile.GetAlpha(lightColor);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 获取历史位置、旋转和翻转状态
                Vector2 position = Projectile.oldPos[i] + drawOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                float rotation2 = Projectile.oldRot[i];
                SpriteEffects effects2 = (Projectile.oldSpriteDirection[i] == -1)
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

            return false; // 阻止原始投射物图像的默认绘制
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

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加绿色光源
            Lighting.AddLight(Projectile.Center, Color.Green.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 每10帧生成翠绿色椭圆形粒子特效
            if (Projectile.ai[0] % 10 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Projectile.velocity * 0.75f, Color.Green, new Vector2(1f, 2.5f), Projectile.rotation - MathHelper.PiOver4, 0.2f, 0.03f, 20);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }

            // 前30帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 24)
            {
                NPC target = Projectile.Center.ClosestNPCAt(2400); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 18f, 0.08f); // 追踪速度为12f
                }
            }
            else
            {
                Projectile.ai[1]++;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center;

            // 更复杂的同心圆法阵
            int numCircles = 5; // 圆的数量增加
            int numPointsPerCircle = 18; // 每个圆的粒子数量增加
            float radiusIncrement = 40f; // 圆之间的半径缩小，使整体更加紧凑

            for (int circle = 0; circle < numCircles; circle++)
            {
                float radius = (circle + 1) * radiusIncrement;
                for (int i = 0; i < numPointsPerCircle; i++)
                {
                    float angle = MathHelper.TwoPi * i / numPointsPerCircle;
                    Vector2 position = center + angle.ToRotationVector2() * radius;

                    for (int j = 0; j < 10; j++) // 每个点释放更多粒子
                    {
                        float speed = MathHelper.Lerp(3f, 10f, j / 10f); // 更宽范围的速度
                        Color particleColor = Color.Lerp(Color.White, Color.LimeGreen, j / 10f); // 保持原有颜色渐变
                        float scale = MathHelper.Lerp(1.8f, 0.6f, j / 10f); // 更强的缩放对比

                        Dust magicDust = Dust.NewDustPerfect(position, 107);
                        magicDust.velocity = angle.ToRotationVector2() * speed;
                        magicDust.color = particleColor;
                        magicDust.scale = scale;
                        magicDust.noGravity = true;
                    }
                }
            }

            // 添加更复杂的旋转法阵
            for (int i = 0; i < 72; i++) // 粒子数量翻倍
            {
                float angle = MathHelper.TwoPi * i / 72f;
                Vector2 position = center + angle.ToRotationVector2() * 25f;

                Dust spinningDust = Dust.NewDustPerfect(position, 107);
                spinningDust.velocity = angle.ToRotationVector2() * 6f; // 更高的旋转速度
                spinningDust.color = Color.GreenYellow;
                spinningDust.scale = 1.5f; // 更大粒子
                spinningDust.noGravity = true;
            }

            // 魔法阵式 SparkParticle 特效
            int numRings = 3; // 魔法阵的环数
            for (int ring = 0; ring < numRings; ring++)
            {
                float ringRadius = 50f + ring * 30f; // 每个环的半径增加
                int particlesPerRing = 24; // 每个环的粒子数量
                for (int i = 0; i < particlesPerRing; i++)
                {
                    float angle = MathHelper.TwoPi * i / particlesPerRing;
                    Vector2 position = center + angle.ToRotationVector2() * ringRadius;
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f); // 随机速度

                    Particle trail = new SparkParticle(position, velocity, false, 60, Main.rand.NextFloat(1.0f, 1.5f), Color.Green);
                    GeneralParticleHandler.SpawnParticle(trail);
                }
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 造成debuff
            //target.AddBuff(ModContent.BuffType<GlacialState>(), 90); // 冰河时代

            // 在原地生成TerratomereExplosion弹幕，倍率为1.25
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<TerratomereExplosion>(), (int)(Projectile.damage * 1.05f), Projectile.knockBack, Projectile.owner);

            //// 在屏幕外围召唤2个TerraBeam弹幕，伤害为1.25倍，速度为1.5倍
            //for (int i = 0; i < 2; i++)
            //{
            //    Vector2 spawnPosition = Main.rand.NextVector2FromRectangle(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight));
            //    Vector2 beamDirection = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitX) * 1.5f * 10f;
            //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, beamDirection, 132, (int)(Projectile.damage * 1.25f), Projectile.knockBack, Projectile.owner);
            //}

            // 触发TerraBeamStorm，生成一系列TerraBeam弹幕攻击目标
            TerraBeamStorm(target.Center);


            // 生成15到20个线性翠绿色粒子
            //int particleCount = Main.rand.Next(15, 21);
            //for (int i = 0; i < particleCount; i++)
            //{
            //    Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, -6f)).RotatedByRandom(MathHelper.PiOver4);
            //    Particle trail = new SparkParticle(Projectile.Center, velocity, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.Green);
            //    GeneralParticleHandler.SpawnParticle(trail);
            //}

            {
                //// 生成特殊的三角形粒子特效
                //// 定义等边三角形的边长
                //float triangleSideLength = 50f;

                //// 计算等边三角形的三个顶点
                //Vector2 vertex1 = Projectile.Center + new Vector2(0, -triangleSideLength / (float)Math.Sqrt(3)); // 顶部顶点
                //Vector2 vertex2 = Projectile.Center + new Vector2(-triangleSideLength / 2f, triangleSideLength / (2f * (float)Math.Sqrt(3))); // 左下顶点
                //Vector2 vertex3 = Projectile.Center + new Vector2(triangleSideLength / 2f, triangleSideLength / (2f * (float)Math.Sqrt(3))); // 右下顶点

                //// 在三角形的每条边上生成粒子
                //int particlesPerEdge = 20; // 每条边上生成的粒子数

                //// 顶点1到顶点2的边
                //for (int i = 0; i < particlesPerEdge; i++)
                //{
                //    float t = i / (float)particlesPerEdge;
                //    Vector2 pointOnEdge = Vector2.Lerp(vertex1, vertex2, t); // 线性插值获取边上的点
                //    Vector2 velocity = (vertex2 - vertex1).RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(3f, 6f); // 垂直边的速度
                //    Particle trail = new SparkParticle(pointOnEdge, velocity, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.Green);
                //    GeneralParticleHandler.SpawnParticle(trail);
                //}

                //// 顶点2到顶点3的边
                //for (int i = 0; i < particlesPerEdge; i++)
                //{
                //    float t = i / (float)particlesPerEdge;
                //    Vector2 pointOnEdge = Vector2.Lerp(vertex2, vertex3, t);
                //    Vector2 velocity = (vertex3 - vertex2).RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(3f, 6f);
                //    Particle trail = new SparkParticle(pointOnEdge, velocity, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.Green);
                //    GeneralParticleHandler.SpawnParticle(trail);
                //}

                //// 顶点3到顶点1的边
                //for (int i = 0; i < particlesPerEdge; i++)
                //{
                //    float t = i / (float)particlesPerEdge;
                //    Vector2 pointOnEdge = Vector2.Lerp(vertex3, vertex1, t);
                //    Vector2 velocity = (vertex1 - vertex3).RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(3f, 6f);
                //    Particle trail = new SparkParticle(pointOnEdge, velocity, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.Green);
                //    GeneralParticleHandler.SpawnParticle(trail);
                //}
            }





        }

        // 在屏幕外围召唤一系列TerraBeam弹幕，类似于PetalStorm的效果
        private void TerraBeamStorm(Vector2 targetPos)
        {
            // 播放攻击音效
            SoundEngine.PlaySound(SoundID.Item105, targetPos);

            // 设置弹幕类型为TerraBeam（132号）
            int type = ModContent.ProjectileType<TerraLanceBEAM>();
            int numBeams = 8;  // 生成8个TerraBeam弹幕
            var source = Projectile.GetSource_FromThis();
            int beamDamage = (int)(Projectile.damage * 0.9f);  // 伤害调整为1.05倍
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