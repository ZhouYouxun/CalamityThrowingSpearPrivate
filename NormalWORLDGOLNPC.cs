//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria;
//using Terraria.ModLoader;
//using CalamityMod.NPCs;
//using CalamityMod.NPCs.ExoMechs.Thanatos;
//using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
//using CalamityMod.Balancing;

//namespace CalamityThrowingSpear
//{
//    public class ProjectileResistBalancingRule : IBalancingRule
//    {
//        public float DamageMultiplier;
//        public int[] ApplicableProjectileTypes;

//        public ProjectileResistBalancingRule(float damageMultiplier, params int[] projTypes)
//        {
//            DamageMultiplier = damageMultiplier;
//            ApplicableProjectileTypes = projTypes;
//        }

//        public bool AppliesTo(NPC npc, NPC.HitModifiers modifiers, Projectile? projectile)
//        {
//            return projectile is not null && ApplicableProjectileTypes.Contains(projectile.type);
//        }

//        public void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers)
//        {
//            modifiers.SourceDamage *= DamageMultiplier;
//        }
//    }


//    public class NormalWORLDGOLNPC : GlobalNPC
//    {
//        public static List<IBalancingRule> NPCSpecificBalancingChanges = new List<IBalancingRule>();

//        public override void Load()
//        {
//            // 添加规则，让 DragonRageJavPROJ 和 OrangeSLASH 对 ThanatosBody 和其他 NPC 造成特定比例的伤害
//            NPCSpecificBalancingChanges.AddRange(Bundle(
//                new[] { ModContent.NPCType<ThanatosBody2>(), ModContent.NPCType<ThanatosBody1>(), ModContent.NPCType<ThanatosTail>(), ModContent.NPCType<ThanatosHead>() },
//                Do(new ProjectileResistBalancingRule(0.006f, ModContent.ProjectileType<DragonRageJavPROJ>(), ModContent.ProjectileType<OrangeSLASH>()))
//            ));








//        }

//        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
//        {
//            foreach (var rule in NPCSpecificBalancingChanges)
//            {
//                if (rule.AppliesTo(npc, modifiers, projectile))
//                {
//                    rule.ApplyBalancingChange(npc, ref modifiers);
//                }
//            }
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

//        public bool AppliesTo(NPC npc, NPC.HitModifiers modifiers, Projectile? projectile)
//        {
//            return npc.type == TargetNPCID && Rule.AppliesTo(npc, modifiers, projectile);
//        }

//        public void ApplyBalancingChange(NPC npc, ref NPC.HitModifiers modifiers)
//        {
//            Rule.ApplyBalancingChange(npc, ref modifiers);
//        }
//    }

//}
