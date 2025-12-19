using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using CalamityMod.Particles; // 特效库：GeneralParticleHandler + 粒子类

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

            // 只在“基准块”绘制一次，避免 3×4 每格重复画
            if (tile.TileFrameX == (int)(Width * 0.5f) * 18 &&
                tile.TileFrameY == (Height - 1) * 18)
            {
                var system = ModContent.GetInstance<SunsetSEEDTileSystem>();
                Point p = new Point(i, j);

                // 画植物本体（由 System 负责）
                system.RenderAt(p, spriteBatch);

                // 根据阶段设置强度（同一个特效函数，不同强度）
                int stage = 0;
                system.TryGetStage(p, out stage);

                float intensity = stage switch
                {
                    0 => 0.25f,
                    1 => 0.45f,
                    2 => 0.70f,
                    _ => 1.00f
                };

                // 特效锚点：跟渲染锚点保持一致，再往上抬一点
                Vector2 anchor = new Vector2((i + 0.5f) * 16f, j * 16f + 24f) + new Vector2(0f, -38f);
                SpawnSeedVFX(anchor, intensity);
            }

            // 不让原生 Tile 自己画
            return false;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];

            // 只在“基准块”注册，避免重复
            if (tile.TileFrameX == (int)(Width * 0.5f) * 18 &&
                tile.TileFrameY == (Height - 1) * 18)
            {
                ModContent.GetInstance<SunsetSEEDTileSystem>().RegisterSeed(new Point(i, j));
            }
        }

        public override bool CanDrop(int i, int j) => false;

        // 同一个特效函数：强度越大越夸张（临时方案，后面好改）
        private static void SpawnSeedVFX(Vector2 anchorWorldPos, float intensity)
        {
            if (Main.gamePaused)
                return;

            // 距离太远就不刷，避免后台浪费
            float maxDist = 1200f;
            if (Vector2.DistanceSquared(Main.LocalPlayer.Center, anchorWorldPos) > maxDist * maxDist)
                return;

            // 刷新频率：强度越大越频繁
            int interval = (int)MathHelper.Lerp(18f, 6f, intensity);
            if (interval < 2)
                interval = 2;

            if (Main.GameUpdateCount % (uint)interval != 0u)
                return;

            // 用确定性角度做“有秩序”的旋绕（避免纯随机重复）
            float t = (float)Main.GameUpdateCount * 0.08f;
            float angle = t + anchorWorldPos.X * 0.003f + anchorWorldPos.Y * 0.002f;
            Vector2 dir = angle.ToRotationVector2();

            float radius = MathHelper.Lerp(8f, 34f, intensity);
            Vector2 spawnPos = anchorWorldPos + dir * radius;

            float speed = MathHelper.Lerp(0.4f, 2.2f, intensity);
            Vector2 vel = dir.RotatedBy(MathHelper.PiOver2) * (speed * 0.35f) + (-Vector2.UnitY * (0.5f + intensity));

            // 1) Spark 粒子：小火花拖尾（强度越大越亮、越大、寿命越长）
            Color sparkColor = Color.Lerp(Color.Orange, Color.OrangeRed, intensity);
            Particle spark = new SparkParticle(
                spawnPos,
                vel,
                false,
                18 + (int)(24f * intensity),
                0.55f + 1.15f * intensity,
                sparkColor
            );
            GeneralParticleHandler.SpawnParticle(spark);

            // 2) Bloom：偶尔来一下“热量脉冲”
            int bloomInterval = (int)MathHelper.Lerp(90f, 30f, intensity);
            if (bloomInterval < 10)
                bloomInterval = 10;

            if (Main.GameUpdateCount % (uint)bloomInterval == 0u)
            {
                Particle bloom = new GenericBloom(
                    anchorWorldPos,
                    Vector2.Zero,
                    Color.Lerp(Color.OrangeRed, Color.Gold, intensity),
                    0.6f + 1.2f * intensity,
                    18 + (int)(18f * intensity)
                );
                GeneralParticleHandler.SpawnParticle(bloom);
            }

            // 3) 原版 Dust：补一点细节火点（确保不报错）
            int dustType = DustID.Torch;
            Dust d = Dust.NewDustPerfect(
                spawnPos + Main.rand.NextVector2Circular(4f, 4f),
                dustType,
                vel * 0.25f,
                0,
                Color.Lerp(Color.Orange, Color.Red, intensity),
                0.8f + 0.9f * intensity
            );
            d.noGravity = true;
        }
    }
}
