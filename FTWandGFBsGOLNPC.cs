using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.OldDuke;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.BansheeHookC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ViolenceC;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.ElectrocoagulationTenmonJav;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace CalamityThrowingSpear
{
    public class FTWandGFBsGOLNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 检查是否为 Bumblefuck（痴愚金龙） 或 Bumblefuck2（癫狂龙裔），以及是否为 ZenithWorld 模式
            if ((npc.type == ModContent.NPCType<Bumblefuck>() || npc.type == ModContent.NPCType<Bumblefuck2>()) &&
                Main.zenithWorld)
            {
                // 检查弹幕类型是否为 画龙点睛 系列
                if (projectile.type == ModContent.ProjectileType<FinishingTouchPROJ>() ||
                    projectile.type == ModContent.ProjectileType<FinishingTouchDASH>() ||
                    projectile.type == ModContent.ProjectileType<FinishingTouchBALL>())
                {
                    // 伤害 × 50
                    modifiers.SourceDamage *= 50f;
                }
            }


            // 检查 NPC 是否是 塔纳托斯
            if (npc.type == ModContent.NPCType<ThanatosBody2>() ||
                npc.type == ModContent.NPCType<ThanatosBody1>() ||
                npc.type == ModContent.NPCType<ThanatosTail>() ||
                npc.type == ModContent.NPCType<ThanatosHead>())
            {
                // 检查是否为指定的弹幕 巨龙之怒
                if (projectile.type == ModContent.ProjectileType<DragonRageJavPROJ>() ||
                    projectile.type == ModContent.ProjectileType<OrangeSLASH>() ||
                    projectile.type == ModContent.ProjectileType<DragonRageFuckYou>())
                {
                    // 将伤害降低为 60%
                    modifiers.FinalDamage *= 0.6f;
                }
                if (projectile.type == ModContent.ProjectileType<FinishingTouchDASHFuckYou>())
                {
                    modifiers.SourceDamage *= 0.25f;
                }
            }

            if (npc.type == ModContent.NPCType<AresBody>() ||
                npc.type == ModContent.NPCType<AresGaussNuke>() ||
                npc.type == ModContent.NPCType<AresLaserCannon>() ||
                npc.type == ModContent.NPCType<AresPlasmaFlamethrower>() ||
                npc.type == ModContent.NPCType<AresTeslaCannon>())
            {
                if (projectile.type == ModContent.ProjectileType<FinishingTouchDASHFuckYou>())
                {
                    modifiers.SourceDamage *= 0.5f;
                }
                if (projectile.type == ModContent.ProjectileType<DragonRageJavPROJ>() ||
                    projectile.type == ModContent.ProjectileType<OrangeSLASH>())
                {
                    // 将伤害降低为 60%
                    modifiers.FinalDamage *= 0.9f;
                }
            }

            /*if (npc.type == NPCID.WallofFleshEye || npc.type == NPCID.WallofFlesh)
            {
                if (projectile.type == ModContent.ProjectileType<ElectrocoagulationTenmonJavLight>())
                {
                    modifiers.SourceDamage *= 0.8f;
                }
            }*/

            if (npc.type == ModContent.NPCType<AstrumDeusHead>() ||
                npc.type == ModContent.NPCType<AstrumDeusBody>() ||
                npc.type == ModContent.NPCType<AstrumDeusTail>())
            {
                if (projectile.type == ModContent.ProjectileType<SurfeiterDrumINV>() ||
                    projectile.type == ModContent.ProjectileType<SurfeiterDrumINVBack>() ||
                    projectile.type == ModContent.ProjectileType<SurfeiterDrum>() ||
                    projectile.type == ModContent.ProjectileType<SurfeiterStonePillars>() ||
                    projectile.type == ModContent.ProjectileType<SurfeiterDrumINVEXP>())
                {
                    modifiers.SourceDamage *= 0.1f;
                }
            }







        }




    }
}
