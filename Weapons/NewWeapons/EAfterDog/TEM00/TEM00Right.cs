using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00
{
    internal class TEM00Right : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TEM00/TEM00";
        public override void SetStaticDefaults()
        {
            // 设置弹幕拖尾长度和模式
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 绘制控制函数，可用于绘制自定义贴图、添加发光效果、叠加特效等
            // 若不需要可返回 true 使用默认绘制【很不推荐】
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2; // 可击中次数
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 0; // 可调节飞行平滑度
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 弹幕生成时执行，用于初始化粒子或播放生成音效
        }

        public override void AI()
        {
            // === 控制旋转 ===
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // === 飞行光效（浅蓝接近白色） ===
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.8f);

            // === 直线粒子轨迹（Dust） ===
            if (Main.rand.NextBool(2))
            {
                Dust d1 = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Electric,
                    Projectile.velocity * -0.2f
                );
                d1.noGravity = true;
                d1.scale = 1.1f;
                d1.color = Color.Lerp(Color.White, Color.LightBlue, 0.6f);
            }

            if (Main.rand.NextBool(3))
            {
                Dust d2 = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.BlueCrystalShard,
                    Projectile.velocity * -0.3f
                );
                d2.noGravity = true;
                d2.scale = 0.9f + Main.rand.NextFloat(0.4f);
                d2.color = Color.Lerp(Color.White, Color.Cyan, 0.4f);
            }

            // === 数学感方块粒子 ===
            if (Main.rand.NextBool(6))
            {
                SquareParticle squareParticle = new SquareParticle(
                    Projectile.Center,
                    Projectile.velocity * 0.2f, // 沿着速度方向漂移
                    false,                      // 不受重力
                    30,                         // 存活时间
                    1.2f + Main.rand.NextFloat(0.4f),
                    Color.Cyan * 1.2f
                );
                GeneralParticleHandler.SpawnParticle(squareParticle);
            }

            // === 光点（GlowOrbParticle） ===
            if (Main.rand.NextBool(5))
            {
                GlowOrbParticle orb = new GlowOrbParticle(
                    Projectile.Center,
                    Vector2.Zero,
                    false,
                    8,
                    0.8f,
                    Color.Lerp(Color.White, Color.Cyan, 0.5f),
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }





        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 只在第一次命中时触发
            if (!Projectile.localAI[0].Equals(1f))
            {
                // 关闭后续伤害
                Projectile.friendly = false;

                // 标记已命中过
                Projectile.localAI[0] = 1f;

                // 随机决定转向方向（-1 = 左转，+1 = 右转）
                Projectile.ai[0] = Main.rand.NextBool() ? -1f : 1f;

                // ====== 召唤 3 条激光 ======
                Player owner = Main.player[Projectile.owner];
                Vector2 center = Projectile.Center;

                // 中心激光：正对自身 → 命中点回收
                Vector2 dirMain = -Projectile.velocity.SafeNormalize(Vector2.UnitY);
                SpawnMathLaser(center, dirMain, owner);

                // 左右两条激光：±45° 对称
                Vector2 dirLeft = dirMain.RotatedBy(MathHelper.ToRadians(45));
                Vector2 dirRight = dirMain.RotatedBy(MathHelper.ToRadians(-45));
                SpawnMathLaser(center, dirLeft, owner);
                SpawnMathLaser(center, dirRight, owner);

                // 音效 / 特效
                SoundEngine.PlaySound(SoundID.Item33, Projectile.Center);
                for (int i = 0; i < 12; i++)
                {
                    Dust d = Dust.NewDustPerfect(center, DustID.Electric,
                        Main.rand.NextVector2Circular(4f, 4f));
                    d.noGravity = true;
                    d.scale = 1.2f;
                    d.color = Color.Lerp(Color.White, Color.Cyan, 0.6f);
                }


                // ====== 在敌人身上生成 TEM00RightAIM ======
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero, // 没有初速度，固定在敌人身上
                    ModContent.ProjectileType<TEM00RightAIM>(),
                    Projectile.damage,
                    0f,
                    owner.whoAmI
                );
            }
        }

        /// <summary>
        /// 生成数学感激光（浅蓝→白）
        /// </summary>
        private void SpawnMathLaser(Vector2 start, Vector2 direction, Player owner)
        {
            int damage = (int)(Projectile.damage * 0.8f); // 略低伤害
            float kb = 2f;
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                start,
                direction,
                ModContent.ProjectileType<TEM00LeftLazer>(), // 直接调用之前的激光弹幕
                damage,
                kb,
                owner.whoAmI
            );
        }






        public override void OnKill(int timeLeft)
        {
            // 弹幕死亡（时间到或碰撞）时执行，可用于生成碎裂粒子、播放破碎音效
        }


    }
}
