using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent.Drawing;

namespace CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.ChaosEssenceSpear
{
    internal class ChaosEssenceSpearFire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewSpears.BPrePlantera";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public float Time
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override bool? CanDamage() => Time >= 12f; // 前 12 帧不造成伤害

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
        }

        public float nextBounceTime = 0; // 记录下一次随机反弹的时间

        public override void AI()
        {
            // 增加时间计数器
            Time++;

            // 轨迹粒子特效（X 形释放）
            if (Main.rand.NextBool(3)) // 降低粒子生成频率
            {
                Vector2 spawnPosition = Projectile.Center;

                // **四个角之一随机选择**
                int randomCorner = Main.rand.Next(4);
                Vector2 offset = randomCorner switch
                {
                    0 => new Vector2(4f, 4f),   // 第一象限
                    1 => new Vector2(-4f, 4f),  // 第二象限
                    2 => new Vector2(4f, -4f),  // 第三象限
                    _ => new Vector2(-4f, -4f), // 第四象限
                };

                int dustType = DustID.CrimsonTorch;
                Color dustColor = Color.OrangeRed;
                float scale = 1.5f;

                CreateDust(spawnPosition + offset, dustType, dustColor, scale);
            }

            // **确保 `nextBounceTime` 不是 `0`，而是初始设定一个延迟触发**
            if (Time == 0)
            {
                nextBounceTime = Main.rand.Next(15, 36); // 设定初始延迟
            }

            if (Time >= nextBounceTime)
            {
                // **在当前位置释放特殊特效**
                //ReleaseSpecialEffect();

                RandomizeBounce();
                nextBounceTime = Time + Main.rand.Next(15, 36); // 设定下一次随机拐弯时间
            }


        }

        // 生成 Dust 粒子
        private void CreateDust(Vector2 position, int dustType, Color color, float scale)
        {
            Dust dust = Dust.NewDustPerfect(position, dustType);
            dust.color = color;
            dust.scale = scale;
            dust.fadeIn = 1f;
            dust.noGravity = true;
        }

        // 让弹幕在空中随机改变方向
        private void RandomizeBounce()
        {
            float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f)); // 只旋转 ±30°
            float speedMultiplier = Main.rand.NextFloat(0.9f, 1.1f); // 速度 90%~110%

            Projectile.velocity = Projectile.velocity.RotatedBy(randomAngle) * speedMultiplier;
        }


        // 释放特殊特效
        private void ReleaseSpecialEffect()
        {
            ParticleOrchestrator.RequestParticleSpawn(
                clientOnly: false,
                ParticleOrchestraType.WallOfFleshGoatMountFlames,
                new ParticleOrchestraSettings { PositionInWorld = Projectile.Center },
                Projectile.owner
            );
        }

        public override void OnKill(int timeLeft)
        {
            // 消失时生成一圈粒子特效
            int particleCount = 20;
            float angleIncrement = MathHelper.TwoPi / particleCount;

            for (int i = 0; i < particleCount; i++)
            {
                Vector2 velocity = new Vector2(3f, 0f).RotatedBy(angleIncrement * i);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.CrimsonTorch, velocity, 0, Color.OrangeRed);
                dust.scale = 1.2f;
                dust.noGravity = true;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300); // 原版的狱炎效果
            target.AddBuff(BuffID.OnFire, 300); // 原版的着火效果
        }
    }
}