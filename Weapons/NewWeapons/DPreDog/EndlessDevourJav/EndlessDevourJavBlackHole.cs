using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using Terraria.Audio;
using CalamityThrowingSpear.LightingBolts.Shader;
using CalamityMod.Graphics.Metaballs;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav
{
    internal class EndlessDevourJavBlackHole : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        // public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 center = Projectile.Center - Main.screenPosition;
            float pulse = 1f + 0.04f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
            float time = Main.GlobalTimeWrappedHourly;


            // 计算缩放比例随 timeLeft 缩小
            float lifeProgress = 1f - Projectile.timeLeft / 120f; // 0~1 (假设最大120，可根据实际弹幕life修改)
            float scaleMultiplier = MathHelper.Lerp(1f, 0.2f, lifeProgress); // 🚩 从 1.0 缩小至 0.2

            // === 1️⃣ 外层亮橙色动态呼吸光环（SmallBloomRing） ===
            Texture2D smallRing = ModContent.Request<Texture2D>("CalamityMod/Particles/SmallBloomRing").Value;
            Color ringColor = Color.Lerp(Color.MediumPurple, Color.MediumPurple, 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f));
            ringColor *= 0.5f;
            ringColor.A = 0;
            Main.EntitySpriteDraw(
                smallRing,
                center,
                null,
                ringColor,
                -time * 1.2f,
                smallRing.Size() * 0.5f,
                2.0f * pulse * scaleMultiplier, // 🚩 加入缩放随时间缩小
                SpriteEffects.None,
                0
            );

            // === 2️⃣ 中层亮橙旋转吸积盘臂（twirl_01 / twirl_02 / twirl_03） ===
            string[] twirlTextures = new string[]
            {
    "CalamityThrowingSpear/Texture/KsTexture/twirl_01",
    "CalamityThrowingSpear/Texture/KsTexture/twirl_02",
    "CalamityThrowingSpear/Texture/KsTexture/twirl_03"
            };
            
            for (int layer = 0; layer < 3; layer++)
            {
                Texture2D twirl = ModContent.Request<Texture2D>(twirlTextures[layer]).Value;
                float angle = time * (1.2f + layer * 0.3f);
                Color swirlColor = Color.Lerp(Color.MediumPurple, Color.MediumPurple, 0.5f + 0.5f * (float)Math.Sin(time * 3f + layer));
                swirlColor *= 0.6f;
                swirlColor.A = 0;

                Main.EntitySpriteDraw(
                    twirl,
                    center,
                    null,
                    swirlColor,
                    angle,
                    twirl.Size() * 0.5f,
                    (1.5f - layer * 0.15f) * scaleMultiplier, // 🚩 加入缩放随时间缩小
                    SpriteEffects.None,
                    0
                );
            }

            //Main.spriteBatch.End();
            //// === 3️⃣ 绘制黑洞黑色圆（已修复锚点，防止漂移和拉条） ===
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            ////Texture2D blackPixel = TextureAssets.MagicPixel.Value;
            //Texture2D blackCircle = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;
            //Vector2 center1 = Projectile.Center - Main.screenPosition;
            //float radius = 80f * Projectile.scale; // 黑洞半径

            //Main.spriteBatch.Draw(
            //    blackCircle,
            //    center1,
            //    null,
            //    Color.Black,
            //    0f,
            //    blackCircle.Size() * 0.5f,
            //    radius * 2f / blackCircle.Width, // 将直径除以贴图宽度（或高度，因其是正方形）
            //    SpriteEffects.None,
            //    0f
            //);





            Main.spriteBatch.End();

            // === 应用【黑洞扭曲】着色器，绘制弹幕自身贴图 ===
            Effect shader = ShaderGames.BlackHoleDistortionShader;
            if (shader == null)
                return true;

            // 传入时间（用于动态螺旋流动）
            shader.Parameters["uTime"].SetValue(Main.GameUpdateCount / 60f);

            // 🚩 使用不会偏移的中心写法：
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            //shader.Parameters["uCenter"].SetValue(
            //    drawPos / new Vector2(Main.screenWidth, Main.screenHeight)
            //);
            shader.Parameters["uCenter"].SetValue(new Vector2(0.5f, 0.5f));


            // 🚩 暴露可调参数（建议范围）：
            float blackHoleRadius = 0.25f;   // 影响黑洞吞噬范围（0.2 ~ 0.4）
            float blackHoleStrength = 0.12f; // 扭曲强度（0.05 ~ 0.2）
            shader.Parameters["uRadius"].SetValue(blackHoleRadius);
            shader.Parameters["uStrength"].SetValue(blackHoleStrength);

            shader.CurrentTechnique.Passes[0].Apply();

            // 启动 SpriteBatch 使用 Shader
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.Default, RasterizerState.CullNone, shader, Main.GameViewMatrix.TransformationMatrix);

            // 🚩 使用修正后位置绘制弹幕本体
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            Main.spriteBatch.Draw(
                texture,
                drawPos,
                null,
                Color.White,
                Projectile.rotation,
                texture.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

            Main.spriteBatch.End();

            // 恢复后续绘制状态
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;





        }






        private int soundTimer = 0;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 1; // 完全不透明
        }
        private Vector2 lockedScreenPosition; // 黑洞生成时锁死的绘制位置
        private Vector2 lockedScreenCenterUV;

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            lockedScreenPosition = Projectile.Center; // 记录生成时的固定位置（世界坐标）

            lockedScreenCenterUV = (Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY)) / new Vector2(Main.screenWidth, Main.screenHeight);

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EndlessDevourJavBlackHole2>(), // “2号黑洞”
                    0,        // 伤
                    0,        // 击
                    Projectile.owner
                );
            }


            // === 黑洞诞生夸张超新星坍缩特效 ===
            Vector2 spawnPosition = Projectile.Center;
            Color blackColor = Color.Black;
            float initialScale = 55.0f;        // 🚩 更大初始范围（可调）
            float finalScale = 1.0f;          // 🚩 缩小至的最终范围（可调）
            int lifetime = 240;                // 🚩 持续帧数（可调）
            float rotationSpeed = Main.rand.NextFloat(-20f, 20f); // 🚩 更快随机旋转速度

            // 创建大范围黑洞坍缩特效
            Particle blackHoleCollapse = new CustomPulse(
                spawnPosition,
                Vector2.Zero,
                blackColor,
                "CalamityMod/Particles/LargeBloom",
                new Vector2(0.5f, 0.5f),
                -rotationSpeed,
                initialScale,
                finalScale,
                lifetime
            );
            GeneralParticleHandler.SpawnParticle(blackHoleCollapse);
        }


        private int orbShootTimer = 0;
        private int pulseTimer = 0;

        public override void AI()
        {

            float lifeProgress = 1f - (Projectile.timeLeft / 250f); // 0 ~ 1
            float scaleFactor = MathHelper.Lerp(160f, 3200f, lifeProgress);

            Vector2 centerBefore = Projectile.Center; // 🚩 记录修改前中心
            Projectile.width = (int)scaleFactor;
            Projectile.height = (int)scaleFactor;
            Projectile.Center = centerBefore;

            // 接收被传入的蓄力影响值（可持续使用）
            float chargeFactor = Projectile.ai[0];

            // 示例用途：
            // - 控制范围：
            float effectiveRadius = 120f + chargeFactor * 15f;
            // - 控制吸力强度：
            float pullStrength = 2f + chargeFactor * 0.3f;
            // - 控制特效数量：
            int particleDensity = 10 + (int)(chargeFactor * 2f);

            // 此处根据需要将上述变量应用到：
            // 吸力逻辑 / 粒子生成数量 / 播放音效强度 等等

            // 后续继续写黑洞核心逻辑...




            {
                // ========================== 🌌 黑洞持续“吸吐”机制 ==========================
                orbShootTimer++;

                // 计算随时间递增的速度和频率
                float progress = 1f - Projectile.timeLeft / 600f; // 可调黑洞存在时长，如 600（0~1）
                float minSpeedSmall = MathHelper.Lerp(8f, 18f, progress);  // EndlessDevourJavOrbSmall
                float maxSpeedSmall = MathHelper.Lerp(14f, 28f, progress);
                float minSpeedLarge = MathHelper.Lerp(10f, 10f, progress); // EndlessDevourJavOrb
                float maxSpeedLarge = MathHelper.Lerp(12f, 12f, progress);

                // 吐（高频率） - EndlessDevourJavOrbSmall
                int smallShootInterval = (int)MathHelper.Lerp(20, 5, progress); // 从20帧加快到5帧

                if (orbShootTimer % smallShootInterval == 0)
                {
                    int shootCount = 4; // 同时喷射数（可调）

                    // 🚩 计算递增伤害倍率
                    float progressDamage = 1f - Projectile.timeLeft / 600f; // 0 ~ 1
                    float damageMultiplier = MathHelper.Lerp(0.1f, 1f, progressDamage);

                    // 限制最大倍率 = 3.5f
                    damageMultiplier = Math.Min(damageMultiplier, 1f);

                    for (int i = 0; i < shootCount; i++)
                    {
                        Vector2 direction = Main.rand.NextVector2Unit();
                        Vector2 velocity = direction * Main.rand.NextFloat(minSpeedSmall, maxSpeedSmall);

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            velocity,
                            ModContent.ProjectileType<EndlessDevourJavOrbSmall>(),
                            (int)(Projectile.damage * damageMultiplier), // 🚩 递增伤害
                            0f,
                            Projectile.owner
                        );
                    }
                }

                // 吸（低频率） - EndlessDevourJavOrb
                int largeShootInterval = (int)MathHelper.Lerp(60, 20, progress); // 从60帧加快到20帧

                if (orbShootTimer % largeShootInterval == 0)
                {
                    int spawnCount = 2; // 每次生成数量（可调）
                    float spawnRadius = 800f; // 在外圈生成半径（可调）

                    for (int i = 0; i < spawnCount; i++)
                    {
                        Vector2 randomOffset = Main.rand.NextVector2Unit() * spawnRadius;
                        Vector2 spawnPosition = Projectile.Center + randomOffset;

                        Vector2 directionToBlackHole = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.UnitY);
                        Vector2 velocity = directionToBlackHole * Main.rand.NextFloat(minSpeedLarge, maxSpeedLarge);

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            spawnPosition,
                            velocity,
                            ModContent.ProjectileType<EndlessDevourJavOrb>(),
                            Projectile.damage / 2, // 可调伤害倍率
                            0f,
                            Projectile.owner
                        );
                    }
                }

            }



            {
                // ===== 🌀 黑洞吸引附近敌怪（略微增强 Cyclone 吸力） =====
                float attractionRange = 800f; // 吸引范围（可调）
                float attractionStrength = 0.92f; // 吸引强度（Cyclone 原为 0.05f，此处增强）

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    // 排除不可被追踪、无敌或 boss
                    if (!npc.CanBeChasedBy(this, false) || npc.boss || npc.dontTakeDamage || npc.friendly)
                        continue;

                    // 检查距离范围
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < attractionRange)
                    {
                        // 计算方向
                        Vector2 direction = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);

                        // 吸引速度受距离影响（越近吸力越强）
                        float distanceFactor = 1f - (distance / attractionRange); // 0 (远) ~ 1 (近)

                        // 应用速度（加速度式，非直接重置速度）
                        npc.velocity += direction * attractionStrength * distanceFactor;

                        // 可选：限制最大加速度，避免超速
                        float maxPullSpeed = 8f; // 可调
                        if (npc.velocity.Length() > maxPullSpeed)
                        {
                            npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * maxPullSpeed;
                        }
                    }
                }
            }


            
            // 在黑洞的 AI() 内部添加：
            pulseTimer++;
            float progress1 = 1f - Projectile.timeLeft / 240; // 存在期间0~1

            // 冲击波生成间隔随时间递减（从30帧递减到5帧）
            int pulseInterval = (int)MathHelper.Lerp(30, 5, progress1);

            if (pulseTimer >= pulseInterval)
            {
                Particle pulse = new CustomPulse(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.Lerp(Color.DarkViolet, Color.Black, 0.3f), // 暗紫色冲击波
                    "CalamityMod/Particles/LargeBloom",
                    new Vector2(1f, 1f),
                    Main.rand.NextFloat(-10, 10), // 随机自转
                    3.5f, // 起始缩放
                    2f,   // 结束缩放
                    20    // 寿命
                );
                GeneralParticleHandler.SpawnParticle(pulse);

                pulseTimer = 0;
            }


            {
                // 在 AI() 内部
                soundTimer++;

                // 控制触发频率（每 8 帧播放一次，可自行调节快慢）
                if (Projectile.timeLeft > 10 && soundTimer > 8)
                {
                    // Pitch 随时间线性上升，模拟吸力越来越强
                    float progress = 1f - Projectile.timeLeft / 120f; // 存在期间0~1
                    float pitch = MathHelper.Lerp(-0.5f, 0.4f, progress);

                    SoundEngine.PlaySound(SoundID.Item93 with { Pitch = pitch }, Projectile.Center);
                    soundTimer = 0;
                }
            }


            {
                // ===== 4️⃣ SparkParticle 构建亮黄色吸积盘 =====
                int particlesPerFrame = 44; // 每帧生成数量（可调）
                float radius = 60f * Projectile.scale; // 黑洞贴图半径，按需调整
                float angularSpeed = 2f; // 角速度，控制圆周移动速度（可调）

                for (int i = 0; i < particlesPerFrame; i++)
                {
                    // 计算在圆周上的角度位置（基于时间偏移保证流动感）
                    float angle = Main.GlobalTimeWrappedHourly * angularSpeed + MathHelper.TwoPi * i / particlesPerFrame;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Vector2 spawnPos = Projectile.Center + offset;

                    // 计算切向速度（与圆周切线方向一致，90°）
                    Vector2 tangentialVelocity = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY) * 8f; // 可调速度

                    SparkParticle spark = new SparkParticle(
                        spawnPos,
                        tangentialVelocity,
                        false,
                        Main.rand.Next(30, 50), // 寿命（可调）
                        Main.rand.NextFloat(1.0f, 2.0f), // 大小（可调）
                        Color.MediumPurple * 0.9f // 亮黄色吸积盘
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            {
                // ======== 黑洞持续吸引混乱恐怖特效（最终极明显强化版） ========
                Vector2 center = Projectile.Center;
                float time = Main.GlobalTimeWrappedHourly;

                int spawnCount = 64; // 🚩 极大量粒子
                float spawnRadius = 3000f; // 🚩 超远距离生成可见
                float targetRadius = 100f;

                for (int i = 0; i < spawnCount; i++)
                {
                    Vector2 randomOffset = Main.rand.NextVector2Unit() * Main.rand.NextFloat(spawnRadius * 0.5f, spawnRadius);
                    Vector2 spawnPos = center + randomOffset;

                    Vector2 toCenter = (center - spawnPos);
                    float distance = toCenter.Length();

                    // 🚩 极高速度，远处极快吸入，形成长拖尾
                    Vector2 velocity = toCenter.SafeNormalize(Vector2.Zero) * MathHelper.Lerp(40f, 120f, distance / spawnRadius);

                    // === 1️⃣ Dust（深色可见流动） ===
                    if (Main.rand.NextBool(1)) // 每次必生成
                    {
                        int dustType = Main.rand.NextBool() ? DustID.Shadowflame : DustID.DarkCelestial;
                        int dust = Dust.NewDust(spawnPos, 0, 0, dustType);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity = velocity;
                        Main.dust[dust].scale = Main.rand.NextFloat(2f, 4f); // 🚩 提升体积
                        Main.dust[dust].fadeIn = 1.2f;
                        Main.dust[dust].color = Color.Lerp(Color.DarkViolet, Color.Black, 0.3f); // 深紫混黑更可见
                    }

                    // === 2️⃣ SparkParticle（暗色线性闪电拖尾） ===
                    if (Main.rand.NextBool(1)) // 每次必生成
                    {
                        SparkParticle spark = new SparkParticle(
                            spawnPos,
                            velocity * 1.015f,
                            false,
                            Main.rand.Next(30, 40), // 寿命保证拖尾可见
                            Main.rand.NextFloat(1.0f, 4.0f), // 🚩 体积大
                            Color.MediumPurple * 0.9f 
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    Color smokeColor = Color.Lerp(Color.BlueViolet, Color.Black, 0.7f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f));
                    // === 3️⃣ HeavySmokeParticle（深色重烟） ===
                    if (Main.rand.NextBool(2)) // 半概率生成
                    {
                        Particle smokeH = new HeavySmokeParticle(
                            spawnPos,
                            velocity * 2.5f,
                            smokeColor, // 深蓝烟雾
                            Main.rand.Next(50, 70),
                            Main.rand.NextFloat(1f, 2.5f), // 🚩 大体积
                            2.8f,
                            Main.rand.NextFloat(-0.02f, 0.02f),
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(smokeH);
                    }
                }

                // 黑洞本体轻微旋转动感
                //Projectile.rotation += 0.02f;
            }







        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPosition = target.Center;
            Vector2 projectileDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);

            // =============================== 1️⃣ 深蓝深紫 LavaMetaball 爆裂 ===============================
            int lavaParticleCount = 20; // 🔹 Lava 粒子数量（可调）
            float spawnRadiusMin = 60f; // 🔹 Lava 半径范围（可调）
            float spawnRadiusMax = 100f;

            for (int i = 0; i < lavaParticleCount; i++)
            {
                Vector2 spawnOffset = Main.rand.NextVector2Circular(32f, 32f); // 🔹 偏移范围（可调）
                float radius = Main.rand.NextFloat(spawnRadiusMin, spawnRadiusMax);

                // 颜色渲染改不了[大悲]
                RancorLavaMetaball.SpawnParticle(
                    hitPosition + spawnOffset,
                    radius
                );
            }

            // =============================== 2️⃣ Spark 粒子网从击中方向喷射 ===============================
            int sparkCount = 60; // 🔹 Spark 粒子数量（可调）
            float sparkSpeedMin = 8f; // 🔹 Spark 速度范围（可调）
            float sparkSpeedMax = 16f;
            Color sparkColor = Color.Lerp(Color.DarkViolet, Color.DarkBlue, 0.5f); // 深紫深蓝

            for (int i = 0; i < sparkCount; i++)
            {
                // 生成从击中方向附近喷射的随机方向，带少量偏差
                Vector2 direction = projectileDirection.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f));
                Vector2 velocity = direction * Main.rand.NextFloat(sparkSpeedMin, sparkSpeedMax);

                SparkParticle spark = new SparkParticle(
                    hitPosition + Main.rand.NextVector2Circular(8f, 8f), // 🔹 生成位置略带偏移
                    velocity,
                    false,
                    Main.rand.Next(20, 30),             // 寿命（可调）
                    Main.rand.NextFloat(0.4f, 0.7f),    // 大小（可调）
                    sparkColor * 0.8f                   // 颜色与透明度
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }





        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center;



            {

                //// ========================== 🌌 黑洞超新星级重烟爆发（HeavySmokeParticle 强化） ==========================
                //int smokeCount = 240; // 超新星原 30，我们提升 8 倍（可调）
                //float smokeRadius = 400f; // 扩大生成环范围（可调）
                //float smokeSpeed = 20f; // 略提速保持张力

                //Color smokeColor = new Color(57, 46, 115) * 1.2f; 

                //for (int i = 0; i < smokeCount; i++)
                //{
                //    Vector2 randVel = new Vector2(smokeRadius * 0.5f, smokeRadius * 0.5f).RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.8f, 1.4f);
                //    Vector2 pos = center + randVel;
                //    Vector2 vel = randVel.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(8f, smokeSpeed); // 均匀四散

                //    Particle smoke = new HeavySmokeParticle(
                //        pos,
                //        vel,
                //        smokeColor,
                //        Main.rand.Next(45, 60),          // 寿命保持长留
                //        Main.rand.NextFloat(1.5f, 3.5f), // 体积适中偏大
                //        8.6f,                             // 不透明度（清晰可见）
                //        Main.rand.NextFloat(-0.03f, 0.03f), // 轻微自转
                //        true
                //    );
                //    GeneralParticleHandler.SpawnParticle(smoke);
                //}

                //// ========================== 🌌 六向真实 Supernova 规格 GlowSparkParticle 光柱 ==========================
                //int beamDirections = 6; // 6 条主光柱
                //List<float> angles = new List<float>();

                //// 生成 6 个相隔足够远的方向（≥ 15°）
                //while (angles.Count < beamDirections)
                //{
                //    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                //    if (!angles.Any(a => Math.Abs(MathHelper.WrapAngle(a - angle)) < MathHelper.ToRadians(15f)))
                //        angles.Add(angle);
                //}

                //// 每条光柱方向
                //foreach (float angle in angles)
                //{
                //    Vector2 dir = angle.ToRotationVector2();

                //    for (int strand = -40; strand <= 40; strand += 8) // ✅ 完全与 Supernova 相同范围
                //    {
                //        // 内层密集短射线
                //        GlowSparkParticle spark = new GlowSparkParticle(
                //            center + dir.RotatedBy(MathHelper.PiOver2) * strand,
                //            dir * Main.rand.NextFloat(1f, 20.5f),
                //            false,
                //            Main.rand.Next(40, 50),                      // 寿命完全一致
                //            Main.rand.NextFloat(0.04f, 0.095f),          // 超小尺寸一致
                //            Color.Red,                                   // Supernova 用色
                //            new Vector2(0.3f, 1.6f),                     // 横向拉伸一致
                //            true
                //        );
                //        GeneralParticleHandler.SpawnParticle(spark);

                //        // 反向射线
                //        GlowSparkParticle spark2 = new GlowSparkParticle(
                //            center - dir.RotatedBy(MathHelper.PiOver2) * strand,
                //            -dir * Main.rand.NextFloat(1f, 20.5f),
                //            false,
                //            Main.rand.Next(40, 50),
                //            Main.rand.NextFloat(0.04f, 0.095f),
                //            Color.MediumTurquoise,                       // Supernova 用色
                //            new Vector2(0.3f, 1.6f),
                //            true
                //        );
                //        GeneralParticleHandler.SpawnParticle(spark2);
                //    }
                //}


                //// ========================== 4️⃣ 暗紫色冲击波（CustomPulse） ==========================
                //for (int i = 0; i < 8; i++)
                //{
                //    Particle pulse = new CustomPulse(
                //        center,
                //        Vector2.Zero,
                //        Color.Lerp(Color.DarkViolet, Color.Black, 0.3f),
                //        "CalamityMod/Particles/LargeBloom",
                //        new Vector2(1f, 1f),
                //        Main.rand.NextFloat(-30f, 30f),
                //        12f - i * 0.8f,
                //        6f - i * 0.5f,
                //        60
                //    );
                //    GeneralParticleHandler.SpawnParticle(pulse);
                //}

            }


            //// 播放黑洞塌缩深沉音效
            //SoundEngine.PlaySound(SoundID.Item62, center);

            //// 屏幕震动极大
            //Main.LocalPlayer.Calamity().GeneralScreenShakePower = 20f;
        }










    }
}