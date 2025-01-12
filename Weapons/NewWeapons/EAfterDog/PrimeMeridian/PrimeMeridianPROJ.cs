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
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Rogue;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian
{
    public class PrimeMeridianPROJ : ModProjectile
    {
        private int phase = 1;
        private int phaseTimer = 0;
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
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2; // 只允许一次伤害
            Projectile.timeLeft = 520;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            //initialSpeed = Projectile.velocity.Length(); // 初始速度
        }

        private bool hasHitFirstEnemy = false;
        private NPC targetEnemy = null;
        private float initialSpeed;
        private float noDamageTime = 0;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasHitFirstEnemy)
            {
                // 标记已击中第一个敌人，弹幕进入不可选中状态
                hasHitFirstEnemy = true;
                noDamageTime = Projectile.timeLeft - 1;
                initialSpeed = Projectile.velocity.Length(); // 记录初始速度

                // 设置旋转角度
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
        }

        public override bool? CanDamage()
        {
            // 确保弹幕在 noDamageTime 设置期间无法造成伤害
            return Projectile.timeLeft <= noDamageTime ? false : (bool?)null;
        }

        // 手动恢复弹幕可造成伤害的状态
        private void EnableDamage()
        {
            noDamageTime = 0; // 重置 noDamageTime 以恢复伤害
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 星空链的绘制
            Player player = Main.player[Projectile.owner];
            Vector2 directionToPlayer = player.Center - Projectile.Center;

            float chainOpacity = 0.5f;
            Color chainColor = Color.Lerp(Color.DarkBlue, Color.Purple, 0.5f);

            for (float i = 0f; i <= 1f; i += 0.1f)
            {
                Vector2 chainPosition = Vector2.Lerp(Projectile.Center, player.Center, i);
                Dust dust = Dust.NewDustPerfect(chainPosition, DustID.Enchanted_Gold);
                dust.noGravity = true;
                dust.scale = 0.9f;
                dust.color = chainColor * chainOpacity;
            }

            // 寻找并锁定敌人
            if (hasHitFirstEnemy && targetEnemy == null)
            {
                targetEnemy = FindClosestEnemy(Projectile.Center, 1000 * 16);

                if (targetEnemy != null)
                {
                    // 恢复可造成伤害的状态
                    EnableDamage();

                    // 旋转到指向敌人的角度
                    Vector2 directionToTarget = (targetEnemy.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.rotation = Projectile.rotation.AngleLerp(directionToTarget.ToRotation() + MathHelper.PiOver4, 0.1f);

                    // 更新速度至初始速度的两倍，并冲向目标敌人
                    Projectile.velocity = directionToTarget * (initialSpeed * 2);
                }
            }
        }

        // 搜索最近的敌人
        private NPC FindClosestEnemy(Vector2 position, float maxDistance)
        {
            NPC closestEnemy = null;
            float closestDistance = maxDistance;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy(this))
                {
                    float distance = Vector2.Distance(position, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = npc;
                    }
                }
            }

            return closestEnemy;
        }

        public override void OnKill(int timeLeft)
        {
            // 在弹幕销毁时生成超新星爆炸效果
            var source = Projectile.GetSource_FromThis();
            Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SupernovaStealthBoom>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
    }
}