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
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PolarEssenceJav
{
    public class PolarEssenceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/PolarEssenceJav/PolarEssenceJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        private bool hasGainedHoming = false;
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
            Projectile.penetrate = 2; // 只允许一次伤害
            Projectile.timeLeft = 720;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.scale = 0.75f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.WhiteSmoke.ToVector3() * 0.55f);

            // DNA 双链粒子特效，一直存在不再取消
            float offset = (float)Math.Sin(Projectile.localAI[0] * 0.1f) * 5f; // 将振幅从10f减小到5f
            Vector2 dnaPos1 = Projectile.Center + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * offset;
            Vector2 dnaPos2 = Projectile.Center + Projectile.velocity.RotatedBy(-MathHelper.PiOver2) * offset;
            Dust.NewDustPerfect(dnaPos1, DustID.BlueCrystalShard, Vector2.Zero).noGravity = true;
            Dust.NewDustPerfect(dnaPos2, 185, Vector2.Zero).noGravity = true;

            // 第一阶段加速
            if (!hasGainedHoming)
            {
                Projectile.velocity *= 1.005f;


            }
            else
            {
                // 开启追踪效果
                CalamityUtils.HomeInOnNPC(Projectile, true, 2000f, 18, 200f);

                // 冰刺特效
                Dust iceDust = Dust.NewDustPerfect(Projectile.Center, DustID.Ice, Projectile.velocity * 0.1f, 0, Color.SkyBlue, 1.2f);
                iceDust.noGravity = true;
                if (Projectile.localAI[0] % 3 == 0)
                {
                    Dust subIceDust = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, Projectile.velocity * 0.01f, 0, Color.SkyBlue, 1.1f);
                    subIceDust.noGravity = true;
                }

                SparkParticle Visual = new SparkParticle(Projectile.Center, Projectile.velocity * 0.1f, false, 2, 1.2f, Color.SkyBlue);
                GeneralParticleHandler.SpawnParticle(Visual);
                if (Projectile.localAI[0] % 3 == 0)
                {
                    LineParticle subTrail = new LineParticle(Projectile.Center, Projectile.velocity * 0.01f, false, 4, 1.1f, Color.SkyBlue);
                    GeneralParticleHandler.SpawnParticle(subTrail);
                }
            }

            Projectile.localAI[0]++; // 更新粒子动画
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 300); // 原版的霜火效果

            if (!hasGainedHoming)
            {
                hasGainedHoming = true;
                Projectile.velocity *= 0.8f; // 模仿穿过敌人后的减速效果

                // === ❄️ 命中后随机 ±？0° 偏转角度 ===
                float randomRotation = Main.rand.NextFloat(-MathHelper.ToRadians(10f), MathHelper.ToRadians(10f));
                Projectile.velocity = Projectile.velocity.RotatedBy(randomRotation);

            }

            {
                int totalParticles = 120;
                float goldenAngle = MathHelper.ToRadians(137.5f);
                Vector2 sprayDirection = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitY); // 随机喷射方向

                for (int i = 0; i < totalParticles; i++)
                {
                    // 核心射流方向加黄金角散射
                    float angle = i * goldenAngle * 0.15f; // 缩小黄金角扩散幅度形成更集中束
                    Vector2 baseDirection = sprayDirection.RotatedBy(angle);

                    // 玫瑰曲线调节喷射速度（速度波动）
                    float theta = MathHelper.TwoPi * i / totalParticles;
                    float roseFactor = 1f + 0.3f * (float)Math.Sin(6 * theta); // 六瓣波动

                    // 阿基米德螺旋递增
                    float spiralT = i * 0.15f;
                    float spiralRadius = 3f + 0.15f * spiralT;

                    Vector2 velocity = baseDirection * spiralRadius * roseFactor * Main.rand.NextFloat(2f, 5f);

                    int dustType = Main.rand.Next(new int[] { DustID.Ice, DustID.BlueCrystalShard, DustID.Snow, DustID.SnowBlock });
                    Color dustColor = Color.Lerp(Color.LightBlue, Color.White, Main.rand.NextFloat(0.3f, 0.7f));

                    Dust snowDust = Dust.NewDustPerfect(
                        target.Center,
                        dustType,
                        velocity,
                        100,
                        dustColor,
                        Main.rand.NextFloat(1.3f, 1.7f)
                    );
                    snowDust.noGravity = true;
                }
            }



        }


        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);

            Particle blastRing = new CustomPulse(
                Projectile.Center,
                Vector2.Zero,
                Color.White,
                "CalamityThrowingSpear/Texture/christmas512",
                Vector2.One * 0.33f,
                Main.rand.NextFloat(-10f, 10f),
                0.10f,
                0.25f,
                30
            );
            GeneralParticleHandler.SpawnParticle(blastRing);

            // === 无序爆散圆圈 ===
            int numParticles = 60;
            float radius = 50f;
            float expansionSpeed = 2f;
            for (int i = 0; i < numParticles; i++)
            {
                float angle = MathHelper.TwoPi / 6 * (i % 6) + Main.rand.NextFloat(-MathHelper.PiOver4 / 2, MathHelper.PiOver4 / 2);
                float offset = radius * (1 + Main.rand.NextFloat(-0.2f, 0.2f));
                Vector2 startPosition = Projectile.Center + angle.ToRotationVector2() * offset;
                float particleAngle = angle + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                Vector2 velocity = particleAngle.ToRotationVector2() * Main.rand.NextFloat(2f, expansionSpeed);
                Dust dust = Dust.NewDustPerfect(startPosition, DustID.BlueCrystalShard, velocity, 100, Color.White, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
                dust.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            // === 有序雪花几何法阵 ===
            int[] snowDustTypes = { 68, 137, 80, 135, 185 };
            int totalRays = 6;
            int raysPerBranch = 6;
            float branchLength = 60f;
            float innerRadius = 12f;

            for (int i = 0; i < totalRays; i++)
            {
                float baseAngle = MathHelper.TwoPi / totalRays * i;

                for (int j = 0; j <= raysPerBranch; j++)
                {
                    float dist = innerRadius + (branchLength * j / raysPerBranch);
                    Vector2 pos = Projectile.Center + baseAngle.ToRotationVector2() * dist;
                    Vector2 vel = baseAngle.ToRotationVector2() * 1.8f * (1f + j * 0.1f) + Main.rand.NextVector2Circular(0.5f, 0.5f);

                    int dustType = snowDustTypes[Main.rand.Next(snowDustTypes.Length)];
                    Dust dust = Dust.NewDustPerfect(pos, dustType, vel, 100, Color.Cyan, 1.6f + Main.rand.NextFloat(-0.3f, 0.3f));
                    dust.noGravity = true;
                    dust.fadeIn = 1.3f;
                }

                // 添加分叉枝干
                for (int j = 1; j < raysPerBranch; j++)
                {
                    float dist = innerRadius + (branchLength * j / raysPerBranch);
                    Vector2 basePos = Projectile.Center + baseAngle.ToRotationVector2() * dist;

                    for (int b = -1; b <= 1; b += 2) // 左右两边
                    {
                        float branchAngle = baseAngle + b * MathHelper.PiOver4;
                        Vector2 sidePos = basePos + branchAngle.ToRotationVector2() * 8f;
                        Vector2 sideVel = branchAngle.ToRotationVector2() * 1.6f + Main.rand.NextVector2Circular(0.3f, 0.3f);

                        int dustType = snowDustTypes[Main.rand.Next(snowDustTypes.Length)];
                        Dust dust = Dust.NewDustPerfect(sidePos, dustType, sideVel, 120, Color.LightBlue, 1.3f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.1f;
                    }
                }
            }



            //检查是否存在 CoolWhipProj
            bool coolWhipExists = Main.projectile.Any(proj => proj.active && proj.type == 917);
            if (!coolWhipExists)
            {
                // 获取弹幕所属玩家
                Player owner = Main.player[Projectile.owner];
                bool isInTundra = owner != null && owner.ZoneSnow; // 检测玩家是否在苔原区域

                // 生成 CoolWhipProj
                int projID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, 917, (int)(Projectile.damage * 1.5f), 0, Projectile.owner);

                // 修改生成投射物的属性
                if (projID != Main.maxProjectiles)
                {
                    Projectile newProj = Main.projectile[projID];
                    if (newProj != null && newProj.active)
                    {
                        //newProj.timeLeft = isInTundra ? 600 : 300; // 如果在苔原，持续时间为 600 帧，否则为 300 帧
                        newProj.timeLeft = 600; // 直接就600，没有加强和减弱的不同版本
                        newProj.usesLocalNPCImmunity = true; // 启用局部无敌帧
                                                             //newProj.localNPCHitCooldown = isInTundra ? 10 : 20; // 如果在苔原，冷却时间设置为 10 帧，否则为 20 帧
                        newProj.localNPCHitCooldown = 10;
                    }

                    // 生成 X 形粒子特效链
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 direction = Vector2.UnitX.RotatedBy(MathHelper.PiOver4 * i); // 旋转45度生成X形
                        for (int j = 0; j < 3; j++)
                        {
                            Dust chainDust = Dust.NewDustPerfect(Projectile.Center, DustID.Snow, direction * (1 + j * 0.5f), 0, Color.Cyan, 1.0f);
                            chainDust.noGravity = true;
                        }
                    }
                }

                int Dusts = 15;
                float radians = MathHelper.TwoPi / Dusts;
                Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                for (int i = 0; i < Dusts; i++)
                {
                    Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                    Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.WhiteSmoke, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }
            }
        }







    }
}


