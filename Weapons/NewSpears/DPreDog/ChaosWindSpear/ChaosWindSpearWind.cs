using CalamityMod;
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

namespace CalamityThrowingSpear.Weapons.NewSpears.DPreDog.ChaosWindSpear
{
    internal class ChaosWindSpearWind : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public Vector2 SpinCenter { get; set; }
        public float SpinDirection { get; set; }
        public ref float SpinOffsetAngle => ref Projectile.ai[0];

        public static int Lifetime => 60;
        public static float SpinConvergencePower => 3.7f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 42;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            //Projectile.Opacity = 0f;
        }

        public override void AI()
        {
            // 让弹幕每帧向左旋转 1 度
            if (Projectile.velocity.LengthSquared() > 0.01f) // 避免 0 速度问题
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-1f));
            }


            // 帧动画切换
            Projectile.frameCounter++;
            Projectile.frame = Projectile.frameCounter / 5 % Main.projFrames[Type];

            // 生成天蓝色粒子
            if (Main.rand.NextBool(5))
            {
                int d = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 136, new Color(172, 238, 255), 1.4f);
                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = 1.5f;
                Main.dust[d].velocity = -Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * 0.2f;
            }

            Time++;
        }

        public ref float Time => ref Projectile.ai[1];

        public override bool? CanDamage() => Time >= 12f; // 初始的时候不会造成伤害，直到x为止
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 explosionPosition = target.Center;
            float radius = 2 * 16f; // 2×16 半径
            int numParticles = 3; // 形成三角形

            for (int i = 0; i < numParticles; i++)
            {
                float angle = MathHelper.TwoPi * i / numParticles; // 120° 间隔
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                ParticleOrchestrator.RequestParticleSpawn(
                    clientOnly: false,
                    ParticleOrchestraType.Keybrand,
                    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制拖尾效果
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return true;
        }



    }
}
