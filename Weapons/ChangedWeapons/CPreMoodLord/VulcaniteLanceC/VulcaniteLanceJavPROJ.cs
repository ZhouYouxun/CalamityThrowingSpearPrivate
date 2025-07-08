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
using CalamityMod.Projectiles.Typeless;
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC
{
    public class VulcaniteLanceJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/VulcaniteLanceC/VulcaniteLanceJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        private bool hasExploded = false; // 用来标记是否已经爆炸
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
            Projectile.penetrate = 3;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 不允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 受重力影响
        }
        public override void OnSpawn(IEntitySource source)
        {
            // 定义范围和发射逻辑
            float baseAngle = Projectile.velocity.ToRotation(); // 当前弹幕的正前方
            float spreadAngle = MathHelper.ToRadians(20); // 总扩散角度为 20 度

            for (int i = 0; i < 3; i++) // 随机选择 3 个方向
            {
                float randomOffset = Main.rand.NextFloat(-spreadAngle / 2, spreadAngle / 2); // 随机角度偏移
                Vector2 direction = baseAngle.ToRotationVector2().RotatedBy(randomOffset); // 计算方向

                float speedMultiplier = Main.rand.NextFloat(0.75f, 1.75f); // 初速度倍率
                Vector2 velocity = direction * (Projectile.velocity.Length() * speedMultiplier); // 计算初始速度

                // 发射 VulcaniteLanceJavTinyFlare 弹幕
                Projectile.NewProjectile(
                    source,
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<VulcaniteLanceJavTinyFlare>(),
                    (int)(Projectile.damage * 0.75f), // 伤害倍率为 0.45
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }
        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 逐渐加速，每帧乘以1.005
            Projectile.velocity *= 1.005f;

            // 添加光照效果
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);


            // 添加烟雾效果，每隔一段时间随机释放烟雾
            if (Main.rand.NextBool(5) && Main.netMode != NetmodeID.Server) // 20%的几率生成烟雾
            {
                // 生成随机的Gore（烟雾）
                int smoke = Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.position, Vector2.Zero, Main.rand.Next(375, 378), 0.75f);
                Main.gore[smoke].velocity = Projectile.velocity * 0.1f; // 控制烟雾的速度
                Main.gore[smoke].behindTiles = true; // 烟雾可以显示在方块后面
            }

            // 每三帧生成浅粉色激光类特效
            if (Projectile.numUpdates % 3 == 0)
            {
                Color outerSparkColor = Color.OrangeRed;
                float scaleBoost = MathHelper.Clamp(Projectile.ai[0] * 0.005f, 0f, 2f);
                float outerSparkScale = 1.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);
            }

        }

        // 击中敌人时的逻辑
        private static int hitCounter = 0; // 追踪命中次数
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);

            // 增加命中计数
            hitCounter++;

            // 计算爆炸弹幕的伤害倍率和大小
            float damageMultiplier = Math.Min(0.5f + 0.25f * (hitCounter - 1), 1f); // 每次提升0.25倍，最大1.0
            float sizeMultiplier = damageMultiplier; // 大小与伤害倍率一致

            // 生成爆炸弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<FuckYou>(),
                (int)(Projectile.damage * 0.125f),
                Projectile.knockBack,
                Projectile.owner
            );

            //// 10% 概率召唤 TinyFlare 弹幕
            //if (Main.rand.NextFloat() < 1f)
            //{
            //    // 攻击后在玩家位置生成TinyFlare弹幕，速度为0.7倍，伤害为0.33倍
            //    Vector2 flareDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
            //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.player[Projectile.owner].Center, flareDirection * 0.7f * 10f,
            //        ModContent.ProjectileType<VulcaniteLanceJavTinyFlare>(), (int)(Projectile.damage * 1.25f), Projectile.knockBack, Projectile.owner);
            //}
        }


        private static int killCounter = 0; // 专用计数器用于触发特效
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);

            // 生成一个伤害倍率为 1.0 的 VulcaniteLanceJavSuperFlame
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<VulcaniteLanceJavSuperFlame>(),
                (int)(Projectile.damage * 0.3),
                Projectile.knockBack,
                Projectile.owner
            );

            // 增加计数器
            killCounter++;

            // 如果计数器达到 20，生成一个伤害倍率为 15 的 VulcaniteLanceJavFlame
            if (killCounter >= 20)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<VulcaniteLanceJavFlame>(),
                    (int)(Projectile.damage * 7.5f),
                    Projectile.knockBack,
                    Projectile.owner
                );

                // 重置计数器
                killCounter = 0;
            }

            // 生成橙红色的椭圆形粒子特效，往8个方向发射
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8 * i;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                // 调整方向向量的大小，将原来的 0.75f 增加到 2f，使粒子飞得更远
                Particle pulse = new DirectionalPulseRing(Projectile.Center, direction * 2f, Color.OrangeRed, new Vector2(1f, 2.5f), 0f, 0.2f, 0.03f, 20);
                GeneralParticleHandler.SpawnParticle(pulse);
            }






            // “火”的位置与结构定义
            Vector2 center = Projectile.Center; // 弹幕死亡的中心点
            float baseSpeed = 2.0f; // 粒子基础初速度
            float acceleration = 0.1f; // 粒子加速度
            int particleLifetime = 60; // 粒子生命周期

            // 使用相对坐标绘制“火”字
            List<Vector2> strokePoints = new List<Vector2>
    {
        // 一撇
        new Vector2(-10, -20),
        new Vector2(-15, -10),
        new Vector2(-20, 0),

        // 一捺
        new Vector2(10, -20),
        new Vector2(15, -10),
        new Vector2(20, 0),

        // 两点
        new Vector2(-5, 10),
        new Vector2(5, 10)
    };

            // 遍历每个点生成粒子
            foreach (var point in strokePoints)
            {
                for (int i = 0; i < 8; i++) // 每个点生成多个粒子
                {
                    Vector2 particlePosition = center + point + Main.rand.NextVector2Circular(2f, 2f); // 基于点位置生成粒子
                    Vector2 velocity = (particlePosition - center).SafeNormalize(Vector2.Zero) * baseSpeed;

                    Dust dust = Dust.NewDustPerfect(
                        particlePosition,
                        Main.rand.Next(new int[] { 55, 35, 174 }), // 混合使用粒子类型
                        velocity
                    );
                    dust.scale = Main.rand.NextFloat(1.85f, 2.35f); // 设置粒子大小
                    dust.alpha = 217; // 设置透明度
                    dust.noGravity = true; // 粒子不受重力影响
                    dust.velocity += velocity * Main.rand.NextFloat(0.1f, 0.3f); // 加速粒子
                    dust.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi); // 随机初始旋转
                    dust.customData = new object[] { acceleration, particleLifetime }; // 存储加速度和生命周期
                }
            }
        }





    }
}
