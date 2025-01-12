using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJavPROJNebula : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/ElementalLanceC/ElementalLanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
        private bool hasTracking = false; // 是否已经获得追踪能力
        private bool hitEnemy = false;    // 是否击中了敌人
        private float hitTime = 0f;       // 记录击中敌人的时间
        private float trackingDelay = 30f; // 追踪能力延迟时间（以帧为单位，60帧=1秒）

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            // 使用紫色绘制拖尾效果
            Color trailColor = new Color(128, 0, 128); // 紫色
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], trailColor * 0.3f, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.55f);
            Projectile.velocity *= 1.001f;

            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(128, 0, 128);
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 如果已经击中敌人，但还没有获得追踪能力
            if (hitEnemy && !hasTracking)
            {
                // 计算击中后的时间是否超过了设定的延迟时间
                if (Projectile.timeLeft <= hitTime - trackingDelay)
                {
                    hasTracking = true; // 获得追踪能力
                    Projectile.netUpdate = true; // 同步状态
                }
            }

            // 如果获得追踪能力，则追踪最近的敌人
            if (hasTracking)
            {
                NPC target = Projectile.Center.ClosestNPCAt(2800);
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 追踪速度为12f
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300); // 元素混合

            hitEnemy = true;  // 标记已击中敌人
            hitTime = Projectile.timeLeft; // 记录击中时的时间
            Projectile.netUpdate = true;

            // 生成粒子效果
            Vector2 hitPosition = target.Center;
            for (int i = 0; i < 20; i++)
            {
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 4f);
                Vector2 velocity = randomDirection * speed;
                Dust particle = Dust.NewDustPerfect(hitPosition, DustID.PurpleCrystalShard);
                particle.velocity = velocity;
                particle.noGravity = true;
                particle.scale = Main.rand.NextFloat(1f, 1.5f);
                particle.fadeIn = 0.5f;
                particle.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            }


            int slashCount = Main.rand.Next(2, 4);
            for (int i = 0; i < slashCount; i++)
            {
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                int slashID = ModContent.ProjectileType<NebulaSLASH>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, randomDirection, slashID, (int)(Projectile.damage * 0.75), Projectile.knockBack, Projectile.owner);
            }
        }

        public override void OnKill(int timeLeft)
        {

        }
    }
}
