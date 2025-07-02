using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.LightingBolts.Particles
{
    public class EnhancementParticle : Particle
    {
        // 调用示范

        //EnhancementParticle particle = new EnhancementParticle(
        //    player.Center,
        //    new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f)),
        //    false,
        //    Main.rand.Next(40, 60),
        //    Main.rand.NextFloat(0.5f, 1f),
        //    Color.White,
        //    0.98f,
        //    Main.rand.NextFloat(-0.1f, 0.1f)
        //);
        //GeneralParticleHandler.SpawnParticle(particle);



        // 粒子颜色、运动相关参数
        public Color InitialColor;
        public bool AffectedByGravity;
        public float ShrinkSpeed;
        public float RotationSpeed;

        // 5 个可选贴图路径
        private static readonly string[] TexturePaths = new string[]
        {
            "CalamityThrowingSpear/LightingBolts/Particles/EnhancementParticle1",
            "CalamityThrowingSpear/LightingBolts/Particles/EnhancementParticle2",
            "CalamityThrowingSpear/LightingBolts/Particles/EnhancementParticle3",
            "CalamityThrowingSpear/LightingBolts/Particles/EnhancementParticle4",
            "CalamityThrowingSpear/LightingBolts/Particles/EnhancementParticle5"
        };

        // **修正关键点：保证 Mod 加载时 `Texture` 不为空**
        public override string Texture => "CalamityThrowingSpear/LightingBolts/Particles/EnhancementParticle1";

        private string SelectedTexture;

        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => false;

        /// <summary>
        /// 构造函数，创建粒子实例
        /// </summary>
        public EnhancementParticle(Vector2 position, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, float shrinkSpeed = 0.95f, float rotationSpeed = 0)
        {
            Position = position;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Scale = scale;
            Lifetime = lifetime;
            Color = InitialColor = color;
            ShrinkSpeed = shrinkSpeed;
            RotationSpeed = rotationSpeed;

            // **粒子生成时随机选择一个贴图**
            SelectedTexture = TexturePaths[Main.rand.Next(TexturePaths.Length)];
        }

        public override void Update()
        {
            Scale *= ShrinkSpeed;
            RotationSpeed *= ShrinkSpeed;
            Color = Color.Lerp(InitialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3D));
            Velocity *= 0.95f;

            if (AffectedByGravity)
            {
                Velocity.Y += 0.25f;
            }

            Rotation += RotationSpeed;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(1f, 1f) * Scale;

            // 加载贴图
            Texture2D texture = ModContent.Request<Texture2D>(SelectedTexture).Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/PearlParticleGlow").Value;

            // **先绘制 Glow（底光）**
            spriteBatch.Draw(
                glowTexture,
                Position - Main.screenPosition,
                null,
                Color * 0.6f, // 让底光稍微透明一点
                Rotation,
                glowTexture.Size() * 0.5f,
                scale * 2.5f, // **放大 Glow**
                SpriteEffects.None,
                0f
            );

            // **再绘制粒子本体**
            spriteBatch.Draw(
                texture,
                Position - Main.screenPosition,
                null,
                Color,
                Rotation,
                texture.Size() * 0.5f,
                scale * 3f, // **放大粒子**
                SpriteEffects.None,
                0f
            );
        }

    }
}
