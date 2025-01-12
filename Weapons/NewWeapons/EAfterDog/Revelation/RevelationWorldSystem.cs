using CalamityMod.Items.Weapons.Ranged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class RevelationWorldSystem : ModSystem
    {
        private int revelationCounter = 0; // 计数器，确保只生成一个Revelation

        public override void PostUpdateItems()
        {
            // 检查是否存在两个特定的掉落物
            Item ultimaItem = null;
            Item starmadaItem = null;

            foreach (Item item in Main.item)
            {
                if (item != null && item.active)
                {
                    if (item.type == ModContent.ItemType<Ultima>())
                    {
                        ultimaItem = item;
                    }
                    if (item.type == ModContent.ItemType<Starmada>())
                    {
                        starmadaItem = item;
                    }
                }

                // 如果两个掉落物同时存在，摧毁它们并生成一个新的掉落物
                if (ultimaItem != null && starmadaItem != null && revelationCounter == 0)
                {
                    Vector2 spawnPosition = (ultimaItem.Center + starmadaItem.Center) / 2f;

                    // 在摧毁前生成电能粒子特效
                    SpawnElectricParticles(ultimaItem.Center);
                    SpawnElectricParticles(starmadaItem.Center);

                    ultimaItem.active = false; // 摧毁Ultima
                    starmadaItem.active = false; // 摧毁Starmada

                    // 在两个掉落物的中间位置生成Revelation掉落物
                    Item.NewItem(null, spawnPosition, ModContent.ItemType<Revelation>());

                    revelationCounter++; // 增加计数器，确保只生成一个Revelation
                }
            }

            // 重置计数器，如果需要在某个条件下重新允许生成Revelation，可以在这里进行逻辑控制
            if (revelationCounter > 0)
            {
                // 这里可以添加重置计数器的条件逻辑（根据需求调整）
                revelationCounter = 0; // 如果需要每次重置，直接在每个更新后重置
            }
        }

        private void SpawnElectricParticles(Vector2 position)
        {
            int particleCount = 30; // 生成的粒子数量

            for (int i = 0; i < particleCount; i++)
            {
                // 生成随机化的方向和速度
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 6f);
                Color electricColor = Color.Lerp(Color.Cyan, Color.Blue, Main.rand.NextFloat()); // 电能颜色渐变

                // 创建粒子
                Particle electricParticle = new SparkParticle(
                    position,
                    velocity,
                    false,
                    Main.rand.Next(30, 60), // 粒子存活时间
                    Main.rand.NextFloat(0.5f, 1f), // 粒子大小
                    electricColor
                );

                GeneralParticleHandler.SpawnParticle(electricParticle);
            }
        }
    }

}
