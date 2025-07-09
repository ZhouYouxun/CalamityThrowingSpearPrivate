using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Weapons.Ranged;
using Terraria.Audio;
using CalamityMod.Items.Materials;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC
{
    public class ElementalLanceJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "ChangedWeapons.DPreDog";
        // 添加一个计数器
        private int projectileIndex = 0;
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 100; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 12; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.shoot = ModContent.ProjectileType<ElementalLanceJavPROJSolar>(); // 使用新的弹幕
            Item.shootSpeed = 17f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

        }
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 根据 projectileIndex 的值选择不同的弹幕和音效
            switch (projectileIndex)
            {
                case 0: // Solar 日曜
                    type = ModContent.ProjectileType<ElementalLanceJavPROJSolar>();
                    SoundEngine.PlaySound(SoundID.Item116, position);
                    break;
                case 1: // Vortex 漩涡
                    type = ModContent.ProjectileType<ElementalLanceJavPROJVortex>();
                    SoundEngine.PlaySound(SoundID.Item98, position);
                    break;
                case 2: // Nebula 星云
                    type = ModContent.ProjectileType<ElementalLanceJavPROJNebula>();
                    SoundEngine.PlaySound(SoundID.Item117, position);
                    break;
                case 3: // Stardust 星尘
                    type = ModContent.ProjectileType<ElementalLanceJavPROJStardust>();
                    SoundEngine.PlaySound(SoundID.Item153, position);
                    break;
                case 4: // Entropy 冥思 (使用湮阳暝风音效)
                    type = ModContent.ProjectileType<ElementalLanceJavPROJEntropy>();
                    SoundEngine.PlaySound(DeadSunsWind.Explosion, position);
                    break;
            }

            // 发射弹幕
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // 轮换 projectileIndex，保证在五个弹幕之间循环
            projectileIndex++;
            if (projectileIndex > 4)
            {
                projectileIndex = 0;
            }

            return false; // 返回 false 避免默认的发射行为
        }

        //public override void AddRecipes()
        //{
        //    CreateRecipe().
        //        AddIngredient<BotanicPiercer>().
        //        AddIngredient(ItemID.LunarBar, 5).
        //        AddIngredient<LifeAlloy>(5).
        //        AddIngredient<GalacticaSingularity>(5).
        //        AddIngredient<MeldConstruct>(5).
        //        AddTile(TileID.LunarCraftingStation).
        //        Register();
        //}
    }
}
