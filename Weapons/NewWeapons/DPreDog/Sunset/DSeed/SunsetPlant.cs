//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Terraria;
//using Terraria.DataStructures;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria.ObjectData;
//using Terraria.Enums;


//namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
//{
//    public class SunsetPlant : ModTile
//    {
//        private const int StageCount = 4;
//        private const int StageHeight = 48; // 3 tiles × 16px

//        public override void SetStaticDefaults()
//        {
//            Main.tileFrameImportant[Type] = true;
//            Main.tileLavaDeath[Type] = true;

//            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
//            TileObjectData.newTile.Width = 3;
//            TileObjectData.newTile.Height = 3;
//            TileObjectData.newTile.Origin = new Point16(1, 2);
//            TileObjectData.newTile.AnchorBottom =
//                new Terraria.DataStructures.AnchorData(
//                    AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidWithTop,
//                    3, 0);

//            TileObjectData.addTile(Type);

//            AddMapEntry(new Color(255, 200, 120));
//        }

//        public override void RandomUpdate(int i, int j)
//        {
//            Tile t = Main.tile[i, j];

//            // 获取当前阶段
//            int stage = t.TileFrameY / StageHeight;

//            // 最终阶段不再升级
//            if (stage >= StageCount - 1)
//                return;

//            int nextStage = stage + 1;
//            int nextFrameY = nextStage * StageHeight;

//            // 更新整株植物的 Frame
//            for (int x = 0; x < 3; x++)
//                for (int y = 0; y < 3; y++)
//                {
//                    Tile tile = Main.tile[i - 1 + x, j - 2 + y];
//                    tile.TileFrameY = (short)(nextFrameY + y * 16);
//                }
//        }

//        public override void KillMultiTile(int i, int j, int frameX, int frameY)
//        {
//            Tile t = Main.tile[i, j];
//            int stage = t.TileFrameY / StageHeight;

//            // 最终阶段掉落武器
//            if (stage == StageCount - 1)
//            {
//                Item.NewItem(new Terraria.DataStructures.EntitySource_TileBreak(i, j),
//                    i * 16, j * 16, 48, 48,
//                    ModContent.ItemType<Sunset>());
//            }
//        }


//        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
//        {
//            Tile t = Main.tile[i, j];

//            // 只在最上格绘制整张贴图
//            if (t.TileFrameX != 16 || t.TileFrameY % StageHeight != 0)
//                return true;

//            int stage = t.TileFrameY / StageHeight;

//            string tex = stage switch
//            {
//                0 => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile1",
//                1 => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile2",
//                2 => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile3",
//                _ => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile4"
//            };

//            Texture2D texture = ModContent.Request<Texture2D>(tex).Value;

//            Vector2 pos = new Vector2(i * 16 - 16, j * 16 - 32) - Main.screenPosition;

//            spriteBatch.Draw(texture, pos, Color.White);
//            return false;
//        }
//    }
//}
