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
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.SausageMakerC
{
    public class SausageMakerJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/SausageMakerC/SausageMakerJav";

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
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
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 6; // 设置为7次穿透
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 血液红色粒子特效
            if (Main.rand.NextBool(5))
            {
                Vector2 trailPos = Projectile.Center;
                float trailScale = Main.rand.NextFloat(0.8f, 1.2f);
                Color trailColor = Color.Red;
                Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(trail);
            }

            // 滴血效果（低概率大血滴）
            if (Main.rand.NextBool(1))
            {
                Vector2 bloodVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(1f, 3f));
                Dust bloodDrop = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, bloodVel, 100, Color.DarkRed, Main.rand.NextFloat(1.2f, 1.8f));
                bloodDrop.noGravity = false; // 模拟滴落
            }

            // 血气环绕（有序血色特效）
            if (Main.GameUpdateCount % 5 == 0)
            {
                int points = 6;
                float radius = 12f;
                float baseAngle = Main.GlobalTimeWrappedHourly * 2f;
                for (int i = 0; i < points; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / points;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = angle.ToRotationVector2() * 0.5f;
                    Dust bloodAura = Dust.NewDustPerfect(pos, DustID.Blood, vel, 80, Color.Red * 0.8f, 0.9f);
                    bloodAura.noGravity = true;
                }
            }

            // 微小血雾弥漫（淡血红）
            if (Main.rand.NextBool(10))
            {
                Dust mist = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, Vector2.Zero, 100, Color.DarkRed * 0.5f, 1.0f);
                mist.noGravity = true;
            }



            // 每帧增加 ai[x] 计数
            Projectile.ai[1]++;

            // 前x帧禁用方块碰撞（防止在平地上某些角度射不出来）
            if (Projectile.ai[1] < 5)
            {
                Projectile.tileCollide = false; // 禁用碰撞
            }
            else
            {
                Projectile.tileCollide = true; // 启用碰撞
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 给敌人添加燃血效果，持续300帧
            target.AddBuff(ModContent.BuffType<BurningBlood>(), 300);

            // 生成血红色的粒子特效
            for (int i = 0; i < 15; i++)
            {
                // 随机角度偏移，使粒子从弹幕后方随机向左或右发射
                float randomAngle = MathHelper.ToRadians(Main.rand.Next(-30, 30)); // 随机角度范围：-30度到30度
                Vector2 offsetDirection = Projectile.velocity.RotatedBy(randomAngle) * -0.5f; // 反向方向偏移一些，使粒子从后方飞出

                // 定义粒子
                Dust bloodDust = Dust.NewDustPerfect(Projectile.position, DustID.Blood, offsetDirection, 0, Color.Red, 1.5f);
                bloodDust.noGravity = true; // 粒子不受重力影响
                bloodDust.velocity *= Main.rand.NextFloat(1f, 2f); // 随机粒子的速度
            }
        }


        public override void OnKill(int timeLeft)
        {
            // 发射 Blood2 弹幕
            for (int i = 0; i < Main.rand.Next(3, 7); i++)
            {
                //Vector2 randomVelocity = Main.rand.NextVector2Circular(1f, 1f) * Projectile.velocity.Length() * 3f;1
                Vector2 randomVelocity = Main.rand.NextVector2Circular(1f, 1f) * 5f; // 将速度统一为5
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, randomVelocity,
                    ModContent.ProjectileType<Blood2>(), (int)(Projectile.damage * 0.70), Projectile.knockBack, Projectile.owner);


                int projIndex = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.position,
                    randomVelocity,
                    ModContent.ProjectileType<Blood2>(),
                    (int)(Projectile.damage * 0.75),
                    Projectile.knockBack,
                    Projectile.owner
                );

                // ✅ 设置属性
                if (projIndex.WithinBounds(Main.maxProjectiles))
                {
                    Projectile proj = Main.projectile[projIndex];
                    proj.friendly = true;
                    proj.hostile = false;
                    proj.tileCollide = false; // 穿墙
                }
            }



            {
                // 播放血肉撕裂音效
                SoundEngine.PlaySound(SoundID.NPCHit8 with { Volume = 0.7f, Pitch = -0.2f }, Projectile.Center);

                // 大量血液尘爆发
                for (int i = 0; i < 40; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Dust blood = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, velocity, 50, Color.Red, Main.rand.NextFloat(1.2f, 2.0f));
                    blood.noGravity = true;
                }

                // 血色冲击波（环状）
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 dir = angle.ToRotationVector2();
                    Dust ring = Dust.NewDustPerfect(Projectile.Center + dir * 8f, DustID.Blood, dir * 5f, 80, Color.DarkRed, 1.5f);
                    ring.noGravity = true;
                }

                // 血腥爆发时产生夸张的线性粒子喷射（SparkParticle）
                for (int i = 0; i < 8; i++)
                {
                    Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitY);
                    Vector2 velocity = direction * Main.rand.NextFloat(5f, 12f); // 极高速爆射

                    Color bloodRed = Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat(0.3f, 0.7f));

                    Particle bloodLine = new SparkParticle(
                        Projectile.Center,
                        velocity,
                        false, // 不受重力影响
                        45, // 存活帧数，保证爆射过程可见
                        Main.rand.NextFloat(1.1f, 1.7f), // 粗壮血线
                        bloodRed
                    );
                    GeneralParticleHandler.SpawnParticle(bloodLine);
                }


            }

        }



    }
}
