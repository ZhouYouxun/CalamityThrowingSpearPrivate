using Terraria;
using Terraria.ModLoader;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    // 仅负责：手持 Sunset 时，移除玩家的防御损伤（Defense Damage）
    public class SunsetPlayerClearDD : ModPlayer
    {
        public override void PostUpdate()
        {
            Player player = Player;

            // 未手持 Sunset，直接退出，不做任何事
            if (player.HeldItem.type != ModContent.ItemType<Sunset>())
                return;

            // 触发 Calamity 的“净化信号”
            // 等效于 AbsorberAura / BlueJellyAura / GreenJellyAura的防御损伤移除逻辑
            player.Calamity().CleansingEffect = 1;
        }
    }
}
