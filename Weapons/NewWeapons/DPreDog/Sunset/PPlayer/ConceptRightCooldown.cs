using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    public class ConceptRightCooldown : ModPlayer
    {
        public int conceptRightCooldown = 0;
        public const int ConceptCooldownMax = 600; // 10 秒

        // 是否在冷却中
        public bool IsConceptCooling => conceptRightCooldown > 0;

        // 冷却读条进度（0~1）
        public float ConceptCooldownProgress =>
            !IsConceptCooling ? 1f : 1f - conceptRightCooldown / (float)ConceptCooldownMax;

        public override void PostUpdate()
        {
            if (conceptRightCooldown > 0)
                conceptRightCooldown--;
        }
    }

    //// 专门负责画 UI 的系统 [这个不错啊]
    //public class SunsetConceptUISystem : ModSystem
    //{
    //    public override void PostDrawInterface(SpriteBatch spriteBatch)
    //    {
    //        Player player = Main.LocalPlayer;
    //        var modPlayer = player.GetModPlayer<ConceptRightCooldown>();

    //        if (!modPlayer.IsConceptCooling)
    //            return;

    //        // UI 在玩家头顶偏上
    //        Vector2 pos = player.Center - Main.screenPosition + new Vector2(0f, -80f);

    //        Texture2D barBG = TextureAssets.MagicPixel.Value;
    //        Texture2D barFG = TextureAssets.MagicPixel.Value;

    //        // 背景条（灰）
    //        spriteBatch.Draw(
    //            barBG,
    //            new Rectangle((int)pos.X - 40, (int)pos.Y, 80, 8),
    //            Color.Black * 0.6f
    //        );

    //        // 前景条
    //        float p = modPlayer.ConceptCooldownProgress;
    //        spriteBatch.Draw(
    //            barFG,
    //            new Rectangle((int)pos.X - 40, (int)pos.Y, (int)(80f * p), 8),
    //            Color.Cyan
    //        );
    //    }
    //}
}
