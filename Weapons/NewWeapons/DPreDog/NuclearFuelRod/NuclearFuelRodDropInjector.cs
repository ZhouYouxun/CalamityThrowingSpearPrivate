using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using CalamityMod.NPCs.AcidRain; // 引用 Calamity NPC
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Items.Weapons.Melee; // 引用你的武器

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    public class NuclearFuelRodDropInjector : GlobalNPC
    {

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // 原先的逻辑：直接追加一个必定掉落的装备
            //if (npc.type == ModContent.NPCType<NuclearTerror>())
            //{
            //    // 添加掉落：武器每次必掉（可自行调整为概率掉落）
            //    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NuclearFuelRod>(), 1));
            //}


            // 试图更改新的逻辑，让这件装备并入他们现在的掉落物的池子里面，让他们概率掉落
            if (npc.type == ModContent.NPCType<NuclearTerror>())
            {
                // 添加一个三选一的掉落池：三者等概率掉落其一
                npcLoot.Add(ItemDropRule.OneFromOptions(
                    1, // 每次掉落1件
                    ModContent.ItemType<GammaHeart>(),
                    ModContent.ItemType<PhosphorescentGauntlet>(),
                    ModContent.ItemType<NuclearFuelRod>() // 新加入的武器
                ));
            }
        }


    }
}
