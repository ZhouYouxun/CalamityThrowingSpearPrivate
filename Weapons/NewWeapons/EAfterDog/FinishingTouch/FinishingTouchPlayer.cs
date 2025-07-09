using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.Yharon;
using CalamityThrowingSpear.Global;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.GildedProboscisC;
using CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.SoulHunterJav;
using Microsoft.Xna.Framework;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchPlayer : ModPlayer
    {
        private bool hasPlayedSound = false;
        private bool wasAliveLastFrame = true;

        public override void PreUpdate()
        {
            if (!Player.dead && !wasAliveLastFrame)
            {
                wasAliveLastFrame = true;
            }

            if (Player.dead && wasAliveLastFrame)
            {
                wasAliveLastFrame = false;

                // 遍历玩家背包，重置所有 FinishingTouch 冷却
                for (int i = 0; i < Player.inventory.Length; i++)
                {
                    if (Player.inventory[i].ModItem is FinishingTouch ft && ft.rightClickCooldownTimer > 0)
                    {
                        ft.rightClickCooldownTimer = 0;
                    }
                }
            }
        }
        public override void PostItemCheck()
        {
            // 检查是否启用了独特音效播放的开关
            if (ModContent.GetInstance<CTSConfigs>().EnableFTSound)
            {
                if (Main.zenithWorld)
                {
                    // 检查当前持有的物品是否是 FinishingTouch
                    if (Player.HeldItem.type == ModContent.ItemType<FinishingTouch>())
                    {
                        // 如果未播放过音效，则播放并设置标记
                        if (!hasPlayedSound)
                        {
                            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/RayquazaRoar"), Player.position);
                            hasPlayedSound = true;
                        }
                    }
                    else
                    {
                        // 如果玩家切换了其他武器，则重置标记
                        hasPlayedSound = false;
                    }
                }
            }
        }

        public override void PostUpdateEquips()
        {
            Player player = Main.LocalPlayer;

            if (Player.HeldItem.type == ModContent.ItemType<FinishingTouch>())
            {
                player.statDefense += 25;
                player.endurance += 0.25f;
                player.lifeRegen += 30;
            }

        }

        // 当玩家被 NPC 攻击时调用
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (Player.HeldItem.type == ModContent.ItemType<FinishingTouch>())
            {
                int buffDuration = 10 * 60; // 5 秒钟，单位为帧（每秒 60 帧）
                Player.AddBuff(ModContent.BuffType<CreateVictoryPEBuff>(), 450);
            }
        }
        

        // 当玩家被投射物击中时调用
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (Player.HeldItem.type == ModContent.ItemType<FinishingTouch>())
            {
                int buffDuration = 10 * 60; // 5 秒钟，单位为帧（每秒 60 帧）
                Player.AddBuff(ModContent.BuffType<CreateVictoryPEBuff>(), 450);
            }
        }




        // 每隔10秒生成一条空闲状态下的文本
        private int textTimer = 0; // 计时器，用于控制10秒的间隔

        // 低于7.5的血量播放一次
        private bool bossHealthTriggerActivated = false; // 标记是否已经触发过

        // 拾取起来的时候说一句话
        private bool hasPickedUpFinishingTouch = false; // 标记是否已拥有该武器

        // 丢到地面上的时候说一句话
        private bool hasPlayedDropText = false; // 标记是否已经触发过文本
        private bool hasTriggeredLowHealthEvent;


        //public override void PostUpdate()
        //{
        //    {
        //        // 检查世界上是否有名为 FinishingTouch 的掉落物
        //        bool hasFinishingTouchDropped = Main.item.Any(item => item.active && item.type == ModContent.ItemType<FinishingTouch>());

        //        if (hasFinishingTouchDropped && !hasPlayedDropText)
        //        {
        //            // 播放随机文本
        //            string[] phrases = new string[]
        //            {
        //    "你就这么不要我了？这好吗？这不好",
        //    "我讨厌你"
        //            };
        //            string selectedPhrase = phrases[Main.rand.Next(phrases.Length)]; // 随机选择一句

        //            // 显示文本在玩家头顶
        //            Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
        //            CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);

        //            hasPlayedDropText = true; // 标记为已触发
        //        }
        //        else if (!hasFinishingTouchDropped)
        //        {
        //            hasPlayedDropText = false; // 如果没有掉落物，重置标记
        //        }
        //    }


        //    {
        //        // 检查玩家是否拥有 FinishingTouch
        //        bool currentlyHasFinishingTouch = Player.inventory.Any(item => item.type == ModContent.ItemType<FinishingTouch>());

        //        if (currentlyHasFinishingTouch && !hasPickedUpFinishingTouch)
        //        {
        //            // 播放随机文本
        //            string[] phrases = new string[]
        //            {
        //    "来！与我共同登上无人知晓的巅峰",
        //    "没错！勇于面对，共同前行吧！",
        //    "来！掌握着手中的胜利之星！"
        //            };
        //            string selectedPhrase = phrases[Main.rand.Next(phrases.Length)]; // 随机选择一句
        //            Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
        //            CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);

        //            hasPickedUpFinishingTouch = true; // 设置标记为已拾取
        //        }
        //        else if (!currentlyHasFinishingTouch)
        //        {
        //            hasPickedUpFinishingTouch = false; // 重置标记
        //        }
        //    }


        //    {
        //        // 检查场上是否有一个 Boss 存活并且其血量首次低于 7.5%
        //        NPC activeBoss = Main.npc.FirstOrDefault(npc => npc.boss && npc.active && npc.life < npc.lifeMax * 0.075f);
        //        if (activeBoss != null && !bossHealthTriggerActivated)
        //        {
        //            bossHealthTriggerActivated = true; // 标记为已触发
        //                                               // 播放文本 "分出胜负吧！一击必杀！" 在玩家头顶显示
        //            Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
        //            CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, "分出胜负吧！一击必杀！", false, false);
        //        }
        //    }


        //    {
        //        // 检查条件：玩家背包中是否有 FinishingTouch，场上无Boss存活，且无进行中的事件
        //        bool hasFinishingTouch = Player.inventory.Any(item => item.type == ModContent.ItemType<FinishingTouch>());
        //        bool noBossAlive = !Main.npc.Any(npc => npc.boss && npc.active);
        //        bool noEventActive = Main.invasionType == 0 && !Main.eclipse && !Main.pumpkinMoon && !Main.snowMoon && !Main.bloodMoon;

        //        if (hasFinishingTouch && noBossAlive && noEventActive)
        //        {
        //            textTimer++;
        //            if (textTimer >= 60 * 10) // 10秒 = 60帧 * 10
        //            {
        //                // 随机选择文本
        //                string[] messages = new string[]
        //                {
        //                "捅死你！捅死你！捅死你！",
        //                "无聊...我要看到血流成河！",
        //                "This is my message To my master",
        //                "哈啰...咕德拜！",
        //                "吾有众好友，分为...风筝、台风、电脑..."
        //                };
        //                string selectedMessage = messages[Main.rand.Next(messages.Length)]; // 随机选取一个文本

        //                // 在玩家头顶显示战斗文本
        //                Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
        //                CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedMessage, false, false);

        //                textTimer = 0; // 重置计时器
        //            }
        //        }
        //        else
        //        {
        //            textTimer = 0; // 如果条件不满足，重置计时器
        //        }
        //    }

        //}

        public override void PostUpdate()
        {
            // 检查世界上是否有名为 FinishingTouch 的掉落物
            bool hasFinishingTouchDropped = Main.item.Any(item => item.active && item.type == ModContent.ItemType<FinishingTouch>());
             bool hasFinishingTouch = Player.inventory.Any(item => item.type == ModContent.ItemType<FinishingTouch>());

            if (hasFinishingTouchDropped && !hasPlayedDropText)
            {
                // 播放随机文本
                string[] phrases = new string[]
                {
            "我自天穹而下，犹未见来人"
                };
                string selectedPhrase = phrases[Main.rand.Next(phrases.Length)]; // 随机选择一句

                // 显示文本在玩家头顶
                Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
                CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);

                // 检查是否启用了独特音效播放的开关
                if (ModContent.GetInstance<CTSConfigs>().EnableFTSound)
                {
                    // 播放对应的音效
                    if (selectedPhrase == "我自天穹而下，犹未见来人")
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/xiacangqiong") with { Volume = 1.5f }, Player.Center);
                    }
                }

                hasPlayedDropText = true; // 标记为已触发
            }
            else if (!hasFinishingTouchDropped)
            {
                hasPlayedDropText = false; // 如果没有掉落物，重置标记
            }

            bool currentlyHasFinishingTouch = Player.inventory.Any(item => item.type == ModContent.ItemType<FinishingTouch>());

            {
                // 检查玩家是否拥有 FinishingTouch

                if (currentlyHasFinishingTouch && !hasPickedUpFinishingTouch)
                {
                    // 播放随机文本
                    string[] phrases = new string[]
                    {
            "雷池铸剑，今霜刃即成，当振天下于大白！"
                    };
                    string selectedPhrase = phrases[Main.rand.Next(phrases.Length)]; // 随机选择一句
                    Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
                    CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);

                    // 检查是否启用了独特音效播放的开关
                    if (ModContent.GetInstance<CTSConfigs>().EnableFTSound)
                    {
                        // 播放对应的音效
                        if (selectedPhrase == "雷池铸剑，今霜刃即成，当振天下于大白！")
                        {
                            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/tianjie") with { Volume = 1.5f }, Player.Center);
                        }
                    }

                    hasPickedUpFinishingTouch = true; // 设置标记为已拾取
                }
                else if (!currentlyHasFinishingTouch)
                {
                    hasPickedUpFinishingTouch = false; // 重置标记
                }
            }

            NPC boss = Main.npc.FirstOrDefault(npc => npc.active && npc.boss);
            int bossCount = Main.npc.Count(npc => npc.active && npc.boss);

            if (boss != null && bossCount == 1) // 如果有Boss
            {
                if (hasFinishingTouch && !hasTriggeredLowHealthEvent && boss.life <= boss.lifeMax * 0.1f) // 如果Boss血量低于10%且未触发过
                {
                    // 播放文本
                    string lowHealthMessage = "喊出我的名字吧！";
                    CombatText.NewText(new Rectangle((int)Player.Center.X, (int)Player.Center.Y - 50, 1, 1), Color.Red, lowHealthMessage, true);

                    // 播放音效
                    if (ModContent.GetInstance<CTSConfigs>().EnableFTSound) // 检查是否启用音效
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/Zett"), Player.Center);
                    }

                    // 给所有玩家添加 10 秒 FinishingTouch10PBuff
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player p = Main.player[i];
                        if (p.active && !p.dead)
                        {
                            p.AddBuff(ModContent.BuffType<FinishingTouch10PBuff>(),600); // 600 = 10秒
                        }
                    }

                    // 设置标记为已触发
                    hasTriggeredLowHealthEvent = true;
                }
            }
            else
            {
                // 如果没有Boss存活，重置标记
                hasTriggeredLowHealthEvent = false;
            }






            {
                // 检查条件：玩家背包中是否有 FinishingTouch，场上无Boss存活，且无进行中的事件
                bool noBossAlive = !Main.npc.Any(npc => npc.boss && npc.active);
                bool noEventActive = Main.invasionType == 0 && !Main.eclipse && !Main.pumpkinMoon && !Main.snowMoon && !Main.bloodMoon;

                if (hasFinishingTouch && noBossAlive && noEventActive)
                {
                    textTimer++;
                    if (textTimer >= 60 * 15) // 15秒 = 60帧 * 10
                    {
                        // 随机选择文本
                        string[] messages = new string[]
                        {
                "捅死你！捅死你！捅死你！",
                "无聊...我要看到血流成河！",
                "This is my message To my master",
                "哈啰...咕德拜！",
                "吾有众好友，分为...风筝、台风、电脑...",
                "公既知天命，识时务，何不倒戈卸甲、卸衣、卸底裤",
                "大丈夫生居天地间，岂能郁郁久居人下",
                "公若不弃，我愿拜为义父",
                "一人超越神明，一人四级烧伤，此可行邪！？",
                "国祚尚为泰，天子尚是星，岂有妄为之理！？",
                "我自冷眼看世界，不问天下是与非",
                "步步为营者，定无后顾之虞",
                "明公彀中藏龙卧虎，放之海内皆可称贤",
                "今提墨笔绘乾坤，湖海添色山永春",
                "手提玉剑斥千军，昔日锦鲤化金龙",
                "你我兄弟齐上，焉有一合之将？",
                "筋疲力尽？厌倦。想要休息？肚子饿了。想离开这里？",
                "平战乱，享太平",
                "8848画龙点睛",
                "ZI-0...GRAND ZI-O"
                        };
                        string selectedMessage = messages[Main.rand.Next(messages.Length)]; // 随机选取一个文本

                        // 在玩家头顶显示战斗文本
                        Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
                        CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedMessage, false, false);

                        // 检查是否启用了独特音效播放的开关
                        if (ModContent.GetInstance<CTSConfigs>().EnableFTSound)
                        {
                            // 播放对应的音效
                            if (selectedMessage == "捅死你！捅死你！捅死你！")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/tongsini") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "无聊...我要看到血流成河！")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/xueliuchenghe") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "This is my message To my master")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/Thisis") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "哈啰...咕德拜！")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/hello") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "吾有众好友，分为...风筝、台风、电脑...")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/wuyouzhonghaoyou") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "公既知天命，识时务，何不倒戈卸甲、卸衣、卸底裤")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/zhitianmingshishiwu") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "大丈夫生居天地间，岂能郁郁久居人下")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/lvbu1") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "公若不弃，我愿拜为义父")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/lvbu2") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "一人超越神明，一人四级烧伤，此可行邪！？")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/cikexing") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "国祚尚为泰，天子尚是星，岂有妄为之理！？")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/qiyouwangweizhili") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "我自冷眼看世界，不问天下是与非")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/buwentianxiashiyufei") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "步步为营者，定无后顾之虞")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/bubuweiying") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "明公彀中藏龙卧虎，放之海内皆可称贤")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/canglongwohu") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "今提墨笔绘乾坤，湖海添色山永春")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/jintimobi") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "手提玉剑斥千军，昔日锦鲤化金龙")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/shoutiyujian") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "ZI-0...GRAND ZI-O")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/GrandTime") with { Volume = 2.5f }, Player.Center);
                            }
                            else if (selectedMessage == "你我兄弟齐上，焉有一合之将？")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/Wegotogether") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "平战乱，享太平")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/pingzhanluan") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "8848画龙点睛")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/8848") with { Volume = 2f }, Player.Center);
                            }
                            else if (selectedMessage == "筋疲力尽？厌倦。想要休息？肚子饿了。想离开这里？")
                            {
                                SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/Roland") with { Volume = 2f }, Player.Center);
                            }
                        }
                        textTimer = 0; // 重置计时器
                    }
                }
                else
                {
                    textTimer = 0; // 如果条件不满足，重置计时器
                }

            }

        }









        //public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        //{
        //    if (Player.HeldItem.type == ModContent.ItemType<FinishingTouch>())
        //    {
        //        // 显示随机文本
        //        string[] phrases = new string[]
        //        {
        //"上吧！起死回生！",
        //"绝望中，仍存有一线生机！",
        //"还不可以认输！"
        //        };
        //        string selectedPhrase = phrases[Main.rand.Next(phrases.Length)]; // 随机选择一句
        //        Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
        //        CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);
        //    }

        //}

        //public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        //{
        //    if (Player.HeldItem.type == ModContent.ItemType<FinishingTouch>())
        //    {
        //        // 显示随机文本
        //        string[] phrases = new string[]
        //        {
        //"上吧！起死回生！",
        //"绝望中，仍存有一线生机！",
        //"还不可以认输！"
        //        };
        //        string selectedPhrase = phrases[Main.rand.Next(phrases.Length)]; // 随机选择一句
        //        Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
        //        CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);
        //    }

        //}
        
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            bool hasFinishingTouch = Player.inventory.Any(item => item.type == ModContent.ItemType<FinishingTouch>());
            if (hasFinishingTouch)
            {
                ShowCombatTextWithSound();
            }
            
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            bool hasFinishingTouch = Player.inventory.Any(item => item.type == ModContent.ItemType<FinishingTouch>());
            if (hasFinishingTouch)
            {
                ShowCombatTextWithSound();
            }
        }

        private void ShowCombatTextWithSound()
        {
            // 显示随机文本
            string[] phrases = new string[]
            {
        "龙翔九天，曳日月于天地，换旧符于新岁",
        "御风万里，辟邪祟于宇外，映祥瑞于神州",
        "绝望中，仍存有一线生机！",
        "还不可以认输！",
        "龙战于野，其血玄黄！",
        "左执青釭，右擎龙胆，此天下可有挡我者！？",
        "背水立阵仿韩信，破釜沉舟学霸王",
        "纵使困顿难行，亦当砥砺奋进",
        "七尺男儿，有战无降",
        "往来皆死战，热血盈袍铠！"
            };
            string selectedPhrase = phrases[Main.rand.Next(phrases.Length)]; // 随机选择一句
            Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
            CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);


            // 检查是否启用了独特音效播放的开关
            if (ModContent.GetInstance<CTSConfigs>().EnableFTSound)
            {
                // 播放对应的语音
                if (selectedPhrase == "龙翔九天，曳日月于天地，换旧符于新岁")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/longxiangjiutian") with { Volume = 2.5f }, Player.Center);
                }
                if (selectedPhrase == "御风万里，辟邪祟于宇外，映祥瑞于神州")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/yufengwanli") with { Volume = 2.5f }, Player.Center);
                }
                else if (selectedPhrase == "绝望中，仍存有一线生机！")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/juewangzhong") with { Volume = 2.5f }, Player.Center);
                }
                else if (selectedPhrase == "还不可以认输！")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/haibukeyirenshu") with { Volume = 2.5f }, Player.Center);
                }
                else if (selectedPhrase == "龙战于野，其血玄黄！")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/longzhanyuye") with { Volume = 2.5f }, Player.Center);
                }
                else if (selectedPhrase == "左执青釭，右擎龙胆，此天下可有挡我者！？")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/juejing") with { Volume = 2.5f }, Player.Center);
                }
                else if (selectedPhrase == "背水立阵仿韩信，破釜沉舟学霸王")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/beishui") with { Volume = 2.5f }, Player.Center);
                }
                else if (selectedPhrase == "纵使困顿难行，亦当砥砺奋进")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/kundun") with { Volume = 2.5f }, Player.Center);
                }
                else if (selectedPhrase == "七尺男儿，有战无降")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/qichinaner") with { Volume = 2.5f }, Player.Center);
                }
                else if (selectedPhrase == "往来皆死战，热血盈袍铠！")
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/rexueyingpaokai") with { Volume = 2.5f }, Player.Center);
                }
            }

        }



        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (Player.HeldItem.type == ModContent.ItemType<FinishingTouch>())
            {
                // 随机选择文本
                string[] phrases = new string[]
                {
            "哦死了啦，都是你害的啦",
            "菜就多练练",
            "无耻小人，胆敢暗算于我",
            "七情难掩，六欲难消，何谓之神",
            "吾主公短命无妨",
                };
                string selectedPhrase = phrases[Main.rand.Next(phrases.Length)]; // 随机选择一句

                // 显示文本在玩家头顶
                Vector2 textPosition = Player.Center - new Vector2(0, Player.height / 2 + 20f); // 玩家头顶位置
                CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);

                // 检查是否启用了独特音效播放的开关
                if (ModContent.GetInstance<CTSConfigs>().EnableFTSound)
                {
                    // 播放对应的语音
                    if (selectedPhrase == "哦死了啦，都是你害的啦")
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/silela") with { Volume = 2.5f }, Player.Center);
                    }
                    else if (selectedPhrase == "吾主公短命无妨")
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/wuzhuduanming") with { Volume = 2f }, Player.Center);
                    }
                    else if (selectedPhrase == "无耻小人，胆敢暗算于我")
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/wuchixiaoren") with { Volume = 2.5f }, Player.Center);
                    }
                    else if (selectedPhrase == "七情难掩，六欲难消，何谓之神")
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/qiqingnanyan") with { Volume = 2.5f }, Player.Center);
                    }
                    else if (selectedPhrase == "菜就多练练")
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/caijiuduolianlian") with { Volume = 2.5f }, Player.Center);
                    }
                }

            }
        }
        //当任意BOSS的生命值达到最大生命值的10%时，播放此文本
        //"喊出我的名字吧！"
        //"CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/tianjie"
        
    }
}



