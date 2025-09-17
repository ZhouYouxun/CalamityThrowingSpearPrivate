using System;
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
    // ===============================
    // ① 1×1 幼苗 Tile（使用 SunsetSEEDTile1.png）
    // - 放置时自动创建 TileEntity 计时
    // - 60 个“游戏日”后尝试长成 3×3 成熟 Tile
    // ===============================
    public class SunsetSEEDTile1 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;   // 帧重要，不合并
            Main.tileNoAttach[Type] = true;         // 不允许被其他块附着
            Main.tileLavaDeath[Type] = true;        // 岩浆会破坏

            // 放置数据：1×1，底部需要坚实地面（类似草/土/石）
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorBottom = new AnchorData(
                AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 1, 0);

            // 放置后由 TileEntity 记录“天数”
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(
                ModContent.GetInstance<SunsetSEEDTE>().Hook_AfterPlacement, -1, 0, true);

            TileObjectData.addTile(Type);

            AddMapEntry(new Color(240, 180, 120), CreateMapEntryName());
            DustType = DustID.Grass;
        }

        // 幼苗被破坏时，记得清除对应的 TE
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // 1×1 也会走 KillMultiTile（兼容性写法）
            ModContent.GetInstance<SunsetSEEDTE>().Kill(i, j);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            // 有时 1×1 更常走 KillTile，这里也清理一次
            if (!fail)
                ModContent.GetInstance<SunsetSEEDTE>().Kill(i, j);
        }
    }

    // ===============================
    // ② 3×3 成熟 Tile（使用 SunsetSEEDTile2.png）
    // - Origin 设为 (1,2)：即以“底部中间”为原点，便于由幼苗位置直接长成
    // - 被破坏时掉落奖励
    // ===============================
    public class SunsetSEEDTile2 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            // 3×3 配置（每格 16px 高）
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 2); // 底部中间
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };

            // 成熟植株底部 3 格都需要有支撑
            TileObjectData.newTile.AnchorBottom = new AnchorData(
                AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 3, 0);

            TileObjectData.addTile(Type);

            AddMapEntry(new Color(255, 230, 130), CreateMapEntryName());
            DustType = DustID.Grass;
        }

        // 成熟体被破坏时掉落奖励物品
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // 掉落范围：3×3
            var source = new EntitySource_TileBreak(i, j);
            Rectangle rect = new Rectangle(i * 16, j * 16, 16 * 3, 16 * 3);

            // TODO：把奖励替换成你真正想要的物品
            int reward = ModContent.ItemType<SunsetSEED>(); // 先用“再得一颗种子”做示范
            Item.NewItem(source, rect, ModContent.ItemType<Sunset>(), 1);
            Item.NewItem(source, rect, reward, 1);
        }
    }

    // ===============================
    // ③ TileEntity：记录“天数”并在 60 天后长成 3×3
    // - 每到黎明（白天开始且 Main.time == 0）计数 +1
    // - 满足 60 天且空间充足时：用 3×3 成熟 Tile 替换
    // ===============================
    public class SunsetSEEDTE : ModTileEntity
    {
        // 可调：成熟所需的“游戏日”数量
        public const int DaysToMature = 60;

        // 已累计“天数”
        private int _daysPassed = 0;

        // 防重复计数：每日只在黎明 +1 次
        private bool _countedToday = false;

        // ——保存/读取，保证退出重进仍保留进度——
        public override void SaveData(TagCompound tag)
        {
            tag["days"] = _daysPassed;
        }
        public override void LoadData(TagCompound tag)
        {
            _daysPassed = tag.GetInt("days");
        }

        // 该实体是否仍然“绑定”在正确的幼苗 Tile 上
        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile t = Framing.GetTileSafely(i, j);
            return t.HasTile && t.TileType == ModContent.TileType<SunsetSEEDTile1>();
        }

        // 放置后由 Hook 调用，负责创建 TE（含联机同步）
        public int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // 客户端：请求服务器创建
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 1);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
                return -1;
            }
            // 单机/服务器：直接创建
            return Place(i, j);
        }

        public override void Update()
        {
            // ——计天数：在“白天且时间刚刚归零（黎明）”的时刻 +1——
            if (Main.dayTime)
            {
                // 白天开始时 Main.time 会重置为 0
                if (!_countedToday && Main.time <= 1.0)
                {
                    _daysPassed++;
                    _countedToday = true;
                }
            }
            else
            {
                // 夜晚：等待下一次黎明
                _countedToday = false;
            }

            // ——满足条件则尝试长成——
            if (_daysPassed >= DaysToMature)
            {
                TryMature();
            }

            // ⚡ 调试用：每过 60 tick（=1秒）就成熟
            //if (Main.GameUpdateCount % 60 == 0)
            //{
            //    TryMature();
            //}

        }

        // 尝试把 1×1 幼苗替换为 3×3 成熟植株
        private void TryMature()
        {
            int i = Position.X;
            int j = Position.Y;

            // 3×3 的原点在“底部中间”(1,2)，因此以 i,j 作为原点即可
            // 需要确认 3×3 范围是否有空间（除原幼苗格外都应为空）
            if (!HasSpaceFor3x3(i, j))
                return; // 空间不够，改天再试

            // 先干净地移除幼苗格，避免掉落：noItem = true
            WorldGen.KillTile(i, j, noItem: true, effectOnly: false);
            // 注意：此时本 TE 将失效，但当前函数仍会继续执行到结束（足够完成下一步）

            // 尝试放置 3×3 成熟体
            bool placed = WorldGen.PlaceObject(i, j, ModContent.TileType<SunsetSEEDTile2>());
            if (placed && Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendObjectPlacement(-1, i, j, ModContent.TileType<SunsetSEEDTile2>(), 0, 0, -1, -1);
            }
        }

        // 检查以 (i,j) 为“底部中间原点”的 3×3 是否有空间
        private bool HasSpaceFor3x3(int i, int j)
        {
            // 3×3 左上角
            int left = i - 1;
            int top = j - 2;

            for (int x = left; x < left + 3; x++)
            {
                for (int y = top; y < top + 3; y++)
                {
                    // 幼苗本身所在的 (i,j) 可以忽略
                    if (x == i && y == j)
                        continue;

                    Tile t = Framing.GetTileSafely(x, y);
                    if (t.HasTile && !Main.tileCut[t.TileType]) // 碰到非“可切割植物”的方块就不行
                        return false;
                }
            }
            return true;
        }

        public override void OnKill()
        {
            // TE 被移除时的收尾（此处无需额外逻辑）
        }





    }
}
