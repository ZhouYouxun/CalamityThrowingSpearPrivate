using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken
{
    internal class TheBrokenSpit : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/TheBroken/TheBroken";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3; // 穿透
            Projectile.timeLeft = 50;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 计算脉冲值，让拖尾闪动有节奏
                float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + i * 0.4f) * 0.5f + 0.5f;

                // 渐变色调：银 → 淡蓝 → 白，形成全息感
                Color baseColor = Color.Lerp(Color.Silver, Color.LightBlue, pulse);
                Color finalColor = Color.Lerp(baseColor, Color.White, 0.2f + 0.4f * pulse);
                finalColor *= 0.45f;
                finalColor.A = 0;

                // 拖尾位置计算
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                // 缩放与透明度衰减
                float scale = Projectile.scale * MathHelper.Lerp(0.8f, 0.3f, i / (float)Projectile.oldPos.Length);
                float intensity = MathHelper.Lerp(0.5f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                finalColor *= intensity;

                Main.EntitySpriteDraw(texture, drawPos, null, finalColor, Projectile.rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0);
            }

            return false; // 完全禁止默认绘制，本体不出现
        }



        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;



            //// 使用 localAI[1] 作为计时器
            //Projectile.localAI[1]++;

            //if (Projectile.localAI[1] >= 4)
            //{
            //    Projectile.localAI[1] = 0; // 重置

            //    // 计算朝向 + 旋转 180° 的偏移位置
            //    Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            //    Vector2 forwardOffset = direction.RotatedBy(MathHelper.Pi) * 16f;
            //    Vector2 glowPos = Projectile.Center + forwardOffset;

            //    CTSLightingBoltsSystem.Spawn_SilverSpearGlow(glowPos);
            //}

            // 使用 localAI[0] 作为冲击波释放计时器
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] >= 9)
            {
                Projectile.localAI[0] = 0;

                // 计算垂直于飞行方向的单位向量（逆时针旋转 90°）
                Vector2 normal = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);

                // 构建并释放 DirectionalPulseRing
                Particle pulse = new DirectionalPulseRing(
                    Projectile.Center,
                    normal * 2f, // 扩散速度向两侧较快
                    Color.White, // 银白色能量波
                    new Vector2(1f, 2.5f), // 椭圆形态（横宽纵长）
                    Projectile.rotation - MathHelper.PiOver4, // 方向和贴图一致
                    0.12f,
                    0.01f,
                    20
                );

                GeneralParticleHandler.SpawnParticle(pulse);
            }

        }




    }
}
