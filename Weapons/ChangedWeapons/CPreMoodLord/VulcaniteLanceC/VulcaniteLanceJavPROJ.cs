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
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
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
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);

            // 生成FuckYou爆炸弹幕，伤害倍率为1.25，大小为2倍
            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), (int)(Projectile.damage * 1.25f), Projectile.knockBack, Projectile.owner, ai0: 2f);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FuckYou>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

            // 10% 概率召唤 TinyFlare 弹幕
            if (Main.rand.NextFloat() < 1f)
            {
                // 攻击后在玩家位置生成TinyFlare弹幕，速度为0.7倍，伤害为0.33倍
                Vector2 flareDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.player[Projectile.owner].Center, flareDirection * 0.7f * 10f,
                    ModContent.ProjectileType<VulcaniteLanceJavTinyFlare>(), (int)(Projectile.damage * 1.25f), Projectile.knockBack, Projectile.owner);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 生成橙红色的椭圆形粒子特效，往8个方向发射
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8 * i;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                // 调整方向向量的大小，将原来的 0.75f 增加到 2f，使粒子飞得更远
                Particle pulse = new DirectionalPulseRing(Projectile.Center, direction * 2f, Color.OrangeRed, new Vector2(1f, 2.5f), 0f, 0.2f, 0.03f, 20);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }





    }
}
