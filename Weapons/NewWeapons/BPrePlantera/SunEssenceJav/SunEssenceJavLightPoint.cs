using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityMod.Graphics.Primitives;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav
{
    internal class SunEssenceJavLightPoint : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectile.CPreMoodLord";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Projectile.MaxUpdates * 180;
            Projectile.extraUpdates = 3;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.aiStyle = ProjAIStyleID.Bounce;
        }
        private float rotationSpeedBase = 0.1f; // 初始旋转速度
        private float rotationSpeedMax = 0.55f; // 最大旋转速度
        private int rotationCycleDuration = 120; // 完整周期的帧数
        private int currentCycleFrame = 0; // 当前帧计数器
        public override void AI()
        {
            // 计算周期性波动的旋转速度
            currentCycleFrame = (currentCycleFrame + 1) % rotationCycleDuration;
            float waveProgress = (float)currentCycleFrame / rotationCycleDuration;
            float rotationSpeed = MathHelper.Lerp(rotationSpeedBase, rotationSpeedMax, (float)Math.Sin(waveProgress * MathHelper.TwoPi) * 0.5f + 0.5f);

            // 应用旋转速度
            Projectile.rotation += rotationSpeed;

            // 生成动态光学效果
            if (Main.rand.NextBool(3))
            {
                Dust dynamicLight = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, Main.rand.NextVector2Circular(3f, 3f), 0, Color.Yellow, 1.5f);
                dynamicLight.noGravity = true;
                dynamicLight.fadeIn = 1.2f;
            }
        }
        //public override bool OnTileCollide(Vector2 oldVelocity)
        //{
        //    // 在碰撞时释放光明粒子特效
        //    for (int i = 0; i < Main.rand.Next(8, 15); i++) // 随机生成 8~15 个粒子
        //    {
        //        // 粒子生成位置
        //        Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);

        //        // 粒子速度
        //        Vector2 particleVelocity = Main.rand.NextVector2Circular(2f, 5f);

        //        // 生成粒子特效
        //        Dust lightDust = Dust.NewDustPerfect(
        //            particlePosition, // 粒子位置
        //            DustID.SolarFlare, // 粒子类型
        //            particleVelocity, // 粒子速度
        //            0, // 不使用额外的Alpha值
        //            Color.White, // 颜色设置为亮白色
        //            Main.rand.NextFloat(1.2f, 1.8f) // 粒子大小随机
        //        );
        //        lightDust.noGravity = true; // 粒子不受重力影响
        //        lightDust.fadeIn = Main.rand.NextFloat(0.8f, 1.5f); // 粒子逐渐显现
        //    }

        //    // 保持弹跳逻辑
        //    return false;
        //}

        public override void OnKill(int timeLeft)
        {
            // 生成烟雾特效
            for (int i = 0; i < 4; i++) // 四个方向
            {
                float baseAngle = MathHelper.PiOver2 * i; // 每个方向间隔90度
                for (int j = 0; j < 15; j++) // 每个方向生成15个烟雾
                {
                    float speed = 1f + j * 0.2f; // 速度逐渐增加
                    Vector2 velocity = baseAngle.ToRotationVector2() * speed;

                    Particle smoke = new HeavySmokeParticle(
                        Projectile.Center,
                        velocity,
                        Color.LightYellow, // 改为亮白色
                        18,
                        Main.rand.NextFloat(0.9f, 1.6f),
                        0.35f,
                        Main.rand.NextFloat(-1, 1),
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
        }
        public Color TrailColor(float completionRatio)
        {
            // 定义渐变起点和终点颜色
            Color startColor = Color.Yellow; // 亮黄色
            Color endColor = Color.White; // 亮白色

            // 根据完成度计算颜色渐变
            float brightness = MathHelper.Lerp(0.8f, 1f, completionRatio); // 调整亮度，完成度越高越亮
            float opacity = MathHelper.Lerp(1f, 0.5f, completionRatio); // 透明度渐变，尾部更透明

            // 返回混合颜色
            return Color.Lerp(startColor * brightness, endColor * brightness, completionRatio) * opacity;
        }
    

        public float TrailWidth(float completionRatio)
        {
            // 计算宽度变化的插值因子
            float widthInterpolant = Utils.GetLerpValue(0f, 0.25f, completionRatio, true) * Utils.GetLerpValue(1.1f, 0.7f, completionRatio, true);

            // 平滑调整轨迹宽度，完成度越高宽度越大
            return MathHelper.SmoothStep(8f, 20f, widthInterpolant);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取弹幕贴图及其绘制信息
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY;
            Vector2 origin = texture.Size() * 0.5f;

            // 启用着色器区域
            Main.spriteBatch.EnterShaderRegion();

            // 设置着色器的纹理资源
            GameShaders.Misc["CalamityMod:ArtAttack"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));

            // 应用着色器
            GameShaders.Misc["CalamityMod:ArtAttack"].Apply();

            // 绘制动态拖尾效果
            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos, // 使用历史轨迹点
                new( // 创建拖尾的绘制逻辑
                    TrailWidth, // 拖尾宽度函数
                    TrailColor, // 拖尾颜色函数
                    (_) => Projectile.Size * 0.5f, // 偏移量设置
                    shader: GameShaders.Misc["CalamityMod:ArtAttack"] // 使用的着色器
                ),
                180 // 拖尾最大长度
            );

            // 退出着色器区域
            Main.spriteBatch.ExitShaderRegion();

            // 绘制弹幕本体
            Main.EntitySpriteDraw(
                texture, // 贴图资源
                drawPosition, // 绘制位置
                null, // 不使用贴图裁剪
                Projectile.GetAlpha(Color.White), // 应用弹幕透明度
                Projectile.rotation, // 应用旋转
                origin, // 旋转中心
                Projectile.scale, // 缩放比例
                0, // 不使用翻转
                0 // 深度值
            );

            return false; // 阻止默认绘制
        }
    }
}
