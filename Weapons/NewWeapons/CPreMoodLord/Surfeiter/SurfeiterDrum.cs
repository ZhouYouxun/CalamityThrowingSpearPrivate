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
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    internal class SurfeiterDrum : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-1";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }


        //1.笞:Flogging-敌怪获得1.02倍的易伤
        //2.杖:Beating-敌怪的接触伤害减少30%
        //3.徒:Imprisoning-敌怪的移动速度减少50%
        //4.流:Banishing-敌怪防御降低40
        //5.死:Executing-2秒的倒计时结束后造成5000点伤害
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 50;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        // 添加形态索引
        private int drumForm = 0; // 0-4 对应五种形态
        public void SwitchForm()
        {
            drumForm = (drumForm + 1) % 5; // 循环切换形态
            Projectile.netUpdate = true;  // 同步到客户端
        }
        public override bool? CanHitNPC(NPC target)
        {
            return false; // 永远不会命中 NPC
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.timeLeft = 50;
            // 如果玩家不存在或死亡，移除弹幕
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            // 跟随玩家头顶
            Vector2 destination = player.Center + new Vector2(0, -15 * 16);
            Projectile.velocity = (destination - Projectile.Center) * 0.1f;

            // 检测与 SurfeiterPROJ 的碰撞
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<SurfeiterPROJ>() && proj.Hitbox.Intersects(Projectile.Hitbox))
                {
                    int projDamage = (int)(proj.damage * 1.0f); // 继承 SurfeiterPROJ 的伤害 的1.0倍
                    for (int i = 0; i < 5; i++)
                    {
                        int projID = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            new Vector2(6, 0).RotatedBy(MathHelper.ToRadians(72 * i)),
                            ModContent.ProjectileType<SurfeiterDrumINV>(),
                            projDamage, // 传递 SurfeiterPROJ 的伤害
                            Projectile.knockBack,
                            Projectile.owner);

                        // 传递模式
                        if (Main.projectile[projID].ModProjectile is SurfeiterDrumINV drumINV)
                        {
                            drumINV.SetDrumForm(drumForm);
                        }

                    }

                    // 释放泥土粒子
                    for (int i = 0; i < Main.rand.Next(50, 101); i++)
                    {
                        Dust.NewDust(Projectile.Center, 10, 10, DustID.Dirt, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                    }

                    // 消除 SurfeiterPROJ
                    proj.Kill();
                    //Projectile.Kill(); 把自己杀了干嘛，神经
                    break;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            string texturePath = $"CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-{drumForm + 1}";
            Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;

            Main.spriteBatch.Draw(texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                texture.Size() / 2,
                Projectile.scale,
                SpriteEffects.None,
                0);

            return false;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }
    }
}