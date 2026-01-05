using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    internal class SuperSOD : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";




        public Player Owner => Main.player[Projectile.owner];

        // 阶段：0 = 减速+锁定，1 = 机枪喷星
        // ai[0] = state
        // ai[1] = timer
        // ai[2] = 消耗的时刻数量（由上级传入）

        private int totalShots;   // 6 + 时刻数量
        private int shotsFired;   // 已经打出的星弹数量
        private float outlinePower; // 描边强度 0~1
        private NPC currentTarget;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.ArmorPenetration = 60;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 记录总射击数：6 + 时刻数量
            int momentCount = (int)Projectile.ai[2];
            if (momentCount < 0)
                momentCount = 0;

            totalShots = 6 + momentCount;
            shotsFired = 0;

            Projectile.rotation = Projectile.velocity.ToRotation();
            outlinePower = 0f;
        }
        private float trueRotation; // 不带45度的“真实角度”

        public override void AI()
        {
            ref float state = ref Projectile.ai[0]; // 0=减速锁定, 1=机枪, 2=自毁冲锋
            ref float timer = ref Projectile.ai[1];

            // 每帧轻微减速，让整体轨迹更稳一点
            Projectile.velocity *= 0.935f;

            // 锁敌（优先 Boss）
            currentTarget = FindBestTarget(Owner, Projectile.Center, 5500f);

            // ======================================================
            // 1. 计算基础真实方向 trueRotation（不带 45° 偏移）
            // ======================================================
            if (Projectile.velocity.Length() > 0.1f)
            {
                float baseRot = Projectile.velocity.ToRotation();
                float targetRot = baseRot;

                if (currentTarget != null && currentTarget.active)
                {
                    Vector2 toTarget = currentTarget.Center - Projectile.Center;
                    if (toTarget.LengthSquared() > 0.001f)
                        targetRot = toTarget.ToRotation();
                }

                // 不同阶段给不同的最大转向速度
                float maxTurn =
                    state == 2f ? MathHelper.ToRadians(14f) : // 自毁冲锋阶段转得更猛
                    state == 1f ? MathHelper.ToRadians(8f) : // 机枪阶段中等
                                  MathHelper.ToRadians(10f);  // 减速阶段稍快一点

                float diff = MathHelper.WrapAngle(targetRot - trueRotation);
                diff = MathHelper.Clamp(diff, -maxTurn, maxTurn);
                trueRotation += diff;
            }

            // 最终显示用 rotation = 真实角度 + 45°
            Projectile.rotation = trueRotation + MathHelper.PiOver4;

            // ======================================================
            // 2. 阶段逻辑
            // ======================================================
            if (state == 0f)
            {
                // =======================
                // 阶段 0：减速 + 锁定方向
                // =======================
                timer++;

                float t = MathHelper.Clamp(timer / 10f, 0f, 1f); // 10 帧内从 0→1
                outlinePower = t;

                if (timer >= 10f)
                {
                    // 进入机枪阶段
                    timer = 0f;
                    state = 1f;
                }
            }
            else if (state == 1f)
            {
                // =======================
                // 阶段 1：机枪喷射 LSTAR
                // =======================
                timer++;
                outlinePower = 1f;

                // 每 2 帧打一发，直到达到上限
                if (shotsFired < totalShots && timer % 2f == 0f)
                {
                    FireStarBurst();
                    shotsFired++;
                }

                // 星弹打完后短暂停留一下，随后进入自毁冲锋
                if (shotsFired >= totalShots && timer > 20f)
                {
                    timer = 0f;
                    state = 2f;

                    // 初始冲锋速度：指向当前目标；没有目标就沿当前朝向
                    Vector2 dashDir;
                    if (currentTarget != null && currentTarget.active)
                        dashDir = (currentTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    else
                        dashDir = trueRotation.ToRotationVector2();

                    Projectile.velocity = dashDir * 26f;
                }
            }
            else // state == 2f
            {
                // =======================
                // 阶段 2：自毁冲锋
                // =======================
                timer++;
                outlinePower = 1f;

                // 略微加速，冲锋感更强
                Projectile.velocity *= 1.12f;

                // 有目标就持续微调方向（已经在上面的 trueRotation 里做了转向）
                // 这里只需在接近目标或时间到时自杀
                if (currentTarget != null && currentTarget.active)
                {
                    float dist = Vector2.Distance(Projectile.Center, currentTarget.Center);
                    if (dist < 40f) // 接近目标自动引爆
                    {
                        Projectile.Kill();
                        return;
                    }
                }

                // 冲锋超过一定时间也强制自爆
                if (timer > 60f)
                {
                    Projectile.Kill();
                    return;
                }
            }
        }

        private void FireStarBurst()
        {
            if (currentTarget == null || !currentTarget.active)
                return;

            // 基础朝向：指向目标
            Vector2 toTarget = currentTarget.Center - Projectile.Center;
            if (toTarget.LengthSquared() < 0.001f)
                toTarget = Vector2.UnitY;

            float baseAngle = toTarget.ToRotation();

            // 在 ±5° 之间做一点随机偏移
            float spread = MathHelper.ToRadians(5f);
            float offset = Main.rand.NextFloat(-spread, spread);
            Vector2 shootDir = (baseAngle + offset).ToRotationVector2();

            float starSpeed = 34f;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                shootDir * starSpeed,
                ModContent.ProjectileType<StarsofDestinyRLIGHT>(),
                Projectile.damage,
                0f,
                Projectile.owner
            );

            // 机枪音效
            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.9f, PitchVariance = 0.1f }, Projectile.Center);

            // ===== 特效：前方冲击波 + 后方双扇形喷射 =====
            SpawnFrontPulse();
            SpawnBackCones();
        }

        private void SpawnFrontPulse()
        {
            // 正前方椭圆冲击波
            Particle pulse = new DirectionalPulseRing(
                Projectile.Center,
                Projectile.velocity.SafeNormalize(Vector2.UnitY) * 8f,
                Color.Lerp(new Color(255, 240, 150), new Color(120, 255, 220), 0.4f),
                new Vector2(1f, 2.5f),
                Projectile.rotation - MathHelper.PiOver4,
                0.20f,
                0.03f,
                20
            );
            GeneralParticleHandler.SpawnParticle(pulse);
        }

        private void SpawnBackCones()
        {
            Vector2 origin = Projectile.Center;

            // 后方两侧：±135° 方向的扇形
            float coneCenter1 = Projectile.rotation + MathHelper.ToRadians(135f);
            float coneCenter2 = Projectile.rotation - MathHelper.ToRadians(135f);
            float halfAngle = MathHelper.ToRadians(20f); // 稍微再宽一点

            Color exo1 = new Color(255, 240, 140);
            Color exo2 = new Color(130, 255, 200);

            // === 每边 7 条主射线，每条再叠 2 个 EXO + 若干水雾 ===
            for (int side = 0; side < 2; side++)
            {
                float center = side == 0 ? coneCenter1 : coneCenter2;
                Color baseColor = side == 0 ? exo1 : exo2;

                int rays = 7; // 原来是 3，直接提到 7
                for (int i = 0; i < rays; i++)
                {
                    float t = (i + 0.5f) / rays; // 0~1
                    float ang = center + MathHelper.Lerp(-halfAngle, halfAngle, t);
                    Vector2 dir = ang.ToRotationVector2();

                    // ---- 2 个层次的 EXO 光粒子（速度不同，数量×2）----
                    for (int k = 0; k < 2; k++)
                    {
                        float speed = Main.rand.NextFloat(5f + 2f * k, 10f + 3f * k); // 第二层更快更远
                        float life = Main.rand.Next(20 + 2 * k, 30 + 4 * k);
                        float scale = Main.rand.NextFloat(0.24f + 0.03f * k, 0.32f + 0.04f * k);

                        SquishyLightParticle exo = new SquishyLightParticle(
                            origin,
                            dir * speed,
                            scale,
                            baseColor,
                            (int)life,
                            opacity: 1f,
                            squishStrenght: 1f,
                            maxSquish: Main.rand.NextFloat(2.2f, 3.4f),
                            hueShift: 0f
                        );
                        GeneralParticleHandler.SpawnParticle(exo);
                    }

                    // ---- 水雾：每条射线再来 1~2 个，填充体积感 ----
                    int mistCount = Main.rand.Next(5, 9);
                    for (int m = 0; m < mistCount; m++)
                    {
                        float mistSpeed = Main.rand.NextFloat(2.0f, 4.0f);
                        float mistLife = Main.rand.Next(18, 26);
                        float mistScale = 1.0f + Main.rand.NextFloat(0.4f);

                        WaterFlavoredParticle mist = new WaterFlavoredParticle(
                            origin,
                            dir * mistSpeed,
                            false,
                            (int)mistLife,
                            mistScale,
                            baseColor * 0.7f
                        );
                        GeneralParticleHandler.SpawnParticle(mist);
                    }
                }
            }
        }


        public override void OnKill(int timeLeft)
        {
            Vector2 center = Projectile.Center;

            // 颜色定义：金黄 + 青绿
            Color gold = new Color(255, 235, 140);
            Color teal = new Color(130, 255, 220);
            Color mix = Color.Lerp(gold, teal, 0.5f);

            // 爆炸音效（偏科幻）
            SoundEngine.PlaySound(SoundID.Item74 with
            {
                Volume = 1.1f,
                PitchVariance = 0.2f
            }, center);

            // ============================
            // 1）核心 Bloom + 多重光环（中心权威）
            // ============================
            GenericBloom core = new GenericBloom(
                center,
                Vector2.Zero,
                mix,
                2.2f,
                28
            );
            GeneralParticleHandler.SpawnParticle(core);

            BloomRing inner = new BloomRing(
                center,
                Vector2.Zero,
                gold * 1.1f,
                2.8f,
                40
            );
            GeneralParticleHandler.SpawnParticle(inner);

            BloomRing mid = new BloomRing(
                center,
                Vector2.Zero,
                teal * 0.95f,
                3.8f,
                48
            );
            GeneralParticleHandler.SpawnParticle(mid);

            BloomRing outer = new BloomRing(
                center,
                Vector2.Zero,
                mix * 0.7f,
                5.0f,
                56
            );
            GeneralParticleHandler.SpawnParticle(outer);

            // ============================
            // 2）八向冲击波（十字 + 对角，双层）
            // ============================
            for (int i = 0; i < 8; i++)
            {
                float ang = MathHelper.PiOver4 * i;
                Vector2 dir = ang.ToRotationVector2();

                float speed = Main.rand.NextFloat(9f, 14f);
                float life = Main.rand.Next(22, 30);

                Particle pulse = new DirectionalPulseRing(
                    center,
                    dir * speed,
                    Color.Lerp(gold, teal, 0.3f + 0.1f * (float)Math.Sin(i)),
                    new Vector2(1.1f, 2.8f),
                    ang - MathHelper.PiOver4,
                    0.24f,
                    0.03f,
                    (int)life
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // ============================
            // 3）EXO 光线：内层 + 外层（有序+无序）
            // ============================
            int innerRays = 24;
            int outerRays = 24;

            // 内层：短、密集
            for (int i = 0; i < innerRays; i++)
            {
                float t = i / (float)innerRays;
                float baseAng = MathHelper.TwoPi * t;
                float jitter = Main.rand.NextFloat(-0.10f, 0.10f);
                float ang = baseAng + jitter;

                Vector2 dir = ang.ToRotationVector2();
                Color c = Color.Lerp(gold, teal, 0.2f + 0.4f * t);

                SquishyLightParticle exo = new SquishyLightParticle(
                    center,
                    dir * Main.rand.NextFloat(6f, 9f),
                    Main.rand.NextFloat(0.22f, 0.30f),
                    c,
                    Main.rand.Next(24, 30),
                    opacity: 1f,
                    squishStrenght: 1.1f,
                    maxSquish: Main.rand.NextFloat(2.4f, 3.0f),
                    hueShift: 0f
                );
                GeneralParticleHandler.SpawnParticle(exo);
            }

            // 外层：长、发散
            for (int i = 0; i < outerRays; i++)
            {
                float t = i / (float)outerRays;
                float baseAng = MathHelper.TwoPi * t;
                float jitter = Main.rand.NextFloat(-0.18f, 0.18f);
                float ang = baseAng + jitter;

                Vector2 dir = ang.ToRotationVector2();
                Color c = Color.Lerp(teal, gold, 0.3f + 0.6f * t);

                SquishyLightParticle exo = new SquishyLightParticle(
                    center,
                    dir * Main.rand.NextFloat(10f, 16f),
                    Main.rand.NextFloat(0.26f, 0.34f),
                    c,
                    Main.rand.Next(30, 38),
                    opacity: 1f,
                    squishStrenght: 1.2f,
                    maxSquish: Main.rand.NextFloat(2.8f, 3.6f),
                    hueShift: 0f
                );
                GeneralParticleHandler.SpawnParticle(exo);
            }

            // ============================
            // 4）GlowOrb 双螺旋带（数学螺线）
            // ============================
            int spiralCount = 64;
            for (int arm = 0; arm < 2; arm++)
            {
                float armOffset = (arm == 0) ? 0f : MathHelper.Pi;
                for (int i = 0; i < spiralCount; i++)
                {
                    float t = i / (float)spiralCount;          // 0→1
                    float ang = MathHelper.TwoPi * t * 2f + armOffset;
                    float radius = MathHelper.Lerp(16f, 96f, t); // 更大半径

                    Vector2 offset = ang.ToRotationVector2() * radius;
                    Color c = Color.Lerp(teal, gold, t) * 0.9f;
                    c.A = 0;

                    GlowOrbParticle orb = new GlowOrbParticle(
                        center + offset,
                        Vector2.Zero,
                        false,
                        Main.rand.Next(22, 32),
                        0.7f + 0.5f * (1f - t),
                        c,
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }

            // ============================
            // 5）能量碎片：内圈规则 + 外圈随机
            // ============================
            int innerShard = 32;
            int outerShard = 48;

            // 内圈：规则环，速度中等
            for (int i = 0; i < innerShard; i++)
            {
                float t = i / (float)innerShard;
                float ang = MathHelper.TwoPi * t;
                float speed = MathHelper.Lerp(6f, 10f, (float)Math.Sin(t * MathHelper.Pi));

                Vector2 vel = ang.ToRotationVector2() * speed;
                Color c = Color.Lerp(gold, teal, 0.3f + 0.4f * t);

                PointParticle shard = new PointParticle(
                    center,
                    vel,
                    false,
                    Main.rand.Next(18, 26),
                    1.2f + Main.rand.NextFloat(0.5f),
                    c
                );
                GeneralParticleHandler.SpawnParticle(shard);
            }

            // 外圈：角度扰动更大、速度更快
            for (int i = 0; i < outerShard; i++)
            {
                float t = i / (float)outerShard;

                float baseAng = MathHelper.TwoPi * t;
                float noisy = baseAng + Main.rand.NextFloat(-0.35f, 0.35f);
                float speed = MathHelper.Lerp(9f, 18f, (float)Math.Sin(t * MathHelper.Pi));

                Vector2 vel = noisy.ToRotationVector2() * speed;
                Color c = Main.rand.NextBool() ? gold : teal;

                PointParticle shard = new PointParticle(
                    center,
                    vel,
                    false,
                    Main.rand.Next(22, 30),
                    1.3f + Main.rand.NextFloat(0.6f),
                    c
                );
                GeneralParticleHandler.SpawnParticle(shard);
            }

            // ============================
            // 6）水雾：填充中层体积感
            // ============================
            for (int i = 0; i < 18; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                Color c = Color.Lerp(gold, teal, Main.rand.NextFloat()) * 0.8f;

                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    center,
                    vel,
                    false,
                    Main.rand.Next(26, 36),
                    1.0f + Main.rand.NextFloat(0.5f),
                    c
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }
        }







        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 basePos = Projectile.Center - Main.screenPosition;

            // 主颜色：偏金黄+青色
            Color coreColor = Color.Lerp(new Color(255, 240, 150), new Color(130, 255, 220), 0.5f) * Projectile.Opacity;
            coreColor.A = 0;

            // 1）拖尾
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float t = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;

                Color trail = coreColor * (0.35f * t);
                trail *= outlinePower * 0.8f;
                Main.EntitySpriteDraw(
                    texture,
                    pos,
                    null,
                    trail,
                    Projectile.rotation,
                    origin,
                    Projectile.scale * (0.9f + 0.2f * t),
                    SpriteEffects.None,
                    0
                );
            }

            // 2）描边（随 outlinePower 填充）
            if (outlinePower > 0f)
            {
                Color outline = Color.Lerp(new Color(255, 255, 180), new Color(160, 255, 230), 0.5f) * outlinePower;
                outline.A = 0;

                float radius = 2f + 3f * outlinePower;
                float scale = Projectile.scale * (1f + 0.18f * outlinePower);
                int samples = 8;
                for (int i = 0; i < samples; i++)
                {
                    float ang = MathHelper.TwoPi * i / samples;
                    Vector2 offset = ang.ToRotationVector2() * radius;
                    Main.EntitySpriteDraw(
                        texture,
                        basePos + offset,
                        null,
                        outline,
                        Projectile.rotation,
                        origin,
                        scale,
                        SpriteEffects.None,
                        0
                    );
                }
            }

            // 3）本体
            Main.EntitySpriteDraw(
                texture,
                basePos,
                null,
                coreColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }

        private static NPC FindBestTarget(Player owner, Vector2 center, float maxDistance)
        {
            NPC best = null;
            float bestDist = maxDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy())
                    continue;

                float dist = Vector2.Distance(center, npc.Center);
                if (dist > maxDistance)
                    continue;

                if (best == null)
                {
                    best = npc;
                    bestDist = dist;
                    continue;
                }

                // Boss 优先，其次距离
                if (npc.boss && !best.boss)
                {
                    best = npc;
                    bestDist = dist;
                }
                else if (npc.boss == best.boss && dist < bestDist)
                {
                    best = npc;
                    bestDist = dist;
                }
            }

            return best;
        }
    }
}