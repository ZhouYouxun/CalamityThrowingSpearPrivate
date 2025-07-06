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
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Content.Items
{
    public class SmallTridentProj : ModProjectile
    {
        private int hitCounter = 0;
        public int Time = 0;

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
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;


            if (Projectile.ai[1] % 30 == 0 && Main.myPlayer == Projectile.owner)
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
                // 保持飞行方向不变
               

                // 添加蓝色光效
                Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 0.55f);

                // 添加高速旋转效果
                //Projectile.rotation += 0.6f; // 你可以根据需求调整旋转速度，增加或减少该值



                //for (int i = 0; i < 2; i++)
                //{
                //    NPC target = null;
                //    // float maxDistance = 1;
                //    float maxDistance = 450;

                //    // 寻找最近的敌人
                //    foreach (NPC npc in Main.npc)
                //    {
                //        if (npc.CanBeChasedBy() && !npc.friendly)
                //        {
                //            float distanceToNPC = Vector2.Distance(Projectile.Center, npc.Center);
                //            if (distanceToNPC < maxDistance)
                //            {
                //                maxDistance = distanceToNPC;
                //                target = npc;
                //            }
                //        }
                //    }        
                //}


            }
            if (Projectile.timeLeft > 3f) 
            {
                Projectile.velocity *= 1f; // 保持匀速直线运动
                Vector2 particleOffset = new Vector2(2f * Projectile.direction, 0);
                particleOffset.X += Main.rand.NextFloat(-3f, 3f); // 随机左右偏移
                Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;
                Particle Smear = new CircularSmearVFX(particlePosition, Color.Blue * Main.rand.NextFloat(0.78f, 0.85f), Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(1.2f, 1.3f));
                GeneralParticleHandler.SpawnParticle(Smear);
            }
            Projectile.ai[0]++;




            // 在前20帧内直线飞行
            if (Projectile.ai[0] <= 20)
            {
                return;
            }

            // 20帧后开始追踪最近的敌人
            NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
            if (target != null)
            {
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 追踪速度为12f
            }


            // 增加计时器，用于控制Feather弹幕生成频率
            Projectile.ai[1] += 1f;
           


        }



        //public override bool PreDraw(ref Color lightColor)
        //{
        //    SpriteBatch spriteBatch = Main.spriteBatch;
        //    Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

        //    // 确保以贴图中心为旋转中心
        //    Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);

        //    // 计算绘制位置，考虑gfxOffY偏移
        //    Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

        //    // 进行绘制，使用正确的旋转中心
        //    spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

        //    return false; // 返回false，防止游戏默认的绘制
        //}








    }
}