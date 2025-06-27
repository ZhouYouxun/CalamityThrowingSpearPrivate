using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using CalamityMod.NPCs.AcidRain; // 引用 Calamity NPC
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod; // 引用你的武器

namespace CalamityThrowingSpear.Systems
{
    public class NuclearFuelRodDropInjector : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // 确保是 Nuclear Terror
            if (npc.type == ModContent.NPCType<NuclearTerror>())
            {
                // 添加掉落：武器每次必掉（可自行调整为概率掉落）
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NuclearFuelRod>(), 1));
            }
        }
    }
}
