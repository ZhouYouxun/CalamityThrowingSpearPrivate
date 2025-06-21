using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.NadirC
{
    public class NadirJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/EAfterDog/NadirC/NadirJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";

        private static Color ShaderColorOne = Color.Black;
        private static Color ShaderColorTwo = Color.DarkGray;
        private static Color ShaderEndColor = Color.DarkBlue;

        private Vector2 altSpawn;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        private float PrimitiveWidthFunction(float completionRatio)
        {
            float arrowheadCutoff = 0.36f;
            float width = 75f;
            float minHeadWidth = 0.03f;
            float maxHeadWidth = width;
            if (completionRatio <= arrowheadCutoff)
                width = MathHelper.Lerp(minHeadWidth, maxHeadWidth, Utils.GetLerpValue(0f, arrowheadCutoff, completionRatio, true));
            return width;
        }

        private Color PrimitiveColorFunction(float completionRatio)
        {
            float endFadeRatio = 0.41f;
            float completionRatioFactor = 2.7f;
            float globalTimeFactor = 5.3f;
            float endFadeFactor = 3.2f;
            float endFadeTerm = Utils.GetLerpValue(0f, endFadeRatio * 0.5f, completionRatio, true) * endFadeFactor;
            float cosArgument = completionRatio * completionRatioFactor - Main.GlobalTimeWrappedHourly * globalTimeFactor + endFadeTerm;
            float startingInterpolant = (float)Math.Cos(cosArgument) * 0.5f + 0.5f;

            float colorLerpFactor = 0.6f;
            Color startingColor = Color.Lerp(ShaderColorOne, ShaderColorTwo, startingInterpolant * colorLerpFactor);
            return Color.Lerp(startingColor, ShaderEndColor, MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0f, endFadeRatio, completionRatio, true)));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 保留现有的拖尾效果
            //CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            // 绘制黑色着色效果
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            Vector2 overallOffset = Projectile.Size * 0.5f;
            overallOffset += Projectile.velocity * 1.4f;
            int numPoints = 96;
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);

            // 绘制本体
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f); // 确保绘制中心为本体中心
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, effects, 0);

            return false; // 我们已经手动绘制了弹幕，返回false以防止默认绘制
        }



        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 400;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        private int particleTimer = 0; // 计时器
        private int spawnedProjectiles = 0;

        public override void AI()
        {
            Projectile.velocity *= 1.005f;

            // 旋转与光效
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

            // 每隔 20 帧生成一次粒子
            particleTimer++;
            if (particleTimer >= 20)
            {
                particleTimer = 0; // 重置计时器

                // 随机在后方左右各 10 度生成粒子
                float angleOffset = Main.rand.NextFloat(-MathHelper.ToRadians(10), MathHelper.ToRadians(10));
                Vector2 particleVelocity = Projectile.velocity.RotatedBy(MathHelper.Pi + angleOffset) * 0.5f; // 反向且速度减小

                // 随机颜色为深灰色或黑色
                Color particleColor = Main.rand.NextBool() ? Color.DarkGray : Color.Black;

                // 随机缩放
                float randomScale = Main.rand.NextFloat(0.5f, 1.0f);

                // 创建并生成粒子
                Particle bolt = new CrackParticle(
                    Projectile.Center,
                    particleVelocity,
                    particleColor * 0.65f,
                    Vector2.One * randomScale,
                    0,
                    0,
                    randomScale,
                    11
                );
                GeneralParticleHandler.SpawnParticle(bolt);
            }



            // 计时器增加
            Projectile.ai[0]++;

            // 从飞行后的第 15 帧开始，每 6 帧触发一次
            if (Projectile.ai[0] >= 15 && Projectile.ai[0] % 6 == 0 && spawnedProjectiles < 25)
            {
                // 计算圆圈中心（绝对正下方 50~60 格）
                Vector2 spawnCenter = Projectile.Center + new Vector2(0, Main.rand.NextFloat(50 * 16, 60 * 16));

                // 在 13 格半径范围内随机选择一个点
                Vector2 spawnPosition = spawnCenter + Main.rand.NextVector2Circular(13 * 16, 13 * 16);

                // 计算朝向自身的初始速度（自身速度的 1 倍）
                Vector2 velocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero) * (Projectile.velocity.Length() * 1f);

                // 生成新的 `NadirJavVoidEssence` 弹幕
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<NadirJavVoidEssence>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );

                // 释放计数增加
                spawnedProjectiles++;
            }
        }

        private void CreatePurpleDustCircle()
        {
            int circleDust = 18;
            Vector2 baseDustVel = new Vector2(3.8f, 0f);
            for (int i = 0; i < circleDust; ++i)
            {
                int dustID = 27;
                float angle = i * (MathHelper.TwoPi / circleDust);
                Vector2 dustVel = baseDustVel.RotatedBy(angle);
                int idx = Dust.NewDust(Projectile.Center, 1, 1, dustID);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].position = Projectile.Center;
                Main.dust[idx].velocity = dustVel;
                Main.dust[idx].scale = 2.4f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //// 随机释放1-3个VoidTentacle，伤害为50%
            //int tentacleCount = Main.rand.Next(1, 2);
            //for (int i = 0; i < tentacleCount; i++)
            //{
            //    Vector2 spawnVelocity = Projectile.velocity.RotatedByRandom(MathHelper.TwoPi) * 2f;
            //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, spawnVelocity, ModContent.ProjectileType<NadirJavVoidEssence>(), (int)(Projectile.damage * 0.25f), Projectile.knockBack, Projectile.owner);
            //}
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item104, Projectile.Center);

            // 1. 生成重型烟雾粒子，颜色为紫色和黑色，左右 30 度范围内随机
            int smokeParticleCount = 20; // 粒子数量可根据需求调整
            for (int i = 0; i < smokeParticleCount; i++)
            {
                // 随机在左右 30 度范围内生成粒子
                float angleOffset = Main.rand.NextFloat(-MathHelper.ToRadians(30), MathHelper.ToRadians(30));
                Vector2 smokeVelocity = Projectile.velocity.RotatedBy(angleOffset) * Main.rand.NextFloat(0.5f, 1.5f);
                Color smokeColor = Main.rand.NextBool() ? Color.Purple : Color.Black; // 颜色为紫色或黑色
                float smokeLifetime = Main.rand.Next(30, 60); // 粒子存活时间

                // 创建并生成重型烟雾粒子
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    smokeVelocity,
                    smokeColor,
                    (int)smokeLifetime,
                    Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f),
                    1.0f,
                    MathHelper.ToRadians(2f),
                    required: true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 计算圆圈中心（绝对正下方 50~60 格）
            Vector2 spawnCenter = Projectile.Center + new Vector2(0, Main.rand.NextFloat(50 * 16, 60 * 16));

            for (int i = 0; i < 6; i++)
            {
                // 在 25 格半径范围内随机选择一个点
                Vector2 spawnPosition = spawnCenter + Main.rand.NextVector2Circular(25 * 16, 25 * 16);

                // 计算朝向自身的初始速度，并在 -10 到 10 度内随机偏移
                float angleOffset = Main.rand.NextFloat(-MathHelper.ToRadians(10), MathHelper.ToRadians(10));
                Vector2 velocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero).RotatedBy(angleOffset) * (Projectile.velocity.Length() * 1f);

                // 生成新的 `NadirJavVoidEssence` 弹幕
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<NadirJavVoidEssence>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        // 这个新的函数，用于在指定位置生成紫色粒子特效
        private void CreatePurpleDustCircleAtPosition(Vector2 position)
        {
            int circleDust = 18;
            Vector2 baseDustVel = new Vector2(3.8f, 0f);
            for (int i = 0; i < circleDust; ++i)
            {
                int dustID = 27;
                float angle = i * (MathHelper.TwoPi / circleDust);
                Vector2 dustVel = baseDustVel.RotatedBy(angle);
                int idx = Dust.NewDust(position, 1, 1, dustID);
                Main.dust[idx].noGravity = true;
                Main.dust[idx].position = position;
                Main.dust[idx].velocity = dustVel;
                Main.dust[idx].scale = 2.4f;
            }
        }









    }
}
