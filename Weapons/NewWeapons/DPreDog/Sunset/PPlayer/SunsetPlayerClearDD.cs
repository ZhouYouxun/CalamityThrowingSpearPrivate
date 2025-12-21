using Terraria;
using Terraria.ModLoader;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    // ======================================================
    // 当玩家手持 Sunset 时：
    // 1) 所有敌方弹幕不再造成 Defense Damage
    // ======================================================
    internal class DisableDefenseDamageGlobalProjectile : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            // 只处理敌方弹幕
            if (!projectile.hostile)
                return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active || player.dead)
                return;

            // 未手持 Sunset，不处理
            if (player.HeldItem.type != ModContent.ItemType<Sunset>())
                return;

            // 关闭防御损伤（弹幕）
            if (projectile.Calamity().DealsDefenseDamage)
                projectile.Calamity().DealsDefenseDamage = false;
        }
    }

    // ======================================================
    // 当玩家手持 Sunset 时：
    // 2) 敌人“接触伤害”不再造成 Defense Damage
    // ======================================================
    internal class DisableBreakDefenseGlobalNPC : GlobalNPC
    {
        public override void AI(NPC npc)
        {
            // 只处理敌对 NPC
            if (!npc.active || npc.friendly)
                return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active || player.dead)
                return;

            // 只有在玩家手持 Sunset 时才生效
            if (player.HeldItem.type != ModContent.ItemType<Sunset>())
                return;

            // ★ 核心：在命中发生前，直接掐断防御破坏能力
            if (npc.Calamity().canBreakPlayerDefense)
            {
                npc.Calamity().canBreakPlayerDefense = false;
            }
        }
    }

}
