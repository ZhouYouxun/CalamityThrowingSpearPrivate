using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.StarsofDestiny
{
    // 时间再生核心效果本体
    public class StarsofDestinyP2Buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // 友方增益，不显示为Debuff
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // —— 生命回复提升 —— //
            // 基础 lifeRegen += 4，大约 +2HP/秒（在无流血伤害时）
            player.lifeRegen += 4;

            // —— 加速消除所有Debuff —— //
            // 让所有带 Main.debuff 标记的Buff额外每帧再扣一点时间
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int buffType = player.buffType[i];
                if (buffType <= 0)
                    continue;

                // buffTime <= 0 代表已经要被清理了，没必要再改
                if (player.buffTime[i] <= 0)
                    continue;

                // 只处理真正的Debuff
                if (Main.debuff[buffType])
                {
                    // 额外再减少一些时间，相当于持续缩短Debuff时长
                    // 这里每帧再额外-1，大致≈时间流逝加倍
                    if (player.buffTime[i] > 2)
                        player.buffTime[i] -= 1;
                }
            }









            {
                // ==============================================
                // 🌌 Sunset / Stars of Destiny 混合粒子核心特效
                // ==============================================

                // 主色调
                Color mainA = Color.White * 0.9f;       // 亮白
                Color mainB = Color.Cyan * 0.8f;        // 天蓝
                Color soft = new Color(100, 200, 255) * 0.8f;

                // 玩家中心
                Vector2 c = player.Center;


                // ---------------------------
                // ① 流动向上星屑（持续性，视觉基底）
                // 60% 几率
                // ---------------------------
                if (Main.rand.NextFloat() < 0.6f)
                {
                    Vector2 spawn = c + Main.rand.NextVector2Circular(30f, 24f);
                    Vector2 vel = new Vector2(
                        Main.rand.NextFloat(-0.15f, 0.15f),
                        Main.rand.NextFloat(-1.9f, -0.7f)
                    );

                    Dust d = Dust.NewDustPerfect(
                        spawn,
                        DustID.GemDiamond,
                        vel,
                        160,
                        soft,
                        Main.rand.NextFloat(1.0f, 1.5f)
                    );

                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }



                // ---------------------------
                // ② 时间折射短线 —— 线性科技感闪现
                //（原：SparkParticle）
                // 20% 概率
                // ---------------------------
                //if (Main.rand.NextFloat() < 0.20f)
                //{
                //    Vector2 pos = c + Main.rand.NextVector2Circular(14f, 14f);
                //    Vector2 vel = new Vector2(0f, -2.6f).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f));

                //    SparkParticle sp = new SparkParticle(
                //        pos,
                //        vel,
                //        false,
                //        18,
                //        1.25f,
                //        mainA
                //    );

                //    GeneralParticleHandler.SpawnParticle(sp);
                //}



                // ---------------------------
                // ③ 十字星 —— 高光星爆点
                //（泛用：GenericSparkle）
                // 8% 概率
                // ---------------------------
                if (Main.rand.NextFloat() < 0.08f)
                {
                    GenericSparkle star = new GenericSparkle(
                        c + Main.rand.NextVector2Circular(10f, 10f),
                        Vector2.Zero,
                        mainA,
                        mainB,
                        Main.rand.NextFloat(1.6f, 2.3f),
                        8,
                        Main.rand.NextFloat(-0.02f, 0.02f),
                        1.7f
                    );

                    GeneralParticleHandler.SpawnParticle(star);
                }



                // ---------------------------
                // ④ 辉光球 —— 亮点闪烁（补光、节奏）
                // 10% 概率
                // ---------------------------
                if (Main.rand.NextFloat() < 0.10f)
                {
                    GlowOrbParticle orb = new GlowOrbParticle(
                        c + Main.rand.NextVector2Circular(12f, 12f),
                        Vector2.Zero,
                        false,
                        8,
                        1.0f,
                        mainB,
                        true,
                        false,
                        true
                    );

                    GeneralParticleHandler.SpawnParticle(orb);
                }



                // ---------------------------
                // ⑤ 四方科技粒子 —— 数字能量碎片
                //（低频出现，提升层次）
                // 5% 概率
                // ---------------------------
                if (Main.rand.NextFloat() < 0.05f)
                {
                    SquareParticle sq = new SquareParticle(
                        c + Main.rand.NextVector2Circular(20f, 20f),
                        new Vector2(0f, -1.2f).RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)),
                        false,
                        26,
                        Main.rand.NextFloat(1.6f, 2.3f),
                        mainB * 1.5f
                    );

                    GeneralParticleHandler.SpawnParticle(sq);
                }

            }






        }
    }
}
