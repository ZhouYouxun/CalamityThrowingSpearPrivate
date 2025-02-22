using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset
{
    internal class SunsetASunsetEDebuff : ModBuff
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true; // 标记为 Debuff
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // 减速效果
            npc.velocity *= 0.85f; // 移动速度减少 15%
        }
    }
}