using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav;
using CalamityMod.Sounds;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Audio;
using CalamityMod.NPCs.Yharon;
using Terraria.DataStructures;
using CalamityMod.Rarities;
using CalamityMod;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.VulcaniteLanceC;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Build.Tasks;
using CalamityThrowingSpear.Global;
using System.Security.Cryptography.X509Certificates;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouch : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;

            // 注册四帧动画，每六帧切换一次
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 750;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 180;
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
            Item.shoot = ModContent.ProjectileType<FinishingTouchDASH>(); // 初始设置为蓄力冲刺弹幕
            Item.shootSpeed = 0f;
            Item.crit = 15;
        }
        private int attackCounter = 0; // 攻击计数
        private int baseDamage = 600;  // 原始伤害

 
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2 && player.statLife >= 600) // 检测是否为右键使用
            {
                // 限制右键只能生成一个FinishingTouchDASH弹幕
                if (player.ownedProjectileCounts[Item.shoot] > 0)
                    return false;

                // 播放龙吼音效
                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/YharonInfernadoNEW"), player.position);

                // 恢复为默认的右键蓄力冲刺弹幕
                Item.shoot = ModContent.ProjectileType<FinishingTouchDASH>();
                Item.shootSpeed = 0f;
                Item.useTime = Item.useAnimation = 60;

                // 给玩家添加 VulnerabilityHex 孱弱巫咒 和 Dragonfire 龙焰，持续时间为4秒（240帧）
                player.AddBuff(ModContent.BuffType<CreateVictoryPEBuff>(), 300);
                player.statLife -= 150;
            }
            else
            {
                // 每次左键使用计数 +1
                attackCounter++;

                if (attackCounter >= 10) // 每攻击多少次触发一次强化攻击
                {
                    // 第 10 次触发强化攻击
                    FinishingTouchPROJ.UseDragonSnakeMode = true;
                    Item.damage = baseDamage * 2;
                    attackCounter = 0; // 重置
                }
                else
                {
                    FinishingTouchPROJ.UseDragonSnakeMode = false;
                    Item.damage = baseDamage;
                }


                // 播放龙吼音效
                SoundEngine.PlaySound(Yharon.ShortRoarSound with { Volume = 0.5f }, player.position);
                // 切换为抛射物弹幕，并设置使用时间和动画为60帧
                Item.shoot = ModContent.ProjectileType<FinishingTouchPROJ>();
                Item.shootSpeed = 20f;
                Item.useTime = Item.useAnimation = 24;


                // 左键攻击时：20% 的概率在玩家头顶显示随机文本
                if (Main.rand.NextFloat() < 1f)
                {
                    // 随机选择文本
                    string[] messages = new string[]
                    {
        "尔等良将,与我，不堪一击！",
        "吾令不从者，当膏霜锋之鄂",
        "不顺我意者，当填在野之壑",
        "从我者可免，拒我者难容",
        "有违吾意者，此子便是下场",
        "汝如欲大败而归，则可进军一战！",
        "卧榻之侧，岂容他人酣睡！",
        "力摧敌阵，如视天光破云！",
        "此等残兵，破之，何其易也",
        "文韬武略兼备，方可破敌如破竹",
        "战火燃尽英雄胆！",
        "翻江覆蹈海，六合定乾坤！",
        "吾心所向天意难挡！更况尔等？",
        "弓马骑射撒热血，突破重围显英豪",
        "水背源则川竭，人背信则名裂",
        "与君酣战，快哉快哉",
        "八百虎贲踏江去，十万吴兵丧胆还！",
        "虎啸逍遥镇千里，江东碧眼犹梦惊！",
        "求田问舍非良策，功业何须与命偕",
        "宁可战死失社稷，绝不拱手让江山",
        "汝等看好了！",
        "秉赤面，观春秋，虓菟踏纛，汗青著峥嵘",
        "着青袍，饮温酒，五关已过，来将且通名",
        "青龙啸肃月，长刀裂空，威降一十九将",
        "春秋着墨十万卷，长髯映雪千里行",
        "义驱千里长路，风起桃园芳菲",
        "长车琳，铁甲铮，桓侯既至百冤明",
        "汝罪之大，似彻天之山，盈渊之海",
        "生犯贪嗔痴戾疑，死受鞭笞釜灼烹",
        "此间不明我明之，此事不平我平之",
        "灼艾分痛失，虽万劫，亦杀之",
        "兄弟三人结义志，桃园英气久长存",
        "怒伤心肝，也阻止不了这复仇之火",
        "听君谏言，去危亡，保宗祠",
        "急军先行，斩将，夺城，再败军",
        "你可闪得过此一击！？",
        "匹马单枪出重围，英风锐气敌胆寒",
        "八面阴风杀气飘，勤王保驾显功劳",
        "登锋履刃，何妨马革裹尸",
        "率军冲锋，不具刀枪所阻",
        "一骑破霄汉，饮马星河，醉卧广寒",
        "马踏祁连山河动，兵起玄黄奈何天",
        "名师大将莫自牢，千兵万马避红袍",
        "干云气惊八万里，一剑光寒十九州",
        "雷池铸剑，今霜刃即成，当振天下于大白"

                    };

                    string[] soundPaths = new string[]
                    {
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/erdengliangjiang",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/wulingbucong",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/bushunwoyi",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/congwozhe",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/youweiwuyizhe",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/ruyudabaiergui",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/wotazhice",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/licuidizhen",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/cidengcanbing",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/wentaowulue",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/ranshang",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/fanjiangfuhai",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/wuxinsuoxiang",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/gongmaqishe",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/shuibeiyuan",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/hanzhan",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/babaihuben",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/huxiaoxiaoyao",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/qiutianwenshe",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/zhanjue",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/kanhaole",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/hanqingzhuzhengrong",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/laijiangqietongming",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/weixiangyishijiujiang",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/chunqiuzhuomo",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/yiquqianlichanglu",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/baiyuanming",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/ruzuizhida",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/tanchenchi",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/cijianbuming",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/zhuoaifentongshi",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/taoyuanjieyi",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/nushangxingan",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/congjian",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/xuanfeng",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/xiyiji",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/longdan1",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/longdan2",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/mageguoshi",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/bujudaoqiangsuozu",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/yiqipoxiaohan",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/mataqilianshanhedong",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/mingshidajiang",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/danyunqijing",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/tianjie"
                };

                    // 随机选择文本和对应的音效
                    int index = Main.rand.Next(messages.Length);
                    string selectedMessage = messages[index];
                    string selectedSoundPath = soundPaths[index];

                    // 显示战斗文本
                    Vector2 textPosition = player.Center - new Vector2(0, player.height / 2 + 20f); // 在玩家头顶位置
                    CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedMessage, false, false);


                    // 检查是否启用了独特音效播放的开关
                    if (ModContent.GetInstance<CTSConfigs>().EnableFTSound)
                    {
                        // 播放对应的音效
                        SoundEngine.PlaySound(new SoundStyle(selectedSoundPath) with { Volume = 2.5f }, player.Center);
                    }
                }



            }
            return base.CanUseItem(player);
        }



        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<GildedProboscisJav>();
            recipe.AddIngredient<YharonSoulFragment>(8);
            recipe.AddTile<CosmicAnvil>();
            recipe.Register();
        }





    }
}

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
//{
//    public class FinishingTouch1 : ModSystem
//    {
//        public override void OnModLoad()
//        {
//            base.OnModLoad();

//            // 要检测的冲突模组的内部名字
//            string conflictingModName = "CalamityOverhaul";

//            // 检测是否加载了目标模组
//            if (ModLoader.HasMod(conflictingModName))
//            {
//                // 以 50% 的概率关闭 tModLoader
//                if (Main.rand.NextBool(2))
//                {
//                    // 输出日志信息（可选）
//                    //Logger.Warn($"检测到与模组 {conflictingModName} 冲突，tModLoader 正在关闭以避免问题。");

//                    // 调用游戏退出方法
//                    Environment.Exit(0); // 正常退出 tModLoader，不报错
//                }
//            }
//        }
//    }
//}

/*
 骚话：
右键冲刺开始时：
1. 一切到此为止！
2. 龙战于野！其血玄黄！
3. 画龙点睛！
有BOSS血量低于7.5%时：（所有都算）
1. 分出胜负吧！一击必杀！
左键攻击时20%概率/击杀一个敌怪时：
1. 尔等良将、与我，不堪一击！
2. 吾令不从者，当膏霜锋之鄂
3. 不顺我意者，当填在野之壑
4. 从我者可免，拒我者难容
5. 有违吾意者，此子便是下场
6. 汝如欲大败而归，则可进军一战！
7. 卧榻之侧，岂容他人酣睡！
8. 力摧敌阵，如视天光破云！
9. 此等残兵，破之，何其易也
空闲时：（不处于BOSS战或者事件中且背包有该物品）
1. 捅死你！捅死你！捅死你！
2. 无聊...我要看到血流成河！
3. This is my message To my master
4. 哈啰...咕德拜！
5. 吾有众好友，分为...风筝、台风、电脑...



被扔到在地上时且不是首次生成：（检测到地面上有该掉落物生成一次）
1. 你就这么不要我了？这好吗？这不好
2. 我讨厌你
被放回物品栏时：（玩家物品栏里新增加该物品时生成一次）
1. 来！与我共同登上无人知晓的巅峰
2. 没错！勇于面对，共同前行吧！
3. 来！掌握着手中的胜利之星！
手持画龙点睛受伤时：
1. 上吧！起死回生！
2. 绝望中，仍存有一线生机！
3. 还不可以认输！
击杀神吞时：
1. 打神吞，特别厉害！
手持画龙点睛死亡时：
1. 哦死了啦，都是你害的啦
2. 菜就多练练

 */