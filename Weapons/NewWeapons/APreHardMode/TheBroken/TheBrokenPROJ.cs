using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken
{
    public class TheBrokenPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/TheBroken/TheBroken";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
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
            Projectile.penetrate = 1; // 允许？次伤害
            Projectile.timeLeft = 120;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 0; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 将光源颜色更改为偏黑的深蓝色，光照强度为 0.55
            //Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.1f, 0.5f) * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.00f;

            // 每帧增加 ai[0] 计数
            Projectile.ai[0]++;

            // 前x帧禁用方块碰撞（防止在平地上某些角度射不出来）
            if (Projectile.ai[0] < 5)
            {
                Projectile.tileCollide = false; // 禁用碰撞
            }
            else
            {
                Projectile.tileCollide = true; // 启用碰撞
            }

            GenerateDirtParticles(); // 调用粒子生成
            //FireAdditionalProjectiles(); // 检测是否需要发射额外弹幕

            {




            }



        }

       /* private bool hasFiredAdditionalProjectiles = false; // 开关，防止重复触发

        private void FireAdditionalProjectiles()
        {
            if (Projectile.timeLeft == 80 && !hasFiredAdditionalProjectiles)
            {
                SoundEngine.PlaySound(SoundID.Item127, Projectile.Center);

                hasFiredAdditionalProjectiles = true; // 标记开关已开启
                int[] projectileTypes = { ProjectileID.Shuriken, ProjectileID.ThrowingKnife }; // 额外弹幕类型

                int count = Main.rand.Next(2, 5); // 随机生成2~4发
                for (int i = 0; i < count; i++)
                {
                    float angleOffset = MathHelper.Lerp(-10, 10, i / (float)(count - 1)); // 每个弹幕的偏移角度
                    Vector2 velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(angleOffset)) * 1.2f; // 计算速度方向
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, Main.rand.Next(projectileTypes), (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner);
                }
            }
        }*/

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {



        }


        private void GenerateDirtParticles()
        {
            if (Main.rand.NextBool(3)) // 每3帧生成一次粒子
            {
                int[] dirtDustTypes = { DustID.Dirt, DustID.Stone, DustID.Smoke }; // 泥土相关粒子类型
                int selectedDust = Main.rand.Next(dirtDustTypes); // 随机选择粒子类型

                // 生成三螺旋效果
                for (int i = 0; i < 3; i++) // 三个螺旋
                {
                    float rotationOffset = MathHelper.TwoPi / 3 * i; // 每个螺旋的初始角度
                    float angle = Projectile.ai[0] * 0.1f + rotationOffset; // 动态角度，随时间变化
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f; // 每个螺旋的偏移位置

                    Vector2 dustPosition = Projectile.Center + offset; // 计算粒子位置
                    Dust dust = Dust.NewDustPerfect(dustPosition, selectedDust, null, 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                    dust.noGravity = true; // 粒子无重力
                    dust.fadeIn = 1.5f; // 粒子淡入效果
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            //SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            CreateGeometricEffect(); // 调用法阵特效
        }

        private void CreateGeometricEffect()
        {
            int particleCount = 36; // 粒子数量，用于形成一个圆形法阵
            float radius = 50f; // 法阵半径

            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount; // 计算每个粒子的位置
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                // 创建泥土粒子
                int[] dirtDustTypes = { DustID.Dirt, DustID.Stone, DustID.Smoke };
                int selectedDust = Main.rand.Next(dirtDustTypes);

                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, selectedDust, null, 100, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true; // 粒子无重力
                dust.fadeIn = 1.2f; // 粒子淡入效果
            }
        }



    }
}