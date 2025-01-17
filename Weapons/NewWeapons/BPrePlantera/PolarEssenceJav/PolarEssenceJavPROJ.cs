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
        private bool dnaEffectActive = true;
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

                // DNA 双链粒子特效
                if (dnaEffectActive)
                {
                    //float offset = (float)Math.Sin(Projectile.localAI[0] * 0.1f) * 5f; // 将振幅从10f减小到5f
                    //Vector2 dnaPos1 = Projectile.Center + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * offset;
                    //Vector2 dnaPos2 = Projectile.Center + Projectile.velocity.RotatedBy(-MathHelper.PiOver2) * offset;

                    //Dust.NewDustPerfect(dnaPos1, DustID.BlueCrystalShard, Vector2.Zero).noGravity = true;
                    //Dust.NewDustPerfect(dnaPos2, DustID.WhiteTorch, Vector2.Zero).noGravity = true;
                }
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
                dnaEffectActive = false; // 停止 DNA 双链粒子特效
                Projectile.velocity *= 0.8f; // 模仿穿过敌人后的减速效果
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);

            // 创建雪花形法阵效果
            int numParticles = 60; // 总粒子数
            float radius = 50f; // 法阵半径
            float expansionSpeed = 2f; // 粒子向外扩展的速度

            for (int i = 0; i < numParticles; i++)
            {
                // 计算粒子初始位置，以雪花形分布
                float angle = MathHelper.TwoPi / 6 * (i % 6) + Main.rand.NextFloat(-MathHelper.PiOver4 / 2, MathHelper.PiOver4 / 2); // 每6个粒子形成一个“雪花分支”
                float offset = radius * (1 + Main.rand.NextFloat(-0.2f, 0.2f)); // 半径随机化

                Vector2 startPosition = Projectile.Center + angle.ToRotationVector2() * offset;

                // 设置粒子的速度，以随机扩散形式
                float particleAngle = angle + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4); // 偏移角度
                Vector2 velocity = particleAngle.ToRotationVector2() * Main.rand.NextFloat(2f, expansionSpeed);

                // 创建粒子
                Dust dust = Dust.NewDustPerfect(startPosition, DustID.BlueCrystalShard, velocity, 100, Color.White, 1.5f);
                dust.noGravity = true; // 悬浮粒子效果
                dust.fadeIn = 1.2f; // 逐渐消失
                dust.scale = Main.rand.NextFloat(1.0f, 1.5f); // 粒子大小随机化
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


