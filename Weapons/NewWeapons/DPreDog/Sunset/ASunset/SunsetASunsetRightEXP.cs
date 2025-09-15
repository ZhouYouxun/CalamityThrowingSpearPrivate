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

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetRightEXP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 📏 全局缩放因子（整体缩小到原来的 X%）
            float globalScale = 1.1f;

            // === 🌌 外围魔法圈（蓝色星空） ===
            string[] outerRings = new[]
            {
                "CalamityThrowingSpear/Texture/KsTexture/magic_01",
                "CalamityThrowingSpear/Texture/KsTexture/magic_02",
                "CalamityThrowingSpear/Texture/KsTexture/magic_03"
            };

            for (int i = 0; i < outerRings.Length; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(outerRings[i]).Value;

                float rotation = Main.GlobalTimeWrappedHourly * (0.4f + 0.1f * i);
                float pulse = 1f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + i);
                float scale = 1.2f * pulse * globalScale;

                Color ringColor = Color.Lerp(Color.Yellow, Color.Gold, 0.4f) * 0.5f;
                ringColor.A = 0;

                Main.EntitySpriteDraw(
                    tex,
                    drawPos,
                    null,
                    ringColor,
                    rotation,
                    tex.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            // === ⭐核心星星组（叠加+反转旋转+脉动） ===
            string[] stars = new[]
            {
                "CalamityThrowingSpear/Texture/KsTexture/star_04",
                "CalamityThrowingSpear/Texture/KsTexture/star_07",
                "CalamityThrowingSpear/Texture/KsTexture/star_08"
            };

            for (int i = 0; i < stars.Length; i++)
            {
                Texture2D tex = ModContent.Request<Texture2D>(stars[i]).Value;

                float pulse = 0.85f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + i);
                float rotation = Main.GlobalTimeWrappedHourly * (i % 2 == 0 ? 1f : -1f); // 交替方向

                Color color = Color.Lerp(Color.Yellow, Color.Gold, 0.3f) * 0.7f;
                color.A = 0;

                Main.EntitySpriteDraw(
                    tex,
                    drawPos,
                    null,
                    color,
                    rotation,
                    tex.Size() * 0.5f,
                    pulse * 0.5f * globalScale,
                    SpriteEffects.None,
                    0
                );
            }

            return false;
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
            Projectile.timeLeft = 50;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // 爆炸持续比例 (0 ~ 1)
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
