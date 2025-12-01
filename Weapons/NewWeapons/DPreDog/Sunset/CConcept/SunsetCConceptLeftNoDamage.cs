using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using Terraria.DataStructures;
using ReLogic.Content;
using Terraria.Audio;
using CalamityMod.Particles;
using System.Collections.Generic;
using CalamityMod;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptLeftNoDamage : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180; // 不会轻易消失
            Projectile.alpha = 255; // 🚩 初始完全透明
        }


        public int NoDamageIndex;   // 这个刀片的编号
        private float Time;

        private bool catchingMagic = false;   // 是否正在捕捉魔法阵
        private float catchProgress = 0f;     // 捕捉进度

        // ======= 可调参数（直接改数值即可） =======
        public const float AngleStepDegrees = 51f; // 每个刀片之间的夹角（改成 5f/60f 都行）
        private static readonly float AngleStepRadians = MathHelper.ToRadians(AngleStepDegrees);


        private const float OrbitRadius = 490f;    // 魔法阵公转半径

        public Color ProjectileColor; // 颜色由 ai[1] 决定

        public float HoverOffsetAngle
        {
            get
            {
                // 用固定角度间隔来计算，不再写死等分
                return NoDamageIndex * AngleStepRadians + Time / 30f;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            NoDamageIndex = (int)Projectile.ai[1]; // 初始化编号
            SoundEngine.PlaySound(SoundID.Item110 with { Volume = 1.2f, Pitch = -0.0f }, Projectile.Center);

            // ========== 参数 ==========
            bool isBig = Projectile.scale > 1.5f; // 🚩 判断是否特大号（可换成 ai[2] 等显式标志）
            int dustCount = isBig ? 48 : 24;
            float dustScale = isBig ? 1.5f : 1f;
            float baseRadius = isBig ? 36f : 24f;

            // ========== 1) 往后喷射（尾焰） ==========
            for (int i = 0; i < (isBig ? 12 : 6); i++)
            {
                Vector2 backDir = -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.25f);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Electric,
                    backDir * Main.rand.NextFloat(2f, 5f),
                    150,
                    Color.Cyan,
                    1.2f * dustScale
                );
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // ========== 2) 黄金角向外扩散（有序印刻） ==========
            float golden = MathHelper.ToRadians(137.5f);
            for (int n = 0; n < dustCount; n++)
            {
                float angle = n * golden;
                float rad = baseRadius * MathF.Sqrt((n + 1f) / dustCount);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * rad;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    (n % 2 == 0) ? DustID.BlueTorch : DustID.GemDiamond,
                    angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 1.5f),
                    160,
                    Color.Lerp(Color.Cyan, Color.WhiteSmoke, 0.5f),
                    0.9f * dustScale
                );
                d.noGravity = true;
            }

            // ========== 3) 粒子点缀（科技感） ==========
            for (int i = 0; i < (isBig ? 4 : 2); i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    12,
                    0.8f * dustScale,
                    Color.LightCyan,
                    true, false, true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }



        // === SquishyLight 绑定容器 ===
        private readonly List<SquishyLightParticle> flightExos = new();
        private readonly List<Vector2> flightDirs = new();
        private readonly List<float> flightDists = new();
        private readonly List<float> flightSteps = new();

        // === GlowOrb 绑定容器 ===
        private readonly List<GlowOrbParticle> flightOrbs = new();
        private readonly List<Vector2> orbDirs = new();
        private readonly List<float> orbDists = new();
        private readonly List<float> orbSteps = new();

        private void SpawnFlightFX(Vector2 pos, Color fxColor)
        {
            // ——局部坐标系：f 为前向，n 为法线——
            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                ? Vector2.Normalize(Projectile.velocity)
                : Vector2.UnitX.RotatedBy(Projectile.rotation);
            Vector2 n = new Vector2(-f.Y, f.X);

            // 以弹幕中心为能量核心
            Vector2 core = Projectile.Center;

            // =========================================================
            // A) 向前爆炸式释放：同心层 + 黄金角扰动（视觉“爆点”）
            // =========================================================
            float cone = MathHelper.ToRadians(22f);          // 前向锥角
            float golden = MathHelper.ToRadians(137.5f);     // 黄金角
            int rings = 3;                                // 同心层数
            for (int r = 0; r < rings; r++)
            {
                // 每一层的粒子数按几何增长，形成“由内到外更密更强”的感觉
                int points = 6 + r * 6;                      // 6, 12, 18
                float radius = 10f + r * 8f;                 // 爆点半径层
                for (int i = 0; i < points; i++)
                {
                    // 用黄金角驱动的正弦扇散：角度落在 [-cone, +cone]
                    float a = (float)Math.Sin((i + r * 0.5f) * golden) * cone;
                    Vector2 dir = f.RotatedBy(a);

                    // 出生位置做“壳层”抖动，让前爆呈现涟漪壳
                    Vector2 spawn = core + dir * (radius + Main.rand.NextFloat(-2f, 2f));

                    // 速度带“能量外抛”，寿命随层加长，尺度递增
                    float spd = Main.rand.NextFloat(4.2f + r * 0.6f, 7.2f + r * 1.2f);
                    float scl = 0.26f + 0.04f * r + Main.rand.NextFloat(0.02f);
                    int life = Main.rand.Next(18 + r * 3, 26 + r * 4);

                    // ——高速能流（细长）——
                    SquishyLightParticle streak = new SquishyLightParticle(
                        spawn,
                        dir * spd,
                        scl,
                        fxColor,
                        life,
                        opacity: 1f,
                        squishStrenght: 1.14f,
                        maxSquish: 3.25f,
                        hueShift: 0f
                    );
                    GeneralParticleHandler.SpawnParticle(streak);

                    // ——能量珠点：用少量 GlowOrb 做“离散高光”——
                    if (Main.rand.NextBool(3))
                    {
                        GlowOrbParticle orb = new GlowOrbParticle(
                            spawn + dir * Main.rand.NextFloat(4f, 10f),
                            dir * Main.rand.NextFloat(1.2f, 2.2f),
                            false,
                            (int)(life * 0.85f),
                            0.8f + 0.05f * r,
                            Color.Lerp(fxColor, Color.White, 0.30f),
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
            }

            // 前爆冲击环（细长椭圆，强调“冲击”）
            if (Main.GameUpdateCount % 2 == 0)
            {
                Particle pulse = new DirectionalPulseRing(
                    core + f * 8f,
                    f * 0.8f,
                    Color.Lerp(fxColor, Color.White, 0.3f),
                    new Vector2(1.05f, 2.6f),
                    Projectile.rotation,
                    0.20f,
                    0.035f,
                    20
                );
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // 中心亮斑（瞬时强 Bloom，做“爆心闪击”）
            if (Main.rand.NextBool(3))
            {
                StrongBloom flash = new StrongBloom(
                    core,
                    Vector2.Zero,
                    Color.Lerp(fxColor, Color.White, 0.25f),
                    1.6f,
                    36
                );
                GeneralParticleHandler.SpawnParticle(flash);
            }

            // =========================================================
            // B) 向后扇形两道弧线：对称曲线（近似对数螺线/渐开线的混合）
            //    公式：r(t) = R * t^p,  θ(t) = θ0 + side * W * E(t)
            //    其中 E(t)=t^2(3-2t)（平滑步进），side=±1（左右两翼）
            // =========================================================
            int segs = 22;                     // 每翼采样点数
            float R = 68f;                    // 弧线半径尺度
            float p = 0.80f;                  // 幂指数，决定“前密后疏”的曲率节奏
            float W = MathHelper.ToRadians(75f); // 后扇展开半角
            Vector2 back = -f;                      // 朝后方向

            for (int side = -1; side <= 1; side += 2)
            {
                for (int j = 0; j <= segs; j++)
                {
                    float t = j / (float)segs;                    // 0→1
                    float e = t * t * (3f - 2f * t);              // smoothstep
                    float r = R * (float)Math.Pow(t, p);          // 半径随 t^p 增长
                    float th = side * W * e;                       // 角度平滑展开

                    // 当前点与切向
                    Vector2 dir = back.RotatedBy(th);
                    Vector2 posOnArc = core + dir * r;

                    // 用相邻采样估计切线方向，求速度向量
                    float t2 = Math.Min(1f, t + 1f / segs);
                    float e2 = t2 * t2 * (3f - 2f * t2);
                    float r2 = R * (float)Math.Pow(t2, p);
                    float th2 = side * W * e2;
                    Vector2 dir2 = back.RotatedBy(th2);
                    Vector2 pos2 = core + dir2 * r2;

                    Vector2 tan = (pos2 - posOnArc);
                    if (tan.LengthSquared() < 1e-4f) tan = dir * 0.5f;

                    // ——沿弧“能流条纹”——
                    SquishyLightParticle arcLine = new SquishyLightParticle(
                        posOnArc,
                        tan.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.9f, 1.8f),
                        0.24f + Main.rand.NextFloat(0.05f),
                        Color.Lerp(fxColor, Color.White, 0.22f),
                        Main.rand.Next(26, 34),
                        opacity: 1f,
                        squishStrenght: 1.10f,
                        maxSquish: 3.0f,
                        hueShift: 0f
                    );
                    GeneralParticleHandler.SpawnParticle(arcLine);

                    // ——稀疏珠点：在弧线外侧轻微“漂移”，增加层次——
                    if (j % 3 == 0 && Main.rand.NextBool(2))
                    {
                        GlowOrbParticle orb = new GlowOrbParticle(
                            posOnArc + n * side * Main.rand.NextFloat(1.2f, 3.2f),
                            tan.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.6f, 1.2f),
                            false,
                            14,
                            0.74f,
                            Color.Lerp(fxColor, Color.White, 0.18f),
                            true, false, true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);
                    }
                }
            }
        }

        // 分身淡出状态控制
        private float fadeOutProgress = 0f; // 0~1，淡出程度
        private const float fadeSpeed = 0.02f; // 每帧透明度变化速度，可自行调节

        private float orbitRadius = 30f;         // 原始公转半径（你现在默认是 30 左右）
        private const float maxOrbitRadius = 150f * 16f; // 右键时的目标半径




        public override void AI()
        {
            // 颜色表（10 种颜色）
            Color[] colors = {
                new Color(75, 0, 130),   // 异能深紫（异能 → 深紫）
                new Color(0, 220, 120),  // 基因绿色（基因 → 绿色）
                new Color(70, 130, 255), // 因果蓝色（因果 → 蓝色）
                new Color(180, 30, 60),  // 神秘深红（神秘 → 深红）
                new Color(120, 150, 200),// 科技银蓝色（科技 → 银蓝色）
                new Color(255, 105, 180),// 幻术粉红（幻术 → 粉红）
                new Color(255, 215, 0),  // 封印金色（封印 → 金色）
                Color.White,             // 修真白色（修真 → 白色）
                new Color(150, 90, 200), // 幻想紫藤色（幻想 → 紫藤色）
                new Color(220, 50, 50),  // 机制白红色（机制 → 白红色）
            };

            // 异能 疯齿丑绅
            // 基因 葬灵木渊
            // 因果 四夜轮回
            // 神秘 挖腹背鳍
            // 科技 千萃碎年
            // 幻术 抚脑幻听
            // 封印 天之缚权
            // 修真 白烛荡想
            // 幻想 托泪逆梦
            // 机制 掩手狂目

            // 【新增】公转计时（整体转动用）
            Time++;

            // === 飞行期间特效 ===
            Vector2 headPosition = Projectile.Center + new Vector2(48f, 0f).RotatedBy(Projectile.rotation);

            // 根据 ai[1] 选择对应主题色
            if (Projectile.ai[1] >= 0 && Projectile.ai[1] < colors.Length)
            {
                ProjectileColor = colors[(int)Projectile.ai[1]];
                Color fxColor = ProjectileColor;

                // 如果之后需要飞行特效，这里可以调用：
                //SpawnFlightFX(headPosition, fxColor);
            }

            // === 飞行 SquishyLight 绑定更新 ===
            for (int i = flightExos.Count - 1; i >= 0; i--)
            {
                var p = flightExos[i];
                if (p.Time >= p.Lifetime)
                {
                    flightExos.RemoveAt(i);
                    flightDirs.RemoveAt(i);
                    flightDists.RemoveAt(i);
                    flightSteps.RemoveAt(i);
                    continue;
                }

                flightDists[i] += flightSteps[i];
                p.Position = Projectile.Center + flightDirs[i] * flightDists[i];
                p.Velocity = Vector2.Zero; // 避免继续叠加
            }

            // === 飞行 GlowOrb 绑定更新 ===
            for (int i = flightOrbs.Count - 1; i >= 0; i--)
            {
                var p = flightOrbs[i];
                if (p.Time >= p.Lifetime)
                {
                    flightOrbs.RemoveAt(i);
                    orbDirs.RemoveAt(i);
                    orbDists.RemoveAt(i);
                    orbSteps.RemoveAt(i);
                    continue;
                }

                orbDists[i] += orbSteps[i];
                p.Position = Projectile.Center + orbDirs[i] * orbDists[i];
                p.Velocity = Vector2.Zero;
            }

            // 🚩 每帧降低 alpha，直到 0 为止（淡入效果）
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 8; // 8 × 30 ≈ 240 -> 大约 30 帧完全淡入
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            // 再次保证颜色与 ai[1] 同步（防止外部改动）
            if (Projectile.ai[1] >= 0 && Projectile.ai[1] < colors.Length)
                ProjectileColor = colors[(int)Projectile.ai[1]];

            // === 模式 A：玩家环绕 ===
            if (Projectile.ai[0] == -1)
            {
                Player owner = Main.player[Projectile.owner];

                // 查找魔法阵（LeftMagic）
                int magicIndex = -1;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active &&
                        Main.projectile[i].type == ModContent.ProjectileType<SunsetCConceptLeftMagic>() &&
                        Main.projectile[i].owner == Projectile.owner)
                    {
                        magicIndex = i;
                        break;
                    }
                }

                // 进入捕捉状态（仅在找到魔法阵时）
                if (magicIndex != -1 && !catchingMagic)
                {
                    catchingMagic = true;
                    Projectile.ai[0] = magicIndex;
                    catchProgress = 0f;
                }

                // 还在玩家轨道上时：正常围绕玩家公转
                if (!catchingMagic)
                {
                    float angle = HoverOffsetAngle; // 属性里已经包含 index 等分 + Time 自转

                    // 围绕玩家的半径（这里先写死 550，需要你再调就改这里）
                    // ===== 右键弹幕检测 =====
                    bool rightExists = false;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active &&
                            Main.projectile[i].owner == Projectile.owner &&
                            Main.projectile[i].type == ModContent.ProjectileType<SunsetCConceptRight>())
                        {
                            rightExists = true;
                            break;
                        }
                    }

                    // ===== 根据 rightExists 平滑切换半径 =====
                    float targetRadius = rightExists ? (150f * 16f) : 550f;   // ★ 半径变化
                    orbitRadius = MathHelper.Lerp(orbitRadius, targetRadius, 0.08f);

                    // ===== 使用动态 orbitRadius 作为公转半径 =====
                    Vector2 offset = angle.ToRotationVector2() * orbitRadius;

                    Projectile.Center = owner.Center + offset;
                    Projectile.rotation = Projectile.AngleFrom(owner.Center) + MathHelper.PiOver4;
                    Projectile.timeLeft = 60;
                }
            }
            else
            {
                // === 模式 B：魔法阵捕捉/公转 ===
                int parentIndex = (int)Projectile.ai[0];

                // 只有当 ai[0] 指向有效魔法阵时才进入，否则自杀
                if (parentIndex < 0 || parentIndex >= Main.maxProjectiles ||
                    !Main.projectile[parentIndex].active ||
                    Main.projectile[parentIndex].type != ModContent.ProjectileType<SunsetCConceptLeftMagic>())
                {
                    Projectile.Kill();
                    return;
                }

                Projectile parentMagic = Main.projectile[parentIndex];

                float targetAngle = HoverOffsetAngle;
                Vector2 targetPos = parentMagic.Center + targetAngle.ToRotationVector2() * OrbitRadius;

                if (catchingMagic && catchProgress < 1f)
                {
                    catchProgress += 0.02f;

                    Vector2 currentDir = Projectile.Center - parentMagic.Center;
                    Vector2 targetDir = targetPos - parentMagic.Center;

                    float currentAngle = currentDir.ToRotation();
                    float desiredAngle = targetDir.ToRotation();
                    float angleDiff = MathHelper.WrapAngle(desiredAngle - currentAngle);

                    float eased = catchProgress * catchProgress * (3f - 2f * catchProgress); // S 型缓动
                    float maxStep = MathHelper.ToRadians(2f + 10f * eased);
                    float newAngle = MathHelper.Lerp(currentAngle, desiredAngle, eased);

                    float newRadius = MathHelper.Lerp(currentDir.Length(), OrbitRadius, eased);
                    Projectile.Center = parentMagic.Center + newAngle.ToRotationVector2() * newRadius;
                }
                else
                {
                    Projectile.Center = targetPos;
                }

                Projectile.rotation = Projectile.AngleFrom(parentMagic.Center) + MathHelper.PiOver4 + MathHelper.Pi;
                Projectile.timeLeft = 600;
                Time++;
                Projectile.netUpdate = true;
            }
        }




        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/疯齿丑绅";

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles,
    List<int> behindNPCs, List<int> behindProjectiles,
    List<int> overPlayers, List<int> overWiresUI)
        {
            // 碎片始终算作“上层”，压在魔法阵之上
            overPlayers.Add(index);
        }

        // === Shader 拖尾需要的缓存 ===
        private VertexStrip TrailDrawer;
        private Vector2[] oldPosCache = new Vector2[45];
        private float[] oldRotCache = new float[45];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 45;
        }
        public Color TrailColorFunction(float completionRatio)
        {
            float opacity = (float)Math.Pow(Utils.GetLerpValue(1f, 0.45f, completionRatio, true), 4D);
            opacity *= Projectile.Opacity * 0.6f;

            // 用你的主题色作为基础颜色
            //Color start = ProjectileColor;
            Color start = Color.LightGray;
            Color end = Color.White;

            float t = MathHelper.Clamp(completionRatio * 1.4f, 0f, 1f);
            return Color.Lerp(start, end, t) * opacity * (1f - fadeOutProgress);
        }
        public float TrailWidthFunction(float completionRatio)
        {
            return Projectile.height * (1f - completionRatio) * 3.8f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // === 1. 贴图选择 ===
            Texture2D texture;
            int index = NoDamageIndex % 10;

            if (index == 0)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/疯齿丑绅").Value;
            else if (index == 1)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/葬灵木渊").Value;
            else if (index == 2)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/四夜轮回").Value;
            else if (index == 3)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/挖腹背鳍").Value;
            else if (index == 4)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/千萃碎年").Value;
            else if (index == 5)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/抚脑幻听").Value;
            else if (index == 6)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/天之缚权").Value;
            else if (index == 7)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/白烛荡想").Value;
            else if (index == 8)
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/托泪逆梦").Value;
            else
                texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/掩手狂目").Value;

            Rectangle frame = texture.Frame();
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 pos = Projectile.Center - Main.screenPosition;


            // ============================================================
            // 🔥🔥🔥 关键 1：拖尾 Shader 透明度同步淡出
            // ============================================================

            float fade = 1f - fadeOutProgress;   // 核心透明度系数

            TrailDrawer ??= new();

            GameShaders.Misc["EmpressBlade"].UseImage0("Images/Extra_201");
            GameShaders.Misc["EmpressBlade"].UseImage1("Images/Extra_193");

            GameShaders.Misc["EmpressBlade"].UseShaderSpecificData(
                new Vector4(
                    ProjectileColor.R / 255f,
                    ProjectileColor.G / 255f,
                    ProjectileColor.B / 255f,
                    0.6f * fade   // ★★★ 拖尾整体透明度在这里乘 fade
                )
            );
            GameShaders.Misc["EmpressBlade"].Apply(null);

            TrailDrawer.PrepareStrip(
                Projectile.oldPos,
                Projectile.oldRot,
                (r) => TrailColorFunction(r) * fade,   // ★★★ 再乘一次保证颜色计算也淡出
                TrailWidthFunction,
                Projectile.Size * 0.5f - Main.screenPosition,
                Projectile.oldPos.Length,
                true
            );
            TrailDrawer.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


            // ============================================================
            // 🔥🔥🔥 关键 2：包边外发光 同步透明度淡出
            // ============================================================

            float glowStrength = 0.55f;
            float glowScale = Projectile.scale * 1.18f;

            Color glowColor =
                ProjectileColor *
                (1f - Projectile.alpha / 255f) *
                glowStrength *
                fade * fade;        // ★★★ 包边完全跟随 fadeOutProgress

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.PiOver2 * i).ToRotationVector2() * 3f;
                Main.EntitySpriteDraw(
                    texture,
                    pos + offset,
                    frame,
                    glowColor,
                    Projectile.rotation,
                    origin,
                    glowScale,
                    SpriteEffects.None,
                    0
                );
            }


            // ============================================================
            // 🔥🔥🔥 关键 3：本体贴图透明度同步淡出
            // ============================================================

            Color mainColor =
                Projectile.GetAlpha(lightColor) *
                fade;              // ★★★ 本体直接淡出

            Main.EntitySpriteDraw(
                texture,
                pos,
                frame,
                mainColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );


            GameShaders.Misc["EmpressBlade"].UseImage0("Images/Extra_209");
            GameShaders.Misc["EmpressBlade"].UseImage1("Images/Extra_210");

            return false;
        }






    }
}


/*
 
 
 
            // 确认颜色
            if (Projectile.ai[1] >= 0 && Projectile.ai[1] < colors.Length)
            {
                ProjectileColor = colors[(int)Projectile.ai[1]];
                Color fxColor = ProjectileColor;

                switch ((int)Projectile.ai[1])
                {
                    case 0: // 异能深紫（“异能”：精神波动/心灵力场；核心色：深紫）
                        {
                            // ——确保我们拿到弹头位置（你若已在外层算过 headPosition，可删掉这里两行）——
                            float fixedRotation = Projectile.rotation;
                            headPosition = Projectile.Center + new Vector2(48f, 0f).RotatedBy(fixedRotation);

                            // 局部正前/法线坐标系（用于“半圆收缩”与触须摆动）
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            fxColor = new Color(75, 0, 130); // 深紫：异能（注：弹幕名“异能”）

                            float time = Main.GlobalTimeWrappedHourly;

                            // ——双触须：李萨茹/玫瑰线参数（半圆前方收缩），每帧相位不同 → 观感“灵能涌动”——
                            int arms = 2;
                            float A = 10f + 4f * (float)Math.Sin(time * 2.1f); // 法线方向振幅（呼吸）
                            float lead = 3f;                                   // 前挤距（触须离弹头的前向基距）
                            for (int arm = 0; arm < arms; arm++)
                            {
                                float phi = time * (1.6f + arm * 0.25f) + arm * MathHelper.PiOver2; // 相位差
                                                                                                    // 玫瑰/李萨茹组合：沿 f 微前伸 + 沿 n 正弦摆动 + 二次谐波“瓣”层
                                Vector2 swirl =
                                    f * (lead + 2f * (float)Math.Sin(2f * phi)) +
                                    n * (A * (float)Math.Sin(phi));

                                Vector2 p = headPosition + swirl;

                                // EXO 光粒：细长高亮，沿 f 轻推（表现“心灵触须”尖端）
                                SquishyLightParticle exo = new SquishyLightParticle(
                                    p,
                                    f * Main.rand.NextFloat(0.7f, 1.3f) + n * (float)Math.Cos(phi) * 0.12f,
                                    0.26f,                                 // 缩放（细）
                                    fxColor,                               // 深紫
                                    20,                                    // 寿命
                                    opacity: 1f,
                                    squishStrenght: 1.15f,
                                    maxSquish: 3.2f,                       // 细长拉伸
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exo);

                                // 辉光球：在触须上稀疏镶嵌“心灵结点”（增强层次）
                                if (arm == 0 && Main.rand.NextBool(3))
                                {
                                    GlowOrbParticle orb = new GlowOrbParticle(
                                        p + Main.rand.NextVector2Circular(1.5f, 1.5f),
                                        Vector2.Zero,
                                        false,
                                        8,
                                        0.8f,
                                        Color.Lerp(fxColor, Color.White, 0.25f),
                                        true, false, true
                                    );
                                    GeneralParticleHandler.SpawnParticle(orb);
                                }
                            }

                            // 心灵雾：沿 -f 方向轻回流，结合正弦横摆，使“场域”有呼吸的氤氲感
                            if (Main.GameUpdateCount % 3 == 0)
                            {
                                Vector2 mistPos = headPosition + n * (float)Math.Sin(time * 4.5f) * 6f;
                                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                                    mistPos,
                                    -f * Main.rand.NextFloat(0.5f, 0.9f),
                                    false,
                                    Main.rand.Next(18, 24),
                                    0.95f + Main.rand.NextFloat(0.25f),
                                    fxColor * 0.55f
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }

                            // 深紫脉冲：细椭圆冲击波（用你的规范：rotation，不再减 Pi/4）
                            if (Main.GameUpdateCount % 14 == 0)
                            {
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.75f,
                                    fxColor,
                                    new Vector2(0.9f, 2.4f), // 细长椭圆，沿前向更长
                                    Projectile.rotation,     // ✅ 使用 rotation 本体
                                    0.18f,
                                    0.03f,
                                    18
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }

                            // 低频“裂光”：极少量暗紫裂隙（点到为止，避免喧宾夺主）
                            if (Main.rand.NextBool(20))
                            {
                                CrackParticle crack = new CrackParticle(
                                    headPosition,
                                    -f * 0.6f,
                                    new Color(90, 0, 130),
                                    new Vector2(0.8f, 0.8f),
                                    Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                                    0.15f,
                                    0.4f,
                                    16
                                );
                                GeneralParticleHandler.SpawnParticle(crack);
                            }
                        }
                        break;


                    case 1: // 基因绿色（DNA 螺旋 · 生命感）
                        {
                            // f：正前方方向；n：法线
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float time = Main.GlobalTimeWrappedHourly * 4f; // 螺旋速度
                            float radius = 6f + (float)Math.Sin(time * 0.5f) * 2f; // 呼吸变化

                            // 双螺旋：两条相位差 π 的正弦波
                            for (int arm = 0; arm < 2; arm++)
                            {
                                float phi = time + arm * MathHelper.Pi; // 相位差
                                Vector2 offset = f * (float)Math.Cos(phi) * 6f + n * (float)Math.Sin(phi) * radius;

                                Vector2 p = headPosition + offset;

                                // GlowOrb：基因链上的“碱基点”
                                if (Main.rand.NextBool(2))
                                {
                                    GlowOrbParticle orb = new GlowOrbParticle(
                                        p,
                                        Vector2.Zero,
                                        false,
                                        12,
                                        0.75f,
                                        fxColor, // 基因绿色
                                        true, false, true
                                    );
                                    GeneralParticleHandler.SpawnParticle(orb);
                                }

                                // SquishyLight：链条能量火花
                                if (Main.rand.NextBool(4))
                                {
                                    SquishyLightParticle exo = new SquishyLightParticle(
                                        p,
                                        f * 0.1f,
                                        0.22f,
                                        fxColor,
                                        16,
                                        opacity: 0.9f,
                                        squishStrenght: 1.0f,
                                        maxSquish: 2.4f,
                                        hueShift: 0f
                                    );
                                    GeneralParticleHandler.SpawnParticle(exo);
                                }
                            }

                            // 水雾：表现“生命气息”
                            if (Main.rand.NextBool(6))
                            {
                                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                                    headPosition + n * Main.rand.NextFloat(-4f, 4f),
                                    -f * 0.3f,
                                    false,
                                    Main.rand.Next(14, 20),
                                    0.8f + Main.rand.NextFloat(0.2f),
                                    fxColor * 0.7f
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }

                            // 偶尔触发脉冲波：基因能量场
                            if (Main.GameUpdateCount % 22 == 0)
                            {
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.5f,
                                    fxColor,
                                    new Vector2(1.2f, 2.6f), // 拉长的椭圆，像扩散的能量
                                    Projectile.rotation,
                                    0.16f,
                                    0.02f,
                                    20
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }
                        }
                        break;


                    case 2: // 因果蓝色（“因→果”连锁：链珠节点 + 涟漪扩散 + 摆钟律动）
                        {
                            // 局部前向/法线坐标系
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float t = (float)Main.GameUpdateCount * 0.06f;

                            // ——① 因果链珠：等比距离上的“节点”，沿 f 铺开，代表因与果的递推——
                            //    节点位置：沿 f 的距离呈近似等比（更远更稀疏），叠加轻微横摆（sin）
                            float[] chain = { 6f, 10f, 16f, 25.6f, 41f }; // 近似等比序列
                            for (int k = 0; k < chain.Length; k++)
                            {
                                float phase = t + k * 0.65f;
                                Vector2 p = headPosition + f * chain[k] + n * (float)System.Math.Sin(phase) * 1.3f;

                                // GlowOrb：蓝色“因果节点”，稳定发光；密度从近到远逐渐降低
                                if (Main.rand.NextFloat() < 0.75f - 0.12f * k)
                                {
                                    var orb = new GlowOrbParticle(
                                        p,
                                        Vector2.Zero,
                                        false,
                                        12,
                                        0.78f,
                                        fxColor,                 // 因果蓝核心色
                                        true, false, true
                                    );
                                    GeneralParticleHandler.SpawnParticle(orb);
                                }

                                // EXO：节点间的“跃迁火花”，小而细，沿 f 微推进
                                if (Main.rand.NextBool(4))
                                {
                                    var exo = new SquishyLightParticle(
                                        p,
                                        f * Main.rand.NextFloat(0.25f, 0.6f) + n * Main.rand.NextFloat(-0.08f, 0.08f),
                                        0.22f,
                                        Color.Lerp(fxColor, Color.White, 0.2f),
                                        16,
                                        opacity: 1f,
                                        squishStrenght: 1.0f,
                                        maxSquish: 2.5f,
                                        hueShift: 0f
                                    );
                                    GeneralParticleHandler.SpawnParticle(exo);
                                }
                            }

                            // ——② 涟漪：因→果的“波纹效应”，分三相位在前向不同距离触发——
                            //    用帧序对 9 取模分三段触发，形成“一个波推动下一个波”的错位连锁观感
                            int mod = (int)(Main.GameUpdateCount % 9);
                            if (mod == 0 || mod == 3 || mod == 6)
                            {
                                float step = (mod == 0 ? 8f : (mod == 3 ? 18f : 30f));
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition + f * step,
                                    f * 0.65f,                                 // 沿前向传播
                                    Color.Lerp(fxColor, Color.White, 0.35f),  // 明亮的因果蓝
                                    new Vector2(1.05f, 2.7f),                 // 椭圆细长（前向更长）
                                    Projectile.rotation,                       // ✅ 按要求使用 rotation
                                    0.18f,
                                    0.03f,
                                    20
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }

                            // ——③ 摆钟：一颗“因果钟摆”在前方小弧上来回摆动，模拟物理律动——
                            //    摆幅缓慢呼吸变化，位置沿 f 推进，体现“既定轨道上的偏差与回归”
                            float amp = 5.2f + 1.2f * (float)System.Math.Sin(t * 0.8f);
                            Vector2 pendPos = headPosition + f * 14f + n * (float)System.Math.Sin(t * 2.2f) * amp;
                            if (Main.rand.NextBool(2))
                            {
                                var exoPend = new SquishyLightParticle(
                                    pendPos,
                                    Vector2.Zero,
                                    0.24f,
                                    Color.Lerp(fxColor, Color.White, 0.15f),
                                    18,
                                    opacity: 1f,
                                    squishStrenght: 1.05f,
                                    maxSquish: 2.6f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exoPend);
                            }
                            if (Main.rand.NextBool(4))
                            {
                                var orbPend = new GlowOrbParticle(
                                    pendPos + Main.rand.NextVector2Circular(1.2f, 1.2f),
                                    Vector2.Zero,
                                    false,
                                    10,
                                    0.7f,
                                    Color.Lerp(fxColor, Color.White, 0.25f),
                                    true, false, true
                                );
                                GeneralParticleHandler.SpawnParticle(orbPend);
                            }

                            // ——④ 生命水雾：极少量，向后溢散，补充层次——
                            if (Main.rand.NextBool(7))
                            {
                                var mist = new WaterFlavoredParticle(
                                    headPosition + n * Main.rand.NextFloat(-3.5f, 3.5f),
                                    -f * Main.rand.NextFloat(0.4f, 0.8f),
                                    false,
                                    Main.rand.Next(16, 22),
                                    0.85f + Main.rand.NextFloat(0.25f),
                                    Color.Lerp(fxColor, Color.White, 0.35f)
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }

                            // ——⑤ 低频加强：极少 StrongBloom 提升“结果”瞬间的亮度，但严格控量——
                            if (Main.GameUpdateCount % 27 == 0 && Main.rand.NextBool(2))
                            {
                                var strong = new StrongBloom(
                                    headPosition + f * 10f,
                                    Vector2.Zero,
                                    Color.Lerp(fxColor, Color.White, 0.25f),
                                    1.6f,
                                    36
                                );
                                GeneralParticleHandler.SpawnParticle(strong);
                            }
                        }
                        break;
                    case 3: // 神秘深红（心跳脉冲 · 心形收缩 · 血雾回流）
                        {
                            // 局部前向/法线，用于在 headPosition 前方构造“心形+脉冲”的数学形状
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float t = (float)Main.GameUpdateCount * 0.085f; // 全局相位（心跳速率）
                            float breath = 1f + 0.18f * (float)System.Math.Sin(t * 1.7f); // 呼吸收缩

                            // ——① 心形收缩（Cardioid）触须：r = R * (1 - k * cosθ)，随时间收缩/旋转
                            //    取三枚“瓣端”，让形状不规则且充满生命感；每帧略变
                            for (int i = 0; i < 3; i++)
                            {
                                float theta = t * 1.6f + i * (MathHelper.TwoPi / 3f); // 三瓣相位错开
                                float k = 0.55f;                        // 心形参数
                                float R = (16f + 6f * (float)System.Math.Sin(t * 2.3f + i)) * breath;
                                float cardio = (1f - k * (float)System.Math.Cos(theta)); // 心形系数

                                // 平面极坐标 → 局部 f/n
                                Vector2 dir = f * (float)System.Math.Cos(theta) + n * (float)System.Math.Sin(theta);
                                Vector2 p = headPosition + dir * (R * cardio);

                                // EXO：血色能量尖光（沿 f 轻推，细长）
                                var exo = new SquishyLightParticle(
                                    p,
                                    f * Main.rand.NextFloat(0.35f, 0.9f) + n * Main.rand.NextFloat(-0.08f, 0.08f),
                                    0.26f,
                                    Color.Lerp(fxColor, Color.White, 0.12f), // 深红微提亮
                                    20,
                                    opacity: 1f,
                                    squishStrenght: 1.12f,
                                    maxSquish: 3.1f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exo);

                                // GlowOrb：心瓣亮点，稀疏点缀
                                if (Main.rand.NextBool(3))
                                {
                                    var orb = new GlowOrbParticle(
                                        p + Main.rand.NextVector2Circular(1.4f, 1.4f),
                                        Vector2.Zero,
                                        false,
                                        10,
                                        0.78f,
                                        Color.Lerp(fxColor, Color.White, 0.2f),
                                        true, false, true
                                    );
                                    GeneralParticleHandler.SpawnParticle(orb);
                                }
                            }

                            // ——② 心跳脉冲：按拍扩散的细长椭圆冲击波（rotation 对齐）
                            if (Main.GameUpdateCount % 16 == 0)
                            {
                                Particle pulse1 = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.75f,
                                    Color.Lerp(fxColor, Color.White, 0.25f),
                                    new Vector2(1.0f, 2.8f),        // 细长，沿前向更长
                                    Projectile.rotation,            // ✅ 使用 rotation
                                    0.22f,
                                    0.04f,
                                    20
                                );
                                GeneralParticleHandler.SpawnParticle(pulse1);

                                // 同拍双响：稍前方再来一道，层次更厚
                                Particle pulse2 = new DirectionalPulseRing(
                                    headPosition + f * 8f,
                                    f * 0.85f,
                                    Color.Lerp(fxColor, Color.White, 0.35f),
                                    new Vector2(1.0f, 3.0f),
                                    Projectile.rotation,
                                    0.20f,
                                    0.035f,
                                    18
                                );
                                GeneralParticleHandler.SpawnParticle(pulse2);
                            }

                            // ——③ 血雾回流：向后（-f）轻散，给“血色蒸汽”层次
                            if (Main.rand.NextBool(3))
                            {
                                var mist = new WaterFlavoredParticle(
                                    headPosition + n * Main.rand.NextFloat(-3.5f, 3.5f),
                                    -f * Main.rand.NextFloat(0.5f, 1.1f),
                                    false,
                                    Main.rand.Next(16, 22),
                                    0.9f + Main.rand.NextFloat(0.3f),
                                    Color.Lerp(fxColor, Color.White, 0.18f)
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }

                            // ——④ 裂隙：极低频暗红裂光（点到为止）
                            if (Main.rand.NextBool(18))
                            {
                                var crack = new CrackParticle(
                                    headPosition,
                                    -f * 0.6f,
                                    new Color(140, 20, 40),         // 暗红
                                    new Vector2(0.9f, 0.9f),
                                    Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                                    0.55f,
                                    1.5f,
                                    18
                                );
                                GeneralParticleHandler.SpawnParticle(crack);
                            }
                        }
                        break;
                    case 4: // 科技银蓝色（电子矩阵 · 数字电弧）
                        {
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float t = (float)Main.GameUpdateCount * 0.08f;

                            // ——① 数字矩阵方块：在 f/n 格点上生成规律方块，表现“数据流”——
                            for (int i = -1; i <= 1; i++)
                            {
                                float offsetN = i * 6f + (float)System.Math.Sin(t + i) * 1.5f;
                                Vector2 p = headPosition + n * offsetN;

                                if (Main.rand.NextBool(3))
                                {
                                    SquareParticle sq = new SquareParticle(
                                        p,
                                        Projectile.velocity * 0.25f,
                                        false,
                                        26,
                                        1.2f + Main.rand.NextFloat(0.4f),
                                        Color.Lerp(fxColor, Color.White, 0.25f)
                                    );
                                    GeneralParticleHandler.SpawnParticle(sq);
                                }
                            }

                            // ——② 量子能流（EXO 粒子）：细长的银蓝光线，像电子流动——
                            if (Main.rand.NextBool(2))
                            {
                                var exo = new SquishyLightParticle(
                                    headPosition,
                                    f * Main.rand.NextFloat(0.3f, 0.7f) + n * Main.rand.NextFloat(-0.15f, 0.15f),
                                    0.24f,
                                    Color.Lerp(fxColor, Color.White, 0.2f),
                                    18,
                                    opacity: 1f,
                                    squishStrenght: 1.1f,
                                    maxSquish: 2.6f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exo);
                            }

                            // ——③ 电弧光球：偶发短路电弧，增强“高能科技感”——
                            if (Main.rand.NextBool(8))
                            {
                                GlowOrbParticle orb = new GlowOrbParticle(
                                    headPosition + Main.rand.NextVector2Circular(3f, 3f),
                                    Vector2.Zero,
                                    false,
                                    10,
                                    0.75f,
                                    Color.Lerp(fxColor, Color.White, 0.3f),
                                    true, false, true
                                );
                                GeneralParticleHandler.SpawnParticle(orb);
                            }

                            // ——④ 偶尔释放电磁波脉冲——
                            if (Main.GameUpdateCount % 24 == 0)
                            {
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.65f,
                                    Color.Lerp(fxColor, Color.White, 0.35f),
                                    new Vector2(1.1f, 2.2f),   // 扁长的科技波动
                                    Projectile.rotation,
                                    0.20f,
                                    0.03f,
                                    22
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }

                            // ——⑤ 背景雾：轻微的银蓝雾化，表现能量散逸——
                            if (Main.rand.NextBool(7))
                            {
                                var mist = new WaterFlavoredParticle(
                                    headPosition,
                                    -f * Main.rand.NextFloat(0.3f, 0.6f),
                                    false,
                                    Main.rand.Next(14, 20),
                                    0.8f + Main.rand.NextFloat(0.3f),
                                    Color.Lerp(fxColor, Color.White, 0.4f)
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }
                        }
                        break;
                    case 5: // 幻术粉红（折射幻影 · 粉色残像）
                        {
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float t = (float)Main.GameUpdateCount * 0.1f;

                            // ——① 幻影残像（多个 GlowOrb，轻微相位偏移）——
                            for (int i = -1; i <= 1; i++)
                            {
                                if (i == 0) continue; // 主体不生成，左右偏移
                                float offset = (float)System.Math.Sin(t + i) * 6f; // 左右摆动
                                Vector2 p = headPosition + n * offset;

                                if (Main.rand.NextBool(3))
                                {
                                    GlowOrbParticle orb = new GlowOrbParticle(
                                        p,
                                        Vector2.Zero,
                                        false,
                                        12,
                                        0.85f,
                                        Color.Lerp(fxColor, Color.White, 0.25f), // 幻术粉红
                                        true, false, true
                                    );
                                    GeneralParticleHandler.SpawnParticle(orb);
                                }
                            }

                            // ——② 镜像折射（对称 EXO 光粒，像幻象的光流）——
                            if (Main.rand.NextBool(2))
                            {
                                for (int j = -1; j <= 1; j += 2) // 左右对称
                                {
                                    Vector2 p = headPosition + n * j * Main.rand.NextFloat(4f, 8f);
                                    SquishyLightParticle exo = new SquishyLightParticle(
                                        p,
                                        f * Main.rand.NextFloat(0.2f, 0.5f),
                                        0.24f,
                                        Color.Lerp(fxColor, Color.White, 0.3f),
                                        18,
                                        opacity: 0.85f,
                                        squishStrenght: 1.1f,
                                        maxSquish: 2.5f,
                                        hueShift: 0f
                                    );
                                    GeneralParticleHandler.SpawnParticle(exo);
                                }
                            }

                            // ——③ 幻象波纹：粉色半透明脉冲（短寿命）——
                            if (Main.GameUpdateCount % 20 == 0)
                            {
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.6f,
                                    Color.Lerp(fxColor, Color.White, 0.4f),
                                    new Vector2(1.0f, 2.5f),
                                    Projectile.rotation,
                                    0.15f,
                                    0.03f,
                                    14 // 短寿命
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }

                            // ——④ 粉色轻雾，像梦境蒸汽——
                            if (Main.rand.NextBool(5))
                            {
                                var mist = new WaterFlavoredParticle(
                                    headPosition + Main.rand.NextVector2Circular(2f, 2f),
                                    -f * 0.25f,
                                    false,
                                    Main.rand.Next(14, 18),
                                    0.9f + Main.rand.NextFloat(0.2f),
                                    Color.Lerp(fxColor, Color.White, 0.4f)
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }
                        }
                        break;
                    case 6: // 封印金色（符文法阵 · 神圣封印）
                        {
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float t = (float)Main.GameUpdateCount * 0.08f;

                            // ——① 符文环（Square + Orb 环绕 headPosition）——
                            int runeCount = 6;
                            float radius = 12f + (float)System.Math.Sin(t * 1.6f) * 2f; // 呼吸半径
                            for (int i = 0; i < runeCount; i++)
                            {
                                float ang = MathHelper.TwoPi * i / runeCount + t;
                                Vector2 offset = f * (float)System.Math.Cos(ang) * radius + n * (float)System.Math.Sin(ang) * radius;
                                Vector2 p = headPosition + offset;

                                if (Main.rand.NextBool(2))
                                {
                                    SquareParticle sq = new SquareParticle(
                                        p,
                                        Vector2.Zero,
                                        false,
                                        20,
                                        1.2f,
                                        Color.Lerp(fxColor, Color.White, 0.25f)
                                    );
                                    GeneralParticleHandler.SpawnParticle(sq);
                                }

                                if (Main.rand.NextBool(4))
                                {
                                    GlowOrbParticle orb = new GlowOrbParticle(
                                        p,
                                        Vector2.Zero,
                                        false,
                                        10,
                                        0.7f,
                                        Color.Lerp(fxColor, Color.White, 0.2f),
                                        true, false, true
                                    );
                                    GeneralParticleHandler.SpawnParticle(orb);
                                }
                            }

                            // ——② 神圣脉冲：偶尔放出一圈金色能量波——
                            if (Main.GameUpdateCount % 24 == 0)
                            {
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.7f,
                                    Color.Lerp(fxColor, Color.White, 0.35f),
                                    new Vector2(1.2f, 2.4f),
                                    Projectile.rotation,
                                    0.2f,
                                    0.03f,
                                    22
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }

                            // ——③ 符文能量残痕（EXO 火花）——
                            if (Main.rand.NextBool(3))
                            {
                                SquishyLightParticle exo = new SquishyLightParticle(
                                    headPosition + Main.rand.NextVector2Circular(2f, 2f),
                                    f * Main.rand.NextFloat(0.2f, 0.6f),
                                    0.25f,
                                    Color.Lerp(fxColor, Color.White, 0.15f),
                                    18,
                                    opacity: 1f,
                                    squishStrenght: 1.1f,
                                    maxSquish: 2.8f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exo);
                            }

                            // ——④ 金色雾气，往后逸散——
                            if (Main.rand.NextBool(5))
                            {
                                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                                    headPosition,
                                    -f * 0.3f,
                                    false,
                                    Main.rand.Next(16, 22),
                                    0.9f + Main.rand.NextFloat(0.3f),
                                    Color.Lerp(fxColor, Color.White, 0.4f)
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }

                            // ——⑤ 稀有时刻：极亮的 StrongBloom（神圣爆光），点到为止——
                            if (Main.GameUpdateCount % 40 == 0 && Main.rand.NextBool(3))
                            {
                                StrongBloom strong = new StrongBloom(
                                    headPosition,
                                    Vector2.Zero,
                                    Color.Lerp(fxColor, Color.White, 0.3f),
                                    1.8f,
                                    40
                                );
                                GeneralParticleHandler.SpawnParticle(strong);
                            }
                        }
                        break;

                    case 7: // 修真白色（剑气 · 灵气 · 真气）
                        {
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float t = (float)Main.GameUpdateCount * 0.09f;

                            // ——① 剑气残痕（细长白光，锐利）——
                            if (Main.rand.NextBool(2))
                            {
                                SquishyLightParticle swordQi = new SquishyLightParticle(
                                    headPosition,
                                    f.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(0.5f, 1.2f),
                                    0.26f,
                                    Color.Lerp(fxColor, Color.White, 0.3f),
                                    20,
                                    opacity: 1f,
                                    squishStrenght: 1.2f,
                                    maxSquish: 3.2f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(swordQi);
                            }

                            // ——② 灵气光点（白色小光球，轻微浮动）——
                            if (Main.rand.NextBool(3))
                            {
                                GlowOrbParticle orb = new GlowOrbParticle(
                                    headPosition + n * (float)System.Math.Sin(t) * 4f,
                                    Vector2.Zero,
                                    false,
                                    12,
                                    0.85f,
                                    Color.Lerp(fxColor, Color.White, 0.2f),
                                    true, false, true
                                );
                                GeneralParticleHandler.SpawnParticle(orb);
                            }

                            // ——③ 仙雾缥缈（白色雾气，往后逸散）——
                            if (Main.rand.NextBool(4))
                            {
                                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                                    headPosition + Main.rand.NextVector2Circular(2f, 2f),
                                    -f * Main.rand.NextFloat(0.3f, 0.6f),
                                    false,
                                    Main.rand.Next(16, 22),
                                    0.9f + Main.rand.NextFloat(0.3f),
                                    Color.Lerp(fxColor, Color.White, 0.5f)
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }

                            // ——④ 剑势脉冲（白色脉冲环）——
                            if (Main.GameUpdateCount % 22 == 0)
                            {
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.7f,
                                    Color.Lerp(fxColor, Color.White, 0.35f),
                                    new Vector2(1.0f, 2.6f),
                                    Projectile.rotation,
                                    0.18f,
                                    0.03f,
                                    20
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }
                        }
                        break;
                    case 8: // 幻想紫藤色（花瓣洒落 · 曼陀罗旋涡）
                        {
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float t = (float)Main.GameUpdateCount * 0.07f;

                            // ——① 花瓣粒子（Square 拉长）——
                            if (Main.rand.NextBool(2))
                            {
                                SquareParticle petal = new SquareParticle(
                                    headPosition + Main.rand.NextVector2Circular(3f, 3f),
                                    -f * Main.rand.NextFloat(0.2f, 0.5f),
                                    false,
                                    Main.rand.Next(16, 22),
                                    1.1f + Main.rand.NextFloat(0.4f),
                                    Color.Lerp(fxColor, Color.White, 0.3f)
                                );
                                GeneralParticleHandler.SpawnParticle(petal);
                            }

                            // ——② 曼陀罗旋涡（GlowOrb 轨迹）——
                            int orbCount = 3;
                            float radius = 6f + (float)System.Math.Sin(t * 1.8f) * 2f;
                            for (int i = 0; i < orbCount; i++)
                            {
                                float ang = t + MathHelper.TwoPi * i / orbCount;
                                Vector2 offset = f * (float)System.Math.Cos(ang) * radius + n * (float)System.Math.Sin(ang) * radius;
                                GlowOrbParticle orb = new GlowOrbParticle(
                                    headPosition + offset,
                                    Vector2.Zero,
                                    false,
                                    10,
                                    0.75f,
                                    Color.Lerp(fxColor, Color.White, 0.2f),
                                    true, false, true
                                );
                                GeneralParticleHandler.SpawnParticle(orb);
                            }

                            // ——③ 梦幻能量（EXO 粒子）——
                            if (Main.rand.NextBool(3))
                            {
                                SquishyLightParticle exo = new SquishyLightParticle(
                                    headPosition,
                                    f * Main.rand.NextFloat(0.3f, 0.7f),
                                    0.24f,
                                    Color.Lerp(fxColor, Color.White, 0.25f),
                                    18,
                                    opacity: 0.9f,
                                    squishStrenght: 1.1f,
                                    maxSquish: 2.6f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exo);
                            }

                            // ——④ 脉冲花圈（淡紫色扩散环）——
                            if (Main.GameUpdateCount % 26 == 0)
                            {
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.65f,
                                    Color.Lerp(fxColor, Color.White, 0.35f),
                                    new Vector2(1.0f, 2.4f),
                                    Projectile.rotation,
                                    0.16f,
                                    0.03f,
                                    20
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }

                            // ——⑤ 梦幻雾气（淡紫蒸汽）——
                            if (Main.rand.NextBool(6))
                            {
                                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                                    headPosition + Main.rand.NextVector2Circular(2f, 2f),
                                    -f * Main.rand.NextFloat(0.2f, 0.5f),
                                    false,
                                    Main.rand.Next(14, 20),
                                    0.85f + Main.rand.NextFloat(0.3f),
                                    Color.Lerp(fxColor, Color.White, 0.4f)
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }
                        }
                        break;

                    case 9: // 机制白红色（机械齿轮 · 过载能量）
                        {
                            Vector2 f = Projectile.velocity.LengthSquared() > 1e-4f
                                ? Vector2.Normalize(Projectile.velocity)
                                : Vector2.UnitX.RotatedBy(Projectile.rotation);
                            Vector2 n = new Vector2(-f.Y, f.X);

                            float t = (float)Main.GameUpdateCount * 0.1f;

                            // ——① 齿轮碎片感（Square 粒子）——
                            if (Main.rand.NextBool(2))
                            {
                                SquareParticle gear = new SquareParticle(
                                    headPosition + Main.rand.NextVector2Circular(2f, 2f),
                                    f.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.3f, 0.7f),
                                    false,
                                    18,
                                    1.1f + Main.rand.NextFloat(0.5f),
                                    Color.Lerp(fxColor, Color.White, 0.25f)
                                );
                                GeneralParticleHandler.SpawnParticle(gear);
                            }

                            // ——② 过载火花（EXO 光粒 + GlowOrb）——
                            if (Main.rand.NextBool(3))
                            {
                                SquishyLightParticle exo = new SquishyLightParticle(
                                    headPosition,
                                    n * Main.rand.NextFloat(-0.6f, 0.6f),
                                    0.26f,
                                    Color.Lerp(Color.White, fxColor, 0.6f),
                                    20,
                                    opacity: 1f,
                                    squishStrenght: 1.2f,
                                    maxSquish: 3.0f,
                                    hueShift: 0f
                                );
                                GeneralParticleHandler.SpawnParticle(exo);
                            }

                            if (Main.rand.NextBool(4))
                            {
                                GlowOrbParticle orb = new GlowOrbParticle(
                                    headPosition + Main.rand.NextVector2Circular(2f, 2f),
                                    Vector2.Zero,
                                    false,
                                    10,
                                    0.8f,
                                    fxColor,
                                    true, false, true
                                );
                                GeneralParticleHandler.SpawnParticle(orb);
                            }

                            // ——③ 能量脉冲（机械震荡波）——
                            if (Main.GameUpdateCount % 24 == 0)
                            {
                                Particle pulse = new DirectionalPulseRing(
                                    headPosition,
                                    f * 0.7f,
                                    Color.Lerp(Color.White, fxColor, 0.4f),
                                    new Vector2(1.0f, 2.0f),
                                    Projectile.rotation,
                                    0.18f,
                                    0.03f,
                                    20
                                );
                                GeneralParticleHandler.SpawnParticle(pulse);
                            }

                            // ——④ 蒸汽排气（白色雾气）——
                            if (Main.rand.NextBool(5))
                            {
                                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                                    headPosition,
                                    -f * Main.rand.NextFloat(0.3f, 0.6f),
                                    false,
                                    Main.rand.Next(16, 22),
                                    0.9f + Main.rand.NextFloat(0.3f),
                                    Color.White * 0.8f
                                );
                                GeneralParticleHandler.SpawnParticle(mist);
                            }

                            // ——⑤ 过载警告闪光（StrongBloom）——
                            if (Main.GameUpdateCount % 36 == 0 && Main.rand.NextBool(3))
                            {
                                StrongBloom strong = new StrongBloom(
                                    headPosition,
                                    Vector2.Zero,
                                    Color.Lerp(Color.Red, Color.White, 0.2f),
                                    1.8f,
                                    40
                                );
                                GeneralParticleHandler.SpawnParticle(strong);
                            }
                        }
                        break;

                    // ……这里继续写 case 3~9，不同的 Dust / 粒子组合

                    default:
                        if (Main.rand.NextBool(4))
                        {
                            Dust d = Dust.NewDustPerfect(headPosition, DustID.Smoke,
                                Vector2.Zero, 100, fxColor, 1f);
                            d.noGravity = true;
                        }
                        break;
                }
            }



 
 */