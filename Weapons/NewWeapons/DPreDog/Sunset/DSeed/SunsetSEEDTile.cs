// SunsetSEEDTile.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using CalamityMod.Particles; // GeneralParticleHandler + 粒子类

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.DSeed
{
    public class SunsetSEEDTile : ModTile
    {
        public const int Width = 3;
        public const int Height = 4;

        public override string Texture =>
            "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/DSeed/SunsetSEEDTile1";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Origin = new Point16(Width / 2, Height - 1);
            TileObjectData.newTile.AnchorBottom =
                new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, Width, 0);
            TileObjectData.newTile.CoordinateHeights =
                new int[Height] { 16, 16, 16, 16 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(255, 120, 80));
            HitSound = SoundID.Grass;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            // 只在“左上角那一格”绘制一次，避免 3×4 每格重复画
            if (tile.TileFrameX % (Width * 18) != 0 ||
                tile.TileFrameY % (Height * 18) != 0)
            {
                return false;
            }

            // i, j 就是左上角
            Point topLeft = new Point(i, j);

            var system = ModContent.GetInstance<SunsetSEEDTileSystem>();

            // 双保险：确保字典有记录（NearbyEffects 也会注册，但这里不赌调用顺序）
            system.RegisterSeed(topLeft);

            // 画植物本体（中心对齐已在 System 内处理）
            system.RenderAt(topLeft, spriteBatch);

            // 刷蓝绿迷幻特效：覆盖范围随阶段扩张
            if (system.TryGetStage(topLeft, out int stage))
                SpawnSeedVFX(topLeft, stage, system);

            // 不让原生 Tile 自己画（否则叠两套）
            return false;
        }


        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];

            // 与 PreDraw 同步：只在左上角注册一次
            if (tile.TileFrameX % (Width * 18) != 0 ||
                tile.TileFrameY % (Height * 18) != 0)
            {
                return;
            }

            ModContent.GetInstance<SunsetSEEDTileSystem>().RegisterSeed(new Point(i, j));
        }

        public override bool CanDrop(int i, int j) => false;

        // ====== 阶段特效覆盖范围（单位：Tile）======
        // Stage0：3×4
        // Stage1：3×5
        // Stage2：5×7
        // Stage3：5×10
        private static readonly Point[] StageVFXAreaTiles =
        {
            new Point(3, 4),
            new Point(3, 5),
            new Point(5, 7),
            new Point(5, 10)
        };

        private static float Frac(float x) => x - (float)Math.Floor(x);

        private static void SpawnSeedVFX(Point topLeftTile, int stage, SunsetSEEDTileSystem system)
        {
            if (Main.gamePaused)
                return;

            stage = Utils.Clamp(stage, 0, 3);

            // 以“贴图绘制中心”为核心（与你的 RenderAt 完全同源），确保永远对齐
            Vector2 worldCenter =
                system.GetWorldCenterForDraw(topLeftTile, stage)
                + new Vector2(-12f * 16f, -12f * 16f);

            // 距离太远就不刷，避免后台浪费
            float maxDist = 1200f;
            if (Vector2.DistanceSquared(Main.LocalPlayer.Center, worldCenter) > maxDist * maxDist)
                return;

            // 覆盖范围（像素）：阶段越高越大
            Point areaTiles = StageVFXAreaTiles[stage];
            float halfW = areaTiles.X * 16f * 0.5f;
            float halfH = areaTiles.Y * 16f * 0.5f;

            // 频率：约“两帧一针”
            if (Main.GameUpdateCount % 2u != 0u)
                return;

            // 数量：明确收敛（主次清晰）
            // 每次触发只喷少量点，但阶段越高覆盖更大
            int count = stage switch
            {
                0 => 3, // 3×4：很克制
                1 => 7, // 3×5
                2 => 9, // 5×7
                _ => 10  // 5×10：最多也就 4 个点/两帧
            };

            // 蓝绿迷幻主色（你要的同源体系）
            Color cA = new Color(40, 255, 220);  // 青绿
            Color cB = new Color(80, 140, 255);  // 青蓝

            // “数学秩序”采样：确定性铺点 + 呼吸相位
            float t = (float)Main.GameUpdateCount * 0.06f;
            ulong step = Main.GameUpdateCount / 2u;

            for (int n = 0; n < count; n++)
            {
                float s = (float)(step * (ulong)count + (ulong)n);

                // 确定性铺点（不靠 Main.rand）：黄金比序列
                float u = Frac(s * 0.6180339f);
                float v = Frac(s * 0.3236068f);

                // 覆盖矩形内的点（阶段越高范围越大）
                float x = MathHelper.Lerp(-halfW, halfW, u);
                float y = MathHelper.Lerp(-halfH, halfH, v);

                // 轻微旋序扰动：让点“活着”，但不乱
                float a = s * 2.39996323f + t;
                Vector2 swirl = new Vector2((float)Math.Cos(a), (float)Math.Sin(a * 1.27f + 0.9f));
                Vector2 pos = worldCenter + new Vector2(x, y) + swirl * (2f + stage * 1.0f);

                // 色彩呼吸：青绿↔青蓝
                float pulse = 0.5f + 0.5f * (float)Math.Sin(t * 1.2f + s * 0.35f);
                Color c = Color.Lerp(cA, cB, pulse);

                // 速度：上扬+轻微发散（非常轻，避免喧宾夺主）
                Vector2 toOut = pos - worldCenter;
                if (toOut.LengthSquared() < 0.001f)
                    toOut = Vector2.UnitY;
                else
                    toOut.Normalize();

                Vector2 vel = (-Vector2.UnitY * (0.35f + 0.08f * stage)) + toOut * (0.05f + 0.02f * stage);

                // ===== 9.EXO之光（SquishyLightParticle）=====
                // 低数量但很亮：作为“主光”
                SquishyLightParticle exoEnergy = new(
                    pos,
                    vel,
                    0.20f + 0.03f * stage,                 // scale：阶段越高略大
                    c,                                     // color：蓝绿迷幻
                    18 + stage * 6,                        // lifetime：略随阶段增加
                    opacity: 1f,
                    squishStrenght: 1f,
                    maxSquish: 3f,
                    hueShift: 0f
                );
                GeneralParticleHandler.SpawnParticle(exoEnergy);

                // ===== 10.辉光球（GlowOrbParticle）=====
                // 作为“辅光点缀”：快、清爽、短命
                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    6,                                     // lifetime：短促
                    0.55f + 0.08f * stage,                 // scale：阶段越高略大
                    c,
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }
        }





    }
}
