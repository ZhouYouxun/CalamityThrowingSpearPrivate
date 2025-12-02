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

            // —— 星光粒子特效 —— //
            // 少量、柔和的星光在玩家周围浮动，呼应“时间/星辰”的主题
            if (Main.rand.NextBool(6))
            {
                // 在玩家附近一个小环形区域随机生成
                Vector2 offset = Main.rand.NextVector2Circular(32f, 24f);
                Vector2 spawnPos = player.Center + offset;

                // 粒子缓慢向玩家中心飘回
                Vector2 vel = (player.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.2f, 0.6f);

                Dust d = Dust.NewDustPerfect(
                    spawnPos,
                    DustID.WhiteTorch,
                    vel,
                    150,
                    Color.White * 0.9f,
                    Main.rand.NextFloat(1.0f, 1.4f)
                );
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // 偶尔加一条细线星光，保持和武器整体风格一致
            if (Main.rand.NextBool(18))
            {
                SparkParticle spark = new SparkParticle(
                    player.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    false,
                    14,
                    1.1f,
                    Color.White
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
