using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord;

namespace CalamityThrowingSpear.Weapons.DeveloperWeapons.Sagittarius
{
    public class YouShouldBeAgile : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.type != NPCID.HallowBoss)
            {
                return;
            }

            if (NPC.downedAncientCultist || CalamityThrowingSpearSystem.HasGivenSagittariusWeapon)
            {
                CompleteAllSagittariusEncounters();
                return;
            }

            Player player = GetRewardPlayer(npc);
            bool success = PlayerSucceeded(player) || AnyActivePlayerSucceeded();

            if (success)
            {
                SpawnReward(player, npc, ModContent.ItemType<Sagittarius>());
                CalamityThrowingSpearSystem.HasGivenSagittariusWeapon = true;

                if (!CalamityThrowingSpearSystem.HasGivenSagittariusTablet)
                {
                    SpawnReward(player, npc, ModContent.ItemType<XiaoZhiTiaoSG>());
                    CalamityThrowingSpearSystem.HasGivenSagittariusTablet = true;
                }

                Main.NewText(Language.GetTextValue("Mods.CalamityThrowingSpear.TheSpecialText.SagittariusPerfect"), 0, 255, 0);
            }
            else
            {
                if (!CalamityThrowingSpearSystem.HasGivenSagittariusTablet)
                {
                    SpawnReward(player, npc, ModContent.ItemType<XiaoZhiTiaoSG>());
                    CalamityThrowingSpearSystem.HasGivenSagittariusTablet = true;
                }

                Main.NewText(Language.GetTextValue("Mods.CalamityThrowingSpear.TheSpecialText.SagittariusFailed"), 255, 0, 0);
            }

            CompleteAllSagittariusEncounters();
        }

        private static Player GetRewardPlayer(NPC npc)
        {
            int playerIndex = npc.lastInteraction;
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers || !Main.player[playerIndex].active)
            {
                playerIndex = npc.target;
            }

            if (playerIndex >= 0 && playerIndex < Main.maxPlayers && Main.player[playerIndex].active)
            {
                return Main.player[playerIndex];
            }

            return null;
        }

        private static void SpawnReward(Player player, NPC npc, int itemType)
        {
            if (player != null)
            {
                player.QuickSpawnItem(npc.GetSource_Loot(), itemType, 1);
            }
            else
            {
                Item.NewItem(npc.GetSource_Loot(), npc.getRect(), itemType);
            }
        }

        private static bool PlayerSucceeded(Player player)
        {
            return player != null &&
                player.active &&
                player.GetModPlayer<BossChallengeBarPlayer>().SagittariusChallengeSucceeded;
        }

        private static bool AnyActivePlayerSucceeded()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (PlayerSucceeded(player))
                {
                    return true;
                }
            }

            return false;
        }

        private static void CompleteAllSagittariusEncounters()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    player.GetModPlayer<BossChallengeBarPlayer>().CompleteSagittariusEncounter();
                }
            }
        }
    }
}

