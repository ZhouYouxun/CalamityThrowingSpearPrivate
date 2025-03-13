using System;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears.DPreDog.BloodstoneSpear
{
    internal class BloodstoneSpearPROJ : BaseSpearProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 90;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.alpha = 255;
        }

        // **使用弯曲长枪 AI**
        public override SpearType SpearAiType => SpearType.GhastlyGlaiveSpear;

        // **设定长枪移动速度**
        public override float TravelSpeed => 22f;

        // **摆动幅度 & 速度修正（可随时修改）**
        private float swingAmplitude = MathHelper.TwoPi;
        private float swingSpeedModifier = 1.0f;

        public override void ExtraBehavior()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 playerRelativePoint = player.RotatedRelativePoint(player.MountedCenter);

            float itemAnimationCompletion = player.itemAnimation / (float)player.itemAnimationMax;
            float completionAsAngle = (1f - itemAnimationCompletion) * swingAmplitude * swingSpeedModifier;
            float startingVelocityRotation = Projectile.velocity.ToRotation();
            float startingVelocitySpeed = Projectile.velocity.Length();

            // **计算摆动轨迹**
            Vector2 flatVelocity = Vector2.UnitX.RotatedBy(MathHelper.Pi + completionAsAngle) *
                new Vector2(startingVelocitySpeed, Projectile.ai[0]);

            Vector2 destination = playerRelativePoint + flatVelocity.RotatedBy(startingVelocityRotation) +
                new Vector2(startingVelocitySpeed + TravelSpeed + 40f, 0f).RotatedBy(startingVelocityRotation);

            Vector2 initialVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // **前方扩散释放红色粒子**
            int dustCount = 4;
            for (int i = 0; i < dustCount; i++)
            {
                float spreadAngle = MathHelper.ToRadians(10) * (i - (dustCount / 2));
                Vector2 dustVelocity = initialVelocity.RotatedBy(spreadAngle) * Main.rand.NextFloat(2f, 5f);

                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? 60 : DustID.Blood, dustVelocity);
                dust.scale = Main.rand.NextFloat(1f, 2f);
                dust.velocity *= Main.rand.NextFloat(0.1f, 0.9f);
                dust.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // **计算长枪摆动时的角度**
            float angle = Projectile.rotation - MathHelper.PiOver4 * Math.Sign(Projectile.velocity.X) +
                (Projectile.spriteDirection == -1).ToInt() * MathHelper.Pi;

            // **攻击范围设定**
            float areaCheck = -95f; // 设定长枪的判定长度（像素）
            float redundantVariable = 0f; // 用于 `Collision.CheckAABBvLineCollision()` 的辅助变量

            // **使用碰撞检测线代替默认的方形碰撞**
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                targetHitbox.Size(), Projectile.Center,
                Projectile.Center + angle.ToRotationVector2() * areaCheck,
                (TravelSpeed + 1f) * Projectile.scale, ref redundantVariable))
                return true;

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPosition = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            //Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D texture = Projectile.spriteDirection == -1 ? ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewSpears/DPreDog/BloodstoneSpear/BloodstoneSpearPROJN").Value : Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(Projectile.spriteDirection == 1 ? texture.Width + 8f : -8f, -8f);
            Main.EntitySpriteDraw(texture, drawPosition, null,
                Projectile.GetAlpha(lightColor), Projectile.rotation,
                origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            Vector2 explosionPosition = target.Center;

            // **释放血液爆炸特效**
            Particle bloodsplosion = new CustomPulse(explosionPosition, Vector2.Zero, Color.DarkRed,
                "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f),
                0.16f, 0.87f, (int)(40 * 0.38f), false);
            GeneralParticleHandler.SpawnParticle(bloodsplosion);

            Particle bloodsplosion2 = new CustomPulse(explosionPosition, Vector2.Zero, new Color(255, 32, 32),
                "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f),
                0.03f, 0.155f, 40);
            GeneralParticleHandler.SpawnParticle(bloodsplosion2);

            // **恢复 3 点生命值**
            int healAmount = 3;
            player.statLife += healAmount;
            player.HealEffect(healAmount);

            // **射出 3 轮扩散型 `BloodstoneSpearBloodProj`**
            int waveCount = 3;
            int shotsPerWave = 3;
            float spreadAngle = MathHelper.ToRadians(15);

            for (int wave = 0; wave < waveCount; wave++)
            {
                for (int i = 0; i < shotsPerWave; i++)
                {
                    float angleOffset = spreadAngle * (i - (shotsPerWave / 2));
                    Vector2 shotVelocity = Projectile.velocity.RotatedBy(angleOffset) * Main.rand.NextFloat(0.9f, 1.1f);

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        explosionPosition,
                        shotVelocity,
                        ModContent.ProjectileType<BloodstoneSpearBloodProj>(),
                        Projectile.damage / 2,
                        0f,
                        Projectile.owner
                    );
                }
            }
        }
    }
}
