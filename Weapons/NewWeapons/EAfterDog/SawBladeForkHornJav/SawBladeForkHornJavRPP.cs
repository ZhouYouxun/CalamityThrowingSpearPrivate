using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    /// <summary>
    /// 发射自炮台的小型高速弹——改成：纯 Dust 视觉（黑/棕），极强追踪，狂野污染风。
    /// 说明：
    /// 1) PreDraw 不改，仍用残影（你要求“就这样不用动”）
    /// 2) 颜色：只用 Smoke(A) 的黑 + Ash 的棕，不再出现亮色
    /// 3) 逻辑：短暂预热 → 强追踪（角速度限制+强加速） → 纯 Dust 的环/锥/尾迹
    /// </summary>
    public class SawBladeForkHornJavRPP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj"; // 不绘制贴图，只看残影 & Dust

        // ====== 追踪参数（可按需微调）======
        private const int WarmupFrames = 10;                 // 预热帧数，给一点“起步喷烟”的时间
        private const float MaxSpeed = 22f;                // 追踪最高速度
        private const float TurnRateRad = 0.35f;              // 每帧最大转角（弧度）≈ 20°
        private const float SpeedLerp = 0.28f;              // 速度靠拢插值（越大加速越狠）
        private const float DriftAccel = 1.0125f;            // 无目标时的小幅前冲加速
        private const float TrailDustScaleMin = 1.2f;               // 尾迹 Dust 尺寸范围（黑）
        private const float TrailDustScaleMax = 1.9f;
        private const float AshDustScaleMin = 1.0f;               // 棕色尘 Ash 的尺寸
        private const float AshDustScaleMax = 1.5f;

        // 计时器：用 ai[1] 当本地计数器
        private ref float Timer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // ✅ 保持你原样：只有残影，不额外贴图
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;                    // 一次命中
            Projectile.timeLeft = 1200;
            Projectile.light = 0.0f;                     // 不打亮（避免偏色）
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            Timer++;

            // === 统一：轻微褐色照明（很暗）===
            Lighting.AddLight(Projectile.Center, new Vector3(0.06f, 0.04f, 0.02f));

            // === 预热阶段：仅直线 & 狂野黑烟尾迹 ===
            if (Timer <= WarmupFrames)
            {
                // 保持初始动量，稍微提速
                Projectile.velocity *= 1.01f;

                SpawnForwardConeDust(Projectile.velocity, 4, true); // 主前喷（黑）
                if (Timer % 2 == 0)
                    SpawnForwardConeAsh(Projectile.velocity, 2);    // 少量棕
                if (Timer % 4 == 0)
                    SpawnInwardRingDust(16, 18f);                   // 吸收环（黑）

                return;
            }

            // === 强追踪阶段（角速度限制 + 强插值提速）===
            NPC target = FindClosestNPC(1400f);
            if (target != null)
            {
                Vector2 desiredVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * MaxSpeed;

                // 角速度限制（平滑但非常强）
                float cr = Projectile.velocity.ToRotation();
                float dr = desiredVel.ToRotation();
                float nr = cr.AngleTowards(dr, TurnRateRad);
                float newSpeed = MathHelper.Lerp(Projectile.velocity.Length(), MaxSpeed, SpeedLerp);
                Projectile.velocity = nr.ToRotationVector2() * newSpeed;
            }
            else
            {
                // 无目标：继续沿当前方向“污染推进”
                Projectile.velocity *= DriftAccel;
                if (Projectile.velocity.Length() > MaxSpeed) // 限速
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxSpeed;
            }

            // === 视觉：纯 Dust（黑/棕），更狂野 ===
            SpawnForwardConeDust(Projectile.velocity, 5, true);     // 前喷强化（黑）
            if (Main.rand.NextBool(3)) SpawnForwardConeAsh(Projectile.velocity, 2); // 棕色点缀
            if (Main.rand.NextBool(5)) SpawnSideWakeDust(Projectile.velocity, 1);   // 侧涡（黑）
            if (Timer % 6 == 0) SpawnInwardRingDust(14, 20f);  // 间歇吸收环（黑）
        }

        // ===============================
        // Dust 生成（全部黑/棕，极简而凶）
        // ===============================

        /// <summary>
        /// 前方锥形喷射（黑烟），dir = 当前速度方向
        /// </summary>
        private void SpawnForwardConeDust(Vector2 dirVel, int count, bool strong = false)
        {
            Vector2 dir = dirVel.SafeNormalize(Vector2.UnitX);
            float spread = MathHelper.ToRadians(strong ? 30f : 22f);
            for (int i = 0; i < count; i++)
            {
                float ang = dir.ToRotation() + Main.rand.NextFloat(-spread, spread);
                Vector2 v = ang.ToRotationVector2() * Main.rand.NextFloat(strong ? 8f : 6f, strong ? 14f : 10f);
                int d = Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke, v.X, v.Y, 0, Color.Black, Main.rand.NextFloat(TrailDustScaleMin, TrailDustScaleMax));
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 1.2f;
            }
        }

        /// <summary>
        /// 前方锥形棕尘（Ash）
        /// </summary>
        private void SpawnForwardConeAsh(Vector2 dirVel, int count)
        {
            Vector2 dir = dirVel.SafeNormalize(Vector2.UnitX);
            float spread = MathHelper.ToRadians(18f);
            for (int i = 0; i < count; i++)
            {
                float ang = dir.ToRotation() + Main.rand.NextFloat(-spread, spread);
                Vector2 v = ang.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                int d = Dust.NewDust(Projectile.Center, 0, 0, DustID.Ash, v.X, v.Y, 0, new Color(70, 45, 25), Main.rand.NextFloat(AshDustScaleMin, AshDustScaleMax));
                Main.dust[d].noGravity = true;
            }
        }

        /// <summary>
        /// 侧向涡迹：沿法线方向两侧翻卷的黑烟
        /// </summary>
        private void SpawnSideWakeDust(Vector2 dirVel, int lines)
        {
            Vector2 perp = dirVel.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
            for (int i = 0; i < lines; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector2 p = Projectile.Center + perp * s * Main.rand.NextFloat(6f, 14f);
                    Vector2 v = perp * s * Main.rand.NextFloat(3f, 6f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                    int d = Dust.NewDust(p, 0, 0, DustID.Smoke, v.X, v.Y, 0, Color.Black, Main.rand.NextFloat(1.2f, 1.7f));
                    Main.dust[d].noGravity = true;
                }
            }
        }

        /// <summary>
        /// 吸收环：在周围形成黑烟圆环并向内回卷
        /// </summary>
        private void SpawnInwardRingDust(int points, float radius)
        {
            for (int i = 0; i < points; i++)
            {
                float ang = MathHelper.TwoPi * i / points;
                Vector2 off = ang.ToRotationVector2() * radius;
                Vector2 pos = Projectile.Center + off;
                Vector2 v = -off.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f);
                int d = Dust.NewDust(pos, 0, 0, DustID.Smoke, v.X, v.Y, 0, Color.Black, Main.rand.NextFloat(1.2f, 1.8f));
                Main.dust[d].noGravity = true;
            }
        }

        // ===============================
        // 追踪目标搜索
        // ===============================
        private NPC FindClosestNPC(float maxRange)
        {
            NPC result = null;
            float min = maxRange;
            foreach (var n in Main.npc)
            {
                if (!n.CanBeChasedBy()) continue;
                float d = Vector2.Distance(Projectile.Center, n.Center);
                if (d < min)
                {
                    min = d;
                    result = n;
                }
            }
            return result;
        }

        // ===============================
        // 命中：Dust-only 黑/棕爆散（有序+无序）
        // ===============================
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);


            target.AddBuff(ModContent.BuffType<CalamityMod.Buffs.StatDebuffs.MarkedforDeath>(), 300);
            target.AddBuff(ModContent.BuffType<CalamityMod.Buffs.StatDebuffs.Crumbling>(), 300);

            // 1) 有序：圆环（黑）
            int ring = 36;
            float r = 14f;
            for (int i = 0; i < ring; i++)
            {
                float a = MathHelper.TwoPi * i / ring;
                Vector2 v = a.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                int d = Dust.NewDust(target.Center, 0, 0, DustID.Smoke, v.X, v.Y, 0, Color.Black, Main.rand.NextFloat(1.4f, 2.1f));
                Main.dust[d].noGravity = true;
            }
            // 2) 有序：前向锥（黑）
            int cone = 40;
            float baseRot = Projectile.velocity.ToRotation();
            float spread = MathHelper.ToRadians(38f);
            for (int i = 0; i < cone; i++)
            {
                float ang = baseRot + Main.rand.NextFloat(-spread, spread);
                Vector2 v = ang.ToRotationVector2() * Main.rand.NextFloat(10f, 18f);
                int d = Dust.NewDust(target.Center, 0, 0, DustID.Smoke, v.X, v.Y, 0, Color.Black, Main.rand.NextFloat(1.6f, 2.3f));
                Main.dust[d].noGravity = true;
            }
            // 3) 无序：大爆散（黑）
            int scatter = 120;
            for (int i = 0; i < scatter; i++)
            {
                Vector2 v = Main.rand.NextVector2Unit() * Main.rand.NextFloat(8f, 26f);
                int d = Dust.NewDust(target.Center, 0, 0, DustID.Smoke, v.X, v.Y, 0, Color.Black, Main.rand.NextFloat(1.8f, 2.6f));
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 1.6f;
            }
            // 4) 棕色碎屑（Ash）
            int ash = 30;
            for (int i = 0; i < ash; i++)
            {
                Vector2 v = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 18f);
                int d = Dust.NewDust(target.Center, 0, 0, DustID.Ash, v.X, v.Y, 0, new Color(70, 45, 25), Main.rand.NextFloat(1.1f, 1.7f));
                Main.dust[d].noGravity = true;
            }
        }
    }
}
