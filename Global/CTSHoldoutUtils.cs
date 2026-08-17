using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityThrowingSpear.Global
{
    public static class CTSHoldoutUtils
    {
        public const float DefaultPullbackFrames = 30f;

        public static float ApplyPulledSpearHoldout(
            Player owner,
            Projectile projectile,
            float timer,
            float pullbackFrames = DefaultPullbackFrames,
            float rotationOffset = MathHelper.PiOver4,
            float aimLerp = 0.1f,
            float forwardOffset = 40f,
            float gripOffset = 16f)
        {
            Vector2 fallbackDirection = Vector2.UnitX * owner.direction;

            if (Main.myPlayer == projectile.owner)
            {
                Vector2 aimDirection = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(fallbackDirection);
                Vector2 currentDirection = projectile.velocity.SafeNormalize(aimDirection);
                projectile.velocity = Vector2.Lerp(currentDirection, aimDirection, aimLerp).SafeNormalize(aimDirection);
            }

            Vector2 direction = projectile.velocity.SafeNormalize(fallbackDirection);
            projectile.rotation = direction.ToRotation() + rotationOffset;

            owner.ChangeDir(direction.X >= 0f ? 1 : -1);

            float pullbackCompletion = GetPullbackCompletion(projectile, timer, pullbackFrames);
            float frontArmRotation = projectile.rotation - rotationOffset - pullbackCompletion * owner.direction * 0.74f;
            if (owner.direction == 1)
                frontArmRotation += MathHelper.Pi;

            Vector2 armOffset = (frontArmRotation + MathHelper.PiOver2).ToRotationVector2() * gripOffset * projectile.scale;
            projectile.Center = owner.MountedCenter + armOffset + direction * forwardOffset * projectile.scale;

            owner.heldProj = projectile.whoAmI;
            owner.SetDummyItemTime(2);
            owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);

            return pullbackCompletion;
        }

        public static bool ReleaseReady(Player owner, Projectile projectile, float timer, float pullbackFrames = DefaultPullbackFrames)
        {
            return !owner.channel && GetPullbackCompletion(projectile, timer, pullbackFrames) >= 1f;
        }

        public static float GetPullbackCompletion(Projectile projectile, float timer, float pullbackFrames = DefaultPullbackFrames)
        {
            float requiredUpdates = pullbackFrames * (projectile.extraUpdates + 1f);
            return MathHelper.Clamp(timer / requiredUpdates, 0f, 1f);
        }
    }
}
