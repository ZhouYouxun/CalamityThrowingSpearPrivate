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
using CalamityMod.Projectiles.Melee;

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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 65;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // **在命中敌人后 (phase == 2)，不绘制**
            if (phase == 2)
                return false;

            // 启用拖尾着色效果
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));

            // 渲染带有紫色渐变效果的光学尾迹
            Vector2 overallOffset = Projectile.Size * 0.5f;
            overallOffset += Projectile.velocity * 1.4f;
            int numPoints = 76;

            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);

            // 绘制弹幕本体
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value; // 获取弹幕的纹理
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2); // 计算纹理中心

            //使用 Projectile.Center 作为绘制位置，确保弹幕在正确位置绘制
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
        private int phase = 1; // 阶段 1: 普通飞行，阶段 2: 命中后特效
        private int shootTimer = 0; // 控制 6 帧间隔发射弹幕
        private int directionIndex = 0; // 用于循环选择四个方向
        private Vector2[] shootOffsets = new Vector2[]
        {
    new Vector2(1, -1),  // 右前
    new Vector2(-1, -1), // 左前
    new Vector2(-1, 1),  // 左后
    new Vector2(1, 1)    // 右后
        };

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 10; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 发出紫色光芒
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.55f);

            // 每帧加速
            Projectile.velocity *= 1.011f;

            // 每帧留下简单的紫色粒子特效
            Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Vector2.Zero).noGravity = true;

            if (phase == 1)
            {
                if (shootTimer <= 0)
                {
                    // 计算发射位置
                    Vector2 offset = shootOffsets[directionIndex] * (16f * 3f);
                    Vector2 spawnPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(offset.ToRotation()) * 3 * 16;

                    // **计算朝向主弹幕的速度**
                    Vector2 portalVelocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero) * 10f; // 让 Portal 速度指向主弹幕

                    // 生成 StreamGougePortal
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, portalVelocity, ModContent.ProjectileType<StreamGougePortal>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

                    // 更新方向索引
                    directionIndex = (directionIndex + 1) % shootOffsets.Length;

                    // 重置计时器
                    shootTimer = 6;
                }
                else
                {
                    shootTimer--;
                }

                // 生成特效
                GenerateSpiralEffects();
            }

        }
        private void GenerateSpiralEffects()
        {
            // FireworkFountain_Pink 和 FireworkFountain_Blue 进行螺旋运动
            float spiralRadius = 1.5f * 16f;
            float spiralAngle = Main.GlobalTimeWrappedHourly * 4f;

            for (int i = 0; i < 2; i++)
            {
                float angleOffset = (i == 0) ? 0f : MathHelper.Pi;
                float angle = spiralAngle + angleOffset;

                Vector2 dustPos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * spiralRadius;
                Dust dust = Dust.NewDustPerfect(dustPos, i == 0 ? DustID.FireworkFountain_Pink : DustID.FireworkFountain_Blue);
                dust.noGravity = true;
                dust.scale = 1.2f;
            }

            // Electric 进行双螺旋运动
            float electricAngle = Main.GlobalTimeWrappedHourly * 6f;
            for (int i = -1; i <= 1; i += 2)
            {
                float angle = electricAngle * i;
                Vector2 electricPos = Projectile.Center + new Vector2((float)Math.Cos(angle), 0) * 8f;
                Dust electricDust = Dust.NewDustPerfect(electricPos, DustID.Electric);
                electricDust.noGravity = true;
                electricDust.scale = 1.0f;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item74, target.Center);

            if (phase == 1) // 仅在第一阶段触发
            {
                phase = 2; // 切换至第二阶段
                Projectile.timeLeft = 120; // 保持存活时间用于特效
                //Projectile.velocity = Vector2.Zero; // 停止移动
                Projectile.alpha = 255;

                // 关闭伤害
                Projectile.friendly = false;

                // 生成超级特效
                GenerateImpactEffects(target.Center);

                // 生成爆炸
                //Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<StreamGougeJavEXP>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

                // 搜索 15 名最近敌人
                SpawnSplitProjectiles(target.Center);
            }
        }

        private void GenerateImpactEffects(Vector2 center)
        {
            // 原版 CreateGalaxyEffect 基础上加强
            int armCount = 6;
            int particlesPerArm = 15;
            float armRotation = MathHelper.TwoPi / armCount;

            for (int arm = 0; arm < armCount; arm++)
            {
                float angleOffset = arm * armRotation;
                for (int i = 0; i < particlesPerArm; i++)
                {
                    float progress = i / (float)particlesPerArm;
                    float spiralRadius = progress * 160f;
                    float particleAngle = angleOffset + progress * MathHelper.TwoPi;

                    Vector2 spawnPosition = center + new Vector2((float)Math.Cos(particleAngle), (float)Math.Sin(particleAngle)) * spiralRadius;
                    Vector2 velocity = Vector2.Normalize(spawnPosition - center) * Main.rand.NextFloat(3f, 7f);
                    StreamGougeMetaball.SpawnParticle(spawnPosition, velocity, Main.rand.NextFloat(50f, 100f));
                }
            }
        }

        private void SpawnSplitProjectiles(Vector2 center)
        {
            var enemies = Main.npc
                .Where(npc => npc.active && !npc.friendly && npc.life > 0)
                .OrderBy(npc => Vector2.Distance(npc.Center, center))
                .Take(15) // 取前 15 个
                .ToList();

            foreach (var enemy in enemies)
            {
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机角度
                Vector2 spawnPos = enemy.Center + new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * (3 * 16);

                // **确保小弹幕初始速度为零**
                Vector2 velocity = Vector2.Zero;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity, // 速度设为零，确保其开始旋转动画
                    ModContent.ProjectileType<StreamGougeJavPROJSPLIT>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }




        private void CreateGalaxyEffect(Vector2 center)
        {
            // 生成银河系悬臂
            int armCount = 4;
            int particlesPerArm = 50;
            float armRotation = MathHelper.TwoPi / armCount;

            for (int arm = 0; arm < armCount; arm++)
            {
                float angleOffset = arm * armRotation;

                for (int i = 0; i < particlesPerArm; i++)
                {
                    float progress = i / (float)particlesPerArm;
                    float spiralRadius = progress * 100f;
                    float particleAngle = angleOffset + progress * MathHelper.TwoPi;

                    Vector2 spawnPosition = center + new Vector2((float)Math.Cos(particleAngle), (float)Math.Sin(particleAngle)) * spiralRadius;
                    Vector2 velocity = Vector2.Normalize(spawnPosition - center) * Main.rand.NextFloat(2f, 5f);

                    StreamGougeMetaball.SpawnParticle(spawnPosition, velocity, Main.rand.NextFloat(20f, 40f));
                }
            }

            //// 在命中点生成两个同心圆的 ImpactParticle
            //for (int circle = 0; circle < 2; circle++)
            //{
            //    float radius = (circle + 1) * 30f;
            //    int impactParticleCount = 12;
            //    for (int i = 0; i < impactParticleCount; i++)
            //    {
            //        float angle = MathHelper.TwoPi / impactParticleCount * i;
            //        Vector2 position = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
            //        ImpactParticle impactParticle = new ImpactParticle(position, 0.1f, 20, 0.5f, Color.Cyan);
            //        GeneralParticleHandler.SpawnParticle(impactParticle);
            //    }
            //}

            ImpactParticle impactParticle = new ImpactParticle(center, 0.1f, 20, 0.5f, Color.Cyan);
            GeneralParticleHandler.SpawnParticle(impactParticle);
        }
        public override bool? CanDamage()
        {
            return base.CanDamage();
        }
        public override void OnKill(int timeLeft)
        {
            // 生成超级爆炸冲击波
            //Particle largePulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Purple, new Vector2(5f, 5f), 20f, 0.3f, 6f, 30);
            //GeneralParticleHandler.SpawnParticle(largePulse);

            // 生成超级银河系特效
            CreateGalaxyEffect(Projectile.Center, true);

            // 释放伤害倍率为1.0的 StreamGougeJavEXP 弹幕
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<StreamGougeJavEXP>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }

        private void CreateGalaxyEffect(Vector2 center, bool enhanced)
        {
            int armCount = enhanced ? 8 : 4;
            int particlesPerArm = enhanced ? 100 : 50;
            float armRotation = MathHelper.TwoPi / armCount;

            for (int arm = 0; arm < armCount; arm++)
            {
                float angleOffset = arm * armRotation;

                for (int i = 0; i < particlesPerArm; i++)
                {
                    float progress = i / (float)particlesPerArm;
                    float spiralRadius = progress * (enhanced ? 200f : 100f);
                    float particleAngle = angleOffset + progress * MathHelper.TwoPi;

                    Vector2 spawnPosition = center + new Vector2((float)Math.Cos(particleAngle), (float)Math.Sin(particleAngle)) * spiralRadius;
                    Vector2 velocity = Vector2.Normalize(spawnPosition - center) * Main.rand.NextFloat(2f, 5f);

                    StreamGougeMetaball.SpawnParticle(spawnPosition, velocity, Main.rand.NextFloat(20f, enhanced ? 80f : 40f));
                }
            }

            if (enhanced)
            {
                // 增强模式下额外增加粒子效果
                for (int i = 0; i < 50; i++)
                {
                    Vector2 randomOffset = Main.rand.NextVector2Circular(300f, 300f);
                    StreamGougeMetaball.SpawnParticle(center + randomOffset, -randomOffset * 0.1f, Main.rand.NextFloat(40f, 100f));
                }
            }
        }



    }
}