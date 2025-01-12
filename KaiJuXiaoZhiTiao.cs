using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear
{
    public class KaiJuXiaoZhiTiao : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            //Item.accessory = true;
            // 困难模式前：Orange，价值15金
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 15, 0, 0);
            Item.value = Item.sellPrice(0, 15, 0, 0);

        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int index = tooltips.FindLastIndex(x => x.Mod == "Terraria" && x.Name.StartsWith("Tooltip"));
            if (index != -1)
            {
                if (Main.keyState.PressingShift()) // 检测按下Shift键
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "ShiftTooltip", GetHjsonText("Items.KaiJuXiaoZhiTiao.ShiftTipDetailed"))
                    { OverrideColor = Color.LightBlue });
                else
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "NormalTooltip", GetHjsonText("Items.KaiJuXiaoZhiTiao.ShiftTip"))
                    { OverrideColor = Color.Gray });
            }


            //// 获取当前玩家
            //Player player = Main.LocalPlayer;

            //// 获取并显示跳跃速度
            //string jumpSpeedText = $"当前跳跃速度: {player.jumpSpeedBoost:F2}";
            //tooltips.Add(new TooltipLine(Mod, "JumpSpeed", jumpSpeedText) { OverrideColor = Color.LightGreen });

            //// 获取并显示飞行时间（例如翅膀的飞行时间）
            //string wingTimeText = $"当前飞行时间: {player.wingTime:F2} 秒";
            //tooltips.Add(new TooltipLine(Mod, "WingTime", wingTimeText) { OverrideColor = Color.LightBlue });

            //// 显示移动速度
            //string moveSpeedText = $"当前移动速度: {player.moveSpeed * 100:F2}%";
            //tooltips.Add(new TooltipLine(Mod, "MoveSpeed", moveSpeedText) { OverrideColor = Color.Yellow });

            //// 显示其他属性...
            //string gravityText = $"当前重力: {player.gravity:F2}";
            //tooltips.Add(new TooltipLine(Mod, "Gravity", gravityText) { OverrideColor = Color.Orange });
        }

        private static string GetHjsonText(string key) => Language.GetTextValue($"Mods.CalamityThrowingSpear.{key}");

    }
}