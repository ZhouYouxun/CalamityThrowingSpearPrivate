using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
public class PearlwoodJavPLAYER : ModPlayer
{
    private int hitCounter = 0; // 记录弹幕造成的总击中次数
    private int idleTimer = 0; // 记录没有造成伤害的时间

    public override void ResetEffects()
    {
        // 每帧增加计时器
        if (idleTimer > 0)
        {
            idleTimer--;
        }
        else
        {
            hitCounter = 0; // 如果5秒内没有任何弹幕造成伤害，重置计数器
        }
    }

    public void IncrementHitCounter()
    {
        hitCounter++; // 增加击中计数

        // 如果击中次数达到 X，清除所有相关弹幕
        if (hitCounter >= 600)
        {
            hitCounter = 0; // 重置计数器
            ClearAllProjectiles(); // 清除相关弹幕
        }

        // 每次造成伤害时，重置5秒计时器
        idleTimer = 60 * 5;
    }

    private void ClearAllProjectiles()
    {
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == Player.whoAmI &&
                (proj.type == ModContent.ProjectileType<PearlwoodJavPROJINV>() ||
                 proj.type == ModContent.ProjectileType<PearlwoodJavPROJ>() ||
                 proj.type == ModContent.ProjectileType<PearlwoodJavRainbowFront>() ||
                 proj.type == ModContent.ProjectileType<PearlwoodJavRainbowTrail>()))
            {
                //proj.Kill(); // 不执行清除弹幕的逻辑，因为现在的效果可以让他不产生大量的弹幕
                // 如果之后重新启用三段彩虹的话，可以考虑启用这一段
            }
        }
    }
}
