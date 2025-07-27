using CalamityMod;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Tiles.Astral;
using CalamityMod.NPCs.Polterghast;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    /*public class GiveYouStarsofDestiny : GlobalNPC
    {
        private static bool hasGivenXiaoZhiTiaoSoD = false; // 静态开关，限制当前世界内只触发一次

        public override bool InstancePerEntity => true; // 每个 NPC 实例独立处理

        // 检测 NPC 死亡逻辑
        public override void OnKill(NPC npc)
        {
            // 检查是否为 Bumblefuck 且奖励未被触发过
            if (npc.type == ModContent.NPCType<Polterghast>() && !hasGivenXiaoZhiTiaoSoD)
            {
                hasGivenXiaoZhiTiaoSoD = true; // 标记物品已触发

                // 获取当前玩家对象
                Player player = Main.LocalPlayer;

                // 给予物品 XiaoZhiTiaoFT
                player.QuickSpawnItem(null, ModContent.ItemType<XiaoZhiTiaoSoD>(), 1);

                // 显示提示文本
                Main.NewText("命运开始变动……", Color.White);
            }
        }
    }*/
    public class GiveYouStarsofDestiny : GlobalTile
    {
        public override void RightClick(int i, int j, int type)
        {
            base.RightClick(i, j, type);

            // 检查当前 Tile 类型是否为 AstralBeacon
            if (type == ModContent.TileType<AstralBeacon>())
            {
                Player player = Main.LocalPlayer;

                // 检查是否已击败 Polterghast
                if (!DownedBossSystem.downedPolterghast)
                    return; // 未击败 Polterghast，直接返回

                // 检查玩家是否手持 PlatinumWatch 或 GoldWatch
                if (player.HeldItem.type != ItemID.PlatinumWatch && player.HeldItem.type != ItemID.GoldWatch)
                    return; // 如果不符合条件，直接返回

                // 给予玩家 StarsofDestiny
                player.QuickSpawnItem(player.GetSource_FromThis(), ModContent.ItemType<StarsofDestiny>());

                // 播放音效
                SoundEngine.PlaySound(SoundID.Item4, player.Center);

                // 显示提示文字
                CombatText.NewText(player.getRect(), Color.White, "收下这个！", true, false);

                // 生成粒子特效
                for (int k = 0; k < 20; k++)
                {
                    Vector2 particleVelocity = Main.rand.NextVector2Circular(3f, 3f);
                    Dust.NewDustPerfect(player.Center, DustID.WhiteTorch, particleVelocity, 150, Color.White, Main.rand.NextFloat(1f, 1.5f)).noGravity = true;
                }

                // 删除玩家手持的 PlatinumWatch【铂金表】 或 GoldWatch【金表】
                player.HeldItem.stack--;
                if (player.HeldItem.stack <= 0)
                    player.inventory[player.selectedItem].TurnToAir();
            }
        }
    }
}
