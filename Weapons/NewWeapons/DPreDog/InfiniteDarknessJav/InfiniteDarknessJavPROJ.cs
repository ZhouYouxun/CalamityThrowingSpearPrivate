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
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using CalamityMod.Sounds;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.InfiniteDarknessJav
{
    public class InfiniteDarknessJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/InfiniteDarknessJav/InfiniteDarknessJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        private Vector2 initialVelocity;

        private int attackPhase = 1; // 阶段控制
        private int frameCounter = 0; // 计数器
        private bool hasTarget = false; // 是否找到目标

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 原有的拖尾效果，使用 CalamityUtils 的函数绘制标准拖尾
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            //// 添加紫色光影拖尾效果，使用自定义着色器
            //GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));

            //// 设置拖尾的宽度函数，宽度更粗，大约是 CosmilampBeam 的六倍
            //float WidthFunction(float completionRatio)
            //{
            //    float maxWidth = Projectile.Opacity * Projectile.width * 10f; // 设置拖尾宽度，放大 10 倍
            //    return MathHelper.Lerp(0f, maxWidth, 1f - (float)Math.Pow(1f - completionRatio, 2)); // 宽度逐渐变化
            //}

            //// 设置拖尾的颜色函数，拖尾颜色在紫色到黑色之间渐变
            //Color ColorFunction(float completionRatio)
            //{
            //    float opacity = Utils.GetLerpValue(0.8f, 0.54f, completionRatio, true) * Projectile.Opacity;
            //    Color startColor = Color.Lerp(Color.Purple, Color.DarkViolet, completionRatio) * opacity; // 使用紫色到深紫色渐变
            //    return Color.Lerp(startColor, Color.Black, 0.3f); // 最终混合一些黑色
            //}

            //// 使用拖尾渲染器绘制更长的拖尾，使用弹幕的 oldPos 来绘制路径
            //var trailSettings = new PrimitiveSettings(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]);
            //PrimitiveRenderer.RenderTrail(Projectile.oldPos, trailSettings, 80); // 设置拖尾的长度为原来的十倍

            return false; // 预防重复绘制
        }

        public override void SetDefaults()
        {
            initialVelocity = Projectile.velocity; // Record initial velocity
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
            Projectile.alpha = 0;
        }

        private int phase = 1;
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            if (phase == 1)
            {
                // 第1阶段：每帧增加12点透明度
                Projectile.alpha += 30;
                if (Projectile.alpha >= 300)
                {
                    Projectile.alpha = 300;
                    frameCounter++; // 增加计数器

                    // 切换到第二阶段
                    phase = 2;
                    frameCounter = 0;
                }
            }
            else if (phase == 2)
            {
                // 搜索最近的敌人
                NPC target = FindClosestNPC(1500f);
                if (target != null)
                {
                    // 传送到目标附近
                    Vector2 teleportPosition = target.Center + Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 16 * 16;
                    Projectile.position = teleportPosition - new Vector2(Projectile.width / 2, Projectile.height / 2);
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();

                    // 在目标的另一个点生成 InfiniteDarknessJavPROJStarBomb
                    Vector2 bombPosition = target.Center + Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 16 * 16;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), bombPosition, Vector2.Zero, ModContent.ProjectileType<InfiniteDarknessJavPROJStarBomb>(), (int)(Projectile.damage * 0.27f), 0, Projectile.owner);

                    // 设定透明度为完全透明，并进入下一阶段
                    Projectile.alpha = 255; // 传送后设为完全透明
                    Projectile.velocity = Vector2.Normalize(target.Center - Projectile.Center) * 10f; // 向目标冲刺
                    phase = 3;
                }

                {
                    // 生成轻型烟雾粒子效果，左右偏离10度
                    int smokeCount = 25;
                    for (int i = 0; i < smokeCount; i++)
                    {
                        // 左右偏离 10 度
                        float angleOffset = MathHelper.ToRadians(10) * (i % 2 == 0 ? 1 : -1);
                        Vector2 dustVelocity = Projectile.velocity.RotatedBy(angleOffset);

                        Particle smoke = new HeavySmokeParticle(
                            Projectile.Center,
                            dustVelocity * Main.rand.NextFloat(1f, 2.6f),
                            Color.Black, // 使用黑色
                            18,
                            Main.rand.NextFloat(0.9f, 1.6f),
                            0.35f,
                            Main.rand.NextFloat(-1, 1),
                            true
                        );
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                }
            }
            else if (phase == 3)
            {
                // 第三阶段：逐渐降低透明度，使弹幕变得可见
                Projectile.alpha -= 50;
                Projectile.velocity = Projectile.velocity * 1.1f;
                Projectile.penetrate = 2;
                if (Projectile.alpha < 0)
                {
                    Projectile.alpha = 0; // 保持完全可见状态
                }
            }





        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int slashDamage = Projectile.damage;
            int slashCount = 3; // 每次击中敌人都会释放两道斩杀

            if (phase == 1) // 第一阶段：传送前，造成 200% 伤害
            {
                slashDamage = (int)(Projectile.damage * 3.25f);
            }
            else if (phase == 3) // 第三阶段：传送后，造成 100% 伤害
            {
                slashDamage = (int)(Projectile.damage * 0.75f);
            }


            // 生成斩杀弹幕
            for (int i = 0; i < slashCount; i++)
            {
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, randomDirection, ModContent.ProjectileType<BlackSLASH>(), slashDamage, Projectile.knockBack, Projectile.owner);
            }

            // 播放斩杀音效
            SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = 0.5f }, Projectile.Center);
        }



        public override void OnKill(int timeLeft)
        {
            // 释放两道斩杀BlackSLASH
            for (int i = 0; i < 2; i++)
            {
                Vector2 slashPosition = Projectile.Center + Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 16;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashPosition, Vector2.Zero, ModContent.ProjectileType<BlackSLASH>(), (int)(Projectile.damage * 0.27f), 0, Projectile.owner);
            }

            if (hasTarget && attackPhase == 3)
            {
                int dustAmount = 400; // 增加粒子数量
                float angleOffset = MathHelper.ToRadians(15);

                for (int i = 0; i < dustAmount; i++)
                {
                    // 使用两个椭圆的分布，一个向上，一个向下
                    float ellipseRadiusX = Main.rand.NextFloat(6f, 18f);
                    float ellipseRadiusY = Main.rand.NextFloat(3f, 9f);

                    // 随机选择椭圆方向
                    Vector2 baseDirection = i % 2 == 0 ? new Vector2(ellipseRadiusX, ellipseRadiusY) : new Vector2(ellipseRadiusX, -ellipseRadiusY);

                    // 设置方向并添加偏移
                    Vector2 direction = baseDirection.RotatedBy(i % 2 == 0 ? angleOffset : -angleOffset) * Main.rand.NextFloat(0.8f, 1.2f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, direction * Main.rand.NextFloat(1f, 2f), 150, Color.Black, 1.2f); // 设为小粒子
                    dust.noGravity = true;
                }
            }
            else
            {
                // 未找到敌人而自毁时生成均匀散布的黑色 Dust 粒子
                int dustAmount = 150;
                for (int i = 0; i < dustAmount; i++)
                {
                    Vector2 direction = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 4f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, direction, 150, Color.Black, 1.5f);
                    dust.noGravity = true;
                }
            }
        }

        // 寻找最近的敌人
        private NPC FindClosestNPC(float maxDetectDistance)
        {
            NPC closestNPC = null;
            float minDistance = maxDetectDistance;
            foreach (NPC npc in Main.npc)
            {
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (npc.CanBeChasedBy(this) && distance < minDistance)
                {
                    minDistance = distance;
                    closestNPC = npc;
                }
            }
            return closestNPC;
        }




    }
}

