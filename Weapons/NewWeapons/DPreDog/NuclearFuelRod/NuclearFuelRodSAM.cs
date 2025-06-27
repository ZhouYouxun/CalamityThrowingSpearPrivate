using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    internal class NuclearFuelRodSAM : ModProjectile, ILocalizedModType
    {

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.Opacity = 0f;
        }


        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.ai[0] += 1f;

            // 帧图切换（保留）
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 5 && Projectile.ai[0] < 480f)
                Projectile.frame = 3;
            else if (Projectile.frame > 7)
                Projectile.frame = 4;

            // 首次播放发射音效（保留）
            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                SoundEngine.PlaySound(SoundID.Item111, Projectile.Center);
            }

            // 飞行轨迹修改：
            Projectile.velocity *= 1.007f; // 指数加速
            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-1f)); // 左拐1°

            // 持续绿色飞行粒子特效（荧光绿色拖尾）
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 107, Scale: 1.2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Projectile.velocity * 0.3f;
                Main.dust[dust].color = Color.LimeGreen;
            }

            // 透明度渐变消失逻辑（保留）
            if (Projectile.ai[0] >= 480f)
            {
                if (Projectile.Opacity > 0f)
                {
                    Projectile.Opacity -= 0.02f;
                    if (Projectile.Opacity <= 0f)
                    {
                        Projectile.Opacity = 0f;
                        Projectile.Kill();
                    }
                }
            }
            else if (Projectile.Opacity < 0.9f)
            {
                Projectile.Opacity += 0.12f;
                if (Projectile.Opacity > 0.9f)
                    Projectile.Opacity = 0.9f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.15f + 0.85f;

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 获取当前帧图像
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);

            // 外圈脉动闪烁
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f - Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 1.2f;
                Color outerColor = Color.Lerp(Color.LimeGreen, Color.White, 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.2f + i));
                outerColor *= 0.3f * Projectile.Opacity; // 柔和发光效果

                float scale = 0.4f * pulse * (1 - i * 0.15f); // 缩小避免过大

                Main.EntitySpriteDraw(
                    texture,
                    drawPos,
                    frame, // 使用当前帧
                    outerColor,
                    angle,
                    new Vector2(frame.Width / 2f, frame.Height / 2f),
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            // 主体贴图发光（使用当前帧）
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                frame,
                Color.Lerp(Color.White, Color.LimeGreen, 0.5f) * Projectile.Opacity,
                Projectile.rotation,
                new Vector2(frame.Width / 2f, frame.Height / 2f),
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // 拖尾效果
            Color glowColor = Color.LimeGreen * Projectile.Opacity;
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], glowColor, 1);

            return false;
        }



        public override void OnKill(int timeLeft)
        {
            //SoundEngine.PlaySound(SoundID.Item14, Projectile.Center); // 爆炸音效

            // 更复杂华丽的死亡特效
            for (int i = 0; i < 40; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 107, Scale: 1.5f);
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(6f, 6f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].color = Color.LimeGreen;
            }

            //for (int i = 0; i < 20; i++)
            //{
            //    Gore gore = Gore.NewGorePerfect(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
            //        ModContent.Find<ModGore>("Terraria/Gore_139").Type, 1.0f);
            //    gore.timeLeft = 60;
            //}






        }


    }
}
