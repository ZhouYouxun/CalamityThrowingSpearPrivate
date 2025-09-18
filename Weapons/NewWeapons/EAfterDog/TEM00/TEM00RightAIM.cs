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
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00RightAIM : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Texture/KsTexture/magic_03";
       
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            // ========== 可调参数 ==========
            int totalLifetime = 15; // 总耗时
            float baseScale = 2.2f; // 初始缩放
            Color baseColor = Color.Cyan; // 基础颜色
            Color edgeColor = Color.White; // 渐变到的颜色
            float rotationSpeed = 0.25f; // 每帧旋转速度
                                         // =============================

            // 生命周期进度（0 = 出生, 1 = 死亡）
            float progress = 1f - (Projectile.timeLeft / (float)totalLifetime);
            float fade = 1f - progress; // 渐隐
            float scale = baseScale * (1f - progress); // 缩小

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 加载纹理
            Texture2D texMagic1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_01").Value;
            Texture2D texMagic2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_02").Value;
            Texture2D texMagic3 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_03").Value;
            Texture2D texMagic4 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_04").Value;

            Texture2D texTwirl1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;
            Texture2D texTwirl2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_02").Value;
            Texture2D texTwirl3 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_03").Value;

            Texture2D texHalo = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/fx_Halo2").Value;
            Texture2D texFlare = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/fx_Flare8").Value;

            // 颜色渐变
            Color col = Color.Lerp(baseColor, edgeColor, progress) * fade;

            // 中心点
            Vector2 origin;

            // ====== 魔法系列绘制 ======
            Texture2D[] magics = { texMagic1, texMagic2, texMagic3, texMagic4 };
            for (int i = 0; i < magics.Length; i++)
            {
                Texture2D tex = magics[i];
                origin = tex.Size() * 0.5f;
                float rot = (Projectile.whoAmI % 2 == 0 ? 1f : -1f) * rotationSpeed * Main.GlobalTimeWrappedHourly * (i + 1);
                spriteBatch.Draw(tex, drawPos, null, col, rot, origin, scale * (1.0f - i * 0.1f), SpriteEffects.None, 0f);
            }

            // ====== Twirl 系列绘制（稍大） ======
            Texture2D[] twirls = { texTwirl1, texTwirl2, texTwirl3 };
            for (int i = 0; i < twirls.Length; i++)
            {
                Texture2D tex = twirls[i];
                origin = tex.Size() * 0.5f;
                float rot = -rotationSpeed * Main.GlobalTimeWrappedHourly * (i + 1);
                spriteBatch.Draw(tex, drawPos, null, col * 0.8f, rot, origin, scale * 1.15f, SpriteEffects.None, 0f);
            }

            // ====== Halo 绘制 ======
            origin = texHalo.Size() * 0.5f;
            spriteBatch.Draw(texHalo, drawPos, null, col * 0.7f, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);

            // ====== Flare8 绘制（两次，互相垂直） ======
            origin = texFlare.Size() * 0.5f;
            spriteBatch.Draw(texFlare, drawPos, null, col * 0.9f, 0f, origin, scale * 1.1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texFlare, drawPos, null, col * 0.9f, MathHelper.PiOver2, origin, scale * 1.1f, SpriteEffects.None, 0f);

            return false;
        }




        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 80; // 范围型爆炸判定大小
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 35; // 存活时间短
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 600; // 防止重复命中
            //Projectile.alpha = 255; // 完全透明
        }

        public override void AI()
        {
            // 此处通常留空，仅用于保证存活期间的判定
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 命中 NPC 时触发，可添加爆炸特效、附加 Buff 等
        }

        public override void OnKill(int timeLeft)
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 center = Projectile.Center;

            int beamCount = 24; // 激光数量
            float radius = 600f; // 激光出生半径
            float baseRotation = Main.rand.NextFloat(MathHelper.TwoPi); // 起始角度随机，避免每次死板

            for (int i = 0; i < beamCount; i++)
            {
                // 每条激光的角度
                float angle = baseRotation + MathHelper.TwoPi * i / beamCount;

                // 出生点（圆周上）
                Vector2 spawnPos = center + angle.ToRotationVector2() * radius;

                // 方向指向中心，带一点随机偏移（±2°）
                float offset = MathHelper.ToRadians(Main.rand.NextFloat(-2f, 2f));
                Vector2 dir = (center - spawnPos).SafeNormalize(Vector2.UnitY).RotatedBy(offset);

                // 生成激光（用 TEM00LeftLazer）
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    dir,
                    ModContent.ProjectileType<TEM00LeftLazer>(),
                    Projectile.damage,
                    0f,
                    owner.whoAmI
                );
            }

            // === 绚烂特效 ===
            for (int i = 0; i < 40; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(center, DustID.Electric, dustVel);
                d.noGravity = true;
                d.scale = 1.2f + Main.rand.NextFloat(0.5f);
                d.color = Color.Lerp(Color.White, Color.Cyan, 0.6f);
            }

            SoundEngine.PlaySound(SoundID.Item122, center); // 激光能量爆裂音效
        }





    }
}
