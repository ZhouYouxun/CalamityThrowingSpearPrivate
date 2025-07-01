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

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav
{
    internal class EndlessDevourJavBlackHole : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

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
            Color ringColor = Color.Lerp(Color.OrangeRed, Color.Gold, 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f));
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
                Color swirlColor = Color.Lerp(Color.Orange, Color.Gold, 0.5f + 0.5f * (float)Math.Sin(time * 3f + layer));
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





            return false;
        }






        private int soundTimer = 0;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 255; // 完全透明
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // === 黑洞诞生夸张超新星坍缩特效 ===
            Vector2 spawnPosition = Projectile.Center;
            Color blackColor = Color.Black;
            float initialScale = 5.0f;        // 🚩 更大初始范围（可调）
            float finalScale = 1.0f;          // 🚩 缩小至的最终范围（可调）
            int lifetime = 120;                // 🚩 持续帧数（可调）
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

        public override void AI()
        {
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
                orbShootTimer++;

                // 计算当前射击间隔（随时间缩短）
                float progress = 1f - Projectile.timeLeft / 120f; // 0~1
                int shootInterval = (int)MathHelper.Lerp(45, 6, progress); // 从45帧加快到6帧

                if (orbShootTimer >= shootInterval)
                {
                    // 基础速度区间随时间增加（越来越快）
                    float minSpeed = MathHelper.Lerp(12f, 20f, progress);
                    float maxSpeed = MathHelper.Lerp(20f, 40f, progress);

                    for (int i = 0; i < 3; i++)
                    {
                        // 每发略微偏移，形成多向放射效果
                        Vector2 direction = Main.rand.NextVector2Unit().RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));
                        Vector2 velocity = direction * Main.rand.NextFloat(minSpeed, maxSpeed);

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            velocity,
                            ModContent.ProjectileType<EndlessDevourJavOrb>(),
                            Projectile.damage / 2, // 可调伤害倍率
                            0f,
                            Projectile.owner
                        );
                    }

                    orbShootTimer = 0;
                }
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
                            velocity * 1.05f,
                            false,
                            Main.rand.Next(40, 60), // 寿命保证拖尾可见
                            Main.rand.NextFloat(1.0f, 5.0f), // 🚩 体积大
                            Color.Orange * 0.9f // 深红拖尾可见
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    // === 3️⃣ HeavySmokeParticle（深色重烟） ===
                    if (Main.rand.NextBool(2)) // 半概率生成
                    {
                        Particle smokeH = new HeavySmokeParticle(
                            spawnPos,
                            velocity * 2.5f,
                            Color.Orange, // 深蓝烟雾
                            Main.rand.Next(50, 70),
                            Main.rand.NextFloat(2f, 3.5f), // 🚩 大体积
                            2.8f,
                            Main.rand.NextFloat(-0.02f, 0.02f),
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(smokeH);
                    }
                }

                // 黑洞本体轻微旋转动感
                Projectile.rotation += 0.02f;
            }








        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }





        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center;

            // 播放黑洞塌缩深沉音效
            SoundEngine.PlaySound(SoundID.Item62, center);

            // 屏幕震动极大
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = 20f;

            // === 1️⃣ 大范围黑烟极速扩散 ===
            for (int i = 0; i < 120; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(20f, 60f);
                Particle smoke = new HeavySmokeParticle(
                    center + velocity * 0.3f,
                    velocity,
                    Color.Black,
                    Main.rand.Next(40, 60),
                    Main.rand.NextFloat(1.2f, 2.5f),
                    0.6f,
                    Main.rand.NextFloat(-0.1f, 0.1f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // === 2️⃣ 大量高速 SparkParticle 黑色射线爆发 ===
            for (int i = 0; i < 100; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(40f, 80f);
                SparkParticle spark = new SparkParticle(
                    center,
                    velocity,
                    false,
                    Main.rand.Next(30, 50),
                    Main.rand.NextFloat(0.8f, 1.4f),
                    Color.Black * 0.95f
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // === 3️⃣ 黑色 GlowSparkParticle 短速爆裂电火花 ===
            for (int i = 0; i < 50; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(30f, 90f);
                GlowSparkParticle glowSpark = new GlowSparkParticle(
                    center,
                    velocity,
                    false,
                    Main.rand.Next(20, 30),
                    Main.rand.NextFloat(0.15f, 0.25f),
                    Color.Black,
                    new Vector2(2f, 0.5f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(glowSpark);
            }

            // === 4️⃣ 黑色 & 深紫 Dust 高速环状爆发 ===
            for (int i = 0; i < 180; i++)
            {
                float angle = MathHelper.TwoPi * i / 180f + Main.rand.NextFloat(-0.05f, 0.05f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(30f, 100f);
                int dustType = Main.rand.NextBool() ? DustID.Shadowflame : DustID.DarkCelestial;
                int dust = Dust.NewDust(center, 0, 0, dustType);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = velocity;
                Main.dust[dust].scale = Main.rand.NextFloat(1.2f, 2.2f);
                Main.dust[dust].color = Color.Black;
            }

            // === 5️⃣ 黑色冲击波爆发（CustomPulse） ===
            for (int i = 0; i < 6; i++)
            {
                Particle pulse = new CustomPulse(
                    center,
                    Vector2.Zero,
                    Color.Black,
                    "CalamityMod/Particles/LargeBloom",
                    new Vector2(1f, 1f),
                    Main.rand.NextFloat(-20f, 20f),
                    6f - i * 0.4f,
                    3f - i * 0.3f,
                    40
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // === 6️⃣ 黑色火焰爆裂（FlameExplosion） ===
            for (int i = 0; i < 10; i++)
            {
                Particle flame = new CustomPulse(
                    center,
                    Vector2.Zero,
                    Color.Black * 0.8f,
                    "CalamityMod/Particles/FlameExplosion",
                    new Vector2(1f, 1f),
                    Main.rand.NextFloat(-15f, 15f),
                    5f,
                    0f,
                    50
                );
                GeneralParticleHandler.SpawnParticle(flame);
            }
        }










    }
}