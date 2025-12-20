using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetRightEXP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";





        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // ============================
            // 🔧【统一可调参数区】==========
            // ============================

            float globalScale = currentScale;
            float time = Main.GlobalTimeWrappedHourly;
            SpriteEffects fx = SpriteEffects.None;

            // 主题颜色（橙 → 红）
            Color themeA = new Color(255, 180, 60);  // 橙色
            Color themeB = new Color(255, 60, 60);   // 红色

            // 1️⃣ Sun 核心圆盘
            float sunCoreBaseScale = 0.3f;
            float sunCorePulseAmp = 0.10f;
            float sunCoreRotSpeed1 = 0.30f;
            float sunCoreRotSpeed2 = -0.22f;

            string[] sunCoreTextures = new[]
            {
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_001",
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_002",
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_003",
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_004",
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_005",
    };

            // 2️⃣ Light 柔光环
            float lightBaseScale = 1.1f;
            float lightPulseAmp = 0.12f;
            float lightRotSpeed = 0.18f;

            string[] lightTextures = new[]
            {
        "CalamityThrowingSpear/Texture/KsTexture/light_01",
        "CalamityThrowingSpear/Texture/KsTexture/light_02",
        "CalamityThrowingSpear/Texture/KsTexture/light_03",
    };

            // ============================
            // 🎨【正式绘制区】============
            // ============================

            // 1️⃣ Sun 核心噪声圆盘
            for (int i = 0; i < sunCoreTextures.Length; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(sunCoreTextures[i]).Value;

                float pulse = 1f + sunCorePulseAmp * MathF.Sin(time * 3.2f + i * 1.3f);
                float layerScale = sunCoreBaseScale * (1f + i * 0.07f) * pulse * globalScale;
                float rot = time * (i % 2 == 0 ? sunCoreRotSpeed1 : sunCoreRotSpeed2);

                // 橙 → 红 差值
                Color col = Color.Lerp(themeA, themeB, i / (float)(sunCoreTextures.Length - 1));
                col.A = 0;

                Main.EntitySpriteDraw(
                    tex,
                    drawPos,
                    null,
                    col,
                    rot,
                    tex.Size() * 0.5f,
                    layerScale,
                    fx,
                    0
                );
            }

            // 2️⃣ Light 柔光环
            for (int i = 0; i < lightTextures.Length; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(lightTextures[i]).Value;

                float pulse = 1f + lightPulseAmp * MathF.Sin(time * 4.1f + i * 0.9f);
                float layerScale = lightBaseScale * (1f + i * 0.15f) * pulse * globalScale;
                float rot = time * lightRotSpeed * (i == 0 ? -1f : 1f);

                // 橙 → 红 差值
                Color col = Color.Lerp(themeA, themeB, 0.5f + 0.5f * i / (float)(lightTextures.Length - 1));
                col *= 0.85f;
                col.A = 0;

                Main.EntitySpriteDraw(
                    tex,
                    drawPos,
                    null,
                    col,
                    rot,
                    tex.Size() * 0.5f,
                    layerScale,
                    fx,
                    0
                );
            }

            return false;
        }



        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            Player owner = Main.player[Projectile.owner];
            int totalCrit = (int)Math.Round(owner.GetTotalCritChance(Projectile.DamageType));
            Projectile.CritChance = totalCrit; // ✅ 自己取总暴击率
        }


        public override void SetDefaults()
        {
            Projectile.width = 500;
            Projectile.height = 500;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
        }
        private int growTimer = 0;
        private float currentScale = 0f; // 实时缩放值
        public override void AI()
        {
            // 让 growTimer 累积，最多 X 帧
            if (growTimer < 20)
                growTimer++;

            // 线性插值：0 到 1.1f
            currentScale = MathHelper.Lerp(0f, 1.1f, growTimer / 20f);

            // 原本的爆炸逻辑保持不变
            float progress = 1f - (Projectile.timeLeft / 50f);
            float radius = MathHelper.Lerp(20f, 250f, progress);


            // ====== 环形扩散 ======
            int count = 10;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi / count * i + Main.GameUpdateCount * 0.05f;
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 dir = offset.SafeNormalize(Vector2.UnitY);

                // 光环闪光
                Vector2 sparkleVel = dir * Main.rand.NextFloat(2f, 4f);
                Particle sparkle = new SparkParticle(
                    Projectile.Center + offset,
                    sparkleVel,
                    false,
                    20,
                    1.0f,
                    Main.rand.NextBool() ? Color.Gold : Color.Orange
                )
                {
                    Rotation = sparkleVel.ToRotation()
                };
                GeneralParticleHandler.SpawnParticle(sparkle);

                // 在同一点位生成 Excalibur 粒子（给个外扩方向）
                if (Main.rand.NextBool(4))
                {
                    ParticleOrchestrator.RequestParticleSpawn(
                        clientOnly: false,
                        ParticleOrchestraType.Excalibur,
                        new ParticleOrchestraSettings
                        {
                            PositionInWorld = Projectile.Center + offset,
                            MovementVector = sparkleVel // ✅ 让 Excalibur 也朝外飞
                        },
                        Projectile.owner
                    );
                }
            }

            // ====== 随机点缀闪光 ======
            if (Main.rand.NextBool(3))
            {
                Vector2 randOffset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 dir = randOffset.SafeNormalize(Vector2.UnitY);
                Vector2 vel = dir * Main.rand.NextFloat(2f, 3.5f);

                Particle point = new PointParticle(
                    Projectile.Center + randOffset,
                    vel,
                    false,
                    12,
                    1.2f,
                    Color.White
                )
                {
                    Rotation = vel.ToRotation()
                };
                GeneralParticleHandler.SpawnParticle(point);
            }

            // ====== 外圈烟雾 ======
            if (Main.rand.NextBool(5))
            {
                Vector2 smokeOffset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 dir = smokeOffset.SafeNormalize(Vector2.UnitY);
                Vector2 vel = dir * Main.rand.NextFloat(1f, 2f);

                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center + smokeOffset,
                    vel,
                    Color.WhiteSmoke,
                    25,
                    Main.rand.NextFloat(0.8f, 1.3f),
                    0.4f,
                    Main.rand.NextFloat(-1f, 1f),
                    false
                )
                {
                    Rotation = vel.ToRotation()
                };
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }



        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SunsetPlayerSpeed.ApplyNoArmorHypothesisHitEffect(
                Projectile,
                target,
                ref modifiers
            );
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SunsetASunsetEDebuff>(), 300); // 300 帧 = 5 秒
        }
    }
}
