using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.StarnightLanceC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CalamityMod.Sounds;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.ElectrocoagulationTenmonJav;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK
{
    public class SHPCK : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.BPrePlantera";
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
            Item.useTime = Item.useAnimation = 60; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.shoot = ModContent.ProjectileType<SHPCKPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 15f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键
            {
                Item.damage = 18; // 设置伤害值
                Item.useTime = 20; // 右键攻击速度更快
                Item.useAnimation = 20;
                Item.shoot = ModContent.ProjectileType<SHPCKFast>(); // 右键使用SHPCKFast弹幕
                Item.shootSpeed = 30f; // 更改使用时的武器弹幕飞行速度
                //Item.UseSound = SoundID.Item92;
                //Item.UseSound = CommonCalamitySounds.LaserCannonSound;
                Item.UseSound = SoundID.Item73;
            }
            else // 左键
            {
                Item.damage = 75; // 设置伤害值
                Item.useTime = 60; // 左键保持现有的攻击速度
                Item.useAnimation = 60;
                Item.shootSpeed = 15f; // 更改使用时的武器弹幕飞行速度
                Item.shoot = ModContent.ProjectileType<SHPCKPROJ>(); // 左键使用SHPCKPROJ弹幕
                //Item.UseSound = SoundID.Item1;
                Item.UseSound = SoundID.Item92;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) // 右键
            {
                int projectileCount = 2; // 一次发射X发

                for (int i = 0; i < projectileCount; i++)
                {
                    // 在 -X° ~ X° 之间随机偏移发射方向
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-150f, 150f));

                    // 计算新的方向
                    Vector2 modifiedVelocity = velocity.RotatedBy(angleOffset) * Main.rand.NextFloat(0.9f, 1.1f); // 速度稍微随机化

                    // 修正后的 `NewProjectile` 调用
                    Projectile.NewProjectile(
                        source,
                        position,
                        modifiedVelocity,  // 正确传递 Vector2 类型的速度
                        ModContent.ProjectileType<SHPCKFast>(),
                        (int)(damage * 1.35 * 3f),
                        knockback,
                        player.whoAmI
                    );
                }

                return false;
            }
            else // 左键
            {
                // 左键直接射出一发 SHPCKPROJ
                Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, ModContent.ProjectileType<SHPCKPROJ>(), (int)(damage * 1.5f), knockback, player.whoAmI);
                return false;
            }
        }
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (Main.zenithWorld)
            //醉酒世界drunkWorld
            //困难世界getGoodWorld
            //10周年世界tenthAnniversaryWorld
            //饥荒世界dontStarveWorld
            //蜜蜂世界notTheBeesWorld
            //地下世界remixWorld
            //陷阱世界noTrapsWorld
            //天顶世界zenithWorld
            {
                bool plantera = NPC.downedPlantBoss;
                bool golem = NPC.downedGolemBoss;
                bool cultist = NPC.downedAncientCultist;
                bool moonLord = NPC.downedMoonlord;
                bool providence = DownedBossSystem.downedProvidence;
                bool devourerOfGods = DownedBossSystem.downedDoG;
                bool yharon = DownedBossSystem.downedYharon;
                float damageMult = 1f +
                    (plantera ? 0.1f : 0f) + //1.1
                    (golem ? 0.15f : 0f) + //1.25
                    (cultist ? 3.5f : 0f) + //4.75
                    (moonLord ? 4.5f : 0f) + //9.25
                    (providence ? 7.5f : 0f) + //16.75
                    (devourerOfGods ? 2.5f : 0f) + //19.25
                    (yharon ? 30f : 0f); //49.25
                damage *= damageMult;
            }


            //// 第1阶段
            //bool kingSlime = NPC.downedSlimeKing; // 史莱姆王
            //bool eyeOfCthulhu = NPC.downedBoss1; // 眼睛
            //bool desertScourge = DownedBossSystem.downedDesertScourge; // 荒漠灾虫
            //bool crabulon = DownedBossSystem.downedCrabulon; // 螃蟹
            //bool eaterOfWorldsOrBrain = NPC.downedBoss2; // EoW BoC
            //bool hiveMind = DownedBossSystem.downedHiveMind; // 意志
            //bool perforator = DownedBossSystem.downedPerforator; // 宿主
            //bool queenBee = NPC.downedQueenBee; // 蜂后
            //bool skeletron = NPC.downedBoss3; // 骷髅王
            //bool deerclops = NPC.downedDeerclops; // 独眼巨鹿
            //bool slimeGod = DownedBossSystem.downedSlimeGod; // 史莱姆之神

            //// 第2阶段
            //bool wallOfFlesh = Main.hardMode; // 肉山
            //bool QueenSlime = NPC.downedQueenSlime; // 史莱姆女王
            //bool mechanicalBosses1 = NPC.downedMechBoss1; // 毁灭者
            //bool mechanicalBosses2 = NPC.downedMechBoss2; // 双子魔眼
            //bool mechanicalBosses3 = NPC.downedMechBoss3; // 机械骷髅王
            //bool brimstoneElemental = DownedBossSystem.downedBrimstoneElemental; // 硫磺火元素
            //bool cryogen = DownedBossSystem.downedCryogen; // 极地冰灵
            //bool aquaticScourge = DownedBossSystem.downedAquaticScourge; // 渊海灾虫

            //// 第3阶段
            //bool plantera = NPC.downedPlantBoss; // 花
            //bool calamitas = DownedBossSystem.downedCalamitas; // 灾厄之影
            //bool Leviathan = DownedBossSystem.downedLeviathan; // 利维坦
            //bool astrumAureus = DownedBossSystem.downedAstrumAureus; // 白金
            //bool golem = NPC.downedGolemBoss; // 石
            //bool empress = NPC.downedEmpressOfLight; // 女皇
            //bool fishron = NPC.downedFishron; // 公爵
            //bool plaguebringer = DownedBossSystem.downedPlaguebringer; // 瘟疫使者歌莉娅
            //bool ravager = DownedBossSystem.downedRavager; // 毁灭魔像
            //bool cultist = NPC.downedAncientCultist; // 教徒
            //bool astrumDeus = DownedBossSystem.downedAstrumDeus; // 星神游龙

            //// 第4阶段
            //bool moonLord = NPC.downedMoonlord; // 月总 
            //bool dragonfolly = DownedBossSystem.downedDragonfolly; // 金龙
            //bool guardians = DownedBossSystem.downedGuardians; // 亵渎守卫
            //bool providence = DownedBossSystem.downedProvidence; // 亵渎天神
            //bool ceaselessVoid = DownedBossSystem.downedCeaselessVoid; // 无尽虚空
            //bool signus = DownedBossSystem.downedSignus; // 西格纳斯
            //bool stormWeaver = DownedBossSystem.downedStormWeaver; // 风暴编织者
            //bool polterghast = DownedBossSystem.downedPolterghast; // 幽花
            //bool boomerDuke = DownedBossSystem.downedBoomerDuke; // 老核弹

            //// 第5阶段
            //bool devourerOfGods = DownedBossSystem.downedDoG; // 神吞
            //bool yharon = DownedBossSystem.downedYharon; // 龙
            //bool exoMechs = DownedBossSystem.downedExoMechs; // 巨械
            //bool calamitasClone = DownedBossSystem.downedCalamitasClone; // 至尊灾厄
            //bool BR = DownedBossSystem.downedBossRush; // BR

            //// 其他：
            // downedGSS：大沙狂鲨
            // downedCLAM：巨像蛤
            // downedCLAMHardMode：肉后巨像蛤
            // downedPlaguebringer：瘟疫使者（是boss）
            // CragmawMire：伽玛史莱姆
            // downedDreadnautilus：恐惧鹦鹉螺
            // Mauler：渊海狂鲨
            // CragmawMire：伽玛史莱姆    

        }


        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<PlasmaDriveCore>();
            recipe.AddIngredient<SuspiciousScrap>(4);
            recipe.AddRecipeGroup("AnyMythrilBar", 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }

      
    }
}
