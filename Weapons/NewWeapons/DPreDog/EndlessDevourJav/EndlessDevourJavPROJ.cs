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
using CalamityMod.Particles;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Boss;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.EndlessDevourJav
{
    public class EndlessDevourJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/EndlessDevourJav/EndlessDevourJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (CurrentState == BehaviorState.Aim) // 蓄力阶段
            {
                // 绘制 HalfStar 特效
                //Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
                //Vector2 shineScale = new Vector2(1.67f, 3f) * Projectile.scale;
                //shineScale *= MathHelper.Lerp(0.9f, 1.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.4f + Projectile.identity) * 0.5f + 0.5f);

                //Vector2 lensFlareWorldPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 2.95f);
                //Color lensFlareColor = Color.Lerp(Color.White, Color.LightGray, 0.23f) with { A = 0 };

                //// 绘制 HalfStar 特效
                //Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, 0f, shineTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
                //Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, MathHelper.PiOver2, shineTex.Size() * 0.5f, shineScale, 0, 0);

                //// 绘制 Sparkle 特效
                //Texture2D sparkleTex = ModContent.Request<Texture2D>("CalamityMod/Particles/Sparkle").Value;
                //float rotationSpeed = MathHelper.ToRadians(24f); // 每帧旋转的角度
                //float sparkleRadius = Projectile.width * 2.75f; // 半径与尖端位置一致

                //for (int i = 0; i < 5; i++) // 绘制 5 个 Sparkle
                //{
                //    float sparkleAngle = MathHelper.ToRadians(72 * i) + Main.GlobalTimeWrappedHourly * rotationSpeed; // 顺时针旋转
                //    Vector2 sparkleOffset = new Vector2((float)Math.Cos(sparkleAngle), (float)Math.Sin(sparkleAngle)) * sparkleRadius;
                //    Vector2 sparklePosition = lensFlareWorldPosition + sparkleOffset; // 根据角度计算 Sparkle 位置

                //    Main.EntitySpriteDraw(sparkleTex, sparklePosition - Main.screenPosition, null, lensFlareColor, sparkleAngle, sparkleTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
                //}

                // 绘制本体
                Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
                Vector2 origin = frame.Size() * 0.5f;
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
                return false;
            }
            else if (CurrentState == BehaviorState.Dash) // 冲刺阶段
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            }
            return false;
        }


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
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 10;
        }
        private int currentSummonedOrbs = 0; // 当前已生成的 Orbs 数量
        private const int MaxSummonedOrbs = 10; // 最大允许生成数量（可调）
        private int soundTimer = 0;

        private void DoBehavior_Aim()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
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
            Owner.heldProj = Projectile.whoAmI;


            {
                // === 瞄准期间定时在远处召唤弹幕打向自己（数量限制 & 延时启动） ===

                float chargeTime = Projectile.localAI[1];

                if (chargeTime >= 100f && Projectile.localAI[0] % 20f == 0) // 每 20 帧触发
                {
                    if (currentSummonedOrbs < MaxSummonedOrbs) // 未超出生成上限才生成
                    {
                        Vector2 playerCenter = Owner.Center;

                        float spawnRadius = 1200f;
                        float projectileSpeed = 18f;

                        // 动态伤害倍率
                        float minDamageMultiplier = 0.1f;
                        float maxDamageMultiplier = 0.7f;
                        float damageMultiplier = MathHelper.Lerp(
                            minDamageMultiplier,
                            maxDamageMultiplier,
                            MathHelper.Clamp(chargeTime / 300f, 0f, 1f)
                        );

                        Vector2 randomOffset = Main.rand.NextVector2Unit() * spawnRadius;
                        Vector2 spawnPosition = playerCenter + randomOffset;

                        Vector2 targetDirection = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.UnitY);
                        targetDirection = targetDirection.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));
                        Vector2 velocity = targetDirection * projectileSpeed;

                        // 让他在持续期间不要再生成任何弹幕了

                        //int p = Projectile.NewProjectile(
                        //    Projectile.GetSource_FromThis(),
                        //    spawnPosition,
                        //    velocity,
                        //    ModContent.ProjectileType<EndlessDevourJavOrb>(),
                        //    (int)(Projectile.damage * damageMultiplier),
                        //    0f,
                        //    Projectile.owner
                        //);

                        //if (p.WithinBounds(Main.maxProjectiles))
                        //    currentSummonedOrbs++; // 成功生成后计数
                    }
                }



                // === 🌌 蓄力期间自动播放越来越尖锐的音效 ===
                soundTimer++;
                if (soundTimer > 8) // 每 8 帧播放一次，可调整
                {
                    float chargeTime3 = Projectile.localAI[1];
                    float progress = MathHelper.Clamp(chargeTime3 / 300f, 0f, 1f); // 0~1, 超过300后锁定1
                    float pitch = MathHelper.Lerp(-0.5f, 0.4f, progress); // 音调从低到高

                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = pitch, Volume = 0.7f }, Projectile.Center);

                    soundTimer = 0;
                }


                Projectile.localAI[0]++; // 持续步进，保证蓄力与震动正常
            }



            {
                Vector2 HeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 6f + Main.rand.NextVector2Circular(3f, 3f);

                float chargeTime = Projectile.localAI[1];

                if (chargeTime < 300f)
                {
                    // ================= 🌑 蓄力未满 300：播放弱化特效 =================
                    if (Projectile.localAI[0] % 8 == 0)
                    {
                        int dustCount = 12;
                        float radius = 60f;
                        for (int i = 0; i < dustCount; i++)
                        {
                            float angle = MathHelper.TwoPi / dustCount * i + Projectile.localAI[0] * 0.02f;
                            Vector2 offset = angle.ToRotationVector2() * radius;

                            // 🩶 计算枪头正上方位置
                            Vector2 upward = -Vector2.UnitY.RotatedBy(Projectile.rotation);
                            Vector2 pos = HeadPosition + upward * 24f + offset * 0.2f; // 在枪头上方偏移

                            // 🩶 让 Dust 往正上方喷射
                            Vector2 vel = upward * Main.rand.NextFloat(4f, 7f) + offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f);

                            Dust dust = Dust.NewDustPerfect(pos, DustID.Shadowflame, vel);
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(1.5f, 1.8f);
                            dust.fadeIn = 0.3f;
                            dust.alpha = 120;
                            dust.color = Color.DarkViolet;
                        }
                    }

                    // ================= 呼吸感弱烟（枪头正上方持续喷射） =================
                    if (Projectile.localAI[0] % 10 == 0)
                    {
                        Vector2 upward = -Vector2.UnitY.RotatedBy(Projectile.rotation);

                        Particle smoke = new HeavySmokeParticle(
                            HeadPosition + upward * 24f + Main.rand.NextVector2Circular(6f, 6f), // 在枪头上方偏移
                            upward * Main.rand.NextFloat(1f, 2f),                                // 🩶 往上喷射
                            Color.DarkSlateBlue,
                            20,
                            Main.rand.NextFloat(0.2f, 0.4f),
                            0.25f,
                            Main.rand.NextFloat(-0.2f, 0.2f),
                            false
                        );
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                }
                else
                {
                    // ================= 🌌 蓄力到达 300 时瞬间爆发一次强特效 =================
                    if (Projectile.localAI[0] == 300)
                    {
                        int burstCount = 64;          // 🔹 粒子数量
                        float burstRadius = 110f;     // 🔹 爆发环半径
                        float speedMin = 20f;         // 🔹 最低速度
                        float speedMax = 40f;         // 🔹 最高速度

                        Color[] burstColors = new Color[]
                        {
        Color.DarkViolet,
        Color.MediumPurple,
        Color.BlueViolet,
        Color.MediumSlateBlue,
        Color.DeepSkyBlue,
        Color.Cyan
                        };

                        for (int i = 0; i < burstCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / burstCount + Main.rand.NextFloat(-0.02f, 0.02f); // 微扰动避免呆板
                            Vector2 dir = angle.ToRotationVector2();
                            Vector2 spawnPos = HeadPosition + dir * burstRadius;
                            Vector2 vel = dir * Main.rand.NextFloat(speedMin, speedMax);

                            Color burstColor = burstColors[i % burstColors.Length] * 0.9f; // 彩色循环

                            SparkParticle spark = new SparkParticle(
                                spawnPos,
                                vel,
                                false,
                                Main.rand.Next(40, 60),             // 寿命更长
                                Main.rand.NextFloat(1.2f, 2.0f),    // 🚩 更大
                                burstColor
                            );
                            GeneralParticleHandler.SpawnParticle(spark);
                        }

                        // 🌌 中心脉冲波环大范围闪光扩散
                        for (int j = 0; j < 5; j++)
                        {
                            Particle ring = new DirectionalPulseRing(
                                HeadPosition,
                                Vector2.Zero,
                                Color.Lerp(Color.DarkViolet, Color.Cyan, 0.5f),
                                new Vector2(3.5f, 3.5f) * (1.2f - j * 0.15f), // 分层不同大小
                                0f,
                                2.5f + j * 0.4f, // 不同扩散速度
                                0.3f,
                                60
                            );
                            GeneralParticleHandler.SpawnParticle(ring);
                        }

                        // 🌌 星芒爆发高亮闪烁
                        for (int k = 0; k < 12; k++)
                        {
                            GenericSparkle sparkle = new GenericSparkle(
                                HeadPosition + Main.rand.NextVector2Circular(24f, 24f),
                                Main.rand.NextVector2Circular(2f, 2f),
                                Color.White,
                                Color.Cyan,
                                Main.rand.NextFloat(2.0f, 3.5f),
                                Main.rand.Next(20, 30),
                                Main.rand.NextFloat(-0.08f, 0.08f),
                                2.0f
                            );
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }

                        // 播放瞬间爆发音效
                        SoundEngine.PlaySound(SoundID.Item4 with { Pitch = -0.3f, Volume = 2.8f }, HeadPosition);
                    }


                    // DirectionalPulseRing
                    if (Projectile.localAI[0] == 300)
                    {
                        Particle shrinkPulse = new DirectionalPulseRing(
                            HeadPosition,
                            Vector2.Zero,
                            Color.Purple * 1.2f,
                            new Vector2(2.8f, 2.8f),
                            0f,
                            12.0f,
                            0.2f,
                            45
                        );
                        GeneralParticleHandler.SpawnParticle(shrinkPulse);
                    }

                    // ================= 🌌 蓄力到达 300 后持续播放完整强特效 =================

                    // 螺旋 Dust（增强）
                    if (Projectile.localAI[0] % 4 == 0)
                    {
                        int dustCount = 24;
                        float radius = 100f;
                        for (int i = 0; i < dustCount; i++)
                        {
                            float angle = MathHelper.TwoPi / dustCount * i + Projectile.localAI[0] * 0.04f;
                            Vector2 offset = angle.ToRotationVector2() * radius * (1f - i / (float)dustCount * 0.3f);
                            Vector2 pos = HeadPosition + offset;
                            Vector2 vel = (HeadPosition - pos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 18f);

                            Dust dust = Dust.NewDustPerfect(pos, DustID.DarkCelestial, vel);
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                            dust.fadeIn = 0.4f;
                            dust.alpha = 100;
                            dust.color = Color.Lerp(Color.DarkViolet, Color.DarkBlue, 0.5f);
                        }
                    }

                    // ================= ⚡ SparkParticle 放射（枪头方向散射喷射） =================
                    if (Projectile.localAI[0] % 5 == 0)
                    {
                        int sparkLines = 8;
                        float sparkRadius = 80f;

                        // 🩶 枪头方向
                        Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                        for (int i = 0; i < sparkLines; i++)
                        {
                            float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-25f, 25f)); // 🔹 左右散射范围可调
                            Vector2 direction = forward.RotatedBy(angleOffset);

                            Vector2 spawnPos = HeadPosition + direction * sparkRadius * Main.rand.NextFloat(0.6f, 1.0f);
                            Vector2 vel = direction * Main.rand.NextFloat(14f, 26f); // 🚩 更快射出速度

                            SparkParticle spark = new SparkParticle(
                                spawnPos,
                                vel,
                                false,
                                Main.rand.Next(24, 34),             // 🚩 更长寿命
                                Main.rand.NextFloat(0.6f, 1.0f),    // 🚩 更大
                                Color.Lerp(Color.DarkViolet, Color.DarkBlue, 0.5f) * 0.9f
                            );
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                    }

                    // ================= ⚡ 呼吸感中心烟雾（枪头方向喷射） =================
                    if (Projectile.localAI[0] % 6 == 0)
                    {
                        // 🩶 枪头方向
                        Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                        Particle smoke = new HeavySmokeParticle(
                            HeadPosition + forward * 12f + Main.rand.NextVector2Circular(8f, 8f), // 稍微在前方生成
                            forward * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(0.5f, 0.5f), // 🚩 往前散射
                            Color.Lerp(Color.DarkViolet, Color.DarkBlue, 0.5f),
                            28,                                    // 🚩 更长寿命
                            Main.rand.NextFloat(0.4f, 0.6f),       // 🚩 大小
                            0.5f,                                   // 不透明度
                            Main.rand.NextFloat(-0.2f, 0.2f),
                            false
                        );
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }

                }

                {
                    // 屏幕震动保持
                    float shakePower;

                    // 当蓄力未满 300 时，线性递增震动（最大 30）
                    if (chargeTime < 300f)
                    {
                        shakePower = MathHelper.Lerp(0f, 30f, chargeTime / 300f);
                    }
                    else // 达到 300 后固定为 30
                    {
                        shakePower = 30f;
                    }

                    float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
                }
            




                Projectile.localAI[0]++;
                Projectile.localAI[1]++;

            }









            // 检测松手
            if (!Owner.channel)
            {
                if (Projectile.localAI[1] < 150f)
                {
                    // 🌑 蓄力不足 150 帧时，松手直接销毁弹幕，什么都不做
                    Projectile.Kill();
                    return;
                }

                Projectile.netUpdate = true;
                Projectile.timeLeft = 300; // 冲刺阶段持续时间
                Projectile.penetrate = 1; // 设置冲刺阶段的穿透次数

                CurrentState = BehaviorState.Dash;
            }
        }

        private void DoBehavior_Dash()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // 重置速度的逻辑
            {
                float initialSpeed = 55f; // 设定初始速度值，可根据需求替换具体值
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            }




            {
                Vector2 center = Projectile.Center;
                Vector2 backward = -Projectile.velocity.SafeNormalize(Vector2.UnitY);

                // =============== 1️⃣ 螺旋收缩 Dust（黑色 / 暗紫） ===============
                int spiralDustCount = 18;
                float spiralRadius = 8f;
                for (int i = 0; i < spiralDustCount; i++)
                {
                    float angle = MathHelper.TwoPi / spiralDustCount * i + Projectile.localAI[0] * 0.04f;
                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;
                    Vector2 pos = center + offset;

                    Vector2 vel = (center - pos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(8f, 14f);

                    int dustType = Main.rand.NextBool() ? DustID.Shadowflame : DustID.DarkCelestial;
                    int dust = Dust.NewDust(pos, 0, 0, dustType);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = Main.rand.NextFloat(0.7f, 1.0f);
                    Main.dust[dust].velocity = vel;
                    Main.dust[dust].alpha = 180;
                    Main.dust[dust].color = Color.Black;
                }

                // =============== 2️⃣ 真正的线性 SparkParticle 黑色拖尾 =================
                if (Projectile.localAI[0] % 1 == 0) // 每帧生成
                {
                    Particle trail = new SparkParticle(
                        center + Main.rand.NextVector2Circular(4f, 4f),
                        Projectile.velocity.SafeNormalize(Vector2.Zero) * -6f + Main.rand.NextVector2Circular(0.5f, 0.5f), // 逆向速度形成拖尾
                        false,
                        40,
                        0.8f,
                        Color.Black * 0.8f
                    );
                    GeneralParticleHandler.SpawnParticle(trail);
                }

                // =============== 3️⃣ 有序线性放射 SparkParticle =================
                if (Projectile.localAI[0] % 2 == 0)
                {
                    int sparkLines = 6;
                    float sparkRadius = 60f;
                    for (int i = 0; i < sparkLines; i++)
                    {
                        float angle = MathHelper.TwoPi / sparkLines * i;
                        Vector2 spawnPos = center + angle.ToRotationVector2() * sparkRadius;
                        Vector2 vel = (center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 16f);

                        SparkParticle spark = new SparkParticle(
                            spawnPos,
                            vel,
                            false,
                            28,
                            0.5f,
                            Color.Black * 0.9f
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

                // =============== 4️⃣ 深色重型烟雾，核心呼吸感 =================
                if (Projectile.localAI[0] % 4 == 0)
                {
                    Particle smokeH = new HeavySmokeParticle(
                        center + backward * Main.rand.NextFloat(20f, 40f), // 稍远生成
                        backward * Main.rand.NextFloat(0.8f, 1.8f) + Main.rand.NextVector2Circular(0.2f, 0.2f),
                        Color.Black,
                        40,
                        Main.rand.NextFloat(0.5f, 0.9f),
                        0.8f,
                        Main.rand.NextFloat(-0.01f, 0.01f),
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smokeH);
                }

                // =============== 5️⃣ 稳定 DirectionalPulseRing 呼吸黑洞波动感 =================
                if (Projectile.localAI[0] % 20 == 0)
                {
                    Particle ring = new DirectionalPulseRing(
                        center,
                        Projectile.velocity.SafeNormalize(Vector2.Zero),
                        Color.Black,
                        new Vector2(1.8f, 1.8f),
                        Projectile.rotation,
                        1.5f,
                        0.25f,
                        40
                    );
                    GeneralParticleHandler.SpawnParticle(ring);
                }

                Projectile.localAI[0]++;
            }









        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
            Vector2 center = Projectile.Center;


           



            {

                // 🩶 屏幕震动
                float shakePower = 8f;
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                int spiralCount = 60;
                int sparkCount = 50;
                int heavySmokeCount = 40;
                int sparkleCount = 15;

                // =============== 1️⃣ 黑洞螺旋 Dust 吸收（银河旋臂状） ===============
                for (int i = 0; i < spiralCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / spiralCount + Main.GlobalTimeWrappedHourly * 6f;
                    float radius = Main.rand.NextFloat(100f, 180f);
                    Vector2 pos = center + angle.ToRotationVector2() * radius;
                    Vector2 vel = (center - pos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 20f);

                    int dustType = Main.rand.NextBool() ? DustID.DarkCelestial : DustID.Shadowflame;
                    int dust = Dust.NewDust(pos, 0, 0, dustType);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = Main.rand.NextFloat(0.8f, 1.2f);
                    Main.dust[dust].velocity = vel;
                    Main.dust[dust].color = Color.Black;
                }

                // =============== 2️⃣ SparkParticle 黑色线性放射（外围扩散） ===============
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 24f);

                    SparkParticle spark = new SparkParticle(
                        center,
                        vel,
                        false,
                        Main.rand.Next(40, 60),
                        Main.rand.NextFloat(0.7f, 1.2f),
                        Color.Black * 0.9f
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // =============== 3️⃣ HeavySmokeParticle 黑色外围爆散烟雾 ===============
                for (int i = 0; i < heavySmokeCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / heavySmokeCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(60f, 100f);
                    Vector2 spawnPos = center + offset;

                    Vector2 vel = offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(1.5f, 1.5f);

                    Particle smokeH = new HeavySmokeParticle(
                        spawnPos,
                        vel,
                        Color.Black,
                        50,
                        Main.rand.NextFloat(0.8f, 1.4f),
                        0.9f,
                        Main.rand.NextFloat(-0.02f, 0.02f),
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(smokeH);
                }

                // =============== 4️⃣ GenericSparkle 十字星爆闪（核心脉动） ===============
                for (int i = 0; i < sparkleCount; i++)
                {
                    GenericSparkle sparker = new GenericSparkle(
                        center + Main.rand.NextVector2Circular(16f, 16f),
                        Vector2.Zero,
                        Color.Black,
                        Color.DarkViolet,
                        Main.rand.NextFloat(1.6f, 2.5f),
                        Main.rand.Next(8, 14),
                        Main.rand.NextFloat(-0.04f, 0.04f),
                        1.6f
                    );
                    GeneralParticleHandler.SpawnParticle(sparker);
                }

                // =============== 5️⃣ 深色 Dust 扩散银河尘带（外围爆散） ===============
                int dustCount = 80;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                    int dust = Dust.NewDust(center, 0, 0, DustID.DarkCelestial);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = Main.rand.NextFloat(0.8f, 1.2f);
                    Main.dust[dust].velocity = vel;
                    Main.dust[dust].color = Color.Black;
                }

                // 播放黑暗破碎音效
                SoundEngine.PlaySound(SoundID.Item74, center);
            }








            // 计算实际蓄力时长（这里假设 localAI[1] 存储了蓄力时长）
            float chargeTime = Projectile.localAI[1];
            // 将比例缩放（例如除以 5），传给黑洞影响后续计算
            //float transferredValue = chargeLevel1 / 5f;
            float transferredValue = 5f;
            //float finalDamage = chargeLevel1 * 0.1f;

            if (chargeTime >= 300f)
            {
                // =============== 蓄力足够时：生成黑洞 ===============
                if (Main.myPlayer == Projectile.owner)
                {
                    // 可根据需求调整传入的黑洞参数（这里传入缩放强度等）
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        center,
                        Vector2.Zero,
                        ModContent.ProjectileType<EndlessDevourJavBlackHole>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner,
                        chargeTime / 5f // 通过 ai[0] 传入黑洞影响后续强度[暂时没用到]
                    );
                }
                /*Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EndlessDevourJavBlackHole>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    transferredValue // 通过 ai[0] 传入
                );*/
            }
            else
            {
                // =============== 🌫️ 蓄力不足时：在周围圆周上生成 EndlessDevourJavOrbSmall ===============

                int orbCount = 8; // 🔹 生成数量（可调）
                float spawnRadius = 50f; // 🔹 圆周半径（可调）
                float orbSpeed = 10f; // 🔹 移动速度（可调）

                // 伤害倍率，蓄力 0~300 对应 0.2x ~ 0.8x（可调）
                float minDamageMultiplier = 0.2f;
                float maxDamageMultiplier = 0.8f;
                float chargeTime1 = Projectile.localAI[1];
                float damageMultiplier = MathHelper.Lerp(minDamageMultiplier, maxDamageMultiplier, chargeTime1 / 300f);

                for (int i = 0; i < orbCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / orbCount;
                    Vector2 offset = angle.ToRotationVector2() * spawnRadius;

                    // 🚩 计算切线方向（沿圆周方向乘 90°）
                    Vector2 direction = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
                    Vector2 velocity = direction * orbSpeed;

                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            center + offset, // 在圆周上生成
                            velocity,        // 切线方向移动
                            ModContent.ProjectileType<EndlessDevourJavOrbSmall>(),
                            (int)(Projectile.damage * damageMultiplier),
                            0f,
                            Projectile.owner
                        );
                    }
                }
            }





        }








    }
}

