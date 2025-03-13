using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.TheLastLanceSpear
{
    public class TheLastLanceSpearSwing : ModProjectile
    {
        private bool initialized = false;
        private Vector2 direction = Vector2.Zero;
        private const float MaxTime = 30;
        private const float SwingWidth = MathHelper.PiOver2 * 2.0f;
        private float Timer => MaxTime - Projectile.timeLeft;
        private Player Owner => Main.player[Projectile.owner];
        public float SwingDirection => Projectile.ai[0] * Math.Sign(direction.X);
        public Vector2 DistanceFromPlayer => direction * 24;

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (int)MaxTime;
        }

        public override void AI()
        {
            if (!initialized)
            {
                Projectile.timeLeft = (int)MaxTime;
                direction = Projectile.velocity;
                direction.Normalize();
                initialized = true;
                Projectile.netUpdate = true;
            }

            // 计算挥舞进度
            float swingProgress = Timer / MaxTime;

            // 计算武器位置，使其围绕玩家挥舞
            Vector2 swingOffset = direction.RotatedBy(MathHelper.Lerp(SwingWidth / 2 * SwingDirection, -SwingWidth / 2 * SwingDirection, swingProgress)) * 24f;
            Projectile.Center = Owner.Center + swingOffset;

            // **修正旋转角度，使其对齐挥舞轨迹**
            Projectile.rotation = (Owner.Center - Projectile.Center).ToRotation() + MathHelper.ToRadians(135f); // 45° + 90°

            // 计算武器大小，使其在中间阶段稍微放大
            Projectile.scale = 1.2f + (float)Math.Sin(swingProgress * MathHelper.Pi) * 0.4f;

            // 调整手臂动画，使其与挥舞同步
            ManipulatePlayerArmPositions();
        }

        // 让玩家手臂正确随武器挥舞
        private void ManipulatePlayerArmPositions()
        {
            Owner.heldProj = Projectile.whoAmI;

            // **修正手臂旋转，使其始终指向武器**
            float armRotation = Projectile.rotation - MathHelper.PiOver2;

            // **确保左挥舞时手臂翻转**
            //if (Owner.direction == -1)
            //    armRotation += MathHelper.Pi;

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D sword = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D smear = ModContent.Request<Texture2D>("CalamityMod/Particles/TrientCircularSmear").Value;

            // **1. 计算翻转效果**
            SpriteEffects flip = Owner.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float extraAngle = Owner.direction < 0 ? MathHelper.PiOver2 : 0f;

            // **2. 计算旋转角度**
            float drawAngle = Projectile.rotation;
            float drawRotation = drawAngle + MathHelper.PiOver4 + extraAngle;

            // **3. 计算绘制位置**
            Vector2 drawOrigin = new Vector2(Owner.direction < 0 ? sword.Width : 0f, sword.Height);
            Vector2 drawOffset = Owner.Center + drawAngle.ToRotationVector2() * 10f - Main.screenPosition;

            // **4. 绘制刀盘特效**
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float opacity = (float)Math.Sin(Timer / MaxTime * MathHelper.Pi);
            float rotation = (-MathHelper.PiOver4 * 0.5f + MathHelper.PiOver4 * 0.5f * Timer / MaxTime) * SwingDirection;

            Main.EntitySpriteDraw(smear, Owner.Center - Main.screenPosition, null, Color.White * (0.5f * opacity),
                Projectile.velocity.ToRotation() + MathHelper.Pi + rotation, smear.Size() / 2f, Projectile.scale * 1.5f, SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // **5. 延迟绘制武器本体**
            if (Projectile.timeLeft < MaxTime - 2) // **只在第三帧及之后绘制武器本体**
            {
                // **绘制残影**
                for (int i = 0; i < Projectile.oldRot.Length; i++)
                {
                    Color trailColor = Main.hslToRgb((i / (float)Projectile.oldRot.Length) * 0.7f, 1, 0.6f);
                    float afterimageRotation = Projectile.oldRot[i] + MathHelper.PiOver4;
                    Main.EntitySpriteDraw(sword, drawOffset, null, trailColor * 0.15f, afterimageRotation + extraAngle, drawOrigin,
                        Projectile.scale - 0.2f * (i / (float)Projectile.oldRot.Length), flip, 0);
                }

                // **绘制武器本体**
                Main.EntitySpriteDraw(sword, drawOffset, null, lightColor, drawRotation, drawOrigin, Projectile.scale, flip, 0);
            }

            return false;
        }



        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            //The hitbox is simplified into a line collision.
            float collisionPoint = 0f;
            float bladeLength = 88f * Projectile.scale;
            Vector2 holdPoint = DistanceFromPlayer.Length() * Projectile.rotation.ToRotationVector2();

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Owner.Center + holdPoint, Owner.Center + holdPoint + Projectile.rotation.ToRotationVector2() * bladeLength, 24, ref collisionPoint);
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }


    }
}







