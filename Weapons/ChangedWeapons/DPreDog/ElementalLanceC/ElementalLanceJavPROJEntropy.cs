using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Ranged;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJavPROJEntropy : ModProjectile, ILocalizedModType
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
            // 使用纯黑色绘制拖尾效果
            Color trailColor = new Color(0, 0, 0); // 纯黑色
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
            Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 为箭矢本体后面添加光束特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(0, 0, 0); // 黑色
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300); // 元素混合

            // 黑色交叉特效（来自死阳）
            float numberOflines = 5;
            float rotFactorlines = 360f / numberOflines;
            for (int i = 0; i < numberOflines; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactorlines);
                Vector2 offset = new Vector2(Main.rand.NextFloat(1, 3.1f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                Vector2 velOffset = new Vector2(Main.rand.NextFloat(1, 3.1f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                AltSparkParticle spark = new AltSparkParticle(Projectile.Center + offset, velOffset, false, 20, Main.rand.NextFloat(1.9f, 2.3f), Color.Black);
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // 死阳同款的黑色滤镜
            {
                // 设置生成粒子的数量
                int particleCount = 10; // 默认数量为10，可以根据需求调整

                // 以弹幕中心为起点，生成粒子
                for (int i = 0; i < particleCount; i++)
                {
                    // 生成一个随机角度，范围在0到360度
                    float randomAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                    // 根据随机角度计算粒子生成的方向向量
                    Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));
                    // 调整粒子的速度，可以根据需求手动设置速度范围
                    Vector2 particleVelocity = direction * Main.rand.NextFloat(2f, 6f); // 速度范围从2到6

                    // 生成粒子，使用你指定的`GenericBloom`类型
                    Particle orb = new GenericBloom(
                        Projectile.Center + Main.rand.NextVector2Circular(10, 10), // 粒子的生成位置，稍微偏移弹幕中心
                        particleVelocity, // 粒子的速度向量
                        Color.Black, // 粒子的颜色，这里为纯黑色
                        Main.rand.NextFloat(0.2f, 0.45f), // 粒子的缩放比例，可以根据需求调整
                        Main.rand.Next(9, 12), // 粒子的生命周期范围
                        true, // 粒子的某种效果标记，保持为`true`，具体效果可自行控制
                        false // 粒子的另一个效果标记，保持为`false`
                    );

                    // 使用粒子处理器生成粒子
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }

            {
                // 在击中敌人时生成一个新的 ELPEntropyEXP 弹幕
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), // 获取弹幕来源
                    Projectile.Center, // 弹幕生成位置，设为当前弹幕的位置
                    Vector2.Zero, // 速度设为0
                    ModContent.ProjectileType<ELPEntropyEXP>(), // 生成的弹幕类型
                    (int)(Projectile.damage * 1.0f), // 伤害倍率为1.0倍
                    Projectile.knockBack, // 传递当前弹幕的击退值
                    Projectile.owner // 指定弹幕的拥有者
                );
                // 死阳同款的音效
                SoundEngine.PlaySound(DeadSunsWind.Ricochet, Projectile.Center);
            }

            // 释放爆炸冲击波特效
            Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, (int)(Viscera.BoomLifetime * 0.38f), false);
            GeneralParticleHandler.SpawnParticle(bloodsplosion);
        }

        public override void OnKill(int timeLeft)
        {

        }


    }
}
