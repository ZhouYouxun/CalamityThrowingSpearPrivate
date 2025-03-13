using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears.CPreMoodLord.TidalMechanicsSpear
{
    internal class TidalMechanicsSpearHoldOutSP : ModProjectile
    {

        public override string Texture => "CalamityThrowingSpear/Weapons/NewSpears/CPreMoodLord/TidalMechanicsSpear/TidalMechanicsSpear";
        public Player Owner => Main.player[Projectile.owner];

        const float BladeLength = 180;
        public const int GetSwingTime = 78;
        public float Timer => SwingTime - Projectile.timeLeft;
        public float Progression => SwingTime > 0 ? Timer / (float)SwingTime : 0;

        public ref float SwingTime => ref Projectile.localAI[0];

        public int Direction => float.IsNaN(Projectile.velocity.X) ? 1 : (Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1);
        public float BaseRotation => Projectile.velocity.ToRotation();

        public float SwingAngleShiftAtProgress(float progress)
            => float.IsNaN(progress) ? 0f : MathHelper.PiOver2 * 1.5f * (float)Math.Sin(progress * MathHelper.Pi);

        public float SwordRotationAtProgress(float progress)
            => float.IsNaN(progress) ? BaseRotation : BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;

        public Vector2 DirectionAtProgress(float progress)
        {
            float angle = SwordRotationAtProgress(progress);
            return float.IsNaN(angle) ? Vector2.Zero : angle.ToRotationVector2();
        }

        public float SwordRotation => SwordRotationAtProgress(Progression);
        public Vector2 SwordDirection => DirectionAtProgress(Progression);

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.MaxUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 8;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft >= 9999)
            {
                Vector2 dir = Owner.MountedCenter.DirectionTo(Owner.Center);
                if (float.IsNaN(dir.X) || float.IsNaN(dir.Y))
                    dir = Vector2.UnitX; // 避免 NaN，默认向右

                Projectile.velocity = dir;
                SwingTime = GetSwingTime;
                Projectile.timeLeft = (int)SwingTime;
            }

            if (!Owner.channel && Projectile.timeLeft > 1)
                Projectile.timeLeft--;

            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.Opacity <= 0f)
                return false;

            DrawBlade(Main.spriteBatch);
            return false;
        }
 
        public void DrawBlade(SpriteBatch spriteBatch)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            if (texture == null)
                return; // 确保纹理存在

            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(texture, Owner.MountedCenter - Main.screenPosition, null, Color.White, SwordRotation, texture.Size() / 2f, Projectile.scale, direction, 0);
        }
    }
}
