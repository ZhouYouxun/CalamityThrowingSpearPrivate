using CalamityMod.Particles;
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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    internal class SurfeiterPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/Surfeiter";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
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
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 将光源颜色改为浅红色，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.LightCoral.ToVector3() * 0.55f);


            // 释放随机粒子特效
            for (int i = 0; i < 2; i++)
            {
                int dustType = Main.rand.Next(new int[] { DustID.Blood, DustID.IceTorch, DustID.Granite });
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(2 * 16, 2 * 16);
                Dust dust = Dust.NewDustPerfect(dustPos, dustType);
                dust.scale = Main.rand.NextFloat(0.75f, 1.45f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {

            // 释放重型烟雾
            for (int i = 0; i < 40; i++)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, Main.rand.NextVector2Circular(3 * 16, 3 * 16), Color.Black, 15, 0.9f, 0.5f, 0.2f, true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            for (int i = 0; i < 75; i++)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, Main.rand.NextVector2Circular(5 * 16, 5 * 16), Color.Black, 15, 0.9f, 0.5f, 0.2f, true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 释放“土”字形粒子
            Vector2 basePos = Projectile.Center;
            for (int i = -3; i <= 3; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    Vector2 dustPos = basePos + new Vector2(i * 6, j * 6);
                    int dustType = Main.rand.Next(new int[] { DustID.Blood, DustID.IceTorch, DustID.Granite });
                    Dust dust = Dust.NewDustPerfect(dustPos, dustType);
                    dust.scale = 1f;
                    dust.noGravity = true;
                }
            }

            // 生成左右两侧的 SurfeiterStonePillars
            int leftPillar = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - new Vector2(30 * 16, 0), Vector2.Zero, ModContent.ProjectileType<SurfeiterStonePillars>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            int rightPillar = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(30 * 16, 0), Vector2.Zero, ModContent.ProjectileType<SurfeiterStonePillars>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

            // 传递信息给 SurfeiterStonePillars
            if (Main.projectile[leftPillar].ModProjectile is SurfeiterStonePillars left)
                left.SetDirection(1);
            if (Main.projectile[rightPillar].ModProjectile is SurfeiterStonePillars right)
                right.SetDirection(-1);

            string soundToPlay = Main.rand.NextBool()
    ? "CalamityMod/Sounds/Custom/Ravager/RavagerStomp1"
    : "CalamityMod/Sounds/Custom/Ravager/RavagerStomp2";

            SoundEngine.PlaySound(new SoundStyle(soundToPlay), Projectile.Center);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }


    }
}