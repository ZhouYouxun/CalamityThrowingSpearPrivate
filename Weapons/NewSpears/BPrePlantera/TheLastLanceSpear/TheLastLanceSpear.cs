using CalamityMod.Items;
using CalamityMod.Rarities;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewSpears.BPrePlantera.TheLastLanceSpear
{
    internal class TheLastLanceSpear : ModItem
    {
        private int attackType = 1; // 交替挥舞方向，1 = 正向，-1 = 反向

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 5000; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 设置使用方式为投掷
            Item.useTime = Item.useAnimation = 30; // 挥舞速度
            Item.knockBack = 8.5f;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<TheLastLanceSpearSwing>(); // 现在使用 `TheLastLanceSpearSwing`
            Item.shootSpeed = 0f; // 由于是近战挥舞，射速为 0
            Item.autoReuse = true;
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 确保屏幕上只存在一个该武器的挥舞弹幕
            if (Main.projectile.Any(proj => proj.active && proj.owner == player.whoAmI && proj.type == type))
                return false;

            // 计算攻击方向
            Vector2 attackDirection = player.SafeDirectionTo(Main.MouseWorld, Vector2.UnitX);

            // 生成新的挥舞弹幕，`ai[0]` 传递 `attackType` 控制交替方向
            Projectile.NewProjectile(source, player.Center, attackDirection, type, damage, knockback, player.whoAmI, attackType);

            // 交替方向，每次攻击后翻转方向
            attackType *= -1;

            return false; // 阻止默认弹幕
        }











    }
}
