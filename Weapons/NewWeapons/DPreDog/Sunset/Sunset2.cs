//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Microsoft.Xna.Framework;
//using CalamityMod;
//using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset
//{
//    internal class Sunset2 : ModItem
//    {
//        public override void SetStaticDefaults()
//        {
//            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
//        }
//        public override void SetDefaults()
//        {
//            Item.width = 44;
//            Item.height = 50;
//            Item.damage = 371;
//            Item.DamageType = DamageClass.Melee;
//            Item.noMelee = true;
//            Item.useTurn = true;
//            Item.noUseGraphic = true;
//            Item.useStyle = ItemUseStyleID.Shoot;
//            Item.useTime = 27;
//            Item.useAnimation = 27;
//            Item.knockBack = 8.5f;
//            Item.UseSound = SoundID.Item1;
//            Item.autoReuse = true;
//            Item.value = Item.buyPrice(0, 10, 0, 0);
//            Item.rare = ItemRarityID.Red;
//            Item.shoot = ModContent.ProjectileType<SunsetASunsetLeft>();
//            Item.shootSpeed = 27f;
//            Item.crit = 4;
//            Item.channel = true; // 右键长按支持
//        }
//        public override bool AltFunctionUse(Player player) => true;

//        private int rightClickCooldown = 0; // 右键冷却计时器

//        public override void HoldItem(Player player)
//        {
//            if (Main.myPlayer == player.whoAmI)
//                player.Calamity().rightClickListener = true;

//            if (rightClickCooldown > 0)
//                rightClickCooldown--; // 递减冷却计时器

//            if (player.Calamity().mouseRight && rightClickCooldown == 0 && CanUseItem(player) && player.whoAmI == Main.myPlayer && !Main.mapFullscreen && !Main.blockMouse)
//            {
//                foreach (Projectile proj in Main.projectile)
//                {
//                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<SunsetASunsetRight>())
//                    {
//                        if (proj.ModProjectile is SunsetASunsetRight rightProj && rightProj.CurrentState == SunsetASunsetRight.BehaviorState.Aim)
//                        {
//                            return; // 只拦截 Aim 状态的弹幕
//                        }
//                    }
//                }

//                int damage = (int)player.GetTotalDamage<MeleeDamageClass>().ApplyTo(Item.damage);
//                float kb = player.GetTotalKnockback<MeleeDamageClass>().ApplyTo(Item.knockBack);
//                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<SunsetASunsetRight>(), damage, kb, player.whoAmI);

//                // 设置右键冷却时间
//                rightClickCooldown = 40;
//            }
//        }
//        public override void UseAnimation(Player player)
//        {
//            if (player.altFunctionUse == 2f)
//            {
//                Item.useStyle = ItemUseStyleID.Shoot;
//                Item.UseSound = null;
//                Item.useTurn = false;
//                Item.channel = true;
//                Item.useTime = Item.useAnimation = 40;
//            }
//            else
//            {
//                Item.useStyle = ItemUseStyleID.Swing;
//                Item.UseSound = SoundID.Item1;
//                Item.useTurn = true;
//                Item.channel = false;
//                Item.useTime = Item.useAnimation = 27;
//                Item.shoot = ModContent.ProjectileType<SunsetASunsetLeft>();
//            }
//        }

//        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//        {
//            if (player.altFunctionUse == 2) // 右键
//            {
//                return false; // 右键不在 Shoot 方法里处理
//            }

//            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
//            return false;
//        }
//    }
//}
