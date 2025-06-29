using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC
{
    public class GoldplumeJavFeather : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
        public override string Texture => "CalamityMod/Projectiles/Magic/TradewindsProjectile";

        private const int TimeLeft = 150;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = TimeLeft;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

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
            if (Projectile.timeLeft > TimeLeft - 5)
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
