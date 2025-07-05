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
using CalamityMod.Items.Tools;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC
{
    public class YateveoBloomJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/YateveoBloomC/YateveoBloomJav";
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
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;


            // Lighting - 添加深绿色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkGreen.ToVector3() * 0.55f);

            //// 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.01f;

            {            
                // 添加粒子效果 - 深红色和深绿色粒子
                if (Main.rand.NextBool(3)) // 以1/3的概率生成深红色或深绿色粒子
                {
                    int dustType = Main.rand.NextBool() ? DustID.RedTorch : DustID.GreenTorch; // 红色或绿色粒子
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
                }

                // 🌹 优美螺旋花瓣尾迹
                float spiralRadius = 6f;
                float spiralSpeed = 0.2f;
                float time = Main.GameUpdateCount * spiralSpeed;

                for (int s = 0; s < 2; s++)
                {
                    float spiralOffset = s * MathHelper.Pi;
                    float angle = time + spiralOffset;
                    Vector2 offset = angle.ToRotationVector2() * spiralRadius;

                    if (Main.rand.NextBool(2))
                    {
                        int dustType = Main.rand.Next(new int[] { DustID.RedTorch, DustID.GreenTorch, DustID.Dirt });
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + offset,
                            dustType,
                            -Projectile.velocity * 0.1f,
                            100,
                            Color.White,
                            1.2f
                        );
                        d.noGravity = true;
                    }
                }
            }

            if (Projectile.localAI[0] > 20f)
            {
                if (Projectile.velocity.Y < 24f)
                {
                    Projectile.velocity.Y += 0.4f;
                }
            }

        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SpawnRoseBloomDust(Projectile.Center);

            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 释放独特的草音效	
            SoundEngine.PlaySound(SoundID.Grass, Projectile.position);

            // 使敌人中毒，持续 180 帧
            target.AddBuff(BuffID.Poisoned, 180);
        }


        public override void OnKill(int timeLeft)
        {
            SpawnRoseBloomDust(Projectile.Center);


            int dustType = Main.rand.NextBool() ? DustID.RedTorch : DustID.GreenTorch; // 红色或绿色粒子
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.5f);
            }

            // 随机释放三个 BladeOfGrass 弹幕，倍率为 95%
            for (int i = 0; i < 3; i++)
            {
                Vector2 direction = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi); // 随机方向
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, direction * 8f, 976, (int)(Projectile.damage * 0.3f), Projectile.knockBack, Projectile.owner);
            }
        }


        private void SpawnRoseBloomDust(Vector2 center)
        {
            // 播放独特草音效
            SoundEngine.PlaySound(SoundID.Grass, Projectile.position);

            int petals = 100;

            // 🌹 层 1：花蕊（中心微颗粒，快散）
            for (int i = 0; i < petals; i++)
            {
                float t = MathHelper.TwoPi * i / petals;
                float r = 2f + 0.5f * (float)Math.Sin(6 * t);

                Vector2 velocity = t.ToRotationVector2() * r * 1.5f;

                Dust d = Dust.NewDustPerfect(
                    center,
                    DustID.Grass,
                    velocity,
                    100,
                    Color.GreenYellow,
                    1.0f
                );
                d.noGravity = true;
            }

            // 🌹 层 2：花瓣（五瓣玫瑰曲线，中速）
            for (int i = 0; i < petals; i++)
            {
                float t = MathHelper.TwoPi * i / petals;
                float r = 6f * (1 + 0.4f * (float)Math.Sin(5 * t));

                Vector2 velocity = t.ToRotationVector2() * r;

                Dust d = Dust.NewDustPerfect(
                    center,
                    DustID.GrassBlades,
                    velocity,
                    100,
                    Color.Green,
                    1.3f
                );
                d.noGravity = true;
            }

            // 🌿 层 3：绿叶（大弧度，慢速，外围叶片结构）
            for (int i = 0; i < petals / 2; i++)
            {
                float t = MathHelper.TwoPi * i / (petals / 2);
                float leafShape = 8f * (1 + 0.3f * (float)Math.Sin(3 * t)); // 三叶模式
                Vector2 velocity = t.ToRotationVector2() * leafShape * 0.7f;

                Dust d = Dust.NewDustPerfect(
                    center,
                    DustID.GrassBlades,
                    velocity,
                    100,
                    Color.ForestGreen,
                    1.7f
                );
                d.noGravity = false;
            }
        }



    }
}
