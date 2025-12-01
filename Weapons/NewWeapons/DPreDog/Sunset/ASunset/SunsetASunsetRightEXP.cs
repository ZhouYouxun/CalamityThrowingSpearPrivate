using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetRightEXP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 📏 全局缩放因子（整体比例由 currentScale 控制）
            float globalScale = currentScale;
            float time = Main.GlobalTimeWrappedHourly;
            SpriteEffects fx = SpriteEffects.None;

            // ============================
            // 1️⃣ Sun 核心噪声圆盘（主体太阳面）
            // ============================
            // 🔧 可调参数
            float sunCoreBaseScale = 1.4f;   // 基础缩放
            float sunCorePulseAmp = 0.10f;  // 脉动幅度
            float sunCoreRotSpeed1 = 0.30f;  // 内层旋转速度
            float sunCoreRotSpeed2 = -0.22f; // 外层反向旋转

            string[] sunCoreTextures = new[]
            {
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_001",
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_002",
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_003",
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_004",
        "CalamityThrowingSpear/Texture/SuperTexturePack/Sun/sun_005",
    };

            for (int i = 0; i < sunCoreTextures.Length; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(sunCoreTextures[i]).Value;
                float pulse = 1f + sunCorePulseAmp * (float)Math.Sin(time * 3.2f + i * 1.3f);
                float layerScale = sunCoreBaseScale * (1f + i * 0.07f) * pulse * globalScale;
                float rot = time * (i % 2 == 0 ? sunCoreRotSpeed1 : sunCoreRotSpeed2);

                Color col = Color.Lerp(new Color(255, 220, 120), new Color(255, 140, 60), i / (float)(sunCoreTextures.Length - 1));
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

            // ============================
            // 2️⃣ Flamelance 火舌（日珥）
            // ============================
            // 🔧 可调参数
            float flameBaseScale = 1.8f;
            float flamePulseAmp = 0.08f;
            float flameRotSpeed = 0.55f;

            string[] flameTextures = new[]
            {
        "CalamityThrowingSpear/Texture/SuperTexturePack/flamelance_009",
        "CalamityThrowingSpear/Texture/SuperTexturePack/flamelance_010",
        "CalamityThrowingSpear/Texture/SuperTexturePack/flamelance_011",
    };

            for (int i = 0; i < flameTextures.Length; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(flameTextures[i]).Value;
                float pulse = 1f + flamePulseAmp * (float)Math.Sin(time * 2.4f + i * 2.1f);
                float layerScale = flameBaseScale * (1f + i * 0.12f) * pulse * globalScale;
                float rot = time * flameRotSpeed * (i % 2 == 0 ? 1f : -1f);

                Color col = Color.Lerp(new Color(255, 200, 80), new Color(255, 80, 40), 0.7f);
                col *= 0.7f;
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

            // ============================
            // 3️⃣ Light 柔光环（内核辐射）
            // ============================
            // 🔧 可调参数
            float lightBaseScale = 1.1f;
            float lightPulseAmp = 0.12f;
            float lightRotSpeed = 0.18f;

            string[] lightTextures = new[]
            {
        "CalamityThrowingSpear/Texture/KsTexture/light_01",
        "CalamityThrowingSpear/Texture/KsTexture/light_02",
        "CalamityThrowingSpear/Texture/KsTexture/light_03",
    };

            for (int i = 0; i < lightTextures.Length; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(lightTextures[i]).Value;
                float pulse = 1f + lightPulseAmp * (float)Math.Sin(time * 4.1f + i * 0.9f);
                float layerScale = lightBaseScale * (1f + i * 0.15f) * pulse * globalScale;
                float rot = time * lightRotSpeed * (i == 0 ? -1f : 1f);

                Color col = Color.Lerp(new Color(255, 255, 200), new Color(255, 210, 100), 0.5f);
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

            // ============================
            // 4️⃣ Twirl 扭曲光纹（日冕气流）
            // ============================
            // 🔧 可调参数
            float twirlBaseScale = 2.1f;
            float twirlPulseAmp = 0.06f;
            float twirlRotSpeed1 = 0.40f;
            float twirlRotSpeed2 = -0.33f;

            string[] twirlTextures = new[]
            {
        "CalamityThrowingSpear/Texture/KsTexture/twirl_01",
        "CalamityThrowingSpear/Texture/KsTexture/twirl_02",
        "CalamityThrowingSpear/Texture/KsTexture/twirl_03",
    };

            for (int i = 0; i < twirlTextures.Length; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(twirlTextures[i]).Value;
                float pulse = 1f + twirlPulseAmp * (float)Math.Sin(time * 1.7f + i * 1.1f);
                float layerScale = twirlBaseScale * (1f + i * 0.18f) * pulse * globalScale;
                float rot = time * (i % 2 == 0 ? twirlRotSpeed1 : twirlRotSpeed2);

                Color col = Color.Lerp(new Color(255, 220, 140), new Color(255, 120, 50), i / (float)(twirlTextures.Length - 1));
                col *= 0.75f;
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





        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SunsetASunsetEDebuff>(), 300); // 300 帧 = 5 秒
        }
    }
}
