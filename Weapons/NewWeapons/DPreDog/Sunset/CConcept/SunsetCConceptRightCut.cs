using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
{
    internal class SunsetCConceptRightCut : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }
        public override void OnSpawn(IEntitySource source)
        {
            // 生成粒子爆炸效果
            Particle blastRing = new CustomPulse(
                Projectile.Center, // 以弹幕为中心
                Vector2.Zero,
                new Color(255, 223, 0), // 金色，增强神圣感
                "CalamityThrowingSpear/texture/IonizingRadiation",
                Vector2.One * 0.33f,
                Main.rand.NextFloat(-10f, 10f),
                0.07f,
                0.15f,
                15
            );
            GeneralParticleHandler.SpawnParticle(blastRing);
        }

        public override void AI()
        {
            // 直线飞行
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;







        }



        public override void OnKill(int timeLeft)
        {


        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetCConceptPBuff>(), 300); // 5 秒

            // 生成尖刺型白色粒子
            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.4f, 1.6f);
                PointParticle spark = new PointParticle(
                    Projectile.Center,
                    velocity,
                    false,
                    15,
                    1.1f,
                    Color.White // 颜色改为白色
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 生成魔法阵
            CreateMagicCircle(target.Center);

            // 播放独特击中音效
            SoundEngine.PlaySound(SoundID.Item30, Projectile.position);
        }
        private void CreateMagicCircle(Vector2 center)
        {
            int circleRadius = 50;

            for (int i = 0; i < 36; i++) // 36 个点，形成环形
            {
                float angle = MathHelper.ToRadians(10 * i);
                Vector2 offset = angle.ToRotationVector2() * circleRadius;
                Vector2 dustPos = center + offset;

                // 使用两种 Dust（彩虹火把和钻石宝石）绘制魔法阵
                Dust dust1 = Dust.NewDustPerfect(dustPos, DustID.RainbowTorch, Vector2.Zero, 150, Color.White, 1.2f);
                dust1.noGravity = true;

                Dust dust2 = Dust.NewDustPerfect(dustPos, DustID.GemDiamond, Vector2.Zero, 200, Color.White, 1.5f);
                dust2.noGravity = true;
            }
        }

    }
}