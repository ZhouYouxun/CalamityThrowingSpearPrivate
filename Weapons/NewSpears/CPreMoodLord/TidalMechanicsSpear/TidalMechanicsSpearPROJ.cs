using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using System;
using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using ReLogic.Content;

namespace CalamityThrowingSpear.Weapons.NewSpears.CPreMoodLord.TidalMechanicsSpear
{
    internal class TidalMechanicsSpearPROJ : ModProjectile
    {
        private float directionTimer = 0f; // 控制蛇形步的变量
        private bool isTurningLeft = true; // 是否正在向左拐
        private static readonly Color DeepSeaBlue = new(10, 30, 60); // 深海蓝
        private static readonly Color LightWaterBlue = new(50, 150, 255); // 浅水蓝


        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 250;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // 让弹幕自旋
            Projectile.rotation += 0.2f;

            // 生成十字星特效
            if (Main.rand.NextBool(4))
            {
                GenericSparkle sparker = new GenericSparkle(
                    Projectile.Center,
                    Vector2.Zero,
                    DeepSeaBlue,
                    LightWaterBlue,
                    Main.rand.NextFloat(1.8f, 2.5f),
                    5,
                    Main.rand.NextFloat(-0.01f, 0.01f),
                    1.68f
                );
                GeneralParticleHandler.SpawnParticle(sparker);
            }

            // 生成蓝色粒子 Dust
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 267, Projectile.velocity * 0.2f, 0, DeepSeaBlue);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.4f);
            }

            // **蛇形步实现**
            //directionTimer++;
            //if (directionTimer >= 15f) // 每 15 帧改变方向
            //{
            //    isTurningLeft = !isTurningLeft;
            //    directionTimer = 0f;
            //}

            //float turnAmount = 0.05f; // 每次拐弯的角度
            //if (isTurningLeft)
            //    Projectile.velocity = Projectile.velocity.RotatedBy(-turnAmount);
            //else
            //    Projectile.velocity = Projectile.velocity.RotatedBy(turnAmount);
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 45; // 拖尾长度
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            GameShaders.Misc["CalamityMod:PrismaticStreak"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak")
            );

            // **修改 overallOffset，让着色器起始点往后推 16 像素**
            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f
                - Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f; // 起始点位
            int numPoints = 45;

            // **拖尾直线效果**
            Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                trailPositions[i] = Projectile.oldPos[i]; // 直接使用原位置，不再进行偏移
            }

            PrimitiveRenderer.RenderTrail(
                trailPositions,
                new(
                    PrismaticWidthFunction,
                    PrismaticColorFunction,
                    (_) => overallOffset,
                    shader: GameShaders.Misc["CalamityMod:PrismaticStreak"]
                ),
                numPoints
            );

            // **绘制本体（缓慢旋转）**
            DrawProjectileBody();

            return false;
        }

        // **额外添加的本体绘制函数**
        private void DrawProjectileBody()
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // **让本体缓慢旋转（独立于 `Projectile.rotation`）**
            float slowRotation = Main.GlobalTimeWrappedHourly * 0.5f;

            Main.spriteBatch.Draw(texture, drawPosition, null, Color.White, slowRotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        }
        private float PrismaticWidthFunction(float completionRatio)
        {
            return 18f + (float)Math.Sin(completionRatio * 10f + Main.GlobalTimeWrappedHourly * 2f) * 3f;
        }

        private Color PrismaticColorFunction(float completionRatio)
        {
            float shift = (completionRatio + Main.GlobalTimeWrappedHourly * 0.5f) % 1f;
            return Color.Lerp(DeepSeaBlue, LightWaterBlue, completionRatio);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // **爆炸特效**
            SoundEngine.PlaySound(SoundID.Item74, Projectile.position);

            for (int i = 0; i < 20; i++)
            {
                Vector2 sparkVelocity = Main.rand.NextVector2Circular(6f, 6f);
                GenericSparkle spark = new GenericSparkle(
                    Projectile.Center,
                    sparkVelocity,
                    DeepSeaBlue,
                    LightWaterBlue,
                    Main.rand.NextFloat(2.0f, 3.2f),
                    8,
                    Main.rand.NextFloat(-0.02f, 0.02f),
                    2.5f
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, 267, Main.rand.NextVector2Circular(4f, 4f), 0, DeepSeaBlue);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
            }
        }
    }
}
