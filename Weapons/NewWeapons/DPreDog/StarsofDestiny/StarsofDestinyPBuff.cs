using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    public class StarsofDestinyPBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true; // Buff 不会保存
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 启用 StarsofDestinyPBuffPlayer 的开关
            player.GetModPlayer<StarsofDestinyPBuffPlayer>().IsBuffActive = true;
        }


    }
}
