using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;



namespace CalamityThrowingSpear
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class CalamityThrowingSpear : Mod
	{
        // 实际颜色转化成三维颜色选取
        // https://www.w3schools.com/colors/colors_picker.asp



        // 原版内置了很多很多的特效【它们可以通过 ParticleOrchestrator.RequestParticleSpawn() 进行调用】，你可以直接检查相关文件来获得相关信息，包括但不限于：
        // Keybrand 圣钥剑特效 金色光束
        // NightsEdge 永夜之刃特效 紫黑色剑气
        // TrueNightsEdge 真·永夜之刃特效 绿色混沌能量爆炸
        // Excalibur 圣剑特效 光芒四射的剑气粒子
        // TrueExcalibur 真·圣剑特效 更强的光剑爆炸效果，带有耀眼黄色光束
        // TerraBlade 泰拉之刃特效 绿色剑气爆炸，具有泰拉粒子
        // PaladinsHammer 圣骑士战锤特效 生成圣光粒子，带有锤子旋转轨迹
        // SlapHand 巨手特效 产生击打音效和轻微冲击波
        // WaffleIron 华夫饼熨斗特效 生成华夫饼粒子，带有金属光效
        // FlyMeal 飞蝇拍特效 生成苍蝇飞散粒子

        // RainbowRodHit 彩虹魔杖击中特效 彩虹色光束爆炸，带有彩色光环
        // SilverBulletSparkle 银弹火花 银色弹道爆炸，带有火花粒子
        // ShimmerArrow 闪耀箭特效 生成闪耀粒子和光点，带有魔法波动
        // StellarTune 星律魔杖特效 生成星云色光斑，带有音乐符号
        // StardustPunch 星尘拳特效 星尘颜色的能量波动，类似星尘龙冲击
        // ShimmerBlock 闪耀方块特效 生成闪烁的光点，类似幻象
        // BlackLightningHit 黑色闪电击中特效 黑紫色闪电冲击，带有能量环
        // BlackLightningSmall 小型黑色闪电 类似 BlackLightningHit，但规模更小

        // ChlorophyteLeafCrystalPassive 叶晶体被动特效 绿色能量粒子，类似叶晶体的能量涌动
        // ChlorophyteLeafCrystalShot 叶晶体射击特效 绿色光束，类似叶晶体发射的激光
        // AshTreeShake 火山灰树震动 产生火山灰尘粒子，模拟树木震落灰烬

        // FlameWaders 火焰行者特效 在地面留下火焰轨迹，类似熔岩靴效果
        // WallOfFleshGoatMountFlames 血肉墙山羊坐骑火焰 生成恶魔山羊燃烧特效

        // PetExchange 宠物交换特效 产生宠物交换时的金色光芒
        // GasTrap 毒气陷阱特效 释放绿色毒雾粒子，扩散状
        // Digestion 消化特效 产生食物消化时的光点和颗粒效果
        // LoadoutChange 装备切换特效 角色切换装备时释放闪光和粒子
        // ItemTransfer 物品传送特效 物品传送时的轨迹粒子，类似魔法传输
        // ShimmerTownNPC 闪耀城镇 NPC 变化 NPC 受到 Shimmer 影响时的变形特效
        // ShimmerTownNPCSend 闪耀城镇 NPC 传送 城镇 NPC 传送时的光斑粒子

        // TownSlimeTransform 城镇史莱姆变形 史莱姆变形时的颜色变换
        // PooFly 苍蝇飞舞特效 生成随机飞舞的小苍蝇粒子











    }
}
