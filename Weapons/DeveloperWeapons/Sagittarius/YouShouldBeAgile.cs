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
            Player successfulPlayer = GetSuccessfulRewardPlayer(npc, player);
            bool success = successfulPlayer != null;

            if (success)
            {
                SpawnReward(successfulPlayer, npc, ModContent.ItemType<Sagittarius>());
                CalamityThrowingSpearSystem.HasGivenSagittariusWeapon = true;

                if (!CalamityThrowingSpearSystem.HasGivenSagittariusTablet)
                {
                    SpawnReward(successfulPlayer, npc, ModContent.ItemType<XiaoZhiTiaoSG>());
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
            Player player = GetActivePlayer(npc.lastInteraction);
            if (player != null)
            {
                return player;
            }

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                player = GetActivePlayer(i);
                if (PlayerInteractedWithNpc(npc, player))
                {
                    return player;
                }
            }

            return GetActivePlayer(npc.target);
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

        private static Player GetSuccessfulRewardPlayer(NPC npc, Player preferredPlayer)
        {
            if (PlayerSucceeded(preferredPlayer) && PlayerInteractedWithNpc(npc, preferredPlayer))
            {
                return preferredPlayer;
            }

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = GetActivePlayer(i);
                if (PlayerSucceeded(player) && PlayerInteractedWithNpc(npc, player))
                {
                    return player;
                }
            }

            return null;
        }

        private static bool PlayerInteractedWithNpc(NPC npc, Player player)
        {
            if (player == null || !player.active)
            {
                return false;
            }

            int playerIndex = player.whoAmI;
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
            {
                return false;
            }

            return npc.lastInteraction == playerIndex ||
                (npc.playerInteraction != null &&
                playerIndex < npc.playerInteraction.Length &&
                npc.playerInteraction[playerIndex]);
        }

        private static Player GetActivePlayer(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < Main.maxPlayers)
            {
                Player player = Main.player[playerIndex];
                if (player.active)
                {
                    return player;
                }
            }

            return null;
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

