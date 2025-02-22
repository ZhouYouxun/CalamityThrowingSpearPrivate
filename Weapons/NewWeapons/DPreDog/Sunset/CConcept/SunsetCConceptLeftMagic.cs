using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Drawing;
using CalamityMod;
using System;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptLeftMagic : ModProjectile
    {
        public int TargetNPCIndex = -1; // 目标敌人索引
        private float RotationAngle = 0f; // 旋转角度
        private const float OpacityIncreaseRate = 0.05f; // 淡入速度
        private const float InnerRotationSpeed = 0.03f; // 内圈旋转速度
        private const float OuterRotationSpeed = -0.02f; // 外圈旋转速度（反向）

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.ignoreWater = true;
            Projectile.Opacity = 0f; // 初始完全透明
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 3; // 受击无敌帧
            Projectile.timeLeft = 36000;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 颜色数组（10 种颜色）
            Color[] colors = {
        Color.Black, Color.White, Color.Green, new Color(255, 105, 180), // 蓝粉
        Color.Blue, Color.Gold, new Color(50, 0, 50), // 紫黑
        Color.Red, Color.Gray, Color.Silver
    };

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi / 10 * i; // 计算均匀分布角度
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 160f;

                int projID = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + offset, Vector2.Zero,
                    ModContent.ProjectileType<SunsetCConceptLeftNoDamage>(),
                    0, 0, Projectile.owner,
                    Projectile.whoAmI,  // 让 NoDamage 绑定 Magic
                    i // 传递颜色索引
                );

                if (projID >= 0)
                {
                    Main.projectile[projID].ai[0] = Projectile.whoAmI; // 绑定 Magic
                    Main.projectile[projID].ai[1] = i; // 绑定颜色索引
                }
            }
        }

        public override void AI()
        {
            // **确保目标 NPC 存在**
            if (Projectile.ai[0] >= 0 && Main.npc[(int)Projectile.ai[0]].active)
            {
                TargetNPCIndex = (int)Projectile.ai[0];
            }
            else
            {
                // 目标死亡，弹幕立即消失
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[TargetNPCIndex];

            // **跟随目标**
            Projectile.Center = target.Center;

            // **淡入效果**
            if (Projectile.Opacity < 1f)
                Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + OpacityIncreaseRate, 0f, 1f);

            // **旋转**
            RotationAngle += OuterRotationSpeed;

            // **防止过早消失**
            Projectile.timeLeft = 6000;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = target.Center;

            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(16f, 16f);
                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.Keybrand,
                    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
                    Projectile.owner
                );
            }
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

            // **销毁 10 个 `NoDamage`**
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<SunsetCConceptLeftNoDamage>() && proj.ai[0] == Projectile.whoAmI)
                {
                    proj.Kill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D outerCircleTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D innerCircleTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftMagicInner").Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = outerCircleTexture.Size() * 0.5f;

            // **绘制外圈（逆时针旋转）**
            Main.EntitySpriteDraw(outerCircleTexture, drawPosition, null, Color.White * Projectile.Opacity, RotationAngle, origin, Projectile.scale, SpriteEffects.None, 0);

            // **绘制内圈（顺时针旋转）**
            //Main.EntitySpriteDraw(innerCircleTexture, drawPosition, null, Color.White * Projectile.Opacity, -RotationAngle * (InnerRotationSpeed / OuterRotationSpeed), origin, Projectile.scale, SpriteEffects.None, 0);


            //// 启用 Shader
            //restartShader(outerCircleTexture, Projectile.Opacity, RotationAngle, BlendState.Additive);
            //Main.EntitySpriteDraw(outerCircleTexture, drawPosition, null, Color.White, 0f, origin, Projectile.scale, SpriteEffects.None, 0);

            //restartShader(innerCircleTexture, Projectile.Opacity * 0.7f, -RotationAngle, BlendState.Additive);
            //Main.EntitySpriteDraw(innerCircleTexture, drawPosition, null, Color.White, 0f, origin, Projectile.scale, SpriteEffects.None, 0);

            //// 禁用 Shader
            //Main.spriteBatch.ExitShaderRegion();


            return false;
        }

        void restartShader(Texture2D texture, float opacity, float circularRotation, BlendState blendMode)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, blendMode, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            CalamityUtils.CalculatePerspectiveMatricies(out Matrix viewMatrix, out Matrix projectionMatrix);

            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseColor(Color.White);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uCircularRotation"].SetValue(circularRotation);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uWorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Apply();
        }

    }
}
