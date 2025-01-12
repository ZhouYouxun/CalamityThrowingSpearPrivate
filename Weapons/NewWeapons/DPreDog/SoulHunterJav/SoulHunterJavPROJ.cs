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
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav
{
    public class SoulHunterJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/SoulHunterJav/SoulHunterJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        private int phase = 1; // 阶段控制
        private int stayCounter = 0; // 停留计数
        private bool hasDashed = false; // 是否已冲刺
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
            Projectile.penetrate = -1; // 允许-1次伤害
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }

        public override void AI()
        {

            // Lighting - 添加蓝色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 0.55f);

            // 生成尾部气泡效果，每隔30帧生成一次（比参考代码密度低）
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 30f)
            {
                // 生成随机的黑色气泡粒子
                for (int d = 0; d < 3; d++) // 生成较少的粒子
                {
                    Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1f)];
                    dust.velocity = Vector2.Zero;
                    dust.position -= Projectile.velocity / 5f * d;
                    dust.noGravity = true;
                    dust.scale = 0.65f;
                    dust.color = Color.Black; // 颜色设为黑色
                }
                Projectile.ai[0] = 0f; // 重置计时器
            }


            if (phase == 1)
            {
                // 旋转效果
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                // 阶段1：减速直到停下
                Projectile.velocity *= 0.97f;
                if (Projectile.velocity.Length() < 0.1f)
                {
                    Projectile.velocity = Vector2.Zero;

                    // 停在原地，释放粒子特效
                    stayCounter++;
                    for (int i = 0; i < Main.rand.Next(3, 6); i++)
                    {
                        Vector2 offset = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(5f, 10f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Water, offset * 0.5f, 0, Color.DarkBlue, Main.rand.NextFloat(1f, 2f));
                        dust.noGravity = true;
                    }

                    // 停留10帧后进入阶段2
                    if (stayCounter > 10)
                    {
                        phase = 2;
                    }
                }
                //if (Projectile.velocity.Length() >= 0.1f)
                //{
                //    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                //}
            }
            else if (phase == 2 && !hasDashed)
            {
                // 旋转效果
                Projectile.rotation += 1;

                // 每20帧生成一个深蓝色冲击波，向自己吸引
                if (Projectile.ai[1] % 20 == 0)
                {
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.BlueViolet, new Vector2(1.5f), Projectile.rotation, 1f, -0.1f, 30); // 负0.1f 表示吸引效果
                    GeneralParticleHandler.SpawnParticle(pulse);
                }

                // 寻找敌人并准备冲刺
                NPC target = FindTarget(1500f);
                if (target != null)
                {
                    Vector2 direction = Vector2.Normalize(target.Center - Projectile.Center);
                    Projectile.velocity = direction * 20f; // 冲刺速度

                    // 冲刺瞬间释放气泡粒子
                    for (int i = 0; i < 30; i++)
                    {
                        float angle = MathHelper.ToRadians(15) * (Main.rand.NextBool() ? 1 : -1);
                        Vector2 bubbleVelocity = direction.RotatedBy(angle) * Main.rand.NextFloat(2f, 6f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Water, bubbleVelocity, 0, Color.DarkBlue, Main.rand.NextFloat(1f, 2f));
                        dust.noGravity = true;
                    }

                    hasDashed = true;
                }
                Projectile.ai[1]++; // 更新计时器
            }

            else if (hasDashed)
            {
                // 旋转效果
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                // 击中敌人后进行间歇性追踪
                IntermittentHoming(1500f, 60f);
            }


        }

        // 寻找最近的敌人
        private NPC FindTarget(float range)
        {
            NPC closestNPC = null;
            float closestDistance = range;

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy(Projectile) && Projectile.Distance(npc.Center) < closestDistance)
                {
                    closestDistance = Projectile.Distance(npc.Center);
                    closestNPC = npc;
                }
            }

            return closestNPC;
        }

        // 间歇性追踪逻辑
        private void IntermittentHoming(float range, float turnSpeed)
        {
            if (Projectile.ai[0] == 1f)
            {
                Projectile.ai[1] += 1f;
                if (Projectile.ai[1] > 25f)
                {
                    Projectile.ai[1] = 0f;
                    Projectile.ai[0] = 0f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    return;
                }
            }

            NPC target = FindTarget(range);
            if (target != null && Projectile.ai[0] == 0f)
            {
                float angleToTarget = Projectile.AngleTo(target.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(angleToTarget, turnSpeed).ToRotationVector2() * 20f;
                Projectile.ai[0] = 1f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 生成规则扩散的深蓝色线性粒子特效
            int points = 30; // 生成更多的粒子
            float radians = MathHelper.TwoPi / points;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            float rotRando = Main.rand.NextFloat(0.1f, 2.5f);
            for (int k = 0; k < points; k++)
            {
                Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f * rotRando);
                LineParticle subTrail = new LineParticle(Projectile.Center + velocity * 20.5f, velocity * 15, false, 30, 0.75f, Color.Blue); // 蓝色粒子
                GeneralParticleHandler.SpawnParticle(subTrail);
            }

            // 在 OnKill 中生成深蓝色的水泡特效
            int bubbleCount = 70; // 生成 70 个水泡
            for (int i = 0; i < bubbleCount; i++)
            {
                Vector2 randomOffset = Main.rand.NextVector2Circular(90f, 90f); // 随机位置偏移
                Vector2 bubblePosition = Projectile.Center + randomOffset;
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), bubblePosition, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                bubble.timeLeft = 8 + Main.rand.Next(6);
                bubble.scale = Main.rand.NextFloat(0.6f, 1f);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
                bubble.velocity *= 0.3f; // 减缓速度，让效果更像漂浮
            }


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }
    }
}