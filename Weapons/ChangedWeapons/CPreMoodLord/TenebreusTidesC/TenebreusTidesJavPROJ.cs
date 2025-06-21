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
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/TenebreusTidesC/TenebreusTidesJav";
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
            Projectile.penetrate = -1; // 设置穿透次数为20
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


            // 每隔10帧发射一次水剑
            shootTimer++;
            if (shootTimer >= 15) // 如果计时器达到10帧
            {
                shootTimer = 0; // 重置计时器

                Vector2 spawnPosition = Main.MouseWorld + Main.rand.NextVector2Circular(15 * 16f, 15 * 16f);
                Vector2 velocity = (Main.MouseWorld - spawnPosition).SafeNormalize(Vector2.Zero) * 12f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                SoundEngine.PlaySound(SoundID.Item34, Projectile.position);

                // 用粒子特效绘制线条
                //for (int i = 0; i < 10; i++)
                //{
                //    Vector2 point = Vector2.Lerp(spawnPosition, Owner.Center, i / 10f) + Main.rand.NextVector2Circular(2f, 2f);
                //    Particle waterEffect = new HeavySmokeParticle(point, Vector2.Zero, Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f), 20, 0.5f, 0.5f, 0f, true);
                //    GeneralParticleHandler.SpawnParticle(waterEffect);
                //}

                // 添加单螺旋粒子特效并绘制线条
                float progress = (Projectile.localAI[0] % 60) / 60f; // 粒子进度控制
                float angle = MathHelper.TwoPi * progress; // 单螺旋角度
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f; // 螺旋的偏移

                // 单螺旋的粒子
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, Projectile.velocity * 0.2f, 0, Color.DarkBlue, 1.2f);
                dust.noGravity = true;

                // 绘制从 spawnPosition 到 Owner.Center 的线条
                for (int i = 0; i < 10; i++)
                {
                    Vector2 point = Vector2.Lerp(spawnPosition, Owner.Center, i / 10f);
                    Dust lineDust = Dust.NewDustPerfect(point, DustID.Water, Vector2.Zero, 0, Color.CadetBlue, 1.0f);
                    lineDust.noGravity = true;
                }
            }

            // 每15帧生成水剑弹幕
            if (Projectile.localAI[0] % 15 == 0)
            {

            }

            // 检测松手
            if (!Owner.channel)
            {
                Projectile.netUpdate = true;
                Projectile.timeLeft = 300; // 冲刺阶段持续时间
                Projectile.penetrate = 20; // 设置冲刺阶段的穿透次数

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
            if (shootTimer >= 10) // 如果计时器达到10帧
            {
                shootTimer = 0; // 重置计时器

                // 往左右固定角度发射水剑
                Vector2 leftVelocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-10)) * 1f;
                Vector2 rightVelocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(10)) * 1f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, leftVelocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.35f), Projectile.knockBack, Projectile.owner);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rightVelocity, ModContent.ProjectileType<TenebreusTidesJavWaterSword>(), (int)(Projectile.damage * 0.35f), Projectile.knockBack, Projectile.owner);
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
            // 定义颜色和粒子类型
            Color smokeColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f);

            // 内圈粒子特效
            int innerRadius = 3 * 16; // 内圈半径
            for (int i = 0; i < 20; i++) // 生成20个粒子
            {
                float angle = MathHelper.ToRadians(360f / 20f * i) + Main.rand.NextFloat(-0.1f, 0.1f); // 随机偏移
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * innerRadius;
                Vector2 particlePosition = Projectile.Center + offset;

                // 随机化粒子速度
                Vector2 particleVelocity = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);

                // 创建粒子
                Particle smoke = new HeavySmokeParticle(
                    particlePosition,
                    particleVelocity,
                    smokeColor,
                    Main.rand.Next(20, 40), // 随机寿命
                    Projectile.scale * Main.rand.NextFloat(0.4f, 0.7f), // 缩放大小
                    1.2f,
                    Main.rand.NextFloat(-0.2f, 0.2f),
                    required: true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 外圈粒子特效
            int outerRadius = 6 * 16; // 外圈半径
            for (int i = 0; i < 40; i++) // 生成40个粒子
            {
                float angle = MathHelper.ToRadians(360f / 40f * i) + Main.rand.NextFloat(-0.1f, 0.1f); // 随机偏移
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * outerRadius;
                Vector2 particlePosition = Projectile.Center + offset;

                // 随机化粒子速度
                Vector2 particleVelocity = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f);

                // 创建粒子
                Particle smoke = new HeavySmokeParticle(
                    particlePosition,
                    particleVelocity,
                    smokeColor,
                    Main.rand.Next(20, 40), // 随机寿命
                    Projectile.scale * Main.rand.NextFloat(0.3f, 0.6f), // 缩放大小
                    1.2f,
                    Main.rand.NextFloat(-0.2f, 0.2f),
                    required: true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }
    }
}
