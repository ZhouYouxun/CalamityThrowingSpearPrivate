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
using Terraria.Audio;
using CalamityMod.Particles;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class TidalMechanicsPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/TidalMechanics/TidalMechanics";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        private const int DecelerationFrames = 60;
        private const int SearchRadius = 19500; // 非常大的距离
        private const float ChargeMultiplier = 4.5f;
        private bool hasTarget = false;
        private Vector2 targetPosition;

        private bool preparedStrike = false;
        private Vector2 strikeVelocity;
        private bool enlarged = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!enlarged)
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, 2, lightColor, 1);
            }
            else
            {
                // 放大贴图 5 倍 + 外层呼吸脉动光效
                Microsoft.Xna.Framework.Graphics.Texture2D texture = ModContent.Request<Microsoft.Xna.Framework.Graphics.Texture2D>(Texture).Value;

                // 本体 5 倍绘制
                Main.EntitySpriteDraw(
                    texture,
                    Projectile.Center - Main.screenPosition,
                    null,
                    lightColor,
                    Projectile.rotation,
                    texture.Size() * 0.5f,
                    2f,
                    0,
                    0
                );

                // === 🚩 新增外层脉动绘制 ===

                // 脉动因子（周期 120 帧，每秒 2 次呼吸）
                float pulsate = 1f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.105f);
                // 外层缩放为 5 倍基础上略大（1.1 倍基础呼吸脉动）
                float outerScale = 2f * 1.2f * pulsate;

                // 外层颜色（淡蓝色 + 透明度呼吸）
                Color outerColor = Color.Cyan * 0.4f * (0.7f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.105f));

                // 外层绘制
                Main.EntitySpriteDraw(
                    texture,
                    Projectile.Center - Main.screenPosition,
                    null,
                    outerColor,
                    Projectile.rotation,
                    texture.Size() * 0.5f,
                    outerScale,
                    0,
                    0
                );

            }
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
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加蓝色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.01f;



            CreateWaterParticles();

            if (Projectile.ai[0] < DecelerationFrames)
            {
                Projectile.tileCollide = false;

                // 减速阶段
                Projectile.velocity *= 0.98f;
                Projectile.ai[0]++;
            }
            else if (!preparedStrike)
            {
                Projectile.tileCollide = true;
                Projectile.width = Projectile.height = 160;

                // 进入准备阶段：禁用绘制，瞬移
                Projectile.hide = true;
                Player player = Main.player[Projectile.owner];
                float xOffset = Main.rand.NextFloat(-1000f, 1000f);
                float yOffset = Main.rand.NextFloat(-100f, 100f);
                Projectile.Center = player.Center + new Vector2(xOffset, -400f + yOffset);

                // 锁定目标并计算冲击速度
                NPC target = FindClosestNPC(19500f);
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    strikeVelocity = direction * Projectile.velocity.Length() * 6f; // 三倍速度砸下
                }
                else
                {
                    strikeVelocity = Vector2.UnitY * Projectile.velocity.Length() * 3f; // 无目标则向下砸
                }

                preparedStrike = true;
            }
            else
            {
                // 开始砸下
                if (!enlarged)
                {
                    enlarged = true;
                    Projectile.hide = false;
                }

                Projectile.velocity = strikeVelocity;
            }
        }


        // 🚩 优雅水流飞行特效重制
        private void CreateWaterParticles()
        {
            Vector2 center = Projectile.Center;


            // 判断是否为传送后的巨化阶段
            bool isEnlarged = enlarged; // 你已有 enlarged 标志

            // 🚩 1️⃣ 水烟粒子：范围与缩放翻 5 倍
            Vector2 smokeVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(
                isEnlarged ? 2f : 0.4f,
                isEnlarged ? 3f : 0.8f
            );
            Particle waterSmoke = new HeavySmokeParticle(
                center,
                smokeVelocity,
                isEnlarged ? Color.Cyan : Color.LightBlue, // 更亮更夸张
                isEnlarged ? 30 : 18,                      // 寿命更长
                isEnlarged ? 3f : 0.8f,                    // 缩放大 5 倍
                isEnlarged ? 0.8f : 0.4f,                  // 不透明度略增
                isEnlarged ? 0.3f : 0.15f,                 // 旋转更快
                true
            );
            GeneralParticleHandler.SpawnParticle(waterSmoke);

            // 🚩 2️⃣ Dust 粒子生成范围扩大，数量翻倍
            int dustCount = isEnlarged ? 10 : 2;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(
                    Projectile.width * (isEnlarged ? 1.5f : 0.3f),
                    Projectile.height * (isEnlarged ? 1.5f : 0.3f)
                );
                Vector2 dustPos = center + offset;
                Vector2 dustVelocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(
                    isEnlarged ? 0.1f : 0.02f,
                    isEnlarged ? 0.4f : 0.08f
                );

                int dust = Dust.NewDust(dustPos, 0, 0, DustID.BlueCrystalShard, dustVelocity.X, dustVelocity.Y, 80, isEnlarged ? Color.Cyan : Color.LightBlue, Main.rand.NextFloat(
                    isEnlarged ? 2f : 0.8f,
                    isEnlarged ? 3f : 1.2f
                ));
                Main.dust[dust].noGravity = true;
            }

            // 🚩 3️⃣ Spark 粒子生成频率提高
            if (isEnlarged || Main.rand.NextBool(2))
            {
                Vector2 sparkVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * (isEnlarged ? 2f : 0.5f) + Main.rand.NextVector2Circular(0.1f, 0.1f);
                Particle spark = new SparkParticle(
                    center,
                    sparkVelocity,
                    false,
                    isEnlarged ? 80 : 40,                      // 更长寿命
                    isEnlarged ? 4f : 0.9f,                    // 大小更大
                    isEnlarged ? Color.Cyan * 0.9f : Color.LightBlue * 0.7f // 颜色更亮
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }

        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float minDistance = maxDetectDistance;

            foreach (NPC npc in Main.npc)
            {
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < minDistance && npc.CanBeChasedBy(this))
                {
                    minDistance = distance;
                    closestNPC = npc;
                }
            }

            // 增加蓝色粒子的数量
            for (int j = 0; j < 4; j++) // 将圈数增至4
            {
                for (int i = 0; i < 30; i++) // 每圈生成30个粒子
                {
                    Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 30 * i) * (1.5f + j);
                    Particle waterParticle = new HeavySmokeParticle(Projectile.Center, velocity, Color.DarkBlue, 15, 0.9f, 0.5f, 0.2f, true);
                    GeneralParticleHandler.SpawnParticle(waterParticle);
                }
            }

            return closestNPC;
        }


        private void SmoothRotateToTarget(Vector2 target)
        {
            Vector2 direction = target - Projectile.Center;
            direction.Normalize();
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, direction.ToRotation(), 0.05f);
        }

        private void CreateOceanDust()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 10f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, null, 0, Color.CadetBlue, 1.5f);
                dust.noGravity = true;
            }
        }

        private void ChargeTowardsTarget(Vector2 target)
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.velocity = (target - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length() * ChargeMultiplier;
                Projectile.ai[1] = 1;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 屏幕震动效果
            float shakePower = 5f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            // 释放公转型弹幕
            SpawnTyphoon();

            // 触发基于散度的粒子炸裂
            GenerateDivergenceExplosion();

            // 播放声音
            SoundEngine.PlaySound(SoundID.Item84, Projectile.Center);
        }
        private void GenerateDivergenceExplosion()
        {
            Vector2 origin = Projectile.Center;

            // 🚩 1️⃣ 有序：指数螺旋 SparkParticle 水流矩阵
            int spiralCount = 120;
            float a = 2f;
            float b = 0.15f;

            for (int i = 0; i < spiralCount; i++)
            {
                float theta = i * MathHelper.TwoPi / 20f; // 多圈分布
                float r = a * (float)Math.Exp(b * theta);
                Vector2 pos = origin + theta.ToRotationVector2() * r;

                Vector2 velocity = theta.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 6f;

                Particle spiralSpark = new SparkParticle(
                    pos,
                    velocity,
                    false,
                    50,
                    1.1f,
                    Color.LightBlue * 0.8f
                );
                GeneralParticleHandler.SpawnParticle(spiralSpark);
            }

            // 🚩 2️⃣ 无序：Dust 海水爆散，带多元正弦扰动
            int dustAmount = 200;
            for (int i = 0; i < dustAmount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(8f, 20f);
                Vector2 direction = angle.ToRotationVector2();
                Vector2 velocity = direction * speed;

                // 多元正弦扰动
                velocity.X += (float)Math.Sin(velocity.Y * 0.1f + Main.GameUpdateCount * 0.05f) * 2f;
                velocity.Y += (float)Math.Sin(velocity.X * 0.1f + Main.GameUpdateCount * 0.05f) * 2f;

                int dust = Dust.NewDust(origin, 0, 0, DustID.Water, velocity.X, velocity.Y, 80, Color.Cyan, Main.rand.NextFloat(1.2f, 1.8f));
                Main.dust[dust].noGravity = true;
            }

            // 🚩 3️⃣ 中和：轻型烟雾环绕
            int smokeCount = 40;
            for (int i = 0; i < smokeCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Particle smoke = new HeavySmokeParticle(
                    origin,
                    velocity,
                    Color.LightBlue * 0.6f,
                    50,
                    Main.rand.NextFloat(0.8f, 1.4f),
                    0.3f,
                    Main.rand.NextFloat(-0.05f, 0.05f),
                    true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 🚩 4️⃣ 特别强化：Square 粒子矩阵（洛伦兹吸引子投影模拟）
            // 洛伦兹参数
            double sigma = 10, rho = 28, beta = 8.0 / 3.0;
            Vector3 p = new Vector3(0.1f, 0f, 0f);

            int squareCount = 120; // 大幅提高
            float dt = 0.01f;
            for (int i = 0; i < squareCount; i++)
            {
                // 洛伦兹吸引子迭代
                double dx = sigma * (p.Y - p.X);
                double dy = p.X * (rho - p.Z) - p.Y;
                double dz = p.X * p.Y - beta * p.Z;
                p.X += (float)(dx * dt);
                p.Y += (float)(dy * dt);
                p.Z += (float)(dz * dt);

                Vector2 spawnPos = origin + new Vector2(p.X, p.Y) * 4f;
                Vector2 velocity = new Vector2(p.Y, -p.X).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(8f, 16f);

                SquareParticle square = new SquareParticle(
                    spawnPos,
                    velocity,
                    false,
                    60,
                    2.0f, // 放大至易见
                    Color.Cyan * 1.4f
                );
                GeneralParticleHandler.SpawnParticle(square);
            }

            // 🚩 5️⃣ CRE 高级光点：水光闪烁
            CTSLightingBoltsSystem.Spawn_GaussDischargeShards(origin);

            // 🚩 新增：在水平方向产生多次 GaussDischargeShards
            int shardCount = 6; // 可根据需要调整密度
            float horizontalSpacing = 40f; // 每个之间的水平距离

            for (int i = -shardCount / 2; i <= shardCount / 2; i++)
            {
                Vector2 spawnPos = origin + new Vector2(i * horizontalSpacing, 0f);
                CTSLightingBoltsSystem.Spawn_SpectralWhispers(spawnPos);

            }

            // 🚩 6️⃣ 有序收尾：脉冲环扩散
            Particle pulse = new DirectionalPulseRing(
                origin,
                Vector2.Zero,
                Color.LightBlue,
                new Vector2(1.2f, 1.2f),
                0f,
                0.5f,
                8f,
                50
            );
            GeneralParticleHandler.SpawnParticle(pulse);
        }



     


        private void SpawnTyphoon()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 position = Projectile.Center;
                float angle = MathHelper.Pi * i; // 0度 和 180度
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, velocity, ModContent.ProjectileType<TidalMechanicsTyphoon>(), Projectile.damage / 2, 0, Projectile.owner);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }

    }
}