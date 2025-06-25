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
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Sounds;
using Terraria.Audio;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Buffs.DamageOverTime;
using Terraria.DataStructures;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC
{
    public class DragonRageJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        private int hitCounter = 0;
        public int Time = 0;

        public enum Mode
        {
            Return,  // 回归模式
            Charge,  // 冲刺模式
            Attract  // 吸引模式
        }

        public Mode currentMode = Mode.Return;
        private int attractEffectTimer = 0; // 用于吸引模式光学效果过渡

        public void SetMode(Mode mode)
        {
            if (currentMode != mode)
            {
                currentMode = mode;

                // 设置目标旋转速度（不同模式不同目标）
                if (mode == Mode.Attract)
                {
                    targetRotationSpeed = -0.15f; // 吸引模式，反向旋转
                    attractEffectTimer = 0;

                    // ✅ 初始化吸引动画位置插值
                    attractStartPosition = Projectile.Center;
                    attractLerpProgress = 0f;
                }

                else
                {
                    targetRotationSpeed = 0.25f; // 非吸引模式，顺时针旋转
                }

            }
        }
        public Color TrailColor(float completionRatio)
        {
            float opacity = Utils.GetLerpValue(1f, 0.6f, completionRatio, true) * Projectile.Opacity;
            return Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f) * opacity;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // 原始绘制
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            //// 吸引模式额外绘制光学拖尾
            //if (currentMode == Mode.Attract)
            //{
            //    Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/ChangedWeapons/EAfterDog/DragonRageC/DragonRageJavPROJ").Value;
            //    Color[] colors = { Color.OrangeRed, Color.Red, Color.DarkOrange };
            //    float timeFactor = Main.GlobalTimeWrappedHourly * 3f;
            //    int colorIndex = (int)(timeFactor % colors.Length);
            //    Color currentColor = Color.Lerp(colors[colorIndex], colors[(colorIndex + 1) % colors.Length], timeFactor % 1f);

            //    float glowAlpha = MathHelper.Lerp(0.3f, 1f, attractEffectTimer / 20f);
            //    Color glowColor = currentColor * glowAlpha;

            //    for (int i = 0; i < Projectile.oldPos.Length; i++)
            //    {
            //        float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;
            //        Color color = Color.Lerp(currentColor, colors[(colorIndex + 2) % colors.Length], colorInterpolation) * glowAlpha;

            //        Vector2 trailPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            //        Vector2 fixedScale = new Vector2(1.5f);

            //        Main.EntitySpriteDraw(lightTexture, trailPosition, null, color, Projectile.rotation, lightTexture.Size() * 0.5f, fixedScale, SpriteEffects.None, 0);
            //        Main.EntitySpriteDraw(lightTexture, trailPosition, null, color * 0.7f, Projectile.rotation, lightTexture.Size() * 0.5f, fixedScale * 0.7f, SpriteEffects.None, 0);
            //    }
            //}


            // 逐渐增强的橙色光晕（仅在 Attract 模式下生效）
            if (currentMode == Mode.Attract)
            {
                float glowAlpha = MathHelper.Lerp(0.3f, 1f, attractEffectTimer / 120f);
                Color glowColor = Color.Orange * glowAlpha;
                glowColor.A = 0;

                float glowOffset = 3f;

                // 绘制光晕
                for (int i = 0; i < 8; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * glowOffset;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, glowColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }


            if (currentMode == Mode.Attract)
            {
                Main.spriteBatch.EnterShaderRegion();

                GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                    .SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/window_04"))
                    .UseColor(new Color(255, 100, 20))
                    .UseSecondaryColor(new Color(255, 180, 100))
                    .Apply();

                // 构造围绕玩家公转的“枪头轨迹”
                Vector2[] gunTipTrail = new Vector2[Projectile.oldPos.Length];
                Player player = Main.player[Projectile.owner];

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    float angle = Projectile.oldRot[i]; // 过去每帧的 rotation
                    Vector2 offset = new Vector2(16f * 12f, 0).RotatedBy(angle); // 枪头距离 + 当前旋转角
                    gunTipTrail[i] = player.Center + offset;
                }

                PrimitiveRenderer.RenderTrail(
                    gunTipTrail,
                    new(
                        ratio => MathHelper.SmoothStep(12f, 2f, ratio),
                        TrailColor,
                        (_) => Projectile.Size * 0.5f,
                        shader: GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                    ),
                    10
                );

                Main.spriteBatch.ExitShaderRegion();
            }


            // 禁用了，禁用原因是这个东西太大太突兀，不适合，建议找一个更加简单的
            //if (currentMode is Mode.Charge or Mode.Attract)
            //{
            //    Texture2D star = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_09").Value;
            //    Texture2D ring = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;

            //    // ❗修正偏移角度：旋转 -40°
            //    float fixedRotation = Projectile.rotation - MathHelper.ToRadians(40f);
            //    Vector2 gunTip = Projectile.Center + new Vector2(16f * 12f, 0).RotatedBy(fixedRotation);
            //    Vector2 screenPos = gunTip - Main.screenPosition;

            //    // 自转角度（匀速）
            //    float rotation = Main.GlobalTimeWrappedHourly * 3.2f;

            //    // 仿 iOS 动画节奏的脉动效果（慢-快-慢）
            //    float easingPulse = 1f + 0.12f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);

            //    // ✅ 整体缩放为原来的 30%
            //    float baseScale = 0.3f;
            //    float scale = baseScale * easingPulse;

            //    Color baseColor = Color.Orange with { A = 0 };
            //    SpriteEffects flip = SpriteEffects.None;
            //    Vector2 origin = star.Size() * 0.5f;

            //    // 准星本体绘制
            //    Main.EntitySpriteDraw(star, screenPos, null, baseColor, rotation, origin, scale, flip, 0);

            //    // 外层 twirl 两圈
            //    for (int i = 0; i < 2; i++)
            //    {
            //        float offsetAngle = rotation * (i == 0 ? 1.8f : -1.2f); // 一圈正转，一圈反转
            //        float ringScale = scale * (i == 0 ? 1.3f : 0.9f);
            //        Color ringColor = (i == 0 ? Color.OrangeRed : Color.White) * 0.6f;
            //        ringColor.A = 0;

            //        Main.EntitySpriteDraw(ring, screenPos, null, ringColor, offsetAngle, ring.Size() * 0.5f, ringScale, flip, 0);
            //    }
            //}

            if (currentMode is Mode.Charge or Mode.Attract)
            {
                Texture2D halfStar = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;

                // 修正偏移角度：旋转 -40°
                float fixedRotation = Projectile.rotation - MathHelper.ToRadians(40f);
                Vector2 gunTip = Projectile.Center + new Vector2(16f * 12f, 0).RotatedBy(fixedRotation);
                Vector2 screenPos = gunTip - Main.screenPosition;

                // 自转角度（匀速）
                float rotation = Main.GlobalTimeWrappedHourly * 2.6f;

                // 脉动效果：慢-快-慢节奏
                float easingPulse = 1f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi);
                float baseScale = 3.35f;
                float scale = baseScale * easingPulse;

                SpriteEffects flip = SpriteEffects.None;
                Vector2 origin = halfStar.Size() * 0.5f;

                // 第一层：主图
                Color baseColor = new Color(255, 120, 40, 0); // 稍暗橙色
                Main.EntitySpriteDraw(halfStar, screenPos, null, baseColor, rotation, origin, scale, flip, 0);

                // 第二层：叠加亮色层
                Color glowColor = new Color(255, 180, 80, 0) * 0.6f;
                Main.EntitySpriteDraw(halfStar, screenPos, null, glowColor, -rotation * 1.5f, origin, scale * 0.9f, flip, 0);

                // 第三层：白色高光层
                Color highlight = Color.White with { A = 0 } * 0.4f;
                Main.EntitySpriteDraw(halfStar, screenPos, null, highlight, rotation * 0.5f, origin, scale * 0.65f, flip, 0);
            }



            return false; // 允许默认绘制
        }

        //public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        //{
        //    // 将弹幕绘制在玩家上方（覆盖玩家）
        //    overPlayers.Add(index);
        //}


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 420;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        private int nextFireTime = 0; // 记录下一次生成火焰的时间
        private float smearRotationTracker = 0f; // 记录当前刀盘特效的旋转角度
        private int smearCounter = 0; // 记录特效释放次数

        private float currentRotationSpeed = 0.25f; // 当前实际旋转速度（初始正向）
        private float targetRotationSpeed = 0.25f;  // 目标旋转速度

        private Vector2 attractStartPosition; // 吸引模式启动时的位置
        private float attractLerpProgress = 0f; // 插值进度（0 → 1）


        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 保持飞行方向不变
            //Projectile.velocity *= 1.006f;

            // 添加橙色光效
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            Projectile.timeLeft = 120; // 不断的刷新剩余时间，让它不要消失

            // 旋转长枪时，调整玩家手臂姿态
            ManipulatePlayerArmPositions();

            // 粒子效果随机化释放
            //if (Time % 3 == 0)
            //{
            //    Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
            //    particleOffset.X += Main.rand.NextFloat(-3f, 3f); // 随机左右偏移
            //    Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;

            //    // 应用 2.1 倍缩放到光环特效
            //    float scaleMultiplier = 2.1f;
            //    Particle Smear = new CircularSmearVFX(
            //        particlePosition,
            //        Color.OrangeRed * Main.rand.NextFloat(0.78f, 0.85f),
            //        Main.rand.NextFloat(-8, 8),
            //        Main.rand.NextFloat(1.2f, 1.3f) * scaleMultiplier // 应用缩放
            //    );
            //    GeneralParticleHandler.SpawnParticle(Smear);
            //}

            // 刀盘特效的处理
            if (currentMode == Mode.Attract)
            {
                //if (Time % 8 == 0)
                //{
                //    Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                //    particleOffset.X += Main.rand.NextFloat(-3f, 3f); // 随机左右偏移
                //    Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;

                //    // 应用 2.1 倍缩放到光环特效
                //    float scaleMultiplier = 2.1f;

                //    // 🔥 让特效按照 `30°` 递增，而不是完全随机
                //    float fixedRotation = smearRotationTracker;

                //    // **释放刀盘特效**
                //    Particle Smear = new CircularSmearVFX(
                //        particlePosition,
                //        Color.OrangeRed * Main.rand.NextFloat(0.78f, 0.85f),
                //        fixedRotation, // 让特效的旋转角度固定变化
                //        Main.rand.NextFloat(1.2f, 1.3f) * scaleMultiplier // 应用缩放
                //    )
                //    {
                //        Lifetime = 4 // 持续时间改为 X 帧
                //    };

                //    GeneralParticleHandler.SpawnParticle(Smear);

                //    // **每两次释放后，旋转 `30°`**
                //    smearCounter++;
                //    if (smearCounter >= 2)
                //    {
                //        smearRotationTracker += MathHelper.ToRadians(30f); // **固定增加 `30°`**
                //        smearCounter = 0; // 重置计数
                //    }
                //}
            }
            else
            {
                // 其他模式下保持原来的 3 帧特效
                if (Time % 3 == 0)
                {
                    Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                    particleOffset.X += Main.rand.NextFloat(-3f, 3f);
                    Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;

                    float scaleMultiplier = 2.1f;
                    Particle Smear = new CircularSmearVFX(
                        particlePosition,
                        Color.OrangeRed * Main.rand.NextFloat(0.78f, 0.85f),
                        Main.rand.NextFloat(-8, 8),
                        Main.rand.NextFloat(1.2f, 1.3f) * scaleMultiplier
                    );

                    GeneralParticleHandler.SpawnParticle(Smear);
                }
            }


            // 处理不同模式的行为
            switch (currentMode)
            {
                case Mode.Return:
                    Projectile.rotation += currentRotationSpeed;
                    
                    // 计算旋转中心，使弹幕位于玩家前方 16 像素处
                    //Vector2 offsetDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero);
                    //Vector2 newCenter = player.Center + offsetDirection * 16f;
                    //Projectile.velocity = (newCenter - Projectile.Center).SafeNormalize(Vector2.Zero) * 10f;

                    // 旋转中心固定在玩家中心
                    Vector2 newCenter = player.Center;
                    Projectile.velocity = (newCenter - Projectile.Center).SafeNormalize(Vector2.Zero) * 10f;

                    break;

                case Mode.Charge:
                    Projectile.rotation += currentRotationSpeed;
                    // 冲刺模式：朝向鼠标移动，远快近慢
                    Vector2 targetPos = Main.MouseWorld;
                    float distance = Vector2.Distance(Projectile.Center, targetPos);
                    float speed = MathHelper.Lerp(3f, 20f, distance / 500f);
                    Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;
                    break;


                case Mode.Attract:
                    Projectile.rotation += currentRotationSpeed;

                    // 吸引模式：从周围制造吸收向玩家并旋转的龙火弹幕
                    //Vector2 attractOffset = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero);
                    //Projectile.Center = player.Center + attractOffset * 16f;
                    //Projectile.Center = player.Center;

                    // 吸引模式：平滑插值靠近玩家中心
                    if (attractLerpProgress < 1f)
                    {
                        attractLerpProgress += 0.05f; // 插值速度（越大越快）
                        attractLerpProgress = MathHelper.Clamp(attractLerpProgress, 0f, 1f);
                        Projectile.Center = Vector2.Lerp(attractStartPosition, player.Center, attractLerpProgress);
                    }
                    else
                    {
                        Projectile.Center = player.Center; // 最终吸附
                    }


                    // 生成火焰弹幕，每隔 8~15 帧
                    if (Time >= nextFireTime)
                    {
                        nextFireTime = Time + Main.rand.Next(8, 16); // 设定下次生成时间

                        // 生成随机角度（围绕玩家）
                        float randomAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);

                        // 生成随机半径 (50~70 方块)
                        float randomDistance = Main.rand.NextFloat(50f, 70f) * 16f;

                        // 计算生成位置
                        Vector2 spawnOffset = new Vector2(randomDistance, 0).RotatedBy(randomAngle);
                        Vector2 spawnPos = player.Center + spawnOffset;

                        // 计算初始方向（朝向玩家），并随机旋转 ±10°
                        Vector2 initialVelocity = (player.Center - spawnPos).SafeNormalize(Vector2.Zero) * 3.5f;
                        Vector2 newVel = initialVelocity.RotatedByRandom(MathHelper.ToRadians(10f));

                        // 生成龙火弹幕
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, newVel,
                            ModContent.ProjectileType<DragonRageJavFireBall>(), (int)(Projectile.damage * 0.8f), Projectile.knockBack, Projectile.owner);
                    }

                    // 获取枪头和枪尾的位置（随机化）
                    Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f * 16f + Main.rand.NextVector2Circular(2 * 16f, 2 * 16f);
                    Vector2 gunTail = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f * 16f + Main.rand.NextVector2Circular(2 * 16f, 2 * 16f);

                    // **直接基于 `Projectile.rotation` 计算枪头和枪尾**
                    gunTip = Projectile.Center +
                        new Vector2(12f * 16f, 0).RotatedBy(Projectile.rotation) + Main.rand.NextVector2Circular(2 * 16f, 2 * 16f);

                    gunTail = Projectile.Center -
                        new Vector2(12f * 16f, 0).RotatedBy(Projectile.rotation) + Main.rand.NextVector2Circular(2 * 16f, 2 * 16f);


                    //// **🔥 在枪头和枪尾生成火焰粒子，每帧 `3` 个**
                    //for (int i = 0; i < 3; i++)
                    //{
                    //    Vector2 particlePos = (i % 2 == 0) ? gunTip : gunTail;

                    //    // **火焰粒子**
                    //    int dustType = Main.rand.Next(new int[] { 35, 174, 158 }); // 随机选一个 Dust
                    //    Dust dust = Dust.NewDustPerfect(particlePos, dustType, Vector2.Zero, 100, Color.OrangeRed, Main.rand.NextFloat(1.5f, 1.75f));
                    //    dust.noGravity = true;

                    //    // **设置火焰的初始速度为辐射状向外**
                    //    dust.velocity = (dust.position - Projectile.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);
                    //}

                    //// **🌀 在枪头和枪尾生成烟雾，每帧 `4` 个**
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    Vector2 smokePos = (i % 2 == 0) ? gunTip : gunTail;

                    //    // **烟雾不会移动，留在原地**
                    //    Particle smoke = new HeavySmokeParticle(
                    //        smokePos,
                    //        Vector2.Zero, // **不具备初始速度**
                    //        Color.Orange, // 改成橙色烟雾
                    //        18, // 生命周期
                    //        Main.rand.NextFloat(0.9f, 1.6f), // 缩放
                    //        0.35f, // 透明度
                    //        Main.rand.NextFloat(-1, 1), // 旋转速度
                    //        false // 关闭发光
                    //    );
                    //    GeneralParticleHandler.SpawnParticle(smoke);
                    //}

                    // 处理光学效果渐变
                    if (attractEffectTimer < 120)
                        attractEffectTimer++;
                    break;
            }

            // 平滑插值旋转速度，使其缓慢趋近目标值
            currentRotationSpeed = MathHelper.Lerp(currentRotationSpeed, targetRotationSpeed, 0.05f); // 趋近速度越小越慢

            Time++;
        }

        public void ManipulatePlayerArmPositions()
        {
            Player Owner = Main.player[Projectile.owner];

            // 计算手臂应指向的角度，使其始终指向鼠标位置
            Vector2 armDirection = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.Zero);
            float armRotation = armDirection.ToRotation() - MathHelper.PiOver2;

            // 让玩家的手臂方向始终朝向鼠标
            Owner.ChangeDir(Main.MouseWorld.X > Owner.Center.X ? 1 : -1);
            Owner.heldProj = Projectile.whoAmI;

            // 设置玩家前臂（主手）和后臂（副手）的角度，使其平行前伸
            // 第1个参数设置为正确，意味着它将会使用自定义手臂，设置为错误，则不进行更改
            // 第2个参数决定了伸手臂的长度：【也就是伸出了多少，并不是指的角度】
            // Full（完全伸展，适用于拿长枪、拉弓等）
            // None（不伸展，手臂保持贴近身体）
            // Quarter（25 % 伸展，适用于轻微举起手臂）
            // ThreeQuarters（75 % 伸展，适用于半握持状态）
            // 第3个参数armRotation决定了手臂的弯曲角度，你要想让他平行向前？高举45度？还是往下放？

            // 设置玩家前臂（主手）和后臂（副手）的角度，使其完全伸展并指向鼠标位置
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);
        }

        public override bool? CanDamage()
        {
            return currentMode == Mode.Attract ? false : base.CanDamage();
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float finalDamageMultiplier = 1f; // 伤害倍率

            switch (currentMode)
            {
                case Mode.Return:
                    finalDamageMultiplier = 1.1f; // 回归模式 1.1 倍伤害
                    break;

                case Mode.Charge:
                    // 计算玩家到弹幕的距离
                    float distanceToPlayer = Vector2.Distance(Main.player[Projectile.owner].Center, Projectile.Center);
                    float distanceInTiles = distanceToPlayer / 16f; // 转换为 "方块单位"

                    if (distanceInTiles > 6f)
                    {
                        float damageReduction = (distanceInTiles - 6f) * 0.01f; // 每增加 1 方块，减少 1% 伤害
                        finalDamageMultiplier = MathHelper.Clamp(1f - damageReduction, 0.7f, 1f); // 最低伤害为 70%
                    }
                    break;

                case Mode.Attract:
                    finalDamageMultiplier = 0.5f; // 吸引模式伤害降低至 0.5 倍
                    break;
            }

            // **应用最终伤害倍率**
            modifiers.FinalDamage *= finalDamageMultiplier;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 龙焰

            switch (currentMode)
            {
                case Mode.Return:
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<DragonRageFuckYou>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
                    break;

                case Mode.Charge:
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<OrangeSLASH>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
                    SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = 0.5f }, Projectile.Center);

                    // 在敌人身上释放橙色火花特效
                    for (int i = 0; i < 18; i++)
                    {
                        int sparkLifetime = Main.rand.Next(22, 36);
                        float sparkScale = Main.rand.NextFloat(0.8f, 1f);
                        Color sparkColor = Color.Lerp(Color.Orange, Color.OrangeRed, Main.rand.NextFloat(0.5f, 1f));

                        if (Main.rand.NextBool(10))
                            sparkScale *= 2f;

                        // **固定方向：向上 + 左右随机扩散**
                        float angleOffset = Main.rand.NextFloat(-25f, 25f);
                        Vector2 sparkVelocity = new Vector2(0, -1).RotatedBy(MathHelper.ToRadians(angleOffset)) * Main.rand.NextFloat(10f, 25f);

                        SparkParticle spark = new SparkParticle(target.Center, sparkVelocity, true, sparkLifetime, sparkScale, sparkColor);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }

                    
                    // 🔥 爆炸性火焰尘土特效
                    for (int i = 0; i < 15; i++)
                    {
                        float speed = Main.rand.NextFloat(6f, 14f);
                        float angle = MathHelper.TwoPi * Main.rand.NextFloat();
                        Vector2 velocity = new Vector2(speed, 0).RotatedBy(angle);

                        int dustType = Main.rand.NextBool() ? 6 : 174; // 火焰 or 狱炎
                        Dust dust = Dust.NewDustPerfect(target.Center, dustType, velocity, 0, Color.Orange, Main.rand.NextFloat(1.2f, 1.8f));
                        dust.noGravity = true;
                    }

                    // 💨 上升烟雾
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(24f, 24f);
                        Vector2 smokeVelocity = new Vector2(0, -1) * Main.rand.NextFloat(0.5f, 2f);
                        Dust smoke = Dust.NewDustPerfect(target.Center + offset, DustID.Smoke, smokeVelocity, 50, Color.DarkOrange, Main.rand.NextFloat(1.4f, 2.2f));
                        smoke.noGravity = true;
                    }

                    // 🔄 烈焰漩涡尘土
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f;
                        float radius = Main.rand.NextFloat(12f, 26f);
                        Vector2 pos = target.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 2.2f; // 切向旋转飞出

                        Dust swirl = Dust.NewDustPerfect(pos, 35, vel, 0, Color.OrangeRed, 1.1f);
                        swirl.noGravity = true;
                    }

                    


                    break;

                case Mode.Attract:
                    // 吸引模式不释放斩杀弹幕
                    break;
            }
        }
     
        public override void OnKill(int timeLeft)
        {
            //// 发射 X 个 DragonRageFireball 弹幕
            //int fireballCount = 12;

            //for (int i = 0; i < fireballCount; i++)
            //{
            //    // 生成一个随机的角度（0 到 360 度之间）
            //    float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);

            //    // 计算每个弹幕的方向向量
            //    Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

            //    // 设定弹幕的速度和伤害
            //    float fireballSpeed = 10f; // 可以调整此值设置弹幕的速度
            //    Vector2 fireballVelocity = direction * fireballSpeed;

            //    // 生成 DragonRageFireball 弹幕，伤害倍率设为 0.1
            //    Projectile.NewProjectile(
            //        Projectile.GetSource_FromThis(),
            //        Projectile.Center,
            //        fireballVelocity,
            //        ModContent.ProjectileType<DragonRageFireball>(),
            //        (int)(Projectile.damage * 0.2f),
            //        Projectile.knockBack,
            //        Projectile.owner
            //    );
            //}

            //SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }


        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item113, Projectile.Center);
        }

    }
}
