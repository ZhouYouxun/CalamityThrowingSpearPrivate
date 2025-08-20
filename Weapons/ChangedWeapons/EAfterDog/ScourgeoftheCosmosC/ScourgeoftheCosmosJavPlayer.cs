using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.ScourgeoftheCosmosC
{
    public class ScourgeoftheCosmosJavPlayer : ModPlayer
    {
        public int X = 1; // 记录吞噬等级，默认值为 1
        private int appleSpawnTimer = 0; // 控制苹果生成的计时器

        public override void ResetEffects()
        {
            // 仅当玩家手持 ScourgeoftheCosmosJav 时，才保持 X
            if (Player.HeldItem.type != ModContent.ItemType<ScourgeoftheCosmosJav>())
            {
                X = 1; // 切换武器时重置吞噬等级
            }
        }

        public override void PostUpdate()
        {
            // 确保玩家持有武器时才生成苹果
            if (Player.HeldItem.type == ModContent.ItemType<ScourgeoftheCosmosJav>())
            {
                appleSpawnTimer++;
                if (appleSpawnTimer >= 45) // 每 X 秒生成一个苹果
                {
                    appleSpawnTimer = 0;
                    SpawnApple();
                    SoundEngine.PlaySound(SoundID.Item8, Player.Center);
                }
            }
        }

        private void SpawnApple()
        {
            int maxApples = X + 20; // 允许的最大苹果数量
            int appleCount = 0;

            // 计算当前存在的苹果数量
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<ScourgeoftheCosmosJavApple>())
                {
                    appleCount++;
                }
            }

            if (appleCount >= maxApples) return; // 超过上限不生成

            // 生成苹果
            Vector2 spawnPosition = Player.Center + new Vector2(Main.rand.NextFloat(-30*16f, 30*16f), -15*16f); // 从玩家头顶掉落
            Projectile.NewProjectile(Player.GetSource_FromThis(), spawnPosition, Vector2.UnitY * 1.5f,
                ModContent.ProjectileType<ScourgeoftheCosmosJavApple>(), 0, 0, Player.whoAmI);
        }

        public void IncreaseX()
        {
            X = (int)MathHelper.Clamp(X + 1, 1, 20); // 最大 20 级
            Player.statLife += 10; // 每次增加等级，回复 2 点生命值
            Player.HealEffect(10);
        }
    }
}
