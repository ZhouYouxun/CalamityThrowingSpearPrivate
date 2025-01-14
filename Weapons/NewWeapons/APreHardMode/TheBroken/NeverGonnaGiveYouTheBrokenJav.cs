using CalamityMod.Items.TreasureBags.MiscGrabBags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken
{
    internal class NeverGonnaGiveYouTheBrokenJav : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            // 确保只针对 StarterBag 生效
            return entity.type == ModContent.ItemType<StarterBag>();
        }


        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            base.ModifyItemLoot(item, itemLoot);

            // 检查是否是 StarterBag
            if (item.type == ModContent.ItemType<StarterBag>())
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<TheBroken>(), 1));
            }
        }

    }
}