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
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.AmidiasTridentC
{
    public class AmidiasTridentJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/AmidiasTridentC/AmidiasTridentJav";
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
            Projectile.penetrate = -1; // 允许无限制伤害
            Projectile.timeLeft = 30;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 不穿透物块
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加海蓝色光源
            Lighting.AddLight(Projectile.Center, Color.DarkBlue.ToVector3() * 0.55f);

            // 释放海蓝色的粒子特效
            if (Main.rand.NextBool(5))
            {
                Dust dust = Main.dust[Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Water, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f)];
                dust.noGravity = true;
                dust.scale = 1.2f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 生成四个夹角为90度的AmidiasTridentJavWhirlpool弹幕
            for (int i = 0; i < 4; i++)
            {
                // 以初始角度进行不同的偏移，确保弹幕之间有角度差异
                float angle = MathHelper.ToRadians(90) * i;  // 90度的角度差
                Vector2 direction = angle.ToRotationVector2();  // 根据角度计算方向向量
                Vector2 whirlpoolVelocity = direction * Projectile.velocity.Length();  // 基于原始速度的长度生成不同方向的速度

                // 生成带有不同方向的弹幕
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, whirlpoolVelocity,
                    ModContent.ProjectileType<AmidiasTridentJavWhirlpool>(), (int)((Projectile.damage)*1.5), Projectile.knockBack, Projectile.owner);
            }


            // 释放随机的线性海蓝色粒子特效
            int points = 25;
            float radians = MathHelper.TwoPi / points;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            float rotRando = Main.rand.NextFloat(0.1f, 2.5f);
            for (int k = 0; k < points; k++)
            {
                Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f * rotRando);
                LineParticle subTrail = new LineParticle(Projectile.Center + velocity * 20.5f, velocity * 15, false, 30, 0.75f, Color.CadetBlue);
                GeneralParticleHandler.SpawnParticle(subTrail);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 180); // 激流
        }

    }
}
