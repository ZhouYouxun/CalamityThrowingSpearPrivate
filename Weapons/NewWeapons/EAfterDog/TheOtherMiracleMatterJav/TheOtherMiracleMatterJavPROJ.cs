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
using CalamityMod.Projectiles.Ranged;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityMod.Sounds;
using Terraria.Audio;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Buffs.DamageOverTime;
using CalamityRangerExpansion.LightingBolts;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TheOtherMiracleMatterJav
{
    public class TheOtherMiracleMatterJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TheOtherMiracleMatterJav/TheOtherMiracleMatterJav";

        private Vector2 initialVelocity;
        private int stage = 0;
        private int stageTimer = 0;
        private int bounces = 0;
        public int Time = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16; // 将缓存长度增加到16，拖尾会更长
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bool isRetracting = returning || stage == 3;

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D textureGlow = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TheOtherMiracleMatterJav/TheOtherMiracleMatterJav").Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float localIdentityOffset = Projectile.identity * 0.1372f;
            Color mainColor = CalamityUtils.MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f + localIdentityOffset) % 1f, Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
            Color secondaryColor = CalamityUtils.MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f + localIdentityOffset + 0.2f) % 1f, Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
            mainColor = Color.Lerp(Color.White, mainColor, 0.85f);
            secondaryColor = Color.Lerp(Color.White, secondaryColor, 0.85f);

            float chargeOffset = 3f;
            Color chargeColor = Color.Lerp(Color.Lime, Color.Cyan, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.1f) * 0.5f + 0.5f) * 0.6f;
            chargeColor.A = 0;

            // ✅根据速度判断是否翻转，并在朝左飞时额外加90度旋转✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅✅
            bool facingLeft = Projectile.velocity.X < 0;
            SpriteEffects direction = facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float rotation = (isRetracting ? Projectile.rotation : Projectile.velocity.ToRotation() + MathHelper.PiOver4) + (facingLeft ? MathHelper.PiOver2 : 0f);

            // 绘制环状光晕
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            }

            // 设置拖尾着色器
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak"));
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].UseColor(mainColor);
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].UseSecondaryColor(secondaryColor);
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].Apply();

            if (!isRetracting)
            {
                // 普通拖尾
                PrimitiveRenderer.RenderTrail(
                    Projectile.oldPos,
                    new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"]),
                    53
                );
            }
            else
            {
                // 回收时拖尾
                Vector2 headPosition = Projectile.Center + new Vector2(16f * 3f, 0f).RotatedBy(Projectile.rotation);
                Vector2[] headTrail = Enumerable.Repeat(headPosition, 10).ToArray();

                PrimitiveRenderer.RenderTrail(
                    headTrail,
                    new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"]),
                    53
                );
            }

            // 绘制本体和发光层
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);
            Main.spriteBatch.Draw(textureGlow, drawPosition, null, Color.White, rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }



        public float PrimitiveWidthFunction(float completionRatio) => Projectile.scale * 30f;

        public Color PrimitiveColorFunction(float _) => Color.Lime * Projectile.Opacity;

        private Vector2 desiredDirection;
        private float travelDuration = 90f; // 第一阶段总时长
        private float maxStage1Duration = 60f;
        private float travelDuration1 = 60f; // 加速阶段时长
        private float travelDuration2 = 60f; // 减速阶段时长
        private float travelDuration3 = 90f; // 倒退阶段时长

        private float maxSpeed = 40f;

        private Vector2 spawnPosition;
        private bool returning = false;
        private float flownDistance = 0f;
        private float maxDistance = 70f * 16f; // 1120px
        private float baseSpeed = 22f;
        private int soundTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 不与方块碰撞
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            // 设置 spawnPosition
            if (stageTimer == 0 && stage == 0)
            {
                spawnPosition = Projectile.Center;
                desiredDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.velocity = desiredDirection * baseSpeed;
            }

            // 设置 spawnPosition
            if (stageTimer == 0 && stage == 0)
            {
                spawnPosition = Projectile.Center;
                desiredDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.velocity = desiredDirection * baseSpeed;
            }

            // =======================
            // 0️⃣ 发射阶段（飞行 30 帧后返回）
            // =======================
            if (!returning)
            {
                stageTimer++;

                // progress: 0 ~ 1 over 30 frames
                float progress = stageTimer / 30f;
                progress = MathHelper.Clamp(progress, 0f, 1f);

                // EaseInOutCubic
                float speedFactor;
                if (progress < 0.5f)
                    speedFactor = 4f * progress * progress * progress;
                else
                {
                    float p = -2f * progress + 2f;
                    speedFactor = 1f - p * p * p / 2f;
                }

                float currentSpeed = baseSpeed + (baseSpeed * speedFactor); // 在22f基础上增加最高可达44f
                Projectile.velocity = desiredDirection * currentSpeed;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                // 飞行满 30 帧，触发回收
                if (stageTimer >= 30)
                {
                    returning = true;
                    stageTimer = 0;

                    {


                        // 🚀 超级回收爆发特效（增强版）

                        int particleStreams = 12; // 更多射线填充空间
                        float spreadAngle = MathHelper.ToRadians(60f); // 更广角度范围
                        Vector2 forward = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();

                        for (int i = 0; i < particleStreams; i++)
                        {
                            float angleOffset = MathHelper.Lerp(-spreadAngle / 2f, spreadAngle / 2f, i / (float)(particleStreams - 1));
                            Vector2 dir = forward.RotatedBy(angleOffset);

                            // ⭐ 光粒（SquishyLightParticle）主角
                            for (int j = 0; j < 6; j++)
                            {
                                float spiralAngle = angleOffset + Main.rand.NextFloat(-0.1f, 0.1f);
                                Vector2 spiralOffset = new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * Main.rand.NextFloat(8f, 18f);
                                Vector2 spawnPosition = Projectile.Center + spiralOffset;

                                SquishyLightParticle exoEnergy = new(
                                    spawnPosition,
                                    dir * Main.rand.NextFloat(1.5f, 4.5f), // 速度提升 3 倍
                                    0.36f,
                                    Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.3f, 0.7f)),
                                    35
                                );
                                GeneralParticleHandler.SpawnParticle(exoEnergy);
                            }

                            // ⚡ SparkParticle （中强度射线填充）
                            for (int j = 0; j < 8; j++)
                            {
                                Particle spark = new SparkParticle(
                                    Projectile.Center,
                                    dir * Main.rand.NextFloat(18f, 36f), // 速度提升
                                    false,
                                    35,
                                    Main.rand.NextFloat(1.2f, 2.0f),
                                    Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.2f, 0.6f))
                                );
                                GeneralParticleHandler.SpawnParticle(spark);
                            }

                            // ✴️ Dust 填充（中量）
                            for (int j = 0; j < 5; j++)
                            {
                                Dust dust = Dust.NewDustPerfect(
                                    Projectile.Center,
                                    DustID.Electric,
                                    dir * Main.rand.NextFloat(12f, 28f),
                                    100,
                                    Color.Cyan,
                                    Main.rand.NextFloat(1.3f, 2.0f)
                                );
                                dust.noGravity = true;
                            }

                            // 🌫️ 重型烟雾（少量点缀）
                            if (Main.rand.NextBool(3))
                            {
                                Particle smoke = new HeavySmokeParticle(
                                    Projectile.Center,
                                    dir * Main.rand.NextFloat(6f, 14f),
                                    new Color(180, 220, 255) * 0.9f,
                                    Main.rand.Next(25, 40),
                                    Main.rand.NextFloat(1.4f, 2.4f),
                                    0.4f,
                                    Main.rand.NextFloat(-0.02f, 0.02f),
                                    false
                                );
                                GeneralParticleHandler.SpawnParticle(smoke);
                            }
                        }
                        // 🚀 超级回收爆发特效 结束




                    }

                }
            }
            // =======================
            // 1️⃣ 回收阶段
            // =======================
            else
            {
                stageTimer++;

                {
                    // 每 2 帧喷射一次（可调整）
                    if (stageTimer % 2 == 0)
                    {
                        int particleCount = 5; // 每次喷射5个

                        Vector2 forward = Projectile.rotation.ToRotationVector2();

                        for (int i = 0; i < particleCount; i++)
                        {
                            // 窄范围 ±10°扰动
                            float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f));
                            Vector2 dir = forward.RotatedBy(angleOffset);

                            // ⭐ 光粒 (SquishyLightParticle)
                            SquishyLightParticle lightParticle = new(
                                Projectile.Center + dir * 12f,
                                dir * Main.rand.NextFloat(20f, 36f), // 快速
                                0.32f,
                                Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.3f, 0.7f)),
                                20
                            );
                            GeneralParticleHandler.SpawnParticle(lightParticle);

                            // ⚡ SparkParticle
                            Particle spark = new SparkParticle(
                                Projectile.Center + dir * 8f,
                                dir * Main.rand.NextFloat(18f, 34f), // 快速
                                false,
                                25,
                                Main.rand.NextFloat(1.0f, 1.5f),
                                Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.2f, 0.6f))
                            );
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                    }
                }

                {
                    // 播放越来越快、音调越来越高的音效
                    soundTimer++;
                    int maxInterval = 20; // 初始间隔（帧）
                    int minInterval = 8;  // 最快间隔（帧）
                    float progress = MathHelper.Clamp(stageTimer / 120f, 0f, 1f); // 0~1 over 120 frames

                    int currentInterval = (int)MathHelper.Lerp(maxInterval, minInterval, progress);
                    float pitch = MathHelper.Lerp(-0.3f, 0.5f, progress); // 从较低到较高

                    if (soundTimer >= currentInterval)
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/VividClarityBeamAppear")
                        {
                            Pitch = pitch,
                            Volume = 1.6f
                        }, Projectile.Center);

                        soundTimer = 0;
                    }

                }


                // 设置无限穿透，仅设置一次
                if (Projectile.penetrate != -1)
                    Projectile.penetrate = -1;

                // 让它体积变大一点
                Projectile.width = Projectile.height = 180;


                Vector2 toPlayer = (Main.player[Projectile.owner].Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float returnSpeed = MathHelper.Lerp(4f, 32f, stageTimer / 60f); // 回收期间加速
                Projectile.velocity = toPlayer * returnSpeed;

                // 高速递增旋转
                float rotationSpeed = MathHelper.ToRadians(10f + 5f * (stageTimer / 60f)); // 10°~15°/tick
                Projectile.rotation += rotationSpeed;

                if (Projectile.Hitbox.Intersects(Main.player[Projectile.owner].Hitbox))
                {
                    Projectile.Kill();
                }
            }







            if (Main.GameUpdateCount % 4 == 0) // 每 4 帧生成一次
            {
                float angle1 = Main.GameUpdateCount * 0.015f; // 更缓慢旋转
                float angle2 = -Main.GameUpdateCount * 0.012f;
                Vector2 dirShort = angle1.ToRotationVector2();
                Vector2 dirLong = angle2.ToRotationVector2();
                CTSLightingBoltsSystem.Spawn_ParallelPlasmaLines(Projectile.Center, dirShort, dirLong);
            }








        }

        public override void OnKill(int timeLeft)
        {
            //SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/380mmExploded"), Projectile.Center);
            // 如果需要，可以在这里添加更多的特效或音效
            //SoundEngine.PlaySound(Photoviscerator.HitSound, Projectile.Center);

            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TheOtherMiracleMatterJavEXP>(), (int)(Projectile.damage * 1f), Projectile.knockBack, Projectile.owner);

            {
                Vector2 center = Projectile.Center;

                // ==================== 1️⃣ 光粒（主角） ====================
                int lightParticleCount = 150;
                for (int i = 0; i < lightParticleCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dir = angle.ToRotationVector2();
                    Vector2 velocity = dir * Main.rand.NextFloat(10f, 24f);

                    SquishyLightParticle light = new SquishyLightParticle(
                        center,
                        velocity,
                        0.42f,
                        Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.3f, 0.7f)),
                        35
                    );
                    GeneralParticleHandler.SpawnParticle(light);
                }

                // ==================== 2️⃣ Spark（线性爆裂） ====================
                int sparkCount = 180;
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dir = angle.ToRotationVector2();
                    Vector2 velocity = dir * Main.rand.NextFloat(16f, 34f);

                    SparkParticle spark = new SparkParticle(
                        center,
                        velocity,
                        false,
                        40,
                        Main.rand.NextFloat(1.0f, 2.0f),
                        Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.2f, 0.5f))
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // ==================== 3️⃣ Dust（电光填充） ====================
                int dustCount = 250;
                for (int i = 0; i < dustCount; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(14f, 26f);
                    Dust dust = Dust.NewDustPerfect(
                        center,
                        DustID.Electric,
                        velocity,
                        100,
                        Color.Cyan,
                        Main.rand.NextFloat(1.2f, 2.0f)
                    );
                    dust.noGravity = true;
                }

                // ==================== 4️⃣ 重型烟雾（少量点缀） ====================
                int smokeCount = 60;
                float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float[] triangleOffsets = { 0f, MathHelper.TwoPi / 3f, MathHelper.TwoPi * 2f / 3f };

                for (int i = 0; i < smokeCount; i++)
                {
                    float triAngle = baseAngle + triangleOffsets[i % 3] + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 dir = triAngle.ToRotationVector2() * Main.rand.NextFloat(20f, 50f);
                    Particle smoke = new HeavySmokeParticle(
                        center + dir,
                        dir * 0.15f,
                        new Color(180, 220, 255) * 0.85f,
                        Main.rand.Next(35, 55),
                        Main.rand.NextFloat(1.4f, 2.8f),
                        0.5f,
                        Main.rand.NextFloat(-0.02f, 0.02f),
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

                // ==================== 5️⃣ 中心闪光环 ====================
                Particle blastRing2 = new CustomPulse(
                    center,
                    Vector2.Zero,
                    Color.White,
                    "CalamityMod/Particles/FlameExplosion",
                    Vector2.One * 0.8f,
                    Main.rand.NextFloat(-10f, 10f),
                    0.07f,
                    0.4f,
                    40
                );
                GeneralParticleHandler.SpawnParticle(blastRing2);
            }






            // 屏幕震动效果
            float shakePower = 3f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);



            // 消亡时释放明黄色爆炸特效
            Particle blastRing = new CustomPulse(
                Projectile.Center, Vector2.Zero, Color.White,
                "CalamityMod/Particles/FlameExplosion",
                Vector2.One * 0.5f, Main.rand.NextFloat(-10f, 10f),
                0.07f, 0.33f, 30
            );
            GeneralParticleHandler.SpawnParticle(blastRing);
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
            // 播放斩击音效
            //SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.5f }, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ExobladeBigHit"));


            {
                // 冷色脉冲冲击波
                Particle pulse = new DirectionalPulseRing(
                    target.Center,
                    Vector2.Zero,
                    Color.Cyan,
                    new Vector2(1.5f, 1.5f),
                    Main.rand.NextFloat(6f),
                    0.18f,
                    0.02f,
                    25
                );
                GeneralParticleHandler.SpawnParticle(pulse);

                int pulseCount = Main.rand.Next(6, 10);
                for (int i = 0; i < pulseCount; i++)
                {
                    Vector2 randomVelocity = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(0.5f, 1.2f); // 微扩散
                    float randomScale = Main.rand.NextFloat(0.8f, 1.6f);
                    float randomRotation = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                    Color[] possibleColors = {
        Color.White,
        new Color(255, 255, 180),
        new Color(180, 220, 255)
    };
                    Color chosenColor = possibleColors[Main.rand.Next(possibleColors.Length)];

                    Particle flash = new CustomPulse(
                        target.Center,
                        randomVelocity,
                        chosenColor,
                        "CalamityMod/Particles/FlameExplosion",
                        Vector2.One * randomScale,
                        randomRotation,
                        0.1f,
                        0.12f,
                        18
                    );
                    GeneralParticleHandler.SpawnParticle(flash);
                }


            }

            // 新增闪电召唤逻辑
            int lightningDamage = (int)(Projectile.damage * 0.2f); // 可调整的伤害倍率
            for (int i = 0; i < 3; i++) // 设置生成闪电次数
            {
                Vector2 lightningSpawnPosition = target.Center - Vector2.UnitY.RotatedByRandom(0.36f) * Main.rand.NextFloat(960f, 1020f);
                Vector2 lightningShootVelocity = (target.Center - lightningSpawnPosition).SafeNormalize(Vector2.UnitY) * 14f;
                int lightning = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    lightningSpawnPosition,
                    lightningShootVelocity,
                    ModContent.ProjectileType<TheOtherMiracleMatterJavExoLightningBolt>(), // 替换为你的闪电弹幕类型
                    lightningDamage,
                    0f,
                    Projectile.owner
                );
                if (Main.projectile.IndexInRange(lightning))
                {
                    Main.projectile[lightning].ai[0] = lightningShootVelocity.ToRotation();
                    Main.projectile[lightning].ai[1] = Main.rand.Next(100);
                }
            }
        }



    }
}