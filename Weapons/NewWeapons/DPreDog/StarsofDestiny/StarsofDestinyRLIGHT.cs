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

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestinyRLIGHT : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_4923";
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
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 200;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.scale = 1.5f;
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;


            //if (Projectile.numUpdates % 3 == 0)
            //{
            //    // 神圣粉红色火花
            //    Color outerSparkColor = Color.Lerp(Color.Pink, Color.White, 0.5f); // 粉红到白色渐变
            //    float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
            //    float outerSparkScale = 1.25f + scaleBoost; // 放大倍率略微增加
            //    SparkParticle spark = new SparkParticle(
            //        Projectile.Center,
            //        Projectile.velocity * Main.rand.NextFloat(0.5f, 1f), // 随机降低速度
            //        false,
            //        7,
            //        outerSparkScale,
            //        outerSparkColor
            //    );
            //    GeneralParticleHandler.SpawnParticle(spark);
            //}


            // 在飞行路径上生成双点转圈的粒子特效
            if (Main.rand.NextBool(1)) // 控制粒子生成频率（1/X 概率）
            {
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(0, 360)); // 随机初始角度
                for (int i = 0; i < 2; i++) // 双点对称生成
                {
                    float angle = angleOffset + MathHelper.Pi * i; // 相对角度
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(8f, 12f); // 偏移位置

                    Vector2 spawnPosition = Projectile.Center + offset;
                    Vector2 velocity = offset * 0.05f; // 非常缓慢的初始速度

                    int dustType = Main.rand.Next(new int[] { DustID.EnchantedNightcrawler, DustID.PinkTorch, DustID.CrystalPulse2 });
                    Dust dust = Dust.NewDustPerfect(spawnPosition, dustType, velocity, 150, Color.Pink, Main.rand.NextFloat(1.25f, 1.75f)); // 大小随机化
                    dust.noGravity = true; // 无重力效果
                    dust.fadeIn = 1.5f; // 渐入效果
                }
            }



            Time++;
        }
        public ref float Time => ref Projectile.ai[1];

        public override bool? CanDamage() => Time >= 5f; // 初始的时候不会造成伤害，直到x为止



    }
}