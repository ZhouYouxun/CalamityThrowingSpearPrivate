using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.ElectrocoagulationTenmonSpear
{
    internal class ElectrocoagulationTenmonSpearPROJ : BaseSpearProjectile
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

        // 设定很快的运动参数
        public override float InitialSpeed => 2.5f;
        public override float ReelbackSpeed => 1.2f;
        public override float ForwardSpeed => 0.6f;

        // 飞行期间释放十字星特效，白色并沿一定角度向前扩散
        public override void ExtraBehavior()
        {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            // 生成 2~4 个白色十字星
            int starAmount = Main.rand.Next(2, 5);
            for (int i = 0; i < starAmount; i++)
            {
                // 让特效在 -30° 到 30° 之间偏移
                float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f));
                Vector2 sparkVelocity = direction.RotatedBy(randomAngle) * Main.rand.NextFloat(4f, 8f);
                CritSpark spark = new CritSpark(
                    Projectile.Center,
                    sparkVelocity + Main.player[Projectile.owner].velocity,
                    Color.White,
                    Color.LightGray,
                    2f, // 让特效变大
                    20  // 延长粒子寿命
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        // 命中敌人后施加 Buff
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slimed, 300);
        }
    }
}
