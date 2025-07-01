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
using CalamityMod.Buffs.StatDebuffs;
using Terraria.Audio;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLancePROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";

        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/TheLastLance/TheLastLancePROJ";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            {
                // 🚩🚩🚩 【TheLastLancePROJ 深海飞行特效重制块】🚩🚩🚩

                // === 1️⃣ 无序：原双螺旋气泡 (Gore Bubble) 代表深海泡沫 ===
                float offset = (float)Math.Sin(Projectile.localAI[0] * 0.1f) * 1.5f;
                Vector2 bubblePos1 = Projectile.Center + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * offset;
                Vector2 bubblePos2 = Projectile.Center + Projectile.velocity.RotatedBy(-MathHelper.PiOver2) * offset;
                Gore bubble1 = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), bubblePos1, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                Gore bubble2 = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), bubblePos2, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                bubble1.timeLeft = 8 + Main.rand.Next(6);
                bubble2.timeLeft = 8 + Main.rand.Next(6);
                bubble1.scale = Main.rand.NextFloat(0.6f, 1f);
                bubble2.scale = Main.rand.NextFloat(0.6f, 1f);
                bubble1.type = Main.rand.NextBool(3) ? 412 : 411;
                bubble2.type = Main.rand.NextBool(3) ? 412 : 411;

                // === 2️⃣ 有序：深海灰蓝 Dust 水流线条 ===
                if (Main.rand.NextBool(3)) // 控制频率，保证节奏感
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(Projectile.width * 0.4f, Projectile.height * 0.4f);
                    Vector2 spawnPos = Projectile.Center + dustOffset;
                    Vector2 dustVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.5f, 1.5f);
                    int dust = Dust.NewDust(spawnPos, 0, 0, DustID.Water, dustVelocity.X, dustVelocity.Y, 100, Color.DarkSlateGray, Main.rand.NextFloat(0.8f, 1.2f));
                    Main.dust[dust].noGravity = true;
                }

                // === 3️⃣ 有序：深海流线 SparkParticle 拖尾 ===
                if (Main.rand.NextBool(2)) // 平均每 2 帧一次
                {
                    Vector2 sparkVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 1.2f + Main.rand.NextVector2Circular(0.2f, 0.2f);
                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        sparkVelocity,
                        false,
                        40,
                        0.9f,
                        Color.DarkBlue * 0.8f
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                Projectile.localAI[0]++; // 更新动画计数
            }


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 根据敌人当前血量设置冻结状态的持续时间
            int freezeDuration = target.life < (target.lifeMax / 2) ? 600 : 300;

            // 对敌人施加冻结状态
            target.AddBuff(ModContent.BuffType<GlacialState>(), freezeDuration); // 冰河时代
            target.AddBuff(BuffID.Frostburn, freezeDuration); // 原版的霜火效果
            target.AddBuff(BuffID.Chilled, freezeDuration); // 原版的寒冷效果

            // 检查敌人是否同时拥有三种冻结状态
            if (target.HasBuff(ModContent.BuffType<GlacialState>()) && target.HasBuff(BuffID.Frostburn) && target.HasBuff(BuffID.Chilled))
            {
                Projectile.damage = (int)(Projectile.damage * 1.75f); // 造成1.75倍伤害
            }

            // 检查玩家是否处于海洋群系，以便造成额外的两倍伤害
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.ZoneBeach)
            {
                Projectile.damage = (int)(Projectile.damage * 2.0f); // 造成两倍伤害
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 播放原版音效 Item69
            SoundEngine.PlaySound(SoundID.Item69, Projectile.position);

            // 现有的屏幕震动效果
            float shakePower = 0.35f; // 震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 根据与玩家距离进行衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            // 生成 3 发随机角度的 TheLastLanceWater 弹幕（正后方）
            for (int i = 0; i < 3; i++)
            {
                // 在 -45 度到 45 度之间随机生成一个角度
                float randomAngle = Main.rand.NextFloat(-45f, 45f);
                Vector2 baseDirection = -Projectile.velocity.SafeNormalize(Vector2.Zero); // 以弹幕的反方向为基准
                Vector2 rotatedDirection = baseDirection.RotatedBy(MathHelper.ToRadians(randomAngle)); // 在基础方向上旋转角度
                Vector2 spawnVelocity = rotatedDirection * 12f; // 固定的初始速度，可以根据需要调整

                // 创建 TheLastLanceWater 弹幕
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    spawnVelocity,
                    ModContent.ProjectileType<TheLastLanceWater>(),
                    (int)(Projectile.damage * 1.0f), // 1.0倍倍率的伤害
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            // 释放究极特效
            GenerateDeepSeaExplosion();

            //// 在弹幕死亡时，向反方向发射大量蓝色Dust特效
            //for (int i = 0; i < 20; i++)
            //{
            //    Vector2 dustVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * -1.2f; // 随机角度旋转并反方向发射Dust
            //    Dust blueDust = Dust.NewDustPerfect(Projectile.position, DustID.BlueCrystalShard, dustVelocity, 0, Color.Blue, 1.5f);
            //    blueDust.noGravity = true; // 使Dust不受重力影响
            //}


         


        }


        // 🚩🚩🚩 TheLastLancePROJ 专属深海终极 OnKill 特效
        private void GenerateDeepSeaExplosion()
        {
            Vector2 origin = Projectile.Center;

            // === 1️⃣ 有序：五芒星深海螺旋矩阵（SparkParticle）===
            int points = 5;
            float outerRadius = 90f;
            float innerRadius = 45f;
            float rotationOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int p = 0; p < points; p++)
            {
                float angle = MathHelper.TwoPi * p / points + rotationOffset;
                Vector2 outerPos = origin + angle.ToRotationVector2() * outerRadius;
                Vector2 innerPos = origin + (angle + MathHelper.PiOver4).ToRotationVector2() * innerRadius;

                for (int i = 0; i < 12; i++)
                {
                    Vector2 lerpPos = Vector2.Lerp(innerPos, outerPos, i / 12f);
                    Vector2 velocity = (lerpPos - origin).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(10f, 20f);

                    Particle spark = new SparkParticle(
                        lerpPos,
                        velocity,
                        false,
                        60,
                        Main.rand.NextFloat(1.2f, 1.6f),
                        Color.DeepSkyBlue * 1.5f
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // === 2️⃣ 有序升级：多螺旋水门粒子特效（深海升级版） ===
            {
                int spiralCount = 4;
                int spiralParticles1 = 100;
                float initialAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float radiusIncrement = 1.8f;
                float baseRadius = 0f;

                for (int s = 0; s < spiralCount; s++)
                {
                    float spiralOffsetAngle = s * MathHelper.TwoPi / spiralCount;
                    float spiralRadius = baseRadius;
                    float angleIncrement = MathHelper.TwoPi / spiralParticles1;

                    for (int i = 0; i < spiralParticles1; i++)
                    {
                        float angle = initialAngle + spiralOffsetAngle + i * angleIncrement;
                        Vector2 offset = angle.ToRotationVector2() * spiralRadius;
                        Vector2 position = origin + offset;

                        Dust spiralDust = Dust.NewDustPerfect(position, DustID.Water, Vector2.Zero, 0, Color.DarkSlateGray, 0.9f);
                        spiralDust.noGravity = true;

                        spiralRadius += radiusIncrement;
                    }
                }
            }

            // === 3️⃣ 有序螺旋波扩散 (SparkParticle) ===
            int spiralParticles = 100;
            for (int i = 0; i < spiralParticles; i++)
            {
                float progress = i / (float)spiralParticles;
                float angle = progress * MathHelper.TwoPi * 6f + Main.GameUpdateCount * 0.04f;
                float radius = progress * 100f;
                Vector2 pos = origin + angle.ToRotationVector2() * radius;
                Vector2 velocity = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 5f;

                Particle spiralSpark = new SparkParticle(
                    pos,
                    velocity,
                    false,
                    50,
                    1.0f,
                    Color.DarkBlue * 0.7f
                );
                GeneralParticleHandler.SpawnParticle(spiralSpark);
            }

            // === 4️⃣ 中和：深灰轻型烟雾 (HeavySmokeParticle) ===
            int smokeCount = 40;
            for (int i = 0; i < smokeCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Particle smoke = new HeavySmokeParticle(
                    origin,
                    velocity,
                    Color.Gray * 0.6f,
                    50,
                    Main.rand.NextFloat(1.0f, 1.6f),
                    0.25f,
                    Main.rand.NextFloat(-0.04f, 0.04f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // === 5️⃣ 特别强化：Square 粒子环（深海科技爆发感） ===
            int squareCount = 45;
            float squareRadius = 70f;
            for (int i = 0; i < squareCount; i++)
            {
                float angle = MathHelper.TwoPi * i / squareCount;
                Vector2 spawnPos = origin + angle.ToRotationVector2() * squareRadius;
                Vector2 velocity = angle.ToRotationVector2() * 10f;

                SquareParticle square = new SquareParticle(
                    spawnPos,
                    velocity,
                    false,
                    50,
                    2.8f,
                    Color.DarkSlateGray * 1.4f
                );
                GeneralParticleHandler.SpawnParticle(square);
            }
        }




    }
}