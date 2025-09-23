using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    public class SunsetCConceptRightMagic : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        private float circularRotation;



        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 130;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // ========== 生存条件 ==========
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // 保持存在（timeLeft 锁定为 2）
            //if (Projectile.timeLeft < 2)
            //    Projectile.timeLeft = 2;

            // ========== 视觉数值 ==========
            float chargeupCompletion = Utils.GetLerpValue(0f, 90f, 180 - Projectile.timeLeft, true);
            Projectile.scale = MathHelper.Lerp(0f, 1.4f, chargeupCompletion);
            Projectile.Opacity = Projectile.scale * Projectile.scale;
            Projectile.rotation -= 0.004f;


            // ========== 视觉数值 ==========
            chargeupCompletion = Utils.GetLerpValue(0f, 90f, 180 - Projectile.timeLeft, true);

            // 淡入 (前 90 帧) 已经有了
            Projectile.scale = MathHelper.Lerp(0f, 1.4f, chargeupCompletion);
            Projectile.Opacity = Projectile.scale * Projectile.scale;

            // === 新增：淡出 (最后 40 帧) ===
            int fadeOutTime = 40;
            if (Projectile.timeLeft < fadeOutTime)
            {
                float fadeFactor = Projectile.timeLeft / (float)fadeOutTime;
                Projectile.Opacity *= fadeFactor; // 乘上一个递减系数
            }

            Projectile.rotation -= 0.004f;



            // ========== 锁定目标敌人 ==========
            NPC target = null;
            float maxDist = 1200f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy() && npc.Distance(owner.Center) < maxDist)
                {
                    maxDist = npc.Distance(owner.Center);
                    target = npc;
                }
            }
            // ========== 敌人存在时的位置逻辑 ==========
            if (target != null)
            {
                // 敌人头顶点（这里的 -100f 可以改成和 bigcut 一样的偏移量）
                Vector2 anchor = target.Center + new Vector2(0f, -20*16f);

                // 半径：决定绕多大圈旋转
                float radius = 80f;

                // 计算旋转偏移
                Vector2 offset = circularRotation.ToRotationVector2() * radius;

                // 设置最终位置 = 敌人头顶点 + 公转偏移
                Projectile.Center = anchor + offset;
            }
            else
            {
                Projectile.Kill();
                return;
            }

            // 更新旋转角度
            float t = (float)Main.GameUpdateCount / 60f;
            float baseSpeed = 0.03f;
            float speedVar = 0.02f;
            circularRotation += baseSpeed + speedVar * (float)Math.Sin(t * 1.2f);









        }





        public override bool PreDraw(ref Color lightColor)
        {
            // ===== 可调参数（函数内部顶端） =====
            float conceptScale = 0.25f;   // ConceptA/B/C 缩放倍率
            float extraScale = 0.75f;   // magic_01/02, star_07/08 缩放倍率
            float rotationMult = 3.5f;    // 全部旋转速度倍率

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 大贴图（需要缩放 conceptScale）
            Texture2D outerCircleTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptA").Value;
            Texture2D outerCircleGlowmask = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptB").Value;
            Texture2D innerCircleTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptC").Value;
            Texture2D innerCircleGlowmask = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;

            // 额外叠加的贴图（需要缩放 extraScale）
            Texture2D texMagic1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_01").Value;
            Texture2D texMagic2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_02").Value;
            Texture2D texStar7 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_07").Value;
            Texture2D texStar8 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_08").Value;

            float directionRotation = Projectile.velocity.ToRotation();
            Color startingColor = Color.Red;
            Color endingColor = Color.Blue;

            // ========== shader 重启函数 ==========
            void restartShader(Texture2D texture, float opacity, float circularRotation, BlendState blendMode)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, blendMode, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                CalamityUtils.CalculatePerspectiveMatricies(out Matrix viewMatrix, out Matrix projectionMatrix);

                GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseColor(startingColor);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSecondaryColor(endingColor);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSaturation(directionRotation);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseOpacity(Projectile.Opacity);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uDirection"].SetValue((float)Projectile.direction);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uCircularRotation"].SetValue(circularRotation);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uImageSize0"].SetValue(texture.Size());
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["overallImageSize"].SetValue(outerCircleTexture.Size());
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uWorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
                GameShaders.Misc["CalamityMod:RancorMagicCircle"].Apply();
            }

            // 外圈 Glowmask（旋转，缩放 conceptScale）
            restartShader(outerCircleGlowmask, Projectile.Opacity, Projectile.rotation * rotationMult, BlendState.Additive);
            Main.EntitySpriteDraw(outerCircleGlowmask, drawPos, null, Color.White, 0f,
                outerCircleGlowmask.Size() / 2f, Projectile.scale * conceptScale * 1.075f, SpriteEffects.None, 0);

            // 外圈本体（旋转，缩放 conceptScale）
            restartShader(outerCircleTexture, Projectile.Opacity * 0.7f, Projectile.rotation * rotationMult, BlendState.AlphaBlend);
            Main.EntitySpriteDraw(outerCircleTexture, drawPos, null, Color.White, 0f,
                outerCircleTexture.Size() / 2f, Projectile.scale * conceptScale, SpriteEffects.None, 0);

            // 内圈 Glowmask（固定，不旋转，缩放 conceptScale）
            restartShader(innerCircleGlowmask, Projectile.Opacity * 0.5f, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(innerCircleGlowmask, drawPos, null, Color.White, 0f,
                innerCircleGlowmask.Size() / 2f, Projectile.scale * conceptScale * 1.075f, SpriteEffects.None, 0);

            // 内圈本体（固定，不旋转，缩放 conceptScale）
            restartShader(innerCircleTexture, Projectile.Opacity * 0.7f, 0f, BlendState.AlphaBlend);
            Main.EntitySpriteDraw(innerCircleTexture, drawPos, null, Color.White, 0f,
                innerCircleTexture.Size() / 2f, Projectile.scale * conceptScale, SpriteEffects.None, 0);

            // ===== 叠加四个小贴图（缩放 extraScale） =====
            restartShader(texMagic1, Projectile.Opacity, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(texMagic1, drawPos, null, Color.White, 0f,
                texMagic1.Size() / 2f, Projectile.scale * extraScale, SpriteEffects.None, 0);

            restartShader(texMagic2, Projectile.Opacity, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(texMagic2, drawPos, null, Color.White, 0f,
                texMagic2.Size() / 2f, Projectile.scale * extraScale, SpriteEffects.None, 0);

            restartShader(texStar7, Projectile.Opacity, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(texStar7, drawPos, null, Color.White, 0f,
                texStar7.Size() / 2f, Projectile.scale * extraScale, SpriteEffects.None, 0);

            restartShader(texStar8, Projectile.Opacity, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(texStar8, drawPos, null, Color.White, 0f,
                texStar8.Size() / 2f, Projectile.scale * extraScale, SpriteEffects.None, 0);

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }









    }
}
























/*
 为什么之前的版本是错的，而现在这一版是对的：

 1) 矩阵错误：
    - 错误写法使用 Matrix.CreateLookAt / Matrix.CreateOrthographicOffCenter，
      只能生成普通的正交投影，画出来永远是平面圆圈，没有任何透视和椭圆变形效果。
    - 正确写法使用 CalamityUtils.CalculatePerspectiveMatricies，
      这是灾厄自带的透视矩阵工具，可以把圆形图案投影成椭圆，
      并随着旋转产生 3D 透视感，这是视觉效果的关键。

 2) 旋转参数没有分层：
    - 错误写法把所有贴图统一传入同一个 circularRotation，
      导致所有层一起旋转，没有区分。
    - 正确写法按照原版逻辑：
        外圈 (outerCircleTexture / outerCircleGlowmask) → 传 Projectile.rotation → 会旋转
        内圈 (innerCircleTexture / innerCircleGlowmask) → 传 0f → 固定不转
      这样外圈旋转、内圈稳定，才符合原版效果。

 3) 批处理 (SpriteBatch) 使用不当：
    - 错误写法用 foreach 简化了逻辑，所有层都用同一套参数和 BlendState，
      导致 Glowmask、本体、内外圈全部混在一起。
    - 正确写法在 restartShader 内部每次 End/Begin，切换 BlendState，
      确保 Glowmask 用 Additive，本体用 AlphaBlend，参数独立设置。
      这样才能保证渲染顺序、透明度和发光效果正确。

 → 总结：必须使用 CalamityUtils 的透视矩阵 + 内外圈分开旋转 + 每层单独重启 shader，
   整体效果才会和 RancorMagicCircle 保持一致。
*/
