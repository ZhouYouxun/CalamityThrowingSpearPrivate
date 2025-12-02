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
using Microsoft.Xna.Framework.Graphics;
using Terraria.Localization;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer;
using Terraria.GameContent;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset
{
    public class Sunset : ModItem, ILocalizedModType
    {
        public static Texture2D TextureA;
        public static Texture2D TextureB;
        public static Texture2D TextureC;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                TextureA = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/SunsetA").Value;
                TextureB = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/SunsetB").Value;
                TextureC = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/SunsetC").Value;
            }
        }


        // 主函数：优先用缓存，不行再即时请求，最后兜底
        // 直接即时加载，不依赖静态缓存
        private Texture2D GetTextureByMode()
        {
            string basePath = "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/";
            string name = currentMode switch
            {
                0 => "SunsetA",
                1 => "SunsetB",
                2 => "SunsetC",
                _ => "SunsetA"
            };

            return ModContent.Request<Texture2D>(basePath + name, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        }


        public override void UpdateInventory(Player player)
        {
            switch (currentMode)
            {
                case 0:
                    Item.SetNameOverride("落日"); // 可选
                    Item.width = 44;
                    Item.height = 50;
                    break;

                case 1:
                    Item.SetNameOverride("勿忘草");
                    Item.width = 44;
                    Item.height = 50;
                    break;

                case 2:
                    Item.SetNameOverride("概念");
                    Item.width = 44;
                    Item.height = 50;
                    break;
            }
        }
        // 通用能量粒子系统
        internal static ChargingEnergyParticleSet SunsetEnergyParticles = new ChargingEnergyParticleSet(-1, 2, Color.White, Color.White, 0.04f, 20f);


        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = GetTextureByMode();

            // 三种贴图不同的缩放倍率
            float customScale = scale;

            if (currentMode == 0)       // 第一种贴图
                customScale *= 0.30f;
            else if (currentMode == 1)  // 第二种贴图
                customScale *= 0.30f;
            else if (currentMode == 2)  // 第三种贴图
                customScale *= 0.30f;

            // 手动绘制
            spriteBatch.Draw(
                tex,
                position,
                null,
                drawColor,
                0f,
                tex.Size() * 0.5f,
                customScale,
                SpriteEffects.None,
                0f
            );

            return false; // 阻止默认绘制
        }



        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = GetTextureByMode();
            Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);
            spriteBatch.Draw(tex, position, null, lightColor, rotation, tex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            return false;
        }








        public new string LocalizationCategory => "NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            //ItemID.Sets.Spears[Item.type] = true;
            //Item.staff[Item.type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        private int rightClickCooldown = 0; // 右键冷却计时器
        private int currentMode = 0; // 当前形态 (0 = A, 1 = B, 2 = C)
        private static readonly string[] modeNames = {
            Language.GetTextValue("Mods.CalamityThrowingSpear.TheSpecialText.Sunset1"),
            Language.GetTextValue("Mods.CalamityThrowingSpear.TheSpecialText.Sunset2"),
            Language.GetTextValue("Mods.CalamityThrowingSpear.TheSpecialText.Sunset3")
        };
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
            Item.useTime = Item.useAnimation = 15; // 更改使用时的武器攻击速度
            Item.knockBack = 8.5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;


            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;

            Item.shoot = ModContent.ProjectileType<SoulHunterJavPROJ>(); // 使用新的弹幕
            Item.shootSpeed = 27f; // 更改使用时的武器弹幕飞行速度
            Item.crit = 4; // 基础暴击率都是4
            Item.channel = true; // 确保右键能够长按
        }
        // 公开当前形态的只读访问
        public int CurrentMode => currentMode;   // 0=A, 1=B, 2=C
        public bool IsCMode => currentMode == 2; // 是否是 C 形态

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            // 必须：Calamity 的右键监听
            if (Main.myPlayer == player.whoAmI)
                player.Calamity().rightClickListener = true;

            // 本地右键冷却（不是概念形态的全局冷却）
            if (rightClickCooldown > 0)
                rightClickCooldown--;

            // ==========================
            //  形态切换逻辑（保持原样）
            // ==========================
            if (KeybindSystem.WeaponSkill.JustPressed)
            {
                currentMode = (currentMode + 1) % 3;

                string modeName = Language.GetTextValue(
                    $"Mods.CalamityThrowingSpear.TheSpecialText.Sunset{currentMode + 1}"
                );

                CombatText.NewText(player.getRect(), Color.Red, modeName);
                SoundEngine.PlaySound(SoundID.Item4, player.position);
                PlaySwitchEffect(player.Center, currentMode);
            }

            // ==========================
            //  C 形态的全局冷却判定
            // ==========================
            var modPlayer = player.GetModPlayer<ConceptRightCooldown>();

            // ⭐ 冷却期间：禁止 C 形态右键启动
            if (currentMode == 2 && modPlayer.IsConceptCooling)
            {
                // 玩家按住右键时，也要阻止进入 Aim 阶段
                if (player.Calamity().mouseRight)
                    return;

                // 如果不是按右键，只是普通 HoldItem 流程，也直接 return
                return;
            }

            // ==========================
            //  右键行为触发逻辑（保持原结构）
            // ==========================
            if (player.Calamity().mouseRight &&
                rightClickCooldown == 0 &&
                CanUseItem(player) &&
                player.whoAmI == Main.myPlayer &&
                !Main.mapFullscreen &&
                !Main.blockMouse)
            {
                // 禁止在左键动画期间触发右键
                if (player.itemAnimation > 0 && player.altFunctionUse != 2)
                    return;

                // ⛔ 检查是否已经存在右键 Aim 投射物（所有三个形态）
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI)
                    {
                        if (proj.type == ModContent.ProjectileType<SunsetASunsetRight>() &&
                            proj.ModProjectile is SunsetASunsetRight rightProj &&
                            rightProj.CurrentState == SunsetASunsetRight.BehaviorState.Aim)
                            return;

                        if (proj.type == ModContent.ProjectileType<SunsetBForgetRight>() &&
                            proj.ModProjectile is SunsetBForgetRight forgetProj &&
                            forgetProj.CurrentState == SunsetBForgetRight.BehaviorState.Aim)
                            return;

                        if (proj.type == ModContent.ProjectileType<SunsetCConceptRight>() &&
                            proj.ModProjectile is SunsetCConceptRight conceptProj &&
                            conceptProj.CurrentState == SunsetCConceptRight.BehaviorState.Aim)
                            return;
                    }
                }

                // 确定右键主弹幕类型
                int projType = currentMode switch
                {
                    0 => ModContent.ProjectileType<SunsetASunsetRight>(),
                    1 => ModContent.ProjectileType<SunsetBForgetRight>(),
                    2 => ModContent.ProjectileType<SunsetCConceptRight>(),
                    _ => ModContent.ProjectileType<SunsetASunsetRight>()
                };

                int damage = (int)player.GetTotalDamage<MeleeDamageClass>().ApplyTo(Item.damage);
                float kb = player.GetTotalKnockback<MeleeDamageClass>().ApplyTo(Item.knockBack);

                // 生成右键主投射物
                int projIndex = Projectile.NewProjectile(
                    Item.GetSource_FromThis(),
                    player.Center,
                    Vector2.Zero,
                    projType,
                    damage,
                    kb,
                    player.whoAmI
                );

                if (projIndex.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[projIndex].CritChance = Item.crit;
                }

                rightClickCooldown = 40;
            }
        }



        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<ConceptRightCooldown>();

            if (!modPlayer.IsConceptCooling)
                return;

            float barScale = 1.0f;
            Texture2D barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            Texture2D barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            Vector2 barOrigin = barBG.Size() * 0.5f;

            // 绘制在武器下方
            Vector2 drawPos = position + new Vector2(0, frame.Height * scale + 12f);

            float progress = 1f - modPlayer.conceptRightCooldown / (float)ConceptRightCooldown.ConceptCooldownMax;
            Rectangle crop = new Rectangle(0, 0, (int)(barFG.Width * progress), barFG.Height);

            Color barColor = Color.Cyan;

            spriteBatch.Draw(barBG, drawPos, null, barColor * 0.6f, 0f, barOrigin, scale * barScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(barFG, drawPos, crop, barColor, 0f, barOrigin, scale * barScale, SpriteEffects.None, 0f);
        }


        // ==============================
        // 形态切换时播放的“数学魔法阵”特效（可调参数版，整体放大）
        // ==============================
        private void PlaySwitchEffect(Vector2 center, int mode)
        {
            // ---------- 🔧 可调参数（整体放大约 2 倍） ----------
            float sigilScale = 0.56f;        // 符印初始缩放基准
            float sigilScaleGrowth = 0.12f;  // 符印层间递增
            float sigilEndScale = 1.12f;     // 符印终点缩放基准

            float satelliteRadius = 120f;    // 卫星冲击波环半径
            int satelliteCount = 6;          // 卫星数量

            int lissaPoints = 128;           // Lissajous 点数
            float lissaAamp = 84f;           // Lissajous X 振幅
            float lissaBamp = 52f;           // Lissajous Y 振幅

            int phylloSeeds = 110;           // 向日葵点数
            float phylloDensity = 7.2f;      // 向日葵常数

            float spiroR = 68f;              // Spirograph 大圆半径
            float spiror = 18f;              // Spirograph 小圆半径
            float spirod = 36f;              // Spirograph 笔尖距
            int spiroPoints = 280;           // Spirograph 点数

            float pulseInit = 13f;           // 同心脉冲初始大小
            float pulseGrowth = 2.4f;        // 同心脉冲层间递增
            int pulseCount = 3;              // 脉冲环层数

            int sparkCount = 36;             // 星屑数量
            float sparkRadius = 128f;        // 星屑散布半径

            // ---------- 🎨 三种形态的配色 ----------
            Color core, edge, accent;
            switch (mode)
            {
                case 0: // 赤红 + 金黄
                    core = new Color(220, 40, 40);   // 深赤红
                    edge = new Color(255, 200, 60);  // 金黄
                    accent = new Color(255, 120, 90); // 点缀：暖橙红
                    break;
                case 1: // 深蓝 + 浅黄
                    core = new Color(40, 80, 200);   // 深蓝
                    edge = new Color(255, 240, 150); // 浅黄
                    accent = new Color(100, 180, 255); // 点缀：亮蓝
                    break;
                default: // 银白 + 金色
                    core = new Color(230, 230, 240); // 银白
                    edge = new Color(255, 210, 80);  // 金色
                    accent = new Color(183, 173, 224); // 点缀：浅紫藤
                    break;
            }

            // ========= A. 中央“符印”脉冲（有序基底） =========
            // 用自定义贴图做三层相位脉冲，奠基“秩序感”
            for (int layer = 0; layer < 3; layer++)
            {
                float rot = Main.rand.NextFloat(-12f, 12f);
                float s0 = 0.28f + 0.06f * layer;
                float s1 = 0.56f + 0.07f * layer;
                Particle sigil = new CustomPulse(
                    center,
                    Vector2.Zero,
                    Color.Lerp(core, edge, 0.35f + 0.3f * layer),
                    "CalamityThrowingSpear/Texture/SunsetChange",
                    Vector2.One * s0,
                    rot,
                    0.08f,
                    s1,
                    34 + 3 * layer
                );
                GeneralParticleHandler.SpawnParticle(sigil);
            }

            // ========= B. 卫星冲击波 =========
            for (int k = 0; k < satelliteCount; k++)
            {
                float ang = MathHelper.TwoPi * k / satelliteCount;
                Vector2 p = center + new Vector2(satelliteRadius, 0f).RotatedBy(ang);
                Particle ring = new DirectionalPulseRing(
                    p,
                    Vector2.Zero,
                    edge,
                    Vector2.One,
                    Main.rand.NextFloat(11f, 17f),
                    0.18f,
                    3.2f,
                    12
                );
                GeneralParticleHandler.SpawnParticle(ring);
            }

            // ========= C. Lissajous 曲线 =========
            int a = (mode == 0) ? 3 : (mode == 1) ? 4 : 5;
            int b = (mode == 0) ? 4 : (mode == 1) ? 5 : 6;
            float delta = Main.GlobalTimeWrappedHourly * 1.8f;
            for (int i = 0; i < lissaPoints; i++)
            {
                float t = MathHelper.TwoPi * i / lissaPoints;
                Vector2 pos = center + new Vector2(
                    lissaAamp * (float)Math.Sin(a * t + delta),
                    lissaBamp * (float)Math.Sin(b * t)
                );
                GlowOrbParticle orb = new GlowOrbParticle(
                    pos,
                    Vector2.Zero,
                    false,
                    14,
                    1.3f,
                    Color.Lerp(core, accent, 0.5f),
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }

            // ========= D. Phyllotaxis 分布 =========
            float golden = MathHelper.ToRadians(137.50776405f);
            for (int n = 0; n < phylloSeeds; n++)
            {
                float theta = n * golden;
                float r = phylloDensity * (float)Math.Sqrt(n);
                Vector2 pos = center + r * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));

                Vector2 vel = new Vector2(0f, -1f).RotatedBy(theta + Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.4f, 1.4f);
                SquishyLightParticle exo = new(
                    pos,
                    vel,
                    0.44f + n * 0.005f,
                    Color.Lerp(edge, accent, 0.35f + 0.35f * (n / (float)phylloSeeds)),
                    28 + Main.rand.Next(6),
                    opacity: 1f,
                    squishStrenght: 1f,
                    maxSquish: 2.7f,
                    hueShift: 0f
                );
                GeneralParticleHandler.SpawnParticle(exo);
            }

            // ========= E. Spirograph 摆线 =========
            float Rbig = spiroR, rSmall = spiror, dPen = spirod;
            float kRatio = Rbig / rSmall;
            for (int i = 0; i < spiroPoints; i++)
            {
                float t = MathHelper.TwoPi * i / spiroPoints;
                float x = (Rbig + rSmall) * (float)Math.Cos(t) - dPen * (float)Math.Cos((kRatio + 1) * t);
                float y = (Rbig + rSmall) * (float)Math.Sin(t) - dPen * (float)Math.Sin((kRatio + 1) * t);
                Vector2 pos = center + new Vector2(x, y);

                SquareParticle sq = new SquareParticle(
                    pos,
                    (pos - center).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.3f, 1.1f),
                    false,
                    34,
                    2.4f,
                    Color.Lerp(core, edge, 0.25f)
                );
                GeneralParticleHandler.SpawnParticle(sq);

                if (i % 20 == 0)
                {
                    GlowOrbParticle orb = new GlowOrbParticle(
                        pos,
                        Vector2.Zero,
                        false,
                        16,
                        1.6f,
                        Color.Lerp(edge, accent, 0.6f),
                        true,
                        false,
                        true
                    );
                    GeneralParticleHandler.SpawnParticle(orb);
                }
            }

            // ========= F. 同心脉冲环 =========
            for (int k = 0; k < pulseCount; k++)
            {
                float init = pulseInit + k * pulseGrowth;
                Particle ring = new DirectionalPulseRing(
                    center,
                    Vector2.Zero,
                    Color.Lerp(core, edge, 0.4f + 0.2f * k),
                    Vector2.One,
                    init,
                    0.18f,
                    3.2f,
                    18 + 2 * k
                );
                GeneralParticleHandler.SpawnParticle(ring);
            }

            // ========= G. 星屑闪烁 =========
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 randPos = center + Main.rand.NextVector2Circular(sparkRadius, sparkRadius);
                GlowOrbParticle spark = new GlowOrbParticle(
                    randPos,
                    Vector2.Zero,
                    false,
                    10 + Main.rand.Next(6),
                    1.1f + Main.rand.NextFloat(0.7f),
                    Color.Lerp(accent, edge, Main.rand.NextFloat(0.2f, 0.7f)),
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }
        }



     
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 三个形态对应不同的键
            string stageKey = currentMode switch
            {
                0 => "TooltipS0",
                1 => "TooltipS1",
                2 => "TooltipS2",
                _ => "TooltipS0"
            };
            string nameKey = currentMode switch
            {
                0 => "DisplayNameS0",
                1 => "DisplayNameS1",
                2 => "DisplayNameS2",
                _ => "DisplayNameS0"
            };

            // 替换名字
            Item.SetNameOverride(this.GetLocalizedValue(nameKey));

            // 替换 Tooltip 中的占位符
            tooltips.FindAndReplace("[Stage]", this.GetLocalizedValue(stageKey));
        }





        // 所见攻击
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
                else // **A/B 形态分开处理**
                {
                    Item.channel = false;

                    if (currentMode == 0) // === A 模式：四连发 ===
                    {
                        Item.UseSound = SoundID.Item1;

                        Item.useTime = 5;              // 每发间隔 5 帧
                        Item.useAnimation = 60;        // 总动画 60 帧
                        Item.useLimitPerAnimation = 4; // 一次动画内打 4 发
                        Item.autoReuse = true;         // 允许长按持续触发

                        Item.shoot = ModContent.ProjectileType<SunsetASunsetLeft>();
                    }
                    else if (currentMode == 1) // === B 模式：双发交替 ===
                    {
                        Item.UseSound = SoundID.Item2;

                        Item.useTime = 5;              // 每发间隔
                        Item.useAnimation = 90;        // 总动画
                        Item.useLimitPerAnimation = 10; // 一次动画内打 X 发
                        Item.autoReuse = true;

                        Item.shoot = ModContent.ProjectileType<SunsetBForgetLeft>();
                    }
                    else // 兜底（如果不是 A 或 B）
                    {
                        Item.UseSound = SoundID.Item1;
                        Item.useTime = 10;
                        Item.useAnimation = 10;
                        Item.useLimitPerAnimation = 1;
                        Item.autoReuse = true;

                        Item.shoot = ModContent.ProjectileType<SunsetASunsetLeft>();
                    }
                }


            }
        }

        public override bool CanUseItem(Player player)
        {
            switch (currentMode)
            {
                case 0: // A形态：四连发
                    Item.shoot = ModContent.ProjectileType<SunsetASunsetLeft>();
                    Item.shootSpeed = 27f;
                    Item.damage = 371;
                    Item.UseSound = SoundID.Item1;
                    Item.channel = false;
                    break;

                case 1: // B形态：双连发
                    Item.shoot = ModContent.ProjectileType<SunsetBForgetLeft>();
                    Item.shootSpeed = 24f;
                    Item.damage = 410;
                    Item.UseSound = SoundID.Item2;
                    Item.channel = false;
                    break;

                case 2: // C形态：Listener 持续
                    Item.shoot = ModContent.ProjectileType<SunsetCConceptLeftListener>();
                    Item.shootSpeed = 30f;
                    Item.damage = 450;
                    Item.UseSound = null;
                    Item.channel = true;
                    break;
            }

            return base.CanUseItem(player);
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
            // === 全局拦截：如果场上已有任意右键长按弹幕（A/B/C），直接拒绝左键攻击 ===
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI)
                {
                    if (proj.type == ModContent.ProjectileType<SunsetASunsetRight>() ||
                        proj.type == ModContent.ProjectileType<SunsetBForgetRight>() ||
                        proj.type == ModContent.ProjectileType<SunsetCConceptRight>())
                    {
                        return false; // 阻止左键生成
                    }
                }
            }

            // === C模式（概念形态）===
            if (type == ModContent.ProjectileType<SunsetCConceptLeftListener>())
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.type == type && proj.owner == player.whoAmI)
                    {
                        return false; // 已有 Listener，拒绝生成
                    }
                }
            }
            // === A模式：四连发 ===
            if (currentMode == 0)
            {
                return true; // ✅ 返回 true，让默认逻辑生效
            }

            // B 模式：不在这里处理，交给 ModifyShootStats
            if (currentMode == 1)
            {
                return false; // 阻止默认逻辑
            }



            // === 其他情况（容错处理）===
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }



        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = Item.shoot;

            // 🚫 如果右键正在按住，不允许 A 模式生成三发
            if (player.altFunctionUse == 2)
            {
                type = 0;
                velocity = Vector2.Zero;
                return;
            }


            //if (currentMode == 0) // A模式：单发 + 随机水平偏移
            //{
            //    // 找到水平向量（垂直于射击方向）
            //    Vector2 offset = Vector2.Normalize(velocity.RotatedBy(MathHelper.PiOver2));

            //    // 在 [-19, 19] 的范围内随机偏移
            //    position += offset * Main.rand.NextFloat(-19f, 19f);

            //    // 稍微往后移，让子弹有生成感
            //    position -= 3f * velocity;
            //}




            if (currentMode == 0)
            {
                // 不用默认发射点
                // A 模式的三发弹幕我们自己生成，阻止默认的弹幕生成
                position = player.Center; // 占位无意义（因为我们马上手动生成）

                // 玩家头顶高度（75 px 上方）
                float heightAbove = 75 * 16f;

                // 左右拓展范围（20 tiles = 20 * 16 = 320 px）
                float horizontalRange = 10f * 16f;

                // 获取玩家头顶中心点（中间那一发）
                Vector2 baseSpawn = new Vector2(player.Center.X, player.Center.Y - heightAbove);

                // 计算朝向鼠标的标准速度（中间那一发）
                Vector2 dir = Vector2.Normalize(Main.MouseWorld - baseSpawn);
                Vector2 trueVelocity = dir * velocity.Length() * 1.15f;

                // 生成三发：左 —— 中 —— 右
                Vector2 leftSpawn = baseSpawn + new Vector2(-Main.rand.NextFloat(horizontalRange), 0);
                Vector2 midSpawn = baseSpawn;
                Vector2 rightSpawn = baseSpawn + new Vector2(+Main.rand.NextFloat(horizontalRange), 0);

                // 左发：平行，不修正方向（直接用中间方向）
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    leftSpawn,
                    trueVelocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );

                // 中发：真正砸向鼠标的那一发
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    midSpawn,
                    trueVelocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );

                // 右发：也平行
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    rightSpawn,
                    trueVelocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );

                // 禁止默认行为
                type = 0;
                velocity = Vector2.Zero;
            }



            if (currentMode == 1) // B模式：玩家身后扇形平行发射（仅一发）
            {
                float baseRadius = 30f;
                float spread = MathHelper.ToRadians(140f);

                // 在扇形范围内生成随机角度
                float angle = Main.rand.NextFloat(-spread / 2f, spread / 2f);

                Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);

                // 半径带随机扰动
                float radius = baseRadius * Main.rand.NextFloat(0.8f, 1.2f);

                // 计算玩家身后的随机点
                Vector2 behind = -dir.RotatedBy(angle) * radius;
                Vector2 spawnPos = player.Center + behind;

                // 发射方向保持正前方 + 轻微速度浮动
                Vector2 shotVel = dir * velocity.Length() * Main.rand.NextFloat(0.9f, 1.1f);

                // 只发射一个
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    spawnPos,
                    shotVel,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );
            }


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














