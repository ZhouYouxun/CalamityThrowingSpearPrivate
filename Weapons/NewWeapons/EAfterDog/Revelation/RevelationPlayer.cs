using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;


namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class RevelationPlayer : ModPlayer
    {
        public int RevelationLevel { get; set; } = 0; // 当前的Revelation等级
        public int HitCounter { get; set; } = 0; // 计数器，用于升级
        public bool[] LevelNotified = new bool[5]; // 用于记录是否已显示提示，假设最多4级（索引0-4）
        public int HitCounterForLevelUp { get; set; } = 0; // 用于追踪击中计数


        public override void ResetEffects()
        {
            // 如果玩家切换了物品（不再手持 Revelation），清空等级和计数器
            if (Player.HeldItem?.type != ModContent.ItemType<Revelation>())
            {
                RevelationLevel = 0;
                HitCounter = 0; // 清空计数器
                HitCounterForLevelUp = 0; // 重置计数器
                LevelNotified = new bool[5]; // 重置通知状态
                return;
            }

            // 检查当前等级并提示玩家
            //CheckAndNotifyLevel();
        }

        public void TrackHit()
        {
            // 每当RevelationPROJ击中敌人时调用
            HitCounterForLevelUp++;
            //Main.NewText($"TrackHit：你当前的击中敌人的次数是 {HitCounterForLevelUp}", Microsoft.Xna.Framework.Color.Yellow);
            if (HitCounterForLevelUp >= 4)
            {
                RevelationLevel++;
                if (RevelationLevel > 4) RevelationLevel = 4; // 限制最大等级（如果需要）
                HitCounterForLevelUp = 0; // 重置计数器

                //Main.NewText($"TrackHit：你当前的等级是 {RevelationLevel}", Microsoft.Xna.Framework.Color.Yellow);
            }
        }

        private void CheckAndNotifyLevel()
        {
            if (RevelationLevel >= 1 && RevelationLevel <= 4 && !LevelNotified[RevelationLevel])
            {
                string[] levelMessages =
                {
                "", // 占位，索引0不会用到
                "你达到了1级！",
                "你达到了2级！",
                "你达到了3级！",
                "你达到了4级！"
            };

                Main.NewText(levelMessages[RevelationLevel], Microsoft.Xna.Framework.Color.Yellow); // 显示战斗文本，颜色可根据需求修改
                LevelNotified[RevelationLevel] = true; // 标记已通知
            }
        }



        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            // 只有当玩家手持 Revelation 时才降低一个等级
            if (Player.HeldItem?.type == ModContent.ItemType<Revelation>())
            {
                DecrementLevel();
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            // 只有当玩家手持 Revelation 时才降低一个等级
            if (Player.HeldItem?.type == ModContent.ItemType<Revelation>())
            {
                DecrementLevel();
            }
        }


        public void DecrementLevel()
        {
            //Main.NewText($"降低一个等级！", Microsoft.Xna.Framework.Color.Yellow);

            // 降低一个等级
            if (RevelationLevel > 0)
            {
                RevelationLevel--;
            }
        }
    }
}
