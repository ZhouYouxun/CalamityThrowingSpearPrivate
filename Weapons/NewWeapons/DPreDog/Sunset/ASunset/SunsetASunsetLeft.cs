using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
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
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetLeft : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            Texture2D tex = ModContent.Request<Texture2D>(
                Projectile.ModProjectile.Texture
            ).Value;

            Vector2 origin = tex.Size() * 0.5f;

            // ======== 太阳黑子橙色调色盘 ========
            Color[] firePalette = new Color[]
            {
        new Color(255, 200, 80),   // 金
        new Color(255, 150, 40),   // 橙
        new Color(255, 100, 30),   // 深橙
        new Color(255, 60, 20),    // 红橙
            };

            // ======== EXO 风格：能量丝带拖尾（Primitive Trail） ========
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			// === 定义宽度函数 ===
			float PrimitiveWidthFunction(float completionRatio, Vector2 vertexPos)
			{
				float w = Projectile.width * 3.55f;
				w *= MathHelper.SmoothStep(
					0.5f,
					1.0f,
					Utils.GetLerpValue(0f, 0.25f, completionRatio, true)
				);
				return w;
			}

			// === 定义颜色函数 ===
			Color PrimitiveTrailColor(float completionRatio, Vector2 vertexPos)
			{
				Color c = firePalette[
					(int)(completionRatio * firePalette.Length) % firePalette.Length
				];

				c *= Projectile.Opacity * (1f - completionRatio);

				float speedBoost =
					Utils.GetLerpValue(1f, 6f, Projectile.velocity.Length(), true);

				c *= speedBoost;

				c.A = 0;
				return c;
			}


			// === 将 oldPos 整体往前移动到“弹幕前端” ===
			Vector2 frontOffset = Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 1.5f);

            // 创建一个新的数组存前推后的 oldPos
            Vector2[] shiftedOldPos = new Vector2[Projectile.oldPos.Length];
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                shiftedOldPos[i] = Projectile.oldPos[i] + frontOffset;
            }


            // === 偏移：让丝带稍微抬起（增强立体感）===
            Vector2 PrimitiveOffsetFunction(float t, Vector2 vertexPos)
			{
                return Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.scale * 2f;
            }

            // === 绘制能量丝带（关键）=====
            GameShaders.Misc["CalamityMod:SideStreakTrail"].UseImage1("Images/Misc/Perlin");

            PrimitiveRenderer.RenderTrail(
                shiftedOldPos,
                new(
                    PrimitiveWidthFunction,
                    PrimitiveTrailColor,
                    PrimitiveOffsetFunction,
                    shader: GameShaders.Misc["CalamityMod:SideStreakTrail"]
                ),
                60
            );

            // ======== 回到正常绘图（主体绘制） ========
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                     DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // === 主体绘制 ===
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                sb.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            // ======== 第二层：普通虚化拖尾（oldPos-based fade trail） ========
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;

                // 透明度衰减
                float fade = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;

                Color c = new Color(255, 160, 60) * 0.35f * fade; // 柔和橙色虚光
                c.A = 0;

                float scale = Projectile.scale * (0.6f + fade * 0.4f);

                Main.spriteBatch.Draw(
                    tex,
                    pos,
                    null,
                    c,
                    Projectile.rotation,
                    tex.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0f
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
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.light = 0.5f;
            Projectile.scale = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // === 新太阳黑子调色盘（橙色 & 深褐色 权重提升） ===
            Color[] firePaletteSolar = new Color[]
            {
        new Color(255, 150, 40),  // 橙色 ×2 权重由双次加入
        new Color(255, 150, 40),

        new Color(255, 210, 90),  // 金色
        new Color(255, 235, 160), // 淡金色
        new Color(255, 250, 200), // 淡黄色

        new Color(180, 90, 30),   // 深褐 ×2
        new Color(180, 90, 30),
            };

            // === 基础方向 ===
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 backDir = -forward;

            // === 枪头 ===
            Vector2 headPos = Projectile.Center + forward * (Projectile.width * 0.55f);

            // === 时间因子（数学结构核心） ===
            float t = Main.GameUpdateCount * 0.12f;
            float pulse = (float)Math.Sin(t * 1.4f) * 0.5f + 0.5f;  // 节奏脉动 0~1
            float swirl = (float)Math.Sin(t * 0.8f) * 6f;          // 半螺旋偏移
            float micro = (float)Math.Cos(t * 2.5f) * 3f;          // 高频噪声微偏移

            // ============================================================
            // ① 主太阳喷流（SquishyLightParticle）── 半螺旋 + 羽状扩散
            // ============================================================
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 3; i++)
                {
                    Color c = firePaletteSolar[Main.rand.Next(firePaletteSolar.Length)];

                    // 半螺旋偏移（加数学结构）
                    Vector2 swirlOffset =
                        forward.RotatedBy(MathHelper.PiOver2) * (swirl + Main.rand.NextFloat(-3f, 3f));

                    Vector2 spawn = headPos + swirlOffset * 0.25f;

                    // 速度：主后喷 + 扰动
                    Vector2 vel = backDir
                        .RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f))
                        * Main.rand.NextFloat(1.1f, 3.1f);

                    SquishyLightParticle flare = new SquishyLightParticle(
                        spawn,
                        vel,
                        Main.rand.NextFloat(0.45f, 0.75f + pulse * 0.2f),
                        c,
                        Main.rand.Next(20, 36),
                        1f,
                        Main.rand.NextFloat(1.2f, 2.0f) + pulse * 0.3f
                    );

                    GeneralParticleHandler.SpawnParticle(flare);
                }
            }

            // ============================================================
            // ② 日珥式雾光（WaterFlavoredParticle）── 高温膨胀感
            // ============================================================
            if (Main.rand.NextBool(3))
            {
                Color c = firePaletteSolar[Main.rand.Next(firePaletteSolar.Length)];

                Vector2 vel = backDir * Main.rand.NextFloat(0.25f, 0.65f);

                // 羽状扩散
                Vector2 radialOffset = Main.rand.NextVector2Circular(4f + pulse * 2f, 4f + pulse * 2f);

                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    headPos + radialOffset,
                    vel,
                    false,
                    Main.rand.Next(24, 36),
                    0.9f + Main.rand.NextFloat(0.4f) + pulse * 0.1f,
                    c * Main.rand.NextFloat(0.7f, 1.0f)
                );

                GeneralParticleHandler.SpawnParticle(mist);
            }

            // ============================================================
            // ③ 高能耀斑火花（PointParticle）── 强能量爆裂
            // ============================================================
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 2; i++)
                {
                    Color c = firePaletteSolar[Main.rand.Next(firePaletteSolar.Length)];

                    Vector2 vel = backDir
                        .RotatedByRandom(MathHelper.ToRadians(18))
                        * Main.rand.NextFloat(1.6f, 3.3f);

                    PointParticle spark = new PointParticle(
                        headPos,
                        vel,
                        false,
                        20,
                        1.1f + Main.rand.NextFloat(0.4f),
                        c
                    );

                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // ============================================================
            // ④ 双螺旋 Dust（数学感最强）── 带微偏移
            // ============================================================
            float osc = (float)Math.Sin(t * 1.6f) * 1.2f + micro * 0.15f;

            Vector2 leftOffset = forward.RotatedBy(MathHelper.PiOver2) * osc;
            Vector2 rightOffset = forward.RotatedBy(-MathHelper.PiOver2) * osc;

            int dustType = DustID.GoldFlame;

            Dust.NewDustPerfect(
                headPos + leftOffset,
                dustType,
                backDir * Main.rand.NextFloat(0.4f, 0.7f),
                Scale: 1.2f + pulse * 0.1f
            ).noGravity = true;

            Dust.NewDustPerfect(
                headPos + rightOffset,
                dustType,
                backDir * Main.rand.NextFloat(0.4f, 0.7f),
                Scale: 1.2f + pulse * 0.1f
            ).noGravity = true;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SunsetPlayerSpeed.ApplyNoArmorHypothesisHitEffect(
                Projectile,
                target,
                ref modifiers
            );
        }

        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = Projectile.Center;
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/磁轨炮开火") with { Volume = 1.2f, Pitch = 0.0f }, Projectile.Center);


            // 屏幕震动效果
            float shakePower = 5f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            // 生成爆炸粒子
            Particle explosion = new DetailedExplosion(
                explosionPosition,
                Vector2.Zero,
                Color.OrangeRed * 0.9f,
                Vector2.One,
                Main.rand.NextFloat(-5, 5),
                0.1f * 2.5f, // 修改原始大小
                0.28f * 2.5f, // 修改最终大小
                10
            );
            GeneralParticleHandler.SpawnParticle(explosion);

            {
                Vector2 pos = Projectile.Center;

                // ================= 爆心闪光 =================
                for (int i = 0; i < 8; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                    SparkleParticle sparkle = new SparkleParticle(
                        pos + offset,
                        Vector2.Zero,
                        Color.Gold,                // 主色：金黄
                        Color.OrangeRed,           // 边缘：橙红
                        2.0f + Main.rand.NextFloat(0.5f),
                        10 + Main.rand.Next(5),
                        Main.rand.NextFloat(-0.05f, 0.05f),
                        2.2f
                    );
                    GeneralParticleHandler.SpawnParticle(sparkle);
                }

                // ================= 魔法阵外围光点 =================
                int ringCount = 12; // 环绕光点数量
                float radius = 48f;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCount;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    SparkleParticle sparkle = new SparkleParticle(
                        pos + offset,
                        Vector2.Zero,
                        Color.WhiteSmoke,
                        Color.Orange,
                        0.6f,
                        14,
                        0f,
                        1.8f
                    );
                    GeneralParticleHandler.SpawnParticle(sparkle);
                }




                // ================= 太阳核心冲击（Solar Core Flash） =================

                // 核心闪光：模拟太阳耀斑爆心
                for (int i = 0; i < 6; i++)
                {
                    Particle core = new GlowSparkParticle(
                        pos,
                        Main.rand.NextVector2Circular(0.5f, 0.5f), // 微小随机漂移
                        false,
                        6,                       // 生命周期
                        0.18f,                   // 粒子尺寸（越大越亮）
                        Color.Gold * 0.9f,       // 核心颜色（金色）
                        new Vector2(1.4f, 0.6f),  // 粒子形状（拉长）
                        true,
                        false,
                        1
                    );
                    GeneralParticleHandler.SpawnParticle(core);
                }


                // ================= 太阳喷流（Solar Flare Jet） =================

                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);

                for (int i = 0; i < 14; i++)
                {
                    Vector2 vel =
                        forward.RotatedByRandom(MathHelper.ToRadians(10f)) // 喷射角度范围
                        * Main.rand.NextFloat(4f, 9f);                       // 喷射速度

                    Particle jet = new GlowSparkParticle(
                        pos,
                        vel,
                        false,
                        Main.rand.Next(8, 12),               // 生命周期
                        Main.rand.NextFloat(0.12f, 0.18f),   // 粒子尺寸
                        new Color(255, 210, 80),              // 金黄色
                        new Vector2(2.4f, 0.45f),            // 长条形火焰
                        true,
                        false,
                        1
                    );

                    GeneralParticleHandler.SpawnParticle(jet);
                }


                // ================= 日冕爆发环（Solar Corona Ring） =================

                int coronaCount = 16;
                float coronaRadius = 10f;

                for (int i = 0; i < coronaCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / coronaCount;

                    Vector2 vel =
                        angle.ToRotationVector2()
                        * Main.rand.NextFloat(2f, 4f);

                    Particle corona = new GlowSparkParticle(
                        pos,
                        vel,
                        false,
                        10,
                        0.14f,
                        Color.Orange * 0.8f,
                        new Vector2(1.5f, 0.5f),
                        true,
                        false,
                        1
                    );

                    GeneralParticleHandler.SpawnParticle(corona);
                }

            }

            // 生成爆炸弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                explosionPosition,
                Vector2.Zero,
                ModContent.ProjectileType<FuckYou>(),
                (int)(Projectile.damage * 1.5f),
                Projectile.knockBack,
                Projectile.owner
            );

            target.AddBuff(ModContent.BuffType<SunsetASunsetEDebuff>(), 300); // 300 帧 = 5 秒
        }
    }
}