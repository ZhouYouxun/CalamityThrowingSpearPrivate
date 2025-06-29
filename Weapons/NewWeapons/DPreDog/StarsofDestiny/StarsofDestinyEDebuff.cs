using CalamityMod.Particles;
using CalamityThrowingSpear.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestinyEDebuff : ModBuff
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // Buff 不会保存
        }

        // 专门用于在敌人身上进行标记，因此不具备削弱能力，仅仅是视觉效果


        public override void Update(NPC npc, ref int buffIndex)
        {
            //// 每隔2~4帧生成一次特效
            //if (Main.rand.NextBool(2, 4))
            //{
            //    // 随机生成一个偏移量
            //    Vector2 offset = new Vector2(Main.rand.NextFloat(-100f, 100f), Main.rand.NextFloat(-100f, 100f));
            //    Vector2 position = npc.Center + offset;

            //    if (Main.rand.NextBool()) // 随机选择一种特效
            //    {
            //        // 第1种特效 - SparkleParticle
            //        Color startColor = Color.White;
            //        Color endColor = Color.Transparent;
            //        SparkleParticle spark = new SparkleParticle(
            //            position,
            //            Vector2.Normalize(npc.Center - position) * Main.rand.NextFloat(0.3f, 0.6f), // 吸引到敌人
            //            startColor,
            //            endColor,
            //            Main.rand.NextFloat(0.3f, 0.6f), // 缩放大小
            //            Main.rand.Next(10, 20), // 粒子寿命
            //            Main.rand.NextFloat(-8, 8), // 旋转速度
            //            0.3f, // 渐隐速度
            //            false
            //        );
            //        GeneralParticleHandler.SpawnParticle(spark);
            //    }
            //    else
            //    {
            //        // 第2种特效 - SparkParticle
            //        Color electricColor = Color.White;
            //        Vector2 randomDirection = Vector2.Normalize(npc.Center - position) * Main.rand.NextFloat(0.8f, 1.2f);
            //        Particle electricParticle = new SparkParticle(
            //            position,
            //            randomDirection,
            //            false,
            //            60, // 粒子寿命
            //            Main.rand.NextFloat(0.8f, 1.2f), // 缩放大小
            //            electricColor
            //        );
            //        GeneralParticleHandler.SpawnParticle(electricParticle);
            //    }
            //}
        }


    }
}
