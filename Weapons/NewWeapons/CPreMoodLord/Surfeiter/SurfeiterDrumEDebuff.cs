using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    internal class SurfeiterDrumEDebuff : ModBuff
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // 不保存 Buff
            Main.debuff[Type] = false;   // Buff 是增益效果
        }

        private int drumForm = 0;

        public void SetDrumForm(int form)
        {
            drumForm = form;
        }

        //public override void Update(NPC npc, ref int buffIndex)
        //{
        //    // 可能后续还要再慢慢调整
        //    switch (drumForm)
        //    {
        //        case 0: // 笞 (Flogging) - 敌怪获得1.02倍易伤
        //            npc.defense = (int)Math.Floor(npc.defense * 0.98); // 将防御减少2%，向下取整
        //            break;
        //        case 1: // 杖 (Beating) - 敌怪接触伤害减少30%
        //            npc.damage = (int)(npc.damage * 0.7f);
        //            break;
        //        case 2: // 徒 (Imprisoning) - 敌怪移动速度减少50%
        //            npc.velocity *= 0.5f;
        //            break;
        //        case 3: // 流 (Banishing) - 敌怪防御降低40
        //            npc.defense -= 40;
        //            if (npc.defense < 0)
        //                npc.defense = 0;
        //            break;
        //        case 4: // 死 (Executing) - 2秒后造成5000点伤害
        //            if (npc.buffTime[buffIndex] == 1) // Buff 即将结束时
        //            {
        //                npc.StrikeNPC(new NPC.HitInfo
        //                {
        //                    Damage = 5000,        // 固定5000点伤害
        //                    Knockback = 0f,       // 没有击退效果
        //                    HitDirection = 0      // 无方向性
        //                });
        //            }
        //            break;
        //    }
        //}
        public override void Update(NPC npc, ref int buffIndex)
        {
            //1.笞:Flogging-敌怪获得1.02倍的易伤
            //2.杖:Beating-敌怪的接触伤害减少30%
            //3.徒:Imprisoning-敌怪的移动速度减少50%
            //4.流:Banishing-敌怪防御降低40
            //5.死:Executing-2秒的倒计时结束后造成5000点伤害

            int drumForm = npc.buffTime[buffIndex] % 5; // 获取模式

            // 通知 GlobalNPC 应用对应效果
            if (npc.TryGetGlobalNPC(out SurfeiterDrumGlobalNpc globalNpc))
            {
                globalNpc.drumForm = drumForm; // 将模式传递给 GlobalNPC
            }

            //// 如果是模式 5: Executing，则保留在 Buff 中处理
            // 不再需要这一段，模式5的“死”依旧转移至GlobalNPC里面
            //if (drumForm == 4)
            //{
            //    if (npc.buffTime[buffIndex] == 1) // Buff 即将结束时
            //    {
            //        npc.StrikeNPC(new NPC.HitInfo
            //        {
            //            Damage = 5000,        // 固定5000点伤害
            //            Knockback = 0f,       // 没有击退效果
            //            HitDirection = 0      // 无方向性
            //        });
            //    }
            //    // 在敌人头顶弹出黑色的 "5000" 战斗文本
            //    //CombatText.NewText(npc.Hitbox, Color.Black, "5000", true, false);
            //}
        }

    }
}
