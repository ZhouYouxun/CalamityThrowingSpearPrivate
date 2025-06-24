using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    internal class StarsofDestinyPBuffPlayer : ModPlayer
    {
        public bool IsBuffActive; // 用于控制 Buff 是否启用

        public override void ResetEffects()
        {
            // 每帧重置开关，确保 Buff 状态不会永久保持
            IsBuffActive = false;
        }
        public override void PreUpdate()
        {
            if (IsBuffActive)
            {
                // 提升玩家的速度和飞行能力
                //Player.velocity *= 1.3f; // 提升移动速度
                //Player.accRunSpeed *= 1.2f; // 提升跑步加速度
                //Player.wingAccRunSpeed *= 1.5f; // 提升飞行时的加速度
                //Player.wingTimeMax = (int)(Player.wingTimeMax * 1.75f); // 提升飞行时长

                Player.moveSpeed += 10.25f;
                Player.runAcceleration *= 2.25f;
                Player.maxRunSpeed *= 2.25f;
            }
        }
        public override void PostUpdateMiscEffects()
        {


        }
    }
}
