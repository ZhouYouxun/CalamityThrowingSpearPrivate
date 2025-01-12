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
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.FestiveHalberd
{
    public class FestiveHalberdPROJ : ModProjectile, ILocalizedModType
    {
        //public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/FestiveHalberd/FestiveHalberd";

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
            Projectile.timeLeft = 120;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 将光源颜色改为浅红色，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.LightCoral.ToVector3() * 0.55f);

            // 受到重力影响，但是会逐渐对抗重力
            //Projectile.velocity.Y -= 0.1f;


            // 为箭矢本体后面添加光束特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(255, 69, 0); // 将颜色改为橙红色
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 每隔20帧生成2个 OrnamentFriendly 弹幕
            if (Projectile.timeLeft % 24 == 0)
            {
                // 左侧弹幕
                Vector2 leftDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-90)); // 相对FestiveHalberdPROJ（335号）的左侧
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, leftDirection * 0.95f, 335, (int)(Projectile.damage * 0.625f), Projectile.knockBack, Projectile.owner);

                // 右侧弹幕
                Vector2 rightDirection = Projectile.velocity.RotatedBy(MathHelper.ToRadians(90)); // 相对FestiveHalberdPROJ的右侧
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rightDirection * 0.95f, 335, (int)(Projectile.damage * 0.625f), Projectile.knockBack, Projectile.owner);

                // 生成烟雾特效
                int Dusts = 5;
                float radians = MathHelper.TwoPi / Dusts;
                Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
                for (int i = 0; i < Dusts; i++)
                {
                    Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                    Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.LightYellow, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                    GeneralParticleHandler.SpawnParticle(smoke);
                }

            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 在命中时生成8个橙红色的椭圆形粒子特效，向8个方向扩散
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8 * i; // 每个粒子的角度
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 14f; // 最后这个数决定了扩散的范围

                Particle pulse = new DirectionalPulseRing(Projectile.Center, velocity, Color.OrangeRed, new Vector2(1f, 2.5f), Projectile.rotation - MathHelper.PiOver4, 0.2f, 0.03f, 20);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }


    }
}