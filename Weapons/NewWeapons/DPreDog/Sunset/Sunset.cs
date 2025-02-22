using CalamityMod.Items.Materials;
using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.RedtideJav;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityThrowingSpear.Global;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.ASunset;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept;
using Terraria.ModLoader.IO;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset
{
    internal class Sunset : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            //ItemID.Sets.Spears[Item.type] = true;
            //Item.staff[Item.type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        private int rightClickCooldown = 0; // 右键冷却计时器
        private int currentMode = 0; // 当前形态 (0 = A, 1 = B, 2 = C)
        private static readonly string[] modeNames = { "A模式", "B模式", "C模式" };
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 371; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 27; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;

            Item.shoot = ModContent.ProjectileType<SoulHunterJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 27f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

            UpdateModeProperties(); // 初始化模式属性

            Item.channel = true; // 确保右键能够长按
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                player.Calamity().rightClickListener = true;

            if (rightClickCooldown > 0)
                rightClickCooldown--; // 递减冷却计时器

            if (KeybindSystem.WeaponSkill.JustPressed)
            {
                currentMode = (currentMode + 1) % 3; // 循环切换形态
                UpdateModeProperties();
                CombatText.NewText(player.getRect(), Color.Red, modeNames[currentMode]);
                SoundEngine.PlaySound(SoundID.Item4, player.position);

                // 触发粒子特效
                Particle blastRing = new CustomPulse(
                    player.Center,
                    Vector2.Zero,
                    Color.Red,
                    "CalamityThrowingSpear/texture/IonizingRadiation",
                    Vector2.One * 0.33f,
                    Main.rand.NextFloat(-10f, 10f),
                    0.07f,
                    0.53f,
                    30
                );
                GeneralParticleHandler.SpawnParticle(blastRing);
            }

            if (player.Calamity().mouseRight && rightClickCooldown == 0 && CanUseItem(player) && player.whoAmI == Main.myPlayer && !Main.mapFullscreen && !Main.blockMouse)
            {
                // 🚨 **检查所有投射物，拦截所有形态的 `Aim` 状态**
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI)
                    {
                        if (proj.type == ModContent.ProjectileType<SunsetASunsetRight>() && proj.ModProjectile is SunsetASunsetRight rightProj && rightProj.CurrentState == SunsetASunsetRight.BehaviorState.Aim)
                            return; // 只拦截 Aim 状态的 `SunsetASunsetRight`

                        if (proj.type == ModContent.ProjectileType<SunsetBForgetRight>() && proj.ModProjectile is SunsetBForgetRight forgetProj && forgetProj.CurrentState == SunsetBForgetRight.BehaviorState.Aim)
                            return; // 只拦截 Aim 状态的 `SunsetBForgetRight`

                        if (proj.type == ModContent.ProjectileType<SunsetCConceptRight>() && proj.ModProjectile is SunsetCConceptRight conceptProj && conceptProj.CurrentState == SunsetCConceptRight.BehaviorState.Aim)
                            return; // 只拦截 Aim 状态的 `SunsetCConceptRight`
                    }
                }

                int projType = currentMode switch
                {
                    0 => ModContent.ProjectileType<SunsetASunsetRight>(),
                    1 => ModContent.ProjectileType<SunsetBForgetRight>(),
                    2 => ModContent.ProjectileType<SunsetCConceptRight>(),
                    _ => ModContent.ProjectileType<SunsetASunsetRight>()
                };

                int damage = (int)player.GetTotalDamage<MeleeDamageClass>().ApplyTo(Item.damage);
                float kb = player.GetTotalKnockback<MeleeDamageClass>().ApplyTo(Item.knockBack);
                Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, projType, damage, kb, player.whoAmI);

                // 设置右键冷却时间
                rightClickCooldown = 40;
            }
        }




        public override void UseAnimation(Player player)
        {
            if (player.altFunctionUse == 2f)
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.UseSound = null;
                Item.useTurn = false;
                Item.channel = true;
                Item.useTime = Item.useAnimation = 40;
                Item.UseSound = null;
                Item.shoot = currentMode switch
                {
                    0 => ModContent.ProjectileType<SunsetASunsetRight>(),
                    1 => ModContent.ProjectileType<SunsetBForgetRight>(),
                    2 => ModContent.ProjectileType<SunsetCConceptRight>(),
                    _ => ModContent.ProjectileType<SunsetASunsetRight>()
                };
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.UseSound = SoundID.Item1;
                Item.useTurn = true;

                if (currentMode == 2) // **C模式支持长按**
                {
                    Item.channel = true; // **启用长按**
                    Item.useTime = Item.useAnimation = 20; // 设定适合连发的速度
                    Item.UseSound = null;
                    Item.shoot = ModContent.ProjectileType<SunsetCConceptLeftListener>(); // **替换为 Listener**
                }
                else // **A/B 形态仍然单点**
                {
                    Item.UseSound = SoundID.Item1;
                    Item.channel = false;
                    Item.useTime = Item.useAnimation = 27;
                    Item.shoot = currentMode switch
                    {
                        0 => ModContent.ProjectileType<SunsetASunsetLeft>(),
                        1 => ModContent.ProjectileType<SunsetBForgetLeft>(),
                        _ => ModContent.ProjectileType<SunsetASunsetLeft>()
                    };
                }
            }
        }

        private void UpdateModeProperties()
        {
            switch (currentMode)
            {
                case 0: // A形态
                    Item.shoot = ModContent.ProjectileType<SunsetASunsetLeft>(); // 左键弹幕
                    Item.shootSpeed = 27f;
                    Item.damage = 371;
                    Item.UseSound = SoundID.Item1;
                    Item.channel = false;
                    break;
                case 1: // B形态
                    Item.shoot = ModContent.ProjectileType<SunsetBForgetLeft>();
                    Item.shootSpeed = 24f;
                    Item.damage = 410;
                    Item.UseSound = SoundID.Item2;
                    Item.channel = false;
                    break;
                case 2: // C形态
                    Item.shoot = ModContent.ProjectileType<SunsetCConceptLeftListener>();
                    Item.shootSpeed = 30f;
                    Item.damage = 450;
                    Item.UseSound = null;
                    Item.channel = true; // **启用长按**
                    break;
            }
        }

        //public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        //{
        //    if (player.altFunctionUse == 2) // 右键
        //    {
        //        // 先检查是否已有右键投射物存在
        //        foreach (Projectile proj in Main.projectile)
        //        {
        //            if (proj.active && proj.owner == player.whoAmI)
        //            {
        //                if (proj.type == ModContent.ProjectileType<SunsetASunsetRight>() ||
        //                    proj.type == ModContent.ProjectileType<SunsetBForgetRight>() ||
        //                    proj.type == ModContent.ProjectileType<SunsetCConceptRight>())
        //                {
        //                    return false; // 如果已经存在一个右键投射物，则不再生成
        //                }
        //            }
        //        }

        //        // 确定正确的右键弹幕类型
        //        type = currentMode switch
        //        {
        //            0 => ModContent.ProjectileType<SunsetASunsetRight>(),
        //            1 => ModContent.ProjectileType<SunsetBForgetRight>(),
        //            2 => ModContent.ProjectileType<SunsetCConceptRight>(),
        //            _ => type
        //        };
        //    }

        //    // 生成新的投射物
        //    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        //    return false; // 阻止生成默认弹幕
        //}



        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // **如果当前模式是 `C模式`（即 `SunsetCConceptLeftListener`），先检查是否已存在**
            if (type == ModContent.ProjectileType<SunsetCConceptLeftListener>())
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == type && proj.owner == player.whoAmI)
                    {
                        return false; // **已有 `SunsetCConceptLeftListener`，拒绝生成**
                    }
                }
            }

            // **如果是其他模式（A、B），直接生成**
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}


/* 
如果手持该武器，则：
1. 直接清除玩家除肾上腺素、暴怒、抗药性以外的所有负面效果
如果玩家持有可以对敌人造成debuff的无职业阵营武器，则关闭该效果，并给予玩家负面惩罚
2. 如果玩家防御力高为100，则增加10%的伤害减免和0.1秒的无敌帧延长
如果玩家防御力高150，则加30%的伤害减免和0.5的无敌帧
如果玩家防御力高200，则加50%的伤害减免和1.0无敌帧，独立降低最终伤害50%
如果玩家防御力高250，则加80%的伤害减免和2.0的无敌帧，独立降低最终伤害80%，额外提供100的防御，生命回复+60
3. 如果防御为0，且饰品栏只包含翅膀和机动性提升类饰品
那么：+35%伤害，+20%穿甲，+20暴击，玩家造成的最终伤害*4，+20%移动速度，+60秒抗药性，禁用玩家无敌帧，受到伤害+25%
主要攻击形态：
点击特殊按键，能够在三种状态下循环切换


第一形态：落日
点击左键丢出聚能爆破片，只能命中一个敌人，产生爆炸
长按右键可蓄力丢出充能日光矛，飞行一段时间后获得追踪能力并产生滞留爆炸效果
命中敌人后对敌人施加落日余晖，此效果可以让敌人的移动速度减15%，并使所有敌人弹幕的飞行速度减5%
第二形态：勿忘草
左键丢出无限穿透的矛片，每次命中都会生成一条触手
右键可长按并在敌人周围生成传送门，释放额外的弹幕
右键会优先选择线性距离玩家最近的Boss，其次才是小怪
命中敌人后对敌人施加永恒之爱，使敌人伤害减免降低15%
同时让自己只受到80%的敌人接触伤害
第三形态：概念
摁住左键，在玩家身后悬停出4个不同颜色的矛片，他们会依次用泰拉棱镜的方式攻击玩家的血条
每命中一次就会产生玩家屏幕级别的爆炸造成1%伤害，，松手时消除他们

Terraprisma = 156
 	Behavior: Includes the Sanguine Bat
Used by: ProjectileID.BatOfLight, ProjectileID.EmpressBlade

长按右键锁定最近的任意敌人，在敌人周围不断的释放额外的弹幕
持续按住右键10秒在敌人正上方生成一个巨大的弹幕，造成极高伤害
命中敌人后会对玩家自己施加概念支配：所有的敌人弹幕都有5%的概率给玩家回复200滴血
 */














