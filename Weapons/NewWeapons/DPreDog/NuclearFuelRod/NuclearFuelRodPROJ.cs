using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using CalamityMod;
using CalamityMod.Sounds;
using Terraria.Audio;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    public class NuclearFuelRodPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/NuclearFuelRod/NuclearFuelRod";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        private int phase = 0;
        private int phaseTimer = 0;
        private bool laserSpawned = false;

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }
        private float mFireTimer = 0f; // M弹幕机枪计时
        private float mAngleOffset = 0f; // 顺时针旋转角
        private float mAngleOffset2 = 0f; // 逆时针旋转角

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (phase == 0)
            {
                // 初始旋转
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                phase++;
            }
            else if (phase == 1)
            {
                // 第一阶段：逐渐减速 + 转向最近敌人
                Projectile.velocity *= 0.98f;

                // ✅ 恢复追踪最近敌人以更新 rotation
                NPC target = Projectile.Center.ClosestNPCAt(1500f);
                if (target != null)
                {
                    Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    float targetRotation = directionToTarget.ToRotation() + MathHelper.PiOver4;
                    Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.1f);
                }

                phaseTimer++;
                if (phaseTimer >= 30)
                {
                    phase++;
                    phaseTimer = 0;
                }
            }
            else if (phase == 2)
            {
                Projectile.velocity *= 0.95f;


                if (!laserSpawned && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // 在发射激光前先精准对准敌人方向
                    NPC target = Projectile.Center.ClosestNPCAt(1500f);
                    if (target != null)
                    {
                        Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Projectile.rotation = directionToTarget.ToRotation() + MathHelper.PiOver4;
                    }

                    int laser = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Vector2.Zero, // 不用管速度
                        ModContent.ProjectileType<NuclearFuelRodLazer>(),
                        (int)(Projectile.damage * 0.514),
                        Projectile.knockBack,
                        Projectile.owner,
                        Projectile.whoAmI); // ai[0] 存储父弹幕

                    if (laser.WithinBounds(Main.maxProjectiles))
                    {
                        Main.projectile[laser].ai[0] = Projectile.whoAmI; // 保守起见再存一次
                    }
                    SoundEngine.PlaySound(SoundID.Zombie104, Projectile.Center);
                    laserSpawned = true;
                }

                if (laserSpawned)
                {
     

                    mFireTimer++;
                    if (mFireTimer % 3f == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 gunTip = Projectile.Center + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 48f;

                        // ✅ 添加保险获取 target
                        NPC target = Projectile.Center.ClosestNPCAt(1500f);
                        if (target != null)
                        {
                            Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                            float targetRotation = directionToTarget.ToRotation() + MathHelper.PiOver4;

                            // 在 phase 2 追踪目标时（转向速度 0.15f 保留）
                            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.15f);
                        }

                        // 在 M 弹幕发射部分：
                        mAngleOffset += MathHelper.ToRadians(0.4f * 2.5f);    // 顺时针旋转角（减小波动，提升旋转速度X.5x）
                        mAngleOffset2 -= MathHelper.ToRadians(0.4f * 2.5f);   // 逆时针旋转角

                        Vector2 dir1 = (Projectile.rotation - MathHelper.PiOver4 + MathHelper.Pi + mAngleOffset).ToRotationVector2() * 0.5f; // 基速
                        Vector2 dir2 = (Projectile.rotation - MathHelper.PiOver4 + MathHelper.Pi + mAngleOffset2).ToRotationVector2() * 0.5f; // 基速

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            gunTip,
                            dir1,
                            ModContent.ProjectileType<NuclearFuelRodM>(),
                            (int)(Projectile.damage * 0.1145),
                            2f,
                            Projectile.owner
                        );

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            gunTip,
                            dir2,
                            ModContent.ProjectileType<NuclearFuelRodM>(),
                            (int)(Projectile.damage * 0.1145),
                            2f,
                            Projectile.owner
                        );






                        Vector2 gunTi2p = Projectile.Center + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 48f;
                        float baseRotation = (Projectile.rotation - MathHelper.PiOver4);

                        // 添加较大 ±20° 随机偏移
                        float randomOffset = MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f));
                        Vector2 velocity = (baseRotation + randomOffset).ToRotationVector2() * 10f;

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            gunTi2p,
                            velocity,
                            ModContent.ProjectileType<NuclearFuelRodL>(),
                            (int)(Projectile.damage * 0.1145),
                            2f,
                            Projectile.owner
                        );

                    }
                }

                {
                    // 更新飞行期间粒子生成位置
                    // 将所有生成位置从 Projectile.Center 改为 "枪头位置" 并让粒子向前随机喷射
                    // 这很重要！枪头位置应该是这样子的，因为这是基于rotation而并非velocity！
                    Vector2 gunTip = Projectile.Center + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 48f;

                    Vector2 sprayDirection = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();

                    for (int i = 0; i < 3; i++)
                    {
                        Particle pulse = new DirectionalPulseRing(
                            gunTip,
                            sprayDirection * 30f + Main.rand.NextVector2Circular(3f, 3f), // 6f -> 30f (5倍)，扰动加大
                            Color.Green,
                            new Vector2(1f, 2.5f),
                            Projectile.rotation - MathHelper.PiOver4,
                            0.2f,
                            0.03f,
                            20
                        );
                        GeneralParticleHandler.SpawnParticle(pulse);

                        Particle trail = new SparkParticle(
                            gunTip,
                            sprayDirection * 15f + Main.rand.NextVector2Circular(2f, 2f), // 3f -> 15f (5倍)，扰动加大
                            false,
                            60,
                            1.0f,
                            Color.LimeGreen
                        );
                        GeneralParticleHandler.SpawnParticle(trail);
                    }

                    int[] dustTypes = new int[] { 74, 75, 107, 110 };
                    for (int i = 0; i < 5; i++)
                    {
                        int dustType = dustTypes[Main.rand.Next(dustTypes.Length)];
                        Vector2 dustVelocity = sprayDirection * Main.rand.NextFloat(5f, 20f) + Main.rand.NextVector2Circular(2f, 2f); // 原1~4f -> 5~20f (5倍)，扰动加大
                        Dust.NewDustPerfect(gunTip, dustType, dustVelocity, 150, Color.LimeGreen, 1.2f);
                    }


                    if (laserSpawned)
                    {
                        float shakePower = 5f;
                        float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                        Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
                    }



                    phaseTimer++;
                    if (phaseTimer >= 180) // 激光形态持续多久
                    {
                        Projectile.Kill();
                    }
                }

              
            }
        }
        private void SafeTrackClosestNPC(float maxTurnDegrees)
        {
            NPC target = Projectile.Center.ClosestNPCAt(1500f); // 直接 NPC，不需要 NPC?
            if (target != null) // 防御性保险（虽一般不为 null）
            {
                Vector2 directionToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                float targetRotation = directionToTarget.ToRotation() + MathHelper.PiOver4;
                float currentRotation = Projectile.rotation;
                float maxTurn = MathHelper.ToRadians(maxTurnDegrees);
                Projectile.rotation = currentRotation.AngleLerp(targetRotation, maxTurn / Math.Abs(MathHelper.WrapAngle(targetRotation - currentRotation)));
            }
        }



        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center); // 强烈爆炸音效

            Vector2 gunTip = Projectile.Center + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 48f;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 32; // 酸雾弹数量可调
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi / count * i + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                    Projectile.NewProjectile(
                        Projectile.GetSource_Death(),
                        gunTip,
                        velocity,
                        ModContent.ProjectileType<NuclearFuelRodSAM>(), // Acid Mist
                        (int)(Projectile.damage * 0.1145), // 酸雾伤害，可调
                        2f,
                        Projectile.owner
                    );
                }
            }

            // 华丽绿色退场粒子雨
            for (int i = 0; i < 80; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 107);
                dust.velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(6f, 18f);
                dust.scale = Main.rand.NextFloat(1.2f, 2.0f);
                dust.noGravity = true;
                dust.color = Color.LimeGreen;
            }
        }



        public override bool PreDraw(ref Color lightColor)
        {
            //// IonizingRadiation 缩小到当前的 1/3，随机在本体周围生成，平滑飞行至准确枪头位置并线性缩小，无抖动
            //Texture2D auraTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/IonizingRadiation").Value;
            //Vector2 auraOrigin = auraTexture.Size() / 2f;
            //float baseScale = 0.066f; // 原 1/3 缩放
            //float auraRotation = Main.GlobalTimeWrappedHourly * 1.5f;

            //Vector2 gunTip = Projectile.Center + (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() * 48f;

            //for (int j = 0; j < 5; j++)
            //{
            //    Vector2 spawnOffset = Main.rand.NextVector2Circular(60f, 60f);
            //    Vector2 startPos = Projectile.Center + spawnOffset;
            //    Vector2 auraDrawPos = Vector2.Lerp(startPos, gunTip, Utils.GetLerpValue(0f, 30f, phaseTimer, true)) - Main.screenPosition;
            //    float auraScale = MathHelper.Lerp(0.12f, baseScale, 1f - Utils.GetLerpValue(0f, 30f, phaseTimer, true)); // 飞行过程中线性缩小

            //    Main.spriteBatch.Draw(
            //        auraTexture,
            //        auraDrawPos,
            //        null,
            //        Color.Lime * 0.12f,
            //        auraRotation + j,
            //        auraOrigin,
            //        auraScale,
            //        SpriteEffects.None,
            //        0f
            //    );
            //}


            if (laserSpawned)
            {
                Texture2D ghostTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value; // 改名
                Vector2 ghostOrigin = ghostTexture.Size() * 0.5f;
                Vector2 basePosition = Projectile.Center - Main.screenPosition;
                float rotation1 = Projectile.rotation;
                SpriteEffects direction1 = SpriteEffects.None;

                for (int i = 0; i < 8; i++)
                {
                    float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f)); // 小幅方向扰动
                    float distanceOffset = Main.rand.NextFloat(2f, 12f); // 偏移距离
                    Vector2 offset = (rotation1 + angleOffset).ToRotationVector2() * distanceOffset;

                    Color glowColor = Color.LimeGreen * Main.rand.NextFloat(0.1f, 0.3f); // 透明度随机化增加躁动感
                    glowColor.A = 0;

                    Main.spriteBatch.Draw(
                        ghostTexture,                // 使用改名变量
                        basePosition + offset,
                        null,
                        glowColor,
                        rotation1,
                        ghostOrigin,                 // 使用改名变量
                        Projectile.scale * Main.rand.NextFloat(0.9f, 1.2f),
                        direction1,
                        0f
                    );
                }
            }



            // 保持本体带荧光绿色 - 深绿色动态描边不变
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            float outlineOffset = 2.5f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.5f;
            float rotation = Projectile.rotation;
            SpriteEffects direction = SpriteEffects.None;

            Color outerColor = Color.LimeGreen * 0.4f;
            Color innerColor = Color.Green * 0.4f;
            outerColor.A = 0;
            innerColor.A = 0;

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * outlineOffset;
                Color lerpColor = Color.Lerp(outerColor, innerColor, (float)i / 8f);
                Main.spriteBatch.Draw(texture, drawPosition + offset, null, lerpColor, rotation, origin, Projectile.scale, direction, 0f);
            }

            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }







    }
}
