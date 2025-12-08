using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestinyRIGHT : ModProjectile
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/StarsofDestiny/StarsofDestinyRIGHT";

        // ====== 阶段控制 ======
        // state = 0：初始减速 + 渐渐透明 + 持续索敌（rotation 不改）
        // state = 1：完全透明的一帧，瞬移到目标背后（rotation 不改）
        // state = 2：停在背后，只旋转瞄准 + 恢复透明度，然后发一次单激光
        // state = 3：追踪阶段（位置+rotation 都改，带角速度限制和最大速度）
        // state = 4：命中后的“失控减速”阶段（弱减速 + 不追踪），60 帧后回到 3
        private int state = 0;
        private int stateTimer = 0;

        // 追踪控制
        private NPC currentTarget = null;
        private float aimRotationStart = 0f;
        private float aimRotationEnd = 0f;

        // 命中之后的“失控减速”计时（你说的第 5 阶段时长，60 帧）
        private int noTrackTimer = 0;

        // 三连激光是否已经释放
        private bool extraLaserFired = false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void OnSpawn(IEntitySource source)
        {
            //base.OnSpawn(source);

            //Player owner = Main.player[Projectile.owner];

            //// 绑定的 INV 弹幕类型 —— 你替换成你自己的绑定小卫星类
            //int invType = ModContent.ProjectileType<StarsofDestinyRIGHTINV>();

            //// 创建 4 个围绕用的 INV
            //for (int i = 0; i < 4; i++)
            //{
            //    int p = Projectile.NewProjectile(
            //        source,
            //        Projectile.Center,
            //        Vector2.Zero,
            //        invType,
            //        Projectile.damage,               // 通常与主体同伤害
            //        Projectile.knockBack,
            //        Projectile.owner,
            //        Projectile.whoAmI,               // ai0：绑定主体
            //        i                                // ai1：分身编号 0~3
            //    );

            //    if (p.WithinBounds(Main.maxProjectiles))
            //    {
            //        Projectile child = Main.projectile[p];

            //        child.timeLeft = Projectile.timeLeft;  // 绑定生命周期
            //        child.netUpdate = true;
            //    }
            //}
        }

        public override void AI()
        {
            Projectile.timeLeft = 10;
            stateTimer++;

            switch (state)
            {
                case 0:
                    {
                        Projectile.velocity *= 0.95f;

                        float fadeTime = 7f;
                        float t = MathHelper.Clamp(stateTimer / fadeTime, 0f, 1f);
                        Projectile.Opacity = MathHelper.Lerp(1f, 0f, t);

                        currentTarget = FindBestTarget(5500f);


                        {
                            // ================================
                            // 起飞前黄金锐角三角 + 突进喷射特效
                            // ================================
                            if (stateTimer >= fadeTime - 2f) // 只在最后 2 帧放特效
                            {
                                // 基准朝向：优先用速度，太小时用 rotation 反推
                                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                                if (forward == Vector2.Zero)
                                    forward = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
                                Vector2 right = forward.RotatedBy(MathHelper.PiOver2);

                                Color mainYellow = new Color(255, 235, 120);
                                Color mainCyan = new Color(130, 210, 255);
                                float triLength = 40f; // 三角前尖长度
                                float triWidth = 20f;  // 三角底边宽度

                                // =========================
                                // 1）GlowOrb：静态锐角三角形（数量 ×5）
                                // =========================
                                int rows = 3 * 5; // ❗ 行数 ×5（原 3 → 15）
                                for (int row = 0; row < rows; row++)
                                {
                                    float rowT = row / (rows - 1f); // 0 → 1

                                    float along = MathHelper.Lerp(triLength, 10f, rowT);

                                    // ❗ 每层的点数 ×5（原 row+4 → row*5+4）
                                    int cols = row * 5 + 4;

                                    for (int k = 0; k < cols; k++)
                                    {
                                        float offsetT = cols == 1 ? 0f : (k / (cols - 1f) - 0.5f);

                                        Vector2 pos =
                                            Projectile.Center +
                                            forward * along +
                                            right * (offsetT * triWidth * (1f - rowT));

                                        float lerpColor = 1f - rowT;
                                        Color c = Color.Lerp(mainCyan, mainYellow, lerpColor) * (0.7f * (1f - t));
                                        c.A = 0;

                                        GlowOrbParticle orb = new GlowOrbParticle(
                                            pos,
                                            Vector2.Zero,
                                            false,
                                            10,
                                            0.7f + 0.15f * (1f - rowT),
                                            c,
                                            true,
                                            false,
                                            true
                                        );

                                        GeneralParticleHandler.SpawnParticle(orb);
                                    }
                                }

                                // =====================================
                                // 2）EXO 之光：有序 + 微随机的前向突击喷射
                                // =====================================
                                int exoCount = 5;
                                float exoSpread = MathHelper.ToRadians(28f);
                                for (int i = 0; i < exoCount; i++)
                                {
                                    float tExo = i / (exoCount - 1f);                       // 0~1
                                    float angleOffset = MathHelper.Lerp(-exoSpread, exoSpread, tExo);
                                    Vector2 dir = forward.RotatedBy(angleOffset + Main.rand.NextFloat(-0.05f, 0.05f));

                                    // 中间更快，两侧稍慢，带一点随机
                                    float centerWeight = 1f - Math.Abs(tExo - 0.5f) * 2f;   // 中间 1，边缘 0
                                    float speed = MathHelper.Lerp(9f, 18f, centerWeight) * Main.rand.NextFloat(0.9f, 1.1f);

                                    Color exoColor = Color.Lerp(mainYellow, mainCyan, 0.3f + 0.4f * centerWeight);

                                    SquishyLightParticle exoEnergy = new SquishyLightParticle(
                                        Projectile.Center,
                                        dir * speed,
                                        0.28f,
                                        exoColor,
                                        25,
                                        opacity: 1f,
                                        squishStrenght: 1f,
                                        maxSquish: 3f,
                                        hueShift: 0f
                                    );
                                    GeneralParticleHandler.SpawnParticle(exoEnergy);
                                }

                                // ==============================
                                // 3）水雾：集中在中段，轻微散逸
                                // ==============================
                                for (int i = 0; i < 3; i++)
                                {
                                    Vector2 mistPos =
                                        Projectile.Center +
                                        forward * Main.rand.NextFloat(10f, 26f) +
                                        right * Main.rand.NextFloat(-6f, 6f);

                                    Vector2 mistVel = forward.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) *
                                                      Main.rand.NextFloat(0.5f, 1.8f);

                                    WaterFlavoredParticle mist = new WaterFlavoredParticle(
                                        mistPos,
                                        mistVel,
                                        false,
                                        Main.rand.Next(18, 26),
                                        0.9f + Main.rand.NextFloat(0.3f),
                                        Color.LightBlue * 0.9f
                                    );
                                    GeneralParticleHandler.SpawnParticle(mist);
                                }

                                // ==============================
                                // 4）椭圆冲击波：前方 2 个叠加
                                // ==============================
                                for (int i = 0; i < 2; i++)
                                {
                                    float dist = 8f + 16f * i;
                                    Vector2 pulsePos = Projectile.Center + forward * dist;

                                    Particle pulse = new DirectionalPulseRing(
                                        pulsePos,
                                        forward * 2.5f,
                                        Color.Lerp(mainYellow, mainCyan, 0.2f * i),
                                        new Vector2(1.1f, 2.4f),
                                        Projectile.rotation - MathHelper.PiOver4,
                                        0.18f,
                                        0.035f,
                                        20
                                    );
                                    GeneralParticleHandler.SpawnParticle(pulse);
                                }
                            }

                        }

                        if (stateTimer >= fadeTime)
                        {
                            if (currentTarget == null)
                            {
                                Projectile.Kill();
                                return;
                            }

                            state = 1;
                            stateTimer = 0;
                        }

                        // ★ 本阶段 rotation = velocity + 45°
                        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                        Projectile.friendly = false;

                        OutlinePower = MathHelper.Lerp(1f, 0f, t);

                        break;
                    }


                // ==========================================================
                // 第 1 阶段：瞬移到敌人背后一帧（全透明） rotation 不变
                // ==========================================================
                case 1:
                    {
                        OutlinePower = 1f;

                        Projectile.Opacity = 0f;

                        Projectile.friendly = true;

                        if (currentTarget == null || !currentTarget.active)
                        {
                            currentTarget = FindBestTarget(5500f);
                            if (currentTarget == null)
                            {
                                Projectile.Kill();
                                return;
                            }
                        }

                        if (stateTimer == 1)
                        {
                            // ★ 改成随机圆周传送（而不是固定“背后”）
                            float teleportRadius = 30f * 16f;
                            float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                            Vector2 offset = randomAngle.ToRotationVector2() * teleportRadius;

                            Projectile.Center = currentTarget.Center + offset;

                            aimRotationStart = Projectile.rotation;
                            aimRotationEnd = (currentTarget.Center - Projectile.Center).ToRotation();
                            Projectile.velocity = Vector2.Zero;

                            state = 2;
                            stateTimer = 0;
                        }

                        // ★ 本阶段 rotation 保持原值，不自动改
                        break;
                    }

                // ==========================================================
                // 第 2 阶段：原地转向瞄准（U 型旋转） + 透明度从 0 → 1
                // ==========================================================
                case 2:
                    {
                        if (currentTarget == null || !currentTarget.active)
                        {
                            Projectile.Kill();
                            return;
                        }

                        Projectile.velocity = Vector2.Zero;

                        float aimTime = 10f;
                        float t = MathHelper.Clamp(stateTimer / aimTime, 0f, 1f);

                        Projectile.Opacity = MathHelper.Lerp(0f, 1f, t);

                        float smooth = 0.5f - 0.5f * MathF.Cos(MathHelper.Pi * t);
                        float baseRot = MathHelper.Lerp(aimRotationStart, aimRotationEnd, smooth);

                        // ★ 本阶段 rotation = 瞄准角度 + 45°
                        Projectile.rotation = MathHelper.WrapAngle(baseRot) + MathHelper.PiOver4;

                        if (stateTimer >= aimTime)
                        {
                            FireLazerAtTarget(true); // 单激光
                            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 6f;

                            state = 3;
                            stateTimer = 0;
                        }
                        break;
                    }

                // ==========================================================
                // 第 3 阶段：真正追踪（速度×3、加速度×1.3、角速度×3）
                // ==========================================================
                case 3:
                    {
                        if (currentTarget == null || !currentTarget.active)
                            currentTarget = FindBestTarget(2000f);

                        if (currentTarget == null)
                        {
                            Projectile.velocity *= 0.97f;
                            if (Projectile.velocity.Length() < 0.2f)
                                Projectile.Kill();
                            return;
                        }

                        float maxSpeed = 54f;   // 18×3
                        float accel = 0.22f * 1.9f;
                        // 追踪时间越久，角速度限制越大（从 0.05 → 0.15）
                        float trackT = MathHelper.Clamp(stateTimer / 90f, 0f, 1f);
                        float maxTurn = MathHelper.Lerp(0.75f, 2.95f, trackT);

                        Vector2 toTarget = currentTarget.Center - Projectile.Center;
                        float targetRot = toTarget.ToRotation();
                        float curRot = Projectile.velocity.ToRotation();

                        float newRot = curRot.AngleTowards(targetRot, maxTurn);
                        Vector2 desiredVel = newRot.ToRotationVector2() * maxSpeed;

                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, accel);

                        if (Projectile.velocity.Length() > maxSpeed)
                            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * maxSpeed;

                        // ★ 本阶段 rotation = velocity + 45°
                        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                        break;
                    }

                // ==========================================================
                // 第 4 阶段：命中后失控滑行（不追踪）60 帧
                // ==========================================================
                case 4:
                    {
                        if (noTrackTimer > 0)
                        {
                            noTrackTimer--;
                            Projectile.velocity *= 0.98f;

                            // ★ 本阶段 rotation = velocity + 45°
                            if (Projectile.velocity.Length() > 0.1f)
                                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                            if (noTrackTimer == 0)
                            {
                                state = 3;
                                stateTimer = 0;
                            }
                        }
                        break;
                    }
            }
        }


        // ==========================================================
        // 击中敌人：进入“第 5 阶段”减速 + 只触发一次三连激光
        // ==========================================================
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 进入第 5 阶段（state = 4），弱减速 & 不追踪
            noTrackTimer = 20;
            state = 4;
            stateTimer = 0;

            // 只在“第一次命中”时安排一次三连激光
            if (!extraLaserFired)
            {
                extraLaserFired = true;

                // 延迟 10 帧后发射扇形激光
                Projectile.ai[0] = 10;
            }
        }

        // 三连激光的延迟计时
        public override void PostAI()
        {
            if (Projectile.ai[0] > 0)
            {
                Projectile.ai[0]--;

                if (Projectile.ai[0] == 0)
                    FireExtraLazers();
            }
        }

        // ==========================================================
        // 发射激光（单发 or 三连）
        // ==========================================================
        private void FireLazerAtTarget(bool singleShot)
        {
            SoundEngine.PlaySound(SoundID.Item91, Projectile.Center);

            if (currentTarget == null)
                return;

            Vector2 dir = (currentTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                dir * 2f,
                ModContent.ProjectileType<SODLazer>(),
                Projectile.damage,
                0f,
                Projectile.owner
            );
        }
        private void FireExtraLazers()
        {
            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);

            if (currentTarget == null)
                return;

            int count = 6;                                // 发射六发
            float step = MathHelper.TwoPi / count;        // 每两发间 60°
            float baseAngle = Projectile.rotation;        // 正前方方向（中心）

            for (int i = 0; i < count; i++)
            {
                // 以当前朝向为中心等角旋转
                float angle = baseAngle + step * i;
                Vector2 dir = angle.ToRotationVector2();

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    dir * 26f,                                   // 速度可改
                    ModContent.ProjectileType<StarsofDestinyRLIGHT>(),
                    Projectile.damage,
                    0f,
                    Projectile.owner
                );
            }
        }


        // ==========================================================
        // OnKill：辉光球星轮爆裂（保持你前面满意的版本）
        // ==========================================================
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);


            {
                // --- 强化触发：检测玩家是否拥有自己的 SODCLK50 ---
                Projectile clkToKill = null;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];

                    if (p.active &&
                        p.owner == Projectile.owner &&
                        p.type == ModContent.ProjectileType<SODCLK50>())
                    {
                        clkToKill = p;   // 记录任意一个
                        break;
                    }
                }

                // 拥有至少 1 个 CLK50 → 触发强化爆炸并扣除一个
                if (clkToKill != null)
                {
                    // 触发爆炸
                    Projectile.NewProjectile(
                        Projectile.GetSource_Death(),
                        Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<StarsofDestinyRStandField>(),
                        Projectile.damage,
                        0f,
                        Projectile.owner
                    );

                    // 扣除一个 CLK50
                    clkToKill.Kill();
                }
            }








            // 第一层：12 条刻痕射线
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12f * i;
                Vector2 dir = angle.ToRotationVector2();

                for (int k = 0; k < 4; k++)
                {
                    Vector2 pos = Projectile.Center + dir * (k * 20f);

                    GlowOrbParticle orb = new GlowOrbParticle(
                        pos,
                        dir * 0.2f,
                        false,
                        12,
                        1.2f,
                        Color.Gold,
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }

            // 第二层：旋转椭圆（不对称）
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 offset = angle.ToRotationVector2();
                offset *= new Vector2(64f, 36f);
                offset = offset.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));

                GlowOrbParticle orb = new GlowOrbParticle(
                    Projectile.Center + offset,
                    Vector2.Zero,
                    false,
                    20,
                    1.1f,
                    Color.White,
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }

            // 第三层：爆裂碎片
            for (int i = 0; i < 25; i++)
            {
                GlowOrbParticle orb = new GlowOrbParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    false,
                    18,
                    Main.rand.NextFloat(0.7f, 1.3f),
                    Color.Lerp(Color.Yellow, Color.White, Main.rand.NextFloat()),
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }

        // ==========================================================
        // 目标搜寻（优先 Boss）
        // ==========================================================
        private NPC FindBestTarget(float maxDist)
        {
            NPC best = null;
            float bestDist = maxDist;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float d = Vector2.Distance(npc.Center, Projectile.Center);

                // 优先 Boss
                if (npc.boss && d < bestDist)
                {
                    best = npc;
                    bestDist = d;
                }
                else if (!npc.boss && d < bestDist && best == null)
                {
                    best = npc;
                    bestDist = d;
                }
            }

            return best;
        }



        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public Color TrailColor(float completionRatio)
        {
            float opacity = Utils.GetLerpValue(1f, 0.6f, completionRatio, true) * Projectile.Opacity;
            return new Color(40, 120, 240) * opacity; // 深海蓝渐隐
        }

        public float TrailWidth(float completionRatio)
        {
            return MathHelper.SmoothStep(16f, 26f, completionRatio);
        }


        // ========== 外部可控的描边强度变量（你将在 AI 里改它） ==========
        public float OutlinePower = 1f;


        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() * 0.5f;

            // ========== 1. Shader 黄金拖尾层 ==========
            Main.spriteBatch.EnterShaderRegion();

            GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
                .SetShaderTexture(ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/window_04"))
                .UseColor(new Color(255, 200, 40))            // 金色主色
                .UseSecondaryColor(new Color(255, 255, 150))  // 淡金次色
                .Apply();

            //PrimitiveRenderer.RenderTrail(
            //    Projectile.oldPos,
            //    new(
            //        ratio => MathHelper.SmoothStep(18f, 6f, ratio), // 拖尾宽度（偏金能量风）
            //        r =>
            //        {
            //            float op = Utils.GetLerpValue(1f, 0.5f, r, true) * Projectile.Opacity;
            //            return new Color(255, 220, 100) * op;
            //        },
            //        _ => Projectile.Size * 0.5f,
            //        shader: GameShaders.Misc["ModNamespace:TrailWarpDistortionEffect"]
            //    ),
            //    12
            //);

            Main.spriteBatch.ExitShaderRegion();

            // ========== 2. 高亮贴图拖尾（加亮金色） ==========
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float fade = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
                Color c = new Color(255, 220, 120) * (0.35f * fade * Projectile.Opacity);
                c.A = 0;

                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float scale = Projectile.scale * (1f + 0.3f * fade);

                Main.EntitySpriteDraw(
                    texture,
                    pos,
                    null,
                    c,
                    Projectile.rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            // ========== 3. 外轮廓描边（亮黄色，随 OutlinePower 可调） ==========
            if (OutlinePower > 0f)
            {
                float outlineSize = 2.5f * OutlinePower; // 可控描边强度

                Color outlineColor = new Color(255, 240, 150) * 0.9f;
                outlineColor.A = 0;

                Vector2 basePos = Projectile.Center - Main.screenPosition;

                for (int k = 0; k < 4; k++)
                {
                    Vector2 offset = new Vector2(1.5f, 0f).RotatedBy(MathHelper.PiOver2 * k) * outlineSize;

                    Main.EntitySpriteDraw(
                        texture,
                        basePos + offset,
                        null,
                        outlineColor,
                        Projectile.rotation,
                        origin,
                        Projectile.scale,
                        SpriteEffects.None,
                        0
                    );
                }
            }

            // ========== 4. 本体绘制 ==========
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
















    }
}
