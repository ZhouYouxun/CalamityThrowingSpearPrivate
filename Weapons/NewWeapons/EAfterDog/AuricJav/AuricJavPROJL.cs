using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav
{
    public class AuricJavPROJL : ModProjectile, ILocalizedModType
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        // 自定义变量（禁止localAI）
        private int time = 0;
        private int angleTimer = 20;
        private int curveDir = 1;
        private int branchCount = 0;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 80; // 和DevilsStrike一致
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 0;
        }

        public override void AI()
        {


            float deathLerp = Utils.Remap(Projectile.timeLeft, 200, 0, 30, 6);

            // ================= 初始化 =================
            if (time == 0)
            {
                angleTimer = 30; // 固定节奏（去掉完全随机）
                curveDir = Main.rand.NextBool() ? 1 : -1;
            }

            time++;

            // ================= 路径（模仿原版但更有秩序） =================
            if (angleTimer > 0)
            {
                angleTimer--;
                Projectile.velocity = Projectile.velocity.RotatedByRandom(0.008f); // 轻微抖动
            }
            else
            {
                angleTimer = 30; // 固定周期 → 有节奏

                // 有控制的转向（不像原版那么乱）
                float turn = 0.6f * Utils.GetLerpValue(0, 200, Projectile.timeLeft, true) * curveDir;
                Projectile.velocity = Projectile.velocity.RotatedBy(turn);

                curveDir *= -1; // 左右交替（关键秩序感）
            }

            // ================= 核心视觉（真正的“闪电”） =================
            if (time % 4 == 0)
            {
                Color auricColor = Color.Lerp(new Color(255, 215, 0), Color.White, Main.rand.NextFloat(0f, 0.4f));

                Particle spark = new CustomSpark(
                    Projectile.Center,
                    Projectile.velocity * 0.15f,
                    "CalamityMod/Particles/BloomCircle",
                    false,
                    20,
                    Main.rand.NextFloat(0.006f, 0.0075f) * deathLerp,
                    auricColor * 0.9f,
                    new Vector2(1.1f, 1f),
                    shrinkSpeed: 0.18f
                );

                GeneralParticleHandler.SpawnParticle(spark);
            }

            // ================= 分裂（有控制，不乱） =================
            if (time % 18 == 0 && branchCount < 3)
            {
                branchCount++;

                if (Main.myPlayer == Projectile.owner)
                {
                    float angle = (branchCount % 2 == 0 ? 0.5f : -0.5f); // 左右交替

                    Projectile p = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        (Projectile.velocity * 0.8f).RotatedBy(angle),
                        Type,
                        Projectile.damage,
                        0f,
                        Projectile.owner
                    );

                    if (p.ModProjectile is AuricJavPROJL child)
                        child.branchCount = this.branchCount;

                    p.timeLeft = 160; // 子分支更短
                }
            }

        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
    }
}