using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;

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

            // 1. 绘制拖尾（原逻辑保留）
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Color color = i % 2 == 0 ? Color.Yellow : Color.LightGreen;
                color *= 0.6f;
                color.A = 0;

                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f
                                       - Main.screenPosition
                                       + new Vector2(0f, Projectile.gfxOffY);

                Main.EntitySpriteDraw(
                    texture,
                    drawPosition,
                    null,
                    color,
                    Projectile.rotation,
                    texture.Size() * 0.5f,
                    0.8f,
                    SpriteEffects.None,
                    0
                );
            }

            // 2. 在武器头部绘制 Extra_89 脉动圆环
            Texture2D ringTex = Terraria.GameContent.TextureAssets.Extra[89].Value;

            // 计算武器头部位置（基于朝向和贴图一半宽度）
            Vector2 headOffset = Projectile.velocity.SafeNormalize(Vector2.UnitY)
                                 * (texture.Width * 0.5f * 0.8f); // 0.8f = 上面拖尾缩放
            Vector2 headPos = Projectile.Center + headOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            float pulse = 1f + 0.08f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * 0.6f;
                Color ringColor = Color.AliceBlue * 1.3f; // 改成亮绿色
                float scale = (0.25f + 0.05f * i) * pulse * 2.5f;

                Main.EntitySpriteDraw(
                    ringTex,
                    headPos,
                    null,
                    ringColor,
                    angle,
                    ringTex.Size() * 0.5f,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            return false; // 告诉游戏我们自己画完了
        }


        public static class SunsetBForgetParticleManager
        {
            public static readonly int[] YellowDusts = { 169, 159, 133 };
            public static readonly int[] BlueDusts = { 80, 67, 48 }; 
            public static readonly int[] GreenDusts = { 3, 46, 89, 128 };
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }








        public override void AI()
        {
            // ===== 统一朝向 =====
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // ===== 参数区（可随时调）=====
            float coreTrailScale = 2.0f;    // 主干线性粒子粗细
            int coreTrailLife = 42;      // 主干线性粒子寿命
            float sideWaveAmp = 10f;     // 两侧波浪振幅（像藤蔓一样摆）
            float sideWaveFreq = 0.28f;   // 两侧波浪频率（越大摆得越密）
            int sideWaveLife = 24;      // 两侧波浪寿命
            float ringRadius = 14f;     // 小环起始半径（螺旋爆发层）
            float ringSpeed = 2.6f;    // 小环外扩速度
            int ringCount = 6;       // 小环点数
            int ringEveryFrames = 10;      // 每隔多少帧放一圈
            float squareScale = 1.8f;    // 科技方块大小
            int squareLife = 26;      // 科技方块寿命
            int mistLifeMin = 16;      // 水味寿命下限
            int mistLifeMax = 22;      // 水味寿命上限
            float smokeScaleMin = 0.28f;   // 深色烟雾缩放（尽量克制用量）
            float smokeScaleMax = 0.6f;

            // ===== 植物科技·蓝(8)：紫(2) 调色器 =====
            Color[] blues = {
        Color.DeepSkyBlue,
        Color.CornflowerBlue,
        new Color(80, 180, 255),
        Color.Cyan
    };
            Color[] purples = {
        Color.MediumPurple,
        new Color(170, 120, 235),
        new Color(140, 100, 210)
    };
            const float purpleRatio = 0.20f; // 紫色占比 20%
            Color PickTechColor()
            {
                Color c = (Main.rand.NextFloat() < purpleRatio)
                    ? purples[Main.rand.Next(purples.Length)]
                    : blues[Main.rand.Next(blues.Length)];
                // 稍加一点白，显“能量感”
                return Color.Lerp(c, Color.White, 0.18f);
            }

            // ===== 基向量与时间相位 =====
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            float forwardAngle = forward.ToRotation();
            float t = (float)Main.GameUpdateCount * 0.06f;

            // =========================================================
            // A. 主干能量束（强力蓝、少量紫）——厚重+稳定（植物科技主干）
            // =========================================================
            if (Main.rand.NextBool(2))
            {
                Particle core = new SparkParticle(
                    Projectile.Center,
                    forward * 0.02f,             // 沿前进方向极慢推进，强调“拖曳能量”
                    false,
                    coreTrailLife,
                    coreTrailScale,
                    PickTechColor()
                );
                core.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(core);
            }

            // =========================================================
            // B. 双股侧藤（波浪并行）——左右两条，同步但反相，曲线更优雅
            // =========================================================
            for (int i = 0; i < 2; i++)
            {
                float sideSign = (i == 0) ? +1f : -1f;
                // 正弦波：相位与 whoAmI 叠加，避免同屏同质
                float wave = (float)Math.Sin(t * (1f + 0.35f * Projectile.whoAmI) + sideSign * MathHelper.PiOver2);
                Vector2 lateral = forward.RotatedBy(MathHelper.PiOver2) * (sideWaveAmp * wave);

                var waveLine = new AltSparkParticle(
                    Projectile.Center + lateral,
                    forward * 0.02f,
                    false,
                    sideWaveLife,
                    1.6f,                         // 比主干略细
                    PickTechColor()
                );
                waveLine.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(waveLine);
            }

            // =========================================================
            // C. 科技碎片（方块）——黄金角分布，连贯却不凌乱
            // =========================================================
            if (Main.rand.NextBool(3))
            {
                // 黄金角分布，显“数学美感”
                const float golden = 2.39996323f; // 约等于 π*(3-√5)
                float k = (Projectile.whoAmI * 1.618f + Main.GameUpdateCount * 0.15f) % 17;
                float ang = golden * k;
                Vector2 radial = ang.ToRotationVector2() * Main.rand.NextFloat(4f, 12f);

                var sq = new SquareParticle(
                    Projectile.Center + radial,
                    forward * Main.rand.NextFloat(0.6f, 1.3f),
                    false,
                    squareLife,
                    squareScale,
                    PickTechColor()
                );
                sq.Rotation = forwardAngle + Main.rand.NextFloat(-0.25f, 0.25f);
                GeneralParticleHandler.SpawnParticle(sq);
            }

            // =========================================================
            // D. 螺旋小环（点阵外抛）——每 N 帧从半径 r 的环上同时外抛
            // =========================================================
            if (Main.GameUpdateCount % ringEveryFrames == 0)
            {
                float baseRot = t * 1.4f; // 整体旋转，形成“螺旋感”
                for (int j = 0; j < ringCount; j++)
                {
                    float angle = baseRot + MathHelper.TwoPi * j / ringCount;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * ringRadius;
                    Vector2 vel = angle.ToRotationVector2() * ringSpeed * Main.rand.NextFloat(0.85f, 1.15f);

                    // 采用两种粒子交替：线性粒子/水味
                    if (j % 2 == 0)
                    {
                        var sp = new SparkParticle(
                            pos, vel, false,
                            24,
                            1.0f,
                            PickTechColor()
                        );
                        sp.Rotation = angle;
                        GeneralParticleHandler.SpawnParticle(sp);
                    }
                    else
                    {
                        var mist = new WaterFlavoredParticle(
                            pos, vel, false,
                            Main.rand.Next(mistLifeMin, mistLifeMax),
                            0.9f + Main.rand.NextFloat(0.25f),
                            PickTechColor() * 0.9f
                        );
                        GeneralParticleHandler.SpawnParticle(mist);
                    }
                }
            }

            // =========================================================
            // E. 背景能雾（水味）——稀疏大片，烘托体积感
            // =========================================================
            if (Main.rand.NextBool(3))
            {
                var mist = new WaterFlavoredParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    forward.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.4f, 0.9f),
                    false,
                    Main.rand.Next(mistLifeMin, mistLifeMax),
                    1.2f + Main.rand.NextFloat(0.3f),
                    PickTechColor() * 0.85f
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }

            // =========================================================
            // F. 深色烟（极少量）——只作层次，不喧宾夺主
            // =========================================================
            if (Main.rand.NextBool(6))
            {
                var smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    forward.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.5f, 1.2f),
                    new Color(40, 60, 90), // 深蓝灰
                    22,
                    Main.rand.NextFloat(smokeScaleMin, smokeScaleMax),
                    0.55f,
                    Main.rand.NextFloat(-1f, 1f),
                    true
                );
                smoke.Rotation = forwardAngle;
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // =========================================================
            // G. 轻点尘（Dust）——科技冷色调点缀
            // =========================================================
            if (Main.rand.NextBool(4))
            {
                int dustType = Main.rand.NextBool() ? DustID.DungeonWater : DustID.BlueTorch;
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    dustType,
                    forward.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.8f, 1.6f),
                    120,
                    PickTechColor(),
                    Main.rand.NextFloat(1.1f, 1.4f)
                );
                d.noGravity = true;
                d.rotation = forwardAngle;
                d.fadeIn = 0.8f;
            }
        }








        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 spawnPosition = target.Center;

            // 计算随机触手数量（3~6个）
            int tentacleCount = Main.rand.Next(3, 7);

            // 计算单个触手的伤害（总伤害固定为 1.0 倍）
            int individualDamage = (int)(Projectile.damage / (float)tentacleCount);

            // 生成多个随机方向的触手
            for (int i = 0; i < tentacleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi); // 0° 到 360° 全角度随机
                Vector2 tentacleVelocity = randomAngle.ToRotationVector2() * 4f; // 全方向随机扩散

                SpawnGreenTentacle(tentacleVelocity, individualDamage);
            }

            // 播放撞击音效
            SoundEngine.PlaySound(SoundID.Item74, Projectile.position);

            // 施加 Debuff 给敌人
            target.AddBuff(ModContent.BuffType<SunsetBForgetEDebuff>(), 300); // 5 秒

            // 施加 Buff 给玩家
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetBForgetPBuff>(), 300); // 5 秒
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