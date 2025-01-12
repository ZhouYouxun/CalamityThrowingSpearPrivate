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
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJavPROJStardust : ModProjectile, ILocalizedModType
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
            // 使用蓝色绘制拖尾效果
            Color trailColor = new Color(0, 0, 139); // 蓝色
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], trailColor * 0.3f, 1);
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
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 为箭矢本体后面添加卡其色光束特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(0, 0, 139); // RGB: (0, 0, 139)
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300); // 元素混合

            // 在敌人为中心，半径20个方块（20 * 16 = 320像素）的圆上生成三个弹幕
            float radius = 320f; // 半径20个方块（像素）
            for (int i = 0; i < 3; i++) // 修改为生成3个弹幕
            {
                // 随机选择圆上的一点
                Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(radius, radius);

                // 使用模组中的ELPStardust弹幕
                int projectileID = ModContent.ProjectileType<ELPStardust>();

                // 计算朝向敌人的速度向量，速度为10
                Vector2 velocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero) * 10f;

                // 生成弹幕，伤害为原伤害的75%
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, projectileID, (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);

                // 在生成弹幕的位置创建蓝色粒子特效，粒子会逐渐向外扩散，缩放为3.2f
                for (int j = 0; j < 4; j++) // 生成4个粒子以形成一个方形
                {
                    Vector2 particlePosition = spawnPosition + Main.rand.NextVector2Circular(4f, 4f); // 生成点附近随机偏移
                    SparkParticle squareParticle = new SparkParticle(particlePosition, Vector2.Zero, false, 5, 3.2f, Color.Blue); // 粒子大小为3.2f
                    GeneralParticleHandler.SpawnParticle(squareParticle);
                }
            }


            // 圆圈型粒子特效，颜色为LightSkyBlue，粒子向外扩散
            for (int i = 0; i < 3; i++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 3); // 生成3个粒子，均匀分布在圆周上
                Particle pulse = new DirectionalPulseRing(Projectile.Center, velocity, Color.LightSkyBlue, new Vector2(1f, 2.5f), Projectile.rotation - MathHelper.PiOver4, 0.2f, 0.03f, 20);
                GeneralParticleHandler.SpawnParticle(pulse);
            }

        }
        public override void OnKill(int timeLeft)
        {
          
        }
    }
}
