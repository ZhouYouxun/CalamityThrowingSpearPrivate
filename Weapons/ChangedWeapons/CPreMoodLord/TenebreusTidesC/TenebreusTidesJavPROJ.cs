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
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC
{
    public class TenebreusTidesJavPROJ : ModProjectile, ILocalizedModType
    {
        //public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/TenebreusTidesC/TenebreusTidesJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";

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

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (CurrentState == BehaviorState.Dash)
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            }
            else
            {
                Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            }
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 20; // 设置穿透次数为20
            Projectile.timeLeft = 1500; // 设置持续时间为1500帧
            Projectile.extraUpdates = 1; // 额外更新次数为1
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
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
        private int shootTimer = 0; // 添加一个计时器
                                    // 初始生成频率
        private int shootInterval = 28;
        private int currentAngle = 0; // 角度累积
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

            // 生成枪头烟雾
            Vector2 smokePosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);
            Particle smoke = new HeavySmokeParticle(
                smokePosition,
                Vector2.UnitY * -1 * Main.rand.NextFloat(3f, 7f),
                Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f),
                Main.rand.Next(30, 60),
                Main.rand.NextFloat(0.25f, 0.5f),
                1.0f,
                MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f)),
                true
            );
            GeneralParticleHandler.SpawnParticle(smoke);




            // 生产弹幕
            shootTimer++;
            if (shootTimer >= shootInterval)
            {
                shootTimer = 0;

                Vector2 mousePos = Main.MouseWorld;
                Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

                // 基准点：鼠标与枪头之间的 25% 靠近枪头的位置
                Vector2 mid = (gunTip + mousePos) / 2f;
                Vector2 basePoint = Vector2.Lerp(mousePos, mid, 0.9f);

                // 基准线方向 & 垂直向量
                Vector2 dir = (mousePos - gunTip).SafeNormalize(Vector2.UnitX);
                Vector2 perp = dir.RotatedBy(MathHelper.PiOver2);

                // 左右两个生成点
                Vector2 leftSpawn = basePoint - perp * 48f;
                Vector2 rightSpawn = basePoint + perp * 48f;

                // 雨刷器式摆动角度
                float swing = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * MathHelper.ToRadians(10f); // 摆动多少角度

                // 左右各生成一颗（对称摆动）
                Vector2 leftDir = dir.RotatedBy(+swing) * 12f;
                Vector2 rightDir = dir.RotatedBy(-swing) * 12f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    leftSpawn,
                    leftDir,
                    ModContent.ProjectileType<TenebreusTidesJavWaterSword>(),
                    (int)(Projectile.damage * 0.27f),
                    Projectile.knockBack,
                    Projectile.owner
                );

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    rightSpawn,
                    rightDir,
                    ModContent.ProjectileType<TenebreusTidesJavWaterSword>(),
                    (int)(Projectile.damage * 0.27f),
                    Projectile.knockBack,
                    Projectile.owner
                );

                SoundEngine.PlaySound(SoundID.Item34, Projectile.position);

                // 射击间隔逐渐减小（保留你的加特林机制）
                if (shootInterval > 8)
                    shootInterval--;
            }





            // 检测松手
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.timeLeft = 180; // 冲刺阶段持续时间
                Projectile.penetrate = 20; // 设置冲刺阶段的穿透次数

                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/New/深渊潮涌音效") with { Volume = 0.5f, Pitch = 0.0f }, Projectile.Center);

                // 屏幕震动
                float shakePower = 2f;
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                // 释放魔法阵
                DrawMagicCircle();

                SoundEngine.PlaySound(SoundID.Item79, Projectile.position);

                CurrentState = BehaviorState.Dash;
            }
        }

        private void DoBehavior_Dash()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // 重置速度的逻辑
            {
                float initialSpeed = 15f; // 设定初始速度值，可根据需求替换具体值
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            }

            // 原地粒子效果
            Color particleColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f);
            Particle spiralParticle = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.2f, particleColor, 30, Projectile.scale * 0.8f, 1.0f, MathHelper.ToRadians(2f), true);
            GeneralParticleHandler.SpawnParticle(spiralParticle);

            // 每隔10帧发射一次水剑
            shootTimer++;
            if (shootTimer == 10) // 如果计时器达到10帧
            {
                shootTimer = 0; // 重置计时器

                // 往左右固定角度发射水剑
                Vector2 leftVelocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-10)) * 1f;
                Vector2 rightVelocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(10)) * 1f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, leftVelocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.7f), Projectile.knockBack, Projectile.owner);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rightVelocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.7f), Projectile.knockBack, Projectile.owner);
            }
        }

        private void DrawMagicCircle()
        {
            // 魔法阵特效
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi / 5 * i;
                Vector2 position = Projectile.Center + angle.ToRotationVector2() * 50f;
                Dust dust = Dust.NewDustPerfect(position, 104, null, 0, Color.DarkBlue, 1.5f);
                dust.noGravity = true;
            }

            for (int i = 0; i < 2; i++)
            {
                float angle = MathHelper.TwoPi / 3 * i;
                Vector2 position = Projectile.Center + angle.ToRotationVector2() * 30f;
                Dust dust = Dust.NewDustPerfect(position, 29, null, 0, Color.CadetBlue, 1.8f);
                dust.noGravity = true;
            }
        }

        //public override void AI()
        //{
        //    // 保持弹幕旋转逻辑不变
        //    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

        //    // 设置深海的深蓝色光效
        //    Lighting.AddLight(Projectile.Center, Color.DarkBlue.ToVector3() * 0.6f);

        //    // 每帧速度乘以1.03，逐渐加速
        //    Projectile.velocity *= 1.03f;

        //    // 每7帧释放一次TenebreusTidesWaterSword弹幕
        //    if (Projectile.timeLeft % 7 == 0 && Main.myPlayer == Projectile.owner)
        //    {
        //        // 当前方向
        //        Vector2 currentVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero);

        //        // 正前方的TenebreusTidesWaterSword弹幕
        //        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, currentVelocity * 8f, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);

        //        // 正左方和正右方的TenebreusTidesWaterSword弹幕（旋转90度和-90度）
        //        Vector2 leftVelocity = currentVelocity.RotatedBy(MathHelper.ToRadians(90)) * 8f;
        //        Vector2 rightVelocity = currentVelocity.RotatedBy(MathHelper.ToRadians(-90)) * 8f;
        //        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, leftVelocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
        //        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rightVelocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
        //    }

        //    // 单螺旋的粒子效果，从左下开始向右下，循环生成
        //    float progress = (Projectile.timeLeft % 60) / 60f; // 粒子进度控制
        //    float angle = MathHelper.TwoPi * progress; // 单螺旋角度

        //    // 计算粒子位置，螺旋从左下到右下
        //    Vector2 spiralOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f; // 控制螺旋的大小
        //    Vector2 spiralPosition = Projectile.BottomLeft + spiralOffset;

        //    // 创建粒子
        //    Color particleColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f); // 深蓝色渐变效果
        //    Particle spiralParticle = new HeavySmokeParticle(spiralPosition, Projectile.velocity * 0.2f, particleColor, 30, Projectile.scale * 0.8f, 1.0f, MathHelper.ToRadians(2f), true);
        //    GeneralParticleHandler.SpawnParticle(spiralParticle);

        //    // 在飞行路径上留下深蓝色的重型烟雾粒子
        //    if (Main.rand.NextBool(4)) // 每4帧生成一次粒子
        //    {
        //        Color smokeColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f); // 使用深蓝色和浅蓝色渐变
        //        Particle smoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f, smokeColor, 30, Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f), 1.0f, MathHelper.ToRadians(2f), required: true);
        //        GeneralParticleHandler.SpawnParticle(smoke);
        //    }
        //}

        public override void OnKill(int timeLeft)
        {

            {
                // === 深海调色板（多色随机） ===
                Color[] smokeColors = new Color[]
                {
    Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f),      // 深蓝 → 冷青
    Color.Lerp(Color.Teal, Color.DarkCyan, 0.7f),           // 蓝绿
    Color.Lerp(Color.Navy, Color.MidnightBlue, 0.5f),       // 海军蓝
    Color.Lerp(Color.DarkSlateBlue, Color.MediumBlue, 0.4f) // 石板蓝
                };

                // === 内圈浓烟（小半径，密度高，寿命长） ===
                int innerRadius = 3 * 16;
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20f * i + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 offset = angle.ToRotationVector2() * innerRadius;
                    Vector2 pos = Projectile.Center + offset;
                    Vector2 vel = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f);

                    Color c = smokeColors[Main.rand.Next(smokeColors.Length)];
                    Particle smoke = new HeavySmokeParticle(
                        pos,
                        vel,
                        c,
                        Main.rand.Next(30, 50),                         // 内圈寿命更长
                        Projectile.scale * Main.rand.NextFloat(0.5f, 0.8f), // 缩放大一些
                        1.3f,
                        Main.rand.NextFloat(-0.25f, 0.25f),
                        required: true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

                // === 外圈扩散烟雾（大半径，数量更多，缩放小，扩散快） ===
                int outerRadius = 6 * 16;
                for (int i = 0; i < 40; i++)
                {
                    float angle = MathHelper.TwoPi / 40f * i + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 offset = angle.ToRotationVector2() * outerRadius;
                    Vector2 pos = Projectile.Center + offset;
                    Vector2 vel = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1.5f, 3f);

                    Color c = smokeColors[Main.rand.Next(smokeColors.Length)];
                    Particle smoke = new HeavySmokeParticle(
                        pos,
                        vel,
                        c * 0.9f,                                       // 外圈稍微透明
                        Main.rand.Next(20, 35),
                        Projectile.scale * Main.rand.NextFloat(0.3f, 0.6f), // 缩放小一些
                        1.1f,
                        Main.rand.NextFloat(-0.2f, 0.2f),
                        required: true
                    );
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

            }
            // === 尖锐能量碎片（WaterFlavoredParticle） ===
            for (int i = 0; i < 36; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) * Main.rand.NextFloat(2f, 5f);
                Color c = Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat()) * 0.9f;

                WaterFlavoredParticle spark = new WaterFlavoredParticle(
                    Projectile.Center,
                    vel,
                    false,
                    Main.rand.Next(15, 30),
                    0.9f + Main.rand.NextFloat(0.3f),
                    c
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }


        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.95);
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }
    }
}
