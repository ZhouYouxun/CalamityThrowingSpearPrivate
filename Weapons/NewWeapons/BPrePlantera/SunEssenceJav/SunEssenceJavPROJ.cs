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
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Sounds;
using Terraria.Audio;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    public class SunEssenceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SunEssenceJav/SunEssenceJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        private bool isSpinning = false; // 标记是否进入高速旋转模式
        private int spinDuration = 90; // 高速旋转持续时间

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            //// 如果未进入高速旋转模式，保持原状
            //if (!isSpinning)
            //{
            //    CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            //    return false;
            //}

            //// 获取纹理资源和位置
            //Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            //Vector2 origin = texture.Size() * 0.5f;
            //Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            //// 背光效果部分 - 亮白色光晕
            //float chargeOffset = 3f; // 控制充能效果扩散的偏移量
            //float spinProgress = MathHelper.Clamp((90 - spinDuration) / 60f, 0f, 1f); // 线性增强过程，持续60帧
            //Color chargeColor = Color.White * (0.6f * spinProgress); // 根据进度调整透明度
            //chargeColor.A = 0; // 设置透明度

            //// 修复旋转逻辑，确保与速度方向同步
            //float rotation = Projectile.rotation;
            //SpriteEffects direction = SpriteEffects.None;

            //// 绘制充能效果 - 圆周上绘制多个充能光效
            //for (int i = 0; i < 8; i++)
            //{
            //    Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
            //    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            //}

            //// 渲染实际的投射物本体
            //Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 7;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // 进入旋转模式前正常飞行
            if (!isSpinning)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

                // 生成后方的火花特效
                if (Main.rand.NextFloat() < 0.2f) // 控制生成概率
                {
                    Vector2 sparkOffset = Projectile.velocity * -0.3f + Main.rand.NextVector2Circular(1f, 1f);

                    // 调整颜色的亮度，将透明度降低到原来的 25%
                    Color startColor = new Color(Color.LightGoldenrodYellow.R, Color.LightGoldenrodYellow.G, Color.LightGoldenrodYellow.B, (int)(Color.LightGoldenrodYellow.A * 0.25f));
                    Color endColor = new Color(Color.LightYellow.R, Color.LightYellow.G, Color.LightYellow.B, (int)(Color.LightYellow.A * 0.25f));

                    GenericSparkle sparker = new GenericSparkle(Projectile.Center + sparkOffset, Vector2.Zero, startColor, endColor, Main.rand.NextFloat(2.5f, 2.9f), 14, Main.rand.NextFloat(-0.01f, 0.01f), 2.5f);
                    GeneralParticleHandler.SpawnParticle(sparker);
                }

            }
            else
            {
                // 高速旋转逻辑
                Projectile.rotation += 0.45f;

                // 获得追踪能力
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 24f, 0.08f); // 追踪速度非常快，因为是直接粘在别人身上
                }

                // 定义旋转速度（每帧旋转的角度）
                float rotationSpeed = MathHelper.ToRadians(7f); // 每帧旋转 7 度
                Projectile.ai[0] += rotationSpeed;

                // 获取两个相反方向的基础角度
                float baseAngle1 = Projectile.ai[0];
                float baseAngle2 = baseAngle1 + MathHelper.Pi; // 相差180度

                // 生成两个方向的粒子特效
                for (int i = 0; i < 2; i++) // 两个方向
                {
                    float currentBaseAngle = (i == 0) ? baseAngle1 : baseAngle2;

                    // 每个方向生成粒子
                    for (int j = 0; j < 3; j++) // 每方向3个粒子
                    {
                        float randomAngle = currentBaseAngle + Main.rand.NextFloat(-MathHelper.Pi / 36, MathHelper.Pi / 36); // 随机偏移角度
                        Vector2 particleVelocity = randomAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f); // 随机速度

                        // 生成粒子
                        Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f); // 粒子初始位置
                        Color particleColor = Color.LightYellow; // 粒子颜色
                        float particleScale = 0.35f; // 粒子缩放

                        GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, particleVelocity, particleColor, particleScale, Main.rand.Next(20) + 10));
                    }
                }

                // 旋转攻击结束判断
                if (--spinDuration <= 0)
                {
                    Projectile.Kill(); // 在结束时触发 OnKill
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300); // 原版的破晓效果

            if (!isSpinning)
            {
                isSpinning = true;
                Projectile.velocity = Vector2.Zero; // 停止移动
                Projectile.timeLeft = spinDuration; // 保证旋转期间不消失
            }
            else
            {
                // 旋转期间每次造成伤害时召唤羽毛
                //int featherCount = Main.dayTime ? 3 : 1; // 白天时获得强化
                int featherCount = 3; // 不再变得强化，而是固定三个
                for (int i = 0; i < featherCount; i++)
                {
                    // 在主弹幕正上方50个方块的位置，以该点为圆心，半径15个方块的范围内随机生成
                    Vector2 featherSpawnPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f) * 16f, -50f * 16f);

                    // 计算羽毛向主弹幕位置的速度向量
                    Vector2 featherVelocity = (Projectile.Center - featherSpawnPosition).SafeNormalize(Vector2.UnitY) * 45f;

                    // 生成羽毛弹幕
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), featherSpawnPosition, featherVelocity, ModContent.ProjectileType<SunEssenceJavFeather>(), (int)(Projectile.damage * 0.6f), 0, Projectile.owner);
                }

            }
        }


        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item90, Projectile.Center);

            // 生成随机数量（2~3个）的 SunEssenceJavLightPoint 弹幕
            int lightPointCount = Main.rand.Next(2, 4); // 2~3 个弹幕
            for (int i = 0; i < lightPointCount; i++)
            {
                // 在绝对正上方 ±45 度范围内随机选择角度
                float randomAngle = MathHelper.ToRadians(Main.rand.Next(-45, 46)); // -45到45度
                Vector2 velocity = (Vector2.UnitY.RotatedBy(randomAngle) * -1f) * Main.rand.NextFloat(4f, 8f) * 0.75f;

                // 发射 SunEssenceJavLightPoint 弹幕
                //Projectile.NewProjectile(
                //    Projectile.GetSource_FromThis(),
                //    Projectile.Center,
                //    velocity,
                //    ModContent.ProjectileType<SunEssenceJavLightPoint>(),
                //    (int)(Projectile.damage * 1.0f), // 伤害倍率为 1.0
                //    Projectile.knockBack,
                //    Projectile.owner
                //);
            }

            // 播放爆炸特效
            Particle blastRing = new CustomPulse(
                Projectile.Center,
                Vector2.Zero,
                Color.Gold, // 亮黄色
                "CalamityThrowingSpear/Texture/ThebigExplosion1",
                Vector2.One * 0.33f,
                Main.rand.NextFloat(-10f, 10f),
                0.078f,
                0.450f,
                30
            );
            GeneralParticleHandler.SpawnParticle(blastRing);

            CreateSunParticleEffect();
        }





        // 生成太阳形状的粒子特效
        private void CreateSunParticleEffect()
        {
            int particleCount = 20; // 粒子数量，形成太阳形状
            float radius = 30f; // 粒子半径

            for (int i = 0; i < particleCount; i++)
            {
                // 每个粒子的角度间隔
                float angle = MathHelper.TwoPi * i / particleCount;

                // 计算粒子的方向和位置
                Vector2 particleDirection = angle.ToRotationVector2();
                Vector2 particlePosition = Projectile.Center + particleDirection * radius;

                // 生成粒子特效
                Dust dust = Dust.NewDustPerfect(particlePosition, DustID.SolarFlare, particleDirection * 2f, Scale: 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
                dust.color = Color.Yellow;
            }
        }

        // 创建粒子特效
        private void CreateParticleEffect()
        {
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, velocity, Scale: 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
                dust.color = Color.Orange;
            }
        }

   


    }
}

