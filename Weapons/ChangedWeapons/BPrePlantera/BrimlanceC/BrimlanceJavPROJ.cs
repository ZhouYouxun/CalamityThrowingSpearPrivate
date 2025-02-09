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
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.BrimlanceC
{
    public class BrimlanceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/BPrePlantera/BrimlanceC/BrimlanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";

        private bool hasBounced = false; // 记录是否已经击中过敌人

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
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 产生DNA形状的粒子特效（颜色改为红色）
            float frequency = 30f;  // 30帧一个回合
            float amplitude = 20f;  // 振动幅度

            Vector2 leftOffset = new Vector2(-amplitude * (float)Math.Sin(Projectile.ai[0] * MathHelper.TwoPi / frequency), 0);
            Vector2 rightOffset = new Vector2(amplitude * (float)Math.Sin(Projectile.ai[0] * MathHelper.TwoPi / frequency), 0);

            if (Projectile.ai[0] % 2 == 0)
            {
                Dust.NewDustPerfect(Projectile.Center + leftOffset, DustID.RedTorch, Vector2.Zero, 0, Color.Red, 1.2f).noGravity = true;
                Dust.NewDustPerfect(Projectile.Center + rightOffset, DustID.RedTorch, Vector2.Zero, 0, Color.Red, 1.2f).noGravity = true;
            }

            Projectile.ai[0] += 2f;  // 更新ai，两倍的绘制速度

            // 生成粉色线性粒子效果
            if (Main.rand.NextBool(5))
            {
                // 直接在弹幕中心生成粒子，没有左右偏移
                Vector2 trailPos = Projectile.Center;

                float trailScale = Main.rand.NextFloat(0.8f, 1.2f); // 维持粒子的缩放效果
                Color trailColor = Color.OrangeRed;

                // 创建粒子
                Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(trail);
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

            //SoundEngine.PlaySound(SoundID.Item68, Projectile.Center);

            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300); // 硫磺火

            // 第1次击中敌人后，释放爆炸弹幕 "FuckYou"
            if (!hasBounced)
            {
                hasBounced = true;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);

                // 瞄准最近的敌人并调整弹幕方向
                NPC closestNPC = Main.npc
                    .Where(npc => npc.active && !npc.friendly && npc.life > 0 && npc.whoAmI != target.whoAmI)
                    .OrderBy(npc => Vector2.Distance(npc.Center, Projectile.Center))
                    .FirstOrDefault();

                if (closestNPC != null)
                {
                    Vector2 direction = closestNPC.Center - Projectile.Center;
                    Projectile.velocity = Vector2.Normalize(direction) * Projectile.velocity.Length();
                }
            }
            else
            {
                // 第2次击中敌人后，触发BrimlanceHellfireExplosion
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BrimlanceHellfireExplosion>(), (int)(Projectile.damage * 1.5f), Projectile.knockBack, Projectile.owner);

                // 从天上降下1发BrimlanceStandingFire弹幕
                for (int i = 0; i < 1; i++)
                {
                    SummonBrimlanceFire(target);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), (int)(Projectile.damage * 0f), Projectile.knockBack, Projectile.owner);
        }


        // 从天上降下BrimlanceStandingFire弹幕
        private void SummonBrimlanceFire(NPC npc)
        {
            Player player = Main.player[npc.target];
            Vector2 targetPosition = npc.Center;
            float radius = 50f * 16f;
            float arrowSpeed = 10f;

            float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 spawnPosition = targetPosition + radius * new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

            Vector2 direction = targetPosition - spawnPosition;
            direction.Normalize();
            float speedX = direction.X * arrowSpeed + Main.rand.Next(-120, 121) * 0.01f;
            float speedY = direction.Y * arrowSpeed + Main.rand.Next(-120, 121) * 0.01f;

            int newDamage = (int)(Projectile.damage * 0.5f); // 伤害为本体的50%
            Projectile.NewProjectile(npc.GetSource_FromThis(), spawnPosition, new Vector2(speedX, speedY), ModContent.ProjectileType<BrimlanceStandingFire>(), newDamage, 0, player.whoAmI);
        }



    }
}
