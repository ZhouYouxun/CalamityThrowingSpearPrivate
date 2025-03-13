using System;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears.DPreDog.TerraLanceSpear
{
    internal class TerraLanceSpearPROJ : BaseSpearProjectile
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

            Vector2 initalVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);
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
            Texture2D texture = Projectile.spriteDirection == -1 ? ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewSpears/DPreDog/TerraLanceSpear/TerraLanceSpearPROJN").Value : Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(Projectile.spriteDirection == 1 ? texture.Width + 8f : -8f, -8f);
            Main.EntitySpriteDraw(texture, drawPosition, null,
                Projectile.GetAlpha(lightColor), Projectile.rotation,
                origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = target.Center;
            int beamCount = Main.rand.Next(2, 5); // 2~4 发 BEAM

            // 释放 TerraBlade 特效
            for (int i = 0; i < 3; i++) // 3 组特效
            {
                Vector2 offset = Main.rand.NextVector2Circular(16f, 16f); // 1×16 范围内随机生成
                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.TerraBlade,
                    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
                    Projectile.owner
                );
            }

            // 释放 BEAM
            for (int i = 0; i < beamCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机方向
                Vector2 beamVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f; // 固定速度

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    explosionPosition,
                    beamVelocity,
                    ModContent.ProjectileType<TerraLanceSpearBEAM>(), // 释放光束
                    Projectile.damage / 2, // 伤害减半
                    0f,
                    Projectile.owner
                );
            }

            // 检查是否已有 TerratomereExplosion
            bool explosionExists = false;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<TerratomereExplosion>())
                {
                    explosionExists = true;
                    break;
                }
            }

            // 如果没有爆炸，则生成
            if (!explosionExists)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    explosionPosition,
                    Vector2.Zero, // 让爆炸静止
                    ModContent.ProjectileType<TerratomereExplosion>(), // 超级爆炸
                    Projectile.damage * 3, // 3 倍伤害
                    0f,
                    Projectile.owner
                );
            }
        }
    }
}
