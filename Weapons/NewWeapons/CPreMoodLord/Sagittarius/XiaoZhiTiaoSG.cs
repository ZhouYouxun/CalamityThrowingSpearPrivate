using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Sagittarius
{
    public class XiaoZhiTiaoSG : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
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
                if (Main.keyState.PressingShift())
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "ShiftTooltip", GetHjsonText("Items.XiaoZhiTiaoSG.ShiftTipDetailed"))
                    { OverrideColor = Color.LightBlue });
                else
                    tooltips.Insert(index + 1, new TooltipLine(Mod, "NormalTooltip", GetHjsonText("Items.XiaoZhiTiaoSG.ShiftTip"))
                    { OverrideColor = Color.Gray });
            }
        }

        private static string GetHjsonText(string key) => Language.GetTextValue($"Mods.CalamityThrowingSpear.{key}");
    }
}
