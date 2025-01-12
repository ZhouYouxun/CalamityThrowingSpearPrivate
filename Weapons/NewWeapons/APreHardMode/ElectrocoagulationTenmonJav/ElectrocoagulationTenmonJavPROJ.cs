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
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.NewWeapons.APreHardMode.ElectrocoagulationTenmonJav
{
    public class ElectrocoagulationTenmonJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/APreHardMode/ElectrocoagulationTenmonJav/ElectrocoagulationTenmonJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.APreHardMode";
        public static int MaxUpdate = 1; // 弹幕每次更新的最大次数
        private int Lifetime = 110; // 弹幕的生命周期为110

        private static Color ShaderColorOne = Color.Pink; // 着色器颜色1，设置为粉红色
        private static Color ShaderColorTwo = Color.White; // 着色器颜色2，设置为白色
        private static Color ShaderEndColor = Color.LightPink; // 着色器结束颜色，设置为浅粉红色

        private Vector2 altSpawn; // 定义备用生成位置向量
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        //public override bool PreDraw(ref Color lightColor)
        //{
        //    CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
        //    return false;
        //}

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
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加亮白色光源
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.8f);

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 在飞行期间留下白色拖尾特效，使用 UelibloomArrowLight 的特效，并将长度设为两倍
            Vector2 offset = Projectile.velocity * 2f; // 两倍长度
            Dust dust = Dust.NewDustPerfect(Projectile.Center - offset, DustID.GemDiamond, null, 0, Color.White, 1.5f);
            dust.noGravity = true;
            dust.scale = 1.5f;
            dust.alpha = 180; // 透明度


            // 每帧增加 ai[0] 计数
            Projectile.ai[0]++;

            // 前x帧禁用方块碰撞（防止在平地上某些角度射不出来）
            if (Projectile.ai[0] < 5)
            {
                Projectile.tileCollide = false; // 禁用碰撞
            }
            else
            {
                Projectile.tileCollide = true; // 启用碰撞
            }
        }




        private float PrimitiveWidthFunction(float completionRatio)
        {
            float arrowheadCutoff = 0.36f;
            float width = 24f;
            float minHeadWidth = 0.03f;
            float maxHeadWidth = width;
            if (completionRatio <= arrowheadCutoff)
                width = MathHelper.Lerp(minHeadWidth, maxHeadWidth, Utils.GetLerpValue(0f, arrowheadCutoff, completionRatio, true));
            return width;      
        }


        private Color PrimitiveColorFunction(float completionRatio)
        {
            
            float endFadeRatio = 0.41f;
            float completionRatioFactor = 2.7f;
            float globalTimeFactor = 5.3f;
            float endFadeFactor = 3.2f;
            float endFadeTerm = Utils.GetLerpValue(0f, endFadeRatio * 0.5f, completionRatio, true) * endFadeFactor;
            float cosArgument = completionRatio * completionRatioFactor - Main.GlobalTimeWrappedHourly * globalTimeFactor + endFadeTerm;
            float startingInterpolant = (float)Math.Cos(cosArgument) * 0.5f + 0.5f;

            float colorLerpFactor = 0.6f;
            Color startingColor = Color.Lerp(ShaderColorOne, ShaderColorTwo, startingInterpolant * colorLerpFactor);
            return Color.Lerp(startingColor, ShaderEndColor, MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0f, endFadeRatio, completionRatio, true)));

        }


        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);

            // 渲染带有粉红色渐变效果的光学尾迹
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak"));
            Vector2 overallOffset = Projectile.Size * 0.5f;
            overallOffset += Projectile.velocity * 1.4f;
            int numPoints = 46;
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), numPoints);
            return false;
 
        }

        public override void OnKill(int timeLeft)
        {

            // 粉红色粒子 - 向上和向下发射
            int numParticles = 7;
            for (int i = 0; i < numParticles; i++)
            {
                float speedFactor = 3f + i * 0.2f; // 控制速度，使粒子平摊成链状

                // 向上发射
                Vector2 velocityUp = new Vector2(0, -speedFactor);
                Dust pinkDustUp = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, velocityUp, 0, Color.Pink, 1.5f);
                pinkDustUp.noGravity = true;

                // 向下发射
                Vector2 velocityDown = new Vector2(0, speedFactor);
                Dust pinkDustDown = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, velocityDown, 0, Color.Pink, 1.5f);
                pinkDustDown.noGravity = true;
            }

            // 白色粒子 - 向左和向右发射
            for (int i = 0; i < numParticles; i++)
            {
                float speedFactor = 1f + i * 0.2f; // 控制速度，使粒子平摊成链状

                // 向左发射
                Vector2 velocityLeft = new Vector2(-speedFactor, 0);
                Dust whiteDustLeft = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, velocityLeft, 0, Color.White, 1.5f);
                whiteDustLeft.noGravity = true;

                // 向右发射
                Vector2 velocityRight = new Vector2(speedFactor, 0);
                Dust whiteDustRight = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, velocityRight, 0, Color.White, 1.5f);
                whiteDustRight.noGravity = true;
            }

            

        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slimed, 300); // 原版的史莱姆效果

            // 计算发射方向：正上方、正下方、正左方和正右方
            Vector2 upDirection = new Vector2(0, -1);
            Vector2 downDirection = new Vector2(0, 1);
            Vector2 leftDirection = new Vector2(-1, 0);
            Vector2 rightDirection = new Vector2(1, 0);

            // 发射四个方向的 ElectrocoagulationTenmonJavLight 弹幕，伤害倍率为 0.33 倍
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, upDirection * 10f, ModContent.ProjectileType<ElectrocoagulationTenmonJavLight>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, downDirection * 10f, ModContent.ProjectileType<ElectrocoagulationTenmonJavLight>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, leftDirection * 10f, ModContent.ProjectileType<ElectrocoagulationTenmonJavLight>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, rightDirection * 10f, ModContent.ProjectileType<ElectrocoagulationTenmonJavLight>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
        }



    }
}
