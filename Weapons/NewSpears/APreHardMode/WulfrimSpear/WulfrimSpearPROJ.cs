using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using Terraria.Localization;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.WulfrimSpear
{
    internal class WulfrimSpearPROJ : BaseSpearProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewSpears.APreHardMode";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 28;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 90;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
        }

        public override float InitialSpeed => 3f;
        public override float ReelbackSpeed => 1f;
        public override float ForwardSpeed => 0.75f;

        // 回收前释放弹幕
        public override Action<Projectile> EffectBeforeReelback => (proj) =>
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                     Projectile.Center,
                                     Projectile.velocity * 0.8f,
                                     ModContent.ProjectileType<WulfrimSpearLight>(),
                                     Projectile.damage,
                                     Projectile.knockBack * 0.85f,
                                     Projectile.owner, 0f, 0f);
        };

        // 额外行为：飞行过程中生成特效
        public override void ExtraBehavior()
        {
            {
                Color outerSparkColor = new Color(144, 238, 144); // 亮绿色 (LightGreen)
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;

                SparkParticle spark = new SparkParticle(Projectile.Center,
                                                        Projectile.velocity,
                                                        false,
                                                        2,
                                                        outerSparkScale,
                                                        outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        // 命中敌人后，生成 Dust 267 形成方框并缓慢旋转消失
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 dustCenter = target.Center;

            for (int i = 0; i < 20; i++) // 生成 20 个 Dust
            {
                Vector2 dustOffset = new Vector2(24, 0).RotatedBy(MathHelper.TwoPi * i / 20); // 均匀分布
                Dust dust = Dust.NewDustDirect(dustCenter + dustOffset, 2, 2, 267, 0, 0, 100, Color.LimeGreen, 1.2f);
                dust.noGravity = true;
                dust.velocity = dustOffset * 0.2f;
            }

            // 逐渐旋转并缩小消失
            for (int j = 0; j < 30; j++)
            {
                float angle = MathHelper.TwoPi * j / 30;
                Vector2 boxOffset = new Vector2(30, 0).RotatedBy(angle);

                Dust boxDust = Dust.NewDustDirect(dustCenter + boxOffset, 2, 2, 267, 0, 0, 100, Color.LimeGreen, 1.5f);
                boxDust.noGravity = true;
                boxDust.velocity = Vector2.Zero;
                boxDust.fadeIn = 1.2f; // 逐渐淡出
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
        }
    }
}
