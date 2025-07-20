using CalamityMod.Particles;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Audio;
using CalamityMod;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav
{
    public class BloodstoneRIGHT : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/BloodstoneJav/BloodstoneJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft > 295)
            {
                // 初始阶段不绘制任何内容
                return false;
            }

            //Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            //Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            //Vector2 origin = texture.Size() * 0.5f;

            //// === 残影绘制 ===
            //for (int i = 0; i < Projectile.oldPos.Length; ++i)
            //{
            //    float afterimageRot = Projectile.oldRot[i];
            //    Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            //    Color afterimageColor = Color.MediumVioletRed * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
            //    Main.spriteBatch.Draw(texture, drawPos, null, afterimageColor, afterimageRot, origin, Projectile.scale * 0.5f, 0, 0f);

            //    if (i > 0)
            //    {
            //        for (float j = 0.2f; j < 0.8f; j += 0.2f)
            //        {
            //            drawPos = Vector2.Lerp(Projectile.oldPos[i - 1], Projectile.oldPos[i], j) +
            //                Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            //            Main.spriteBatch.Draw(texture, drawPos, null, afterimageColor, afterimageRot, origin, Projectile.scale * 0.5f, 0, 0f);
            //        }
            //    }
            //}



            {
                Texture2D mainTex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Rectangle frame = mainTex.Frame();
                Vector2 origin = frame.Size() * 0.5f;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                // === 1️⃣ 呼吸透明度 ===
                float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + Projectile.whoAmI) * 0.15f + 0.85f;
                Color pulseColor = Color.White * pulse;
                pulseColor.A = 0;

                // === 2️⃣ 绘制残影（原版保留） ===
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], pulseColor, 1);

                // === 3️⃣ 主体贴图绘制（用于遮罩/本体） ===
                Main.EntitySpriteDraw(mainTex, drawPos, frame, pulseColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

                // === 4️⃣ 顶端发光核心绘制 ===
                Vector2 tip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * (Projectile.height * 0.5f); // 顶端位置

                // 🧩 主光效材质
                Texture2D coreTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_89").Value;
                Vector2 coreOrigin = coreTex.Size() * 0.5f;
                Color coreColor = Color.Red * 0.8f;
                coreColor.A = 0;

                Main.EntitySpriteDraw(coreTex, tip - Main.screenPosition, null, coreColor, 0f, coreOrigin, 1.2f, SpriteEffects.None, 0);




                // === 🔧 尺寸缩放因子统一设置（方便调试） ===
                float flareScale1 = 0.25f;         // fx_Flare9 第一道（小圈）
                float flareScale2 = 0.45f;         // fx_Flare9 第二道（大圈）
                float energyBaseScale = 0.06f;      // energy_001 基础缩放倍数（会脉动）

                // === 5️⃣ 叠加旋转光纹 fx_Flare9 ===
                Texture2D flareTex = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/fx_Flare9").Value;
                Vector2 flareOrigin = flareTex.Size() * 0.5f;
                float flareRot = Main.GlobalTimeWrappedHourly * 1.8f;
                Color flareColor = Color.White * 0.4f;
                flareColor.A = 0;

                Main.EntitySpriteDraw(flareTex, tip - Main.screenPosition, null, flareColor, flareRot, flareOrigin, flareScale1, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(flareTex, tip - Main.screenPosition, null, flareColor * 0.6f, -flareRot * 0.7f, flareOrigin, flareScale2, SpriteEffects.None, 0);

                // === 6️⃣ 再叠加 energy_001 模糊爆点纹理 ===
                Texture2D energyTex = ModContent.Request<Texture2D>("CalamityThrowingSpear/Texture/SuperTexturePack/energy_001").Value;
                Vector2 energyOrigin = energyTex.Size() * 0.5f;
                float scalePulse = 1f + 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f + Projectile.identity);

                Main.EntitySpriteDraw(energyTex, tip - Main.screenPosition, null, Color.Red * 0.5f, 0f, energyOrigin, scalePulse * energyBaseScale, SpriteEffects.None, 0);



                return false;
            }

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }

        private Vector2[] triangleVertices = new Vector2[3]; // 存储三角形顶点的位置
        private float rotationAngle = 0f; // 当前旋转角度

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 添加深红色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.55f);

            if (Projectile.timeLeft <= 295)
            {
                // 初始化等边三角形顶点的相对位置（半径为20像素，可以根据需要调整）
                float triangleRadius = 20f;
                if (triangleVertices[0] == Vector2.Zero)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = MathHelper.TwoPi / 3 * i; // 等分的三个顶点
                        triangleVertices[i] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * triangleRadius;
                    }
                }

                // 更新旋转角度，每帧旋转更大的角度以增加旋转速度
                rotationAngle += MathHelper.ToRadians(2); // 每帧旋转2度

                // 生成直线粒子特效
                if (Main.rand.NextBool(2)) // 每帧有 50% 概率生成
                {
                    Dust trailDust = Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(new int[] { DustID.RedTorch, DustID.Blood, DustID.LifeCrystal, DustID.GemRuby, DustID.Cloud }),
                        Projectile.velocity * Main.rand.NextFloat(0.5f, 1.2f), 0, Color.Red, Main.rand.NextFloat(1.0f, 1.5f));
                    trailDust.noGravity = true;
                }

                // 计算旋转后的顶点位置
                for (int i = 0; i < 3; i++)
                {
                    // 旋转顶点位置
                    Vector2 rotatedPosition = triangleVertices[i].RotatedBy(rotationAngle) + Projectile.Center;

                    // 外围粒子特效
                    for (int j = 0; j < 2; j++) // 每帧每个顶点生成 2 个粒子
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(3f, 3f); // 随机偏移
                        Dust vertexDust = Dust.NewDustPerfect(rotatedPosition + offset, Main.rand.Next(new int[] { DustID.RedTorch, DustID.Blood, DustID.LifeCrystal, DustID.GemRuby, DustID.Cloud }),
                            (rotatedPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.8f, 1.5f), 0, Color.Red, Main.rand.NextFloat(1.2f, 1.8f));
                        vertexDust.noGravity = true;
                    }
                }
            }
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 反弹效果
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item110 with { Volume = 1.2f, Pitch = -0.2f }, Projectile.Center);

            {
                // 恢复玩家生命值
                Player player = Main.player[Projectile.owner];
                float healMultiplier = Main.zenithWorld ? 100f : 0.01f; // 根据是否启用 zenithWorld 设置恢复倍率
                int healAmount = (int)(damageDone * healMultiplier);
                player.statLife += healAmount;
                player.HealEffect(healAmount);
            }


            {
                // 生成血雾效果
                int dustCount = 3;
                float radians = MathHelper.Pi / dustCount;
                Vector2 smokePoint = Vector2.Normalize(new Vector2(-1f, -1f));
                for (int i = 0; i < dustCount; i++)
                {
                    Vector2 dustVelocity = smokePoint.RotatedBy(radians * i - MathHelper.ToRadians(15)) * Main.rand.NextFloat(1f, 2.6f);
                    Color smokeColor = Color.Red;
                    Particle bloodFog = new HeavySmokeParticle(Projectile.Center, dustVelocity, smokeColor, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                    GeneralParticleHandler.SpawnParticle(bloodFog);
                }
            }


            {
                // 生成类似 Visceral 爆炸效果
                int projectileIndex = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<VisceraBoom>(),
                    Projectile.damage,
                    Projectile.knockBack * 4,
                    Projectile.owner
                );
                // 设置属性
                Projectile proj = Main.projectile[projectileIndex];
                proj.DamageType = DamageClass.Ranged;
            }



            {
                // 血液爆炸冲击波
                Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkRed, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, (int)(40 * 0.38f), false);
                GeneralParticleHandler.SpawnParticle(bloodsplosion);
                Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, new Color(255, 32, 32), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f, 0.155f, 40);
                GeneralParticleHandler.SpawnParticle(bloodsplosion2);
            }


        }

    }
}

