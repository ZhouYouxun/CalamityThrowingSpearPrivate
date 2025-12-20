// SunsetSEEDTileSystem.cs
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
        // key：左上角 Tile 坐标
        private readonly Dictionary<Point, int> seedDays = new();

        private bool wasDayTimeLastTick = false;

        public void RegisterSeed(Point topLeft)
        {
            if (!seedDays.ContainsKey(topLeft))
                seedDays[topLeft] = 0;
        }

        public override void PostUpdateWorld()
        {
            bool isNewDay =
                Main.dayTime &&
                !wasDayTimeLastTick &&
                Main.time == 0;

            wasDayTimeLastTick = Main.dayTime;

            if (!isNewDay)
                return;

            foreach (Point p in seedDays.Keys.ToList())
            {
                if (!WorldGen.InWorld(p.X, p.Y) || !Main.tile[p.X, p.Y].HasTile)
                {
                    seedDays.Remove(p);
                    continue;
                }

                if (seedDays[p] < 3)
                    seedDays[p]++;
            }
        }

        public bool TryGetStage(Point topLeft, out int stage)
        {
            if (seedDays.TryGetValue(topLeft, out int days))
            {
                stage = GetStage(days);
                return true;
            }

            stage = 0;
            return false;
        }

        private int GetStage(int days) => days >= 3 ? 3 : days;

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

        // === 你校对好的【阶段偏移轨迹（Tile 单位）】===
        private Vector2 GetStageOffsetTiles(int stage)
        {
            return stage switch
            {
                0 => new Vector2(12f, 12.6f),  // 幼苗
                1 => new Vector2(12f, 10f),
                2 => new Vector2(12.2f, 8.3f),
                _ => new Vector2(12.2f, 7.5f)  // 完全体
            };
        }

        // === 世界坐标中心（唯一真源）===
        public Vector2 GetWorldCenterForDraw(Point topLeftTile, int stage)
        {
            Vector2 stageOffset = GetStageOffsetTiles(stage);

            return new Vector2(
                (topLeftTile.X + SunsetSEEDTile.Width * 0.5f) * 16f + stageOffset.X * 16f,
                (topLeftTile.Y + SunsetSEEDTile.Height * 0.5f) * 16f + stageOffset.Y * 16f
            );
        }

        // === 核心绘制入口 ===
        public void RenderAt(Point topLeftTile, SpriteBatch spriteBatch)
        {
            if (!seedDays.TryGetValue(topLeftTile, out int days))
                return;

            int stage = GetStage(days);
            Texture2D plantTex = GetStageTexture(stage);

            Vector2 worldCenter = GetWorldCenterForDraw(topLeftTile, stage);
            Vector2 drawPos = worldCenter - Main.screenPosition;

            float time = (float)Main.GlobalTimeWrappedHourly;

            // =============================
            // ① 呼吸描边（蓝 ↔ 黄）
            // =============================
            float pulse = 0.5f + 0.5f * (float)global::System.Math.Sin(time * 2.2f);
            Color outlineColor = Color.Lerp(Color.Cyan, Color.Gold, pulse);

            for (int k = 0; k < 6; k++)
            {
                float angle = MathHelper.TwoPi / 6f * k;
                Vector2 offset = angle.ToRotationVector2() * (2f + pulse * 2f);

                spriteBatch.Draw(
                    plantTex,
                    drawPos + offset,
                    null,
                    outlineColor * 0.75f,
                    0f,
                    plantTex.Size() * 0.5f,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            // =============================
            // ② 植物本体
            // =============================
            spriteBatch.Draw(
                plantTex,
                drawPos,
                null,
                Color.White,
                0f,
                plantTex.Size() * 0.5f,
                1f,
                SpriteEffects.None,
                0f
            );

            //// =============================
            //// ③ 魔法绘制（flare2_003 ×3）
            //// =============================
            //Texture2D flare = ModContent.Request<Texture2D>(
            //    "CalamityThrowingSpear/Texture/SuperTexturePack/flare2_003").Value;

            //for (int i = 0; i < 3; i++)
            //{
            //    float rot = time * (0.6f + i * 0.4f) + i;
            //    float scale = 0.055f + pulse * (0.15f + i * 0.05f);
            //    Color magicColor = Color.Lerp(Color.Cyan, Color.Lime, 0.5f + 0.5f * pulse);

            //    spriteBatch.Draw(
            //        flare,
            //        drawPos,
            //        null,
            //        magicColor * 0.6f,
            //        rot,
            //        flare.Size() * 0.5f,
            //        scale,
            //        SpriteEffects.None,
            //        0f
            //    );
            //}
        }

        public bool IsFinalStage(Point p)
        {
            return seedDays.TryGetValue(p, out int days) && days >= 3;
        }
    }
}
