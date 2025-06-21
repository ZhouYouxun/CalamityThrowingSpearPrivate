using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Terraria.ID;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav
{
    internal class AuricJavLighting : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 4;
        }


        public override void AI()
        {
            float angleRange = MathHelper.ToRadians(25f);
            float randomAngle = Main.rand.NextFloat(-angleRange, angleRange);
            Vector2 particleVelocity = Projectile.velocity.RotatedBy(MathHelper.Pi + randomAngle).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f);

            Color particleColor = Main.rand.NextBool() ? Color.LightPink : Color.LightSalmon;
            float randomScale = Main.rand.NextFloat(0.85f, 1.25f);
            Particle bolt = new CrackParticle(
                Projectile.Center,
                particleVelocity,
                particleColor * 0.65f,
                Vector2.One * randomScale,
                0, 0,
                randomScale,
                11
            );
            GeneralParticleHandler.SpawnParticle(bolt);

            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(255, 215, 0);
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.52f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 选择最近的敌人，但排除：
            // - 刚刚命中的 `target`
            // - 距离小于 20 像素的敌人（防止来回弹射相同目标）
            NPC closestNPC = Main.npc
                .Where(npc => npc.active && !npc.friendly && npc.life > 0
                    && npc.whoAmI != target.whoAmI // 不选同一个敌人
                    && Vector2.Distance(npc.Center, Projectile.Center) > 20f) // 至少 20 像素外
                .OrderBy(npc => Vector2.Distance(npc.Center, Projectile.Center))
                .FirstOrDefault();

            if (closestNPC != null)
            {
                // 计算新方向
                Vector2 direction = Vector2.Normalize(closestNPC.Center - Projectile.Center);
                Projectile.velocity = direction * Projectile.velocity.Length();

                // 立即调整位置，防止卡在原地
                //Projectile.position = closestNPC.Center - Projectile.Size * 0.5f;
            }
            else
            {
                // 没有可弹射目标时，减少穿透次数
                //Projectile.penetrate--;
            }

            // 播放命中音效
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/AuricBulletHit"), Projectile.Center);
        }


    }

}
