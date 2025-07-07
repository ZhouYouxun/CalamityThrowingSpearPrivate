using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using static Terraria.ModLoader.ModContent;
using CalamityMod.Projectiles.Pets;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.TheBroken;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.ShadowJav
{
    public class ShadowJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 70; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 12; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;

            Item.UseSound = new SoundStyle("CalamityThrowingSpear/Sound/Windows/WindowsBackground")
            {
                Volume = 3.0f,
                Pitch = 0f
            };

            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<ShadowJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度
        }
        public override bool CanUseItem(Player player)
        {
            // 无论左键还是右键，首先检查场上是否有 PearlwoodJavRainbowFront 和 PearlwoodJavRainbowTrail
            //for (int i = 0; i < Main.maxProjectiles; i++)
            //{
            //    Projectile proj = Main.projectile[i];
            //    if (proj.active && proj.owner == player.whoAmI &&
            //        (proj.type == ModContent.ProjectileType<PearlwoodJavRainbowFront>() ||
            //         proj.type == ModContent.ProjectileType<PearlwoodJavRainbowTrail>()))
            //    {
            //        proj.Kill(); // 清除 PearlwoodJavRainbowFront 和 PearlwoodJavRainbowTrail 弹幕
            //    }
            //}

            if (player.altFunctionUse == 2) // 右键点击
            {
                // 检查并清除场上已存在的 ShadowJavREPROJ 弹幕
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ShadowJavREPROJ>())
                    {
                        proj.Kill(); // 清除已有的 ShadowJavREPROJ 弹幕
                    }
                }

                // 设置右键的使用参数
                Item.useTime = Item.useAnimation = 60;
                Item.shoot = ModContent.ProjectileType<ShadowJavREPROJ>(); // 右键使用时发射的弹幕
            }
            else // 左键点击
            {
                // 左键点击也会清除右键已存在的滞留弹幕
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ShadowJavREPROJ>())
                    {
                        proj.Kill(); // 清除已有的 ShadowJavREPROJ 弹幕
                    }
                }

                // 设置左键的使用参数
                Item.useTime = Item.useAnimation = 12;
                Item.shoot = ModContent.ProjectileType<ShadowJavPROJ>(); // 左键使用时发射的弹幕
            }

            return true; // 允许使用
        }



        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(); 
            recipe.AddIngredient<TheBroken>(1);
            recipe.AddIngredient<ShadowspecBar>(5);
            //recipe.AddCondition(Condition.BirthdayParty);
            recipe.AddTile(TileType<DraedonsForge>());
            recipe.Register();
        }
    }
}

