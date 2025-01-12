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
using CalamityMod.Projectiles.Melee;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC
{
    public class StarnightLanceJavPROJ : ModProjectile, ILocalizedModType
    {

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/BPrePlantera/StarnightLanceC/StarnightLanceJav";

        private bool hasBounced = false; // 记录是否已经反弹过一次
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
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加浅粉色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * 0.55f);

            // 第 20 帧以后才开始追踪
            if (Projectile.ai[0] >= 20)
            {
                // 弱追踪逻辑
                CalamityUtils.HomeInOnNPC(Projectile, true, 450f, 15f, 50f);
            }

            // 每5帧生成浅粉色原版粒子特效
            if (Projectile.ai[0] % 5 == 0)
            {
                for (int i = 0; i <= 5; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, Projectile.velocity);
                    dust.scale = Main.rand.NextFloat(1.6f, 2.5f);
                    dust.velocity = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.3f, 1.6f);
                    dust.noGravity = true;
                    dust.color = Color.Pink; // 浅粉色
                }
            }

            // 每10帧生成浅粉色椭圆形专有粒子特效
            if (Projectile.ai[0] % 10 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Projectile.velocity * 0.75f, Color.Pink, new Vector2(1f, 2.5f), Projectile.rotation - MathHelper.PiOver4, 0.2f, 0.03f, 20);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }


            Projectile.ai[0]++;
        }


        public override void OnKill(int timeLeft)
        {
            // 释放三个 StarnightBeam 弹幕，夹角为 120 度
            for (int i = 0; i < 3; i++)
            {
                Vector2 shootDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(120f * i));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootDirection, ModContent.ProjectileType<StarnightLanceJavBeam>(), (int)(Projectile.damage * 0.2f), Projectile.knockBack, Projectile.owner);
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 300); // 原版的冻伤效果
        }

    }
}
