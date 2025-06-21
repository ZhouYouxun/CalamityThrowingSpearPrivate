using System;
using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Terraria.Graphics.Shaders;


namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TyphonsGreedC
{
    public class TyphonsGreedJavBubble2 : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/TyphonsGreedC/TyphonsGreedJavBubble";

        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";


        private bool turningLeft = true; // 当前是否向左拐
        private int actionCounter = 0;   // 计时器，用于控制拐弯和执行时间

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = false; // 不允许与方块碰撞
            Projectile.ignoreWater = true; // 无视水
            Projectile.aiStyle = 0; // 自定义AI
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {

            Projectile.rotation = Projectile.velocity.ToRotation();

            // 动画处理
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
            {
                Projectile.frame = 0;
            }

            actionCounter++;

            Projectile.velocity = Projectile.velocity.RotatedBy(-MathHelper.ToRadians(2f));
            Projectile.velocity *= 1.01f;

            //// 扇形波形运动逻辑
            //if (turningLeft)
            //{
            //    // 向左拐
            //    Projectile.velocity = Projectile.velocity.RotatedBy(-MathHelper.ToRadians(1f));
            //}
            //else
            //{
            //    // 向右拐
            //    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(1f));
            //}

            // 每2帧生成黑色浓烟粒子
            if (Projectile.ai[0] % 1 == 0)
            {
                Color smokeColor = Color.Black; // 黑色浓烟
                float smokeScale = 0.75f; // 缩放调整

                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 2f),
                    Projectile.velocity * 0.1f,
                    smokeColor,
                    30, // 持续时间
                    smokeScale * Main.rand.NextFloat(0.7f, 1.3f), // 随机缩放
                    0.8f,
                    MathHelper.ToRadians(3f),
                    true
                );

                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 每2帧生成 Dust 特效
            if (Projectile.ai[0] % 2 == 0)
            {
                int[] dustTypes = { 108, 31, 14 }; // Dust 类型
                int selectedDustType = dustTypes[Main.rand.Next(dustTypes.Length)]; // 随机 Dust 类型

                Vector2 dustOffset = Projectile.velocity.RotatedBy(MathHelper.ToRadians(15)) * -1; // 单螺旋偏移
                Vector2 dustVelocity = dustOffset * Main.rand.NextFloat(0.5f, 1.5f); // 随机速度

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + dustOffset,
                    selectedDustType,
                    dustVelocity,
                    100,
                    new Color(0, 255, 255), // 颜色
                    Main.rand.NextFloat(1f, 1.5f) // 随机缩放
                );
                dust.noGravity = true; // 无重力
            }

            // 切换方向
            if (actionCounter >= 30) // 每30帧切换一次方向
            {
                turningLeft = !turningLeft;
                actionCounter = 0;
            }

            // 更新旋转以匹配移动方向
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 增加光效
            Lighting.AddLight(Projectile.Center, 0f, 0.2f, 0.6f); // 深海蓝色光效
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 检查当前速度方向，并改变方向（反弹逻辑）
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            // 在碰撞时触发特定逻辑，例如释放额外粒子或改变状态
            CreateCollisionEffects();

            // 返回 true 表示弹幕会继续存在，返回 false 表示弹幕会被移除
            return true; // 这里根据你的需求调整返回值
        }

        private void CreateCollisionEffects()
        {
            // 添加碰撞特效逻辑，例如生成粒子效果
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water,
                             Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, Color.Blue, 1.2f);
            }
        }

        //public override void PostDraw(Color lightColor)
        //{
        //    // 绘制拖尾特效
        //    PrimitiveRenderer.RenderTrail(
        //        Projectile.oldPos,
        //        new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f),
        //        30);

        //    // 绘制发光效果
        //    Texture2D glowTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        //    Main.EntitySpriteDraw(
        //        glowTexture,
        //        Projectile.Center - Main.screenPosition,
        //        null,
        //        Color.White,
        //        Projectile.rotation,
        //        glowTexture.Size() * 0.5f,
        //        Projectile.scale,
        //        SpriteEffects.None,
        //        0);
        //}

        //// 拖尾宽度函数
        //internal float WidthFunction(float completionRatio)
        //{
        //    return (1f - completionRatio) * Projectile.scale * 9f;
        //}

        //// 拖尾颜色函数
        //internal Color ColorFunction(float completionRatio)
        //{
        //    float hue = 0.55f + 0.2f * completionRatio * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
        //    Color trailColor = Color.Lerp(Color.DarkBlue, Color.Cyan, hue); // 深海配色渐变
        //    return trailColor * Projectile.Opacity;
        //}





        // Drawing effects remain unchanged
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int framing = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y6 = framing * Projectile.frame;
            Main.spriteBatch.Draw(texture2D13,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture2D13.Width, framing)),
                Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2((float)texture2D13.Width / 2f, (float)framing / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }

        // On death, maintain original effects
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item21, Projectile.position);
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 64;
            Projectile.position.X -= (float)(Projectile.width / 2);
            Projectile.position.Y -= (float)(Projectile.height / 2);

            for (int i = 0; i < 2; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, new Color(0, 255, 255), 1.5f);
            }

            for (int j = 0; j < 6; j++)
            {
                int bubblyDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedsWingsRun, 0f, 0f, 0, new Color(0, 255, 255), 2.5f);
                Main.dust[bubblyDust].noGravity = true;
                Main.dust[bubblyDust].velocity *= 3f;
                bubblyDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedsWingsRun, 0f, 0f, 100, new Color(0, 255, 255), 1.5f);
                Main.dust[bubblyDust].velocity *= 2f;
                Main.dust[bubblyDust].noGravity = true;
            }

            // 在死亡时向正后方发射一个 TyphonsGreedJavBubble
            Vector2 backDirection = -Projectile.velocity.SafeNormalize(Vector2.Zero); // 计算正后方的方向
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                backDirection * 5f, // 调整速度大小
                ModContent.ProjectileType<TyphonsGreedJavBubble>(),
                Projectile.damage, // 伤害倍率为 1.0
                Projectile.knockBack,
                Projectile.owner
            );
        }










    }
}
