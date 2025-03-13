using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.PolarEssenceSpear
{
    internal class PolarEssenceSpearPROJ : BaseSpearProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 11;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        // 设定中等速度
        public override float InitialSpeed => 4.2f;
        public override float ReelbackSpeed => 2.5f;
        public override float ForwardSpeed => 1.8f;

        // 飞行期间释放亮白色尖刺特效，大小提升
        public override void ExtraBehavior()
        {
            // 亮白色尖刺特效
            Color outerSparkColor = Color.White;
            float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
            float outerSparkScale = 2.4f + scaleBoost;

            SparkParticle spark = new SparkParticle(Projectile.Center,
                                                    Projectile.velocity,
                                                    false,
                                                    2,
                                                    outerSparkScale,
                                                    outerSparkColor);
            GeneralParticleHandler.SpawnParticle(spark);

            // --- 添加雪花扩散 Dust 特效 ---
            if (Main.rand.NextBool(2)) // 50% 概率生成，防止过量
            {
                int dustType = Main.rand.Next(new int[] { 137, 135, 185, 1974 }); // 随机选择 DustID
                Vector2 dustOffset = Main.rand.NextVector2Circular(10f, 10f); // 10 像素范围内随机扩散

                Dust snowDust = Dust.NewDustPerfect(Projectile.Center + dustOffset, dustType,
                                                    new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)),
                                                    150,
                                                    Color.White,
                                                    Main.rand.NextFloat(1.2f, 1.8f)); // 白色粒子，随机缩放
                snowDust.noGravity = false; // 受重力影响
                snowDust.fadeIn = 1.3f; // 逐渐消失
            }
        }

        // 命中敌人后，随机射出 1~2 个 PolarEssenceSpearExtra，并播放 Item28 音效
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item28, Projectile.position);

            int extraCount = Main.rand.Next(1, 3); // 1~2 个额外弹幕
            for (int i = 0; i < extraCount; i++)
            {
                Vector2 randomVelocity = Main.rand.NextVector2Circular(4f, 4f); // 方向随机

                Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                                         Projectile.Center,
                                         randomVelocity,
                                         ModContent.ProjectileType<PolarEssenceSpearExtra>(),
                                         (int)(Projectile.damage * 1.0f), // 伤害倍率 1.0 倍
                                         0f,
                                         Projectile.owner);
            }
        }
    }
}
