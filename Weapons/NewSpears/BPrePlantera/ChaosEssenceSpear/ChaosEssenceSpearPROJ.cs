using System;
using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.ChaosEssenceSpear
{
    internal class ChaosEssenceSpearPROJ : BaseSpearProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<ChaosEssenceSpear>();
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

        // 设定它使用弯曲长枪的 AI
        public override SpearType SpearAiType => SpearType.GhastlyGlaiveSpear;

        // 设定移动速度
        public override float TravelSpeed => 22f;

        // 设定摆动幅度（可以随时修改）
        //private float swingAmplitude = MathHelper.TwoPi;
        //private float swingSpeedModifier = 1.0f; // 影响摆动速度的系数

        // 在回缩前发射 `ChaosEssenceSpearFire`
        //public override Action<Projectile> EffectBeforeReelback => (proj) =>
        //{
        //    float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(-1f, 1f));
        //    Vector2 fireVelocity = proj.velocity.RotatedBy(randomAngle);

        //    Projectile.NewProjectile(
        //        proj.GetSource_FromThis(),
        //        proj.Center + proj.velocity * 0.5f,
        //        fireVelocity,
        //        ModContent.ProjectileType<ChaosEssenceSpearFire>(), // 释放火焰弹幕
        //        proj.damage / 2, proj.knockBack * 0.85f,
        //        proj.owner
        //    );
        //};


        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPosition = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            //Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D texture = Projectile.spriteDirection == -1 ? ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewSpears/BPrePlantera/ChaosEssenceSpear/ChaosEssenceSpearPROJN").Value : Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(Projectile.spriteDirection == 1 ? texture.Width + 8f : -8f, -8f);
            Main.EntitySpriteDraw(texture, drawPosition, null,
                Projectile.GetAlpha(lightColor), Projectile.rotation,
                origin, Projectile.scale, SpriteEffects.None, 0); 
            return false;
        }

        public override void ExtraBehavior()
        {
            Player player = Main.player[Projectile.owner];

            Vector2 playerRelativePoint = player.RotatedRelativePoint(player.MountedCenter);

            float itemAnimationCompletion = player.itemAnimation / (float)player.itemAnimationMax;
            float completionAsAngle = (1f - itemAnimationCompletion) * MathHelper.TwoPi;
            float startingVelocityRotation = Projectile.velocity.ToRotation();
            float startingVelocitySpeed = Projectile.velocity.Length();

            // The motion moves in an imaginary circle, but the cane does not because it relies on
            // its ai[0] X multiplier, giving it the "swiping" motion.
            Vector2 flatVelocity = Vector2.UnitX.RotatedBy(MathHelper.Pi + completionAsAngle) *
                new Vector2(startingVelocitySpeed, Projectile.ai[0]);

            Vector2 destination = playerRelativePoint + flatVelocity.RotatedBy(startingVelocityRotation) +
                new Vector2(startingVelocitySpeed + TravelSpeed + 40f, 0f).RotatedBy(startingVelocityRotation);

            Vector2 directionTowardsEnd = player.SafeDirectionTo(destination, Vector2.UnitX * player.direction);
            Vector2 initalVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);


            // **释放粒子特效**
            int dustCount = 4;
            int[] dustTypes = { DustID.CrimsonTorch, DustID.FlameBurst, DustID.HealingPlus, DustID.DesertTorch };

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustSpawnPos = Projectile.Center + initalVelocity.RotatedBy(completionAsAngle * 2f + i / (float)dustCount * MathHelper.TwoPi) * 15f;
                Dust dust = Dust.NewDustPerfect(dustSpawnPos, dustTypes[Main.rand.Next(dustTypes.Length)], Vector2.Zero, 110, default, 1.2f);

                dust.velocity = initalVelocity * 10f;
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
            float reduntantVariable = 0f; // 用于 `Collision.CheckAABBvLineCollision()` 的辅助变量

            // **使用碰撞检测线代替默认的方形碰撞**
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                targetHitbox.Size(), Projectile.Center,
                Projectile.Center + angle.ToRotationVector2() * areaCheck,
                (TravelSpeed + 1f) * Projectile.scale, ref reduntantVariable))
                return true; // **如果碰撞检测线与目标 `NPC` 碰撞，返回 `true`（表示命中）**

            return false; // **否则，返回 `false`（表示未命中）**
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 explosionPosition = target.Center;

                int fireballCount = 1;

                for (int i = 0; i < fireballCount; i++)
                {
                    // **计算弹幕的初始方向（正上方 ±10°）**
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f));
                    Vector2 fireVelocity = Vector2.UnitY.RotatedBy(angleOffset) * -8f; // 速度向上

                    // **创建火焰弹幕**
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        explosionPosition,
                        fireVelocity,
                        ModContent.ProjectileType<ChaosEssenceSpearFire>(), // 释放火焰弹幕
                        Projectile.damage / 2, Projectile.knockBack * 0.85f,
                        Projectile.owner
                    );
                }
            }
        }

    }
}
