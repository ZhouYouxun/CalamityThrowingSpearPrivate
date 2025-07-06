using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.IL_Player;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class CreateVictoryPEBuff : ModBuff, ILocalizedModType
    {
        public new string LocalizationCategory => "ModBuff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            int damagePerSecond = 3000; // 每秒伤害
            int damagePerFrame = damagePerSecond / 60; // 将伤害平摊到每一帧（每秒60帧）
            if (Main.zenithWorld)
            {
                damagePerFrame = 2500;
            }

            if (npc.life > damagePerFrame)
            {
                npc.life -= damagePerFrame;
                npc.HitEffect(0, damagePerFrame);

                // 在 NPC 头顶显示减少的生命值
               // CombatText.NewText(npc.Hitbox, new Color(255, 0, 0), damagePerFrame, true, false);
            }
            else
            {
                npc.life = 0;
                npc.checkDead();
            }
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 每秒流失 50 点生命值
            /*int damagePerSecond = 2;
            player.statLife -= damagePerSecond;
            if (player.statLife <= 0)
            {
                player.KillMe(PlayerDeathReason.ByCustomReason($"{player.name} 创业未半而中道崩殂."), 10.0, 0);
            }*/

            player.statDefense += 25;
            player.endurance += 0.25f;
            player.lifeRegen += 20;

            // 生成橙色 "V" 形粒子特效
            Vector2 playerCenter = player.Center; // 以玩家为中心
            Color particleColor = Color.Orange; // 粒子的橙色
            int particleCount = 10; // 每帧生成的粒子数量

            for (int i = 0; i < particleCount; i++)
            {
                // 修改线段方向为左上角和右上角
                // 0为右下角，正90为左下角
                // 因此右上角为-90，左上角为-180
                float angle = (i < particleCount / 2)
                    ? MathHelper.ToRadians(-105) // 右上角
                    : MathHelper.ToRadians(-165); // 左上角

                // 偏移量和长度不变
                float lengthMultiplier = 6f; // 将长度扩大 5 倍
                Vector2 offset = new Vector2(i % (particleCount / 2), i % (particleCount / 2))
                                 * Main.rand.NextFloat(0.5f, 1.5f) * lengthMultiplier;

                Vector2 spawnPos = playerCenter + offset.RotatedBy(angle);
                Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f); // 随机速度

                // 创建粒子
                Dust dust = Dust.NewDustPerfect(spawnPos, DustID.Torch, velocity, 100, particleColor, 1.2f);
                dust.noGravity = true; // 粒子不受重力影响
                dust.fadeIn = 0.1f; // 快速淡入
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f); // 粒子大小
            }




        }






    }
}