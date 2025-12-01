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

        // === 自定义旋转，用于魔法阵自转（不依赖 Projectile.rotation） ===
        private float magicCircleRot;

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // ===== 视觉淡入淡出 =====
            float chargeupCompletion = Utils.GetLerpValue(0f, 90f, 180 - Projectile.timeLeft, true);
            Projectile.scale = MathHelper.Lerp(0f, 1.4f, chargeupCompletion);
            Projectile.Opacity = Projectile.scale * Projectile.scale;

            int fadeOutTime = 40;
            if (Projectile.timeLeft < fadeOutTime)
            {
                float fadeFactor = Projectile.timeLeft / (float)fadeOutTime;
                Projectile.Opacity *= fadeFactor;
            }

            // ===== 锁定目标敌人 =====
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

            if (target == null)
            {
                Projectile.Kill();
                return;
            }

            // ===== 固定在敌人头顶，不公转 =====
            Vector2 anchor = target.Center + new Vector2(0f, -35 * 16f);
            Projectile.Center = anchor;

            // ===== 魔法阵自转变量（弹幕不转）=====
            magicCircleRot += 0.03f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float conceptScale = 0.45f;
            float extraScale = 1.25f;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D outerCircleTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptA").Value;
            Texture2D outerCircleGlowmask = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptB").Value;
            Texture2D innerCircleTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptC").Value;
            Texture2D innerCircleGlowmask = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;

            Texture2D texMagic1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_01").Value;
            Texture2D texMagic2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_02").Value;
            Texture2D texStar7 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_07").Value;
            Texture2D texStar8 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_08").Value;

            float directionRotation = Projectile.velocity.ToRotation();
            Color startingColor = new Color(255, 215, 0);
            Color endingColor = new Color(120, 90, 160);

            // === shader 启动函数 ===
            void restartShader(Texture2D texture, float circularRotation, BlendState blendMode)
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

            // 外圈 Glowmask（使用 magicCircleRot 自转）
            restartShader(outerCircleGlowmask, magicCircleRot, BlendState.Additive);
            Main.EntitySpriteDraw(outerCircleGlowmask, drawPos, null, Color.White, 0f,
                outerCircleGlowmask.Size() / 2f, Projectile.scale * conceptScale * 1.075f, SpriteEffects.None, 0);

            // 外圈本体
            restartShader(outerCircleTexture, magicCircleRot, BlendState.AlphaBlend);
            Main.EntitySpriteDraw(outerCircleTexture, drawPos, null, Color.White, 0f,
                outerCircleTexture.Size() / 2f, Projectile.scale * conceptScale, SpriteEffects.None, 0);

            // 内圈 Glowmask（不旋转）
            restartShader(innerCircleGlowmask, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(innerCircleGlowmask, drawPos, null, Color.White, 0f,
                innerCircleGlowmask.Size() / 2f, Projectile.scale * conceptScale, SpriteEffects.None, 0);

            // 内圈本体
            restartShader(innerCircleTexture, 0f, BlendState.AlphaBlend);
            Main.EntitySpriteDraw(innerCircleTexture, drawPos, null, Color.White, 0f,
                innerCircleTexture.Size() / 2f, Projectile.scale * conceptScale, SpriteEffects.None, 0);

            // 四个小贴图（不旋转）
            restartShader(texMagic1, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(texMagic1, drawPos, null, Color.White, 0f,
                texMagic1.Size() / 2f, Projectile.scale * extraScale, SpriteEffects.None, 0);

            restartShader(texMagic2, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(texMagic2, drawPos, null, Color.White, 0f,
                texMagic2.Size() / 2f, Projectile.scale * extraScale, SpriteEffects.None, 0);

            restartShader(texStar7, 0f, BlendState.Additive);
            Main.EntitySpriteDraw(texStar7, drawPos, null, Color.White, 0f,
                texStar7.Size() / 2f, Projectile.scale * extraScale, SpriteEffects.None, 0);

            restartShader(texStar8, 0f, BlendState.Additive);
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
