using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    public class SawBladeForkHornPBuff : ModBuff, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/SawBladeForkHornJav/SBFHBuff";

        public new string LocalizationCategory => "ModBuff";
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            int stackCount = player.GetModPlayer<SawBladeForkHornPlayer>().StackCount;
            player.statDefense += 5 * stackCount;
            player.endurance += 0.01f * stackCount;
        }
    }
}
