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
using CalamityMod.Rarities;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian
{
    public class PrimeMeridian : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Spears[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 100000; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 50; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<PrimeMeridianHouldOut>(); // 使用新的弹幕
            Item.shootSpeed = 25f; // 更改使用时的武器弹幕飞行速度

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
        }



        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            // 左键攻击保护机制 - 检测是否已经存在指定类型的弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == type) // 检查是否已经存在左键攻击的弹幕
                {
                    return false; // 如果已存在，则阻止生成新的弹幕
                }
            }

            // 左键攻击逻辑 - 创建新的弹幕
            int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false; // 阻止生成默认弹幕
        }






        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Zenith, 1);
            recipe.AddIngredient<Nadir>();
            recipe.AddIngredient<ShadowspecBar>(5);
            recipe.Register();
        }
    }
}


/*
 
本初子午线：
类似手持法杖，但是握住末端
法杖顶端将蓄力，视觉效果为背光效果逐渐变强
变到最强之后，自动触发一轮攻击【音效为僵尸104】，在攻击形态下顶端会发出大量的光芒，并且持续性射出激光【传统激光但拥有更强特效】，激光主题为黑色的虚空主题，命中后从屏幕下方连续射出多发虚空精华往上，攻击持续期间主弹幕转动速度大幅降低
本体是个手持弹幕，也会造成极高频率的碰撞伤害
激光寿命进行限制，并且在激光期间也会从本体顶端往周围一次射出11组，共22发追踪弹幕，每组各往两边对称角度射出
这11*2弹幕分别会用这些不同的贴图并有不同的表现形式【但他们都会追踪】：
铜短：命中后释放白色斩切
附魔：命中后释放五角星粒子特效，并且继续飞行一段时间，期间减速，造成第2次伤害
星怒：命中后天将两颗星星
养蜂：命中后爆炸产生蜜蜂，可追踪
种子：命中后炸出多枚种子碎片，可弹射
无头：命中后从屏幕周围召唤数个追踪南瓜
波勇：命中后连续在周围召唤4个额外弹幕反复打击
狂星：命中后从下方很远一段距离射出大量追踪星星
彩猫：命中后往周围炸出一堆彩猫的弹幕
泰拉144【泰拉之刃】：携带着圆盘型剑气前进，范围很大，击中后直线飞行，造成无数次伤害
泰拉143【泰拉之锋】：携带着十字型特效前进，命中后十字形状召唤老版本的剑气 

 */





