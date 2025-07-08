using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityThrowingSpear.Global;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.DiseasedPikeC
{
    public class DiseasedJavLight : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        //public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static int MaxUpdate = 4; // 定义一个静态变量，表示弹幕每次更新的最大次数
        private int Lifetime = 550; // 定义弹幕的生命周期

        // 更改颜色：深绿色、黑色、另一种深绿色
        private static Color ShaderColorOne = Color.DarkGreen; // 着色器颜色1，设置为深绿色
        private static Color ShaderColorTwo = Color.Black; // 着色器颜色2，设置为黑色
        private static Color ShaderEndColor = Color.ForestGreen; // 着色器结束颜色，设置为森林绿色（另一种深绿色）
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
            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(
                ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak")
            );

            Vector2 offset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f;

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new(
                    PrimitiveWidthFunction,
                    PrimitiveColorFunction,
                    (_) => offset,
                    shader: GameShaders.Misc["CalamityMod:TrailStreak"]
                ),
                46
            );

            return false; // 不绘制默认贴图
        }


        private Vector2 altSpawn; // 定义一个备用生成位置向量

        public override void SetStaticDefaults() // 设置弹幕的静态默认值
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // 设置拖尾模式为2
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21; // 设置拖尾缓存长度为21
        }

        public override void SetDefaults() // 设置弹幕的默认值
        {
            Projectile.width = Projectile.height = 24;
            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.MaxUpdates = MaxUpdate;
            Projectile.penetrate = 2;
            Projectile.ArmorPenetration = 15;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        // 刚出现的前15帧不追踪敌人
        public override void AI()
        {
            // 调整弹幕的旋转，使其在飞行时保持水平
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;

            Projectile.ai[0]++; // 弹幕AI计数器递增

            if (Projectile.timeLeft <= 5)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(9, 9) - Projectile.velocity * 5, DustID.GemDiamond, Projectile.velocity * 30 * Main.rand.NextFloat(0.1f, 0.95f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.9f, 1.45f);
                dust.alpha = 235;
                dust.color = Color.DarkGreen;
            }

            if (Projectile.ai[0] > 150)
            {
                // 超过150帧后开始追踪敌人
                NPC target = Projectile.Center.ClosestNPCAt(1800); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 12f, 0.08f); // 调整速度
                }
            }
            else
            {
                // 未到150帧时，每隔20~25帧随机左右拐90度
                if (Projectile.ai[0] % Main.rand.Next(20, 26) == 0)
                {
                    float angle = MathHelper.ToRadians(Main.rand.Next(0, 2) == 0 ? 90 : -90); // 随机左拐或右拐90度
                    Projectile.velocity = Projectile.velocity.RotatedBy(angle);
                }
            }


            if (Projectile.timeLeft <= 80)
                Projectile.velocity *= 0.96f; // 缓慢减小弹幕速度
        }

   
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Venom, 180); // 给敌人施加毒液效果
            target.AddBuff(ModContent.BuffType<Plague>(), 180); // 瘟疫
            target.AddBuff(BuffID.Poisoned, 180);
        }




    }
}
