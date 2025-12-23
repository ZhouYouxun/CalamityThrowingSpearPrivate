using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget.SunsetBForgetLeft;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetTantacle : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.height = 160;
            Projectile.width = 160;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.MaxUpdates = 3;

            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 4;
        }






        public override void AI()
        {
            // ======= 这部分保持你原样，一行不动 =======
            // HOW CAN THIS CODE EVER RUN
            if (Projectile.velocity.X != Projectile.velocity.X)
            {
                if (Math.Abs(Projectile.velocity.X) < 1f)
                    Projectile.velocity.X = -Projectile.velocity.X;
                else
                    Projectile.Kill();
            }
            if (Projectile.velocity.Y != Projectile.velocity.Y)
            {
                if (Math.Abs(Projectile.velocity.Y) < 1f)
                    Projectile.velocity.Y = -Projectile.velocity.Y;
                else
                    Projectile.Kill();
            }

            Vector2 center10 = Projectile.Center;
            Projectile.scale = 1f - Projectile.localAI[0];
            Projectile.width = (int)(20f * Projectile.scale);
            Projectile.height = Projectile.width;
            Projectile.position.X = center10.X - (float)(Projectile.width / 2);
            Projectile.position.Y = center10.Y - (float)(Projectile.height / 2);
            if ((double)Projectile.localAI[0] < 0.1)
            {
                Projectile.localAI[0] += 0.01f;
            }
            else
            {
                Projectile.localAI[0] += 0.025f;
            }
            if (Projectile.localAI[0] >= 0.95f)
            {
                Projectile.Kill();
            }
            Projectile.velocity.X = Projectile.velocity.X + Projectile.ai[0] * 1.5f;
            Projectile.velocity.Y = Projectile.velocity.Y + Projectile.ai[1] * 1.5f;
            if (Projectile.velocity.Length() > 16f)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= 16f;
            }
            Projectile.ai[0] *= 1.05f;
            Projectile.ai[1] *= 1.05f;

            if (Projectile.scale < 1f)
            {
                int i = 0; // ✨ 保留你的 i

                // ======= 从这里开始是我们修复/优化后的内容 =======

                // 用 localAI[0] 推进来计算“生命周期进度”(0~1)，无需 timeLeftMax
                // localAI[0] 从 0 增长到 ~0.95 → 这里做一次归一化
                float progress = MathHelper.Clamp(Projectile.localAI[0] / 0.95f, 0f, 1f);

                // 生成数量：3 → 2 → 1（越到后期越“细”）
                int count = (progress < 0.33f) ? 3 : (progress < 0.66f ? 2 : 1);

                // 偏移范围：16 → 2（越到后期越收敛到中心）
                float maxOffset = MathHelper.Lerp(16f, 2f, progress);

                // ✅ 关键修复：复用外层 i，避免 CS0136；并且确保 i 会自增，杜绝死循环/内存暴涨
                for (; i < (int)(Projectile.scale * 4f); i++)
                {
                    for (int k = 0; k < count; k++)
                    {
                        // 随时间收缩的偏移
                        Vector2 offset = Main.rand.NextVector2Circular(maxOffset, maxOffset);

                        // 统一计算一次速度，避免每个分支各算各的（保证视觉一致）
                        Vector2 vel = Projectile.velocity * Main.rand.NextFloat(0.05f, 0.2f);

                        // === GlowOrbParticle 主体（保留你原来的视觉）===
                        GlowOrbParticle orb = new GlowOrbParticle(
                            Projectile.Center + offset,
                            vel,
                            false,
                            12,
                            1.0f + Projectile.scale * 0.5f,
                             GetTentacleColor(),
                            true,
                            false,
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(orb);

                        // === 20% 概率混杂“水味”特效（参数对齐，风格统一）===
                        if (Main.rand.NextFloat() < 0.2f)
                        {
                            WaterFlavoredParticle mist = new WaterFlavoredParticle(
                                Projectile.Center + offset,
                                vel,
                                false,
                                Main.rand.Next(18, 26),
                                0.9f + Main.rand.NextFloat(0.3f),
                                GetTentacleColor() * 0.9f
                            );
                            GeneralParticleHandler.SpawnParticle(mist);
                        }
                    }
                }
            }
        }



        // ================= 配色参数 =================
        private static readonly Color PrimaryColor = Color.CornflowerBlue; // 主色：蓝色
        private static readonly Color SecondaryColor = new Color(180, 100, 255); // 辅色：紫色
        private const float SecondaryRatio = 0.2f; // 紫色比例（20%）

        // 获取混合颜色（按照比例随机选）
        private Color GetTentacleColor()
        {
            return (Main.rand.NextFloat() < SecondaryRatio)
                ? SecondaryColor
                : PrimaryColor;
        }



        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SunsetPlayerSpeed.ApplyNoArmorHypothesisHitEffect(
                Projectile,
                target,
                ref modifiers
            );
        }

        private bool useYellowDust; // 记录该触手属于哪个阵营

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            useYellowDust = Main.rand.NextBool(); // 触手在生成时随机选择黄色或蓝色阵营
        }

    }
}
