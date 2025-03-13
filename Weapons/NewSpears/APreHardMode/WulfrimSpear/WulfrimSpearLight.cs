using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Global;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.WulfrimSpear
{
    internal class WulfrimSpearLight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectile.APreHardMode";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 画残影效果
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
        public override void SetDefaults()
        {
            // 设置弹幕的基础属性
            Projectile.width = 11; // 弹幕宽度
            Projectile.height = 24; // 弹幕高度
            Projectile.friendly = true; // 对敌人有效
            Projectile.DamageType = DamageClass.Melee; // 远程伤害类型
            Projectile.penetrate = 5; // 穿透力为1，击中一个敌人就消失
            Projectile.timeLeft = 300; // 弹幕存在时间为600帧
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.ignoreWater = true; // 弹幕不受水影响
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }

        public override void AI()
        {
            // 调整弹幕的旋转，使其在飞行时保持水平
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;

            // Lighting - 添加天蓝色光源，光照强度为 0.49
            Lighting.AddLight(Projectile.Center, Color.LightSkyBlue.ToVector3() * 0.49f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 追踪效果（比原版更慢）
            CalamityUtils.HomeInOnNPC(Projectile, true, 2000f, 12f, 250f); // homingVelocity 从 18 降到 12，inertia 从 200 降到 250

            // 飞行轨迹上留下绿色粒子（类似 SparkParticle）
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(144, 238, 144); // 亮绿色 (LightGreen)
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 在飞行路径上留下绿色 Dust 特效
            if (Main.rand.NextBool(4)) // 25% 概率
            {
                Dust greenDust = Dust.NewDustPerfect(Projectile.Center, 267, Projectile.velocity * 0.5f, 150, Color.LimeGreen, 1.2f);
                greenDust.noGravity = true;
                greenDust.velocity *= 0.3f;
                greenDust.fadeIn = 1.5f;
            }

            // 生成 `WulfrumDroidEmote` 粒子，让它往上漂浮
            if (Main.rand.NextBool(8)) // 12.5% 概率
            {
                Vector2 emoteDirection = new Vector2(0, -1).RotatedByRandom(MathHelper.ToRadians(20)); // 往上漂浮
                Particle emote = new WulfrumDroidEmote(
                    Projectile.Center + emoteDirection * 10f,
                    emoteDirection * Main.rand.NextFloat(1.5f, 3f), // 速度比原版小
                    Main.rand.Next(40, 80),
                    Main.rand.NextFloat(1.2f, 1.8f)
                );
                GeneralParticleHandler.SpawnParticle(emote);
            }
        }
    }
}