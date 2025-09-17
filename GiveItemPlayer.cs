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

        // 新增：进入世界后消息延迟计时器（单位：tick，60 = 1秒）
        private int worldEnterMessageTimer = -1;

        // 玩家进入游戏时触发
        public override void OnEnterWorld()
        {
            // 每次进入世界都启动计时器（3秒 = 180 tick）
            worldEnterMessageTimer = 180;

            if (!hasReceivedKaiJuXiaoZhiTiao)
            {
                // 发放 KaiJuXiaoZhiTiao 物品
                Player.QuickSpawnItem(Player.GetSource_Misc("CalamityThrowingSpear"), ModContent.ItemType<KaiJuXiaoZhiTiao>());

                // 设置状态，防止重复发放
                hasReceivedKaiJuXiaoZhiTiao = true;
            }
        }

        public override void PostUpdate()
        {
            if (worldEnterMessageTimer > 0)
            {
                worldEnterMessageTimer--;

                if (worldEnterMessageTimer == 0)
                {
                    // 3秒后播放分两行的提示
                    Main.NewText("这个模组已经正式完结，以后将不会再有更大的更新", Color.OrangeRed);
                    Main.NewText("欢迎关注作者的下一个模组", Color.OrangeRed);
                }
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
