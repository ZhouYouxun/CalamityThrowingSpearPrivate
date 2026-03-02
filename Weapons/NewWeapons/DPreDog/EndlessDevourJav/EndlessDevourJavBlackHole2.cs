using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav
{
    internal class EndlessDevourJavBlackHole2 : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.timeLeft = 250;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            Projectile.velocity = Vector2.Zero;

            // 寻找唯一存在的黑洞并紧贴
            int blackHoleType = ModContent.ProjectileType<EndlessDevourJavBlackHole>(); // 黑洞弹幕
            Projectile blackHole = Main.projectile.FirstOrDefault(p => p.active && p.type == blackHoleType && p.owner == Projectile.owner);


            // === 🌌 让体积从 16x16 平滑增大到 1600x1600，且中心固定 ===

            /*float lifeProgress = 1f - (Projectile.timeLeft / 250f); // 0 ~ 1
            float scaleFactor = MathHelper.Lerp(16f, 1600f, lifeProgress);

            Vector2 centerBefore = Projectile.Center; // 🚩 记录修改前中心
            Projectile.width = (int)scaleFactor;
            Projectile.height = (int)scaleFactor;
            Projectile.Center = centerBefore;*/         // 🚩 重置回中心

        }

        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center;

            // 播放深沉音效和震屏
            SoundEngine.PlaySound(SoundID.Item62, center);
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = 30f;

            // ========================== 🌌 1️⃣ 重烟（Phoenix's Pride 同款风格 + 路子更野） ==========================
            int smokeCount = 400; // 🚩 更大释放规模
            float smokeRadius = 300f; // 🚩 更大生成范围
            float smokeSpeed = 36f;   // 🚩 更快扩散速度

            for (int i = 0; i < smokeCount; i++)
            {
                float angle = MathHelper.TwoPi * i / smokeCount + Main.rand.NextFloat(-0.05f, 0.05f);
                Vector2 pos = center + angle.ToRotationVector2() * smokeRadius * Main.rand.NextFloat(0.8f, 2.5f);
                Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * smokeSpeed * Main.rand.NextFloat(0.8f, 1.8f);

                // 主暗紫色烟雾（呼吸色）
                Color smokeColor = Color.Lerp(
                    Color.MidnightBlue,
                    Color.Indigo, // 原本是Color.Indigo,
                    0.5f + 0.25f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f)
                );

                float scale = Main.rand.NextFloat(2.8f, 3.2f);       // 🚩 更大体积
                float opacity = 0.9f + Main.rand.NextFloat(0f, 0.2f); // 🚩 更高可见度

                Particle smoke = new HeavySmokeParticle(
                    pos,
                    vel,
                    smokeColor,
                    Main.rand.Next(55, 75), // 🚩 更长寿命（持久散逸）
                    scale,
                    opacity,
                    Main.rand.NextFloat(-0.02f, 0.02f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(smoke);

                // === 偶发橙红高亮点（火星）增强层次感 ===
                if (Main.rand.NextBool(3))
                {
                    Color glowColor = Color.OrangeRed;
                    float glowScale = Main.rand.NextFloat(2.0f, 2.5f);
                    float glowOpacity = opacity * 1.5f;

                    Particle glow = new HeavySmokeParticle(
                        pos,
                        vel * 0.9f,
                        glowColor,
                        Main.rand.Next(40, 50),
                        glowScale,
                        glowOpacity,
                        0f,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(glow);
                }
            }

            // ========================== 2️⃣ Dust 环状喷射 ==========================
            int dustCount = 1200;
            float dustRadius = 240f;
            float dustSpeed = 26f;

            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.05f, 0.05f);
                Vector2 pos = center + angle.ToRotationVector2() * dustRadius * Main.rand.NextFloat(0.6f, 1.8f);
                Vector2 vel = angle.ToRotationVector2() * dustSpeed * Main.rand.NextFloat(0.8f, 1.2f);

                int dustType = Main.rand.NextBool() ? DustID.Shadowflame : DustID.DarkCelestial;
                int dust = Dust.NewDust(pos, 0, 0, dustType);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = vel;
                Main.dust[dust].scale = Main.rand.NextFloat(1.5f, 2.5f);
                Main.dust[dust].fadeIn = 1.0f;
                Main.dust[dust].color = Color.Lerp(Color.DarkViolet, Color.Black, 0.5f);
            }

            // ========================== 🌌 六向分散粗亮巨大光柱（暗紫呼吸风格 + 亮橙火花） ==========================
            int beamDirections = 6;
            List<float> angles = new List<float>();
            while (angles.Count < beamDirections)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                if (!angles.Any(a => Math.Abs(MathHelper.WrapAngle(a - angle)) < MathHelper.ToRadians(15f)))
                    angles.Add(angle);
            }

            foreach (float angle in angles)
            {
                Vector2 dir = angle.ToRotationVector2();
                int particlesPerBeam = 120; // 🚩 略微增加密度
                float maxDistance = 350f;   // 🚩 略微增加长度

                for (int i = 0; i < particlesPerBeam; i++)
                {
                    float distance = Main.rand.NextFloat(0f, maxDistance);
                    Vector2 offset = dir * distance;
                    Vector2 randomSideOffset = dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-40f, 40f); // 🚩 略增粗度
                    Vector2 spawnPos = center + offset + randomSideOffset;
                    Vector2 vel = dir * Main.rand.NextFloat(12f, 26f) + Main.rand.NextVector2Circular(2f, 2f);

                    // === 动态呼吸暗紫色调（Phoenix's Pride 风格） ===
                    Color purpleColor = Color.Lerp(
                        Color.MidnightBlue,
                        Color.Indigo,
                        0.5f + 0.25f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f)
                    );

                    float scale = Main.rand.NextFloat(0.12f, 0.18f); // 🚩 略微增大体积

                    GlowSparkParticle spark = new GlowSparkParticle(
                        spawnPos,
                        vel,
                        false,
                        Main.rand.Next(55, 80),            // 🚩 稍长寿命
                        scale,
                        purpleColor,
                        new Vector2(0.5f, 2.5f),           // 🚩 略微更粗拉伸
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(spark);

                    // === 偶发亮橙红高光火花（补充层次感） ===
                    if (Main.rand.NextBool(8))
                    {
                        Color orangeGlow = Color.OrangeRed;
                        float glowScale = scale * Main.rand.NextFloat(0.8f, 1.2f);

                        GlowSparkParticle glowSpark = new GlowSparkParticle(
                            spawnPos,
                            vel * 0.8f,
                            false,
                            Main.rand.Next(45, 65),
                            glowScale,
                            orangeGlow,
                            new Vector2(0.5f, 2.5f),
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(glowSpark);
                    }
                }
            }

            float numberOflines = 405;
            float rotFactorlines = 360f / numberOflines;
            for (int e = 0; e < numberOflines; e++)
            {
                Color randomColor = Main.rand.Next(4) switch
                {
                    0 => Color.Red,
                    1 => Color.MediumTurquoise,
                    2 => Color.Orange,
					_ => Color.LawnGreen,
                };

                float rot = MathHelper.ToRadians(e * rotFactorlines);
                Vector2 offset = (Vector2.UnitX * Main.rand.NextFloat(0.2f, 3.1f)).RotatedBy(rot + Main.rand.NextFloat(0.1f, 5.1f));
                Vector2 velOffset = (Vector2.UnitX * Main.rand.NextFloat(0.2f, 3.1f)).RotatedBy(rot + Main.rand.NextFloat(0.1f, 5.1f));
                SparkParticle spark = new SparkParticle(
                    Projectile.Center + offset,
                    velOffset * Main.rand.NextFloat(15.5f, 25.5f),
                    true,                     // 受重力影响
                    95,                       // 寿命
                    Main.rand.NextFloat(0.3f, 1.1f), // 缩放
                    Color.Lerp(Color.White, randomColor, 0.3f)
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }


            // ========================== 4️⃣ 可选：生成黑洞核心投射物 ==========================
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EndlessDevourJavBlackHoleIBV>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }







        }




    }
}