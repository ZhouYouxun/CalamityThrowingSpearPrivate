using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using System;
using CalamityMod.Graphics.Primitives;
using ReLogic.Content;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.TheLastLanceSpear
{
    internal class TheLastLanceSpearHoldOut : ModProjectile
    {
        private float swingAngle = 0f; // 旋转惯量
        private float swingSpeed = 0.05f; // 旋转速度
        private int swingDirection = 1; // 1 表示顺时针，-1 表示逆时针
        private Vector2 initialMouseDirection; // 记录挥砍方向
        public override string Texture => "CalamityThrowingSpear/Weapons/NewSpears/BPrePlantera/TheLastLanceSpear/TheLastLanceSpear";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 36;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
        }

        public Player Owner => Main.player[Projectile.owner];
        public override void AI()
        {
            if (Owner.channel)
                Projectile.timeLeft = 300;
            else
            {
                Projectile.Kill();
                return;
            }

            // **实时计算鼠标偏移，使弹幕始终位于鼠标方向 15 像素处**
            Vector2 mouseDirection = Owner.SafeDirectionTo(Main.MouseWorld); // **计算鼠标方向**
            Vector2 handOffset = mouseDirection * 15f; // **始终偏移 15 像素**
            Projectile.Center = Owner.Center + handOffset; // **实时更新 `Projectile.Center`**

            // **让 swingAngle 线性变化**
            swingAngle += swingSpeed * swingDirection;

            // **当 swingAngle 达到 ±80° 时，改变方向**
            if (Math.Abs(swingAngle) >= MathHelper.ToRadians(80f))
            {
                swingDirection *= -1;
                initialMouseDirection = Owner.SafeDirectionTo(Main.MouseWorld); // **更新挥砍方向**
            }

            ManipulatePlayerArmPositions();
        }

        private void ManipulatePlayerArmPositions()
        {
            Owner.heldProj = Projectile.whoAmI;
            float armRotation = swingAngle - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // **保持我们的着色拖尾**
            GameShaders.Misc["CalamityMod:PrismaticStreak"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak")
            );

            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f;
            int numPoints = 45;

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    PrismaticWidthFunction,
                    PrismaticColorFunction,
                    (_) => overallOffset,
                    shader: GameShaders.Misc["CalamityMod:PrismaticStreak"]
                ),
                numPoints
            );

            // **绘制本体**
            DrawBlade(lightColor);

            return false;
        }

        private void DrawBlade(Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // **计算当前鼠标方向**
            Vector2 mouseDirection = Owner.SafeDirectionTo(Main.MouseWorld);

            // **计算实时旋转角度**
            float realSwingAngle = swingAngle + (float)Math.Atan2(mouseDirection.Y, mouseDirection.X);

            // **计算枪尾（旋转中心）的位置**
            Vector2 rotationOrigin = Projectile.Center - mouseDirection.SafeNormalize(Vector2.Zero) * 16f * 3f - Main.rand.NextVector2Circular(5f, 5f);

            // **计算绘制中心（确保绘制中心和碰撞体积一致）**
            Vector2 drawPosition = rotationOrigin - Main.screenPosition;

            // **计算旋转中心（让贴图围绕枪尾旋转）**
            Vector2 origin = new Vector2(0, texture.Height / 2f); // **左下角作为旋转中心**

            Main.EntitySpriteDraw(
                texture,
                drawPosition,
                null,
                lightColor,
                realSwingAngle, // **实时计算的旋转角度**
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
        }


        private float PrismaticWidthFunction(float completionRatio)
        {
            return 18f + (float)Math.Sin(completionRatio * 10f + Main.GlobalTimeWrappedHourly * 2f) * 3f;
        }

        private Color PrismaticColorFunction(float completionRatio)
        {
            Color deepBlue = new Color(10, 30, 60);
            Color lightBlue = new Color(50, 150, 255);
            return Color.Lerp(deepBlue, lightBlue, completionRatio);
        }
    }
}
