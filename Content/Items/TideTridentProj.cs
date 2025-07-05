using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Audio;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Rogue;
using Mono.Cecil;
using CalamityMod.Projectiles.Typeless;
using CalamityMod;

namespace CalamityThrowingSpear.Content.Items
{
    public class TideTridentProj : ModProjectile
    {
        // 设置弹幕的基本属性
        public override void SetDefaults()
        {
            Projectile.width = 40; 
            Projectile.height = 40; 
            Projectile.friendly = true; 
            Projectile.DamageType = DamageClass.Melee; 
            Projectile.tileCollide = true; 
            Projectile.penetrate = 1; 
            Projectile.timeLeft = 180;             
        }
       

        // 定义弹幕的行为
        public override void AI()
        {
            Projectile.velocity *= 1f; // 每帧降低弹幕速度
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(50f); // 根据速度方向设置旋转角度，并加上45度偏移
           
            if (Projectile.ai[1] % 30 == 0 && Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 2; i++)
                {
                    NPC target = null;
                    float maxDistance = 2400f; // 半径150个方块范围

                    // 寻找最近的敌人
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.CanBeChasedBy() && !npc.friendly)
                        {
                            float distanceToNPC = Vector2.Distance(Projectile.Center, npc.Center);
                            if (distanceToNPC < maxDistance)
                            {
                                maxDistance = distanceToNPC;
                                target = npc;
                            }
                        }
                    }

                    // 发射Feather弹幕
                    Vector2 featherVelocity;
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        featherVelocity = direction * Projectile.velocity.Length() * 2f; // 速度为本体的两倍
                    }
                    else
                    {
                        featherVelocity = Projectile.velocity * 2f;
                    }


                    Vector2 spawnPosition = Projectile.Center;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, featherVelocity.RotatedBy(MathHelper.ToRadians(90)),



                        ModContent.ProjectileType<SmallTridentProj>(), (int)(Projectile.damage * 0.95), Projectile.knockBack, Projectile.owner);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, featherVelocity.RotatedBy(MathHelper.ToRadians(-90)),



                       ModContent.ProjectileType<SmallTridentProj>(), (int)(Projectile.damage * 0.95), Projectile.knockBack, Projectile.owner);
                }
            }

            // 增加计时器，用于控制Feather弹幕生成频率
            Projectile.ai[1] += 1.5f;


        }









    }
}