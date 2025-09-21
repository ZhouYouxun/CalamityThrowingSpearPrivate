using System;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
{
    // 阶段1：1x1 幼苗
    public class SunsetSEEDTile1 : ModTile
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile1";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(
                AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 1, 0);

            // 放置时创建 TE
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(
                ModContent.GetInstance<SunsetSEEDTE>().Hook_AfterPlacement, -1, 0, true);

            TileObjectData.addTile(Type);
            AddMapEntry(new Color(240, 180, 120), CreateMapEntryName());
            DustType = DustID.Grass;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) =>
            ModContent.GetInstance<SunsetSEEDTE>().Kill(i, j);
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        { if (!fail) ModContent.GetInstance<SunsetSEEDTE>().Kill(i, j); }
    }

    // 阶段2：3x3
    public class SunsetSEEDTile2 : ModTile
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile2";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3; // ★ 别写 6
            TileObjectData.newTile.Origin = new Point16(1, 2); // 底部中间
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(
                AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 3, 0);

            TileObjectData.addTile(Type);
            AddMapEntry(new Color(230, 240, 130), CreateMapEntryName());
            DustType = DustID.Grass;
        }
    }

    // 阶段3：3x3
    public class SunsetSEEDTile3 : ModTile
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile3";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3; // ★ 修正
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(
                AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 3, 0);

            TileObjectData.addTile(Type);
            AddMapEntry(new Color(235, 245, 140), CreateMapEntryName());
            DustType = DustID.Grass;
        }
    }

    // 阶段4（最终）：3x3，破坏掉落奖励
    public class SunsetSEEDTile4 : ModTile
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile4";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3; // ★ 修正
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(
                AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 3, 0);

            TileObjectData.addTile(Type);
            AddMapEntry(new Color(255, 230, 130), CreateMapEntryName());
            DustType = DustID.Grass;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            var src = new EntitySource_TileBreak(i, j);
            Rectangle rect = new Rectangle(i * 16, j * 16, 16 * 3, 16 * 3);
            // TODO: 把奖励替换成真正道具
            Item.NewItem(src, rect, ModContent.ItemType<Sunset>(), 1);
            Item.NewItem(src, rect, ModContent.ItemType<SunsetSEED>(), 1);
        }
    }

    // ============ TileEntity：记录天数并按 15 天/阶段成长 =============
    public class SunsetSEEDTE : ModTileEntity
    {
        public const int DaysPerStage = 15; // 每阶段 15 天
        private int _daysPassed = 0;
        private bool _countedToday = false;

        public override bool IsTileValidForEntity(int i, int j) =>
            Framing.GetTileSafely(i, j).TileType == ModContent.TileType<SunsetSEEDTile1>();

        public int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 1);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type); return -1;
            }
            return Place(i, j);
        }

        public override void SaveData(TagCompound tag) => tag["days"] = _daysPassed;
        public override void LoadData(TagCompound tag) => _daysPassed = tag.GetInt("days");

        public override void Update()
        {
            // 每到黎明 +1 天
            if (Main.dayTime) { if (!_countedToday && Main.time <= 1.0) { _daysPassed++; _countedToday = true; } }
            else _countedToday = false;

            // 0→15→30→45 天
            if (_daysPassed == DaysPerStage * 1) TryGrow(ModContent.TileType<SunsetSEEDTile2>());
            else if (_daysPassed == DaysPerStage * 2) TryGrow(ModContent.TileType<SunsetSEEDTile3>());
            else if (_daysPassed == DaysPerStage * 3) TryGrow(ModContent.TileType<SunsetSEEDTile4>());
        }

        private void TryGrow(int tileType)
        {
            int i = Position.X, j = Position.Y;
            if (!HasSpaceFor3x3(i, j)) return;

            WorldGen.KillTile(i, j, noItem: true, effectOnly: false);
            bool placed = WorldGen.PlaceObject(i, j, tileType);
            if (placed && Main.netMode == NetmodeID.Server)
                NetMessage.SendObjectPlacement(-1, i, j, tileType, 0, 0, -1, -1);
        }

        private bool HasSpaceFor3x3(int i, int j)
        {
            int left = i - 1, top = j - 2;
            for (int x = left; x < left + 3; x++)
                for (int y = top; y < top + 3; y++)
                {
                    if (x == i && y == j) continue; // 幼苗自身
                    Tile t = Framing.GetTileSafely(x, y);
                    if (t.HasTile && !Main.tileCut[t.TileType]) return false;
                }
            return true;
        }
    }
}
