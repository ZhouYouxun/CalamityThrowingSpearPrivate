using CalamityMod.Graphics.Metaballs;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Weapons.Magic;

namespace CalamityThrowingSpear.LightingBolts.Metaballs
{
    internal class ShadowAmmoMetaball : Metaball
    {
        // 这个类定义了 Metaball 内部的粒子，每个粒子会有自己的位置、速度和大小

        // 这种特效基于shader，如果没有掌握shader，那么就只能借助本体模组的shader以及相关的父类
        // 这个东西是高度可定制化的，功能非常多，当然相比于各种特效，它适合某一类型，但又不适合另外一种类型
        // 比如说它就适合：能量炮，黑洞武器，幽灵武器，液态武器，魔法光球，范围攻击，深水攻击，时空扭曲

        public class VoidParticle
        {
            public float Size; // 粒子的当前大小
            public Vector2 Velocity; // 粒子的运动方向和速度
            public Vector2 Center; // 粒子的位置

            public VoidParticle(Vector2 center, Vector2 velocity, float size)
            {
                Center = center;
                Velocity = velocity;
                Size = size;
            }

            // 更新粒子的运动状态
            public void Update()
            {
                Center += Velocity; // 粒子按照自己的速度移动
                Velocity *= 0.96f; // 逐渐减少速度，模拟阻力
                Size *= 0.92f; // 缩小粒子，使其逐渐消失
            }
        }

        // 用于存储 Metaball 粒子
        public static List<VoidParticle> Particles { get; private set; } = new();

        // 这个属性决定 Metaball 是否应该被绘制
        // 如果 `Particles` 为空，就不会被绘制，优化性能
        public override bool AnythingToDraw => Particles.Any();

        // 这里定义了 Metaball 叠加的纹理层
        // 你可以替换 `ShadowAmmoLayer` 为自己的贴图路径
        public static Asset<Texture2D> LayerAsset { get; private set; }

        // 这里返回 Metaball 叠加的层
        // 可以修改 `LayerAsset.Value` 以使用不同的视觉效果
        public override IEnumerable<Texture2D> Layers
        {
            get { yield return LayerAsset.Value; }
        }

        // 定义 Metaball 在哪一层绘制
        // `AfterProjectiles` 代表它会在所有弹幕之后绘制
        // 你可以改成 `BeforeProjectiles` 让它在弹幕之前绘制
        public override MetaballDrawLayer DrawContext => MetaballDrawLayer.AfterProjectiles;

        // 定义 Metaball 的边缘颜色
        // `Color.Lerp(Color.Black, Color.Purple, 0.7f)` 让边缘呈现暗紫色渐变
        // 你可以改成 `Color.Lerp(Color.Black, Color.Gray, 0.7f)` 来让它更偏黑白风格
        public override Color EdgeColor => Color.Lerp(Color.Black, Color.Purple, 0.7f);

        // 这个方法在 Mod 加载时执行，负责加载 Metaball 纹理
        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            // 加载 Metaball 叠加的纹理层
            LayerAsset = ModContent.Request<Texture2D>($"CalamityThrowingSpear/LightingBolts/Metaballs/ShadowAmmoLayer", AssetRequestMode.ImmediateLoad);
        }

        // 这个方法用于更新所有粒子
        public override void Update()
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                Particles[i].Update();

                // 让粒子互相排斥
                //for (int j = 0; j < Particles.Count; j++)
                //{
                //    if (i == j) continue;

                //    Vector2 direction = Particles[i].Center - Particles[j].Center;
                //    float distance = direction.Length();
                //    if (distance < Particles[i].Size * 1.5f) // 如果两个光球太近
                //    {
                //        direction.Normalize();
                //        Particles[i].Velocity += direction * 0.5f; // 施加排斥力
                //        Particles[j].Velocity -= direction * 0.5f;
                //    }
                //}

                // 我 甚 至 还 能 让 它 造 成 伤 害
                //foreach (NPC npc in Main.npc)
                //{
                //    if (!npc.active || npc.friendly) continue;

                //    float distance = Vector2.Distance(npc.Center, Particles[i].Center);
                //    if (distance < Particles[i].Size * 0.75f) // 伤害范围
                //    {
                //        // 计算伤害信息
                //        NPC.HitInfo hitInfo = npc.CalculateHitInfo(20, 0, false, 0); // 20点伤害，无击退，无暴击
                //        npc.StrikeNPC(hitInfo); // 传播伤害
                //    }
                //}
            }

            Particles.RemoveAll(p => p.Size <= 2f);
        }



        // 在弹幕命中敌人时调用这个方法
        public static void SpawnParticle(Vector2 position, Vector2 velocity, float size) =>
            Particles.Add(new(position, velocity, size));

        // 这个方法控制 Metaball 层的偏移
        // 你可以让它随时间产生滚动或动态效果

        // 最简单最基础的移动
        public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
        {
            return Vector2.UnitX * Main.GlobalTimeWrappedHourly * 0.03f;
        }

        // 来点整活的移动【谨慎使用】
        //public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
        //{
        //    float time = Main.GlobalTimeWrappedHourly;
        //    float xOffset = (float)Math.Sin(time * 1.5f) * 10f; // 左右摆动
        //    float yOffset = (float)Math.Cos(time * 1.2f) * 15f; // 上下摆动
        //    Vector2 randomOffset = Main.rand.NextVector2Circular(3f, 3f); // 轻微抖动
        //    return new Vector2(xOffset, yOffset) + randomOffset;
        //}

        // 这个方法负责绘制所有 Metaball 【不用管】
        public override void DrawInstances()
        {
            // 使用灾厄自己的 `BasicCircle` 贴图【不建议改，除非你想】
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;

            foreach (VoidParticle particle in Particles)
            {
                Vector2 drawPosition = particle.Center - Main.screenPosition;
                Vector2 origin = tex.Size() * 0.5f;
                Vector2 scale = Vector2.One * particle.Size / tex.Size();

                // 绘制粒子
                Main.spriteBatch.Draw(tex, drawPosition, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }












    }
}
