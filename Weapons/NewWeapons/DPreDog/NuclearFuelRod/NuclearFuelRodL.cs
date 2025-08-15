using CalamityMod.Dusts;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.GameContent;
using CalamityMod.Graphics.Primitives;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    public class NuclearFuelRodL : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        private float offsetTimer = 0f; // 用于周期偏转
        private bool initialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (!initialized)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                initialized = true;
            }

            Lighting.AddLight(Projectile.Center, Color.LimeGreen.ToVector3() * 1.2f);
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 指数加速
            Projectile.velocity *= 1.015f;

            // 周期性随机左右偏转
            offsetTimer += 1f;
            if (offsetTimer >= 5f)
            {
                offsetTimer = 0f;
                float randomOffset = Main.rand.NextBool() ? MathHelper.ToRadians(3f) : -MathHelper.ToRadians(3f);
                Projectile.velocity = Projectile.velocity.RotatedBy(randomOffset);
            }

            // 查找场上的唯一 NuclearFuelRodPROJ
            int targetIndex = -1;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<NuclearFuelRodPROJ>())
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex == -1)
            {
                // 没有找到父弹幕，自杀
                Projectile.Kill();
                return;
            }

            // 飞行粒子特效
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 107);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Projectile.velocity * 0.3f;
                Main.dust[dust].scale = Main.rand.NextFloat(1.2f, 1.8f);
                Main.dust[dust].color = Color.LimeGreen;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            for (int i = 0; i < 60; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 107, Scale: 1.5f);
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(8f, 8f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].color = Color.LimeGreen;
            }


        }

        // =============================== 绿色拖尾 ===============================
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY;
            Vector2 origin = texture.Size() * 0.5f;

            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                .SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"))
                .UseColor(Color.LimeGreen) // 荧光绿色
                .Apply();

            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(TrailWidth, TrailColor, (_) => Projectile.Size * 0.5f,
                shader: GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]), 10);

            Main.spriteBatch.ExitShaderRegion();

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(Color.White),
                Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public Color TrailColor(float completionRatio)
        {
            float opacity = Utils.GetLerpValue(1f, 0.5f, completionRatio, true) * Projectile.Opacity;
            return Color.LimeGreen * opacity; // 荧光绿色
        }

        public float TrailWidth(float completionRatio)
        {
            return MathHelper.SmoothStep(12f, 26f, Utils.GetLerpValue(0f, 1f, completionRatio, true));
        }
    }
}
