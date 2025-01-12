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
    public class ElementalLanceJavPROJVortex : ModProjectile, ILocalizedModType
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
            // 使用墨绿色绘制拖尾效果
            Color trailColor = new Color(0, 128, 128); // 墨绿色
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
            Lighting.AddLight(Projectile.Center, Color.DarkGreen.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.00f;

            // 为箭矢本体后面添加卡其色光束特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = new Color(0, 128, 128); // RGB: (0, 128, 128)
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 300); // 元素混合

            // 1. 获取生成位置和目标位置
            Vector2 spawnPosition = Main.player[Projectile.owner].Center; // 从玩家位置生成
            Vector2 projectileToKillPosition = Projectile.Center; // 获取主弹幕消失的位置

            // 2. 计算方向向量
            Vector2 forwardDirection = (projectileToKillPosition - spawnPosition).SafeNormalize(Vector2.Zero);

            // 3. 定义角度偏移
            float[] angles = { MathHelper.ToRadians(60), MathHelper.ToRadians(-60), MathHelper.ToRadians(120), MathHelper.ToRadians(-120) };

            // 4. 生成4个弹幕
            foreach (float angle in angles)
            {
                // 计算旋转后的方向
                Vector2 rotatedDirection = forwardDirection.RotatedBy(angle);

                // 生成弹幕
                int newProjectile = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPosition,
                    rotatedDirection * 10f, // 保持原速度
                    ProjectileID.VortexBeaterRocket,
                    (int)(Projectile.damage * 0.33f), // 伤害降低为主弹幕的0.33倍
                    0f,
                    Projectile.owner
                );

                // 获取生成的投射物实例并修改属性
                Projectile newProj = Main.projectile[newProjectile];
                if (newProj != null && newProj.active)
                {
                    newProj.scale = 1f; // 保持大小为1.0
                    newProj.DamageType = DamageClass.Melee; // 设置为近战职业类型
                    newProj.usesLocalNPCImmunity = true; // 使用本地无敌帧
                    newProj.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
                }
            }





            // 2. 在弹幕消失位置的正前方生成深绿色线性粒子特效
            for (int i = 0; i < 10; i++)
            {
                // 计算粒子的生成方向，基于弹幕的当前朝向
                Vector2 particleSpawnDirection = Projectile.velocity.SafeNormalize(Vector2.Zero); // 获取弹幕的正前方向
                                                                                                  // 生成两个方向偏移的粒子，一个偏移 10 度，一个偏移 -10 度
                Vector2 velocityLeft = particleSpawnDirection.RotatedBy(MathHelper.ToRadians(10)) * Main.rand.NextFloat(5f, 10f); // 向左偏移10度
                Vector2 velocityRight = particleSpawnDirection.RotatedBy(MathHelper.ToRadians(-10)) * Main.rand.NextFloat(5f, 10f); // 向右偏移10度
                                                                                                                                    // 创建深绿色（墨绿色）粒子，偏移10度
                Particle trailLeft = new SparkParticle(Projectile.Center + particleSpawnDirection * 10f, velocityLeft, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.DarkGreen);
                Particle trailRight = new SparkParticle(Projectile.Center + particleSpawnDirection * 10f, velocityRight, false, 60, Main.rand.NextFloat(0.8f, 1.2f), Color.DarkGreen);
                // 生成粒子
                GeneralParticleHandler.SpawnParticle(trailLeft);
                GeneralParticleHandler.SpawnParticle(trailRight);
            }
        }








        public override void OnKill(int timeLeft)
        {
        
        }


    }
}
