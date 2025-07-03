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
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.SupremeCalamitas;
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ViolenceC
{
    public class ViolenceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/EAfterDog/ViolenceC/ViolenceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        internal Player Owner => Main.player[Projectile.owner]; // 定义Owner，引用发射弹幕的玩家

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
            Projectile.penetrate = 2; // 允许两次伤害
            Projectile.timeLeft = 666;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 3; // 额外更新次数改为2
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 将光效改为深红色
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.55f);

            // 保留加速效果
            Projectile.velocity *= 1.01f;

            // 每隔一定时间产生轨迹
            if (Main.rand.NextBool(2))
            {
                float sideOffset = Main.rand.NextFloat(-1f, 1f);
                Vector2 trailPos = Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * sideOffset;

                float trailScale = Main.rand.NextFloat(0.8f, 1.2f);
                Color trailColor = Main.rand.NextBool() ? Color.Red : Color.DarkRed;

                Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(trail);
            }

            {
                // === ViolenceJav 飞行特效：真实破空-撕裂-回收流线结构 ===

                if (Main.GameUpdateCount % 1 == 0)
                {
                    Vector2 forward = Projectile.rotation.ToRotationVector2();

                    // 1️⃣ 前方破空喷射（Spark + Dust）
                    Vector2 frontPos = Projectile.Center + forward * 24f;
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 sideDir = forward.RotatedBy(MathHelper.ToRadians(45f * i));
                        // Spark
                        Particle spark = new SparkParticle(
                            frontPos,
                            sideDir * Main.rand.NextFloat(12f, 22f),
                            false,
                            18,
                            Main.rand.NextFloat(1.0f, 1.4f),
                            Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat(0.3f, 0.7f))
                        );
                        GeneralParticleHandler.SpawnParticle(spark);

                        // Dust
                        Dust dust = Dust.NewDustPerfect(
                            frontPos,
                            DustID.Blood,
                            sideDir * Main.rand.NextFloat(6f, 14f),
                            100,
                            Color.Red,
                            Main.rand.NextFloat(1.0f, 1.5f)
                        );
                        dust.noGravity = true;
                    }

                    // 2️⃣ 中段撕裂（Spark + Dust）
                    Vector2 midPos = Projectile.Center;
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 sideDir = forward.RotatedBy(MathHelper.ToRadians(30f * i));
                        // Spark
                        Particle spark = new SparkParticle(
                            midPos,
                            sideDir * Main.rand.NextFloat(8f, 16f),
                            false,
                            22,
                            Main.rand.NextFloat(0.9f, 1.3f),
                            Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat(0.2f, 0.6f))
                        );
                        GeneralParticleHandler.SpawnParticle(spark);

                        // Dust
                        Dust dust = Dust.NewDustPerfect(
                            midPos,
                            DustID.Blood,
                            sideDir * Main.rand.NextFloat(4f, 10f),
                            100,
                            Color.Red,
                            Main.rand.NextFloat(0.9f, 1.3f)
                        );
                        dust.noGravity = true;
                    }

                    // 3️⃣ 后方收拢（Spark + Dust）
                    Vector2 backPos = Projectile.Center - forward * 16f;
                    Vector2 inwardDir = -forward;
                    for (int j = 0; j < 2; j++)
                    {
                        // Spark
                        Particle spark = new SparkParticle(
                            backPos,
                            inwardDir.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(5f, 10f),
                            false,
                            25,
                            Main.rand.NextFloat(0.8f, 1.2f),
                            Color.DarkRed
                        );
                        GeneralParticleHandler.SpawnParticle(spark);

                        // Dust
                        Dust dust = Dust.NewDustPerfect(
                            backPos,
                            DustID.Blood,
                            inwardDir.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(3f, 8f),
                            100,
                            Color.DarkRed,
                            Main.rand.NextFloat(0.8f, 1.2f)
                        );
                        dust.noGravity = true;
                    }



                    // === ViolenceJav 飞行期间复杂重型烟雾特效 ===

                    // 每 2 帧执行一次（频率加快）
                    if (Main.GameUpdateCount % 1 == 0)
                    {
                        Vector2 forward1 = Projectile.rotation.ToRotationVector2();
                        Vector2 midPos1 = Projectile.Center;

                        // 每次生成 3 个微型烟雾环绕旋转
                        for (int i = 0; i < 3; i++)
                        {
                            float baseAngle = Main.GlobalTimeWrappedHourly * 4f + i * MathHelper.TwoPi / 3f; // 持续旋转
                            Vector2 offsetDir = baseAngle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f); // 环绕距离
                            Vector2 spawnPos = midPos1 + offsetDir;

                            // 微调速度向旋转切线偏移形成螺旋感
                            Vector2 velocity = offsetDir.RotatedBy(MathHelper.PiOver2) * 0.2f + forward1 * -0.5f;

                            Particle smoke = new HeavySmokeParticle(
                                spawnPos,
                                velocity,
                                // 颜色交替深红、黑
                                Main.rand.NextBool() ? new Color(80, 0, 0) * 0.7f : new Color(20, 0, 0) * 0.7f,
                                Main.rand.Next(18, 28), // 寿命稍短
                                Main.rand.NextFloat(0.6f, 0.9f), // 体积更小
                                0.4f,
                                Main.rand.NextFloat(-0.02f, 0.02f),
                                false
                            );
                            GeneralParticleHandler.SpawnParticle(smoke);
                        }
                    }

                }


            }











        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            // 计算场上有效的 Boss 数量，排除蠕虫身体部分
            int bossCount = 2;

            foreach (NPC npc in Main.npc)
            {
                // 检查 NPC 是否是有效的 Boss
                if (npc.active && (npc.boss || npc.type == ModContent.NPCType<SupremeCataclysm>() ||
                                   npc.type == ModContent.NPCType<SupremeCatastrophe>() ||
                                   npc.type == ModContent.NPCType<SoulSeekerSupreme>() ||
                                   npc.type == ModContent.NPCType<BrimstoneHeart>()))
                {
                    // 对于蠕虫类 Boss，仅计入头部部分
                    if (npc.realLife != -1 && npc.whoAmI != npc.realLife)
                        continue;

                    bossCount++;

                    // 特殊处理：增加特定 NPC 计数权重
                    //if (npc.type == ModContent.NPCType<AresBody>())
                    //{
                        //bossCount += 3; // 将 AresBody 视作额外的多个 Boss
                    //}
                }
            }


            // 5% 概率翻倍 Boss 数量并触发五角星特效
            if (Main.rand.NextFloat() < 1f)
            {
                bossCount += 3;

                //// 五角星粒子特效
                //for (int i = 0; i < 5; i++)
                //{
                //    float angle = MathHelper.Pi * 1.5f - i * MathHelper.TwoPi / 5f;
                //    float nextAngle = MathHelper.Pi * 1.5f - (i + 2) * MathHelper.TwoPi / 5f;
                //    Vector2 start = angle.ToRotationVector2();
                //    Vector2 end = nextAngle.ToRotationVector2();
                //    for (int j = 0; j < 40; j++)
                //    {
                //        Dust starDust = Dust.NewDustPerfect(Projectile.Center, 267);
                //        starDust.scale = 2.5f;
                //        starDust.velocity = Vector2.Lerp(start, end, j / 40f) * 16f;
                //        starDust.color = Color.Crimson;
                //        starDust.noGravity = true;
                //    }
                //}
            }

            // 发射弹幕，随机方向，每个方向间隔 360 度 / bossCount
            for (int i = 0; i < bossCount; i++)
            {
                // 随机选择一个 360 度的方向
                float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 randomDirection = randomAngle.ToRotationVector2();

                // 从玩家位置发射弹幕
                Projectile.NewProjectile(
                    Owner.GetSource_FromThis(),
                    Owner.Center,
                    randomDirection * 12f, // 设置速度
                    ModContent.ProjectileType<ViolenceJavLight>(),
                    (int)(Projectile.damage * 0.333f),
                    0,
                    Main.myPlayer
                );
            }




            {
                // === ViolenceJav 命中特效：强化五角星旋转推进 + 大量血液/火花喷射 + 交叉血雾 ===

                Vector2 hitPosition = target.Center;
                Vector2 forward = Projectile.rotation.ToRotationVector2();

                // 1️⃣ 五角星旋转推进（随机方向、随机距离、随机旋转角度）

                for (int k = 0; k < 3; k++)
                {
                    // 每个五角星随机方向
                    float angleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 randomDir = angleOffset.ToRotationVector2();

                    // 每个五角星随机距离（12f * 10f ~ 24f * 10f）
                    float starDistance = Main.rand.NextFloat(12f * 10f, 24f * 10f);

                    // 每个五角星随机旋转角度
                    float rotationIncrement = Main.rand.NextFloat(MathHelper.TwoPi);

                    Vector2 starPos = hitPosition + randomDir * starDistance;

                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.Pi * 1.5f - i * MathHelper.TwoPi / 5f + rotationIncrement;
                        float nextAngle = MathHelper.Pi * 1.5f - (i + 2) * MathHelper.TwoPi / 5f + rotationIncrement;
                        Vector2 start = angle.ToRotationVector2();
                        Vector2 end = nextAngle.ToRotationVector2();

                        for (int j = 0; j < 30; j++)
                        {
                            Dust starDust = Dust.NewDustPerfect(starPos, 267);
                            starDust.scale = 2.5f;
                            starDust.velocity = Vector2.Lerp(start, end, j / 30f) * 18f;
                            starDust.color = Color.Crimson;
                            starDust.noGravity = true;
                        }
                    }
                }


                // 2️⃣ BloodParticle or Golden Spark based on target type
                if (target.Organic())
                {
                    // 血液喷射（极大量，狂野）
                    int bloodStreams = 20; // 原来 15，提升到 20
                    for (int i = 0; i < bloodStreams; i++)
                    {
                        float angleOffset = Main.rand.NextFloat(-45f, 45f);
                        float baseAngle = MathHelper.PiOver4; // 45°抬高
                        float angle = baseAngle + MathHelper.ToRadians(angleOffset);
                        float speed = Main.rand.NextFloat(10f, 24f);
                        Vector2 bloodVelocity = angle.ToRotationVector2() * speed;

                        var blood = new BloodParticle(hitPosition, bloodVelocity, Main.rand.Next(35, 50), Main.rand.NextFloat(1.0f, 1.6f), Color.DarkRed);
                        GeneralParticleHandler.SpawnParticle(blood);

                        // 多层血液效果（核心/外围）
                        if (Main.rand.NextBool(2))
                        {
                            Vector2 spiralVel = bloodVelocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.7f, 1.3f);
                            var bloodSpiral = new BloodParticle(hitPosition, spiralVel, Main.rand.Next(25, 40), 1.0f, Color.Red);
                            GeneralParticleHandler.SpawnParticle(bloodSpiral);
                        }
                    }
                }
                else
                {
                    // 金色火花爆散（丰富结构）
                    int sparkCount = 40;
                    for (int i = 0; i < sparkCount; i++)
                    {
                        float angleOffset = Main.rand.NextFloat(-35f, 35f);
                        float baseAngle = 0f;
                        float angle = baseAngle + MathHelper.ToRadians(angleOffset);
                        float speed = Main.rand.NextFloat(12f, 28f);
                        Vector2 sparkVelocity = angle.ToRotationVector2() * speed;

                        var spark = new SparkParticle(hitPosition, sparkVelocity, true, Main.rand.Next(25, 40), Main.rand.NextFloat(0.9f, 1.5f), Color.Gold);
                        GeneralParticleHandler.SpawnParticle(spark);

                        // 螺旋感额外火花
                        if (Main.rand.NextBool(3))
                        {
                            Vector2 spiralVel = sparkVelocity.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(0.8f, 1.2f);
                            var spiralSpark = new SparkParticle(hitPosition, spiralVel, true, 30, 1.2f, Color.Orange);
                            GeneralParticleHandler.SpawnParticle(spiralSpark);
                        }
                    }
                }

                // 3️⃣ HeavySmokeParticle 血黑交错矩阵扩散
                Vector2[] offsets = { new Vector2(20f, 0f), new Vector2(0f, 20f), new Vector2(-20f, 0f), new Vector2(0f, -20f) };
                Color[] colors = { new Color(60, 0, 0), new Color(20, 0, 0) };

                for (int i = 0; i < offsets.Length; i++)
                {
                    for (int j = 0; j < 2; j++) // 两层交错
                    {
                        Particle smoke = new HeavySmokeParticle(
                            hitPosition + offsets[i].RotatedBy(MathHelper.PiOver4 * j),
                            offsets[i].RotatedBy(MathHelper.PiOver4 * j) * 0.15f,
                            colors[j] * 0.9f,
                            Main.rand.Next(30, 45),
                            Main.rand.NextFloat(1.2f, 1.8f),
                            0.4f,
                            Main.rand.NextFloat(-0.02f, 0.02f),
                            false
                        );
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                }

            }




        }

        public override void OnKill(int timeLeft)
        {
            Vector2 forward = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
            Vector2 center = Projectile.Center;

            int layers = 5; // 阶梯层数
            float angleSpread = MathHelper.ToRadians(45f);

            for (int layer = 0; layer < layers; layer++)
            {
                float layerProgress = layer / (float)(layers - 1); // 0~1
                float speedBase = MathHelper.Lerp(4f, 20f, layerProgress); // 越后层速度越快
                float distanceOffset = MathHelper.Lerp(0f, 60f, layerProgress); // 阶梯前移
                float scaleBase = MathHelper.Lerp(0.6f, 1.4f, layerProgress); // 越后层越大
                int particlePerLayer = 6 + layer * 2; // 每层粒子数量递增

                for (int i = 0; i < particlePerLayer; i++)
                {
                    float angleOffset = MathHelper.Lerp(-angleSpread, angleSpread, i / (float)(particlePerLayer - 1));
                    Vector2 dir = forward.RotatedBy(angleOffset);

                    // 计算偏移位置（形成阶梯喷射形态）
                    Vector2 spawnPos1 = center + dir * distanceOffset;

                    // 4️⃣ HeavySmokeParticle（血雾缓慢弥散）
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 smokeVel = dir.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * (speedBase * 0.4f);
                        Particle smoke = new HeavySmokeParticle(
                            spawnPos1,
                            smokeVel,
                            new Color(60, 0, 0) * 0.8f,
                            30 + layer * 5,
                            scaleBase * Main.rand.NextFloat(0.8f, 1.2f),
                            0.4f,
                            Main.rand.NextFloat(-0.02f, 0.02f),
                            false
                        );
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                }


            }



            // 1️⃣ 拼出完整“简易骷髅头”结构（由多个 DesertProwlerSkullParticle 组成）
            // 骷髅轮廓点相对坐标（简易圆+眼洞）
            Vector2[] skullPoints = new Vector2[]
            {
    // 外圆（8点）
    new Vector2(0f, -12f),
    new Vector2(10f, -8f),
    new Vector2(12f, 0f),
    new Vector2(10f, 8f),
    new Vector2(0f, 12f),
    new Vector2(-10f, 8f),
    new Vector2(-12f, 0f),
    new Vector2(-10f, -8f),

    // 左眼
    new Vector2(-4f, -4f),
    new Vector2(-4f, 0f),
    new Vector2(-4f, 4f),

    // 右眼
    new Vector2(4f, -4f),
    new Vector2(4f, 0f),
    new Vector2(4f, 4f),

    // 鼻子
    new Vector2(0f, 2f),
            };

            foreach (Vector2 offset in skullPoints)
            {
                // 让整体旋转以保持动态
                Vector2 rotatedOffset = offset.RotatedBy(Main.GlobalTimeWrappedHourly * 2f);

                // 构建粒子
                Particle skull = new DesertProwlerSkullParticle(
                    Projectile.Center + rotatedOffset * 1f, // 放大 skull 模板
                    rotatedOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.5f),
                    Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.3f, 0.7f)) * 0.7f,
                    Color.Lerp(Color.Red, Color.OrangeRed, Main.rand.NextFloat(0.2f, 0.6f)),
                    Main.rand.NextFloat(0.8f, 1.2f),
                    60 // 较短寿命保证瞬时爆发
                );
                GeneralParticleHandler.SpawnParticle(skull);
            }






            // === ViolenceJav 死亡：Spark + Dust 双曲螺旋抛物线喷射特效（修正命名） ===

            Vector2 sparkSpawnPos = Projectile.Center;
            Vector2 forwardDir_Death = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();

            int sparkLayers_Death = 5;             // 层数（已重命名）
            int sparksPerLayer = 18;               // 每层射线数
            float sparkBaseAngle = MathHelper.PiOver2; // 基础方向
            float sparkAngleSpread = MathHelper.ToRadians(90f); // 扩散范围

            for (int layer = 0; layer < sparkLayers_Death; layer++)
            {
                float layerProgress = layer / (float)(sparkLayers_Death - 1);
                float baseSpeed = MathHelper.Lerp(16f, 48f, layerProgress);
                float scaleBase = MathHelper.Lerp(0.8f, 1.4f, layerProgress);

                for (int i = 0; i < sparksPerLayer; i++)
                {
                    float normalized = i / (float)(sparksPerLayer - 1); // 0~1
                    float x = normalized * 2f - 1f; // -1 ~ 1

                    // 使用双曲正切控制速度：中间慢，两边快
                    float speedWeight = (float)Math.Abs(Math.Tanh(x * 2.5)); // 中间 ~0，两侧 ~0.98
                    float finalSpeed = baseSpeed * (0.3f + speedWeight * 0.7f) + Main.rand.NextFloat(1f, 3f);

                    // 正弦扰动形成螺旋波动
                    float angleOffset = MathHelper.Lerp(-sparkAngleSpread / 2f, sparkAngleSpread / 2f, normalized);
                    float angleDisturb = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f + x * MathHelper.Pi) * MathHelper.ToRadians(10f);

                    float angle = sparkBaseAngle + angleOffset + angleDisturb;
                    Vector2 dir = forwardDir_Death.RotatedBy(angleOffset + angleDisturb);

                    // SparkParticle
                    Particle spark = new SparkParticle(
                        sparkSpawnPos,
                        dir * finalSpeed,
                        false,
                        40 + layer * 5,
                        scaleBase * Main.rand.NextFloat(0.9f, 1.3f),
                        Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat(0.2f, 0.6f))
                    );
                    GeneralParticleHandler.SpawnParticle(spark);

                    // Dust 同步生成
                    Dust dust = Dust.NewDustPerfect(
                        sparkSpawnPos,
                        DustID.Blood,
                        dir * finalSpeed * Main.rand.NextFloat(0.8f, 1.2f),
                        100,
                        Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat(0.3f, 0.7f)),
                        scaleBase * Main.rand.NextFloat(0.9f, 1.3f)
                    );
                    dust.noGravity = true;
                }
            }








        }




        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SCalSounds/SCalDash"));
        }




    }
}
