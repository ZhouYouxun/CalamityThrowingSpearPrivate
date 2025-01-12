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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.WulfrimJav
{
    public class WulfrimJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/WulfrimJav/WulfrimJav";
        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
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
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
            Projectile.aiStyle = ProjAIStyleID.Arrow; // 让弹幕受到重力影响

        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深橙色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.01f;

            // 释放亮绿色粒子特效
            if (Main.rand.NextBool(5))
            {
                Vector2 trailPos = Projectile.Center;
                float trailScale = Main.rand.NextFloat(0.8f, 1.2f); // 粒子缩放
                Color trailColor = Color.LimeGreen;

                // 创建粒子
                Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(trail);
            }



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


        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 播放音效
            SoundEngine.PlaySound(SoundID.Item15, Projectile.position);

            // 定义damage和knockback，使用当前弹幕的数值
            int damage = Projectile.damage; // 获取当前弹幕的伤害值
            float knockback = Projectile.knockBack; // 获取当前弹幕的击退值

            // 在敌人上方50个方块处召唤WulfrimJavExtraPROJ
            Vector2 spawnPos = target.Center - new Vector2(0, 50 * 16); // 大约50个方块
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<WulfrimJavExtraPROJ>(), damage, knockback, Projectile.owner);

            // 生成特效，颜色改为亮绿色
            for (int i = 0; i < 75; i++)
            {
                float offsetAngle = MathHelper.TwoPi * i / 75f;
                float unitOffsetX = (float)Math.Pow(Math.Cos(offsetAngle), 3D);
                float unitOffsetY = (float)Math.Pow(Math.Sin(offsetAngle), 3D);

                Vector2 puffDustVelocity = new Vector2(unitOffsetX, unitOffsetY) * 5f;
                Dust magic = Dust.NewDustPerfect(target.Center, 267, puffDustVelocity); // 使用敌人的中心位置生成特效
                magic.scale = 1.8f;
                magic.fadeIn = 0.5f;
                magic.color = Color.LightGreen; // 将颜色改为亮绿色
                magic.noGravity = true;
            }
        }



    }
}