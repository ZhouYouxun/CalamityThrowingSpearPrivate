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
using CalamityMod.Projectiles.Melee;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.SausageMakerC
{
    public class SausageMakerJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/SausageMakerC/SausageMakerJav";

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
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
            Projectile.penetrate = 6; // 设置为7次穿透
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 血液红色粒子特效
            if (Main.rand.NextBool(5))
            {
                Vector2 trailPos = Projectile.Center;
                float trailScale = Main.rand.NextFloat(0.8f, 1.2f);
                Color trailColor = Color.Red;
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
            // 给敌人添加燃血效果，持续300帧
            target.AddBuff(ModContent.BuffType<BurningBlood>(), 300);

            // 生成血红色的粒子特效
            for (int i = 0; i < 15; i++)
            {
                // 随机角度偏移，使粒子从弹幕后方随机向左或右发射
                float randomAngle = MathHelper.ToRadians(Main.rand.Next(-30, 30)); // 随机角度范围：-30度到30度
                Vector2 offsetDirection = Projectile.velocity.RotatedBy(randomAngle) * -0.5f; // 反向方向偏移一些，使粒子从后方飞出

                // 定义粒子
                Dust bloodDust = Dust.NewDustPerfect(Projectile.position, DustID.Blood, offsetDirection, 0, Color.Red, 1.5f);
                bloodDust.noGravity = true; // 粒子不受重力影响
                bloodDust.velocity *= Main.rand.NextFloat(1f, 2f); // 随机粒子的速度
            }
        }


        public override void OnKill(int timeLeft)
        {
            // 发射Blood2 弹幕
            for (int i = 0; i < Main.rand.Next(3, 7); i++)
            {
                //Vector2 randomVelocity = Main.rand.NextVector2Circular(1f, 1f) * Projectile.velocity.Length() * 3f;
                Vector2 randomVelocity = Main.rand.NextVector2Circular(1f, 1f) * 5f; // 将速度统一为5
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, randomVelocity,
                    ModContent.ProjectileType<Blood2>(), (int)(Projectile.damage * 0.75), Projectile.knockBack, Projectile.owner);
            }

        }



    }
}
