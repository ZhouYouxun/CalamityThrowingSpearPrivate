using CalamityMod.Buffs.StatDebuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.EarthenC
{
    public class EarthenJavSHARD : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";

        public override void SetDefaults()
        {
            base.Projectile.width = 10;
            base.Projectile.height = 10;
            base.Projectile.friendly = true;
            base.Projectile.DamageType = DamageClass.Melee;
            base.Projectile.penetrate = 1;
            base.Projectile.aiStyle = 1;
            base.Projectile.timeLeft = 250;
            base.Projectile.tileCollide = false;
            AIType = ProjectileID.WoodenArrowFriendly;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.Y;


            //// 灵感来自于shellshocklive里面的武器："卫星"
            //{
            //    //// 这个是线性反转，也就是折线
            //    //gravityTimer++;
            //    //if (gravityTimer >= gravityDuration)
            //    //{
            //    //    gravityTimer = 0;

            //    //    // 每次周期结束，反转重力方向
            //    //    gravityDirection *= -1f;

            //    //    // 生成新的周期长度（每次不同）
            //    //    gravityDuration = Main.rand.Next(20, 46); // 周期 20~45 帧
            //    //}

            //    //// 垂直速度线性增加（每帧都加）
            //    //verticalSpeed += speedGainPerFrame;

            //    //// 应用速度变化
            //    //Projectile.velocity.Y = gravityDirection * verticalSpeed;

            //    //// 保持水平速度不变（这句不能省，否则你之前的 *= 会不断衰减）
            //    //Projectile.velocity.X = Projectile.velocity.X;



            //    // 这个是指数反转，也就是每次都曲线
            //    // 每gravityDuration帧反转一次重力方向
            //    gravityTimer++;
            //    if (gravityTimer >= gravityDuration)
            //    {
            //        gravityTimer = 0;
            //        gravityDirection *= -1f;
            //        gravityDuration = Main.rand.Next(20, 46); // 每段时长变动
            //    }

            //    // 垂直加速度累加（模拟抛物线效果）
            //    Projectile.velocity.Y += gravityDirection * gravityAccel;

            //    // 水平速度维持不变
            //    // （可略做扰动让轨迹更“生动”）
            //    Projectile.velocity.X = Projectile.velocity.X;


            //}


            if (Projectile.velocity.Y <= 0f)
                Projectile.velocity.Y = 0.1f;
            Projectile.rotation += Projectile.velocity.Y;
            Projectile.velocity.Y *= 1.05f;


            //if (Main.rand.NextBool(2)) // 每帧约50%概率生成
            //{
            //    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.Dirt);
            //    d.scale = Main.rand.NextFloat(1.2f, 1.6f);
            //    d.velocity = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
            //    d.noGravity = false;
            //}

            //if (Main.rand.NextBool(4)) // 每4帧左右生成一次
            //{
            //    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Stone);
            //    d.scale = 1.0f;
            //    d.velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * 1.2f;
            //    d.noGravity = true;
            //}

            // 钻地期间的特效
            //if (Projectile.velocity.Y > 4f && Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SandstormInABottle);
            //        d.velocity = Main.rand.NextVector2Circular(2f, 1.5f);
            //        d.scale = 1.3f;
            //        d.noGravity = true;
            //    }
            //}

        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 60);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 2; i++)
            {
                Dust.NewDust(base.Projectile.position + base.Projectile.velocity, base.Projectile.width, base.Projectile.height, 32, base.Projectile.oldVelocity.X * 0.5f, base.Projectile.oldVelocity.Y * 0.5f);
            }

            for (int i = 0; i < 12; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.Dirt : DustID.Stone;
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.scale = Main.rand.NextFloat(1.5f, 2.3f);
                d.noGravity = Main.rand.NextBool();
            }

        }
    }
}