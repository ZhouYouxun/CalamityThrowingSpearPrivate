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
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.StarGunSagittarius
{
    public class StarGunSagittariusPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/StarGunSagittarius/StarGunSagittarius";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        // 声明静态计数器，所有普通长枪共享计数
        private static int shotCounter = 0;
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
            Projectile.penetrate = 1; // 允许1次伤害
            Projectile.timeLeft = 420;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 能够穿透方块
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 1; // 无敌帧冷却时间为1帧
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.velocity *= 1.005f;

            // 每一帧生成混合粒子特效
            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextFloat() < 0.7f) // 70% 概率生成新特效
                {
                    Color particleColor = Color.LightYellow;
                    float particleScale = 0.35f;
                    Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f); // 扩散到周围随机位置
                    Vector2 particleVelocity = Main.rand.NextVector2Circular(15f, 15f); // 扩散速度（这个需要快一点）

                    GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, particleVelocity, particleColor, particleScale, Main.rand.Next(20) + 10));
                }
                else // 30% 概率生成原有特效
                {
                    Vector2 sparklePosition = Projectile.Center;
                    Vector2 sparkleVelocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(0.5f, 1.2f);
                    Color startColor = Color.Gold * 0.5f;
                    Color endColor = Color.LightGoldenrodYellow * 0.9f;

                    SparkleParticle spark = new SparkleParticle(sparklePosition, sparkleVelocity, startColor, endColor, Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(7, 16), Main.rand.NextFloat(-8, 8), 0.2f, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            Lighting.AddLight(Projectile.Center, Color.LightGoldenrodYellow.ToVector3() * 0.55f);
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item88, Projectile.Center);

            shotCounter++;
            if (shotCounter >= 5)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * (1f / 3f),
                    ModContent.ProjectileType<StarGunSagittariusPROJECHO>(),
                    (int)(Projectile.damage * 1.35f), Projectile.knockBack, Projectile.owner);

                shotCounter = 0;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                if (Main.rand.NextFloat() < 0.7f) // 70% 概率生成新特效
                {
                    Color particleColor = Color.LightYellow;
                    float particleScale = 0.35f;
                    Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f); // 扩散到周围随机位置
                    Vector2 particleVelocity = Main.rand.NextVector2Circular(29f, 29f); // 扩散速度（这个需要快一点）

                    GeneralParticleHandler.SpawnParticle(new GenericBloom(particlePosition, particleVelocity, particleColor, particleScale, Main.rand.Next(20) + 10));
                }
                else // 30% 概率生成原有特效
                {
                    Vector2 offset = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(1.5f, 3f); // 调整速度范围
                    Color startColor = Color.Gold * 0.6f;
                    Color endColor = Color.LightGoldenrodYellow * 1.0f;

                    SparkleParticle spark = new SparkleParticle(Projectile.Center, offset, startColor, endColor, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(10, 20), Main.rand.NextFloat(-8, 8), 0.3f, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

        }



    }
}