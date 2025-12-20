using CalamityMod;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetLeft : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            Texture2D texture = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/BForget/SunsetBForgetLeft"
            ).Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);
            Vector2 origin = texture.Size() * 0.5f;

            // ================================
            // 1. 主题色描边（干净、柔光）
            // ================================
            Color outlineColor = new Color(120, 220, 255) * 0.65f; // 淡蓝青，适合梦境风
            for (int i = 0; i < 4; i++)
            {
                Vector2 off = (MathHelper.PiOver2 * i).ToRotationVector2() * 3f;
                Main.EntitySpriteDraw(texture, drawPos + off, null, outlineColor, Projectile.rotation,
                    origin, Projectile.scale, SpriteEffects.None, 0);
            }

            // ================================
            // 2. 绘制本体（保持清晰）
            // ================================
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            // ================================
            // 3. 椭圆 + 正弦抖动环绕光点
            // ================================
            //Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[98].Value;


            float time = Main.GlobalTimeWrappedHourly * 2.4f;
            float a = 18f;
            float b = 10f;
            int count = 6;
            float wobbleAmp = 3.6f;
            float wobbleSpeed = 3.2f;

            for (int i = 0; i < count; i++)
            {
                float t = time + i * MathHelper.TwoPi / count;

                // 椭圆轨道
                Vector2 ellipsePos = new Vector2(
                    (float)Math.Cos(t) * a,
                    (float)Math.Sin(t) * b
                );

                // 正弦抖动
                float wobble = (float)Math.Sin(t * wobbleSpeed) * wobbleAmp;
                Vector2 wobbleOffset = new Vector2(
                    (float)Math.Cos(t + MathHelper.PiOver2),
                    (float)Math.Sin(t + MathHelper.PiOver2)
                ) * wobble;

                Vector2 finalPos = Projectile.Center + ellipsePos + wobbleOffset;
                finalPos -= Main.screenPosition;

                Color glowColor = new Color(150, 240, 255) * (0.8f * (1f - Projectile.alpha / 255f));
                float scale = 1.0f + 0.15f * (float)Math.Sin(t * 1.7f + i);

                Main.EntitySpriteDraw(
                    texture,            // ✔ 使用本体贴图
                    finalPos,
                    null,
                    glowColor,
                    Projectile.rotation,   // ⭐ 关键修改：朝向跟本体完全一致
                    texture.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }



            // 我们处理了所有绘制
            return false;
        }



        public static class SunsetBForgetParticleManager
        {
            public static readonly int[] YellowDusts = { 169, 159, 133 };
            public static readonly int[] BlueDusts = { 80, 67, 48 }; 
            public static readonly int[] GreenDusts = { 3, 46, 89, 128 };
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }
        private bool killedByRightCheck = false;
        private bool firstHitTriggered = false;

        public override void OnSpawn(IEntitySource source)
        {
            bool hasRight = Main.projectile.Any(p =>
                p.active &&
                p.owner == Projectile.owner &&
                p.type == ModContent.ProjectileType<SunsetBForgetRight>()
            );

            if (hasRight)
            {
                killedByRightCheck = true; // 标记：是因为检测到 Right 而消失
                Projectile.Kill();
                return;
            }

            Projectile.velocity *= 1.5f;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SunsetPlayerSpeed.ApplyNoArmorHypothesisHitEffect(
                Projectile,
                target,
                ref modifiers
            );
        }


        public override void AI()
        {
            // ===== 统一朝向（保持你的原始逻辑）=====
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ===== 前 30 帧可穿墙，之后恢复实体碰撞 =====
            if (Projectile.timeLeft > 135)
                Projectile.tileCollide = false;
            else
                Projectile.tileCollide = true;





            // ========== 击中第一次后的减速与透明化 ==========
            if (firstHitTriggered)
            {
                // 强力减速
                Projectile.velocity *= 0.94f;

                // 逐渐变透明（0 为完全亮，255 完全透明）
                Projectile.alpha += 12;
                if (Projectile.alpha > 255)
                    Projectile.alpha = 255;
            }


            // ===== 先调用外包：蓝/紫孢子 + 正弦信号波纹（持续释放）=====
            CalamityThrowingSpear.CTSLightingBoltsSystem.Spawn_PlantTechSporeTrail(
                Projectile.Center,
                Projectile.velocity,
                1.0f // 全局强度，可改 0.8f/1.2f 做微调
            );

            // ===== 以下是你原有的飞行特效 —— “存在感削弱 50% + 更优雅参数” =====
            // 通过统一衰减因子 fx 对所有参数做半幅处理，同时降低出现概率
            float fx = 0.5f;                 // 统一强度衰减：50%
            float t = (float)Main.GameUpdateCount * 0.06f;

            // —— 原配置的“基参数”，在此做幅度/寿命等统一缩放 —— //
            float coreTrailScale = 2.0f * fx;                     // 2.0 → 1.0
            int coreTrailLife = (int)(42 * fx);                // 42 → 21
            float sideWaveAmp = 10f * fx;                      // 10 → 5
            float sideWaveFreq = 0.28f;                         // 频率保留
            int sideWaveLife = (int)(24 * fx);                // 24 → 12
            float ringRadius = 14f * fx;                      // 14 → 7
            float ringSpeed = 2.6f * fx;                     // 2.6 → 1.3
            int ringCount = Math.Max(3, (int)(6 * fx));    // 6 → 3
            int ringEveryFrames = (int)(10 / fx);                // 10 → 20（更稀疏）
            float squareScale = 1.8f * fx;                     // 1.8 → 0.9
            int squareLife = (int)(26 * fx);                // 26 → 13
            int mistLifeMin = (int)(16 * fx);                // 16 → 8
            int mistLifeMax = (int)(22 * fx);                // 22 → 11
            float smokeScaleMin = 0.28f * fx;                    // 0.28 → 0.14
            float smokeScaleMax = 0.6f * fx;                     // 0.6 → 0.3

            // 方向、角度
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            float forwardAngle = forward.ToRotation();

            // =========================================================
            // A. 主干能量束（线性粒子）—— 出现概率也减半
            // =========================================================
            if (Main.rand.NextBool(4)) // 原来 1/2，现在 1/4
            {
                Particle core = new SparkParticle(
                    Projectile.Center,
                    forward * 0.02f,
                    false,
                    coreTrailLife,
                    coreTrailScale,
                    Color.Lerp(Color.DeepSkyBlue, Color.White, 0.15f) // 稍微偏白，显“能量”
                );
                core.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(core);
            }

            // =========================================================
            // B. 双股侧藤（正弦波形）—— 幅度/寿命减半，更优雅
            // =========================================================
            if (Main.rand.NextBool(3)) // 原来每帧，现在 1/3 帧
            {
                for (int i = 0; i < 2; i++)
                {
                    float sideSign = (i == 0) ? +1f : -1f;
                    float wave = (float)Math.Sin(t * (1f + 0.35f * Projectile.whoAmI) + sideSign * MathHelper.PiOver2);
                    Vector2 lateral = forward.RotatedBy(MathHelper.PiOver2) * (sideWaveAmp * wave);

                    var waveLine = new AltSparkParticle(
                        Projectile.Center + lateral,
                        forward * 0.02f,
                        false,
                        sideWaveLife,
                        1.6f * fx, // 粗细也降一点
                        new Color(120, 200, 255)
                    );
                    waveLine.Rotation = forwardAngle;
                    GeneralParticleHandler.SpawnParticle(waveLine);
                }
            }

            // =========================================================
            // C. 科技碎片（方块）—— 数量/寿命/尺度下降
            // =========================================================
            if (Main.rand.NextBool(6)) // 原 1/3 → 1/6
            {
                const float golden = 2.39996323f;
                float k = (Projectile.whoAmI * 1.618f + Main.GameUpdateCount * 0.15f) % 17;
                float ang = golden * k;
                Vector2 radial = ang.ToRotationVector2() * Main.rand.NextFloat(3f, 9f); // 半径也降一点

                var sq = new SquareParticle(
                    Projectile.Center + radial,
                    forward * Main.rand.NextFloat(0.4f, 1.0f),
                    false,
                    squareLife,
                    squareScale,
                    new Color(100, 180, 255)
                );
                sq.Rotation = forwardAngle + Main.rand.NextFloat(-0.2f, 0.2f);
                GeneralParticleHandler.SpawnParticle(sq);
            }

            // =========================================================
            // D. 螺旋小环 —— 更稀疏（帧间隔加倍），半径/速度更小
            // =========================================================
            if (Main.GameUpdateCount % ringEveryFrames == 0)
            {
                float baseRot = t * 1.2f;
                for (int j = 0; j < ringCount; j++)
                {
                    float angle = baseRot + MathHelper.TwoPi * j / ringCount;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * ringRadius;
                    Vector2 vel = angle.ToRotationVector2() * ringSpeed * Main.rand.NextFloat(0.85f, 1.1f);

                    var sp = new SparkParticle(pos, vel, false, 18, 0.9f * fx, new Color(110, 190, 255));
                    sp.Rotation = angle;
                    GeneralParticleHandler.SpawnParticle(sp);
                }
            }

            // =========================================================
            // E. 背景能雾 —— 频率减半，颜色调淡
            // =========================================================
            if (Main.rand.NextBool(6)) // 原 1/3 → 1/6
            {
                var mist = new WaterFlavoredParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    forward.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.3f, 0.7f),
                    false,
                    Main.rand.Next(mistLifeMin, mistLifeMax),
                    1.0f + Main.rand.NextFloat(0.2f),
                    new Color(120, 180, 255) * 0.75f
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }

            // =========================================================
            // F. 深色烟 —— 稀有（原 1/6 → 1/12），规模缩小
            // =========================================================
            if (Main.rand.NextBool(12))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    forward.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.4f, 0.9f),
                    new Color(40, 60, 90),
                    18,
                    Main.rand.NextFloat(smokeScaleMin, smokeScaleMax),
                    0.45f,
                    Main.rand.NextFloat(-1f, 1f),
                    true
                );
                smoke.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // =========================================================
            // G. 轻点尘 —— 频率减半
            // =========================================================
            if (Main.rand.NextBool(8)) // 原 1/4 → 1/8
            {
                int dustType = Main.rand.NextBool() ? DustID.DungeonWater : DustID.BlueTorch;
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType,
                    forward.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.6f, 1.2f),
                    120,
                    new Color(120, 200, 255),
                    Main.rand.NextFloat(1.0f, 1.3f)
                );
                d.noGravity = true;
                d.rotation = forwardAngle;
                d.fadeIn = 0.7f;
            }
        }





        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center;

            // ========== 1. 椭圆冲击波（主冲击波） ==========
            {
                float rot = Projectile.rotation;
                Particle pulse = new DirectionalPulseRing(
                    center,
                    Projectile.velocity.SafeNormalize(Vector2.UnitX) * 2.1f,        // 有明显方向性的冲击
                    new Color(100, 255, 200),                                     // 蓝绿梦境色
                    new Vector2(1.2f, 3.2f),                                      // 椭圆比例（数学椭圆 a,b）
                    rot - MathHelper.PiOver4,                                     // 椭圆朝向
                    0.24f,                                                        // 初始 scale
                    0.045f,                                                       // 最终 scale（外扩速度）
                    28                                                            // lifetime
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // ========== 2. 高能 EXO 光喷射（喷射特效） ==========  
            int exoCount = 18; // 建议 6~10 个
            for (int i = 0; i < exoCount; i++)
            {
                float angle = Projectile.rotation - MathHelper.PiOver4 + Main.rand.NextFloat(-0.7f, 0.7f); // ±40° 扩散
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3.4f, 6.2f);

                SquishyLightParticle exo = new SquishyLightParticle(
                    center,
                    vel,
                    0.33f,                                                         // scale（大且亮）
                    Main.rand.NextBool() ? Color.Cyan : Color.White,              // 青蓝/白色交错（梦境能量）
                    32,                                                           // lifetime
                    opacity: 1f,
                    squishStrenght: 1.2f,
                    maxSquish: 3.2f,
                    hueShift: Main.rand.NextFloat(-0.04f, 0.04f)                  // 微小色相漂移
                );
                GeneralParticleHandler.SpawnParticle(exo);
            }

            // ========== 3. 数学方块碎片（几何能量碎裂） ==========
            int squareCount = 5;
            for (int i = 0; i < squareCount; i++)
            {
                float angle = MathHelper.TwoPi * i / squareCount + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.2f, 3f);

                SquareParticle sq = new SquareParticle(
                    center,
                    vel,
                    false,
                    32 + Main.rand.Next(12),
                    1.4f + Main.rand.NextFloat(0.6f),
                    Color.Cyan * 1.3f
                );
                GeneralParticleHandler.SpawnParticle(sq);
            }

            // ====================================================================
            // ========== 4. 数学魔法阵（GlowOrb ★ 玫瑰曲线 + 旋转相位） ==========
            // ====================================================================
            //
            // 使用 6 个 GlowOrb 点：
            //   - 构成正六边形
            //   - 再叠加玫瑰曲线 r = sin(3θ)
            //   - 叠加旋转相位 offset
            //   - 形成完美青绿梦境法阵
            //
            // ====================================================================

            int nodes = 6;
            float baseR = 36f;                     // 六边形半径
            float phase = Main.rand.NextFloat();   // 随机旋转相位（确保每次不同）
            float roseAmp = 9f;                    // 玫瑰曲线振幅

            for (int i = 0; i < nodes; i++)
            {
                float theta = MathHelper.TwoPi * i / nodes + phase;

                // 六边形 + 玫瑰曲线（r = baseR + A*sin(3θ)）
                float r = baseR + roseAmp * (float)Math.Sin(3f * theta);

                Vector2 pos = center + theta.ToRotationVector2() * r;

                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    7,                              // lifetime: 非常短暂，但亮
                    0.9f,                            // scale
                    new Color(120, 255, 235),        // 蓝绿梦境色
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }

            // ========== 结束 ==========
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 第一次命中后触发减速与消散
            if (!firstHitTriggered)
            {
                firstHitTriggered = true;
                Projectile.timeLeft = 20;
                Projectile.friendly = false;
                Projectile.tileCollide = false;
            }

            // 施加 Debuff 给敌人
            target.AddBuff(ModContent.BuffType<SunsetBForgetEDebuff>(), 300); // 5 秒

            // 施加 Buff 给玩家
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetBForgetPBuff>(), 300); // 5 秒

            DoSB(target);
        }


        private void DoSB(NPC hitNPC)
        {
            if (killedByRightCheck)
            {
                // 如果是检测到 Right 弹幕后自杀，不执行任何额外逻辑
                return;
            }

            Vector2 spawnPosition = Projectile.Center;

            // 计算随机触手数量（3~6个）
            int tentacleCount = Main.rand.Next(3, 7);

            // 计算单个触手的伤害（总伤害固定为 1.0 倍）
            int individualDamage = (int)(Projectile.damage / (float)tentacleCount);

            // ============ 原本的随机触手 ============

            for (int i = 0; i < tentacleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi); // 0°～360° 全角度随机
                Vector2 tentacleVelocity = randomAngle.ToRotationVector2() * 4f;

                SpawnGreenTentacle(tentacleVelocity, individualDamage);
            }

            // 播放撞击音效
            SoundEngine.PlaySound(SoundID.Item74, Projectile.position);

            // ============ 新逻辑：附近敌人额外弹幕扩散 ============

            const float searchRadius = 5f * 16f; // 5×16 半径
            List<NPC> extraTargets = new List<NPC>();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                // ★ 把“本次被击中的那一只”排除掉
                if (hitNPC != null && npc.whoAmI == hitNPC.whoAmI)
                    continue;

                // 只要在范围内就作为候选
                if (Vector2.Distance(npc.Center, Projectile.Center) <= searchRadius)
                    extraTargets.Add(npc);
            }

            if (extraTargets.Count > 0)
            {
                // 最多锁敌 2 个，按距离由近到远
                extraTargets = extraTargets
                    .OrderBy(n => Vector2.Distance(n.Center, Projectile.Center))
                    .Take(2)
                    .ToList();

                foreach (NPC npc in extraTargets)
                {
                    // 在这些额外目标身上，各自随机方向再长出 2 条触手
                    for (int i = 0; i < 2; i++)
                    {
                        float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                        Vector2 vel = angle.ToRotationVector2() * 4f;

                        SpawnGreenTentacle(vel, individualDamage);
                    }
                }
            }

            // ============ 保留你原来后面的特效部分不动 ============

            {
                CalamityThrowingSpear.CTSLightingBoltsSystem.Spawn_PlantScatterBurst(spawnPosition, 22, 7f);

                // ===================================================
                // ② Calamity 粒子：伞状喷射
                // ===================================================
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                float forwardAngle = forward.ToRotation();

                int rays = 8; // 基础射线数量
                float cone = MathHelper.ToRadians(40f); // ±40°扇形

                for (int i = 0; i < rays; i++)
                {
                    float offset = MathHelper.Lerp(-cone, cone, i / (float)(rays - 1));
                    Vector2 dir = forward.RotatedBy(offset);
                    float speed = Main.rand.NextFloat(3.5f, 6.5f);

                    // --- 能量火花 ---
                    Particle spark = new SparkParticle(
                        spawnPosition,
                        dir * speed,
                        false,
                        30,
                        Main.rand.NextFloat(1.2f, 1.8f),
                        Color.Lerp(Color.DeepSkyBlue, Color.MediumPurple, Main.rand.NextFloat())
                    );
                    spark.Rotation = dir.ToRotation();
                    GeneralParticleHandler.SpawnParticle(spark);

                    // --- 侧向波纹 ---
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 sidePos = spawnPosition + dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-4f, 4f);
                        Particle wave = new AltSparkParticle(
                            sidePos,
                            dir * (speed * 0.8f),
                            false,
                            24,
                            1.0f,
                            new Color(100, 200, 255)
                        );
                        wave.Rotation = dir.ToRotation();
                        GeneralParticleHandler.SpawnParticle(wave);
                    }

                    // --- 方块碎片 ---
                    if (Main.rand.NextBool(3))
                    {
                        Particle sq = new SquareParticle(
                            spawnPosition + dir * Main.rand.NextFloat(4f, 12f),
                            dir * (speed * 0.6f),
                            false,
                            18,
                            1.0f,
                            new Color(120, 180, 255)
                        );
                        sq.Rotation = dir.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                        GeneralParticleHandler.SpawnParticle(sq);
                    }
                }
            }
        }


        // 生成绿色触手的方法
        private void SpawnGreenTentacle(Vector2 tentacleVelocity, int damage)
        {
            float kb = Projectile.knockBack;

            float ai0 = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? -1f : 1f);
            float ai1 = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? -1f : 1f);

            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, tentacleVelocity, ModContent.ProjectileType<SunsetBForgetTantacle>(), damage, kb, Projectile.owner, ai0, ai1);
        }


    }
}