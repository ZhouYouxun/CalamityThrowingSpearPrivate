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
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC
{
    public class DiseasedJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/DiseasedPikeC/DiseasedJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private int frameCounter = 0; // 用于计数每帧
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
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
            Projectile.penetrate = 3; // 只允许一次穿透
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        private int trackingDelay;
        public override void OnSpawn(IEntitySource source)
        {
            // 随机生成一个 35~75 的整数
            trackingDelay = Main.rand.Next(35, 76);
        }
        public override void AI()
        {
            // 逐渐加速，每帧乘以1.015
            Projectile.velocity *= 1.005f;

            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 生成尾迹烟雾效果，每隔6帧生成一次
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 6f)
            {
                for (int d = 0; d < 5; d++)
                {
                    // 在弹幕的中心点为圆心，X范围内随机生成粒子
                    Vector2 randomOffset = Main.rand.NextVector2Circular(1 * 16, 1 * 16);
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center + randomOffset, // 粒子生成位置
                        Main.rand.Next(new int[] { DustID.TerraBlade, DustID.GemEmerald, DustID.GreenTorch }), // 随机粒子类型
                        Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f) // 初始速度为弹幕正前方
                    );
                    dust.color = Color.DarkGreen; // 粒子染成深绿色
                    dust.noGravity = false; // 粒子受重力影响
                    dust.scale = Main.rand.NextFloat(0.65f, 1.25f); // 粒子大小随机在 0.65~1.25
                }
            }

            if (Projectile.ai[1] > trackingDelay)
            {
                // 超过45帧后开始追踪敌人
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 调整速度
                }
            }
            else
            {
                // 未到45帧时，每帧随机左右拐1~2度
                float angle = MathHelper.ToRadians(Main.rand.Next(1, 3)) * (Main.rand.Next(0, 2) == 0 ? -1 : 1); // 随机左或右1~2度
                Projectile.velocity = Projectile.velocity.RotatedBy(angle);
                Projectile.ai[1]++;
            }
        }

        // 击中敌人时粘附效果，并造成三次伤害
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Projectile.ai[0] = 2f; // 设置粘附状态
            Projectile.timeLeft = 180; // 时间为 X 帧
            // Projectile.velocity = Vector2.Zero; // 停止弹幕移动
            target.AddBuff(BuffID.Venom, 180); // 给敌人施加毒液效果
            target.AddBuff(ModContent.BuffType<Plague>(), 180);
            target.AddBuff(BuffID.Poisoned, 180);
            // 连续造成三次伤害，每次间隔
            //Projectile.ModifyHitNPCSticky(30); // 连续三次伤害

            // 随机选择一个360度的方向
            float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机角度（0到2π弧度）
            Vector2 randomDirection = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)); // 计算随机方向

            // 调整速度为原速度的随机比例（0.25到0.45之间）
            float speedMultiplier = Main.rand.NextFloat(0.25f, 0.45f);
            float newSpeed = Projectile.velocity.Length() * speedMultiplier;

            // 如果计算后的速度低于xf，则将速度强行设置为xf
            newSpeed = Math.Max(newSpeed, 5f);

            // 设置弹幕的新速度
            Projectile.velocity = randomDirection * newSpeed;
        }

        // 粘附效果，弹幕消失时发射 DiseasedJavLight
        public override void OnKill(int timeLeft)
        {
            // 生成360度发射的DiseasedJavLight
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi / 3 * i;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 5f, ModContent.ProjectileType<DiseasedJavLight>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
            }

            Particle blastRing = new CustomPulse(
                Projectile.Center, Vector2.Zero, Color.DarkGreen,
                "CalamityThrowingSpear/Texture/BiologicalHazards",
                Vector2.One * 0.33f, Main.rand.NextFloat(-10f, 10f),
                0.078f, 0.650f, 30
            );
            GeneralParticleHandler.SpawnParticle(blastRing);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero, // 静止在原地
                ModContent.ProjectileType<DiseasedJavEXP>(),
                (int)(Projectile.damage * 0.2f), // 伤害倍率为 0.75
                Projectile.knockBack,
                Projectile.owner
            );

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 生成粒子特效
            for (int i = 0; i < 3; i++)
            {
                // 随机选择一个方向
                Vector2 randomDirection = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitX);

                // 创建小圆圈的粒子
                int particleCount = Main.rand.Next(20, 36); // 每个圆圈20~35个粒子
                float radius = 2 * 16; // 小圆圈半径

                for (int j = 0; j < particleCount; j++)
                {
                    // 计算粒子的位置
                    float angle = MathHelper.TwoPi * (j / (float)particleCount);
                    Vector2 particlePosition = Projectile.Center + randomDirection * radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                    // 生成粒子
                    Dust dust = Dust.NewDustPerfect(
                        particlePosition,
                        Main.rand.Next(new int[] { DustID.GreenTorch, DustID.WhiteTorch }),
                        randomDirection * Main.rand.NextFloat(1f, 3f) // 粒子速度
                    );
                    dust.color = Color.DarkGreen; // 染色为深绿色
                    dust.noGravity = false; // 受重力影响
                    dust.scale = Main.rand.NextFloat(1.2f, 1.8f); // 随机缩放大小
                }
            }
        }
    }
}
