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
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    public class SawBladeForkHornJavRPP : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        private float rotationAngle = 0f; // 旋转角度
        private const float rotationSpeed = 0.05f; // 旋转速度
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
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 1200;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深棕色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Brown.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.005f;


            // 每 2 帧生成 Spray 粒子特效
            int[] sprayDusts = { DustID.PureSpray, DustID.HallowSpray, DustID.CorruptSpray, DustID.MushroomSpray, DustID.CrimsonSpray, DustID.SandSpray, DustID.SnowSpray, DustID.DirtSpray };
            int selectedSpray = sprayDusts[Main.rand.Next(sprayDusts.Length)];
            int sprayIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, selectedSpray, 0f, 0f, 100, default, Main.rand.NextFloat(0.8f, 1.2f));
            Dust spray = Main.dust[sprayIndex];
            spray.noGravity = true; // 粒子不受重力影响
            spray.velocity = Projectile.velocity * Main.rand.NextFloat(0.8f, 1.2f); // 增加随机化速度
            spray.scale *= Main.rand.NextFloat(1.1f, 1.5f); // 粒子大小略微波动


            // 更新旋转角度
            rotationAngle += rotationSpeed * 2.5f; // 旋转速度增加到原来的2.5倍
            if (rotationAngle > MathHelper.TwoPi)
            {
                rotationAngle -= MathHelper.TwoPi;
            }

            // 计算三个粒子的位置，互为120度夹角
            float[] angles = { 0f, MathHelper.TwoPi / 3f, MathHelper.TwoPi * 2f / 3f }; // 粒子间隔120度
            float radius = Main.rand.NextFloat(0.5f, 1.25f) * 16f; // 公转半径随机化
            foreach (float initialAngle in angles)
            {
                // 计算粒子的当前角度和位置
                float angle = initialAngle + rotationAngle;
                Vector2 position = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;

                // 创建具有魔法效果的粒子特效
                int magicDustIndex = Dust.NewDust(position, 0, 0, DustID.PurpleCrystalShard, 0f, 0f, 100, default, Main.rand.NextFloat(1.0f, 2.0f));
                Main.dust[magicDustIndex].noGravity = true; // 粒子不受重力影响
                Main.dust[magicDustIndex].velocity = Vector2.Zero; // 粒子初始速度为零
                Main.dust[magicDustIndex].color = Color.Lerp(Color.Purple, Color.Cyan, Main.rand.NextFloat(0.3f, 0.7f)); // 粒子颜色带有渐变效果
            }




            // 前x帧不追踪，之后开始追踪敌人
            if (Projectile.ai[1] > 30)
            {
                NPC target = Projectile.Center.ClosestNPCAt(6666); // 查找xx范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 16f, 0.08f); // 追踪速度为xf
                }
            }
            else
            {
                Projectile.ai[1]++;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 给予敌人 MarkedforDeath死亡标记
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 300); // 5秒持续时间

            // 给予敌人 Crumbling粉碎
            target.AddBuff(ModContent.BuffType<Crumbling>(), 300); // 5秒持续时间

            // 生成缩小的棕褐色冲击波特效
            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Brown, new Vector2(0.25f), Projectile.rotation, 3f, 0.1f, 30);
            GeneralParticleHandler.SpawnParticle(pulse);
        }


    }
}