using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class TidalMechanicsBubblePLAYER : ModPlayer
    {
        private int bubbleCooldown = 0; // 计时器，初始化为0

        public override void ResetEffects()
        {
            // 每帧减少计时器，确保不会低于0
            if (bubbleCooldown > 0)
            {
                bubbleCooldown--;
            }
        }

        // 触发泡泡破裂后调用此方法开始计时
        public void StartBubbleCooldown()
        {
            bubbleCooldown = 600; // 10秒冷却（600帧）
        }

        // 检查冷却是否完成，返回是否可以生成新泡泡
        public bool CanSpawnBubble()
        {
            return bubbleCooldown <= 0;
        }
    }
}
