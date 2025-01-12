using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Default;

namespace CalamityThrowingSpear
{
    public class KaiJuXiaoZhiTiaoPlayer : ModPlayer
    {
        // 用于标记玩家是否已收到物品
        public bool hasReceivedKaiJuXiaoZhiTiao = false;

        // 玩家进入游戏时触发
        public override void OnEnterWorld()
        {
            if (!hasReceivedKaiJuXiaoZhiTiao)
            {
                // 发放 KaiJuXiaoZhiTiao 物品
                Player.QuickSpawnItem(Player.GetSource_Misc("CalamityThrowingSpear"), ModContent.ItemType<KaiJuXiaoZhiTiao>());

                // 设置状态，防止重复发放
                hasReceivedKaiJuXiaoZhiTiao = true;
            }
        }

        // 保存玩家数据
        public override void SaveData(TagCompound tag)
        {
            tag["hasReceivedKaiJuXiaoZhiTiao"] = hasReceivedKaiJuXiaoZhiTiao;
        }

        // 加载玩家数据
        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("hasReceivedKaiJuXiaoZhiTiao"))
            {
                hasReceivedKaiJuXiaoZhiTiao = tag.GetBool("hasReceivedKaiJuXiaoZhiTiao");
            }
        }
    }
}
