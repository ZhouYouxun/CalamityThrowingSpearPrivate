using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Projectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod;
using System.Linq;
using Terraria.DataStructures;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Items.Materials;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Items.Weapons.Ranged;

namespace CalamityThrowingSpear.Weapons.NewWeapons.ZOthers.GlobalStorm
{
    public class GlobalStorm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 100;
            Item.crit = 5;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.knockBack = 5f;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.DamageType = DamageClass.Magic;
            Item.shoot = ProjectileID.None;
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void UseAnimation(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.UseSound = null;
                Item.useTurn = false;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.UseSound = SoundID.Item38;
                Item.useTurn = true;
            }
        }



        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                player.Calamity().rightClickListener = true;

            // 右键逻辑：生成 GlobalStormRightHoldOut
            if (player.Calamity().mouseRight && CanUseItem(player) && player.whoAmI == Main.myPlayer)
            {
                // 检查是否已存在 GlobalStormRightHoldOut，避免重复生成
                if (!Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<GlobalStormRightHoldOut>() && p.owner == player.whoAmI))
                {
                    Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<GlobalStormRightHoldOut>(), 0, 0, player.whoAmI);
                }
            }
            else if (player.ownedProjectileCounts[ModContent.ProjectileType<GlobalStormRightHoldOut>()] <= 0)
            {
                // 设置左键的基础属性
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.UseSound = SoundID.Item38;
                Item.useTurn = true;
                Item.autoReuse = true;
                Item.shoot = ModContent.ProjectileType<ArcherfishShot>();
                Item.shootSpeed = 10f;
                Item.useAmmo = AmmoID.Bullet;
                Item.damage = 100;
                Item.knockBack = 5f;
                Item.DamageType = DamageClass.Ranged;
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 如果玩家正在使用右键，则阻止射击
            if (player.altFunctionUse == 2)
                return false;

            // 检查场上是否已经存在属于玩家的 GlobalStormRightHoldOut，如果存在则拒绝左键射击
            if (Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<GlobalStormRightHoldOut>() && p.owner == player.whoAmI))
                return false;

            // 重新计算 velocity，使其指向鼠标
            Vector2 aimDirection = (Main.MouseWorld - position).SafeNormalize(Vector2.UnitX); // 计算正确的方向
            float baseSpeed = velocity.Length(); // 保持原始速度

            int shotCount = Main.rand.Next(3, 10); // 生成 3~9 发
            for (int i = 0; i < shotCount; i++)
            {
                float damageMult = Main.rand.NextFloat(0.75f, 1.5f); // 伤害浮动
                float speedMult = Main.rand.NextFloat(0.95f, 1.05f); // 初始速度浮动
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f)); // 20 度散射角

                Vector2 shotVelocity = aimDirection.RotatedBy(angleOffset) * baseSpeed * speedMult; // 以鼠标方向为基准计算旋转角度
                //Projectile.NewProjectile(source, position, shotVelocity, ModContent.ProjectileType<ArcherfishShot>(), (int)(damage * damageMult), knockback, player.whoAmI);
                Projectile.NewProjectile(source, position, shotVelocity, type, (int)(damage * damageMult), knockback, player.whoAmI);

            }
            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<AquasScepter>(1);
            recipe.AddIngredient<Seadragon>(1);
            recipe.AddIngredient<Disseminator>(1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
    }
}

/*
 支援武器：
全球风暴
阿库娅法杖+一把枪，但类似法杖
手持召唤雷云，悬停在玩家头顶一段距离尝试往正下方发射箭矢，往敌人处发射子弹，如果玩家没有则发射火枪子弹和木箭。
左键普通攻击。射出一堆子弹
右键长按蓄力，使用怨戾的法阵，类似发条弓，逐渐填装更多的能量球，松手直接射出，能量球会拐弯，最后追踪。 
 */