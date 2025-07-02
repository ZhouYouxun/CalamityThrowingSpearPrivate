using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TLLCoolDown : ModSystem
    {
        private static int _cooldownTimer;
        private static bool _isCoolingDown;

        public static void StartCooldown(int time)
        {
            _cooldownTimer = time;
            _isCoolingDown = true;
        }

        public static bool IsCoolingDown => _isCoolingDown;

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (_isCoolingDown)
            {
                // 绘制冷却图标
                DrawCooldownIcon();
            }
        }

        private void DrawCooldownIcon()
        {
            // 冷却条的绘制逻辑
            SpriteBatch spriteBatch = Main.spriteBatch;

            Texture2D iconTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/TheLastLance/TLLCoolDown").Value;
            Texture2D overlayTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/TheLastLance/TLLCoolDownOverlay").Value;

            Vector2 position = new Vector2(60, 60); // 图标显示在左上角
            Rectangle destination = new Rectangle((int)position.X, (int)position.Y, 64, 64); // 图标大小
            float opacity = 1f;

            spriteBatch.Draw(iconTexture, destination, Color.White * opacity);

            // 冷却进度条
            float progress = 1f - (_cooldownTimer / 300f); // 假设最大冷却时间为 300 帧
            int lostHeight = (int)(destination.Height * progress);
            Rectangle crop = new Rectangle(0, lostHeight, overlayTexture.Width, overlayTexture.Height - lostHeight);

            spriteBatch.Draw(overlayTexture, position + new Vector2(0, lostHeight), crop, Color.White * opacity);

            // 递减冷却计时器
            _cooldownTimer--;
            if (_cooldownTimer <= 0)
            {
                _isCoolingDown = false;
            }
        }
    }
}
