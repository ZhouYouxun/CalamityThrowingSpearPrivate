using CalamityMod.Items.Materials;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SunEssenceJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Rarities;
using CalamityMod;
using CalamityMod.Particles;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLance : ModItem, ILocalizedModType
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public new string LocalizationCategory => "NewWeapons.BPrePlantera";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 50; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 20; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SunEssenceJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 18f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }
        public override bool AltFunctionUse(Player player) => true;

        private int rightClickCooldown = 0;

        //public override void UpdateInventory(Player player) // 给右键添加一定的冷却时间
        //{
        //    if (rightClickCooldown > 0)
        //    {
        //        rightClickCooldown--;
        //    }
        //}

        public override void UpdateInventory(Player player)
        {
            if (rightClickCooldown > 0)
            {
                rightClickCooldown--;

                // 冷却时间归零的一瞬间触发效果
                if (rightClickCooldown == 0)
                {
                    // 在玩家中心释放爆炸特效
                    Particle blastRing = new CustomPulse(
                        player.Center, // 以玩家为中心
                        Vector2.Zero,
                        new Color(0, 0, 139), // 深海的蓝色
                        "CalamityThrowingSpear/texture/IonizingRadiation",
                        Vector2.One * 0.33f,
                        Main.rand.NextFloat(-10f, 10f),
                        0.07f,
                        0.53f,
                        30
                    );
                    GeneralParticleHandler.SpawnParticle(blastRing);

                    // 随机释放粒子特效
                    for (int i = 0; i < 30; i++)
                    {
                        Dust dust = Dust.NewDustDirect(
                            player.Center, // 以玩家为中心
                            0, 0,
                            Main.rand.Next(new int[] { DustID.Water, 180 }) // 使用混合 DustID
                        );
                        dust.scale = Main.rand.NextFloat(1.25f, 1.75f); // 大小随机
                        dust.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f)); // 随机扩散
                        dust.noGravity = true; // 防止粒子受重力影响
                    }

                    // 播放音效
                    SoundEngine.PlaySound(SoundID.Item30, player.Center);
                }
            }
        }


        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键点击
            {
                if (rightClickCooldown > 0 || TLLCoolDown.IsCoolingDown)
                {
                    return false; // 如果冷却中，不能使用
                }

                if (player.ownedProjectileCounts[ModContent.ProjectileType<TheLastLanceDASH>()] > 0)
                {
                    return false; // 禁止多重冲刺
                }

                Item.shoot = ModContent.ProjectileType<TheLastLanceDASH>();
                Item.shootSpeed = 0f;
                Item.useTime = Item.useAnimation = 60;

                // 冷却时间设置
                if (DownedBossSystem.downedLeviathan)
                {
                    rightClickCooldown = 300;
                }
                else
                {
                    rightClickCooldown = 300;
                }

                // 启动冷却视觉效果
                //TLLCoolDown.StartCooldown(rightClickCooldown);
            }


            else // 左键攻击
            {
                // 扔出左键的
                Item.shoot = ModContent.ProjectileType<TheLastLancePROJ>();
                Item.shootSpeed = 18f;
                Item.useTime = Item.useAnimation = 20;
            }

            return base.CanUseItem(player);
        }


        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (DownedBossSystem.downedLeviathan) // 如果击败了利维坦
            {
                Item.damage = 80; // 那么基础伤害提升至80
            }
        }

        //public override void AddRecipes()
        //{
        //    Recipe recipe = CreateRecipe();
        //    recipe.AddIngredient(ItemID.Seahorse, 1);
        //    recipe.AddIngredient(ItemID.Anchor, 1);
        //    recipe.AddTile(TileID.Anvils);
        //    recipe.Register();
        //} 
    }
}


/*
 “最后的骑枪” The Last Lance
获取方式：使用“锚”首次杀死海马（原版泰拉瑞亚海洋群系小动物）会掉落，掉落后可以由海盗出售（肉后初期可获取）
击败猪鲨后得到特效与属性强化
点击左键：【PROJ】
较慢的频率扔出高速投枪，在空中留下深海气泡【气泡】，残影和水流拖尾【轻型烟雾】，命中敌人造成冰河时代。穿0打1，无范围攻击。
点击右键：【DASH】
在玩家的手上拿着骑枪，经过0.5秒的蓄力之后，向前方发动冲刺，对沿途敌人造成伤害和霜冻减益，撞到墙会停止冲刺。冲刺距离与速度固定，不会随着摁住右键的时长而改变。
冲刺时间为45，冲刺速度为30f
其他属性：【player】
手持时防御力加5%，如果敌人对你造成了接触伤害，那么对他释放霜冻和冰河时代
【DASH存在期间】冲刺状态时如果水平有一定速度（只要不是完全垂直的往正左方和正右方冲刺），那么防御力+20%
手持该武器时受到一次致命伤害会重生，给予玩家一个很强大的生命回复效果：【血量逐渐恢复至玩家最大生命值的25%（回复速度为每秒50点）】重生过程与重生后2秒无敌
重生效果复活后可以恢复，也就是说一条命只能用一次
在这两秒无敌之后，进入冲刺模式：
玩家会强制拿起骑枪进行可以穿墙的无限冲刺，对冲刺命中的敌人造成伤害与霜冻和冰河时代。
冲刺期间防御力+100%，但是会至少受到敌人10%的伤害
冲刺期间接触到任何一种液体（水，岩浆，蜂蜜，微光），则取消冲刺进入复活冷却

对两种弹幕而言：
对身上有两种冰属性减益的敌人【霜冻和冰河时代】以及海洋类敌人【打表，由于小怪太复杂，因此仅算海洋类Boss】造成1.75倍伤害。
如果被这两种弹幕打到的敌人血量不足50%，则造成两倍时长的霜冻和冰河时代



取消冲刺状态:复活后死亡或者接触液体，如果是复活取消的可以直接可以获得复活的次数，如果是接触液体的则要进入一定时长的冷却


Flavor Text：“现在你是'命运的宠儿'。”
"At this moment,you are the ‘Fate's Favored Child’."

原话：
攻击方式：左键投出高速投枪（特效是残影和水流拖尾），命中敌人造成冰河时代。右键可以进行类似混沌之盾那样的冲刺，对沿途敌人造成伤害和霜冻减益，撞到墙会停止冲刺。
手持时防御力+5%并且对攻击你的敌人造成霜冻和冰河时代（不好写弹幕来源的敌人那就直接仅对接触伤害也行，我也没办法（），冲刺状态并且平行mph不为0时防御力+20%，受到致命伤害后会进入仅一次的重生，血量逐渐恢复至25%并且不再可以通过其他手段恢复，重生过程无法移动，重生过程与重生后2秒无敌，防御力+100%（但是会受到保底伤害，具体多少看平衡），玩家变为类似渡魂圣物那样的冲刺状态（可以穿墙）并且不再可以停止，对冲刺命中的敌人造成伤害与霜冻和冰河时代。
对身上有两种冰属性减益的敌人以及海洋类敌人造成1.75倍伤害。上述描述中造成减益的情况，当敌人血量只有一半一下时，减益时长翻倍

 
 */



