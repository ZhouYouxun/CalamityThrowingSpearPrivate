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
using CalamityRangerExpansion.LightingBolts;
using Terraria.Audio;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    public class NuclearFuelRod : ModItem, ILocalizedModType
    {
        private const int CooldownMax = 180;
        public int cooldownTimer = 0;

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
            // 玩家死亡时立即清空冷却
            if (player.dead && cooldownTimer > 0)
            {
                cooldownTimer = 0;
                return;
            }
            if (cooldownTimer > 0)
                cooldownTimer--;

            if (cooldownTimer == 1 && player.whoAmI == Main.myPlayer)
            {
                // 🚩 播放声音
                SoundEngine.PlaySound(SoundID.Item122, player.Center);

                // 🚩 冷却结束瞬间触发核燃料棒核能放射冲击波特效
                Particle nuclearPulse = new CustomPulse(
                    player.Center, // 粒子生成位置：玩家中心
                    Vector2.Zero,  // 粒子静止不动
                    Color.LimeGreen * 0.8f, // 放射性荧光绿，带透明度
                    "CalamityThrowingSpear/Texture/IonizingRadiation", // 使用自备辐射贴图
                    new Vector2(1f, 1f), // 正圆，无椭圆变形
                    Main.rand.NextFloat(-5f, 5f), // 随机轻微旋转
                    0.05f, // 初始缩放（略大，能量感）
                    0.25f, // 最终缩放（快速扩散）
                    24      // 粒子存活时间（24 帧，约 0.4 秒快速扩散）
                );
                // 生成特效粒子
                GeneralParticleHandler.SpawnParticle(nuclearPulse);

                int ringCount = 8;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi / ringCount * i;
                    Vector2 offset = angle.ToRotationVector2() * 40f;
                    Vector2 pos = player.Center + offset;

                    // 创建一次性临时粒子显示
                    Dust d = Dust.NewDustPerfect(pos, DustID.GreenTorch, Vector2.Zero, 0, Color.LimeGreen, 1.8f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                {
                    int leafCount = 3;
                    int pointsPerLeaf = 40; // 更细腻
                    float radius = 180f; // 半径扩大 3 倍

                    for (int leaf = 0; leaf < leafCount; leaf++)
                    {
                        float leafAngle = MathHelper.TwoPi / leafCount * leaf;
                        for (int p = 0; p < pointsPerLeaf; p++)
                        {
                            float angle = leafAngle + MathHelper.ToRadians(15f) * (p / (float)pointsPerLeaf);
                            float dist = radius * (p / (float)pointsPerLeaf);

                            Vector2 pos = player.Center + angle.ToRotationVector2() * dist;

                            int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.GreenTorch;
                            float scale = Main.rand.NextFloat(2.5f, 4f);
                            Dust d = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 0, Color.LimeGreen, scale);
                            d.noGravity = true;
                            d.fadeIn = Main.rand.NextFloat(1.5f, 2f);
                        }
                    }

                    // 外环加粗
                    for (int i = 0; i < 100; i++)
                    {
                        float angle = MathHelper.TwoPi / 100 * i;
                        Vector2 pos = player.Center + angle.ToRotationVector2() * (radius + 20f); // 半径 200
                        int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.GreenTorch;
                        Dust d = Dust.NewDustPerfect(pos, dustType, Vector2.Zero, 0, Color.LimeGreen, 3f);
                        d.noGravity = true;
                        d.fadeIn = 1.5f;
                    }

                    // 添加线性放射线以制造 3D 立体感
                    int rayCount = 24;
                    for (int r = 0; r < rayCount; r++)
                    {
                        float angle = MathHelper.TwoPi / rayCount * r;
                        for (float dist = 50f; dist <= radius + 20f; dist += 15f)
                        {
                            Vector2 pos = player.Center + angle.ToRotationVector2() * dist;
                            Dust d = Dust.NewDustPerfect(pos, DustID.GreenTorch, angle.ToRotationVector2() * 4f, 0, Color.LimeGreen, 2.5f);
                            d.noGravity = true;
                            d.fadeIn = 1.5f;
                        }
                    }

                    // 添加 SparkParticle 能量线性放射以制造真实 3D 辐射感
                    int sparkRayCount = 38; // 辐射线数量
                    for (int r = 0; r < sparkRayCount; r++)
                    {
                        float angle = MathHelper.TwoPi / sparkRayCount * r + Main.rand.NextFloat(-0.05f, 0.05f); // 微扰增加层次
                        Vector2 direction = angle.ToRotationVector2();

                        // 每条射线分段制造多段光带，模拟能量流动感
                        for (int s = 0; s < 5; s++)
                        {
                            float distance = 40f + s * 30f + Main.rand.NextFloat(-5f, 5f); // 起点到远点，带轻微扰动
                            Vector2 spawnPos = player.Center + direction * distance;
                            Vector2 velocity = direction * Main.rand.NextFloat(2f, 6f); // 不同速度形成流动感
                            Color color = Color.Lerp(Color.LimeGreen, Color.GreenYellow, Main.rand.NextFloat(0.3f, 0.7f)); // 深浅交错

                            Particle spark = new SparkParticle(
                                spawnPos,
                                velocity,
                                false, // 不受重力影响
                                Main.rand.Next(40, 70), // 生命周期随机，流动感
                                Main.rand.NextFloat(0.8f, 1.6f), // 缩放大小随机，形成立体层次
                                color * 0.9f // 保留透明度
                            );
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                    }

                }


                // 🚩 CRE GaussDischargeShards 核能电光闪烁
                int creCount = Main.rand.Next(2, 5); // 随机生成 2 ~ 4 个
                for (int i = 0; i < creCount; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(80f, 80f); // 在玩家周围 80 像素随机散布
                    CTSLightingBoltsSystem.Spawn_GaussDischargeShards(player.Center + offset);
                }
            }

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
