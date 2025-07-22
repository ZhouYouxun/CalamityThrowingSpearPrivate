using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CalamityMod;


namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    internal class NuclearFuelRodLightCircle : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // 使用完全透明贴图

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // === 🧪 可调参数区 ===

            // Extra_89
            Color extra89Color = new Color(100, 255, 100) * 1.6f;
            float extra89BaseScale = 0.25f;
            float extra89ScaleOffset = 0.05f;

            // IonizingRadiation
            Color radiationColor = new Color(100, 255, 100) * 0.25f;
            float radiationBaseScale = 0.2f;
            float radiationScaleOffset = 0.05f;

            // fx_Halo2
            Color haloColor = new Color(100, 255, 100) * 0.22f;
            float haloBaseScale = 0.95f;
            float haloScaleOffset = 0.3f;

            // fx_Smoke15
            Color smokeColor = new Color(100, 255, 100) * 0.18f;
            float smokeBaseScale = 0.05f;
            float smokeScaleOffset = 0.2f;

            float pulse = 1f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);

            // === 🌟 Extra_89 - 旋转能量圈 ===
            Texture2D extra89 = Terraria.GameContent.TextureAssets.Extra[89].Value;
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 0.6f;
                float scale = (extra89BaseScale + extra89ScaleOffset * i) * pulse * 2.5f;
                Main.EntitySpriteDraw(extra89, drawPos, null, extra89Color, angle, extra89.Size() / 2f, scale, SpriteEffects.None, 0);
            }

            // === ☢ 放射标志 x2 层，反向叠加 ===
            Texture2D radTex = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/IonizingRadiation").Value;
            for (int i = 0; i < 2; i++)
            {
                float rot = Main.GlobalTimeWrappedHourly * (i == 0 ? 1f : -1f);
                float scale = radiationBaseScale + radiationScaleOffset * i;
                Main.EntitySpriteDraw(radTex, drawPos, null, radiationColor, rot, radTex.Size() / 2f, scale, SpriteEffects.None, 0);
            }

            // === 💥 fx_Halo2 爆炸光圈 x2 ===
            Texture2D halo = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/fx_Halo2").Value;
            for (int i = 0; i < 2; i++)
            {
                float rot = Main.GlobalTimeWrappedHourly * (i == 0 ? 0.8f : -0.6f);
                float scale = haloBaseScale + haloScaleOffset * i;
                Main.EntitySpriteDraw(halo, drawPos, null, haloColor, rot, halo.Size() / 2f, scale, SpriteEffects.None, 0);
            }

            // === 🌫 fx_Smoke15 气浪叠加 x2 ===
            Texture2D smoke = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/fx_Smoke15").Value;
            for (int i = 0; i < 2; i++)
            {
                float rot = Main.GlobalTimeWrappedHourly * (i == 0 ? -0.4f : 0.7f);
                float scale = smokeBaseScale + smokeScaleOffset * i;
                Main.EntitySpriteDraw(smoke, drawPos, null, smokeColor, rot, smoke.Size() / 2f, scale, SpriteEffects.None, 0);
            }

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 800; // 范围型爆炸判定大小
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15; // 存活时间短
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60; // 防止重复命中
            Projectile.alpha = 255; // 完全透明
        }
        public override void AI()
        {
            // 绑定到主弹幕
            int parentIndex = (int)Projectile.ai[0];

            if (!parentIndex.WithinBounds(Main.maxProjectiles) || !Main.projectile[parentIndex].active || Main.projectile[parentIndex].type != ModContent.ProjectileType<NuclearFuelRodPROJ>())
            {
                Projectile.Kill(); // 主弹幕消失则自杀
                return;
            }

            // 每帧同步位置 & 延长生命周期
            Projectile.Center = Main.projectile[parentIndex].Center;
            Projectile.timeLeft = 15;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 120);
        }

        public override void OnKill(int timeLeft)
        {
            // 弹幕消失时触发，可用于播放音效、生成爆炸粒子等
        }
    }
}
