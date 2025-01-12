using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK
{
    public class SHPCKFast : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/SHPCK/SHPCK";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";

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
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            //Projectile.scale = 0.7f; // 大小小一点
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity *= 1.25f;
        }
        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深蓝色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.01f;

            //// 变色的线性粒子特效
            //if (Main.rand.NextBool(12))
            //{
            //    // 随机生成左右偏移量
            //    float sideOffset = Main.rand.NextFloat(-0f, 0f); // 随机左右偏移的距离

            //    // 计算 trailPos，添加左右偏移
            //    Vector2 trailPos = Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * sideOffset;

            //    float trailScale = Main.rand.NextFloat(0.8f, 1.2f);

            //    // 使用类似SHPL的变色逻辑，使用Main.DiscoG来动态改变绿色通道
            //    Color trailColor = new Color(255, Main.DiscoG, 155);

            //    // 创建粒子
            //    Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 40, trailScale, trailColor);
            //    GeneralParticleHandler.SpawnParticle(trail);
            //}



        }



        public override void OnKill(int timeLeft)
        {
            int dustAmt = Main.rand.Next(3, 7);
            for (int d = 0; d < dustAmt; d++)
            {
                int dustType = Utils.SelectRandom(Main.rand, new int[]
                {
            246, // Dust类型，类似SHPL的粒子
            73,
            187
                });
                int idx = Dust.NewDust(Projectile.Center - Projectile.velocity / 2f, 0, 0, dustType, 0f, 0f, 100, default, 2.1f);
                Main.dust[idx].velocity *= 2f;
                Main.dust[idx].noGravity = true;
            }
        }


        //public override bool OnTileCollide(Vector2 oldVelocity)
        //{
        //    // 反弹效果
        //    if (Projectile.velocity.X != oldVelocity.X)
        //    {
        //        Projectile.velocity.X = -oldVelocity.X;
        //    }
        //    if (Projectile.velocity.Y != oldVelocity.Y)
        //    {
        //        Projectile.velocity.Y = -oldVelocity.Y;
        //    }
        //    return false;
        //}

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300); // 原版的带电效果
            //target.AddBuff(ModContent.BuffType<GalvanicCorrosion> (), 300); // 电偶腐蚀
        }
    }
}
