//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System.Collections.Generic;
//using System.Linq;
//using Terraria;
//using Terraria.ModLoader;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
//{
//    public class SunsetSEEDTileSystem : ModSystem
//    {
//        // 记录每株植物已经经历了多少个“游戏日”
//        // key：左下角基准 Tile 坐标
//        private readonly Dictionary<Point, int> seedDays = new();

//        // 用于检测“新的一天开始”的锁
//        private bool wasDayTimeLastTick = false;

//        public void RegisterSeed(Point p)
//        {
//            if (!seedDays.ContainsKey(p))
//                seedDays[p] = 0;
//        }

//        public override void PostUpdateWorld()
//        {
//            // 判断是否刚刚从 夜晚 -> 白天
//            bool isNewDay =
//                Main.dayTime &&
//                !wasDayTimeLastTick &&
//                Main.time == 0;

//            wasDayTimeLastTick = Main.dayTime;

//            if (!isNewDay)
//                return;

//            // 新的一天开始，所有存活的植物推进一天
//            foreach (Point p in seedDays.Keys.ToList())
//            {
//                // Tile 不存在就清理，防止字典膨胀
//                if (!WorldGen.InWorld(p.X, p.Y) || !Main.tile[p.X, p.Y].HasTile)
//                {
//                    seedDays.Remove(p);
//                    continue;
//                }

//                // 最终阶段锁死，不再递增
//                if (seedDays[p] < 3)
//                    seedDays[p]++;
//            }
//        }

//        // 由 Tile.PreDraw 调用，在安全绘制阶段画
//        public void RenderAt(Point tilePos, SpriteBatch spriteBatch)
//        {
//            if (!seedDays.TryGetValue(tilePos, out int days))
//                return;

//            int stage = GetStage(days);
//            Texture2D texture = GetStageTexture(stage);

//            Vector2 worldPos = new Vector2(
//                (tilePos.X + 0.5f) * 16f,
//                tilePos.Y * 16f + 24f
//            );

//            spriteBatch.Draw(
//                texture,
//                worldPos - Main.screenPosition,
//                null,
//                Color.White,
//                0f,
//                texture.Size() * new Vector2(0.5f, 1f),
//                1f,
//                SpriteEffects.None,
//                0f
//            );
//        }

//        private int GetStage(int days)
//        {
//            // 0,1,2,3 四个阶段，最后阶段永久停留
//            return days >= 3 ? 3 : days;
//        }

//        private Texture2D GetStageTexture(int stage)
//        {
//            return stage switch
//            {
//                0 => ModContent.Request<Texture2D>(
//                    "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile1").Value,
//                1 => ModContent.Request<Texture2D>(
//                    "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile2").Value,
//                2 => ModContent.Request<Texture2D>(
//                    "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile3").Value,
//                _ => ModContent.Request<Texture2D>(
//                    "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile4").Value,
//            };
//        }

//        // 提供给后续“破坏掉落判断”的接口
//        public bool IsFinalStage(Point p)
//        {
//            return seedDays.TryGetValue(p, out int days) && days >= 3;
//        }
//    }
//}


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
{
    public class SunsetSEEDTileSystem : ModSystem
    {
        // 记录每株植物已经经历了多少个“游戏日”
        // key：左下角基准 Tile 坐标
        private readonly Dictionary<Point, int> seedDays = new();

        // 用于检测“新的一天开始”的锁
        private bool wasDayTimeLastTick = false;

        public void RegisterSeed(Point p)
        {
            if (!seedDays.ContainsKey(p))
                seedDays[p] = 0;
        }

        public override void PostUpdateWorld()
        {
            // 判断是否刚刚从 夜晚 -> 白天
            bool isNewDay =
                Main.dayTime &&
                !wasDayTimeLastTick &&
                Main.time == 0;

            wasDayTimeLastTick = Main.dayTime;

            if (!isNewDay)
                return;

            // 新的一天开始，所有存活的植物推进一天
            foreach (Point p in seedDays.Keys.ToList())
            {
                // Tile 不存在就清理，防止字典膨胀
                if (!WorldGen.InWorld(p.X, p.Y) || !Main.tile[p.X, p.Y].HasTile)
                {
                    seedDays.Remove(p);
                    continue;
                }

                // 最终阶段锁死，不再递增
                if (seedDays[p] < 3)
                    seedDays[p]++;
            }
        }

        // 由 Tile.PreDraw 调用，在安全绘制阶段画
        public void RenderAt(Point tilePos, SpriteBatch spriteBatch)
        {
            if (!seedDays.TryGetValue(tilePos, out int days))
                return;

            int stage = GetStage(days);
            Texture2D texture = GetStageTexture(stage);

            Vector2 worldPos = new Vector2(
                (tilePos.X + 0.5f) * 16f,
                tilePos.Y * 16f + 24f
            );

            spriteBatch.Draw(
                texture,
                worldPos - Main.screenPosition,
                null,
                Color.White,
                0f,
                texture.Size() * new Vector2(0.5f, 1f),
                1f,
                SpriteEffects.None,
                0f
            );
        }

        // ✅ 给 Tile 用：查询当前阶段（0~3），不存在则返回 false
        public bool TryGetStage(Point p, out int stage)
        {
            if (seedDays.TryGetValue(p, out int days))
            {
                stage = GetStage(days);
                return true;
            }

            stage = 0;
            return false;
        }

        private int GetStage(int days)
        {
            // 0,1,2,3 四个阶段，最后阶段永久停留
            return days >= 3 ? 3 : days;
        }

        private Texture2D GetStageTexture(int stage)
        {
            return stage switch
            {
                0 => ModContent.Request<Texture2D>(
                    "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile1").Value,
                1 => ModContent.Request<Texture2D>(
                    "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile2").Value,
                2 => ModContent.Request<Texture2D>(
                    "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile3").Value,
                _ => ModContent.Request<Texture2D>(
                    "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile4").Value,
            };
        }

        // 提供给后续“破坏掉落判断”的接口
        public bool IsFinalStage(Point p)
        {
            return seedDays.TryGetValue(p, out int days) && days >= 3;
        }
    }
}
