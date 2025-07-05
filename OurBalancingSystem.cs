//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System;
//using System.Collections.Generic;
//using CalamityMod;
//using Terraria.ModLoader;
//using CalamityMod.Balancing;
//using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
//using System.Collections.Generic;
//using System.Linq;
//using CalamityMod.NPCs.AstrumAureus;
//using CalamityMod.NPCs.CeaselessVoid;
//using CalamityMod.NPCs.Crabulon;
//using CalamityMod.NPCs.ExoMechs.Apollo;
//using CalamityMod.NPCs.ExoMechs.Artemis;
//using CalamityMod.NPCs.OldDuke;
//using CalamityMod.NPCs.ProfanedGuardians;
//using CalamityMod.NPCs.Providence;
//using CalamityMod.NPCs.Ravager;
//using CalamityMod.NPCs.SupremeCalamitas;
//using CalamityMod.NPCs.Yharon;
//using CalamityMod.Projectiles.DraedonsArsenal;
//using CalamityMod.Projectiles.Magic;
//using CalamityMod.Projectiles.Melee;
//using CalamityMod.Projectiles.Ranged;
//using CalamityMod.Projectiles.Rogue;
//using CalamityMod.Projectiles.Summon;
//using CalamityMod.Projectiles.Summon.MirrorofKalandraMinions;
//using CalamityMod.Projectiles.Typeless;
//using Terraria;
//using Terraria.ID;
//using static Terraria.ModLoader.ModContent;



//namespace CalamityThrowingSpear
//{
//    public class OurBalancingSystem
//    {
//        private static List<IBalancingRule> NPCSpecificBalancingChanges = new List<IBalancingRule>();

//        public static void ApplyBalancingRules()
//        {
//            // 添加一个规则，让 DragonRageJavPROJ 对 CalamityLists.ThanatosIDs 造成 60% 的伤害
//            NPCSpecificBalancingChanges.AddRange(Bundle(
//                CalamityLists.ThanatosIDs, // 目标 NPC 列表
//                Do(new ProjectileResistBalancingRule(0.6f, ModContent.ProjectileType<DragonRageJavPROJ>())) // 设置 60% 伤害倍率
//            ));
//        }

//        private static IEnumerable<IBalancingRule> Bundle(IEnumerable<int> targetIDs, params IBalancingRule[] rules)
//        {
//            foreach (int id in targetIDs)
//            {
//                foreach (IBalancingRule rule in rules)
//                {
//                    yield return new NPCSpecificBalancingRule(id, rule);
//                }
//            }
//        }


//        private static IBalancingRule Do(IBalancingRule rule) => rule;
//    }

//    public class NPCSpecificBalancingRule : IBalancingRule
//    {
//        public int TargetNPCID { get; }
//        public IBalancingRule Rule { get; }

//        public NPCSpecificBalancingRule(int targetNPCID, IBalancingRule rule)
//        {
//            TargetNPCID = targetNPCID;
//            Rule = rule;
//        }

//        // 实现 IBalancingRule 的相关方法和逻辑
//    }

//}



