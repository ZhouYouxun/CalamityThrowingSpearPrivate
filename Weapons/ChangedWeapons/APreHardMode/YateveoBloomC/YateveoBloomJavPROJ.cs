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
using CalamityMod.Items.Tools;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.APreHardMode.YateveoBloomC
{
    public class YateveoBloomJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/APreHardMode/YateveoBloomC/YateveoBloomJav";
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.APreHardMode";
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
            Projectile.timeLeft = 240;
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


            // Lighting - 添加深绿色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkGreen.ToVector3() * 0.55f);

            //// 弹幕保持直线运动并逐渐加速
            //Projectile.velocity *= 1.01f;

            // 添加粒子效果 - 深红色和深绿色粒子
            if (Main.rand.NextBool(3)) // 以1/3的概率生成深红色或深绿色粒子
            {
                int dustType = Main.rand.NextBool() ? DustID.RedTorch : DustID.GreenTorch; // 红色或绿色粒子
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
            }

            if (Projectile.localAI[0] > 20f)
            {
                if (Projectile.velocity.Y < 24f)
                {
                    Projectile.velocity.Y += 0.4f;
                }
            }

        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
            {
                Projectile.Kill();
            }
            else
            {
                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 使敌人中毒，持续 180 帧
            target.AddBuff(BuffID.Poisoned, 180);

            // 释放独特的草音效	
            SoundEngine.PlaySound(SoundID.Grass, Projectile.position);
            //// 粘附到敌人，持续造成伤害
            //Projectile.ModifyHitNPCSticky(20);
        }


        public override void OnKill(int timeLeft)
        {
            // 释放独特的草音效	
            SoundEngine.PlaySound(SoundID.Grass, Projectile.position);

            int dustType = Main.rand.NextBool() ? DustID.RedTorch : DustID.GreenTorch; // 红色或绿色粒子
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.5f);
            }

            // 随机释放三个 BladeOfGrass 弹幕，倍率为 95%
            for (int i = 0; i < 3; i++)
            {
                Vector2 direction = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi); // 随机方向
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, direction * 8f, 976, (int)(Projectile.damage * 0.3f), Projectile.knockBack, Projectile.owner);
            }
        }





    }
}
