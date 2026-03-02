using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Graphics.Shaders;
using Terraria.Audio;
using CalamityMod.Graphics.Primitives;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJavPROJEntropy : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/ElementalLanceC/ElementalLanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";

        private int hitCooldown = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 黑色冲击波特效（每10帧）
            if (Projectile.ai[0] % 10 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Particle pulse = new DirectionalPulseRing(
                        Projectile.Center,
                        Projectile.velocity * 0.5f,
                        Color.Black * 0.6f,
                        new Vector2(1f, 2.3f),
                        Projectile.rotation - MathHelper.PiOver4,
                        0.22f,
                        0.035f,
                        20
                    );
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }

            // 无序粒子：在前方喷出黑色 Dust
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20f;
                Vector2 spawnPos = Projectile.Center + offset;
                Dust dust = Dust.NewDustPerfect(spawnPos, DustID.Shadowflame, Projectile.velocity.RotatedByRandom(0.5f) * 0.5f, 150, Color.Black, 1.1f);
                dust.noGravity = true;
            }

            // 冷却倒计时
            if (hitCooldown > 0)
                hitCooldown--;

            Projectile.ai[0]++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hitCooldown > 0)
                return;

            hitCooldown = 15; // 设置 X 帧冷却

            // 生成标记弹幕（后续处理）
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<ElementalLanceJavPROJE>(), (int)(Projectile.damage * 1.2f), 0, Projectile.owner);

            SpawnBlackImpactEffects();
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (hitCooldown > 0)
            {
                modifiers.SourceDamage *= 0f; // 使伤害为 0
            }
        }

        public override void OnKill(int timeLeft)
        {
            SpawnBlackImpactEffects();
        }

        private void SpawnBlackImpactEffects()
        {
            // 黑色冲击波
            Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, 18, false);
            GeneralParticleHandler.SpawnParticle(bloodsplosion);

            // 有序粒子环：黑色光点环状放射
            for (int i = 0; i < 3; i++)
            {
                float radius = 20f + 10f * i;
                int count = 10 + i * 4;
                for (int j = 0; j < count; j++)
                {
                    float angle = MathHelper.TwoPi * j / count;
                    Vector2 dir = angle.ToRotationVector2();
                    Vector2 pos = Projectile.Center + dir * radius;
                    Vector2 vel = dir * 1.2f;
                    Particle p = new SparkParticle(pos, vel, false, 40, 0.9f, Color.Black);
                    GeneralParticleHandler.SpawnParticle(p);
                }
            }

            // 无序粒子组：四个不同方向多点 Dust 散射
            for (int g = 0; g < 4; g++)
            {
                Vector2 dir = Main.rand.NextVector2Unit();
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = dir.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(1f, 4f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, vel, 0, Color.Black, Main.rand.NextFloat(1.2f, 2f));
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 原本的光学印记描边效果
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            Color trailColor = new Color(0, 0, 0);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], trailColor * 0.3f, 1);

            Main.spriteBatch.EnterShaderRegion();

            GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                .SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/window_04"))
                .UseColor(new Color(10, 10, 10)) // 极黑色
                .UseSecondaryColor(new Color(30, 30, 30))
                .Apply();

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    (completionRatio, vertexPos) => MathHelper.SmoothStep(12f, 2f, completionRatio),
					(completionRatio, vertexPos) => new Color(10, 10, 10),
					(completionRatio, vertexPos) => Projectile.Size * 0.5f,
                    shader: GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                ),
                10
            );
            
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
