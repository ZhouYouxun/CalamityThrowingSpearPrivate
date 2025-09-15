using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetEDebuff : ModBuff
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // 标记为 Debuff
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.Calamity() != null)
            {
                npc.Calamity().DR -= 0.15f; // DR -15%
                if (npc.Calamity().DR < 0f)
                    npc.Calamity().DR = 0f; // 确保 DR 不会低于 0
            }
        }
    }
}