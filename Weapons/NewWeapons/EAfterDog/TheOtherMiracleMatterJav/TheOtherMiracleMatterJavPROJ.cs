using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using CalamityMod.Sounds;
using Terraria.Audio;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TheOtherMiracleMatterJav
{
    public class TheOtherMiracleMatterJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TheOtherMiracleMatterJav/TheOtherMiracleMatterJav";

        private Vector2 initialVelocity;
        private int stage = 0;
        private int stageTimer = 0;
        private int bounces = 0;
        public int Time = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16; // 将缓存长度增加到16，拖尾会更长
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取纹理资源和位置
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Texture2D textureGlow = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/TheOtherMiracleMatterJav/TheOtherMiracleMatterJav").Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // 计算颜色渐变，用于动态的尾迹效果
            float localIdentityOffset = Projectile.identity * 0.1372f;
            Color mainColor = CalamityUtils.MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f + localIdentityOffset) % 1f, Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
            Color secondaryColor = CalamityUtils.MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f + localIdentityOffset + 0.2f) % 1f, Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);

            // 混合颜色，进一步控制白色和渐变色的比例
            mainColor = Color.Lerp(Color.White, mainColor, 0.85f);
            secondaryColor = Color.Lerp(Color.White, secondaryColor, 0.85f);

            // 背光效果部分，增加充能效果的光晕
            float chargeOffset = 3f; // 控制充能效果扩散的偏移量
            Color chargeColor = Color.Lerp(Color.Lime, Color.Cyan, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.1f) * 0.5f + 0.5f) * 0.6f;
            chargeColor.A = 0; // 透明度

            // 修复旋转逻辑，确保与速度方向同步
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            SpriteEffects direction = SpriteEffects.None;

            // 绘制充能效果 - 圆周上绘制多个充能光效
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            }

            // 使用自定义着色器应用尾迹效果
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/EternityStreak"));
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].UseColor(mainColor);
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].UseSecondaryColor(secondaryColor);
            GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"].Apply();

            // 渲染尾迹，通过存储的弹幕位置数据渲染弹幕移动的尾巴
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:HeavenlyGaleTrail"]), 53);

            // 渲染实际的投射物本体
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

            // 渲染发光的投射物效果
            Main.spriteBatch.Draw(textureGlow, drawPosition, null, Color.White, rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }


        public float PrimitiveWidthFunction(float completionRatio) => Projectile.scale * 30f;

        public Color PrimitiveColorFunction(float _) => Color.Lime * Projectile.Opacity;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 600;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 不与方块碰撞
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            // 弹幕旋转逻辑
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 第一阶段：减速阶段
            if (stage == 0)
            {
                Projectile.velocity *= 0.99f; // 每帧减速
                stageTimer++;

                // 进入加速阶段
                if (stageTimer >= 60) // 假设持续60帧
                {
                    //stage = 1;
                    //Projectile.velocity *= 80f; // 瞬间加速
                    stage = 2; // 直接进入旋转斩切状态
                    Projectile.penetrate = -1; // 穿透次数变为无限
                }
            }

            //// 第二阶段：加速冲刺阶段 (已废弃)
            //else if (stage == 1)
            //{
            //    // 检测是否碰到屏幕边缘
            //    if (CheckScreenBounds())
            //    {
            //        stage = 2; // 进入旋转斩切状态
            //        Projectile.penetrate = -1; // 穿透次数变为无限
            //    }
            //}

            // 直接进入第三阶段：旋转斩切状态
            else if (stage == 2)
            {
                // 确保在进入旋转斩切状态时，穿透次数变为无限
                if (Projectile.penetrate != -1)
                {
                    Projectile.penetrate = -1; // 允许无限次穿透敌人
                    Projectile.velocity *= 1.5f;
                }

                // 粒子效果随机化释放
                if (Time % 3 == 0)
                {
                    Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                    particleOffset.X += Main.rand.NextFloat(-3f, 3f); // 随机左右偏移
                    Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;
                    Particle Smear = new CircularSmearVFX(particlePosition, Color.GhostWhite * Main.rand.NextFloat(0.78f, 0.85f), Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(1.2f, 1.3f));
                    GeneralParticleHandler.SpawnParticle(Smear);
                }

                Time++;

                // 调整方向追踪玩家
                Vector2 playerDirection = (Main.player[Projectile.owner].Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 30f; // 速度乘以3
                Projectile.velocity = playerDirection;

                // 如果弹幕与玩家重叠，销毁弹幕
                if (Projectile.Hitbox.Intersects(Main.player[Projectile.owner].Hitbox))
                {
                    Projectile.Kill(); // 销毁弹幕
                }
            }
        }

        // 使用自定义的边界检测方法
        //private bool CheckScreenBounds()
        //{
        //    Vector2 screenBoundsMin = Main.screenPosition;
        //    Vector2 screenBoundsMax = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight);

        //    if (Projectile.position.X <= screenBoundsMin.X || Projectile.position.X + Projectile.width >= screenBoundsMax.X ||
        //        Projectile.position.Y <= screenBoundsMin.Y || Projectile.position.Y + Projectile.height >= screenBoundsMax.Y)
        //    {
        //        return true; // 碰到屏幕边缘
        //    }
        //    return false;
        //}


        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TheOtherMiracleMatterJavEXP>(), (int)(Projectile.damage * 1f), Projectile.knockBack, Projectile.owner);

            int numberOfParticles = 30;  // 设定生成粒子的数量
            for (int i = 0; i < numberOfParticles; i++)
            {
                // 计算360度内的随机角度
                Vector2 direction = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);

                // 生成随机粒子，使用类似ExoFlareCluster的粒子特效
                Vector2 spawnPosition = Projectile.Center;
                float speed = Main.rand.NextFloat(3.5f, 6.5f);  // 随机速度
                Vector2 velocity = direction * speed;

                // 生成粒子（可以使用SquishyLightParticle等光效）
                SquishyLightParticle exoEnergy = new SquishyLightParticle(spawnPosition, velocity, 0.5f, Color.Orange, 40);
                GeneralParticleHandler.SpawnParticle(exoEnergy);
            }

            // 屏幕震动效果
            float shakePower = 3f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);



            // 消亡时释放明黄色爆炸特效
            Particle blastRing = new CustomPulse(
                Projectile.Center, Vector2.Zero, Color.White,
                "CalamityMod/Particles/FlameExplosion",
                Vector2.One * 0.5f, Main.rand.NextFloat(-10f, 10f),
                0.07f, 0.33f, 30
            );
            GeneralParticleHandler.SpawnParticle(blastRing);

            //// 生成闪电效果
            //int numberOfLightningStrikes = 3; // 设定召唤闪电的次数
            //for (int i = 0; i < numberOfLightningStrikes; i++)
            //{
            //    Vector2 lightningSpawnPosition = Projectile.Center - Vector2.UnitY.RotatedByRandom(0.24f) * Main.rand.NextFloat(960f, 1020f); // 生成位置在上方随机偏移
            //    Vector2 lightningShootVelocity = (Projectile.Center - lightningSpawnPosition).SafeNormalize(Vector2.Zero) * 30f; // 瞄准中心位置的速度向量
            //    Projectile.NewProjectile(
            //        Projectile.GetSource_FromThis(),
            //        lightningSpawnPosition,
            //        lightningShootVelocity,
            //        ModContent.ProjectileType<TheOtherMiracleMatterJavExoLightningBolt>(), // 闪电
            //        Projectile.damage,
            //        Projectile.knockBack,
            //        Projectile.owner
            //    );
            //}



            // 如果需要，可以在这里添加更多的特效或音效
            //SoundEngine.PlaySound(Photoviscerator.HitSound, Projectile.Center);
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
            // 播放斩击音效
            //SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.5f }, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ExobladeBigHit"));

            // 在敌人头顶随机位置生成5道ExoFire激光
            //for (int j = 0; j < 5; j++)
            //{
            // 生成的激光中心在敌人头顶50格处，半径为10格范围内随机生成
            //Vector2 spawnPosition = target.Center + new Vector2(Main.rand.NextFloat(-10f, 10f) * 16, -50 * 16); // -50 表示敌人头顶50个方块

            // 激光的飞行方向是直接朝向敌人
            //Vector2 direction = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY);

            // 调整速度，使其与PhotovisceratorHoldout中的ExoFire一致，降低速度为0.8倍
            //direction *= 8f; // 调整后的速度，原速度的80%

            // 生成ExoFire弹幕
            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, direction, ModContent.ProjectileType<TheOtherMiracleMatterJavExoFire>(), (int)(Projectile.damage * 0.15), Projectile.knockBack, Projectile.owner);
            //}



            // 新增闪电召唤逻辑
            int lightningDamage = (int)(Projectile.damage * 0.2f); // 可调整的伤害倍率
            for (int i = 0; i < 3; i++) // 设置生成闪电次数
            {
                Vector2 lightningSpawnPosition = target.Center - Vector2.UnitY.RotatedByRandom(0.36f) * Main.rand.NextFloat(960f, 1020f);
                Vector2 lightningShootVelocity = (target.Center - lightningSpawnPosition).SafeNormalize(Vector2.UnitY) * 14f;
                int lightning = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    lightningSpawnPosition,
                    lightningShootVelocity,
                    ModContent.ProjectileType<TheOtherMiracleMatterJavExoLightningBolt>(), // 替换为你的闪电弹幕类型
                    lightningDamage,
                    0f,
                    Projectile.owner
                );
                if (Main.projectile.IndexInRange(lightning))
                {
                    Main.projectile[lightning].ai[0] = lightningShootVelocity.ToRotation();
                    Main.projectile[lightning].ai[1] = Main.rand.Next(100);
                }
            }
        }



    }
}