using CalamityMod.UI.CalamitasEnchants;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SoulSeekerJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SoulSeekerJav
{
    public class SCEnchantmentPlusSystem : ModSystem
    {
        public override void PostSetupContent()
        {
            // 检查是否加载了 CalamityMod，确保引用生效
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamityMod))
            {
                // 定义新的升级关系
                if (EnchantmentManager.ItemUpgradeRelationship != null)
                {
                    EnchantmentManager.ItemUpgradeRelationship[ModContent.ItemType<GildedProboscisJav>()] = ModContent.ItemType<SoulSeekerJav>();
                }
            }
        }
    }
}
