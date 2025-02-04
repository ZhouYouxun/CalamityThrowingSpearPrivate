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
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using CalamityMod;
using CalamityMod.Rarities;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav;
using CalamityMod.Projectiles.Melee;
using static Terraria.ModLoader.ModContent;
using CalamityMod.Projectiles.Pets;
using System.IO.Pipelines;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation;


namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    public class SawBladeForkHornJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 8848; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = Item.useAnimation = 30;
            Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<SawBladeForkHornJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
        }
        public override bool AltFunctionUse(Player player) // 允许使用右键
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            // 处理右键攻击的逻辑
            if (player.altFunctionUse == 2) // 如果使用的是右键
            {
                // 查找并清除已有的 SawBladeForkHornJavRIGHT 弹幕
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<SawBladeForkHornJavRIGHT>())
                    {
                        proj.Kill(); // 清除已有弹幕
                    }
                }

                // 访问玩家的堆叠层数
                var modPlayer = player.GetModPlayer<SawBladeForkHornPlayer>();
                int stackCount = modPlayer.StackCount; // 读取当前堆叠层数

                // 设置右键攻击的参数
                Item.useTime = Item.useAnimation = 20; // 设置右键攻击的使用时间和动画时间
                Item.shoot = ModContent.ProjectileType<SawBladeForkHornJavRIGHT>(); // 右键发射的弹幕类型
                Item.shootSpeed = 10f; // 初始速度设置为 10f
                //Item.damage = (int)(Item.damage * (1 + stackCount / 20f)); // 使用堆叠层数计算伤害倍率

                // 计算伤害倍率，并添加最大值限制
                //int calculatedDamage = (int)(Item.damage * (1 + stackCount / 20f));
                //Item.damage = Math.Min(calculatedDamage, 250000); // 将伤害限制为 25 万
            }
            else
            {
                // 恢复左键攻击的设置
                Item.useTime = Item.useAnimation = 30;
                Item.shoot = ModContent.ProjectileType<SawBladeForkHornJavPROJ>();
                Item.shootSpeed = 25f;
            }

            return base.CanUseItem(player);
        }
        //public override void PostUpdate()
        //{
        //    // 每帧重置面板伤害为 8848
        //    if (Item.damage != 8848)
        //    {
        //        Item.damage = 8848;
        //    }
        //}
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) // 右键攻击时
            {
                // 访问玩家的堆叠层数
                var modPlayer = player.GetModPlayer<SawBladeForkHornPlayer>();
                int stackCount = modPlayer.StackCount; // 读取当前堆叠层数
                float damageMultiplier = 1 + (stackCount / 20f); // 计算伤害倍率

                // 限制伤害为 25 万上限
                int finalDamage = Math.Min((int)(damage * damageMultiplier), 250000);

                // 创建新的 SawBladeForkHornJavRIGHT 弹幕
                Projectile.NewProjectile(source, position, velocity, type, finalDamage, knockback, player.whoAmI);
                return false; // 阻止生成默认弹幕
            }


            // 遍历当前世界中的所有弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
                {
                    // 检查是否为 Aim 状态
                    if (proj.ModProjectile is SawBladeForkHornJavPROJ SBFHJ && SBFHJ.CurrentState == SawBladeForkHornJavPROJ.BehaviorState.Aim)
                    {
                        return false; // 如果已经存在一个 Aim 状态的弹幕，阻止新的生成
                                      // Fire 阶段的弹幕不会影响这个判断
                    }
                }
            }


            // 左键攻击逻辑 - 创建新的弹幕
            int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false; // 阻止生成默认弹幕
        }



        //public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        //{
        //    if (player.channel) // 持续按住则保持蓄力
        //    {
        //        return false; // 不发射
        //    }

        //    // 蓄力完成后投掷
        //    velocity = velocity.SafeNormalize(Vector2.UnitY) * Item.shootSpeed;
        //    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        //    return false;
        //}

        //public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        //{
        //    // 创建新的弹幕，伤害倍率始终为 1.0 倍
        //    int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

        //    return false; // 阻止生成默认弹幕
        //}

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<Aftershock>(1);
            recipe.AddIngredient<EarthenPike>(1);
            recipe.AddIngredient(ItemID.SoulofMight, 30);
            recipe.AddIngredient<ShadowspecBar>(5);
            recipe.AddTile(TileType<DraedonsForge>());
            recipe.Register();
        }
    }
}

