using System;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.DLOASSpear
{
    internal class DLOASSpearPROJ : BaseSpearProjectile
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

        // **在回缩前生成蛇形弹幕**
        public override Action<Projectile> EffectBeforeReelback => (proj) =>
        {
            int owner = proj.owner;
            Vector2 spawnPosition = proj.Center;

            // **生成头部**
            int prev = Projectile.NewProjectile(proj.GetSource_FromThis(), spawnPosition, proj.velocity * 0.35f,
                ModContent.ProjectileType<DLOASSpear1Head>(), proj.damage * 3, proj.knockBack, owner, proj.whoAmI);

            // **生成身体**
            for (int j = 0; j < 3; j++)
            {
                prev = Projectile.NewProjectile(proj.GetSource_FromThis(), spawnPosition, proj.velocity * 0.35f,
                    ModContent.ProjectileType<DLOASSpear2Body>(), proj.damage * 3, proj.knockBack, owner, prev);
            }

            // **生成尾巴**
            Projectile.NewProjectile(proj.GetSource_FromThis(), spawnPosition, proj.velocity * 0.35f,
                ModContent.ProjectileType<DLOASSpear3Tail>(), proj.damage * 3, proj.knockBack, owner, prev);
        };

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

            // **释放紫色粒子特效**
            int dustCount = 4;
            int[] purpleDusts = { DustID.PurpleTorch, DustID.Shadowflame, DustID.Vortex };

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustSpawnPos = Projectile.Center + initialVelocity.RotatedBy(completionAsAngle * 2f + i / (float)dustCount * MathHelper.TwoPi) * 15f;
                Dust dust = Dust.NewDustPerfect(dustSpawnPos, purpleDusts[Main.rand.Next(purpleDusts.Length)], Vector2.Zero, 110, default, 1.2f);

                dust.velocity = initialVelocity * 4f;
                dust.scale = 1.2f + Main.rand.NextFloat(0.5f);
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
            Texture2D texture = Projectile.spriteDirection == -1 ? ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewSpears/APreHardMode/DLOASSpear/DLOASSpearPROJN").Value : Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(Projectile.spriteDirection == 1 ? texture.Width + 8f : -8f, -8f);
            Main.EntitySpriteDraw(texture, drawPosition, null,
                Projectile.GetAlpha(lightColor), Projectile.rotation,
                origin, Projectile.scale, SpriteEffects.None, 0);
            return false; 
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = target.Center;
            int effectCount = 3;

            for (int i = 0; i < effectCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(16f, 16f);

                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.NightsEdge,
                    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
                    Projectile.owner
                );
            }
        }
    }
}
