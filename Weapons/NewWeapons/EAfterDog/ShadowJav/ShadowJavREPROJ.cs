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
using CalamityMod.Projectiles.Melee;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ElementalArkJav;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.ShadowJav
{
    public class ShadowJavREPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/ShadowJav/ShadowJav";
        public int Time = 0;

        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        private bool hasSplit = false; // 是否已分裂
        private static readonly string[] SplitProjectiles = new string[]
        {
            "AmidiasTridentJavPROJ", "GoldplumeJavPROJ", "SausageMakerJavPROJ", "YateveoBloomJavPROJ",
            "BrimlanceJavPROJ", "EarthenJavPROJ", "StarnightLanceJavPROJ",
            "AstralPikeJavPROJ", "BotanicPiercerJavPROJ", "DiseasedJavPROJ", "GalvanizingGlaiveJavPROJ", "HellionFlowerJavPROJ",
            "TenebreusTidesJavPROJ", "TyphonsGreedJavPROJ", "VulcaniteLanceJavPROJ",
            "BansheeHookJavPROJ", "GildedProboscisJavPROJ",
            "ElementalLanceJavPROJNebula", "ElementalLanceJavPROJSolar", "ElementalLanceJavPROJStardust", "ElementalLanceJavPROJVortex", "ElementalLanceJavPROJEntropy",
            "DragonRageJavPROJ", "NadirJavPROJ", "ScourgeoftheCosmosJavPROJ", "StreamGougeJavPROJ", "ViolenceJavPROJ",

            "GraniteJavPROJ", "WulfrimJavPROJ", "RedtideJavPROJ", "BraisedPorkJavPROJ", "ElectrocoagulationTenmonJavPROJ",
            "ElectrocutionHalberdPROJ", "HeartSwordPROJ", "PearlwoodJavPROJ",
            "ChaosEssenceJavPROJ", "SunEssenceJavPROJ", "PolarEssenceJavPROJ",
            "SHPCKPROJ", "SHPCKFast", "FestiveHalberdPROJ",
            "TerraLancePROJ", "BloodstoneJavPROJ",
            "EndlessDevourJavPROJ", "ChaosWindJavPROJ", "InfiniteDarknessJavPROJ", "SoulHunterJavPROJ",
            "AuricJavPROJ", "MiracleMatterJavPROJ", "TheOtherMiracleMatterJavPROJ",
            "SoulSeekerJavPROJ"
        };


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60000;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }

        public override void AI()
        {
            // 如果粘附在目标上，则保持投射物跟随目标
            if (Projectile.ai[0] == 1f)
            {
                int targetIndex = (int)Projectile.ai[1];
                if (targetIndex >= 0 && targetIndex < 200)
                {
                    NPC target = Main.npc[targetIndex];
                    if (target.active)
                    {
                        // 跟随目标移动
                        Projectile.Center = target.Center;
                        // 保持原有的旋转角度
                        Projectile.rotation += 0.33f; // 粘在敌人身上时会开始不断的旋转
                        if (Main.zenithWorld)
                        {
                            Projectile.timeLeft = 600; // 一旦咬住就不会松口，除非手动取消
                        }

                        // 粒子效果随机化释放
                        if (Time % 3 == 0)
                        {
                            Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                            particleOffset.X += Main.rand.NextFloat(-3f, 3f); // 随机左右偏移
                            Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;
                            Particle Smear = new CircularSmearVFX(particlePosition, Color.Black * Main.rand.NextFloat(0.78f, 0.85f), Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(1.2f, 1.3f));
                            GeneralParticleHandler.SpawnParticle(Smear);
                        }
                        Time++;

                        // 当弹幕黏附在敌人身上时，生成额外弹幕
                        if (Projectile.timeLeft % 5 == 0) // 每隔5帧生成一个额外弹幕
                        {
                            string selectedProjectile = SplitProjectiles[Main.rand.Next(SplitProjectiles.Length)];

                            // 随机从更远的左右两侧生成弹幕，并且在上下范围内有较大的偏移
                            float offsetX = Main.rand.NextBool() ? -900f : 900f; // 左侧或右侧偏移（可手动更改数值调整左右偏移的距离）
                            float offsetY = Main.rand.NextFloat(-400f, 400f); // 上下偏移范围（可手动更改数值调整上下偏移的范围）

                            // 计算生成位置
                            Vector2 spawnPosition = new Vector2(Projectile.Center.X + offsetX, Projectile.Center.Y + offsetY);

                            // 计算速度向量，使弹幕从生成位置朝着弹幕中心射出
                            Vector2 velocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.UnitX) * 15f;

                            // 生成额外的弹幕
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, Mod.Find<ModProjectile>(selectedProjectile).Type, (int)(Projectile.damage * 20f), 0f, Projectile.owner);
                        }

                        // 亮黄色冲击波效果
                        if (Projectile.timeLeft % 20 == 0)
                        {
                            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Black, new Vector2(1.5f), Projectile.rotation, 1f, 0.1f, 30);
                            GeneralParticleHandler.SpawnParticle(pulse);
                        }
                    }
                    else
                    {
                        // 如果目标不再活跃，则销毁投射物
                        Projectile.Kill();
                    }
                }
            }
            else
            {
                // 前30帧不追踪，之后开始追踪敌人
                if (Projectile.ai[1] > 30)
                {
                    NPC target = Projectile.Center.ClosestNPCAt(88888); // 查找范围内最近的敌人
                    if (target != null)
                    {
                        Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 50f, 0.08f); // 追踪速度为xf
                    }
                }
                else
                {
                    Projectile.ai[1]++;
                }

                // 保持弹幕的原始旋转角度，直到第一次击中目标
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                //Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.ai[0];

                // 修改粘附时旋转摆正的问题
                if (Projectile.ai[0] == 0f)
                {
                    //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                    Projectile.ai[0] = Projectile.velocity.ToRotation() + MathHelper.PiOver4; // 记录初始旋转角度
                }

                // 添加黑色光源
                Lighting.AddLight(Projectile.Center, Color.Black.ToVector3() * 0.55f);

                // 弹幕逐渐加速
                Projectile.velocity *= 1.005f;

                // 添加黑色能量光效
                LineParticle energy = new LineParticle(Projectile.Center + Projectile.velocity * 4, Projectile.velocity * 4.95f, false, 9, 2.4f, Color.Black);
                GeneralParticleHandler.SpawnParticle(energy);
            }
        }


        // 击中敌人后黏附逻辑
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.ModifyHitNPCSticky(30); // 弹幕击中目标后粘附，并保持6帧的无敌状态
            hasSplit = true; // 表示弹幕已经击中了敌人，进入附着状态
            Projectile.tileCollide = false; // 碰撞时不再消失
            Projectile.velocity = Vector2.Zero; // 使弹幕不再移动
            Projectile.ai[1] = target.whoAmI; // 记录目标ID，确保跟随目标移动
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasSplit)
            {
                NPC.HitModifiers modifiers = default; // 创建一个可赋值的变量
                ModifyHitNPC(target, ref modifiers);  // 传递这个变量
            }
        }



    }
}