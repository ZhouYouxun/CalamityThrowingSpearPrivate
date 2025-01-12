using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.GameInput;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class RevelationKILLZUOJIANPlayer : ModPlayer
    {
        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {
            // 检测玩家是否手持 "Revelation" 武器
            if (Player.HeldItem != null && Player.HeldItem.type == ModContent.ItemType<Revelation>())
            {
                // 检测世界上是否存在飞行中的 "RevelationPROJ" 弹幕
                bool projectileExists = Main.projectile.Any(proj => proj.active && proj.type == ModContent.ProjectileType<RevelationPROJ>());

                // 如果存在该弹幕且玩家右键点击
                if (projectileExists && Main.mouseRight && Player.controlUseTile)
                {
                    // 清除所有 "RevelationPROJ" 弹幕
                    foreach (Projectile proj in Main.projectile.Where(proj => proj.active && proj.type == ModContent.ProjectileType<RevelationPROJ>()))
                    {
                        proj.Kill(); // 清除弹幕
                    }

                    // 生成 30 发随机方向的电能粒子特效
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 randomDirection = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f);
                        // 生成粒子特效（假设使用电能粒子类型）
                        Color electricColor = Color.Cyan; // 这里可以自定义颜色
                        Particle electricParticle = new SparkParticle(Player.Center, randomDirection, false, 60, Main.rand.NextFloat(0.8f, 1.2f), electricColor);
                        GeneralParticleHandler.SpawnParticle(electricParticle);
                    }
                }
            }
        }










    }
}
