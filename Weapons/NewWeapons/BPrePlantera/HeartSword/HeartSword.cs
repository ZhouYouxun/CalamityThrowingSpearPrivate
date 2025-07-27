using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.HeartSword
{
    public class HeartSword : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.BPrePlantera";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true; // 设置为长枪类武器
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 75; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot; // 使用投掷模式
            Item.useTime = Item.useAnimation = 30;
            Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true; // 允许自动使用
            Item.channel = true;  // 允许持续按住左键
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.LightRed;
            Item.shoot = ModContent.ProjectileType<HeartSwordPROJ>(); // 绑定弹幕
            Item.shootSpeed = 10f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 遍历所有投射物
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI) // 确保是当前玩家的投射物
                {
                    // 检查是否为 HeartSwordPROJ 类型
                    if (proj.ModProjectile is HeartSwordPROJ heartProj && heartProj.CurrentState == HeartSwordPROJ.BehaviorState.Aim)
                    {
                        return false; // 如果是瞄准阶段，阻止生成新的 HeartSwordPROJ 弹幕
                    }

                    // 检查是否为 HeartSwordPROJExtra 类型
                    if (proj.ModProjectile is HeartSwordPROJExtra)
                    {
                        continue; // HeartSwordPROJExtra 不阻止生成新的弹幕，直接跳过
                    }
                }
            }

            // 创建一个新的 HeartSwordPROJ 弹幕
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false; // 阻止生成默认弹幕
        }



        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LifeCrystal, 10);
            recipe.AddIngredient(ItemID.Pearlwood, 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }

    }
}
