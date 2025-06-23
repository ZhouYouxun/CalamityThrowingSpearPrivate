using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
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
using Terraria.ModLoader.IO;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimordialState
{
    internal class PrimordialStatePROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/PrimordialState/PrimordialState";

        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        private int phase = 1;
        private int phaseTimer = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 72;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1; // 允许1次伤害
            Projectile.timeLeft = 6000;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

            // 根据阶段执行对应逻辑
            switch (phase)
            {
                case 1:
                    PerformPhaseOne();
                    break;
                case 2:
                    PerformPhaseTwo();
                    break;
                case 3:
                    PerformPhaseThree();
                    break;
                case 4:
                    PerformPhaseFour(); // 新增的停留旋转阶段
                    break;
                case 5:
                    PerformPhaseFive(); // 原来的冲刺阶段
                    break;
            }
        }

        private void PerformPhaseOne() // 第一阶段：弱追踪
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 前30帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 24)
            {
                // 寻找最近的敌人并调整方向
                NPC target = Projectile.Center.ClosestNPCAt(5000);
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float desiredRotation = direction.ToRotation();
                    float currentRotation = Projectile.velocity.ToRotation();
                    float rotationDifference = MathHelper.WrapAngle(desiredRotation - currentRotation);
                    float rotationAmount = MathHelper.ToRadians(Main.rand.Next(1, 17)) * Math.Sign(rotationDifference);

                    if (Math.Abs(rotationDifference) < Math.Abs(rotationAmount))
                    {
                        rotationAmount = rotationDifference;
                    }

                    Projectile.velocity = Projectile.velocity.RotatedBy(rotationAmount);
                }
            }
            else
            {
                Projectile.ai[1]++;
            }

            // 四螺旋粒子特效
            int[] dustTypes = { DustID.AncientLight, DustID.RainbowMk2, DustID.SilverFlame, DustID.SteampunkSteam };
            for (int i = 0; i < 4; i++)
            {
                float rotation = (Main.GameUpdateCount * 0.1f + MathHelper.PiOver2 * i) % MathHelper.TwoPi;
                Vector2 offset = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation)) * 24;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.Next(dustTypes), null, 150, default, Main.rand.NextFloat(1.55f, 1.95f));

                // 让粒子有一定的向后加速度
                dust.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);

                // 让粒子旋转并随时间消失
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
        }

        private void PerformPhaseTwo() // 第2阶段：超强追踪
        {
            // 进入第二阶段的第一次调用时，调用 CreateSlashProjectile()
            if (phaseTimer == 0)
            {
                CreateSlashProjectile();
            }

            // 取消随机冲撞，只进行精准追踪
            NPC target = Projectile.Center.ClosestNPCAt(2400);
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 28f, 0.08f);
            }
            Projectile.rotation += 0.45f;

            // 每 6 帧生成一个 `PrimordialStateNight` 弹幕
            if (phaseTimer % 6 == 0)
            {
                Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(50 * 16, 50 * 16);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<PrimordialStateNight>(),
                    (int)(Projectile.damage * 0.75f), // 伤害倍率 1.0
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            if (++phaseTimer >= 150)
            {
                phase = 3;
                phaseTimer = 0;
            }
        }

        private void PerformPhaseThree() // 第3阶段：持续追踪
        {
            NPC target = Projectile.Center.ClosestNPCAt(2400);
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 28f, 0.08f);
            }
            Projectile.rotation += 0.45f;
            // 每隔 20 帧，增加随机冲刺的效果
            //if (phaseTimer % 20 == 0)
            //{
            //    float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机生成一个角度
            //    Vector2 randomDash = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * 10f; // 冲刺速度大小
            //    Projectile.velocity += randomDash; // 在当前速度基础上增加随机冲刺速度
            //}
            if (++phaseTimer >= 150)
            {
                phase = 4;
                phaseTimer = 0;
                RandomDash();
            }
        }

        private void PerformPhaseFour() // 第4阶段：最终冲刺
        {
            // 释放 6~10 个 `PrimordialStateLight` 弹幕
            int projCount = Main.rand.Next(6, 11);
            List<NPC> targets = new List<NPC>();
            float searchRadius = 1500f;
            float minDistance = 50f;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance > minDistance && distance <= searchRadius)
                    {
                        targets.Add(npc);
                    }
                }
            }

            targets = targets.OrderBy(npc => Vector2.Distance(Projectile.Center, npc.Center)).Take(projCount).ToList();

            for (int i = 0; i < projCount; i++)
            {
                Vector2 shootDirection;
                if (i < targets.Count)
                {
                    shootDirection = Vector2.Normalize(targets[i].Center - Projectile.Center) * 19f;
                }
                else
                {
                    shootDirection = Main.rand.NextVector2CircularEdge(1f, 1f) * 19f;
                }

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    shootDirection,
                    ModContent.ProjectileType<PrimordialStateLight>(),
                    (int)(Projectile.damage * 1.0f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            // 随机角度释放X个太极纹理
            for (int i = 0; i < 1; i++)
            {
                Particle blastRing = new CustomPulse(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.Black,
                    "CalamityThrowingSpear/Texture/YingYang",
                    Vector2.One * 0.33f,
                    Main.rand.NextFloat(-10f, 10f),
                    0.07f,
                    0.33f,
                    30
                );
                GeneralParticleHandler.SpawnParticle(blastRing);
            }

            // 进入最终直线冲刺阶段
            phase = 5;
            phaseTimer = 0;
        }


        private void PerformPhaseFive() // 第5阶段：冲出去
        {
            Projectile.rotation += 0.45f;
            if (++phaseTimer >= 180) // 一段时间后消除自己
            {
                Projectile.Kill();
            }
        }

        private void CreateSlashProjectile()
        {
            int slashID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PrimordialStateSlash>(), Projectile.damage * 0, 0f, Projectile.owner, Projectile.whoAmI);
            Projectile.localAI[0] = slashID + 1;
        }

        private void RandomDash()
        {
            float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.velocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * 30f;
        }

        public override void OnKill(int timeLeft)
        {
            {
                // 随机角度释放X个太极纹理
                for (int i = 0; i < 1; i++)
                {
                    Particle blastRing = new CustomPulse(
                        Projectile.Center,
                        Vector2.Zero,
                        Color.Black,
                        "CalamityThrowingSpear/Texture/YingYang",
                        Vector2.One * 0.33f,
                        Main.rand.NextFloat(-10f, 10f),
                        0.17f,
                        0.53f,
                        30
                    );
                    GeneralParticleHandler.SpawnParticle(blastRing);
                }

                // 🔴 对称爆圈 Dust（黑+白/灰）
                for (int i = 0; i < 36; i++)
                {
                    float angle = MathHelper.TwoPi * i / 36f;
                    Vector2 unit = angle.ToRotationVector2();

                    Dust dust1 = Dust.NewDustPerfect(
                        Projectile.Center + unit * 64f,
                        DustID.Shadowflame,
                        unit * 2f,
                        100,
                        Color.Black,
                        Main.rand.NextFloat(1.5f, 2.2f)
                    );
                    dust1.noGravity = true;

                    Dust dust2 = Dust.NewDustPerfect(
                        Projectile.Center + unit * 96f,
                        DustID.SilverFlame,
                        unit * 1.5f,
                        100,
                        Color.White,
                        Main.rand.NextFloat(1.2f, 1.8f)
                    );
                    dust2.noGravity = true;
                }

                // 🔥 中心烟雾爆发
                for (int i = 0; i < 20; i++)
                {
                    Vector2 smokeDir = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust smoke = Dust.NewDustPerfect(
                        Projectile.Center + smokeDir * 8f,
                        DustID.Smoke,
                        smokeDir * Main.rand.NextFloat(2f, 5f),
                        120,
                        Color.DarkGray,
                        Main.rand.NextFloat(1.3f, 2.2f)
                    );
                    smoke.noGravity = true;
                }

                // 🌪 旋转流转粒子（阴阳之舞）
                for (int i = 0; i < 18; i++)
                {
                    float angle = MathHelper.TwoPi * i / 18f + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * 48f;
                    Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 3f;

                    Dust swirl = Dust.NewDustPerfect(
                        pos,
                        DustID.Torch,
                        vel,
                        100,
                        Color.Lerp(Color.Red, Color.White, Main.rand.NextFloat()),
                        Main.rand.NextFloat(1.2f, 1.8f)
                    );
                    swirl.noGravity = true;
                }
            }

            {
                for (int i = 0; i < 40; i++)
                {
                    float angle = MathHelper.TwoPi * i / 40f;
                    Vector2 dir = angle.ToRotationVector2();

                    // 外环：白色火焰流星感
                    Dust white = Dust.NewDustPerfect(
                        Projectile.Center + dir * 90f,
                        DustID.SilverFlame,
                        dir * 4f,
                        100,
                        Color.White,
                        Main.rand.NextFloat(1.8f, 2.5f)
                    );
                    white.noGravity = true;

                    // 内环：黑紫混合烟雾
                    Dust black = Dust.NewDustPerfect(
                        Projectile.Center + dir * 60f,
                        DustID.Shadowflame,
                        dir * 2.5f,
                        100,
                        Color.Lerp(Color.Black, Color.DarkViolet, Main.rand.NextFloat()),
                        Main.rand.NextFloat(1.4f, 2.0f)
                    );
                    black.noGravity = true;
                }

                for (int i = 0; i < 30; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust smoke = Dust.NewDustPerfect(
                        Projectile.Center + offset * 8f,
                        DustID.Smoke,
                        offset * Main.rand.NextFloat(4f, 7f),
                        120,
                        Color.DarkGray,
                        Main.rand.NextFloat(1.5f, 2.8f)
                    );
                    smoke.noGravity = true;
                }

                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24f + Main.rand.NextFloat(-0.15f, 0.15f);
                    float radius = Main.rand.NextFloat(40f, 70f);
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 2.5f;

                    Dust swirl = Dust.NewDustPerfect(
                        pos,
                        DustID.Torch,
                        vel,
                        100,
                        Color.Lerp(Color.White, Color.Red, Main.rand.NextFloat(0.3f, 0.7f)),
                        Main.rand.NextFloat(1.5f, 2.2f)
                    );
                    swirl.noGravity = true;
                }

                for (int i = 0; i < 12; i++)
                {
                    Dust arc = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(40f, 40f),
                        DustID.RainbowTorch,
                        Main.rand.NextVector2Circular(2f, 2f),
                        100,
                        Color.Cyan,
                        Main.rand.NextFloat(1.3f, 1.9f)
                    );
                    arc.noGravity = true;
                }
            }



            int slashID = (int)(Projectile.localAI[0] - 1);
            if (Main.projectile.IndexInRange(slashID) && Main.projectile[slashID].type == ModContent.ProjectileType<PrimordialStateSlash>())
            {
                Main.projectile[slashID].Kill();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 切换到第二阶段
            if (phase == 1)
            {
                phase = 2;
                phaseTimer = 0; // 重置阶段计时器

                SoundStyle sound = new SoundStyle("CalamityMod/Sounds/Item/AuricBulletHit")
                {
                    Volume = 0.4f // 将音量设置为 x%
                };
                SoundEngine.PlaySound(sound, Projectile.position);
            }

            SoundEngine.PlaySound(SoundID.Item15, Projectile.position);

            
            // 电火花特效逻辑
            for (int i = 0; i < 12; i++)
            {
                int sparkLifetime = Main.rand.Next(22, 36);
                float sparkScale = Main.rand.NextFloat(0.8f, 1f);
                Color sparkColor = Color.Lerp(Color.Black, Color.Gray, Main.rand.NextFloat(0.5f, 1f)); // 黑色和灰色渐变

                if (Main.rand.NextBool(10))
                    sparkScale *= 2f;

                // 随机方向的速度
                Vector2 sparkVelocity = Main.rand.NextVector2Circular(25f, 25f); // 生成一个随机方向的速度向量，速度范围为 [-25, 25]
                SparkParticle spark = new SparkParticle(Projectile.Center, sparkVelocity, true, sparkLifetime, sparkScale, sparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 随机角度释放X个太极纹理
            for (int i = 0; i < 1; i++)
            {
                Particle blastRing = new CustomPulse(
                    Projectile.Center,
                    Vector2.Zero,
                    Color.Black,
                    "CalamityThrowingSpear/Texture/YingYang",
                    Vector2.One * 0.33f,
                    Main.rand.NextFloat(-10f, 10f),
                    0.17f,
                    0.53f,
                    30
                );
                GeneralParticleHandler.SpawnParticle(blastRing);
            }

            target.AddBuff(ModContent.BuffType<BurningBlood>(), 300);
            target.AddBuff(ModContent.BuffType<BrainRot>(), 300);   
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300); 
            target.AddBuff(31, 300);
        }







    }
}
