using CalamityMod;
using CalamityThrowingSpear.Global;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.GoldplumeSpearC
{
    internal class GoldplumeJavWind : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            // 设置拖尾效果和长度
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        // 修改为灰白色的残影效果
        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.LightGray, 2);
            return false;
        }
        public override void SetDefaults()
        {
            // 基础属性设置
            Projectile.width = Projectile.height = 50;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 300; // 弹幕存活时间
            Projectile.extraUpdates = 1;
            Projectile.penetrate = -1; // 可以击中两个敌人
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.tileCollide = false; // 不与地形碰撞
        }

        public override void AI()
        {
            // 控制弹幕旋转和透明度
            Projectile.rotation += 0.5f; // 持续旋转
            Projectile.alpha -= 5; // 渐渐显示弹幕
            Projectile.timeLeft = 300; // 不断地重置自己的寿命
            // 获取鼠标位置
            Vector2 mousePosition = Main.MouseWorld;

            // 强追踪鼠标位置
            Vector2 direction = (mousePosition - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 20f, 0.1f); // 平滑追踪鼠标位置


            // 螺旋式云朵粒子效果
            float spiralSpeed = 0.1f; // 螺旋旋转速度
            float spiralRadius = 30f; // 螺旋半径
            int dustAmount = 2; // 每帧生成的粒子数量

            for (int i = 0; i < dustAmount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustAmount + Projectile.ai[1] * spiralSpeed;
                Vector2 offset = angle.ToRotationVector2() * spiralRadius;

                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.Cloud);
                dust.scale = Main.rand.NextFloat(1.25f, 1.75f); // 随机大小
                dust.velocity = offset.SafeNormalize(Vector2.Zero) * 2f; // 向外扩散
                dust.noGravity = true; // 禁用重力
            }

            // 增加AI计数器
            Projectile.ai[1]++;
        }

   




        public override void OnKill(int timeLeft)
        {


            
        }
    }
}