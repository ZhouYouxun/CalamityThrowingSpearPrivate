using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Enums;


namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
{
    // 种子物品：种下后生成 SunsetPlant
    public class SunsetSEED : ModTile
    {
        // 4 阶段对应的贴图
        public static Texture2D[] StageTextures;

        // 用于记录每颗植物的放置时间（现实时间）
        public static Dictionary<Point16, DateTime> PlacedTimes = new();

        public override void Load()
        {
            StageTextures = new Texture2D[4];
            StageTextures[0] = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile1").Value;
            StageTextures[1] = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile2").Value;
            StageTextures[2] = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile3").Value;
            StageTextures[3] = ModContent.Request<Texture2D>(
                "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile4").Value;
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileCut[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(3, 2);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(
                AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidWithTop,
                TileObjectData.newTile.Width,
                0
            );
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.DrawYOffset = 3;

            TileObjectData.addTile(Type);

            AddMapEntry(new Color(150, 200, 150));
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            Point16 pos = new(i, j);
            PlacedTimes[pos] = DateTime.Now;
        }

        public override void KillTile(int i, int j,
            ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail)
            {
                Point16 pos = new(i, j);
                if (PlacedTimes.ContainsKey(pos))
                    PlacedTimes.Remove(pos);
            }
        }

        // ========================
        // 🌸 自定义绘制（模仿熔岩百合）
        // ========================
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            // 只在“Tile 中央那格”绘制整朵植物，避免重复
            if (tile.TileFrameX != 36 || tile.TileFrameY != 18)
                return;

            // 计算经过的现实时间
            int stage = 0;
            Point16 pos = new(i, j);

            if (PlacedTimes.TryGetValue(pos, out DateTime placed))
            {
                TimeSpan elapsed = DateTime.Now - placed;

                if (elapsed >= TimeSpan.FromHours(24 * 3))
                    stage = 3;
                else if (elapsed >= TimeSpan.FromHours(24 * 2))
                    stage = 2;
                else if (elapsed >= TimeSpan.FromHours(24 * 1))
                    stage = 1;
                else
                    stage = 0;
            }

            Texture2D tex = StageTextures[stage];

            // 模仿 LavaLily 的大贴图偏移与尺寸
            Rectangle source = new Rectangle(0, 0, 178, 184);
            Vector2 world = new Vector2(i, j).ToWorldCoordinates();
            Vector2 drawPos = world - Main.screenPosition + new Vector2(0, -12);
            Vector2 origin = new Vector2(98, 191); // 模仿熔岩百合

            spriteBatch.Draw(
                tex,
                drawPos,
                source,
                Lighting.GetColor(i, j),
                0f,
                origin,
                1f,
                SpriteEffects.None,
                0f
            );

            // ============================
            // 💠 掉落绿色粒子点缀（随机）
            // ============================
            if (Main.rand.NextBool(30)) // 随机概率
            {
                int dust = Dust.NewDust(world + new Vector2(0, -40), 20, 20, DustID.Grass, 0f, -0.6f, 0, default, 1.1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.2f;
            }
        }
    }
}