using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.StarGunSagittarius
{
    public class StarGunSagittariusEDebuff : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "ModBuff";
        private int summonCounter = 0;
        private int baseDamage;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.dayTime)
            {
                // 白天减少敌人攻击力30%
                npc.damage = (int)(npc.damage * 0.7);

                // 添加蓝色光源
                Lighting.AddLight(npc.Center, Color.CornflowerBlue.ToVector3() * 0.6f);

                // 随机生成青色、蓝色和深蓝色的水平粒子
                if (Main.rand.NextBool())
                {
                    Vector2 npcSize = npc.Center + new Vector2(Main.rand.NextFloat(-npc.width / 2, npc.width / 2), Main.rand.NextFloat(-npc.height / 2, npc.height / 2));
                    Vector2 sparkleVelocity = new Vector2(Main.rand.NextFloat(-5f, 5f), 0); // 水平方向随机移动

                    SparkParticle spark = new SparkParticle(npcSize, sparkleVelocity, false, Main.rand.Next(11, 13), Main.rand.NextFloat(0.2f, 0.5f), Main.rand.NextBool(7) ? Color.Aqua : Color.DarkBlue);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            else
            {
                // 夜晚减少敌人防御力50%
                npc.defense = (int)(npc.defense * 0.5);

                // 添加亮黄色光源
                Lighting.AddLight(npc.Center, Color.LightGoldenrodYellow.ToVector3() * 0.6f);

                // 随机生成亮黄色粒子，水平方向
                if (Main.rand.NextBool())
                {
                    Vector2 npcSize = npc.Center + new Vector2(Main.rand.NextFloat(-npc.width / 2, npc.width / 2), Main.rand.NextFloat(-npc.height / 2, npc.height / 2));
                    Vector2 sparkleVelocity = new Vector2(Main.rand.NextFloat(-5f, 5f), 0); // 水平方向随机移动

                    SparkParticle spark = new SparkParticle(npcSize, sparkleVelocity, false, Main.rand.Next(11, 13), Main.rand.NextFloat(0.2f, 0.5f), Main.rand.NextBool(7) ? Color.Gold : Color.Yellow);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }

            // 每 0.75 秒（45 帧）生成一个分裂长枪
            if (++summonCounter >= 45)
            {
                summonCounter = 0; // 重置计数器以实现定时
                //SpawnSplitProjectile(npc);
            }
        }

        //private void SpawnSplitProjectile(NPC npc)
        //{
        //    // 生成分裂长枪的位置
        //    float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
        //    Vector2 spawnPosition = npc.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 70f;

        //    // 设置分裂长枪的速度
        //    Vector2 velocitySPIT = Vector2.Normalize(npc.Center - spawnPosition) * 16;

        //    // 生成分裂长枪
        //    Projectile.NewProjectile(null, spawnPosition, velocitySPIT, ModContent.ProjectileType<StarGunSagittariusSPIT>(), baseDamage, 0, Main.myPlayer);

        //    // 在圆周上生成闪光粒子效果
        //    for (int i = 0; i < 36; i++)
        //    {
        //        float particleAngle = MathHelper.TwoPi * i / 36f;
        //        Vector2 sparklePosition = npc.Center + new Vector2((float)Math.Cos(particleAngle), (float)Math.Sin(particleAngle)) * 70f * 16f;
        //        Vector2 sparkleVelocity = -Vector2.Normalize(sparklePosition - npc.Center) * Main.rand.NextFloat(0.5f, 1.5f);

        //        SparkleParticle spark = new SparkleParticle(sparklePosition, sparkleVelocity, Color.Gold * 0.6f, Color.LightGoldenrodYellow * 0.3f, Main.rand.NextFloat(0.3f, 0.6f), 15, Main.rand.NextFloat(-8, 8), 0.2f, false);
        //        GeneralParticleHandler.SpawnParticle(spark);
        //    }
        //}




    }
}
