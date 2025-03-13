using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.GraniteSpear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewSpears.DPreDog.ChaosWindSpear
{
    internal class ChaosWindSpear : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewSpears.DPreDog";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1200;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 17;
            Item.knockBack = 4.5f;
            Item.UseSound = SoundID.Item1;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<ChaosWindSpearPROJ>();
            Item.shootSpeed = 6f;
            Item.channel = true;
            Item.autoReuse = true;
        }
        //public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
    }
}
