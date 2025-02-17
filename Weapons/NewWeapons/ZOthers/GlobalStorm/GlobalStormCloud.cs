using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.ZOthers.GlobalStorm
{
    public class GlobalStormCloud : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;
        }

        Texture2D flashTex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/AquasScepterCloudFlash").Value;
        Texture2D glowTex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/AquasScepterCloudGlowmask").Value;

        public int DrawFlashTimer = 0;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.HeldItem.type != ModContent.ItemType<GlobalStorm>())
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = owner.Center + new Vector2(0, -60);

            if (Projectile.ai[0]++ % 45 == 0)
            {
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/LightningAura"), Projectile.Center);
                DrawFlashTimer = 27; // 触发闪光效果
                for (int i = 0; i < Main.rand.Next(1, 3); i++) // 生成 1~2 个弹幕
                {
                    NPC target = Main.npc.Where(n => n.active && !n.friendly)
                        .OrderBy(n => Vector2.Distance(n.Center, Projectile.Center))
                        .FirstOrDefault();

                    if (target != null && owner.HasAmmo(owner.HeldItem))
                    {
                        bool dontConsumeAmmo = Main.rand.NextBool();
                        owner.PickAmmo(owner.HeldItem, out int ammoProjectile, out float shootSpeed, out int damage, out float knockback, out _, dontConsumeAmmo);

                        // 计算发射位置
                        float offsetX = Main.rand.NextFloat(-16f, 16f) * (i % 2 == 0 ? 1 : -1);
                        Vector2 spawnPosition = Projectile.Center + new Vector2(offsetX, 0);

                        // 计算发射方向
                        Vector2 velocity = (target.Center - spawnPosition).SafeNormalize(Vector2.Zero) * shootSpeed;

                        // 发射弹幕
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            spawnPosition,
                            velocity,
                            ammoProjectile, // 使用玩家的弹药
                            damage,
                            knockback,
                            Projectile.owner
                        );
                    }
                }
            }
        }

        public override bool? CanDamage()
        {
            return false;
        }
        public override void PostDraw(Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            MiscShaderData msd = GameShaders.Misc["CalamityMod:WavyOpacity"];
            msd.SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/BlobbyNoise"), 1);
            msd.UseOpacity(0.7f);

            Vector2 glowOrigin = new Vector2(glowTex.Width / 2, glowTex.Height / 2); // 计算居中偏移
            Vector2 glowPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            DrawData dd = new()
            {
                texture = glowTex,
                position = glowPos,
                sourceRect = glowTex.Bounds,
            };
            msd.Apply(dd);
            Main.EntitySpriteDraw(glowTex, glowPos, null, dd.color, 0f, glowOrigin, 1f, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            if (DrawFlashTimer > 0)
            {
                float opacity = 1f - ((27 - DrawFlashTimer) / 27f);
                Vector2 flashOrigin = new Vector2(flashTex.Width / 2, flashTex.Height / 2); // 计算居中偏移
                Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                Main.EntitySpriteDraw(flashTex, drawPosition, null, Color.White * opacity, 0f, flashOrigin, 1f, SpriteEffects.None);
                DrawFlashTimer--;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;                    
        }
    }
}
