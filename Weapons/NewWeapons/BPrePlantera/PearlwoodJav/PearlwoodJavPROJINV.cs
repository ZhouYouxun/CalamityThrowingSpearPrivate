using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.PearlwoodJav
{
    public class PearlwoodJavPROJINV : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static int MaxUpdate = 3; // 定义一个静态变量，表示弹幕每次更新的最大次数
        private int Lifetime = 180; // 定义弹幕的生命周期为110

        // 更改颜色：深绿色、黑色、另一种深绿色
        private static Color ShaderColorOne = Color.Cyan; // 着色器颜色1，设置为深绿色
        private static Color ShaderColorTwo = Color.Lime; // 着色器颜色2，设置为黑色
        private static Color ShaderEndColor = Color.White; // 着色器结束颜色，设置为森林绿色（另一种深绿色）

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
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void AI()
        {
            Projectile.ai[0]++; // 弹幕AI计数器递增

            if (Projectile.timeLeft <= 5)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(9, 9) - Projectile.velocity * 5, DustID.GemDiamond, Projectile.velocity * 30 * Main.rand.NextFloat(0.1f, 0.95f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.9f, 1.45f);
                dust.alpha = 235;
                dust.color = Color.White;
            }

            // 刚出现时不追踪，超过60帧后开始追踪敌人
            if (Projectile.ai[0] > 60)
            {
                NPC target = Projectile.Center.ClosestNPCAt(2400); // 查找范围内最近的敌人
                if (target != null)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 32f, 0.08f); // 追踪速度为12f，调整跟随效果
                }
            }
            else
            {
                // 每一帧右拐 7 度
                //Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(7));
                Projectile.ai[1]++;
            }

            if (Projectile.timeLeft <= 80)
                Projectile.velocity *= 0.96f; // 缓慢减小弹幕速度
        }


        public override void OnKill(int timeLeft)
        {
            Vector2 baseDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // === 核心直射层（Spark，圣光碎片） ===
            for (int i = 0; i < 15; i++)
            {
                float angle = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4); // ±45°
                Vector2 dir = baseDir.RotatedBy(angle) * Main.rand.NextFloat(8f, 14f);

                Color c = Color.Lerp(Color.Pink, Color.White, Main.rand.NextFloat(0.3f, 0.8f));

                Particle spark = new SparkParticle(
                    Projectile.Center,
                    dir,
                    false,
                    Main.rand.Next(18, 25),
                    Main.rand.NextFloat(1.0f, 1.6f),
                    c
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }

            // === 环绕散射层（WaterFlavored，柔和光雾） ===
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f; // 均匀一圈
                Vector2 dir = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 9f);

                Color c = Color.Lerp(Color.LightPink, Color.White, Main.rand.NextFloat(0.2f, 0.7f));

                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    Projectile.Center,
                    dir,
                    false,
                    Main.rand.Next(18, 26),
                    0.9f + Main.rand.NextFloat(0.3f),
                    c * 0.9f
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }

            // === 点缀余晖（混合 Spark + WaterFlavored） ===
            for (int i = 0; i < 10; i++)
            {
                Vector2 dir = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 6f);

                Color sparkColor = Color.Lerp(Color.HotPink, Color.White, Main.rand.NextFloat());
                Color mistColor = Color.Lerp(Color.MistyRose, Color.LightPink, Main.rand.NextFloat());

                // 长寿命 Spark
                Particle spark = new SparkParticle(
                    Projectile.Center,
                    dir * 1.5f,
                    false,
                    45,
                    1.2f,
                    sparkColor
                );
                GeneralParticleHandler.SpawnParticle(spark);

                // 淡粉余晖雾
                WaterFlavoredParticle mist = new WaterFlavoredParticle(
                    Projectile.Center,
                    dir,
                    false,
                    30,
                    1.0f,
                    mistColor
                );
                GeneralParticleHandler.SpawnParticle(mist);
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            //target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
        }


        private float PrimitiveWidthFunction(float completionRatio)
        {
            // 保持尖头形状
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
            // PearlwoodJavPROJ 的颜色逻辑：粉色渐变
            Color start = Color.Pink;
            Color mid = Color.LightPink;
            Color end = Color.White;

            if (completionRatio < 0.5f)
                return Color.Lerp(start, mid, completionRatio * 2f);
            else
                return Color.Lerp(mid, end, (completionRatio - 0.5f) * 2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用 PearlwoodJavPROJ 的同款着色器
            GameShaders.Misc["CalamityMod:Flame"].UseColor(Color.Pink);
            GameShaders.Misc["CalamityMod:Flame"].UseSecondaryColor(Color.LightPink);

            Vector2 overallOffset = Projectile.Size * 0.5f + Projectile.velocity * 1.4f;
            int numPoints = 60; // 更平滑

            PrimitiveRenderer.RenderTrail(
                Projectile.oldPos,
                new PrimitiveSettings(PrimitiveWidthFunction, PrimitiveColorFunction, (_) => overallOffset,
                shader: GameShaders.Misc["CalamityMod:Flame"]),
                numPoints
            );
            return false;
        }




    }
}
