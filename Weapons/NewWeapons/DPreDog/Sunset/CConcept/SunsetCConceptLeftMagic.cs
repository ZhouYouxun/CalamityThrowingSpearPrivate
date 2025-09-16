using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Drawing;
using CalamityMod;
using System;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptLeftMagic : ModProjectile
    {
        public int TargetNPCIndex = -1; // 目标敌人索引
        private float RotationAngle = 0f; // 旋转角度
        private const float OpacityIncreaseRate = 0.05f; // 淡入速度
        private const float InnerRotationSpeed = 0.03f; // 内圈旋转速度
        private const float OuterRotationSpeed = -0.02f; // 外圈旋转速度（反向）

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.ignoreWater = true;
            Projectile.Opacity = 0f; // 初始完全透明
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4; // 受击无敌帧
            Projectile.timeLeft = 36000;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }
        
        // 分批生成控制
        private const int SatellitesTotal = 10;   // 卫星总数
        private const int SpawnInterval = 5;     // 每隔5帧生成1个
        private int spawnedCount = 0;             // 已生成数量
        private int spawnTimer = 0;             // 帧计时器

        // （保留原来的颜色表以对齐索引；NoDamage 通常只要索引，不直接读这里的颜色）
        private static readonly Color[] RingColors = new Color[]
        {
            Color.Black, Color.White, Color.Green, new Color(255, 105, 180), // 蓝粉
            Color.Blue, Color.Gold, new Color(50, 0, 50),                    // 紫黑
            Color.Red, Color.Gray, Color.Silver
        };



        // ========== VFX（魔法阵AI特效）新增字段 ==========
        private int vfxTick = 0;      // 统一的帧计数
        private float vfxPhase = 0f;    // 用于双螺旋的相位推进

        // 科技蓝主色调 + 辅助白色
        private static readonly Color[] TechBluePalette = new Color[]
        {
    new Color( 80, 200, 255),  // Electric Blue
    new Color(120, 220, 255),  // Light Tech Blue
    new Color( 64, 180, 255),  // Deep Sky Blue
    Color.Cyan,                // 青色
    Color.WhiteSmoke           // 辅色（偏白）
        };

        public override void AI()
        {
            // **确保目标 NPC 存在**
            if (Projectile.ai[0] >= 0 && Main.npc[(int)Projectile.ai[0]].active)
            {
                TargetNPCIndex = (int)Projectile.ai[0];
            }
            else
            {
                // 目标死亡，弹幕立即消失
                Projectile.Kill();
                return;
            }




            NPC target = Main.npc[TargetNPCIndex];

            // **跟随目标**
            Projectile.Center = target.Center;

            // **淡入效果**
            if (Projectile.Opacity < 1f)
                Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + OpacityIncreaseRate, 0f, 1f);

            // **旋转**
            RotationAngle += OuterRotationSpeed;

            // **防止过早消失**
            Projectile.timeLeft = 6000;

            {
                // ======================= 下面是“科技蓝系”三类特效 =======================
                vfxTick++;
                vfxPhase += 0.25f; // 双螺旋的相位推进（越大越快）

                Vector2 center = Projectile.Center;

                // ---------------- ① ConstellationRingVFX（科技蓝）一直执行 ----------------
                // 从出生到消亡都持续；频率稍微节流以控量（每6帧一次）
                if ((vfxTick % 1) == 0)
                {
                    // 主色：科技蓝（调亮度系数更“电感”）
                    Color techBlue = TechBluePalette[Main.rand.Next(0, 4)] * 0.9f;

                    ConstellationRingVFX constellationRing = new ConstellationRingVFX(
                        center,                                     // 圆心 = 魔法阵中心
                        techBlue,                                   // 科技蓝
                        Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi), // 初始旋转
                        1.7f,                                       // 缩放（你指定的1.7）
                        new Vector2(1f, 1f),                        // 椭圆比
                        0.9f,                                       // 透明度
                        5,                                          // 星点数量
                        1.5f,                                       // 星点缩放
                        0.06f,                                      // 自转速度
                        false                                       // 是否重要
                    );
                    GeneralParticleHandler.SpawnParticle(constellationRing);
                }

                // ---------------- ② CrackParticle：半径≈3×16的环上向外“随机大量”喷发 ----------------
                // 以魔法阵原点为圆心，半径48的环上随机挑若干点，向外随机速度喷出
                int crackPerFrame = Main.rand.Next(3, 6); // 每帧3~5个，量感“足”但不爆
                for (int n = 0; n < crackPerFrame; n++)
                {
                    float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dir = ang.ToRotationVector2();      // 径向方向（向外）
                    Vector2 pos = center + dir * (3f * 16f);    // 半径≈48
                    float spd = Main.rand.NextFloat(2.8f, 5.6f);

                    // 科技蓝主色 + 少量白做高光
                    Color c = TechBluePalette[Main.rand.Next(TechBluePalette.Length)];

                    CrackParticle crack = new CrackParticle(
                        pos,                                      // 起点：圆环上
                        dir * spd,                                // 径向外喷
                        c,                                        // 科技蓝/白
                        new Vector2(1f + Main.rand.NextFloat(-0.15f, 0.25f),
                                    1f + Main.rand.NextFloat(-0.15f, 0.25f)), // 轻微拉伸
                        Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),   // 初始旋转
                        Main.rand.NextFloat(0.10f, 0.20f),        // 初始缩放
                        Main.rand.NextFloat(0.3f, 0.5f),          // 最终缩放
                        Main.rand.Next(13, 26)                    // 寿命：帧
                    );
                    GeneralParticleHandler.SpawnParticle(crack);
                }

                // ---------------- ③ AltSparkParticle：双螺旋围绕自身高速旋转 ----------------
                // 思路：我们不修改已生成粒子轨迹，而是“按相位”在两条相反臂位上持续生成短寿命粒子；
                // 看起来就是两条高速旋转的双螺旋“光线”。
                // 半径、寿命与速度都偏小，保证是“细线条”质感。
                {
                    float rHelix = 28f;                                     // 双螺旋半径（可按观感微调）
                    float a1 = vfxPhase;
                    float a2 = vfxPhase + MathHelper.Pi;                    // 另一臂相差180°

                    Vector2 n1 = a1.ToRotationVector2();
                    Vector2 n2 = a2.ToRotationVector2();

                    Vector2 p1 = center + n1 * rHelix;
                    Vector2 p2 = center + n2 * rHelix;

                    // 切向极小速度（形成短弧），让“旋转感”更明显
                    Vector2 tang1 = n1.RotatedBy(MathHelper.PiOver2) * 0.6f;
                    Vector2 tang2 = n2.RotatedBy(MathHelper.PiOver2) * 0.6f;

                    Color helixColor1 = (TechBluePalette[Main.rand.Next(0, 4)] * 0.135f); // 淡青蓝
                    Color helixColor2 = (TechBluePalette[Main.rand.Next(0, 4)] * 0.135f); // 淡青蓝

                    AltSparkParticle spark1 = new AltSparkParticle(
                        p1, tang1, false,
                        10,                                           // 8~10帧：短寿命细线
                        Main.rand.NextFloat(1.1f, 1.35f),             // 轻微缩放差异
                        helixColor1
                    );
                    GeneralParticleHandler.SpawnParticle(spark1);

                    AltSparkParticle spark2 = new AltSparkParticle(
                        p2, tang2, false,
                        10,
                        Main.rand.NextFloat(1.1f, 1.35f),
                        helixColor2
                    );
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
            }







        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = target.Center;

            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/空中分裂") with { Volume = 0.7f, Pitch = 0.0f }, Projectile.Center);


            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(16f, 16f);
                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.Keybrand,
                    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
                    Projectile.owner
                );
            }
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

            // **销毁 10 个 `NoDamage`**
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<SunsetCConceptLeftNoDamage>() && proj.ai[0] == Projectile.whoAmI)
                {
                    proj.Kill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // ========== 可调参数（全部集中在这里，按需改） ==========
            float globalAlpha = 0.5f;   // 全局半透明强度

            // —— Concept 系列（大底图层）——
            float conceptScale = 0.30f; // 三张统一缩放到 30%
            float conceptASpin = 0.50f; // 旋转速度倍率（相对于 RotationAngle）
            float conceptBSpin = -0.70f; // 反转
            float conceptCSpin = 0.90f;

            // —— Twirl 花瓣环 ——（每张贴图代表 60°花瓣，整体位置随相位旋转）
            float twirlScale = 0.90f;
            float twirlRadius = 90f;    // 花瓣圆环半径
            float twirlSpin = -0.30f; // 反转

            // —— Magic 中层能量 ——（两张相互反转）
            float magic1Scale = 0.80f;
            float magic2Scale = 0.70f;
            float magic1Spin = 1.50f;
            float magic2Spin = -1.80f; // 反转

            // —— 星点环（改为固定N个，沿环慢速公转，不再每帧随机角度）——
            int starCount = 8;     // 星点数量
            float starScale = 0.80f;
            float starRingRadius = 100f;  // 星点环半径
            float starSpin = 0.60f; // 星点随相位转动的倍率（越大越快）

            // —— 原有外圈/内圈 ——（相互反转）
            float outerScale = 1.00f;
            float innerScale = 0.90f;
            float outerSpin = 1.00f;
            float innerSpin = -1.20f; // 反转

            // ✅ 用 AI 中累积的 RotationAngle 作为全局相位（不要用 Projectile.rotation）
            float rot = RotationAngle;

            // ========== 颜色（科技蓝为主，辅以白与少量金/紫点缀） ==========
            Color mainBlue = Color.Cyan * globalAlpha;
            Color glowBlue = new Color(120, 220, 255) * globalAlpha;
            Color whiteSoft = Color.WhiteSmoke * globalAlpha;
            Color accentGold = Color.Gold * (globalAlpha * 0.5f);
            Color accentPurple = new Color(150, 100, 200) * (globalAlpha * 0.4f);

            // ========== 读取贴图 ==========
            Texture2D texConceptA = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptA").Value;
            Texture2D texConceptB = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptB").Value;
            Texture2D texConceptC = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/ConceptC").Value;

            Texture2D texTwirl1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_01").Value;
            Texture2D texTwirl2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_02").Value;
            Texture2D texTwirl3 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/twirl_03").Value;

            Texture2D texMagic1 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_01").Value;
            Texture2D texMagic2 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/magic_02").Value;

            Texture2D texStar7 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_07").Value;
            Texture2D texStar8 = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/star_08").Value;

            Texture2D texOuter = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D texInner = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/CConcept/SunsetCConceptLeftMagicInner").Value;

            // ========== 绘制：Concept 大底图层（系列内部互相反转） ==========
            Main.EntitySpriteDraw(texConceptA, drawPos, null, mainBlue, rot * conceptASpin, texConceptA.Size() / 2f, conceptScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texConceptB, drawPos, null, glowBlue, rot * conceptBSpin, texConceptB.Size() / 2f, conceptScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texConceptC, drawPos, null, whiteSoft, rot * conceptCSpin, texConceptC.Size() / 2f, conceptScale, SpriteEffects.None, 0);

            // ========== 绘制：周边 Twirl 花瓣环（整体反向旋转） ==========
            for (int i = 0; i < 6; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / 6f + rot * twirlSpin; // ✅ 用 rot
                Vector2 pos = drawPos + baseAngle.ToRotationVector2() * twirlRadius;

                Texture2D twirlTex = (i % 3 == 0) ? texTwirl1 : (i % 3 == 1) ? texTwirl2 : texTwirl3;
                Color twirlColor = (i % 2 == 0) ? mainBlue : accentPurple;

                // 贴图本身也按其朝向轻微旋转，增强“花瓣展开感”
                Main.EntitySpriteDraw(twirlTex, pos, null, twirlColor, baseAngle, twirlTex.Size() / 2f, twirlScale, SpriteEffects.None, 0);
            }

            // ========== 绘制：中层 Magic 能量核（互相反转） ==========
            Main.EntitySpriteDraw(texMagic1, drawPos, null, glowBlue, rot * magic1Spin, texMagic1.Size() / 2f, magic1Scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texMagic2, drawPos, null, accentGold, rot * magic2Spin, texMagic2.Size() / 2f, magic2Scale, SpriteEffects.None, 0);

            // ========== 绘制：星点环（固定数量，沿环缓慢公转，不再每帧随机角度） ==========
            for (int i = 0; i < starCount; i++)
            {
                float a = MathHelper.TwoPi * i / starCount + rot * starSpin; // ✅ 随 rot 平稳转动
                Vector2 sp = drawPos + a.ToRotationVector2() * starRingRadius;

                Texture2D starTex = (i % 2 == 0) ? texStar7 : texStar8;
                Color starColor = (i % 3 == 0) ? whiteSoft : glowBlue;

                // 小幅脉动（缩放轻微抖动），避免呆板
                float pulse = 0.92f + 0.08f * (float)Math.Sin(rot * 2f + i * 1.3f);
                Main.EntitySpriteDraw(starTex, sp, null, starColor, a, starTex.Size() / 2f, starScale * pulse, SpriteEffects.None, 0);
            }

            // ========== 原有外圈/内圈（互相反转） ==========
            Main.EntitySpriteDraw(texOuter, drawPos, null, glowBlue, rot * outerSpin, texOuter.Size() / 2f, outerScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texInner, drawPos, null, whiteSoft, rot * innerSpin, texInner.Size() / 2f, innerScale, SpriteEffects.None, 0);

            return false;
        }




        void restartShader(Texture2D texture, float opacity, float circularRotation, BlendState blendMode)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, blendMode, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            CalamityUtils.CalculatePerspectiveMatricies(out Matrix viewMatrix, out Matrix projectionMatrix);

            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseColor(Color.White);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uCircularRotation"].SetValue(circularRotation);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uWorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Apply();
        }

    }
}
