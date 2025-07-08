using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.BrimlanceC
{
    public class BrimlanceJavFireWall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 300;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.usesLocalNPCImmunity = true; 
            Projectile.localNPCHitCooldown = 15;
        }

        public override void OnSpawn(IEntitySource source)
        {
            int lifetime = Projectile.timeLeft;
            Color crimsonRed = new Color(255, 60, 60); // 鲜红色

            Particle pulse = new StaticPulseRing(
                Projectile.Center,
                Vector2.Zero,
                crimsonRed,
                new Vector2(1f, 1f),
                0f,
                0.0152f,
                0.152f,
                lifetime
            );
            GeneralParticleHandler.SpawnParticle(pulse);
        }

        public override void AI()
        {
            {
                // ✦ 每帧在空心甜甜圈内随机位置释放 FlameWaders 特效
                float radius = Main.rand.NextFloat(175f, 250f);
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 randomOffset = angle.ToRotationVector2() * radius;
                Vector2 effectPosition = Projectile.Center + randomOffset;

                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.FlameWaders,
                    new ParticleOrchestraSettings { PositionInWorld = effectPosition },
                    Projectile.owner
                );
            }

            // ✦ 每隔 15 帧发射 BrimlanceStandingFire 弹幕
            if (Projectile.timeLeft % 15 == 0)
            {
                // 随机半径在 175 到 250 之间
                float radius = Main.rand.NextFloat(175f, 250f);
                // 随机角度
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                // 得到圆环内随机点偏移
                Vector2 spawnOffset = angle.ToRotationVector2() * radius;
                Vector2 spawnPosition = Projectile.Center + spawnOffset;

                // 以正上方向为基准 ±30°
                float angleOffset = MathHelper.ToRadians(Main.rand.Next(-30, 31));
                Vector2 baseDirection = -Vector2.UnitY;
                Vector2 velocity = baseDirection.RotatedBy(angleOffset) * 15f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<BrimlanceStandingFire>(),
                    (int)(Projectile.damage * 0.6),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }





            {
                // 🎇 狂野红色粒子（每帧执行）
                for (int i = 0; i < 3; i++)
                {
                    // 形状 1：边缘大光点
                    Dust dust1 = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2CircularEdge(160f, 160f),
                        235 // 你也可以尝试改成 DustID.Torch or RedTorch for红色调
                    );
                    dust1.scale = Main.rand.NextFloat(2.0f, 3.5f);
                    dust1.color = Color.Red;
                    dust1.noGravity = true;
                }

                // 形状 2：中心稀疏小颗粒
                for (int i = 0; i < 2; i++)
                {
                    Dust dust2 = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(120f, 120f),
                        235
                    );
                    dust2.scale = Main.rand.NextFloat(0.9f, 1.4f);
                    dust2.color = Color.OrangeRed;
                    dust2.noGravity = true;
                }

            }

        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 240);

            // ✦ 在敌人脸上释放视觉弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<FuckYou>(),
                (int)(Projectile.damage * 0.1f), // 伤害仅为10%
                0f,
                Projectile.owner
            );
        }

    }
}
