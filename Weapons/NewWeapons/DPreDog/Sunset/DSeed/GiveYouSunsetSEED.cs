using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using CalamityMod.NPCs.Providence; // 引用 Providence
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed; // 引用 SunsetSEED

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
{
    public class GiveYouSunsetSEED : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // 目标是 Providence
            if (npc.type == ModContent.NPCType<Providence>())
            {
                // 只在夜晚才有可能掉落
                npcLoot.Add(ItemDropRule.ByCondition(new NightTimeCondition(),
                    ModContent.ItemType<SunsetSEED>(), 2));
                // 这里的 2 表示 1/2 概率掉落（50%）
            }
        }
    }

    // 自定义条件：夜晚时才成立
    public class NightTimeCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return !Main.dayTime; // 游戏内部的夜晚布尔
        }

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription() => "只会在夜晚击败 Providence 时掉落";
    }
}
