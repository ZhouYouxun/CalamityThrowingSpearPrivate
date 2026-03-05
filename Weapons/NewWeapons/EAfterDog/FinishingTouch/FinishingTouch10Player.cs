using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    internal class FinishingTouch10Player : ModPlayer
    {
        public bool finishingTouchOrangeTrailActive;

        private const int TrailLength = 10;
        public Vector2[] oldPos = new Vector2[TrailLength];
        public float[] oldRot = new float[TrailLength];

        public override void ResetEffects()
        {
            finishingTouchOrangeTrailActive = false;

            // === 在手持武器 FinishingTouch 时自动激活光晕尾迹调试 ===
            if (Player.HeldItem != null && Player.HeldItem.type == ModContent.ItemType<FinishingTouch>())
            {
                finishingTouchOrangeTrailActive = true;
            }
        }


        public override void PostUpdate()
        {
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }
            oldPos[0] = Player.Center; // 🚩 将 position 改为 Center，避免偏移
            oldRot[0] = Player.fullRotation;
        }
    }
}