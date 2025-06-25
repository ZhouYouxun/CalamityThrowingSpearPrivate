using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using System.Linq;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC
{
    public class DragonRageJavFire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/Magic/RancorFog"; // 透明烟雾贴图

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }

        private Player owner;
        private bool isTracking = true;
        private float orbitRadius;
        private float orbitAngle;
        private Vector2 targetPosition;
        private bool isDashing = false;

        private Color FireColor = new Color(255, 80, 30); // 更加强烈的红橙色
        private const float MaxScale = 0.75f; // 火焰最大缩放参考值
        private int Time = 0; // 用于粒子节奏控制

        private int flightStage = 0; // 0 = 游荡, -1 = 插值进入轨道, 1 = 旋转, 2 = 冲刺
        private int flightTimer = 0; // 阶段计时器
        private int rotationDuration = 150; // 第 2 阶段持续时间
        private Vector2 initialRandomVelocity; // 第一阶段漂移速度

        private Vector2 orbitShape = Vector2.One; // 椭圆轨道尺寸
        private float orbitSpeed = 1f; // 转速倍率
        private float orbitStartAngle = 0f; // 起始角度

        private bool isEnteringOrbit = false; // 是否在过渡阶段
        private Vector2 orbitStartPosition; // 插值目标点
        private float orbitLerpProgress = 0f; // 插值进度

        private Vector2 lockedMousePosition = Vector2.Zero;

        private Vector2 lockedDirection = Vector2.Zero;





        // **查找最近的 `DragonRageJavPROJ`**
        private Vector2 FindDragonRageJavPROJCenter()
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<DragonRageJavPROJ>())
                    return proj.Center;
            }
            return Main.player[Projectile.owner].Center; // 如果找不到，默认返回玩家中心
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // === 1️⃣ 状态检测：是否应该进入冲刺或销毁自身 ===

            bool shouldEnterDash = true;
            bool shouldDestroy = true;

            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<DragonRageJavPROJ>())
                {
                    DragonRageJavPROJ mainProj = proj.ModProjectile as DragonRageJavPROJ;
                    if (mainProj != null && mainProj.currentMode == DragonRageJavPROJ.Mode.Attract)
                    {
                        shouldEnterDash = false;
                        shouldDestroy = false;
                        break;
                    }
                }
            }

            // 如果没有主弹幕存在，则销毁自己
            if (shouldDestroy)
            {
                Projectile.Kill();
                return;
            }

            // 如果主弹幕不再吸引，立刻切换为冲刺阶段
            if (shouldEnterDash && flightStage < 2)
            {
                flightStage = 2;
                flightTimer = 0;
                lockedMousePosition = Main.MouseWorld; // 立刻锁定鼠标位置
                lockedDirection = (lockedMousePosition - Projectile.Center).SafeNormalize(Vector2.UnitY); // ✅ 锁定方向

            }

            // === 2️⃣ 阶段控制 ===

            if (flightStage == 0) // 🟠 初始游荡阶段
            {
                if (flightTimer == 0)
                {
                    // 初始化游荡参数
                    initialRandomVelocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                    orbitShape = new Vector2(Main.rand.NextFloat(13f, 18f) * 16f, Main.rand.NextFloat(10f, 14f) * 16f);
                    orbitSpeed = Main.rand.NextFloat(0.7f, 1.4f);
                    orbitStartAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                }

                // === 1️⃣ 引力吸附（缓慢趋向玩家） ===
                Vector2 toPlayer = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Vector2 attraction = toPlayer * 0.25f;

                // === 2️⃣ 扰动抖动（高频随机变化） ===
                float noiseAngle = Main.GlobalTimeWrappedHourly * 4f + Projectile.whoAmI; // 不同弹幕不同抖动
                Vector2 swirlOffset = new Vector2((float)Math.Cos(noiseAngle), (float)Math.Sin(noiseAngle)) * 1.8f;

                // === 3️⃣ 逐渐减弱的初始漂移（惯性）===
                float inertiaFactor = MathHelper.Lerp(1f, 0f, flightTimer / 180f); // 3秒内慢慢减弱
                Vector2 drift = initialRandomVelocity * inertiaFactor;

                // === 总合速度 ===
                Vector2 finalVelocity = attraction + swirlOffset + drift;

                // 平滑赋值
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, finalVelocity, 0.08f);
            }

            else if (flightStage == -1) // 🌕 插值阶段：平滑移动到轨道起点
            {
                Projectile.velocity = Vector2.Zero;

                orbitLerpProgress += 0.05f;
                orbitLerpProgress = MathHelper.Clamp(orbitLerpProgress, 0f, 1f);

                Projectile.Center = Vector2.Lerp(Projectile.Center, orbitStartPosition, orbitLerpProgress);

                if (orbitLerpProgress >= 1f)
                {
                    flightStage = 1;
                    flightTimer = 0;
                    shaderDisableTimer = 60; // 前X帧关闭 Shader 拖尾
                }
            }

            else if (flightStage == 1) // 🌀 轨道旋转阶段
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation += 0.05f;

                // 计算当前旋转角
                float orbitAngle = orbitStartAngle + flightTimer * 0.05f * orbitSpeed;
                Vector2 offset = new Vector2(
                    (float)Math.Cos(orbitAngle) * orbitShape.X,
                    (float)Math.Sin(orbitAngle) * orbitShape.Y
                );
                Projectile.Center = FindDragonRageJavPROJCenter() + offset;

                // 每帧维持生命周期，防止消亡
                Projectile.timeLeft = 90;

                // 达到旋转时长后切入冲刺
                if (flightTimer >= rotationDuration)
                {
                    flightStage = 2;
                    flightTimer = 0;
                    lockedMousePosition = Main.MouseWorld; // 冲刺目标：鼠标锁定位置
                }
            }

            else if (flightStage == 2)
            {
                float baseSpeed = 3f;
                float exponentialGrowth = MathF.Pow(1.05f, flightTimer);
                float finalSpeed = MathHelper.Clamp(baseSpeed * exponentialGrowth, 0f, 48f);

                Projectile.velocity = lockedDirection * finalSpeed;

                if (Projectile.velocity.LengthSquared() > 0.1f)
                {
                    float targetRotation = Projectile.velocity.ToRotation();
                    Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.15f);
                }

                Projectile.timeLeft = 90;
            }


            // === 3️⃣ 特效生成 ===

            if (flightStage == 0 && flightTimer >= 120) // 🧭 开始插值进入轨道
            {
                flightStage = -1;
                flightTimer = 0;
                orbitLerpProgress = 0f;

                float orbitAngle = orbitStartAngle;
                Vector2 offset = new Vector2(
                    (float)Math.Cos(orbitAngle) * orbitShape.X,
                    (float)Math.Sin(orbitAngle) * orbitShape.Y
                );
                orbitStartPosition = FindDragonRageJavPROJCenter() + offset;
            }

            // 每5帧生成一次火花
            if (Time % 5 == 0)
            {
                float sparkAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + new Vector2(16f, 0f).RotatedBy(sparkAngle);
                Vector2 sparkVel = (sparkPos - Projectile.Center).SafeNormalize(Vector2.UnitX) * 4f;

                CritSpark spark = new CritSpark(
                    sparkPos,
                    sparkVel,
                    Color.OrangeRed,
                    Color.DarkOrange,
                    1.2f,
                    20
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // === 4️⃣ 计时器递增与 Shader 控制 ===

            if (flightStage != -1) // 插值阶段不计时
                flightTimer++;

            Time++;

            if (shaderDisableTimer > 0)
                shaderDisableTimer--;
        }





        // **进入冲刺模式**
        private void EnterDashMode(Vector2 dashTarget)
        {
            if (isTracking)
            {
                isTracking = false;
                isDashing = true;
                targetPosition = dashTarget;
                Projectile.timeLeft = 90; // 进入冲刺后不再重置
            }
        }







        public Color TrailColor(float completionRatio)
        {
            float opacity = Utils.GetLerpValue(1f, 0.6f, completionRatio, true) * Projectile.Opacity;
            return Color.Lerp(Color.OrangeRed, Color.Orange, 0.4f) * opacity;
        }

        public override void SetStaticDefaults()
        {
            //ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = true;

            // 确保 oldPos 正常记录（用于拖尾）
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        private int shaderDisableTimer = 0; // Shader渲染关闭计时器（单位帧）

        public override bool PreDraw(ref Color lightColor)
        {
            //Main.spriteBatch.SetBlendState(BlendState.Additive);

            //Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            //Vector2 drawPosition = Projectile.Center - Main.screenPosition;




            // 原有烟雾主贴图绘制
            //float randomRotation = Projectile.rotation + Main.rand.NextFloat(-0.35f, 0.35f);
            //float opacity = Projectile.Opacity * 0.6f;
            //Color drawColor = FireColor * opacity;

            //Main.EntitySpriteDraw(texture, drawPosition, null, drawColor, randomRotation, texture.Size() / 2, MaxScale, SpriteEffects.None);



            //// ✅ 新增光球效果
            //Texture2D star = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_09").Value;
            //Texture2D ring = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;

            //float time = (float)Main.GlobalTimeWrappedHourly;
            //float easingPulse = 1f + 0.12f * (float)Math.Sin(time * MathHelper.TwoPi);
            //float baseScale = 0.25f;
            //float scale = baseScale * easingPulse;

            //float spin = time * 3.6f;

            //Color baseColor = Color.Orange * 0.9f; // 或者指定 A 值
            //Vector2 origin = star.Size() * 0.5f;

            //// 绘制准星核心
            //Main.EntitySpriteDraw(star, drawPosition, null, baseColor, spin, origin, scale, SpriteEffects.None);

            //// 绘制 twirl 环圈
            //for (int i = 0; i < 2; i++)
            //{
            //    float angle = spin * (i == 0 ? 1.8f : -1.2f);
            //    float ringScale = scale * (i == 0 ? 1.3f : 0.9f);
            //    Color ringColor = (i == 0 ? Color.OrangeRed : Color.White) * 0.6f;
            //    // 不要再设置 .A = 0

            //    Main.EntitySpriteDraw(ring, drawPosition, null, ringColor, angle, ring.Size() * 0.5f, ringScale, SpriteEffects.None);
            //}


            //Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);




            Texture2D star = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_09").Value;
            Texture2D ring = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;

            float fixedRotation = Projectile.rotation;
            Vector2 gunTip = Projectile.Center + new Vector2(16f * 0f, 0).RotatedBy(fixedRotation);
            Vector2 screenPos = gunTip - Main.screenPosition;

            // 自转角度（匀速）
            float rotation = Main.GlobalTimeWrappedHourly * 3.2f;

            // 仿 iOS 动画节奏的脉动效果（慢-快-慢）
            float easingPulse = 1f + 0.12f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);

            // ✅ 整体缩放为原来的 30%
            float baseScale = 0.2f;
            float scale = baseScale * easingPulse;

            Color baseColor = Color.Orange with { A = 0 };
            SpriteEffects flip = SpriteEffects.None;
            Vector2 origin = star.Size() * 0.5f;

            // 准星本体绘制
            Main.EntitySpriteDraw(star, screenPos, null, baseColor, rotation, origin, scale, flip, 0);

            // 外层 twirl 两圈
            for (int i = 0; i < 2; i++)
            {
                float offsetAngle = rotation * (i == 0 ? 1.8f : -1.2f); // 一圈正转，一圈反转
                float ringScale = scale * (i == 0 ? 0.8f : 0.7f);
                Color ringColor = (i == 0 ? Color.OrangeRed : Color.White) * 0.6f;
                ringColor.A = 0;

                Main.EntitySpriteDraw(ring, screenPos, null, ringColor, offsetAngle, ring.Size() * 0.5f, ringScale, flip, 0);
            }





            if (flightStage >= 1) // 只有旋转阶段和冲刺阶段才绘制 Shader
            {
                Main.spriteBatch.EnterShaderRegion();

                GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                    .SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/scorch_03"))
                    .UseColor(new Color(255, 100, 20))           // 橘红主色
                    .UseSecondaryColor(new Color(255, 180, 100)) // 浅橘副色
                    .Apply();

                PrimitiveRenderer.RenderTrail(
                    Projectile.oldPos,
                    new(
                        ratio => MathHelper.SmoothStep(12f, 2f, ratio),
                        TrailColor,
                        (_) => Projectile.Size * 0.5f,
                        shader: GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                    ),
                    30
                );

                Main.spriteBatch.ExitShaderRegion();
            }




            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 龙焰
        }
    }
}
