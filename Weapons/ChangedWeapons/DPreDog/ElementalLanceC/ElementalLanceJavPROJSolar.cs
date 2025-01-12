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
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJavPROJSolar : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/DPreDog/ElementalLanceC/ElementalLanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/StarProj").Value;
            // 使用橙红色绘制拖尾效果
            Color trailColor = new Color(255, 69, 0); // 橙红色
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], trailColor * 0.3f, 1);
            return false;
        }



        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 8; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.02f;


            // 为箭矢本体后面添加卡其色光束特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(255, 69, 0); // RGB: (255, 69, 0)
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300); // 元素混合

            // 在原地生成一个FuckYou弹幕，伤害倍率为1.0，大小倍率为3.0
            int projectileID = ModContent.ProjectileType<FuckYou>();

            // 生成弹幕，大小倍率为3.0
            Projectile fuckYouProjectile = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, projectileID, Projectile.damage, Projectile.knockBack, Projectile.owner);

            // 调整弹幕的大小倍率为3.0
            fuckYouProjectile.scale *= 3.0f;



            // 在原地生成随机橙红色粒子特效
            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)); // 随机方向
                Color particleColor = new Color(255, 69, 0); // 橙红色
                float scale = Main.rand.NextFloat(0.5f, 1.5f); // 随机缩放粒子大小
                SparkParticle particle = new SparkParticle(Projectile.Center, velocity, false, 7, scale, particleColor);
                GeneralParticleHandler.SpawnParticle(particle);
            }

        }
        public override void OnKill(int timeLeft)
        {
            //// 计算等边三角形三个顶点相对于中心的位置偏移
            //float triangleSize = 60f; // 三角形的边长，可以根据需要调整
            //Vector2 offset1 = new Vector2(0, -triangleSize / (float)Math.Sqrt(3)); // 顶点1
            //Vector2 offset2 = new Vector2(-triangleSize / 2, triangleSize / (2 * (float)Math.Sqrt(3))); // 顶点2
            //Vector2 offset3 = new Vector2(triangleSize / 2, triangleSize / (2 * (float)Math.Sqrt(3))); // 顶点3

            //// 生成三个爆炸弹幕，大小为当前的3.5倍
            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + offset1, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: 0, ai1: 0);
            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + offset2, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: 0, ai1: 0);
            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + offset3, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: 0, ai1: 0);


        }


    }
}
