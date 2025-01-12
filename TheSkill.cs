//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Microsoft.Xna.Framework;
//using System;
//using CalamityMod.Items.Armor.Aerospec;
//using CalamityMod.Items.Weapons.Melee;
//using CalamityThrowingSpear.Global;
//using Terraria.GameInput;

//namespace CalamityThrowingSpear
//{
//    internal class TheSkill : ModPlayer
//    {
//        private bool skillActive = false; // 技能是否激活
//        private int skillTimer = 0; // 技能持续时间计时
//        private int cooldownTimer = 0; // 冷却时间计时
//        private Item originalWeapon; // 保存原始武器
//        private int originalPrefix; // 保存原武器的词缀
//        private Item[] originalArmor = new Item[3]; // 保存原始盔甲
//        public bool skillEnabled = false; // 开关，用于饰品启用技能

//        public override void ResetEffects()
//        {
//            skillEnabled = false; // 每帧重置技能开关
//        }

//        public override void ProcessTriggers(TriggersSet triggersSet)
//        {
//            // 检测是否按下自定义按键 Skill，并且技能未激活且冷却结束且技能开关启用
//            if (KeybindSystem.Skill.JustPressed && !skillActive && cooldownTimer <= 0 && skillEnabled)
//            {
//                ActivateSkill(); // 激活技能
//            }
//        }

//        private void ActivateSkill()
//        {
//            // 保存原武器和词缀
//            originalWeapon = Player.HeldItem.Clone();
//            originalPrefix = Player.HeldItem.prefix;

//            // 保存原盔甲
//            for (int i = 0; i < 3; i++)
//            {
//                originalArmor[i] = Player.armor[i].Clone();
//            }

//            // 替换武器为 WindBlade（风之刃）
//            Player.HeldItem.SetDefaults(ModContent.ItemType<WindBlade>());
//            Player.HeldItem.Prefix(originalPrefix); // 继承原词缀

//            // 替换盔甲为 Aerospec （天蓝）套装
//            Player.armor[0].SetDefaults(ModContent.ItemType<AerospecHelm>());
//            Player.armor[1].SetDefaults(ModContent.ItemType<AerospecBreastplate>());
//            Player.armor[2].SetDefaults(ModContent.ItemType<AerospecLeggings>());

//            // 设置技能状态和计时器
//            skillActive = true;
//            skillTimer = 30 * 60; // 持续 30 秒

//            // 释放技能启动粒子特效
//            SpawnSkillEffect(Player.Center, Color.Cyan, Color.LightBlue);
//        }

//        public override void PostUpdate()
//        {
//            if (skillActive)
//            {
//                // 强制快捷栏保持在技能武器的位置
//                Player.selectedItem = Array.FindIndex(Player.inventory, item => item.type == ModContent.ItemType<WindBlade>());

//                // 技能计时
//                skillTimer--;
//                if (skillTimer <= 0)
//                {
//                    DeactivateSkill(); // 技能结束
//                }
//            }
//            else if (cooldownTimer > 0)
//            {
//                cooldownTimer--; // 冷却计时
//                if (cooldownTimer == 0)
//                {
//                    // 冷却结束时释放粒子特效
//                    SpawnSkillEffect(Player.Center, Color.LightBlue, Color.White);
//                }
//            }
//        }

//        public override void PreUpdate()
//        {
//            if (skillActive)
//            {
//                // 确保盔甲不可被替换
//                Player.armor[0] = Player.armor[0].type == ModContent.ItemType<AerospecHelm>() ? Player.armor[0] : originalArmor[0];
//                Player.armor[1] = Player.armor[1].type == ModContent.ItemType<AerospecBreastplate>() ? Player.armor[1] : originalArmor[1];
//                Player.armor[2] = Player.armor[2].type == ModContent.ItemType<AerospecLeggings>() ? Player.armor[2] : originalArmor[2];
//            }
//        }

//        private void DeactivateSkill()
//        {
//            // 归还原武器
//            if (originalWeapon != null)
//            {
//                Player.inventory[Player.selectedItem] = originalWeapon; // 替换回原始武器
//            }

//            // 归还原盔甲
//            for (int i = 0; i < 3; i++)
//            {
//                if (originalArmor[i] != null)
//                {
//                    Player.armor[i] = originalArmor[i]; // 替换回原始盔甲
//                }
//            }

//            // 设置冷却时间
//            cooldownTimer = 20 * 60; // 冷却 20 秒

//            // 重置技能状态
//            skillActive = false;

//            // 释放技能结束粒子特效
//            SpawnSkillEffect(Player.Center, Color.Cyan, Color.LightBlue);
//        }


//        private void SpawnSkillEffect(Vector2 position, Color primaryColor, Color secondaryColor)
//        {
//            int particleCount = 36; // 粒子数量
//            float radius = 5f * 16f; // 半径

//            // 绘制一个六芒星魔法阵
//            for (int i = 0; i < 6; i++)
//            {
//                float angle = MathHelper.TwoPi * i / 6;
//                Vector2 starPosition = position + angle.ToRotationVector2() * radius;

//                Dust.NewDustPerfect(starPosition, DustID.Smoke, Vector2.Zero, 0, primaryColor, 1.5f).noGravity = true;
//                for (int j = 0; j < 6; j++) // 六芒星边的粒子
//                {
//                    float edgeAngle = angle + MathHelper.TwoPi / 36 * j;
//                    Vector2 edgePosition = position + edgeAngle.ToRotationVector2() * radius * 0.5f;
//                    Dust.NewDustPerfect(edgePosition, DustID.Electric, Vector2.Zero, 0, secondaryColor, 1.2f).noGravity = true;
//                }
//            }
//        }
//    }
//}




