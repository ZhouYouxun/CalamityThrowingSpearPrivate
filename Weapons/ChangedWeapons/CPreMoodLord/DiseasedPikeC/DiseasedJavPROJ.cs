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
                    Dust dust = Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.TerraBlade, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1f)];
                    dust.velocity = Vector2.Zero;
                    dust.position -= Projectile.velocity / 5f * d;
                    dust.noGravity = true;
                    dust.scale = 0.65f;
                    dust.noLight = true;
                }
            }

            // 前x帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 30)
            {
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找xx范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 追踪速度为xf
                }
            }
            else
            {
                Projectile.ai[1]++;
            }
        }

        // 击中敌人时粘附效果，并造成三次伤害
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Projectile.ai[0] = 2f; // 设置粘附状态
            Projectile.timeLeft = 90; // 粘附时间为 90 帧
            // Projectile.velocity = Vector2.Zero; // 停止弹幕移动
            target.AddBuff(BuffID.Venom, 180); // 给敌人施加毒液效果
            target.AddBuff(ModContent.BuffType<Plague>(), 180);
            target.AddBuff(BuffID.Poisoned, 180);
            // 连续造成三次伤害，每次间隔
            //Projectile.ModifyHitNPCSticky(30); // 连续三次伤害

            // 随机选择一个360度的方向
            float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi); // 随机角度（0到2π弧度）
            Vector2 randomDirection = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)); // 计算随机方向

            // 调整速度为原速度的随机比例（0.6到0.9之间）
            float speedMultiplier = Main.rand.NextFloat(0.6f, 0.9f);
            float newSpeed = Projectile.velocity.Length() * speedMultiplier;

            // 如果计算后的速度低于xf，则将速度强行设置为xf
            newSpeed = Math.Max(newSpeed, 15f);

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
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * 5f, ModContent.ProjectileType<DiseasedJavLight>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner);
            }
        }



    }

}
