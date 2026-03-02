using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.ModLoader.PlayerDrawLayer;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod;
using Terraria.GameContent;
using CalamityMod.Items.VanillaArmorChanges;


namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    internal class FinishingTouch10TrailLayer : PlayerDrawLayer
    {

        // ✅ 修复插入位置，使用推荐的 LastVanillaLayer

        // 倒数第2个版本的代码【现已弃用】
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.LastVanillaLayer);

        // 以下均可直接替换使用，用于不同渲染时机控制
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc); // 在背饰层后绘制，适合背部特效
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);    // 在头饰层后绘制，适合头部发光
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FrontAcc); // 在前饰层后绘制，适合胸前发光特效
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem); // 在持武器层后绘制，适合手持特效
        // public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.SolarShield); // 在日耀护盾层后绘制，特殊情况使用


        // 倒数第2个版本的代码【现已弃用】
        //public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        //{
        //    //return drawInfo.drawPlayer.GetModPlayer<FinishingTouch10Player>().finishingTouchOrangeTrailActive;

        //    Player player = drawInfo.drawPlayer;
        //    return player.HeldItem?.type == ModContent.ItemType<FinishingTouch>() && !player.dead && drawInfo.shadow == 0f;
        //}


        //protected override void Draw(ref PlayerDrawSet drawInfo)
        //{
        //    Player player = drawInfo.drawPlayer;
        //    var modPlayer = player.GetModPlayer<FinishingTouch10Player>();
        //    //Texture2D texture = Terraria.GameContent.TextureAssets.Player[player.body].Value; 这个是错的
        //    //Texture2D texture = Terraria.GameContent.TextureAssets.ArmorBody[player.body].Value;
        //    // 如果要使用未穿衣服裸身材质
        //    // Texture2D texture = Terraria.GameContent.TextureAssets.Players[0, player.skinVariant].Value;

        //    {
        //        // 这会直接导致一个以玩家为中心，网下无限延长的长方形被绘制出来，第二个尺寸决定了粗细
        //        //Texture2D texture = TextureAssets.MagicPixel.Value; // 使用1x1像素纯色
        //        //Vector2 scale = new Vector2(2f); // 尾迹可见尺寸
        //    }




        //    // 使用玩家当前身体贴图，以便光晕与身体匹配
        //    Texture2D texture = Terraria.GameContent.TextureAssets.Players[0, player.skinVariant].Value;

        //    for (int i = 0; i < modPlayer.oldPos.Length; i++)
        //    {
        //        Vector2 position = modPlayer.oldPos[i] - Main.screenPosition;
        //        Color color = Color.Orange * (0.6f * (1f - i / (float)modPlayer.oldPos.Length)); // 更明显
        //        float rotation = modPlayer.oldRot[i];
        //        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        //        SpriteEffects effects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        //        drawInfo.DrawDataCache.Add(new DrawData(
        //            texture,
        //            position,
        //            null,
        //            color,
        //            rotation,
        //            origin,
        //            1f, // 不缩放
        //            effects,
        //            0
        //        ));
        //    }



        //}



        //protected override void Draw(ref PlayerDrawSet drawInfo)
        //{
        //    Player player = drawInfo.drawPlayer;
        //    List<DrawData> existingDrawData = drawInfo.DrawDataCache;
        //    var modPlayer = player.GetModPlayer<FinishingTouch10Player>();
        //    Vector2[] oldPositions = modPlayer.oldPos;

        //    // === 可调参数 ===
        //    int trailSteps = 1; // 拖影长度（越大越长）
        //    float maxOpacity = 0.4f; // 拖影最大透明度
        //    float minOpacity = 0.15f; // 拖影最小透明度
        //    float maxScale = 1.05f; // 拖影开始时缩放
        //    float minScale = 0.95f; // 拖影结束时缩放
        //    Color trailColor = Color.Orange;

        //    for (float step = 0; step < trailSteps; step += 0.9f)
        //    {
        //        float completion = step / trailSteps;
        //        float opacity = MathHelper.Lerp(maxOpacity, minOpacity, completion);
        //        float scale = MathHelper.Lerp(maxScale, minScale, completion);

        //        List<DrawData> afterimages = new List<DrawData>();
        //        foreach (var drawData in existingDrawData)
        //        {
        //            DrawData copy = drawData;
        //            // 核心修正：
        //            copy.position = drawData.position - player.position + modPlayer.oldPos[(int)step];
        //            copy.color = trailColor * opacity;
        //            copy.scale = new Vector2(scale);
        //            afterimages.Add(copy);
        //        }
        //        drawInfo.DrawDataCache.InsertRange(0, afterimages);
        //    }
        //}

        // 这是倒数第2个版本的代码
        //protected override void Draw(ref PlayerDrawSet drawInfo)
        //{
        //    Player drawPlayer = drawInfo.drawPlayer;
        //    List<DrawData> existingDrawData = drawInfo.DrawDataCache;

        //    // 保留动态根据速度控制透明度（可选）
        //    float movementSpeedInterpolant = CobaltArmorSetChange.CalculateMovementSpeedInterpolant(drawPlayer);

        //    // === 可调参数 ===
        //    float stepInterval = 2.5f; // 分身相隔帧数（原1.7f），越大距离越远
        //    float minScale = 0.16f;
        //    float maxScale = 1f;
        //    float minOpacity = 0.04f; // 可微调
        //    float maxOpacity = 0.18f; // 可微调
        //    Color trailColor = new Color(255, 140, 0); // 稳定橙色

        //    for (float i = 0f; i < drawPlayer.Calamity().oldPos.Length; i += stepInterval)
        //    {
        //        float completionRatio = i / (float)drawPlayer.Calamity().OldPositions.Length;
        //        float scale = MathHelper.Lerp(maxScale, minScale, completionRatio);
        //        float opacity = MathHelper.Lerp(maxOpacity, minOpacity, completionRatio) * movementSpeedInterpolant;

        //        List<DrawData> afterimages = new List<DrawData>();
        //        for (int j = 0; j < existingDrawData.Count; j++)
        //        {
        //            var drawData = existingDrawData[j];

        //            // ✅ 精华核心：使用原来的位置计算逻辑，在历史位置绘制
        //            drawData.position = existingDrawData[j].position - drawPlayer.position + drawPlayer.oldPosition;

        //            drawData.color = trailColor * opacity;
        //            drawData.scale = new Vector2(scale);

        //            afterimages.Add(drawData);
        //        }
        //        drawInfo.DrawDataCache.InsertRange(0, afterimages);
        //    }
        //}





        public override Position GetDefaultPosition()
         => new AfterParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
            => drawInfo.shadow == 0f;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;
            var modPlayer = drawPlayer.GetModPlayer<AfterimagePlayer>();

            List<DrawData> existingDrawData = drawInfo.DrawDataCache;

            float stepInterval = 2.5f;
            float minScale = 0.16f;
            float maxScale = 1f;
            float minOpacity = 0.04f;
            float maxOpacity = 0.18f;

            Color trailColor = new Color(255, 140, 0);

            for (float i = 0; i < modPlayer.OldPositions.Length; i += stepInterval)
            {
                Vector2 oldPos = modPlayer.OldPositions[(int)i];

                float completionRatio = i / modPlayer.OldPositions.Length;
                float scale = MathHelper.Lerp(maxScale, minScale, completionRatio);
                float opacity = MathHelper.Lerp(maxOpacity, minOpacity, completionRatio);

                List<DrawData> afterimages = new List<DrawData>();

                foreach (var original in existingDrawData)
                {
                    DrawData drawData = original;

                    drawData.position =
                        original.position
                        - drawPlayer.position
                        + oldPos;

                    drawData.color = trailColor * opacity;
                    drawData.scale *= scale;

                    afterimages.Add(drawData);
                }

                drawInfo.DrawDataCache.InsertRange(0, afterimages);
            }
        }




    }


    public class AfterimagePlayer : ModPlayer
    {
        public Vector2[] OldPositions = new Vector2[20];

        public override void PostUpdate()
        {
            for (int i = OldPositions.Length - 1; i > 0; i--)
            {
                OldPositions[i] = OldPositions[i - 1];
            }

            OldPositions[0] = Player.position;
        }
    }








}