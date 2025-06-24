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
using CalamityMod.Particles;
using Terraria.GameContent.Drawing;



namespace CalamityThrowingSpear
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	//public class CalamityThrowingSpear : Mod
	//{

    //}

    // 实际颜色转化成三维颜色选取
    // https://www.w3schools.com/colors/colors_picker.asp

    public class TeachingProjectile : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            // 1. 尖刺型粒子特效💉💉💉---------------------------------
            // 原灾示范：北辰鹦哥鱼【新版】命中特效
            PointParticle leftSpark = new PointParticle(
                Projectile.Center, // 设定粒子的初始位置，与弹幕中心重合
                -Projectile.velocity * 0.5f, // 让粒子朝弹幕运动方向的反方向移动
                false, // 是否受重力影响 (false = 不受重力影响)
                15, // 粒子的生命周期（帧数，15 帧）
                1.1f, // 粒子的缩放大小，数值越大粒子越大
                Color.Orange // 设定粒子的颜色（橙色）
            );

            // 生成该粒子，使其出现在游戏世界中
            GeneralParticleHandler.SpawnParticle(leftSpark);


            // 2.轻型烟雾💨💨💨---------------------------------
            // 原灾示范：余烬箭命中特效
            // 适用于爆炸、蒸汽、灰尘等较轻的烟雾效果
            Particle smokeL = new HeavySmokeParticle(
                Projectile.Center, // 粒子生成位置，与弹幕中心重合
                Projectile.velocity * 0.5f, // 烟雾的移动速度，略微跟随弹幕
                Color.WhiteSmoke, // 烟雾颜色
                18, // 烟雾的生命周期（帧数，18 帧后消失）
                Main.rand.NextFloat(0.9f, 1.6f), // 轻型烟雾的缩放大小，随机值
                0.35f, // 透明度（opacity），较低的值意味着更轻的烟雾
                Main.rand.NextFloat(-1, 1), // 旋转速度
                false // ❌ `required = false`，即轻型烟雾
            );
            // 生成烟雾粒子
            GeneralParticleHandler.SpawnParticle(smokeL);


            // 3.重型烟雾🌫️🌫️🌫️---------------------------------
            // 原灾示范：影流喷射器，奥密克戎线枪口火焰，深渊之刃线分裂弹
            // 适用于爆炸核心、强烈冲击、深海水流等浓厚的烟雾
            Particle smokeH = new HeavySmokeParticle(
                Projectile.Center + new Vector2(0, -10), // 粒子生成位置，略微偏移弹幕中心
                new Vector2(0, -1) * 5f, // 让烟雾向上飘散
                Color.Gray, // 烟雾颜色
                30, // 生命周期（帧数，30 帧后消失）
                Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f), // 缩放大小，较大范围
                1.0f, // 透明度（opacity），较高值意味着更浓的烟雾
                MathHelper.ToRadians(2f), // 旋转速度（更显著）
                true // ✅ `required = true`，即重型烟雾
            );
            // 生成烟雾粒子
            GeneralParticleHandler.SpawnParticle(smokeH);

            //Opacity（透明度）
            //0.35f → 轻型
            //1.0f → 重型
            //Scale（缩放大小）
            //0.9f - 1.6f → 轻型
            //0.7f - 1.3f → 重型
            //Lifetime（生命周期）
            //15 - 18 → 轻型
            //30 + → 重型

            // 4.椭圆形冲击波🔄🔄🔄---------------------------------
            // 原灾示范：影流喷射器，奥密克戎线枪口火焰，深渊之刃线分裂弹
            // 适用于能量爆炸、波动冲击、领域扩散等效果
            Particle pulse = new DirectionalPulseRing(
                Projectile.Center, // 设定粒子的初始位置，与弹幕中心重合
                Projectile.velocity * 0.75f, // 设定冲击波的传播方向与速度
                Color.Green, // 设定粒子的颜色（绿色）
                new Vector2(1f, 2.5f), // 🔹 `Squish` 参数决定椭圆的长短轴比例 (X 轴 = 1, Y 轴 = 2.5)
                Projectile.rotation - MathHelper.PiOver4, // 🔄 椭圆的旋转角度（影响椭圆的朝向）
                0.2f, // 初始缩放大小（`OriginalScale`）
                0.03f, // 最终缩放大小（`FinalScale`）
                20 // 粒子存活时间（20 帧）
            );
            // 生成冲击波粒子
            GeneralParticleHandler.SpawnParticle(pulse);

            // 5.线性粒子---------------------------------
            // 原灾示范：太多了,包括宇宙之火和巨龙之火的debuff效果，日蚀之陨，哈雷彗星炮主体等等
            // 适用于尾迹、火花、能量拖尾等效果
            Particle trail = new SparkParticle(
                Projectile.Center, // 设定粒子的初始位置，与弹幕中心重合
                Projectile.velocity * 0.2f, // 设定粒子的运动方向与速度（较慢）
                false, // ❌ `AffectedByGravity` = false，不受重力影响
                60, // 粒子的生命周期（帧数，60 帧）
                1.0f, // 设定粒子的缩放大小
                Color.Orange // 设定粒子的颜色（橙色）
            );
            // 生成粒子
            GeneralParticleHandler.SpawnParticle(trail);

            // 6.十字星---------------------------------
            // 原灾示范：整个方舟和环境之刃系列，p90和金源弹命中特效
            // 适用于华丽的能量闪光、星爆效果、命中反馈等
            GenericSparkle sparker = new GenericSparkle(
                Projectile.Center, // 设定粒子的初始位置，与弹幕中心重合
                Vector2.Zero, // 粒子不移动，停留在生成位置
                Color.Gold, // 设定十字星的主要颜色（金色）
                Color.Cyan, // 设定十字星的光晕颜色（青色）
                Main.rand.NextFloat(1.8f, 2.5f), // 设定粒子的缩放大小（随机值）
                5, // 粒子的生命周期（帧数，5 帧）
                Main.rand.NextFloat(-0.01f, 0.01f), // 设定旋转速度（正负方向随机）
                1.68f // 设定光晕的扩散大小
            );
            // 生成十字星粒子
            GeneralParticleHandler.SpawnParticle(sparker);

            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX); // 使用正前方方向
            Vector2 sparkVelocity = direction.RotatedBy(Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4)) * 6f; // 随机旋转方向
            CritSpark spark = new CritSpark(
                Projectile.Center, // 粒子生成位置
                sparkVelocity + Main.player[Projectile.owner].velocity, // 粒子初始速度
                Color.White, // 粒子起始颜色
                Color.LightBlue, // 粒子结束颜色
                1f, // 粒子缩放
                16 // 粒子寿命
            );
            GeneralParticleHandler.SpawnParticle(spark);

            // 7.骷髅头💀💀💀---------------------------------
            // 原灾示范：重做后的憎恨左键，沙漠巡游者套装
            // 适用于幽灵效果、亡灵能量、恐怖气息等
            Particle smoke = new DesertProwlerSkullParticle(
                Projectile.Center, // 粒子生成位置，与弹幕中心重合
                Projectile.velocity * 0.5f, // 让骷髅头缓慢飘动
                Color.DarkGray * 0.8f, // 初始颜色，较暗的灰色
                Color.LightGray, // 逐渐变淡至浅灰色
                Main.rand.NextFloat(0.5f, 1.0f), // 设定骷髅头的缩放大小（随机范围）
                150 // 骷髅头的初始透明度
            );
            // 生成骷髅头粒子
            GeneralParticleHandler.SpawnParticle(smoke);


            // 8.圆形冲击波🌊🌊🌊---------------------------------
            // 原灾示范：钛晶磁轨炮，蜂巢发射器，天蝎座，哈雷彗星
            // 往内收缩
            Particle shrinkingpulse = new DirectionalPulseRing(
                Projectile.Center, // 粒子生成位置，与弹幕中心重合
                Vector2.Zero, // 粒子静止不动
                Color.Purple, // 冲击波的颜色（紫色）
                new Vector2(1f, 1f), // 冲击波的初始形状（圆形）
                Main.rand.NextFloat(6f, 10f), // 初始缩放大小
                0.15f, // 最终缩放大小（收缩到非常小）
                3f, // 设定扩散范围
                10 // 粒子的存活时间（10 帧）
            );
            // 生成收缩冲击波粒子
            GeneralParticleHandler.SpawnParticle(shrinkingpulse);

            // 往外扩张
            Particle expandingPulse = new DirectionalPulseRing(
                Projectile.Center,
                Vector2.Zero,
                Color.Cyan,
                new Vector2(1.2f, 1.2f),
                0f, // 设定旋转角度
                0.5f, // 🔹 初始尺寸较小
                6.0f, // 🔹 让冲击波变大
                20 // 存活时间更长
            );
            // 生成扩张冲击波粒子
            GeneralParticleHandler.SpawnParticle(expandingPulse);


            // 9.自定义冲击波【需要自备纹理】---------------------------------
            // 原灾示范：诺法雷爆炸
            // 适用于特定能量场、放射性爆炸、魔法冲击等特效
            Particle bolt = new CustomPulse(
                Projectile.Center, // 粒子生成位置，与弹幕中心重合
                Vector2.Zero, // 粒子静止不动
                Color.Aqua, // 设定冲击波颜色（青色）
                "CalamityMod/Particles/HighResFoggyCircleHardEdge", // 设定粒子使用的贴图【可以随意修改】
                Vector2.One * (Projectile.ai[0] == 1 ? 1.5f : 1f), // 冲击波的椭圆变形比例
                Main.rand.NextFloat(-10f, 10f), // 设定旋转角度
                0.03f, // 初始缩放大小
                0.16f, // 最终缩放大小（逐渐变大）【也可以是逐渐变小】
                16 // 粒子的存活时间（16 帧）
            );
            // 生成自定义冲击波粒子
            GeneralParticleHandler.SpawnParticle(bolt);

            // 10.细长线性粒子---------------------------------
            // 原灾示范：NE猎枪
            // 适用于能量流动、光束轨迹、尾迹等效果
            AltSparkParticle spark5 = new AltSparkParticle(
                Projectile.Center - Projectile.velocity * 1.5f, // 生成位置，略微延迟以形成轨迹
                Projectile.velocity * 0.01f, // 速度极低，几乎静止
                false, // ❌ 不受重力影响
                8, // 存活时间（帧数）
                1.3f, // 设定粒子的缩放大小
                Color.Cyan * 0.135f // 颜色较淡的效果
            );
            // 生成细长星尘粒子
            GeneralParticleHandler.SpawnParticle(spark5);

            // 11.刀盘刀光---------------------------------
            // 原灾示范：宇宙暗流前奏，海神之馈赠线，环境之刃转盘模式
            // 适用于快速旋转的近战武器、回旋镖残影等
            // 🟠 **4/5圆的刀光**
            CircularSmearVFX fullSmear = new CircularSmearVFX(
                Projectile.Center, // 生成位置
                Color.OrangeRed * Main.rand.NextFloat(0.78f, 0.85f), // 颜色
                Main.rand.NextFloat(-8, 8), // 旋转角度
                Main.rand.NextFloat(1.2f, 1.3f) * 2.1f // 2.1 倍缩放
            );
            GeneralParticleHandler.SpawnParticle(fullSmear);
            // 🟡 **1/2圆的刀光**
            SemiCircularSmearVFX semiSmear = new SemiCircularSmearVFX(
                Projectile.Center, // 生成位置
                Color.Yellow * Main.rand.NextFloat(0.78f, 0.85f), // 颜色
                Main.rand.NextFloat(-8, 8), // 旋转角度
                Main.rand.NextFloat(1.2f, 1.3f) * 2.1f, // 2.1 倍缩放
                new Vector2(1f, 0.8f) // 半圆的扁平变形比例
            );
            GeneralParticleHandler.SpawnParticle(semiSmear);
            // 🟢 **1/3圆的刀光**
            TrientCircularSmear trientSmear = new TrientCircularSmear(
                Projectile.Center, // 生成位置
                Color.Lime * Main.rand.NextFloat(0.78f, 0.85f), // 颜色
                Main.rand.NextFloat(-8, 8), // 旋转角度
                Main.rand.NextFloat(1.2f, 1.3f) * 2.1f // 2.1 倍缩放
            );
            GeneralParticleHandler.SpawnParticle(trientSmear);

            // 10.细节爆炸💥---------------------------------
            // 原灾示范：圣火弹爆炸
            // 适用于高质量爆炸、能量释放、冲击波特效
            Particle explosion = new DetailedExplosion(
                Projectile.Center, // 爆炸中心位置
                Vector2.Zero, // 爆炸粒子不会移动
                Color.OrangeRed * 0.9f, // 设定爆炸的颜色
                Vector2.One, // 爆炸的形状比例（1,1 代表正圆）
                Main.rand.NextFloat(-5, 5), // 设定随机旋转角度
                0f, // 初始缩放大小
                0.28f, // 最终缩放大小，逐渐变大
                10 // 爆炸持续时间（帧数）
            );
            // 生成细节爆炸粒子
            GeneralParticleHandler.SpawnParticle(explosion);

            // 11.四方粒子🔳---------------------------------
            // 原灾示范：肾上腺素期间，
            // 适用于科技风格武器、能量构造、数字化特效等

            SquareParticle squareParticle = new SquareParticle(
                Projectile.Center, // 粒子生成位置
                Projectile.velocity * 0.5f, // 设定粒子的移动速度
                false, // ❌ 不受重力影响
                30, // 粒子的存活时间（帧数）
                1.7f + Main.rand.NextFloat(0.6f), // 设定粒子的大小，增加随机性
                Color.Cyan * 1.5f // 粒子的颜色（可调整）
            );

            // 生成四方形粒子
            GeneralParticleHandler.SpawnParticle(squareParticle);





        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);


            // 原版内置了很多很多的特效【它们可以通过 ParticleOrchestrator.RequestParticleSpawn() 进行调用】，你可以直接检查相关文件来获得相关信息，包括但不限于：
            // 可以看一下这一段：https://gist.github.com/Rijam/971b5252707860b65b582093580aa49c
            // 想去学习的话可以去这里：namespace Terraria.GameContent.Drawing; public class ParticleOrchestrator

            // Keybrand 圣钥剑特效 三棱刺
            // NightsEdge 永夜之刃特效 对于后者只剩下紫色
            // TrueNightsEdge 真·永夜之刃特效 绿色和紫色的双倾斜交叉
            // Excalibur 圣剑特效 光芒四射的剑气粒子
            // TrueExcalibur 真·圣剑特效 更强的光剑爆炸效果，黄色和粉色混合
            // TerraBlade 泰拉之刃特效 十字星但是会与玩家的方向相关
            // PaladinsHammer 圣骑士战锤特效 生成圣光粒子，带有锤子旋转轨迹
            // SlapHand 巨手特效 产生击打音效和轻微冲击波
            // WaffleIron 华夫饼熨斗特效 生成华夫饼的各种黄色粒子特效
            // FlyMeal 飞蝇拍特效 生成苍蝇飞散粒子

            // RainbowRodHit 彩虹魔杖击中特效 各种彩虹色的十字星往周围集体扩散，慢慢消失
            // SilverBulletSparkle 银弹火花 银色十字闪光
            // ShimmerArrow 微光箭特效 闪耀粒子和光点，带有魔法波动
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

            //ParticleOrchestrator.RequestParticleSpawn(
            //    clientOnly: false,
            //    ParticleOrchestraType.Keybrand,
            //    new ParticleOrchestraSettings { PositionInWorld = explosionPosition + offset },
            //    Projectile.owner
            //);
        }







    }


















    }
