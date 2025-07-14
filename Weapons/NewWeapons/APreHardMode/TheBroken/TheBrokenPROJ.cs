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
using CalamityMod.Particles;
using CalamityRangerExpansion.LightingBolts;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken
{
    public class TheBrokenPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/TheBroken/TheBroken";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        private bool EnableStickMode => _enableStickMode;
        private bool _enableStickMode = false;
        private bool stuckToTarget = false;
        private NPC stuckTarget = null;
        private int spawnCounter = 0;

        public override bool PreDraw(ref Color lightColor)
        {
            if (stuckToTarget)
                return false; // 完全隐形
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3; // 允许？次伤害
            Projectile.timeLeft = 120 * 3;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 初始化：仅执行一次，使用稳定的 localAI[0] 决定是否启用扎入模式
            if (!Projectile.localAI[1].Equals(1f))
            {
                _enableStickMode = Projectile.localAI[0] == 1f;
                Projectile.localAI[1] = 1f; // 已初始化
            }

            // 自定义重力影响
            Projectile.velocity.Y += 0.075f;

            // 旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 粒子特效始终保留
            GenerateSilverDustAndSparks();

            if (_enableStickMode)
            {
                // 扎入模式
                if (stuckToTarget && stuckTarget != null && stuckTarget.active)
                {
                    // 紧跟目标
                    Projectile.Center = stuckTarget.Center + new Vector2(0, -16f);

                    // 持续生成飞刀手里剑
                    spawnCounter++;
                    if (spawnCounter % 5 == 0)
                    {
                        int[] projectileTypes = { ProjectileID.Shuriken, ProjectileID.ThrowingKnife };
                        int type = projectileTypes[Main.rand.Next(projectileTypes.Length)];

                        // 范围更狂野一些（更远更随机）🐘
                        Vector2 spawnPos = Projectile.Center + new Vector2(
                            Main.rand.NextFloat(-64f, 64f),  // 水平方向扩大
                            Main.rand.NextFloat(-96f, -64f)  // 垂直方向提高
                        );

                        Vector2 velocity = (Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(9f, 12f);

                        int projID = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            spawnPos,
                            velocity,
                            type,
                            (int)(Projectile.damage * 0.5f),
                            Projectile.knockBack,
                            Projectile.owner
                        );

                        if (projID.WithinBounds(Main.maxProjectiles))
                        {
                            Projectile proj = Main.projectile[projID];
                            proj.friendly = true;
                            proj.hostile = false;
                            proj.penetrate = 1;
                            proj.usesLocalNPCImmunity = true;
                            proj.localNPCHitCooldown = 60;
                        }

                        CTSLightingBoltsSystem.Spawn_SilverSpearGlow(spawnPos);

                        // 🌟 CritSpark 特效喷射
                        Vector2 sparkDirection = -(Projectile.Center - spawnPos).SafeNormalize(Vector2.UnitY);
                        int sparkCount = Main.rand.Next(2, 4);
                        for (int j = 0; j < sparkCount; j++)
                        {
                            Vector2 sparkVelocity = sparkDirection.RotatedByRandom(MathHelper.ToRadians(12f)) * Main.rand.NextFloat(3f, 5f);
                            CritSpark spark = new CritSpark(
                                spawnPos,
                                sparkVelocity,
                                Color.White,
                                Color.LightBlue,
                                0.7f,
                                18
                            );
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                    }



                    // 平滑跟随敌人
                    Projectile.Center = Vector2.Lerp(Projectile.Center, stuckTarget.Center, 0.2f);
                }
            }
            else
            {
                // 飞刀雨模式
                Projectile.localAI[2]++; // 帧计数

                if (Projectile.localAI[2] < 5)
                    Projectile.tileCollide = false;
                else
                    Projectile.tileCollide = true;

                // 35帧时触发散射飞刀飞镖
                if (Projectile.localAI[2] == 25)
                    ScatterShurikenAndKnives();
            }
        }




        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {


        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            if (EnableStickMode && !stuckToTarget && target.CanBeChasedBy())
            {
                stuckToTarget = true;
                stuckTarget = target;
                Projectile.timeLeft = 250; // 固定持续时间
                Projectile.tileCollide = false;
                Projectile.velocity = Vector2.Zero; // 停止运动
            }
        }

        private void GenerateSilverDustAndSparks()
        {
            // 银光Dust
            if (Main.rand.NextBool(1))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Silver,
                    -Projectile.velocity * 0.2f,
                    150,
                    Color.White,
                    Main.rand.NextFloat(0.9f, 1.2f)
                );
                d.noGravity = true;
            }

            // CritSpark闪光十字
            if (Main.rand.NextBool(1))
            {
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 sparkVelocity = direction.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)) * 5f;
                CritSpark spark = new CritSpark(
                    Projectile.Center,
                    sparkVelocity,
                    Color.White,
                    Color.LightBlue,
                    0.8f, // 缩放适中
                    18 // 寿命稍长
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 深蓝SparkParticle（冷调提升高级感）
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(1f, 1f) * 3f;
                Particle spark = new SparkParticle(
                    Projectile.Center,
                    sparkVel,
                    false,
                    20,
                    0.7f,
                    Color.LightSteelBlue
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }


        private void ScatterShurikenAndKnives()
        {
            int[] projectileTypes = { ProjectileID.Shuriken, ProjectileID.ThrowingKnife };
            int projectiles = 6; // 3 shuriken + 3 knives
            float radius = 64f; // 翻倍范围

            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY); // 正前方方向
            Vector2 spreadBase = forward * 10f; // 基础喷射速度

            for (int i = 0; i < projectiles; i++)
            {
                // 在正前方附近随机偏移范围散射
                Vector2 randomOffset = Main.rand.NextVector2Circular(radius, radius);
                Vector2 spawnPos = Projectile.Center + randomOffset;

                // 以正前方为基础
                Vector2 baseDirection = forward;

                // 在 ±10° 内随机旋转方向
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f));
                Vector2 direction = baseDirection.RotatedBy(angleOffset);

                // 速度正负30%随机浮动
                float speedMultiplier = Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 velocity = direction * spreadBase.Length() * speedMultiplier;

                // 发射飞刀或手里剑
                int projID = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velocity,
                    projectileTypes[i % 2],
                    (int)(Projectile.damage * 0.4f),
                    Projectile.knockBack,
                    Projectile.owner
                );

                if (projID.WithinBounds(Main.maxProjectiles))
                {
                    Projectile proj = Main.projectile[projID];
                    proj.friendly = true;
                    proj.hostile = false;
                    proj.penetrate = 1;
                    proj.usesLocalNPCImmunity = true;
                    proj.localNPCHitCooldown = 60;
                }


                // 在落点生成银色光点
                CTSLightingBoltsSystem.Spawn_SilverSpearGlow(spawnPos);

                // 可保留原银色 Dust 辅助可视化
                Dust d = Dust.NewDustPerfect(
                    spawnPos,
                    DustID.Silver,
                    velocity * 0.1f,
                    120,
                    Color.White,
                    1.2f
                );
                d.noGravity = true;

                // 生成 2~3 个 CritSpark 往正后方退，形成爆散感
                int critSparks = Main.rand.Next(2, 4);
                for (int j = 0; j < critSparks; j++)
                {
                    Vector2 backwardVelocity = -forward.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(3f, 5f);
                    CritSpark spark = new CritSpark(
                        spawnPos,
                        backwardVelocity,
                        Color.White,
                        Color.LightBlue,
                        0.7f,
                        16
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
        }


        public override void OnKill(int timeLeft)
        {
            CreateSilverDeathEffect();
        }

        private void CreateSilverDeathEffect()
        {
            int particles = 30;
            for (int i = 0; i < particles; i++)
            {
                float angle = MathHelper.TwoPi * i / particles + Main.rand.NextFloat(-0.05f, 0.05f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Silver,
                    velocity,
                    120,
                    Color.White,
                    Main.rand.NextFloat(1.0f, 1.3f)
                );
                d.noGravity = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                CritSpark spark = new CritSpark(
                    Projectile.Center,
                    velocity,
                    Color.White,
                    Color.LightBlue,
                    0.8f,
                    20
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }



    }
}