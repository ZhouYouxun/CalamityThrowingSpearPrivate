using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Sounds;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch.FTDragon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FinishingTouch";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
            Main.projFrames[Projectile.type] = 4; // 设置投射物的帧数为 4
        }
        private float dnaWaveCounter = 0f; // 用于计算螺旋偏移波动

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // === 本体部分 ===
            // 计算当前动画帧
            int frameCount = 4;
            int frameHeight = texture.Height / frameCount;
            int currentFrame = (int)(Main.GameUpdateCount / 6 % frameCount);
            Rectangle sourceRectangle = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);

            // 设置绘制原点和位置
            Vector2 drawOrigin = new Vector2(texture.Width / 2, frameHeight / 2);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // 绘制当前帧
            spriteBatch.Draw(texture, drawPosition, sourceRectangle, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);






            // === 螺旋双线平行拖尾部分 ===

            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/spark_07")
            );

            // 左右基础偏移距离
            float offsetDistance = 10f;

            // 使用飞行距离驱动 DNA 波动
            float travelLength = Projectile.Center.Length() * 0.000015f; // 可替换为 Projectile.Distance(Main.LocalPlayer.Center) * 0.05f 根据实际需要
            float freqMultiplier = 1.5f;
            float dnaOffset = (float)Math.Sin(travelLength * freqMultiplier) * 8f; // 螺旋幅度

            // 平行向量
            Vector2 perpendicular = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
            Vector2 leftOffset = perpendicular * offsetDistance + perpendicular * dnaOffset;
            Vector2 rightOffset = -perpendicular * offsetDistance - perpendicular * dnaOffset;

            // 提前约 30 像素到贴图前端
            Vector2 frontOffset = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 100f;

            // 渲染左侧拖尾
            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    FinishingTouchWidthFunction,
                    FinishingTouchColorFunction,
                    (_) => Projectile.Size * 0.5f + frontOffset + leftOffset,
                    shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]
                ),
                36
            );

            // 渲染右侧拖尾
            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    FinishingTouchWidthFunction,
                    FinishingTouchColorFunction,
                    (_) => Projectile.Size * 0.5f + frontOffset + rightOffset,
                    shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]
                ),
                36
            );


            return false;
        }

        /// <summary>
        /// 拖尾宽度函数：基础宽度 + 微抖动，模拟火焰呼吸感
        /// </summary>
        private float FinishingTouchWidthFunction(float completionRatio)
        {
            float baseWidth = 18f;
            float flicker = (float)Math.Sin(completionRatio * 6f + Main.GlobalTimeWrappedHourly * 4f) * 2f;
            return baseWidth + flicker;
        }

        /// <summary>
        /// 拖尾颜色函数：橙红 → 黄橙 → 透明的平滑渐变
        /// </summary>
        private Color FinishingTouchColorFunction(float completionRatio)
        {
            float intensity = (float)Math.Sin(completionRatio * 10f + Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f;
            Color baseColor = Color.Lerp(Color.OrangeRed, Color.Orange, intensity);
            return Color.Lerp(baseColor, Color.Transparent, completionRatio);
        }





        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 允许1次伤害
            Projectile.timeLeft = 60;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 1; // 无敌帧冷却时间为1帧
        }

        public override void AI()
        {
            // 每 6 帧切换一次帧
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0; // 重置帧计数器
                Projectile.frame++; // 切换到下一帧
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0; // 如果超过了最大帧数，回到第一帧
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);
            Projectile.velocity *= 1.001f;

            // 刚出现时的初始粒子特效
            if (Projectile.timeLeft == 180) // Assuming timeLeft is initially 180
            {
                GenerateInitialParticles();
            }

            // 定义一个偏移距离，用来增加粒子之间的间隔
            float offsetDistance = 20f;

            // 计算特效生成位置，始终在弹幕的正左方和正右方（基于弹幕当前方向）
            Vector2 leftOffset = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * offsetDistance;
            Vector2 rightOffset = Projectile.velocity.RotatedBy(-MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * offsetDistance;

            Vector2 leftTrailPos = Projectile.Center + leftOffset;
            Vector2 rightTrailPos = Projectile.Center + rightOffset;

            // 生成橙红色粒子特效
            Color orangeRed = Color.OrangeRed;
            Particle leftTrail = new SparkParticle(leftTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, orangeRed);
            Particle rightTrail = new SparkParticle(rightTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, orangeRed);
            GeneralParticleHandler.SpawnParticle(leftTrail);
            GeneralParticleHandler.SpawnParticle(rightTrail);




            // 生成左右漂移的轻型白色烟雾特效
            int dustCount = 1; // 每次生成的烟雾数量
            float radians = MathHelper.TwoPi / dustCount;
            Vector2 smokePoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustVelocity = smokePoint.RotatedBy(radians * i).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2.6f);
                Color smokeColor = Color.White;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.Orange, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }


        private void GenerateInitialParticles()
        {
            for (float angle = -15f; angle <= 15f; angle += 1f)
            {
                Vector2 particleDirectionLeft = Projectile.velocity.RotatedBy(MathHelper.ToRadians(angle));
                Vector2 particleDirectionRight = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-angle));

                // 左右方向各释放粒子
                Particle particleLeft = new SparkParticle(Projectile.Center, particleDirectionLeft * 3f, false, 40, 1.5f, Color.OrangeRed);
                Particle particleRight = new SparkParticle(Projectile.Center, particleDirectionRight * 3f, false, 40, 1.5f, Color.OrangeRed);

                GeneralParticleHandler.SpawnParticle(particleLeft);
                GeneralParticleHandler.SpawnParticle(particleRight);
            }
        }

        private void ReleaseFireballs()
        {
            int fireballType = ModContent.ProjectileType<FinishingTouchBALL>();
            float baseAngle = MathHelper.TwoPi / 16; // 每个火球的角度
            int splitCount = 4;

            for (int i = 0; i < splitCount; i++)
            {
                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 spawnPosition = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 70f * 16f;

                Vector2 velocitySPIT = Vector2.Normalize(Projectile.Center - spawnPosition) * 16;

                // 生成分裂长枪，伤害为充能长枪的1/5
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocitySPIT, ModContent.ProjectileType<SagittariusSPIT>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner);
            }


            {
                Vector2 sparkleVelocity = (Projectile.Center - Main.rand.NextVector2Circular(40f, 40f)) // 缩小随机偏移范围
               .SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.25f, 0.75f); // 降低初始速度范围

                Color startColor = Color.OrangeRed * 0.4f;
                Color endColor = Color.LightGoldenrodYellow * 0.8f;
            }
            /*for (int i = 0; i < 16; i++)
            {
                float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);

                // 计算每个弹幕的方向向量
                Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

                // 设定弹幕的速度和伤害
                Vector2 fireballVelocity = baseAngle.ToRotationVector2().RotatedBy(baseAngle * i) * 10f; // 初始速度为原来的8.5倍Main.rand.NextFloat(0.75f, 2f)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireballVelocity, fireballType, (int)(Projectile.damage * 0.275f), Projectile.knockBack, Projectile.owner);
            }*/
        }


        private void ReleaseLinearParticles()
        {
            float baseAngle = MathHelper.TwoPi / 24; // 20个粒子的扩散角度

            for (int i = 0; i < 24; i++)
            {
                Vector2 trailPos = Projectile.Center;
                Vector2 trailVelocity = baseAngle.ToRotationVector2().RotatedBy(baseAngle * i) * 0.2f;
                Color trailColor = Color.OrangeRed;
                float trailScale = 1.5f;

                Particle trail = new SparkParticle(trailPos, trailVelocity, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(trail);
            }
        }


        public static bool UseDragonSnakeMode = false;


        public override void OnKill(int timeLeft)
        {
            ReleaseFireballs();
            ReleaseLinearParticles();

            {
                {
                    if (!UseDragonSnakeMode)
                    {
                        // 开关关闭，默认召唤 FinishingTouchINV
                        int invProjType = ModContent.ProjectileType<FinishingTouchINV>();
                        Vector2 shootDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                        float shootSpeed = Projectile.velocity.Length();
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            shootDirection * shootSpeed * 2,
                            invProjType,
                            (int)(Projectile.damage * 0.75f),
                            Projectile.knockBack,
                            Projectile.owner
                        );
                    }
                    else
                    {
                        // 开关开启时，使用单文件新蛇 FinishingTouchDragon
                        Vector2 spawnPosition = Projectile.Center;
                        Vector2 spawnVelocity = Vector2.UnitY * -1f * 25f; // 固定正上方发射

                        int proj = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            spawnPosition,
                            spawnVelocity,
                            ModContent.ProjectileType<FinishingTouchDragon>(),
                            (int)(Projectile.damage * 10),
                            Projectile.knockBack,
                            Projectile.owner
                        );

                        // 保险设置为 A 方案（即便默认就是 A）
                        if (proj.WithinBounds(Main.maxProjectiles))
                        {
                            (Main.projectile[proj].ModProjectile as FinishingTouchDragon)?.SetBPlan(false);
                        }


                    }



                }

                // 🌀 2️⃣ 生成 16 个橙色椭圆冲击波粒子（等角分布）
                int pulseCount = 16;
                float baseAngle = MathHelper.TwoPi / pulseCount;
                float particleSpeed = Projectile.velocity.Length() * 0.75f;
                for (int i = 0; i < pulseCount; i++)
                {
                    float angle = baseAngle * i;
                    Vector2 velocity = angle.ToRotationVector2() * particleSpeed;

                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center,
                        velocity,
                        Color.Orange,
                        new Vector2(1f, 2.5f), // 椭圆长短轴比例
                        Projectile.rotation - MathHelper.PiOver4,
                        0.2f,
                        0.03f,
                        20
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 龙焰

            for (int i = 0; i < 5; i++)
            {
                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 spawnPosition = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 70f * 16f;

                Vector2 velocitySPIT = Vector2.Normalize(Projectile.Center - spawnPosition) * 16;

                // 生成分裂长枪，伤害为充能长枪的1/5
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocitySPIT, ModContent.ProjectileType<FinishingTouchEcho>(), (int)(Projectile.damage * 0.75), Projectile.knockBack, Projectile.owner);
            }


            {
                Vector2 sparkleVelocity = (Projectile.Center - Main.rand.NextVector2Circular(40f, 40f)) // 缩小随机偏移范围
               .SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.25f, 0.75f); // 降低初始速度范围

                Color startColor = Color.OrangeRed * 0.4f;
                Color endColor = Color.LightGoldenrodYellow * 0.8f;
            }

            /*int slashCount = 2; // 生成2到3个斩击特效
            for (int i = 0; i < slashCount; i++)
            {
                // 随机生成方向
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                int slashID = ModContent.ProjectileType<OrangeSLASH>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, randomDirection, slashID, (int)(Projectile.damage * 2.5f), Projectile.knockBack, Projectile.owner);
            }*/

            // 给予5秒钟的创造胜利
            int buffDuration = 5 * 60; // 5 秒钟，单位为帧（每秒 60 帧）
            target.AddBuff(ModContent.BuffType<CreateVictoryPEBuff>(), buffDuration);


            SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.5f }, Projectile.Center);
        }

    }
}

