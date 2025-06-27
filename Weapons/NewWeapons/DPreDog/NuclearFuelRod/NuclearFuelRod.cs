using CalamityMod.Items.Materials;
using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    internal class NuclearFuelRod : ModItem, ILocalizedModType
    {
        private const int CooldownMax = 1800;
        private int cooldownTimer = 0;

        public new string LocalizationCategory => "NewWeapons.DPreDog";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 1145; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 10; // 更改使用时的武器攻击速度
            Item.knockBack = 18f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<NuclearFuelRodPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 8f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
        }
        public override bool CanUseItem(Player player)
        {
            // 冷却期间禁止使用
            return cooldownTimer <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 强制将发射方向改为【正上方】
            Vector2 shootVelocity = Vector2.UnitY * -Item.shootSpeed;

            // 发射弹幕
            Projectile.NewProjectile(
                source,
                position,
                shootVelocity,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            // 发射后立即开始冷却
            cooldownTimer = CooldownMax;


            return false; // 阻止默认发射逻辑，使用自定义发射方向
        }

        public override void UpdateInventory(Player player)
        {
            if (cooldownTimer > 0)
                cooldownTimer--;
        }


        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (cooldownTimer <= 0)
                return;

            // 进度条贴图
            var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            float barScale = 0.8f;
            Vector2 drawPos = position + Vector2.UnitY * (frame.Height - 4f) * scale;

            // 进度百分比（反向，填满后缓慢下降）
            float progress = 1f - cooldownTimer / (float)CooldownMax;
            Rectangle frameCrop = new Rectangle(0, 0, (int)(barFG.Width * progress), barFG.Height);

            Color barColor = progress < 0.33f ? Color.Green : (progress < 0.66f ? Color.Yellow : Color.Red);

            // 绘制背景
            spriteBatch.Draw(barBG, drawPos, null, barColor * 0.6f, 0f, Vector2.Zero, barScale, SpriteEffects.None, 0f);
            // 绘制填充
            spriteBatch.Draw(barFG, drawPos, frameCrop, barColor, 0f, Vector2.Zero, barScale, SpriteEffects.None, 0f);
        }







    }
}
