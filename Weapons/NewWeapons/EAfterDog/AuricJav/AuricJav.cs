using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Items.DraedonMisc;
using CalamityMod.Rarities;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ChaosEssenceJav;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using CalamityMod;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.AuricJav
{
    public class AuricJav : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "NewWeapons.EAfterDog";
        public float AngerMeter = 0f; // 愤怒值（0~100）
        public float MaxAnger = 100f;

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 85; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 26; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.shoot = ModContent.ProjectileType<AuricJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 12.5f; // 更改使用时的武器弹幕飞行速度
        }
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键
            {
                return AngerMeter >= 10; // 至少有 10% 才能释放
            }
            else // 左键
            {
                //return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<AuricJavLighting>());
                return true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2) // 右键
            {
                float multiplier = 1 + 3 * (AngerMeter / MaxAnger);
                damage = (int)(damage * multiplier);

                // 让闪电初始速度翻X倍
                Vector2 lightningVelocity = velocity * 1f;

                Projectile.NewProjectile(source, position, lightningVelocity, ModContent.ProjectileType<AuricJavLighting>(), damage, knockback, player.whoAmI);

                AngerMeter = 0; // 释放后清空

                // 屏幕震动效果
                float shakePower = 4f; // 震动强度
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, player.Distance(Main.LocalPlayer.Center), true); // 震动随距离衰减
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                // 播放音效
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ArcNovaDiffuserBigShot"), player.Center);

                return false;
            }
            else // 左键
            {
                if (AngerMeter >= 80)
                {
                    int lifeDrain = Main.rand.Next(1, 4);
                    player.statLife -= lifeDrain;
                    //CombatText.NewText(player.getRect(), Color.Red, lifeDrain.ToString(), true);

                    // 生成血红色和电能粒子特效
                    for (int i = 0; i < 15; i++) // 生成 x 个粒子
                    {
                        Vector2 spawnPosition = player.Center + Main.rand.NextVector2Circular(24f, 24f); // 以玩家为中心，随机生成 24x24 范围内的 Dust
                        int dustType = Main.rand.NextBool() ? DustID.Blood : DustID.Electric; // 随机选择血红色 Dust 或电能 Dust
                        Dust dust = Dust.NewDustPerfect(spawnPosition, dustType, Vector2.Normalize(player.Center - spawnPosition) * Main.rand.NextFloat(1f, 3f), 100, Color.Red, Main.rand.NextFloat(1.2f, 1.8f));
                        dust.noGravity = true; // 让粒子悬浮
                        dust.fadeIn = 1.2f;
                    }

                    damage = (int)(damage * 1.5f);
                }
                else if (AngerMeter >= 40)
                {
                    damage = (int)(damage * 1.25f);
                }

                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<AuricJavPROJ>(), damage, knockback, player.whoAmI);

                AngerMeter += Main.rand.NextFloat(3f, 5f);
                if (AngerMeter > MaxAnger) AngerMeter = MaxAnger;

                return false;
            }
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.LocalPlayer.HeldItem.type != Item.type)
                return;

            if (AngerMeter <= 0f)
                return;

            float barScale = 3.34f;
            var barBG = Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            var barFG = Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            Vector2 barOrigin = barBG.Size() * 0.5f;
            float yOffset = 20f; // 往下的偏移（可调）
            Vector2 drawPos = position + Vector2.UnitY * scale * (frame.Height - yOffset);

            Rectangle frameCrop = new Rectangle(0, 0, (int)(AngerMeter / MaxAnger * barFG.Width), barFG.Height);
            Color barColor = AngerMeter < 40 ? Color.Green : AngerMeter < 80 ? Color.Yellow : Color.Red;

            spriteBatch.Draw(barBG, drawPos, null, barColor, 0f, barOrigin, scale * barScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(barFG, drawPos, frameCrop, barColor * 0.8f, 0f, barOrigin, scale * barScale, SpriteEffects.None, 0f);
        }




        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HallowJoustingLance, 1);
            recipe.AddIngredient<AuricQuantumCoolingCell>();
            recipe.AddIngredient(ItemID.Wire, 5);
            recipe.AddTile<CosmicAnvil>();
            recipe.Register();
        }
    }
}
