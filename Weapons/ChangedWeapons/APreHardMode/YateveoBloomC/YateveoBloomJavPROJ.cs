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
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";

        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/YateveoBloomC/YateveoBloomJav";
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
            Projectile.penetrate = 6; // 穿透次数改为 10
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 10; // 无敌帧冷却时间为14帧
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

            // 每次反弹减少 2 次穿透
            Projectile.penetrate -= 2;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
                return false;
            }

            // 精准物理反弹（入射角 = 出射角）
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }

            // 反弹后在出射角 ±20°扇形范围随机抽取 3 个点，生成 BladeOfGrass 弹幕，并锁定最近敌人
            for (int i = 0; i < 3; i++)
            {
                Vector2 baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f));
                Vector2 spawnDirection = baseDirection.RotatedBy(randomAngle);

                // 在出射方向前方半径 60 像素范围内随机偏移位置
                Vector2 spawnPosition = Projectile.Center + spawnDirection * Main.rand.NextFloat(30f, 60f);

                // 锁定最近敌人作为目标方向
                NPC target = Main.npc
                    .Where(n => n.CanBeChasedBy() && !n.friendly && n.active && Vector2.Distance(spawnPosition, n.Center) < 800f)
                    .OrderBy(n => Vector2.Distance(spawnPosition, n.Center))
                    .FirstOrDefault();

                Vector2 offset = Main.rand.NextVector2Circular(32f, 32f); // 在32像素范围内随机偏移
                Vector2 velocityToTarget = target != null
                    ? (target.Center + offset - spawnPosition).SafeNormalize(Vector2.UnitY) * 8f
                    : spawnDirection * 8f;

                int leafProj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    velocityToTarget,
                    ProjectileID.BladeOfGrass,
                    (int)(Projectile.damage * 0.3f),
                    Projectile.knockBack,
                    Projectile.owner
                );

                // 设置生成的弹幕可穿墙且无限穿透
                if (leafProj.WithinBounds(Main.maxProjectiles))
                {
                    Projectile proj = Main.projectile[leafProj];
                    proj.friendly = true;
                    proj.hostile = false;
                    proj.penetrate = 3;
                    proj.extraUpdates = 3;
                    proj.tileCollide = false;
                    proj.localNPCHitCooldown = 60;
                    proj.usesLocalNPCImmunity = true;
                }
            }

            //// 发射一发 YateveoBloomJavBall 向反弹后法线方向（下一步单独实现）
            //Vector2 normalDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            //Projectile.NewProjectile(
            //    Projectile.GetSource_FromThis(),
            //    Projectile.Center,
            //    normalDir * 8f,
            //    ModContent.ProjectileType<YateveoBloomJavBall>(), // 需确认类存在
            //    Projectile.damage,
            //    Projectile.knockBack,
            //    Projectile.owner
            //);

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
        }


        private void SpawnRoseBloomDust(Vector2 center)
        {
            // 播放独特草音效
            SoundEngine.PlaySound(SoundID.Grass, Projectile.position);

            int petals = 100;
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY); // 弹幕面向方向

            // 🌹 层 1：花蕊（中心微颗粒，快散，沿前方微偏移）
            for (int i = 0; i < petals; i++)
            {
                float t = MathHelper.TwoPi * i / petals;
                float r = 2f + 0.5f * (float)Math.Sin(6 * t);

                // 让花蕊围绕飞行方向偏移展开
                Vector2 baseDirection = t.ToRotationVector2();
                Vector2 velocity = baseDirection * r * 1.5f;

                // 在弹幕前方微偏移生成
                Vector2 spawnPos = center + forward * 8f; // 偏移 8px 可自行调整

                Dust d = Dust.NewDustPerfect(
                    spawnPos,
                    DustID.Grass,
                    velocity,
                    100,
                    Color.GreenYellow,
                    1.0f
                );
                d.noGravity = true;
            }

            // 🌹 层 2：花瓣（五瓣玫瑰曲线，中速，沿前方偏移更明显）
            for (int i = 0; i < petals; i++)
            {
                float t = MathHelper.TwoPi * i / petals;
                float r = 6f * (1 + 0.4f * (float)Math.Sin(5 * t));

                Vector2 baseDirection = t.ToRotationVector2();
                Vector2 velocity = baseDirection * r;

                // 在弹幕前方偏移生成（更远）
                Vector2 spawnPos = center + forward * 16f; // 偏移 16px 可自行调整

                Dust d = Dust.NewDustPerfect(
                    spawnPos,
                    DustID.GrassBlades,
                    velocity,
                    100,
                    Color.Green,
                    1.3f
                );
                d.noGravity = true;
            }
        }



    }
}
