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
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav
{
    public class GraniteJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/GraniteJav/GraniteJav";
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
            Projectile.penetrate = 3; // 允许3次伤害
            Projectile.timeLeft = 300;
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

            // Lighting - 将光源颜色更改为偏黑的深蓝色，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.1f, 0.5f) * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 添加深蓝色粒子效果，随机出现在弹幕附近
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 150, default, 1.2f);
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
            // 当命中敌人时，弹幕向上飞行，且带有一定的左右偏移
            float angle = MathHelper.ToRadians(Main.rand.Next(-10, 10)); // 在正上方左右10度范围内取随机角度
            Vector2 upwardVelocity = new Vector2(0, -8f).RotatedBy(angle); // 初始向上飞行的速度
            Projectile.velocity = upwardVelocity;

            // 继续受到重力影响
            Projectile.velocity.Y += 0.2f; // 模拟逐渐受重力下坠

            // 播放音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // 在弹幕消失时释放深蓝色粒子特效
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 2.2f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 在弹幕消失时释放深蓝色粒子特效
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default, 2.2f);
            }
        }



    }
}