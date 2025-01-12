using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameInput;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC
{
    public class GildedProboscisJavPlayer : ModPlayer
    {
        public bool rightClickTriggered = false; // 监听右键是否被按下

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Main.mouseRight) // 检测右键点击
            {
                rightClickTriggered = true;
            }
        }

        public override void PostUpdate()
        {
            if (rightClickTriggered)
            {
                Vector2 mousePosition = Main.MouseWorld; // 获取当前鼠标位置
                rightClickTriggered = false; // 重置右键监听状态

                // 遍历所有小鸟弹幕并发送冲刺命令
                foreach (var proj in Main.projectile)
                {
                    if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<GildedProboscisJavBIRD>())
                    {
                        GildedProboscisJavBIRD birdProj = proj.ModProjectile as GildedProboscisJavBIRD;
                        birdProj?.DashToPosition(mousePosition); // 触发小鸟的冲刺行为
                    }
                }
            }
        }
    }
}
