using CalamityMod.Buffs.StatDebuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.BPrePlantera.EarthenC
{
    public class EarthenJavSHARD : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.BPrePlantera";

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation;
            SpriteEffects effects = SpriteEffects.None;

            // 渐进式描边数量：从 0 ~ 8 层
            int outlineCount = Utils.Clamp((int)(spawnTime / 2.5f), 0, 8);
            float outlineOffset = 1.5f;
            Color outlineColor = Color.SandyBrown * 0.4f;
            outlineColor.A = 0;

            for (int i = 0; i < outlineCount; i++)
            {
                float angle = MathHelper.TwoPi * i / outlineCount;
                Vector2 offset = angle.ToRotationVector2() * outlineOffset;
                Main.spriteBatch.Draw(texture, drawPos + offset, null, outlineColor, rotation, origin, Projectile.scale, effects, 0f);
            }

            // 本体绘制
            Main.spriteBatch.Draw(texture, drawPos, null, lightColor, rotation, origin, Projectile.scale, effects, 0f);
            return false;
        }

        public override void SetDefaults()
        {
            base.Projectile.width = 10;
            base.Projectile.height = 10;
            base.Projectile.friendly = true;
            base.Projectile.DamageType = DamageClass.Melee;
            base.Projectile.penetrate = 3;
            base.Projectile.aiStyle = 1;
            base.Projectile.timeLeft = 250;
            base.Projectile.tileCollide = true;
            AIType = ProjectileID.WoodenArrowFriendly;
        }
        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/SSL/钻地武器进入地面的音效") with { Volume = 1.7f, Pitch = -0.2f }, Projectile.Center);

            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 1.5f);

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    Main.rand.NextBool() ? DustID.Dirt : DustID.Stone,
                    velocity,
                    60,
                    Color.SandyBrown,
                    Main.rand.NextFloat(0.8f, 1.2f)
                );
                d.noGravity = true;
            }
        }
        private int spawnTime = 0;
        private bool hasJumped = false;
        public int spawnDirection = 1; // +1 右, -1 左（默认右）

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.Y;

            {
                spawnTime++;

                if (spawnTime <= 5)
                {
                    Projectile.velocity = Vector2.Zero;
                }
                else
                {
                    if (!hasJumped)
                    {
                        // 搜索半径范围内的敌人
                        float searchRadius = 800f; // 半径像素（50格）
                        NPC target = null;
                        float minDist = searchRadius;

                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                            {
                                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    target = npc;
                                }
                            }
                        }

                        if (target != null)
                        {
                            // 🚀 朝敌人跳，速度 ×2.5
                            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                            Projectile.velocity = toTarget * 10.5f * 2.5f;
                        }
                        else
                        {
                            // 🪂 原有兜底逻辑：往上轻微偏移
                            float sideBias = spawnDirection < 0 ? -1f : 1f;
                            float offsetAngle = MathHelper.ToRadians(5f) * sideBias;
                            Vector2 launchVelocity = Vector2.UnitY.RotatedBy(offsetAngle) * -10.5f;
                            Projectile.velocity = launchVelocity;
                        }

                        hasJumped = true;
                    }


                    // 更真实的重力模拟（逐渐下落，而非速度指数增长）
                    Projectile.velocity.Y += 0.1f; // 每帧叠加
                }
            }

            {
                // === 🧱 Earthen 微型尘土特效 ===
                if (spawnTime <= 20)
                {
                    // ⏳ 停滞阶段：围绕本体生成轻微扰动的漂浮尘土
                    if (Main.rand.NextBool(55))
                    {
                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center + Main.rand.NextVector2Circular(2f, 2f),
                            DustID.Dirt,
                            Main.rand.NextVector2Circular(0.2f, 0.2f),
                            100,
                            Color.SandyBrown,
                            Main.rand.NextFloat(0.8f, 1.1f)
                        );
                        d.noGravity = true;
                    }
                }
                else
                {
                    // 🚀 飞行阶段：喷射向下/向后的土屑
                    if (Main.rand.NextBool(8))
                    {
                        Vector2 sprayDir = -Projectile.velocity.SafeNormalize(Vector2.UnitY);
                        Vector2 dustVel = sprayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.2f, 0.2f);

                        Dust d = Dust.NewDustPerfect(
                            Projectile.Center,
                            Main.rand.NextBool() ? DustID.Dirt : DustID.Stone,
                            dustVel,
                            80,
                            Color.SaddleBrown,
                            Main.rand.NextFloat(0.9f, 1.2f)
                        );
                        d.noGravity = Main.rand.NextBool(3); // 少量无重力粒子
                    }
                }

            }
            //// 灵感来自于shellshocklive里面的武器："卫星"
            //{
            //    //// 这个是线性反转，也就是折线
            //    //gravityTimer++;
            //    //if (gravityTimer >= gravityDuration)
            //    //{
            //    //    gravityTimer = 0;

            //    //    // 每次周期结束，反转重力方向
            //    //    gravityDirection *= -1f;

            //    //    // 生成新的周期长度（每次不同）
            //    //    gravityDuration = Main.rand.Next(20, 46); // 周期 20~45 帧
            //    //}

            //    //// 垂直速度线性增加（每帧都加）
            //    //verticalSpeed += speedGainPerFrame;

            //    //// 应用速度变化
            //    //Projectile.velocity.Y = gravityDirection * verticalSpeed;

            //    //// 保持水平速度不变（这句不能省，否则你之前的 *= 会不断衰减）
            //    //Projectile.velocity.X = Projectile.velocity.X;



            //    // 这个是指数反转，也就是每次都曲线
            //    // 每gravityDuration帧反转一次重力方向
            //    gravityTimer++;
            //    if (gravityTimer >= gravityDuration)
            //    {
            //        gravityTimer = 0;
            //        gravityDirection *= -1f;
            //        gravityDuration = Main.rand.Next(20, 46); // 每段时长变动
            //    }

            //    // 垂直加速度累加（模拟抛物线效果）
            //    Projectile.velocity.Y += gravityDirection * gravityAccel;

            //    // 水平速度维持不变
            //    // （可略做扰动让轨迹更“生动”）
            //    Projectile.velocity.X = Projectile.velocity.X;


            //}


            //if (Projectile.velocity.Y <= 0f)
            //    Projectile.velocity.Y = 0.1f;
            //Projectile.rotation += Projectile.velocity.Y;


          


            //if (Main.rand.NextBool(2)) // 每帧约50%概率生成
            //{
            //    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.Dirt);
            //    d.scale = Main.rand.NextFloat(1.2f, 1.6f);
            //    d.velocity = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
            //    d.noGravity = false;
            //}

            //if (Main.rand.NextBool(4)) // 每4帧左右生成一次
            //{
            //    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Stone);
            //    d.scale = 1.0f;
            //    d.velocity = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * 1.2f;
            //    d.noGravity = true;
            //}

            // 钻地期间的特效
            //if (Projectile.velocity.Y > 4f && Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SandstormInABottle);
            //        d.velocity = Main.rand.NextVector2Circular(2f, 1.5f);
            //        d.scale = 1.3f;
            //        d.noGravity = true;
            //    }
            //}

        }

        // 在类里新建一个字段
        private float damageMultiplier = 1f;
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 每次击中，提升 25%
            damageMultiplier *= 1.25f;

            // 应用到伤害
            modifiers.FinalDamage *= damageMultiplier;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 60);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i <= 2; i++)
            {
                Dust.NewDust(base.Projectile.position + base.Projectile.velocity, base.Projectile.width, base.Projectile.height, 32, base.Projectile.oldVelocity.X * 0.5f, base.Projectile.oldVelocity.Y * 0.5f);
            }

            for (int i = 0; i < 12; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.Dirt : DustID.Stone;
                Dust d = Dust.NewDustPerfect(Projectile.Center, dustType);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.scale = Main.rand.NextFloat(1.5f, 2.3f);
                d.noGravity = Main.rand.NextBool();
            }

        }
    }
}