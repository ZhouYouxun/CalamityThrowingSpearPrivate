using CalamityMod.Particles;
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
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.Surfeiter
{
    public class SurfeiterPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/CPreMoodLord/Surfeiter/Surfeiter";
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        private struct GhostRecord
        {
            public Vector2 position;
            public float rotation;
            public float alpha;
        }
        private List<GhostRecord> ghostList = new();

        private struct BloodCircleRecord
        {
            public Vector2 position;
            public float alpha;
            public float scale;
        }
        private List<BloodCircleRecord> circleList = new();

        private float ghostSpawnTimer = 0f;
        private Vector2 lastCirclePosition;
        private float circleDistanceThreshold = 80f;
        private float circleTimer = 0f;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            if (!Projectile.hide)
            {
                // ✅ 正常情况下绘制拖尾
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

                // ✅ 幻影绘制
                Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
                Vector2 origin = texture.Size() / 2f;
                foreach (GhostRecord ghost in ghostList)
                {
                    Color ghostColor = Color.DarkRed * ghost.alpha;
                    sb.Draw(texture,
                        ghost.position - Main.screenPosition,
                        null,
                        ghostColor,
                        ghost.rotation,
                        origin,
                        Projectile.scale,
                        SpriteEffects.None,
                        0);
                }
            }

            // ✅ 不论 hide 状态如何，始终绘制血圈
            //Texture2D circleTex = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/KsTexture/circle_03").Value;
            //foreach (BloodCircleRecord circle in circleList)
            //{
            //    Color bloodColor = new Color(160, 0, 0) * circle.alpha;
            //    sb.Draw(circleTex,
            //        circle.position - Main.screenPosition,
            //        null,
            //        bloodColor,
            //        0f,
            //        circleTex.Size() / 2,
            //        circle.scale,
            //        SpriteEffects.None,
            //        0);
            //}

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 将光源颜色改为浅红色，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.LightCoral.ToVector3() * 0.55f);


            // 释放随机粒子特效
            for (int i = 0; i < 2; i++)
            {
                int dustType = Main.rand.Next(new int[] { DustID.Blood, DustID.IceTorch, DustID.Granite });
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(2 * 16, 2 * 16);
                Dust dust = Dust.NewDustPerfect(dustPos, dustType);
                dust.scale = Main.rand.NextFloat(0.75f, 1.45f);
                dust.noGravity = true;
            }
            // 👻 生成幻影（每隔3帧）
            ghostSpawnTimer++;
            if (ghostSpawnTimer >= 3f)
            {
                ghostSpawnTimer = 0f;
                Vector2 randomOffset = Main.rand.NextVector2Circular(6f, 6f);
                ghostList.Add(new GhostRecord
                {
                    position = Projectile.Center + randomOffset,
                    rotation = Projectile.rotation,
                    alpha = 0.9f
                });
            }
            for (int i = 0; i < ghostList.Count; i++)
            {
                GhostRecord g = ghostList[i];
                g.alpha -= 0.05f;
                if (g.alpha <= 0f)
                    ghostList.RemoveAt(i--);
                else
                    ghostList[i] = g;
            }

            // 🔴 法阵圆圈生成（按距离）
            if (!Projectile.hide)
            {
                if (Vector2.Distance(Projectile.Center, lastCirclePosition) > circleDistanceThreshold)
                {
                    lastCirclePosition = Projectile.Center;
                    circleList.Add(new BloodCircleRecord
                    {
                        position = Projectile.Center,
                        scale = Main.rand.NextFloat(0.08f, 0.1f),
                        alpha = 1f
                    });
                }
            }



            for (int i = 0; i < circleList.Count; i++)
            {
                BloodCircleRecord c = circleList[i];
                c.alpha -= 0.03f;
                c.scale += 0.01f;
                if (c.alpha <= 0f)
                    circleList.RemoveAt(i--);
                else
                    circleList[i] = c;
            }

        }

        public override void OnKill(int timeLeft)
        {
            // 屏幕震动效果
            float shakePower = 25f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);


            // 释放重型烟雾
            for (int i = 0; i < 40; i++)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, Main.rand.NextVector2Circular(3 * 16, 3 * 16), Color.Black, 15, 0.9f, 0.5f, 0.2f, true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            for (int i = 0; i < 75; i++)
            {
                Particle smoke = new HeavySmokeParticle(Projectile.Center, Main.rand.NextVector2Circular(5 * 16, 5 * 16), Color.Black, 15, 0.9f, 0.5f, 0.2f, true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // 释放“土”字形粒子
            Vector2 basePos = Projectile.Center;
            for (int i = -3; i <= 3; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    Vector2 dustPos = basePos + new Vector2(i * 6, j * 6);
                    int dustType = Main.rand.Next(new int[] { DustID.Blood, DustID.IceTorch, DustID.Granite });
                    Dust dust = Dust.NewDustPerfect(dustPos, dustType);
                    dust.scale = 1f;
                    dust.noGravity = true;
                }
            }

            // 生成左右两侧的 SurfeiterStonePillars
            int leftPillar = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - new Vector2(30 * 16, 0), Vector2.Zero, ModContent.ProjectileType<SurfeiterStonePillars>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            int rightPillar = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(30 * 16, 0), Vector2.Zero, ModContent.ProjectileType<SurfeiterStonePillars>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

            // 传递信息给 SurfeiterStonePillars
            if (Main.projectile[leftPillar].ModProjectile is SurfeiterStonePillars left)
                left.SetDirection(1);
            if (Main.projectile[rightPillar].ModProjectile is SurfeiterStonePillars right)
                right.SetDirection(-1);

            string soundToPlay = Main.rand.NextBool()
    ? "CalamityMod/Sounds/Custom/Ravager/RavagerStomp1"
    : "CalamityMod/Sounds/Custom/Ravager/RavagerStomp2";

            SoundEngine.PlaySound(new SoundStyle(soundToPlay), Projectile.Center);
        }
        private bool hasTriggeredEffect = false;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasTriggeredEffect)
                return;

            hasTriggeredEffect = true;

            // === 🔴 血肉烟尘喷发（自然方向 + 不再是矩形） ===
            for (int i = 0; i < 140; i++)
            {
                // 以正上方向（-Y）为基础，±45°内随机喷射
                float angle = MathHelper.ToRadians(Main.rand.NextFloat(-45f, 45f));
                Vector2 velocity = (-Vector2.UnitY).RotatedBy(angle) * Main.rand.NextFloat(8f, 16f);

                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, velocity);
                d.scale = Main.rand.NextFloat(1.3f, 2.2f);
                d.noGravity = false;
                d.fadeIn = 1.5f;
            }

            // === 🌫️ 重型黑烟柱（从爆心上喷）===
            for (int i = 0; i < 20; i++)
            {
                Particle smoke = new HeavySmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(3f, 6f)),
                    Color.Black,
                    30,
                    Main.rand.NextFloat(1.0f, 1.6f),
                    0.5f,
                    0.02f,
                    true
                );
                GeneralParticleHandler.SpawnParticle(smoke);
            }

            // === 🔻 AltSparkParticle：低亮度赤红能量点 ===
            for (int i = 0; i < 15; i++)
            {
                AltSparkParticle alt = new AltSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    new Vector2(Main.rand.NextFloat(-5.5f, 5.5f), -Main.rand.NextFloat(5f, 15f)),
                    false,
                    24,
                    2.3f,
                    Color.DarkRed * 0.4f
                );
                GeneralParticleHandler.SpawnParticle(alt);
            }

            // === 🩸 CritSpark：血光四散爆裂（全方向 + 明亮） ===
            for (int i = 0; i < 30; i++)
            {
                // 从圆周方向随机生成
                Vector2 dir = Main.rand.NextVector2Unit(); // 等价于随机角度单位向量
                Vector2 sparkVelocity = dir * Main.rand.NextFloat(30f, 60f); // 同样高速

                CritSpark spark = new CritSpark(
                    Projectile.Center,
                    sparkVelocity,
                    Color.Lerp(Color.IndianRed, Color.Red, 0.4f), // 起始色更亮偏肉白
                    Color.Lerp(Color.Red, Color.Yellow, 0.3f),      // 结束色更热更亮
                    3.1f,
                    60
                );
                GeneralParticleHandler.SpawnParticle(spark);
            }


            // === 🧿 仪式感能量环（保留）===
            Particle ring = new CustomPulse(
                Projectile.Center + new Vector2(0, -16),
                Vector2.Zero,
                Color.DarkRed,
                "CalamityMod/Particles/HighResHollowCircleHardEdge",
                new Vector2(1f, 1.6f),
                0f,
                0.06f,
                0.24f,
                25
            );
            GeneralParticleHandler.SpawnParticle(ring);

            // === 🫥 弹幕处理：退出状态 ===
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;

        }



    }
}