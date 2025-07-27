using CalamityMod;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Tiles.Astral;
using CalamityMod.NPCs.Polterghast;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    internal class XiaoZhiTiaoGetting
    {
        public class GiveYouStarsofDestiny : GlobalNPC
        {
            private static bool hasGivenXiaoZhiTiaoSoD = false; // 静态开关，限制当前世界内只触发一次

            public override bool InstancePerEntity => true; // 每个 NPC 实例独立处理

            // 检测 NPC 死亡逻辑
            public override void OnKill(NPC npc)
            {
                // 检查是否为 Bumblefuck 且奖励未被触发过
                if (npc.type == ModContent.NPCType<Polterghast>() && !hasGivenXiaoZhiTiaoSoD)
                {
                    hasGivenXiaoZhiTiaoSoD = true; // 标记物品已触发

                    // 获取当前玩家对象
                    Player player = Main.LocalPlayer;

                    // 给予物品 XiaoZhiTiaoFT
                    player.QuickSpawnItem(null, ModContent.ItemType<XiaoZhiTiaoSoD>(), 1);

                    // 显示提示文本
                    Main.NewText("命运开始变动……", Color.White);
                }
            }
        }
    }
}
