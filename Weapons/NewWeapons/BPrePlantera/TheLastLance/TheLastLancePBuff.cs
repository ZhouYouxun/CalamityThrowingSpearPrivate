using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLancePBuff : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "ModBuff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false; // 这个Buff不是一个减益效果
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 每一帧给玩家回复 1 点生命值
            player.statLife += 1;

            // 确保玩家的生命值不超过最大生命值的 25%
            int maxLife25Percent = player.statLifeMax2 / 4;
            if (player.statLife > maxLife25Percent)
            {
                player.statLife = maxLife25Percent;
                player.DelBuff(buffIndex); // 当恢复到最大生命值的 25% 时，移除 Buff
            }

            // 在玩家头顶显示绿色的 "1"
            CombatText.NewText(player.getRect(), Color.Green, "1", dramatic: false, dot: false);
        }
    }
}

