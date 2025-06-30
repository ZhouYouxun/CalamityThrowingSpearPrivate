using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    internal class NuclearFuelRodPlayer : ModPlayer
    {
        private bool wasAliveLastFrame = true;

        public override void PreUpdate()
        {
            if (!Player.dead && !wasAliveLastFrame)
            {
                wasAliveLastFrame = true;
            }

            if (Player.dead && wasAliveLastFrame)
            {
                // 玩家死亡瞬间触发
                wasAliveLastFrame = false;

                // 遍历玩家背包清除冷却
                for (int i = 0; i < Player.inventory.Length; i++)
                {
                    if (Player.inventory[i].ModItem is NuclearFuelRod rod && rod.cooldownTimer > 0)
                    {
                        rod.cooldownTimer = 0;
                    }
                }
            }
        }
    }
}
