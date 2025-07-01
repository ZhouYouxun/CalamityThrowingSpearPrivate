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
                // === 追加：瞄准期间召唤随机远处弹幕打向自己（带偏差） ===

                // 仅在本地玩家执行生成
                if (Main.myPlayer == Projectile.owner && Projectile.localAI[0] % 60f == 0) // 每 X 帧触发一次
                {
                    Vector2 playerCenter = Owner.Center;

                    float spawnRadius = 1200f; // ✅ 半径很大（你可调，如 1200f）
                    float damageMultiplier = 0.5f; // ✅ 伤害倍率（你可调）

                    // 在大圆周上随机选取位置
                    Vector2 randomOffset = Main.rand.NextVector2Unit() * spawnRadius;
                    Vector2 spawnPosition = playerCenter + randomOffset;

                    // 计算飞向投射物方向（略微偏离）
                    Vector2 targetDirection = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.UnitY);
                    targetDirection = targetDirection.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)); // ✅ 偏离范围（你可调）
                    Vector2 velocity = targetDirection * 18f; // ✅ 速度（你可调）

                    // 生成 EndlessDevourJavOrb
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPosition,
                        velocity,
                        ModContent.ProjectileType<EndlessDevourJavOrb>(),
                        (int)(Projectile.damage * damageMultiplier), // ✅ 伤害倍率（你可调）
                        0f,
                        Projectile.owner
                    );
                }

                Projectile.localAI[0]++; // ✅ 确保帧计数器增加以触发上述逻辑

            }



            {
                Vector2 HeadPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 6f + Main.rand.NextVector2Circular(3f, 3f);

                // =============== 1️⃣ 螺旋 Dust （高速度、轻量） ===============
                int spiralDustCount = 24;
                float spiralRadius = 100f;

                for (int i = 0; i < spiralDustCount; i++)
                {
                    float angle = (MathHelper.TwoPi / spiralDustCount) * i + Projectile.localAI[0] * 0.04f;
                    Vector2 offset = angle.ToRotationVector2() * spiralRadius * (1f - i / (float)spiralDustCount * 0.3f);
                    Vector2 dustPos = HeadPosition + offset;

                    Vector2 velocity = (HeadPosition - dustPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 18f); // 🚀 高速推进

                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.DarkCelestial, velocity);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.2f); // 🚩 更小
                    dust.fadeIn = 0.4f;                           // 🚩 更透明
                    dust.alpha = 100;                             // 🚩 透明度
                    dust.color = Color.Black;
                }

                // =============== 2️⃣ DirectionalPulseRing（节奏保持） ===============
                if (Projectile.localAI[0] % 12 == 0)
                {
                    Particle shrinkPulse = new DirectionalPulseRing(
                        HeadPosition,
                        Vector2.Zero,
                        Color.Black,
                        new Vector2(2.8f, 2.8f),
                        0f,
                        2.0f,
                        0.2f,
                        45
                    );
                    GeneralParticleHandler.SpawnParticle(shrinkPulse);
                }

                // =============== 3️⃣ SparkParticle（高速度可达中心） ===============
                if (Projectile.localAI[0] % 3 == 0)
                {
                    int sparkLines = 8;
                    float sparkRadius = 80f;
                    for (int i = 0; i < sparkLines; i++)
                    {
                        float angle = MathHelper.TwoPi / sparkLines * i;
                        Vector2 spawnPos = HeadPosition + angle.ToRotationVector2() * sparkRadius;
                        Vector2 velocity = (HeadPosition - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(12f, 22f); // 🚀 高速

                        SparkParticle spark = new SparkParticle(
                            spawnPos,
                            velocity,
                            false,
                            Main.rand.Next(18, 26),
                            Main.rand.NextFloat(0.3f, 0.6f), // 🚩 更小
                            Color.Black * 0.9f               // 🚩 稍透明
                        );
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }

                // =============== 4️⃣ 中心白烟（保持呼吸感，更小） ===============
                if (Projectile.localAI[0] % 5 == 0)
                {
                    Particle smoke = new HeavySmokeParticle(
                        HeadPosition,
                        Main.rand.NextVector2Circular(0.1f, 0.1f),
                        Color.WhiteSmoke,
                        20,
                        Main.rand.NextFloat(0.3f, 0.5f),
                        0.35f,
                        Main.rand.NextFloat(-0.3f, 0.3f),
                        false
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

                // 屏幕震动保持
                float shakePower = MathHelper.Clamp(Projectile.localAI[1] * 0.1f, 5f, 100f);
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                Projectile.localAI[0]++;
            }










            // 检测松手
            if (!Owner.channel)
            {
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
                float spiralRadius = 80f;
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


            {
                Vector2 center = Projectile.Center;

                // 🩶 屏幕震动（固定最大狂野级别）
                float shakePower = 80f;
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                int spiralCount = 60;
                int sparkCount = 50;
                int heavySmokeCount = 20;
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

                    Vector2 vel = offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(0.5f, 0.5f);

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







           
            // 计算蓄力强度（假设最大 100，最小 1）
            float chargeLevel1 = MathHelper.Clamp(Projectile.localAI[1], 1f, 100f);

            // 将比例缩放（例如除以 5），传给黑洞影响后续计算
            float transferredValue = chargeLevel1 / 5f;
            //float finalDamage = chargeLevel1 * 0.1f;

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EndlessDevourJavBlackHole>(),
                    //(int)(Projectile.damage * 1+finalDamage),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    transferredValue // 通过 ai[0] 传入
                );
            }
           


        }








    }
}

