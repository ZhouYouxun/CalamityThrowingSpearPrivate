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
using System.Collections.Generic;

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
        private int initTimeLeft;

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            initTimeLeft = Projectile.timeLeft; // 记录出生时的最大寿命

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


            {
                // === 淡入淡出效果 ===
                int fadeInTime = 90;   // 淡入时间（帧）
                int fadeOutTime = 40;  // 淡出时间（帧）

                // 淡入：前 fadeInTime 帧从 0 → 1
                float fadeInFactor = Utils.GetLerpValue(0f, fadeInTime, 36000 - Projectile.timeLeft, true);
                Projectile.Opacity = MathHelper.Clamp(fadeInFactor, 0f, 1f);

                // 淡出：最后 fadeOutTime 帧逐渐变透明
                if (Projectile.timeLeft < fadeOutTime)
                {
                    float fadeOutFactor = Projectile.timeLeft / (float)fadeOutTime;
                    Projectile.Opacity *= fadeOutFactor;
                }
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
            //Projectile.timeLeft = 6000;






            {
                // ======================= 下面是“科技蓝系”三类特效 =======================
                vfxTick++;
                vfxPhase += 0.25f;

                Vector2 center = Projectile.Center;

                // --- 新增淡入系数 ---
                int fadeInTime = 60;
                // 已经过了多少帧
                int elapsed = initTimeLeft - Projectile.timeLeft;
                float fxFactor = Utils.GetLerpValue(0f, fadeInTime, elapsed, true);

                // ---------------- ① ConstellationRingVFX ----------------
                if (Main.rand.NextFloat() < 0.2f * fxFactor) // 频率受 fxFactor 控制
                {
                    Color techBlue = TechBluePalette[Main.rand.Next(0, 4)] * 0.9f;

                    ConstellationRingVFX constellationRing = new ConstellationRingVFX(
                        center,
                        techBlue,
                        Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                        1.7f,
                        new Vector2(1f, 1f),
                        0.9f * fxFactor, // 透明度也受控制
                        5,
                        1.5f,
                        0.06f,
                        false
                    );
                    GeneralParticleHandler.SpawnParticle(constellationRing);
                }

                // ---------------- ② CrackParticle ----------------
                int crackPerFrame = (int)(fxFactor * Main.rand.Next(3, 6)); // 数量随 fxFactor 增加
                for (int n = 0; n < crackPerFrame; n++)
                {
                    float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 dir = ang.ToRotationVector2();
                    Vector2 pos = center + dir * (3f * 16f);
                    float spd = Main.rand.NextFloat(2.8f, 5.6f);

                    Color c = TechBluePalette[Main.rand.Next(TechBluePalette.Length)];

                    CrackParticle crack = new CrackParticle(
                        pos,
                        dir * spd,
                        c,
                        new Vector2(1f + Main.rand.NextFloat(-0.15f, 0.25f),
                                    1f + Main.rand.NextFloat(-0.15f, 0.25f)),
                        Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi),
                        Main.rand.NextFloat(0.10f, 0.20f),
                        Main.rand.NextFloat(0.3f, 0.5f),
                        Main.rand.Next(13, 26)
                    );
                    GeneralParticleHandler.SpawnParticle(crack);
                }

                // ---------------- ③ AltSparkParticle 双螺旋 ----------------
                if (Main.rand.NextFloat() < 0.8f * fxFactor) // 前期几乎不会出现，后面正常
                {
                    float rHelix = 28f;
                    float a1 = vfxPhase;
                    float a2 = vfxPhase + MathHelper.Pi;

                    Vector2 n1 = a1.ToRotationVector2();
                    Vector2 n2 = a2.ToRotationVector2();

                    Vector2 p1 = center + n1 * rHelix;
                    Vector2 p2 = center + n2 * rHelix;

                    Vector2 tang1 = n1.RotatedBy(MathHelper.PiOver2) * 0.6f;
                    Vector2 tang2 = n2.RotatedBy(MathHelper.PiOver2) * 0.6f;

                    Color helixColor1 = TechBluePalette[Main.rand.Next(0, 4)] * 0.135f;
                    Color helixColor2 = TechBluePalette[Main.rand.Next(0, 4)] * 0.135f;

                    AltSparkParticle spark1 = new AltSparkParticle(
                        p1, tang1, false,
                        10,
                        Main.rand.NextFloat(1.1f, 1.35f),
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


        public override void DrawBehind(int index, List<int> behindNPCsAndTiles,
    List<int> behindNPCs, List<int> behindProjectiles,
    List<int> overPlayers, List<int> overWiresUI)
        {
            // 魔法阵始终算作“下层投射物”
            behindProjectiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // ========== 可调参数（全部集中在这里，按需改） ==========
            float baseAlpha = 0.5f;   // 基础透明度

            // —— Concept 系列（大底图层）——
            float conceptScale = 0.30f;
            float conceptASpin = 0.50f;
            float conceptBSpin = -0.70f;
            float conceptCSpin = 0.90f;

            // —— Twirl 花瓣环 ——
            float twirlScale = 0.90f;
            float twirlRadius = 90f;
            float twirlSpin = -0.30f;

            // —— Magic 中层能量 ——
            float magic1Scale = 0.80f;
            float magic2Scale = 0.70f;
            float magic1Spin = 1.50f;
            float magic2Spin = -1.80f;

            // —— 星点环 ——
            int starCount = 8;
            float starScale = 0.80f;
            float starRingRadius = 100f;
            float starSpin = 0.60f;

            // —— 原有外圈/内圈 ——
            float outerScale = 1.00f;
            float innerScale = 0.90f;
            float outerSpin = 1.00f;
            float innerSpin = -1.20f;

            // ✅ 用 AI 中累积的 RotationAngle 作为全局相位
            float rot = RotationAngle;

            // ========== 淡入淡出控制 ==========
            int fadeInTime = 90;
            int fadeOutTime = 40;

            // 已经过了多少帧（需要在 OnSpawn 里保存 initTimeLeft）
            int elapsed = initTimeLeft - Projectile.timeLeft;

            float fadeInFactor = Utils.GetLerpValue(0f, fadeInTime, elapsed, true);
            float fadeOutFactor = Projectile.timeLeft > fadeOutTime
                ? 1f
                : (Projectile.timeLeft / (float)fadeOutTime);

            float fadeFactor = fadeInFactor * fadeOutFactor;
            float globalAlpha = baseAlpha * fadeFactor;

            // ========== 颜色（金色 + 紫色） ==========
            Color mainGold = new Color(255, 215, 0) * globalAlpha;
            Color accentGold = new Color(255, 215, 0) * (globalAlpha * 0.8f);
            Color mainPurple = new Color(120, 90, 160) * globalAlpha;
            Color accentPurple = new Color(120, 90, 160) * (globalAlpha * 0.8f);

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

            // ========== 绘制：Concept 大底图层 ==========
            Main.EntitySpriteDraw(texConceptA, drawPos, null, mainGold, rot * conceptASpin, texConceptA.Size() / 2f, conceptScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texConceptB, drawPos, null, mainPurple, rot * conceptBSpin, texConceptB.Size() / 2f, conceptScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texConceptC, drawPos, null, accentGold, rot * conceptCSpin, texConceptC.Size() / 2f, conceptScale, SpriteEffects.None, 0);

            // ========== 绘制：Twirl 花瓣环 ==========
            for (int i = 0; i < 6; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / 6f + rot * twirlSpin;
                Vector2 pos = drawPos + baseAngle.ToRotationVector2() * twirlRadius;

                Texture2D twirlTex = (i % 3 == 0) ? texTwirl1 : (i % 3 == 1) ? texTwirl2 : texTwirl3;
                Color twirlColor = (i % 2 == 0) ? mainGold : mainPurple;

                Main.EntitySpriteDraw(twirlTex, pos, null, twirlColor, baseAngle, twirlTex.Size() / 2f, twirlScale, SpriteEffects.None, 0);
            }

            // ========== 绘制：Magic 能量核 ==========
            Main.EntitySpriteDraw(texMagic1, drawPos, null, mainPurple, rot * magic1Spin, texMagic1.Size() / 2f, magic1Scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texMagic2, drawPos, null, accentGold, rot * magic2Spin, texMagic2.Size() / 2f, magic2Scale, SpriteEffects.None, 0);

            // ========== 绘制：星点环 ==========
            for (int i = 0; i < starCount; i++)
            {
                float a = MathHelper.TwoPi * i / starCount + rot * starSpin;
                Vector2 sp = drawPos + a.ToRotationVector2() * starRingRadius;

                Texture2D starTex = (i % 2 == 0) ? texStar7 : texStar8;
                Color starColor = (i % 2 == 0) ? mainGold : mainPurple;

                float pulse = 0.92f + 0.08f * (float)Math.Sin(rot * 2f + i * 1.3f);
                Main.EntitySpriteDraw(starTex, sp, null, starColor, a, starTex.Size() / 2f, starScale * pulse, SpriteEffects.None, 0);
            }

            // ========== 绘制：外圈/内圈 ==========
            Main.EntitySpriteDraw(texOuter, drawPos, null, mainGold, rot * outerSpin, texOuter.Size() / 2f, outerScale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texInner, drawPos, null, mainPurple, rot * innerSpin, texInner.Size() / 2f, innerScale, SpriteEffects.None, 0);

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
