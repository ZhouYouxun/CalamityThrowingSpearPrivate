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
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd
{
    public class ElectrocutionHalberdPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/ElectrocutionHalberd/ElectrocutionHalberdJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        internal Color ColorFunction(float completionRatio)
        {
            // 计算末端的淡化效果
            float fadeToEnd = MathHelper.Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

            // 控制拖尾的不透明度，越接近末尾越透明
            float fadeOpacity = Utils.GetLerpValue(1f, 0.64f, completionRatio, true) * Projectile.Opacity;

            // 拖尾颜色以 HSL 渐变
            Color colorHue = Main.hslToRgb(0.1f, 1, 0.8f); // 色相设置为金色

            // 动态颜色效果
            Color endColor = Color.Lerp(colorHue, Color.PaleTurquoise, (float)Math.Sin(completionRatio * MathHelper.Pi * 1.6f - Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);

            return Color.Lerp(Color.White, endColor, fadeToEnd) * fadeOpacity;
        }

        internal float WidthFunction(float completionRatio)
        {
            // 拖尾宽度随位置衰减，越靠近末端越窄
            float expansionCompletion = (float)Math.Pow(1 - completionRatio, 3); // 位置越远，衰减越快
            return MathHelper.Lerp(0f, 22 * Projectile.scale * Projectile.Opacity, expansionCompletion);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理资源和位置
            Texture2D textureGlow = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 originGlow = textureGlow.Size() * 0.5f;
            Vector2 drawPositionGlow = Projectile.Center - Main.screenPosition;

            // 计算蓄力比例，范围为 0~1
            float chargeProgress = chargeTimer / 60f;

            // 蓝色光晕，根据 chargeProgress 动态变化
            float chargeOffset = 3f * chargeProgress; // 光晕的偏移量随蓄力增长
            Color chargeColorBlue = Color.Blue * chargeProgress; // 蓝色光晕强度
            chargeColorBlue.A = 0;

            // 渲染动态蓝色光晕
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(textureGlow, drawPositionGlow + drawOffset, null, chargeColorBlue, Projectile.rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);
            }

            // 渲染实际的投射物本体
            Main.EntitySpriteDraw(textureGlow, drawPositionGlow, null, Projectile.GetAlpha(lightColor), Projectile.rotation, originGlow, Projectile.scale, SpriteEffects.None, 0f);

            // 冲刺阶段启用拖尾特效
            if (CurrentState == BehaviorState.Dash)
            {
                GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 30);
            }

            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.alpha = 1;
        }
        private bool hasHitTarget = false; // 标志位，记录是否已经击中目标

        public enum BehaviorState
        {
            Aim,
            Dash
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public Player Owner => Main.player[Projectile.owner];
        private float chargeTimer; // 蓄力计时器
        private int soundTimer = 0;

        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Dash:
                    DoBehavior_Dash();
                    break;
            }
        }
        private bool soundPlayed = false; // 确保音效只播放一次

        private void DoBehavior_Aim()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.ToRadians(25);
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;


            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 对齐到玩家中心
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            //Projectile.Center = Owner.Center;
            Owner.heldProj = Projectile.whoAmI;

            // 枪头的位置
            Vector2 HeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);


            // 计时器递增，限制最大值为60
            chargeTimer = MathHelper.Clamp(chargeTimer + 1, 0, 60);
            // 生成粒子和音效（达到最大蓄力时）
            if (chargeTimer == 60)
            {
                // 确保音效只播放一次
                if (!soundPlayed)
                {
                    SoundEngine.PlaySound(SoundID.Item91, Projectile.Center); // 播放音效
                    soundPlayed = true; // 标记音效已播放
                }

                // 每帧生成蓝色粒子特效
                Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 1.25f);
                Color startColor = Color.Blue;
                Color endColor = Color.LightBlue;

                // 获取鼠标位置方向
                Vector2 targetDirection = Main.MouseWorld - Projectile.Center;
                targetDirection.Normalize(); // 归一化为方向向量

                // 粒子生成逻辑
                for (int i = 0; i < 1; i++) // 每帧生成1个粒子
                {
                    // 在目标方向左右各扩散10度的范围内随机选取一个角度
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f));
                    Vector2 velocity = targetDirection.RotatedBy(angleOffset); // 旋转角度

                    // 设置粒子的速度（调整此值控制速度范围）
                    float speed = Main.rand.NextFloat(8f, 12f); // 速度范围8f~12f
                    Vector2 particleVelocity = velocity * speed;

                    // 创建粒子
                    CritSpark spark = new CritSpark(
                        headPosition,
                        particleVelocity, // 使用计算后的速度
                        startColor,
                        endColor,
                        Main.rand.NextFloat(1f, 1.5f), // 随机放大粒子
                        Main.rand.Next(15, 25) // 粒子寿命
                    );

                    // 生成粒子
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // === 🌌 蓄力期间自动播放越来越尖锐的音效 ===
            soundTimer++;
            if (soundTimer > 8) // 每 8 帧播放一次，可调整
            {
                float chargeTime3 = Projectile.localAI[1];
                float progress = MathHelper.Clamp(chargeTime3 / 300f, 0f, 1f); // 0~1, 超过300后锁定1
                float pitch = MathHelper.Lerp(-0.5f, 0.4f, progress); // 音调从低到高

                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/拉链闪电") with { Pitch = pitch, Volume = 1.7f }, Projectile.Center);
    
                soundTimer = 0;
            }

            // 检测松手
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.timeLeft = 300; // 冲刺阶段持续时间
                Projectile.penetrate = 1; // 设置冲刺阶段的穿透次数

                // 根据蓄力时间调整伤害倍率
                float damageMultiplier = MathHelper.Lerp(0.58f, 1f, chargeTimer / 60f);
                Projectile.damage = (int)(Projectile.damage * damageMultiplier);

                CurrentState = BehaviorState.Dash;
            }
        }
        // 放在类字段区域
        private List<SparkParticle> ownedSilverSparks = new();

        private void DoBehavior_Dash()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.ToRadians(25);
            Projectile.tileCollide = true;

            // 重置速度的逻辑
            {
                float initialSpeed = 25f; // 设定初始速度值，可根据需求替换具体值
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            }

            {
                // ⚡ 高阶冲刺飞行特效（红尖刺 + 银 Spark + 蓝 Electric Dust）

                // === 🔴 红色尖刺（李萨如曲线 + 矩阵旋转扰动） ===
                float t = Main.GameUpdateCount * 0.25f;
                float lissX = (float)Math.Sin(3 * t);
                float lissY = (float)Math.Sin(2 * t + MathHelper.PiOver4);
                Vector2 lissOffset = new Vector2(lissX, lissY) * 12f;
                Vector2 redVelocity = (Projectile.velocity.SafeNormalize(Vector2.UnitY) * 0.8f).RotatedBy(Math.Sin(t) * 0.3f);
                PointParticle redSpark = new PointParticle(Projectile.Center + lissOffset, redVelocity, false, 20, 1.3f, Color.Red);
                GeneralParticleHandler.SpawnParticle(redSpark);

                // === ⚪ 银色 SparkParticle（黄金角螺旋散射） ===
                float goldenAngle = MathHelper.ToRadians(137.5f);
                int sparkCount = 3;
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = i * goldenAngle + Main.GameUpdateCount * 0.05f;
                    Vector2 sparkVelocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                    //SparkParticle silverSpark = new SparkParticle(
                    //    Projectile.Center,
                    //    sparkVelocity,
                    //    false,
                    //    20,
                    //    1.0f,
                    //    Color.Silver
                    //);
                    //GeneralParticleHandler.SpawnParticle(silverSpark);
                    SparkParticle silverSpark = new SparkParticle(
                        Projectile.Center,
                        sparkVelocity,
                        false,
                        20,
                        1.0f,
                        Color.Silver
                    );
                    GeneralParticleHandler.SpawnParticle(silverSpark);
                    ownedSilverSparks.Add(silverSpark);

                }

                // === 🔵 蓝色 Electric Dust（阿基米德双螺旋，稳定缓慢扩散） ===
                float spiralSpeed = 0.12f; // 螺旋增长速度（影响旋转速度）
                float maxTheta = MathHelper.TwoPi * 6f; // 最大旋转角度（6圈）
                float spiralT = (Main.GameUpdateCount * spiralSpeed) % maxTheta;

                // 阿基米德螺旋参数
                float a = 1.4f * 16f;          // 起始半径
                float b = 0.08f;       // 每弧度增长率（小，避免飞太远）

                for (int i = 0; i < 10; i++)
                {
                    float phaseOffset = MathHelper.TwoPi * i / 4f; // X等分相位

                    // === 正向螺旋 ===
                    float theta1 = spiralT + phaseOffset;
                    float r1 = a + b * theta1;
                    Vector2 spiralOffset1 = theta1.ToRotationVector2() * r1;

                    Dust electricDust1 = Dust.NewDustPerfect(
                        Projectile.Center + spiralOffset1,
                        DustID.Electric,
                        spiralOffset1 * 0.2f, // 慢速飘动
                        100,
                        Color.Blue,
                        0.6f
                    );
                    electricDust1.noGravity = true;

                    // === 反向螺旋（交替方向） ===
                    float theta2 = -spiralT + phaseOffset;
                    float r2 = a + b * Math.Abs(theta2);
                    Vector2 spiralOffset2 = theta2.ToRotationVector2() * r2;

                    Dust electricDust2 = Dust.NewDustPerfect(
                        Projectile.Center + spiralOffset2,
                        DustID.Electric,
                        spiralOffset2 * 0.2f,
                        100,
                        Color.Red,
                        0.4f
                    );
                    electricDust2.noGravity = true;
                }


            }


            {
                // === 🌀 SparkParticle 蛇形步轨迹修正 ===
                for (int i = ownedSilverSparks.Count - 1; i >= 0; i--)
                {
                    SparkParticle p = ownedSilverSparks[i];

                    // 粒子超时后移除引用避免持久保留
                    if (p.Time >= p.Lifetime)
                    {
                        ownedSilverSparks.RemoveAt(i);
                        continue;
                    }

                    // 计算蛇形步旋转
                    int cycle = 10; // 5 左 + 5 右 = 10 帧循环
                    int phase = p.Time % cycle;

                    float angleOffset = MathHelper.ToRadians(3f); // 每帧 3°
                    if (phase < 5)
                    {
                        // 前 5 帧左转
                        p.Velocity = p.Velocity.RotatedBy(-angleOffset);
                    }
                    else
                    {
                        // 后 5 帧右转
                        p.Velocity = p.Velocity.RotatedBy(angleOffset);
                    }
                }

            }


            // 弹幕保持旋转并持续加速
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.ToRadians(25);
            Projectile.velocity *= 1.01f;

            // 添加X色光源
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.55f);        
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果

            //if (!hasHitTarget)
            //{
            //    hasHitTarget = true; // 设置已击中标志位
            //    Projectile.friendly = false; // 关闭弹幕的伤害渠道
            //}

            // 在当前位置生成 ElectrocutionHalberdField
            if (Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<ElectrocutionHalberdField>()) >= 2)
            {
                // 删除场上的一个现有 ElectrocutionHalberdField
                var existingFields = Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<ElectrocutionHalberdField>()).ToList();
                if (existingFields.Count > 0)
                {
                    int randomIndex = Main.rand.Next(existingFields.Count);
                    existingFields[randomIndex].Kill();
                }
            }

            // 生成新的 ElectrocutionHalberdField
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero, // 初始速度为0
                ModContent.ProjectileType<ElectrocutionHalberdField>(),
                (int)(Projectile.damage * 1.5f), // 伤害倍率为2.0倍
                Projectile.knockBack,
                Projectile.owner
            );

            // 保留 CrackParticle 效果
            for (int i = 0; i < 4; i++)
            {
                float randomAngle = Projectile.velocity.ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-60f, 60f));
                float randomSpeed = Main.rand.NextFloat(5f, 8f);
                float randomScale = Main.rand.NextFloat(0.6f, 1.1f);
                Vector2 particleVelocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * randomSpeed;

                Particle bolt = new CrackParticle(
                    Projectile.Center,
                    particleVelocity,
                    Color.Aqua * 0.65f,
                    Vector2.One * randomScale,
                    0,
                    0,
                    randomScale,
                    11
                );
                GeneralParticleHandler.SpawnParticle(bolt);
            }
        }


        public override void OnKill(int timeLeft)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item94, Projectile.position);


            // 第一圈（小圈） - 以自身为原点
            int smallCircleCount = Main.rand.Next(5, 9); // 随机生成5~8个粒子
            for (int i = 0; i < smallCircleCount; i++)
            {
                // 随机生成角度
                float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(360f));

                // 随机生成大小和速度
                float randomSpeed = Main.rand.NextFloat(2f, 5f); // 小圈速度稍低
                float randomScale = Main.rand.NextFloat(0.8f, 1.5f);

                // 设置粒子的速度方向
                Vector2 particleVelocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * randomSpeed;

                // 创建并生成粒子
                Particle smallCircleParticle = new CrackParticle(
                    Projectile.Center,
                    particleVelocity,
                    Color.Aqua * 0.65f,
                    Vector2.One * randomScale,
                    0,
                    0,
                    randomScale,
                    11
                );
                GeneralParticleHandler.SpawnParticle(smallCircleParticle);
            }

            // 第二圈（大圈） - 半径为2*16的圆环
            int largeCircleCount = Main.rand.Next(15, 19); // 随机生成15~18个粒子
            float circleRadius = 2 * 16; // 圆环半径

            for (int i = 0; i < largeCircleCount; i++)
            {
                // 随机生成角度
                float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(360f));

                // 计算粒子的初始位置（圆环上的随机点）
                Vector2 initialPosition = Projectile.Center + new Vector2(
                    (float)Math.Cos(randomAngle),
                    (float)Math.Sin(randomAngle)
                ) * circleRadius;

                // 随机生成大小和速度
                float randomSpeed = Main.rand.NextFloat(4f, 6f); // 大圈速度稍高
                float randomScale = Main.rand.NextFloat(1.2f, 1.55f);

                // 设置粒子的速度方向
                Vector2 particleVelocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * randomSpeed;

                // 创建并生成粒子
                Particle largeCircleParticle = new CrackParticle(
                    initialPosition,
                    particleVelocity,
                    Color.Aqua * 0.65f,
                    Vector2.One * randomScale,
                    0,
                    0,
                    randomScale,
                    11
                );
                GeneralParticleHandler.SpawnParticle(largeCircleParticle);
            }
        }

        public override bool? CanDamage()
        {
            // 如果是正常世界，那么蓄力状态下不造成伤害
            if (CurrentState == BehaviorState.Aim)
            {
                return false;
            }

            // 如果当前状态是冲刺状态，允许造成伤害
            return true;
        }


    }
}
