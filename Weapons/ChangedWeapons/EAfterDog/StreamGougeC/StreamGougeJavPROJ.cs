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
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using CalamityMod.Graphics.Primitives;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.StreamGougeC
{
    public class StreamGougeJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/EAfterDog/StreamGougeC/StreamGougeJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        private static Color ShaderColorOne = Color.Purple;      // 紫色
        private static Color ShaderColorTwo = Color.Gray;        // 灰色
        private static Color ShaderEndColor = Color.MediumPurple;// 中紫色

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 保留现有的拖尾效果
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            // 启用拖尾着色效果
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));

            // 渲染带有紫色渐变效果的光学尾迹
            Vector2 overallOffset = Projectile.Size * 0.5f;
            overallOffset += Projectile.velocity * 1.4f;
            int numPoints = 76;

            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);

            // 绘制弹幕本体
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value; // 获取弹幕的纹理
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2); // 计算纹理中心

            // 使用 Projectile.Center 作为绘制位置，确保弹幕在正确位置绘制
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,  // 修正偏移量，确保弹幕在正确位置
                null,
                lightColor,
                Projectile.rotation,  // 使用弹幕的旋转角度
                drawOrigin,  // 纹理的中心作为旋转中心
                Projectile.scale,  // 弹幕的缩放
                SpriteEffects.None,
                0);

            return false; // 返回 false，因为我们已经手动绘制了本体
        }


        private float PrimitiveWidthFunction(float completionRatio)
        {
            return MathHelper.Lerp(36f, 76f, completionRatio); // 拖尾宽度函数
        }

        private Color PrimitiveColorFunction(float completionRatio)
        {
            // 使用紫色、灰色和中紫色生成渐变效果
            float colorLerpFactor = 0.6f;
            float cosArgument = completionRatio * 2.7f - Main.GlobalTimeWrappedHourly * 5.3f;
            float startingInterpolant = (float)Math.Cos(cosArgument) * 0.5f + 0.5f;
            Color startingColor = Color.Lerp(ShaderColorOne, ShaderColorTwo, startingInterpolant * colorLerpFactor);
            return Color.Lerp(startingColor, ShaderEndColor, MathHelper.SmoothStep(0f, 1f, completionRatio));
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 360;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 20; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 发出紫色光芒
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.55f);

            // 每帧加速
            Projectile.velocity *= 1.005f;

            // 保持直线运动30帧，之后开始追踪
            if (Projectile.ai[1] > 5)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 在1800距离内查找最近敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 20f, 0.08f);
                }

                if (Projectile.localAI[0] == 0f)
                {
                    // 三角形的半径，控制传送门与中心的距离
                    float radius = 50f; // 可根据需要调整

                    // 计算三个传送门随机角度的位置
                    Vector2[] portalPositions = new Vector2[3];
                    for (int i = 0; i < 3; i++)
                    {
                        // 每个传送门的随机角度
                        float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi); // 0 到 360 度的随机角度
                        portalPositions[i] = Projectile.Center + new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * radius;
                    }

                    // 生成三个传送门弹幕
                    foreach (Vector2 position in portalPositions)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero, ModContent.ProjectileType<StreamGougeJavPortal>(), (int)(Projectile.damage * 1), Projectile.knockBack, Projectile.owner);
                    }


                    // 在原地生成2到6发随机分裂弹幕
                    int numSplits = Main.rand.Next(3, 4); // 固定生成3发弹幕
                    for (int i = 0; i < numSplits; i++)
                    {
                        // 随机角度在 -10 度到 10 度之间
                        float randomAngle = Main.rand.NextFloat(-10f, 10f);

                        // 计算旋转后的方向
                        Vector2 splitDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(randomAngle));

                        // 生成分裂弹幕
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, splitDirection * 0.5f, ModContent.ProjectileType<StreamGougeJavPROJSPLIT>(), (int)(Projectile.damage * 0.4), Projectile.knockBack, Projectile.owner);
                    }


                    // 生成紫色小型冲击波
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Purple, new Vector2(1f, 1f), Main.rand.NextFloat(6f, 10f), 0.15f, 3f, 10);
                    GeneralParticleHandler.SpawnParticle(pulse);

                    // 设置标志位为1，确保只触发一次
                    Projectile.localAI[0] = 1f;
                }
            }
            else
            {
                Projectile.ai[1]++;
            }

            // 每帧留下简单的紫色粒子特效
            Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Vector2.Zero).noGravity = true;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[1] >= 30)
            {
                // 在追踪阶段之后，将穿透次数设置为 1
                Projectile.penetrate = 1;
            }

            // 添加弑神者火焰buff，持续300帧
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 300);

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item74, target.Center);

            // ImpactParticle 特效
            if (Main.netMode != NetmodeID.Server)
            {
                Color impactColor = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat(0.3f, 0.64f));
                Vector2 impactPoint = Vector2.Lerp(Projectile.Center, target.Center, 0.65f);
                ImpactParticle impactParticle = new ImpactParticle(impactPoint, 0.1f, 20, Main.rand.NextFloat(0.4f, 0.5f), impactColor);
                GeneralParticleHandler.SpawnParticle(impactParticle);
            }

            // 生成宇宙能量光球粒子效果
            for (int i = 0; i < 20; i++)
            {
                Vector2 spawnPosition = target.Center + Main.rand.NextVector2Circular(30f, 30f);
                StreamGougeMetaball.SpawnParticle(spawnPosition, Main.rand.NextVector2Circular(3f, 3f), 60f);

                float scale = MathHelper.Lerp(24f, 64f, CalamityUtils.Convert01To010(i / 19f));
                spawnPosition = target.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * MathHelper.Lerp(-40f, 90f, i / 19f);
                Vector2 particleVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.23f) * Main.rand.NextFloat(2.5f, 9f);
                StreamGougeMetaball.SpawnParticle(spawnPosition, particleVelocity, scale);
            }
        }




    }
}
