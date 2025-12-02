using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    // 控制“时间再生”Buff施加逻辑的 Player 类
    public class StarsofDestinyPBuffCTRL : ModPlayer
    {
        public override void PostUpdateEquips()
        {
            // 如果玩家当前手持的是命星武器，则持续给予时间再生Buff
            if (Player.HeldItem.type == ModContent.ItemType<StarsofDestiny>())
            {
                // 5 秒钟（300帧）持续时间，每帧都会刷新，相当于手持时无限时长
                Player.AddBuff(ModContent.BuffType<StarsofDestinyP2Buff>(), 300);
            }
        }
    }
}
