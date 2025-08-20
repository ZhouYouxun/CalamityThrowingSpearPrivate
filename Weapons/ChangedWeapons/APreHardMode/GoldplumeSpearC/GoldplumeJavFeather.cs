using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC
{
    public class GoldplumeJavFeather : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
        public override string Texture => "CalamityMod/Projectiles/Magic/TradewindsProjectile";


        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        private bool isTracking = false;

        public override void OnSpawn(IEntitySource source)
        {
            // 根据 ai[0] 决定逻辑
            if (Projectile.ai[0] == 1f)
                isTracking = true;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            {

                if (!isTracking)
                {
                    // === 普通逻辑 ===
                    if (Time % 2 == 0)
                    {
                        int dustType = Main.rand.Next(new int[] { DustID.YellowTorch, 57, DustID.Gold });
                        Dust dust = Dust.NewDustPerfect(
                            Projectile.Center + Main.rand.NextVector2Circular(1f, 1f),
                            dustType, null, 100, default,
                            Main.rand.NextFloat(1.25f, 1.5f));
                        dust.velocity = Projectile.velocity * 0.2f;
                        dust.noGravity = true;
                    }
                }
                else
                {
                    // === 追踪逻辑（延迟20帧启动）===
                    if (Time > 20)
                    {
                        NPC target = Projectile.Center.ClosestNPCAt(800f);
                        if (target != null)
                        {
                            Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.15f);
                        }
                    }

                    if (Main.rand.NextBool(3))
                    {
                        Dust.NewDustPerfect(
                            Projectile.Center,
                            DustID.GoldFlame,
                            Projectile.velocity * 0.1f,
                            100,
                            Color.Gold,
                            1.6f
                        ).noGravity = true;
                    }
                }



            }



            // 添加路径粒子特效，每隔两帧释放一次
            if (Projectile.ai[1] % 2 == 0) // 仅在帧计数为偶数时生成粒子
            {
                int dustType = Main.rand.Next(new int[] { DustID.YellowTorch, 57, DustID.Gold }); // 随机选择粒子类型
                Vector2 dustPosition = Projectile.Center + Main.rand.NextVector2Circular(1f, 1f); // 粒子生成位置稍有随机偏移
                Dust dust = Dust.NewDustPerfect(
                    dustPosition,             // 粒子生成位置
                    dustType,                 // 粒子类型
                    null,                     // 初始速度为空
                    100,                      // 不透明度
                    default,                  // 粒子颜色
                    Main.rand.NextFloat(1.25f, 1.5f) // 粒子大小在1.25~1.5之间随机
                );
                dust.velocity = Projectile.velocity * 0.2f; // 粒子速度与弹幕速度相关联
                dust.noGravity = true; // 禁用重力，确保粒子漂浮
            }

            Time++;
        }
        public ref float Time => ref Projectile.ai[1];
        public override bool? CanDamage() => Time >= 5f; // 初始的时候不会造成伤害，直到x为止
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 150 - 5)
                return false;

            return true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, Projectile.oldVelocity.X, Projectile.oldVelocity.Y, 100, default, 1.4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }
        }
    }
}
