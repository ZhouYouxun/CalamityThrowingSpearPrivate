using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items;
using CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.GraniteJav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.SHPCK;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.Revelation;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.ElectrocutionHalberd
{
    public class ElectrocutionHalberd : ModItem, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/ElectrocutionHalberd/ElectrocutionHalberdJav";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
            //Item.staff[Item.type] = true;
            ItemID.Sets.Spears[Item.type] = true;
        }
        public override bool AltFunctionUse(Player player) => true;
        public new string LocalizationCategory => "NewWeapons.BPrePlantera";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 50;
            Item.damage = 125; // 设置伤害值
            Item.DamageType = DamageClass.Melee; // 设置为近战武器
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing; // 更改使用模式为投掷
            Item.useTime = Item.useAnimation = 38; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.shoot = ModContent.ProjectileType<ElectrocutionHalberdPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 10f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4

            Item.autoReuse = true;
            Item.channel = true; // 允许持续按住左键
        }

        //public override void UseItemFrame(Player player)
        //{
        //    if (player.altFunctionUse == 2) // 仅右键时启用动画
        //    {
        //        player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

        //        // 计算动画进度
        //        float animProgress = 1 - player.itemTime / (float)player.itemTimeMax;

        //        // 调整枪口旋转角度，使贴图正确对齐
        //        float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir - MathHelper.PiOver4;
        //                        //+ MathHelper.PiOver4 + MathHelper.ToRadians(25) + MathHelper.ToRadians(25);

        //        // 在动画前半段抬起武器
        //        if (animProgress < 0.5f)
        //            rotation += -0.45f * (float)Math.Pow((0.5f - animProgress) / 0.5f, 2) * player.direction;

        //        // 设置前臂的旋转和拉伸
        //        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);

        //        // 在动画后半段模拟装填动作
        //        if (animProgress > 0.5f)
        //        {
        //            float backArmRotation = rotation + 0.52f * player.direction;
        //            Player.CompositeArmStretchAmount stretch = ((float)Math.Sin(MathHelper.Pi * (animProgress - 0.5f) / 0.36f)).ToStretchAmount();
        //            player.SetCompositeArmBack(true, stretch, backArmRotation);
        //        }
        //    }
        //}

        //public override void UseStyle(Player player, Rectangle heldItemFrame)
        //{
        //    if (player.altFunctionUse == 2) // 仅右键时启用动画逻辑
        //    {
        //        player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

        //        // 设置武器的旋转和位置，调整旋转角度和偏移
        //        float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 + MathHelper.PiOver4 + MathHelper.ToRadians(25);
        //        Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * 35f;

        //        // 调整旋转中心
        //        Vector2 itemSize = new Vector2(Item.width, Item.height);
        //        Vector2 itemOrigin = new Vector2(-20, -0); // 使用 HoldoutOffset 定义的左下角偏移，这是中心，想要改的话可以改这两个数

        //        // 调用 Calamity 的自定义工具进行清理
        //        CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin);
        //    }
        //}

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2) // 右键
            {
                Item.damage = 30; // 设置伤害值
                Item.useTime = 50;
                Item.useAnimation = 50;
                Item.useLimitPerAnimation = 1;
                Item.shoot = ModContent.ProjectileType<ElectrocutionHalberdRIGHTJav>();
                Item.shootSpeed = 15f;
                Item.UseSound = SoundID.Item73;
                Item.useStyle = ItemUseStyleID.Swing;
                Item.noUseGraphic = true;
            }
            else // 左键
            {
                Item.damage = 125;
                Item.useTime = Item.useAnimation = 70;
                Item.shootSpeed = 10f;
                Item.shoot = ModContent.ProjectileType<ElectrocutionHalberdPROJ>();
                Item.UseSound = null;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.noUseGraphic = true;
            }
            return base.CanUseItem(player);
        }

        //public override Vector2? HoldoutOffset() => new Vector2(-100, -100);
        //public override Vector2? HoldoutOrigin()
        //{
        //    // 调整贴图的原点
        //    return new Vector2(-500, -500);
        //}
        //public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        //{
        //    // 获取当前物品的贴图
        //    Texture2D texture = Terraria.GameContent.TextureAssets.Item[Item.type].Value;

        //    // 计算贴图的原点（中心点）
        //    Vector2 origin = texture.Size() * 0.5f;

        //    // 调整旋转角度（增加额外的旋转）
        //    float customRotation = rotation + MathHelper.PiOver4 + MathHelper.ToRadians(25);

        //    // 在世界中绘制物品
        //    spriteBatch.Draw(
        //        texture,                           // 贴图
        //        Item.Center - Main.screenPosition, // 绘制位置
        //        null,                              // 贴图矩形（null 表示整张贴图）
        //        lightColor,                        // 光照颜色
        //        customRotation,                    // 自定义旋转角度
        //        origin,                            // 旋转中心
        //        scale,                             // 缩放
        //        SpriteEffects.None,                // 水平翻转或垂直翻转
        //        0f                                 // 图层深度
        //    );
        //}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //if (player.altFunctionUse == 2) // 右键攻击时
            //{
            //    // 生成三发子弹
            //    for (int i = 0; i < 3; i++)
            //    {
            //        // 随机角度偏移（-10度到10度之间）
            //        float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f));
            //        Vector2 modifiedVelocity = velocity.RotatedBy(angleOffset);

            //        // 随机初始速度调整（0.85倍到1.25倍之间）
            //        float speedMultiplier = Main.rand.NextFloat(0.85f, 1.25f);
            //        modifiedVelocity *= speedMultiplier;

            //        // 随机伤害倍率（0.95倍到1.5倍之间）
            //        float damageMultiplier = Main.rand.NextFloat(0.95f, 1.5f);

            //        // 创建子弹
            //        Projectile.NewProjectile(
            //            source,
            //            position,
            //            modifiedVelocity,
            //            type,
            //            (int)(damage * damageMultiplier),
            //            knockback,
            //            player.whoAmI
            //        );
            //    }

            //    // 生成粒子特效
            //    //CreateParticleEffect(position, velocity);

            //    return false; // 阻止生成默认弹幕
            //}

            if (player.altFunctionUse == 2) // 右键攻击
            {
                // 默认发射一发子弹，目标为鼠标位置
                Vector2 mouseDirection = Vector2.Normalize(Main.MouseWorld - position) * velocity.Length();
                Projectile.NewProjectile(source, position, mouseDirection, type, damage, knockback, player.whoAmI);

                return false; // 阻止生成默认弹幕
            }

            // 遍历当前世界中的所有弹幕
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == Item.shoot)
                {
                    // 检查是否为 Aim 状态
                    if (proj.ModProjectile is ElectrocutionHalberdPROJ P && P.CurrentState == ElectrocutionHalberdPROJ.BehaviorState.Aim)
                    {
                        return false; // 如果已经存在一个 Aim 状态的弹幕，阻止新的生成
                                      // Fire 阶段的弹幕不会影响这个判断
                    }
                }
            }
            // 左键攻击逻辑 - 创建新的弹幕
            int projIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false; // 阻止生成默认弹幕
        }
        private void CreateParticleEffect(Vector2 position, Vector2 velocity)
        {
            int particleCount = Main.rand.Next(135, 196); // 粒子数量：135~195
            for (int i = 0; i < particleCount; i++)
            {
                // 随机选择 Dust 类型
                int dustType = Main.rand.Next(new int[] { DustID.Electric, DustID.UltraBrightTorch, DustID.IceTorch });

                // 随机生成粒子速度和方向
                Vector2 randomVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(180)) * Main.rand.NextFloat(6f, 20f);

                // 创建 Dust
                Dust dust = Dust.NewDustPerfect(position, dustType, randomVelocity, 150, default, Main.rand.NextFloat(1.5f, 1.95f));
                dust.noGravity = true; // 粒子无重力
            }
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup("AnyAdamantiteBar", 10);
            recipe.AddIngredient(ItemID.CrystalShard, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }


        


    }
}
