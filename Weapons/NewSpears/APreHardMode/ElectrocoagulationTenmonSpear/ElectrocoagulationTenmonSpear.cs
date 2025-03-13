using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.WulfrimSpear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.ElectrocoagulationTenmonSpear
{
    internal class ElectrocoagulationTenmonSpear : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewSpears.APreHardMode";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 12;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = Item.useTime = 12;
            Item.knockBack = 4.5f;
            Item.UseSound = SoundID.Item1;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<ElectrocoagulationTenmonSpearPROJ>();
            Item.shootSpeed = 6f;
            Item.autoReuse = true;
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
    }
}
