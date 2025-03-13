using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Global;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Graphics.Metaballs;
using Terraria.GameContent.Drawing;

namespace CalamityThrowingSpear.Weapons.NewSpears.DPreDog.BloodstoneSpear
{
    internal class BloodstoneSpearBloodProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectile.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 画残影效果
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
        public override void SetDefaults()
        {
            // 设置弹幕的基础属性
            Projectile.width = 11; // 弹幕宽度
            Projectile.height = 24; // 弹幕高度
            Projectile.friendly = true; // 对敌人有效
            Projectile.DamageType = DamageClass.Ranged; // 远程伤害类型
            Projectile.penetrate = 1; // 穿透力为1，击中一个敌人就消失
            Projectile.timeLeft = 300; // 弹幕存在时间为600帧
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.ignoreWater = true; // 弹幕不受水影响
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = target.Center;

            // **释放多个 `AshTreeShake` 特效**
            int particleCount = 3; // 生成 3 组
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(16f, 16f);
                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.AshTreeShake,
                    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
                    Projectile.owner
                );
            }

            // **使用 `BloodBoilerFire` 的光球特效**
            BloodBoilerMetaball2.SpawnParticle(explosionPosition, 30f);
            BloodBoilerMetaball.SpawnParticle(explosionPosition, 22f);
        }

        public override void AI()
        {
            // 让弹幕在飞行时保持旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;

            // 添加血红色光源
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.49f);

            // 让弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // **三发平行 `SparkParticle`**
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = Color.DarkRed;
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;

                // **左侧偏移**
                Vector2 leftOffset = new Vector2(-6f, 0);
                SparkParticle leftSpark = new SparkParticle(Projectile.Center + leftOffset, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(leftSpark);

                // **右侧偏移**
                Vector2 rightOffset = new Vector2(6f, 0);
                SparkParticle rightSpark = new SparkParticle(Projectile.Center + rightOffset, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(rightSpark);

                // **中间略微靠前**
                Vector2 centerOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 4f;
                SparkParticle centerSpark = new SparkParticle(Projectile.Center + centerOffset, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(centerSpark);
            }

            // **飞行路径上留下血红色粒子特效**
            if (Main.rand.NextBool(4))
            {
                Dust bloodDust = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Projectile.velocity * 0.5f, 150, Color.DarkRed, 1.2f);
                bloodDust.noGravity = true;
                bloodDust.velocity *= 0.3f;
                bloodDust.fadeIn = 1.5f;
            }
        }






    }
}