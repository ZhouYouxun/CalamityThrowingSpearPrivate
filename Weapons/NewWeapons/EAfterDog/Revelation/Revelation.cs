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
using System.Linq;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation
{
    public class Revelation : ModItem, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Ultima>();
            Item.ResearchUnlockCount = 1;
            //ItemID.Sets.BonusAttackSpeedMultiplier[Item.type] = 0.33f;
            ItemID.Sets.Spears[Item.type] = true;
        }

        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 222;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = Item.useAnimation = 45;
            Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<RevelationPROJ>();
            Item.shootSpeed = 20f;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }

        public override bool CanUseItem(Player player)
        {
            // 遍历当前世界中的所有弹幕
            //foreach (Projectile proj in Main.projectile)
            //{
            //    if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
            //    {
            //        // 检查是否为 Aim 状态
            //        if (proj.ModProjectile is RevelationPROJ revelationProj && revelationProj.CurrentState == RevelationPROJ.BehaviorState.Aim)
            //        {
            //            return false; // 如果已经存在一个 Aim 状态的弹幕，阻止新的生成
            //                          // Fire 阶段的弹幕不会影响这个判断
            //        }
            //    }
            //}
            return true; // 如果没有 Aim 状态的弹幕，允许生成
        }


        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 获取当前玩家的等级
            int playerRevelationLevel = 0;
            if (player.TryGetModPlayer<RevelationPlayer>(out RevelationPlayer modPlayer))
            {
                playerRevelationLevel = modPlayer.RevelationLevel;
            }


            // 根据等级调整伤害倍率
            float damageMultiplier = 1.0f; // 默认的0级倍率
            switch (playerRevelationLevel)
            {
                case 1:
                    damageMultiplier = 1.35f;
                    break;
                case 2:
                    damageMultiplier = 1.9f;
                    break;
                case 3:
                    damageMultiplier = 2.25f;
                    break;
                case 4:
                    damageMultiplier = 2.25f;
                    break;
            }

            damage = (int)(damage * damageMultiplier);

            // 遍历当前世界中的所有弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
                {
                    // 检查是否为 Aim 状态
                    if (proj.ModProjectile is RevelationPROJ revelationProj && revelationProj.CurrentState == RevelationPROJ.BehaviorState.Aim)
                    {
                        return false; // 如果已经存在一个 Aim 状态的弹幕，阻止新的生成
                                      // Fire 阶段的弹幕不会影响这个判断
                    }
                }
            }

            // 创建新的弹幕
            int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // 将玩家的等级赋值给新生成的弹幕
            if (projIndex >= 0 && projIndex < Main.projectile.Length)
            {
                //Main.NewText($"成功创建投射物，ID: {projIndex}", Microsoft.Xna.Framework.Color.Green);
                Projectile proj = Main.projectile[projIndex];
                if (proj.ModProjectile is RevelationPROJ revelationProj)
                {
                    //Main.NewText($"在发射弹幕的时候传递的等级: {playerRevelationLevel}", Microsoft.Xna.Framework.Color.White);
                    revelationProj.ProjectileLevel = playerRevelationLevel; // 传递等级
                }
            }

            return false; // 阻止生成默认弹幕
        }

        //public override void AddRecipes()
        //{
        //    Recipe recipe = CreateRecipe();
        //    recipe.AddIngredient<Ultima>();
        //    recipe.AddIngredient<Starmada>();
        //    recipe.AddTile(TileType<CosmicAnvil>());
        //    recipe.Register();
        //}
    }
}

