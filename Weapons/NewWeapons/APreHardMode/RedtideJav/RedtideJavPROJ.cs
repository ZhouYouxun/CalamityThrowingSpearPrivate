using CalamityMod.Particles;
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
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav
{
    public class RedtideJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/RedtideJav/RedtideJav";
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
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
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
            // 保持弹幕旋转 (在这基础上再增加一点角度，为了适配这个特殊的贴图)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.ToRadians(15);

            // 添加红色光源
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.55f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.003f;

            // 每隔一段时间生成水泡特效
            if (Main.rand.NextBool(5)) // 每5帧有20%概率生成一次水泡
            {
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
                bubble.timeLeft = 8 + Main.rand.Next(6);
                bubble.scale = Main.rand.NextFloat(0.6f, 1f) * (1 + Projectile.timeLeft / (float)Projectile.timeLeft);
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }


            // 每帧增加 ai[x] 计数
            Projectile.ai[1]++;

            // 前x帧禁用方块碰撞（防止在平地上某些角度射不出来）
            if (Projectile.ai[1] < 5)
            {
                Projectile.tileCollide = false; // 禁用碰撞
            }
            else
            {
                Projectile.tileCollide = true; // 启用碰撞
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 销毁时生成更强的水泡爆炸效果
            int bubbleCount = 40 + Main.rand.Next(20); // 更多的泡泡
            for (int i = 0; i <= bubbleCount; i++)
            {
                Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity * 1.5f + Main.rand.NextVector2Circular(4f, 4f), 411);
                bubble.timeLeft = 8 + Main.rand.Next(6);
                bubble.scale = Main.rand.NextFloat(0.8f, 1.5f); // 更大的泡泡
                bubble.type = Main.rand.NextBool(3) ? 412 : 411;
            }

            // 在当前弹幕的位置生成 RedtideJavEXP 弹幕
            Projectile.NewProjectile(
                Projectile.GetSource_Death(),          // 弹幕生成来源
                Projectile.Center,                    // 弹幕生成的位置（当前弹幕的中心）
                Vector2.Zero,                         // 初始速度为零（原地爆炸效果）
                ModContent.ProjectileType<RedtideJavEXP>(), // RedtideJavEXP 的类型
                (int)(Projectile.damage * 1.0f),      // 伤害倍率为1倍
                Projectile.knockBack,                 // 使用当前弹幕的击退值
                Projectile.owner                      // 当前弹幕的所有者
            );

            //// 释放伤害，半径为5格的范围内所有敌人受到弹幕的伤害
            //float explosionRadius = 5 * 16; // 5格（每格16像素）
            //foreach (NPC target in Main.npc.Where(n => n.active && !n.friendly))
            //{
            //    float dist = Vector2.Distance(Projectile.Center, target.Center);
            //    if (dist <= explosionRadius)
            //    {
            //        int damage = Projectile.damage; // 1倍伤害
            //        bool crit = Main.rand.Next(100) <= Main.player[Projectile.owner].GetCritChance<MeleeDamageClass>();
            //        target.StrikeNPC(target.CalculateHitInfo(damage, 0, crit, 0));

            //        if (Main.netMode != NetmodeID.SinglePlayer)
            //            NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, target.whoAmI, damage, 0f, 0f, crit ? 1 : 0);
            //    }
            //}

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 240);


        }




    }
}