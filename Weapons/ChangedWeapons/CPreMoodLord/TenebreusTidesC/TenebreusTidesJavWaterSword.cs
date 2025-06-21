using System;
using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC
{
    public class TenebreusTidesJavWaterSword : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.CPreMoodLord";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        private int penetrationAmt = 2;
        private bool dontDraw = false;
        private int drawInt = 0;

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.penetrate = penetrationAmt;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5 * Projectile.MaxUpdates;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(penetrationAmt);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            penetrationAmt = reader.ReadInt32();
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            //// 创建小型冲击波粒子效果
            //Particle pulse = new DirectionalPulseRing(
            //    Projectile.Center, // 冲击波的中心位置
            //    Projectile.velocity * 0.75f, // 冲击波的速度，略低于弹幕的速度
            //    Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f), // 使用深蓝到浅蓝的主题颜色渐变
            //    new Vector2(1f, 2.5f), // 冲击波的大小范围
            //    Projectile.rotation - MathHelper.PiOver4, // 冲击波的旋转角度
            //    0.2f, // 初始透明度
            //    0.03f, // 透明度的衰减速度
            //    20 // 粒子寿命
            //);
            //GeneralParticleHandler.SpawnParticle(pulse);
        }

        public override void AI()
        {
            // 弹幕的速度每帧乘以 1.01，逐渐加速
            Projectile.velocity *= 1.01f;

            {
                //// 添加小型烟雾粒子
                //Color smokeColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f); // 使用之前定义的颜色渐变
                //Particle smoke = new HeavySmokeParticle(
                //    Projectile.Center,
                //    Projectile.velocity * Main.rand.NextFloat(-0.2f, -0.6f),
                //    smokeColor,
                //    30, // 粒子存活时间
                //    Main.rand.NextFloat(0.45f, 0.6f), // 粒子缩放大小
                //    0.3f,
                //    Main.rand.NextFloat(-0.2f, 0.2f),
                //    false,
                //    required: true
                //);
                //GeneralParticleHandler.SpawnParticle(smoke);

                // 添加双螺旋粒子特效
                float progress = (Projectile.localAI[0] % 60) / 60f; // 粒子进度控制
                float angle1 = MathHelper.TwoPi * progress; // 第一条螺旋
                float angle2 = MathHelper.TwoPi * (progress + 0.5f); // 第二条螺旋，相差 180 度
                Vector2 offset1 = new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * 10f; // 第一条螺旋的偏移
                Vector2 offset2 = new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * 10f; // 第二条螺旋的偏移

                // 第一条螺旋的粒子
                Dust dust1 = Dust.NewDustPerfect(Projectile.Center + offset1, DustID.Water, Projectile.velocity * 0.2f, 0, Color.DarkBlue, 1.2f);
                dust1.noGravity = true;

                // 第二条螺旋的粒子
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center + offset2, DustID.Water, Projectile.velocity * 0.2f, 0, Color.CadetBlue, 1.2f);
                dust2.noGravity = true;

            }


            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);

            // 如果弹幕没有击中任何东西
            if (Projectile.ai[0] == 0f)
            {
                Projectile.localAI[0] += 1f;
                if (Projectile.localAI[0] > 7f)
                {
                    int water = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, default, 0.4f);
                    Main.dust[water].noGravity = true;
                    Main.dust[water].velocity *= 0.5f;
                    Main.dust[water].velocity += Projectile.velocity * 0.1f;
                }

                float scalar = 0.01f;
                int alphaAmt = 5;
                int alphaCeiling = alphaAmt * 15;
                int alphaFloor = 0;

                if (Projectile.localAI[0] > 7f)
                {
                    if (Projectile.localAI[1] == 0f)
                    {
                        Projectile.scale -= scalar;
                        Projectile.alpha += alphaAmt;
                        if (Projectile.alpha > alphaCeiling)
                        {
                            Projectile.alpha = alphaCeiling;
                            Projectile.localAI[1] = 1f;
                        }
                    }
                    else if (Projectile.localAI[1] == 1f)
                    {
                        Projectile.scale += scalar;
                        Projectile.alpha -= alphaAmt;
                        if (Projectile.alpha <= alphaFloor)
                        {
                            Projectile.alpha = alphaFloor;
                            Projectile.localAI[1] = 0f;
                        }
                    }
                }
            }

            // 弹幕在命中敌人后会开始追踪，并返回攻击相同的敌人
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.alpha += 15;
                Projectile.velocity *= 0.98f;
                Projectile.localAI[0] = 0f;

                if (Projectile.alpha >= 255)
                {
                    // 寻找最近的敌人以追踪
                    int whoAmI = -1;
                    Vector2 targetSpot = Projectile.Center;
                    float detectRange = 700f;
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.CanBeChasedBy(Projectile, false))
                        {
                            float targetDist = Vector2.Distance(npc.Center, Projectile.Center);
                            if (targetDist < detectRange)
                            {
                                detectRange = targetDist;
                                targetSpot = npc.Center;
                                whoAmI = npc.whoAmI;
                            }
                        }
                    }

                    // 如果找到敌人，则追踪返回
                    if (whoAmI >= 0)
                    {
                        Projectile.netUpdate = true;
                        Projectile.ai[0] = 2f; // 标记为第二次攻击
                        Projectile.position = targetSpot + ((float)Main.rand.NextDouble() * 6.28318548f).ToRotationVector2() * 100f - new Vector2(Projectile.width, Projectile.height) / 2f;
                        Projectile.velocity = Vector2.Normalize(targetSpot - Projectile.Center) * 18f; // 加速追踪敌人
                    }
                    else
                    {
                        Projectile.Kill();
                    }
                }
            }
            else if (Projectile.ai[0] == 2f)
            {
                // 第二次攻击逻辑，追踪敌人并继续攻击
                Projectile.scale = 0.9f;
                Projectile.ai[1] += 1f;

                if (Projectile.ai[1] >= 15f)
                {
                    Projectile.alpha += 51;
                    Projectile.velocity *= 0.8f;

                    if (Projectile.alpha >= 255)
                        Projectile.Kill();
                }
                else
                {
                    Projectile.alpha -= 125;
                    if (Projectile.alpha < 0)
                        Projectile.alpha = 0;

                    Projectile.velocity *= 0.98f;
                }

                Projectile.localAI[0] += 1f;

                int water = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, 0f, 0f, 100, default, 0.4f);
                Main.dust[water].noGravity = true;
                Main.dust[water].velocity *= 0.5f;
                Main.dust[water].velocity += Projectile.velocity * 0.1f;
            }

            Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, 0f, 0f, (255 - Projectile.alpha) * 1f / 255f);
        }

        public override Color? GetAlpha(Color lightColor) => new Color(50, 50, 255, Projectile.alpha);

        public override bool PreDraw(ref Color lightColor)
        {
            //if (dontDraw)
            //    return false;
            //Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            //Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), Projectile.scale, SpriteEffects.None, 0);
            //return false;

            // 获取 SpriteBatch 和投射物纹理
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D lightTexture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/ChangedWeapons/CPreMoodLord/TenebreusTidesC/TenebreusTidesJavWaterSword").Value;

            // 遍历投射物的旧位置数组，绘制光学拖尾效果
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 计算颜色插值值，使颜色在旧位置之间平滑过渡
                float colorInterpolation = (float)Math.Cos(Projectile.timeLeft / 32f + Main.GlobalTimeWrappedHourly / 20f + i / (float)Projectile.oldPos.Length * MathHelper.Pi) * 0.5f + 0.5f;

                // 使用蓝色渐变
                Color color = Color.Lerp(Color.DarkBlue, Color.MidnightBlue, colorInterpolation) * 0.4f;
                color.A = 0;

                // 计算绘制位置，将位置调整到碰撞箱的中心
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                // 计算外部和内部的颜色
                Color outerColor = color;
                Color innerColor = color * 0.5f;

                // 计算强度，使拖尾逐渐变弱
                float intensity = 0.9f + 0.15f * (float)Math.Cos(Main.GlobalTimeWrappedHourly % 60f * MathHelper.TwoPi);
                intensity *= MathHelper.Lerp(0.15f, 1f, 1f - i / (float)Projectile.oldPos.Length);
                if (Projectile.timeLeft <= 60)
                {
                    intensity *= Projectile.timeLeft / 60f; // 如果弹幕即将消失，则拖尾也逐渐消失
                }

                // 计算外部和内部的缩放比例，使拖尾具有渐变效果
                Vector2 outerScale = new Vector2(2f) * intensity;
                Vector2 innerScale = new Vector2(2f) * intensity * 0.7f;
                outerColor *= intensity;
                innerColor *= intensity;

                // 绘制外部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, outerColor, Projectile.rotation, lightTexture.Size() * 0.5f, outerScale * 0.6f, SpriteEffects.None, 0);

                // 绘制内部的拖尾效果，并应用旋转
                Main.EntitySpriteDraw(lightTexture, drawPosition, null, innerColor, Projectile.rotation, lightTexture.Size() * 0.5f, innerScale * 0.6f, SpriteEffects.None, 0);
            }

            // 绘制默认的弹幕，并应用旋转
            Main.EntitySpriteDraw(lightTexture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, lightColor, Projectile.rotation, lightTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override bool? CanDamage()
        {
            // Do not do damage if a tile is hit OR if projectile has 'split' and hasn't been live for more than 5 frames
            if (((int)(Projectile.ai[0] - 1f) / penetrationAmt == 0 && penetrationAmt < 3 || Projectile.ai[1] < 5f) && Projectile.ai[0] != 0f)
                return false;
            return true;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 弹幕击中敌人后，穿透一次并获得追踪能力，准备第二次攻击
            if (Projectile.ai[0] == 0f)
            {
                Projectile.ai[0] = 1f; // 标记为第一次命中
            }
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.ai[0] = 2f; // 标记为第二次命中
            }

            Projectile.ai[1] = 0f;
            Projectile.netUpdate = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300); // 深渊水压
        }
    }
}
