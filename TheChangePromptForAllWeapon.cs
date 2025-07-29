using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear
{
    internal class TheChangePromptForAllWeapon : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // 遍历 WeaponSetB 中的每个类型，判断当前物品是否匹配
            foreach (var type in SwitchWeapons.WeaponSetB)
            {
                if (item.ModItem != null && item.ModItem.GetType() == type)
                {
                    // 添加本地化提示文本
                    tooltips.Add(new TooltipLine(Mod, "ReworkPrompt", Language.GetTextValue("Mods.CalamityThrowingSpear.TheSpecialText.ReworkPrompt"))
                    {
                        OverrideColor = Microsoft.Xna.Framework.Color.LightSkyBlue
                    });
                    break;
                }
            }
        }
    }
}