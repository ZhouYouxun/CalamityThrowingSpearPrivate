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
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    internal class SurfeiterDrum : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-1";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }


        //1.笞:Flogging-敌怪获得1.02倍的易伤
        //2.杖:Beating-敌怪的接触伤害减少30%
        //3.徒:Imprisoning-敌怪的移动速度减少50%
        //4.流:Banishing-敌怪防御降低40
        //5.死:Executing-2秒的倒计时结束后造成5000点伤害
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 50;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        private int prevForm = 0;
        private float transitionTimer = 0f;
        private const float transitionDuration = 12f; // 持续 12 帧（0.2秒）

        // 添加形态索引
        private int drumForm = 0; // 0-4 对应五种形态
        public void SwitchForm()
        {
            prevForm = drumForm;
            drumForm = (drumForm + 1) % 5;
            transitionTimer = transitionDuration;
            Projectile.netUpdate = true;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false; // 永远不会命中 NPC
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.timeLeft = 50;
            // 如果玩家不存在或死亡，移除弹幕
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (transitionTimer > 0f)
                transitionTimer -= 1f;


            // 跟随玩家头顶
            Vector2 destination = player.Center + new Vector2(0, -15 * 16);
            Projectile.velocity = (destination - Projectile.Center) * 0.1f;

            // 检测与 SurfeiterPROJ 的碰撞
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<SurfeiterPROJ>() && proj.Hitbox.Intersects(Projectile.Hitbox))
                {
                    int projDamage = (int)(proj.damage * 1.0f); // 继承 SurfeiterPROJ 的伤害 的1.0倍
                    for (int i = 0; i < 5; i++)
                    {
                        int projID = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            new Vector2(6, 0).RotatedBy(MathHelper.ToRadians(72 * i)),
                            ModContent.ProjectileType<SurfeiterDrumINV>(),
                            projDamage, // 传递 SurfeiterPROJ 的伤害
                            Projectile.knockBack,
                            Projectile.owner);

                        // 传递模式
                        if (Main.projectile[projID].ModProjectile is SurfeiterDrumINV drumINV)
                        {
                            drumINV.SetDrumForm(drumForm);
                        }

                    }

                    // 释放泥土粒子
                    for (int i = 0; i < Main.rand.Next(50, 101); i++)
                    {
                        Dust.NewDust(Projectile.Center, 10, 10, DustID.Dirt, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                    }
                    {
                        // 🪵 层1：爆发性棕色 Dust（木屑 / 碎片）
                        for (int i = 0; i < 60; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                            int dustType = Main.rand.NextBool() ? DustID.BorealWood : DustID.RichMahogany;
                            Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel);
                            d.scale = Main.rand.NextFloat(1.0f, 1.6f);
                            d.noGravity = true;
                            d.color = Color.Lerp(new Color(90, 40, 20), Color.SaddleBrown, Main.rand.NextFloat());
                        }

                        // 🌀 层2：符文粒子爆炸（魔法感强化）
                        for (int i = 0; i < 6; i++)
                        {
                            Particle runePulse = new CustomPulse(
                                Projectile.Center,
                                Vector2.Zero,
                                Color.Brown,
                                "CalamityThrowingSpear/Texture/KsTexture/light_01",
                                Vector2.One * Main.rand.NextFloat(0.8f, 1.2f),
                                Main.rand.NextFloat(-10f, 10f),
                                0.03f,
                                0.16f,
                                18
                            );
                            GeneralParticleHandler.SpawnParticle(runePulse);
                        }

                        // 💀 层3：骷髅头炸裂（黑暗仪式感）
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 skullVel = Main.rand.NextVector2Circular(3f, 3f);
                            Particle skull = new DesertProwlerSkullParticle(
                                Projectile.Center,
                                skullVel,
                                Color.DarkGray * 0.8f,
                                Color.LightGray,
                                Main.rand.NextFloat(0.5f, 1.1f),
                                180
                            );
                            GeneralParticleHandler.SpawnParticle(skull);
                        }

                        // 🔥 层4：棕红能量火花
                        for (int i = 0; i < 16; i++)
                        {
                            Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                            Particle trail = new SparkParticle(
                                Projectile.Center,
                                sparkVel,
                                false,
                                50,
                                Main.rand.NextFloat(0.9f, 1.4f),
                                Color.OrangeRed
                            );
                            GeneralParticleHandler.SpawnParticle(trail);
                        }

                        // 🪨 层5：深色石尘 + 重力感尘块
                        for (int i = 0; i < 25; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                            Dust dirt = Dust.NewDustPerfect(Projectile.Center + vel * 3f, DustID.Stone, vel);
                            dirt.scale = Main.rand.NextFloat(1.2f, 1.8f);
                            dirt.color = Color.DarkGray;
                            dirt.noGravity = false;
                        }

                        // 🌪️ 层6：四向 Dust 爆炸（风）
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 dir = Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i);
                            for (int j = 0; j < 10; j++)
                            {
                                Dust d = Dust.NewDustPerfect(
                                    Projectile.Center + dir * 4f,
                                    DustID.SandstormInABottle,
                                    dir.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 4f),
                                    100,
                                    Color.Brown,
                                    Main.rand.NextFloat(1f, 1.4f)
                                );
                                d.noGravity = true;
                            }
                        }

                        // 🌊 层7：扩散冲击波
                        Particle blast = new CustomPulse(
                            Projectile.Center,
                            Vector2.Zero,
                            Color.SaddleBrown,
                            "CalamityThrowingSpear/Texture/KsTexture/circle_03",
                            Vector2.One,
                            0f,
                            0.04f,
                            0.22f,
                            22
                        );
                        GeneralParticleHandler.SpawnParticle(blast);
                    }

                    // 消除 SurfeiterPROJ
                    proj.Kill();
                    //Projectile.Kill(); 把自己杀了干嘛，神经
                    break;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float progress = Utils.GetLerpValue(transitionDuration, 0f, transitionTimer, true); // 从1降到0

            // 主贴图路径
            string texturePathCurrent = $"CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-{drumForm + 1}";
            Texture2D textureCurrent = ModContent.Request<Texture2D>(texturePathCurrent).Value;

            // 旧贴图路径（用于过渡）
            string texturePathOld = $"CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/SurfeiterDrum-{prevForm + 1}";
            Texture2D textureOld = ModContent.Request<Texture2D>(texturePathOld).Value;

            Vector2 slideOffset = new Vector2(40f, 0f);

            if (transitionTimer > 0f)
            {
                // === 旧贴图滑出 ===
                Vector2 oldOffset = Vector2.Lerp(Vector2.Zero, -slideOffset, progress);
                Main.spriteBatch.Draw(textureOld,
                    drawPos + oldOffset,
                    null,
                    lightColor * progress,
                    Projectile.rotation,
                    textureOld.Size() / 2,
                    Projectile.scale,
                    SpriteEffects.None,
                    0);

                // === 新贴图滑入 ===
                Vector2 newOffset = Vector2.Lerp(slideOffset, Vector2.Zero, 1f - progress);
                Main.spriteBatch.Draw(textureCurrent,
                    drawPos + newOffset,
                    null,
                    lightColor * (1f - progress),
                    Projectile.rotation,
                    textureCurrent.Size() / 2,
                    Projectile.scale,
                    SpriteEffects.None,
                    0);
            }
            else
            {
                // === 默认状态：直接绘制当前形态贴图 ===
                Main.spriteBatch.Draw(textureCurrent,
                    drawPos,
                    null,
                    lightColor,
                    Projectile.rotation,
                    textureCurrent.Size() / 2,
                    Projectile.scale,
                    SpriteEffects.None,
                    0);

                // === 额外加一层：棕色柔和外圈光晕（略大 + 染色 + 透明）===
                Color auraColor = new Color(100, 60, 40, 80); // 带Alpha的棕色
                float auraScale = Projectile.scale * 1.2f;

                Main.spriteBatch.Draw(textureCurrent,
                    drawPos,
                    null,
                    auraColor,
                    Projectile.rotation,
                    textureCurrent.Size() / 2,
                    auraScale,
                    SpriteEffects.None,
                    0);
            }

            return false;
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {


        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }
    }
}