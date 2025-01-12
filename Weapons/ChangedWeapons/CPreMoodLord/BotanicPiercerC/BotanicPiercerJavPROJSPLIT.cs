using CalamityMod.Particles;
using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC
{
    public class BotanicPiercerJavPROJSPLIT : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";

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
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Projectile.ai[0]++;

            // 在前6帧内直线飞行
            if (Projectile.ai[0] <= 15)
            {
                return;
            }

            // 6帧后开始追踪最近的敌人
            NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 追踪速度为12f
            }

            // 每5帧生成一个翠绿色的尖锥粒子
            if (Projectile.ai[0] % 5 == 0)
            {
                PointParticle spark = new PointParticle(Projectile.Center, -Projectile.velocity * 0.5f, false, 5, 1.1f, Color.LimeGreen);
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 在击中敌人时生成一个由3个顶点组成的翠绿色阵法特效

            // 定义三角形顶点数
            int trianglePoints = 3;

            // 在三角形的每个顶点上生成粒子
            for (int i = 0; i < trianglePoints; i++)
            {
                // 计算每个顶点的角度（360度被3等分，每个顶点相隔120度）
                float angle = MathHelper.TwoPi * i / trianglePoints;

                // 生成多个粒子，以形成逐渐扩散的视觉效果
                for (int j = 0; j < 12; j++)
                {
                    // 粒子的速度从1到7逐渐增加
                    float speed = MathHelper.Lerp(1f, 7f, j / 12f);

                    // 粒子的颜色从白色到翠绿色逐渐变化
                    Color particleColor = Color.Lerp(Color.White, Color.LimeGreen, j / 12f);

                    // 粒子的缩放比例从1.6逐渐减小到0.85
                    float scale = MathHelper.Lerp(1.6f, 0.85f, j / 12f);

                    // 创建粒子，使用Dust类型107
                    Dust magicDust = Dust.NewDustPerfect(Projectile.Center, 107);

                    // 粒子的方向根据当前顶点的角度旋转，乘以粒子速度
                    magicDust.velocity = angle.ToRotationVector2() * speed;

                    // 设置粒子的颜色和缩放比例
                    magicDust.color = particleColor;
                    magicDust.scale = scale;

                    // 设置粒子不受重力影响
                    magicDust.noGravity = true;
                }
            }

            // 播放击中敌人时的视觉效果
            int ovalPoints = 42; // 环形粒子的数量
            for (int i = 0; i < ovalPoints; i++)
            {
                // 计算每个粒子的发射角度
                float angle = MathHelper.TwoPi * i / ovalPoints;

                // 创建翠绿色的环形粒子
                Dust ringDust = Dust.NewDustPerfect(Projectile.Center, 107);

                // 粒子的速度为6，沿着角度方向发射
                ringDust.velocity = angle.ToRotationVector2() * 6f;

                // 粒子的缩放比例为1.1
                ringDust.scale = 1.1f;

                // 设置粒子不受重力影响
                ringDust.noGravity = true;
            }
        }



    }
}
