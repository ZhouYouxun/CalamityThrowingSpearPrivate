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
using Terraria.DataStructures;

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
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity *= 1.5f;
        }




        public override void AI()
        {
            // ===== 统一朝向（保持你的原始逻辑）=====
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

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

            Vector2 spawnPosition = Projectile.Center;

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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
          

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