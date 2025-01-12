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
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.Sounds;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using Terraria.Audio;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    internal class FinishingTouchPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FinishingTouch";
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
            Main.projFrames[Projectile.type] = 4; // 设置投射物的帧数为 4
        }

        public override bool PreDraw(ref Color lightColor) // 确保贴图的中心点为绘制的中心点
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // 计算当前动画帧
            int frameCount = 4; // 总共 4 帧
            int frameHeight = texture.Height / frameCount; // 每帧的高度
            int currentFrame = (int)(Main.GameUpdateCount / 6 % frameCount); // 每 6 帧切换一次，总共 4 帧
            Rectangle sourceRectangle = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);

            // 设置绘制的原点和位置
            Vector2 drawOrigin = new Vector2(texture.Width / 2, frameHeight / 2); // 每帧的高度作为原点
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // 绘制当前帧
            spriteBatch.Draw(texture, drawPosition, sourceRectangle, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 允许6次伤害
            Projectile.timeLeft = 60;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 1; // 无敌帧冷却时间为1帧
        }

        public override void AI()
        {
            // 每 6 帧切换一次帧
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0; // 重置帧计数器
                Projectile.frame++; // 切换到下一帧
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0; // 如果超过了最大帧数，回到第一帧
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);
            Projectile.velocity *= 1.001f;

            // 刚出现时的初始粒子特效
            if (Projectile.timeLeft == 180) // Assuming timeLeft is initially 180
            {
                GenerateInitialParticles();
            }

            if (Projectile.ai[0] % 60 == 0)
            {
                //// 特效尾迹
                //Vector2 trailPos = Projectile.Center;
                //float trailScale = Main.rand.NextFloat(0.8f, 1.2f);
                //Color trailColor = Color.Orange; // 橙色特效
                //Particle trail = new SparkParticle(trailPos, Projectile.velocity * 0.2f, false, 60, trailScale, trailColor);
                //GeneralParticleHandler.SpawnParticle(trail);
            }

            // 定义一个偏移距离，用来增加粒子之间的间隔
            float offsetDistance = 20f;

            // 计算特效生成位置，始终在弹幕的正左方和正右方（基于弹幕当前方向）
            Vector2 leftOffset = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * offsetDistance;
            Vector2 rightOffset = Projectile.velocity.RotatedBy(-MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * offsetDistance;

            Vector2 leftTrailPos = Projectile.Center + leftOffset;
            Vector2 rightTrailPos = Projectile.Center + rightOffset;

            // 生成橙红色粒子特效
            Color orangeRed = Color.OrangeRed;
            Particle leftTrail = new SparkParticle(leftTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, orangeRed);
            Particle rightTrail = new SparkParticle(rightTrailPos, Projectile.velocity * 0.2f, false, 60, 1f, orangeRed);
            GeneralParticleHandler.SpawnParticle(leftTrail);
            GeneralParticleHandler.SpawnParticle(rightTrail);




            // 生成左右漂移的轻型白色烟雾特效
            int dustCount = 1; // 每次生成的烟雾数量
            float radians = MathHelper.TwoPi / dustCount;
            Vector2 smokePoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustVelocity = smokePoint.RotatedBy(radians * i).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2.6f);
                Color smokeColor = Color.White;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.Orange, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }



            //// 每隔 60 帧生成一次火球和粒子特效
            //Projectile.ai[0]++;
            //if (Projectile.ai[0] >= 60)
            //{
            //    ReleaseFireballs();
            //    ReleaseLinearParticles();
            //    Projectile.ai[0] = 0; // 重置计数
            //}
        }


        private void GenerateInitialParticles()
        {
            for (float angle = -15f; angle <= 15f; angle += 1f)
            {
                Vector2 particleDirectionLeft = Projectile.velocity.RotatedBy(MathHelper.ToRadians(angle));
                Vector2 particleDirectionRight = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-angle));

                // 左右方向各释放粒子
                Particle particleLeft = new SparkParticle(Projectile.Center, particleDirectionLeft * 3f, false, 40, 1.5f, Color.OrangeRed);
                Particle particleRight = new SparkParticle(Projectile.Center, particleDirectionRight * 3f, false, 40, 1.5f, Color.OrangeRed);

                GeneralParticleHandler.SpawnParticle(particleLeft);
                GeneralParticleHandler.SpawnParticle(particleRight);
            }
        }

        private void ReleaseFireballs()
        {
            int fireballType = ModContent.ProjectileType<FinishingTouchBALL>();
            float baseAngle = MathHelper.TwoPi / 16; // 每个火球的角度

            for (int i = 0; i < 16; i++)
            {
                float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);

                // 计算每个弹幕的方向向量
                Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

                // 设定弹幕的速度和伤害
                Vector2 fireballVelocity = baseAngle.ToRotationVector2().RotatedBy(baseAngle * i) * 10f; // 初始速度为原来的8.5倍Main.rand.NextFloat(0.75f, 2f)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireballVelocity, fireballType, (int)(Projectile.damage * 0.25f), Projectile.knockBack, Projectile.owner);
            }
        }


        private void ReleaseLinearParticles()
        {
            float baseAngle = MathHelper.TwoPi / 24; // 20个粒子的扩散角度

            for (int i = 0; i < 24; i++)
            {
                Vector2 trailPos = Projectile.Center;
                Vector2 trailVelocity = baseAngle.ToRotationVector2().RotatedBy(baseAngle * i) * 0.2f;
                Color trailColor = Color.OrangeRed;
                float trailScale = 1.5f;

                Particle trail = new SparkParticle(trailPos, trailVelocity, false, 60, trailScale, trailColor);
                GeneralParticleHandler.SpawnParticle(trail);
            }
        }

      


        public override void OnKill(int timeLeft)
        {
            ReleaseFireballs();
            ReleaseLinearParticles();
            // 释放爆炸弹幕
            //int explosionType = ModContent.ProjectileType<FinishingTouchDASHFuckYou>();
            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, explosionType, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 龙焰

            //// 检查是否击中了指定 Boss，并进行伤害加成
            //if (target.type == ModContent.NPCType<Bumblefuck>() || target.type == ModContent.NPCType<Bumblefuck2>())
            //{
            //    hit.Damage = (int)(hit.Damage * 50);
            //}
            int slashCount = 2; // 生成2到3个斩击特效
            for (int i = 0; i < slashCount; i++)
            {
                // 随机生成方向
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                int slashID = ModContent.ProjectileType<OrangeSLASH>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, randomDirection, slashID, (int)(Projectile.damage * 2.5f), Projectile.knockBack, Projectile.owner);
            }

            // 给予5秒钟的创造胜利
            int buffDuration = 5 * 60; // 5 秒钟，单位为帧（每秒 60 帧）
            target.AddBuff(ModContent.BuffType<CreateVictoryPEBuff>(), buffDuration);


            SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.5f }, Projectile.Center);
        }

    }
}

