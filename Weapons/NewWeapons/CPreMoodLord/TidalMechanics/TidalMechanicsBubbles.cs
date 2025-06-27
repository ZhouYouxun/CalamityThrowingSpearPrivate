using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewWeapons.CPreMoodLord.TidalMechanics
{
    public class TidalMechanicsBubbles : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.CPreMoodLord";
        private int counter = 0;
        public override bool PreDraw(ref Color lightColor) // 确保贴图的中心点为绘制的中心点
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.timeLeft = 1800; // 30秒
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.damage = 0; // 无伤害
            Projectile.scale = 2.75f; // 更大的大小
        }

        private int blockedProjectiles = 0; // 记录已阻挡的弹幕数量
        private bool isInUnlimitedBlockPhase = false; // 标记是否进入无限阻挡阶段
        private int unlimitedBlockStartTime; // 记录进入无限阻挡阶段的时间

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center; // 跟随玩家

            // 检查是否启用 ZenithWorld
            if (Main.zenithWorld)
            {
                Projectile.timeLeft = 36000; // 每帧重置弹幕时间，确保不会消失

                // 无限阻挡敌人弹幕
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile otherProj = Main.projectile[i];
                    if (otherProj.active && otherProj.hostile && otherProj.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        // 消除敌人弹幕
                        otherProj.Kill();

                        // 生成橙色粒子特效
                        for (int j = 0; j < 10; j++)
                        {
                            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WaterCandle, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default(Color), 1.9f);
                        }
                    }
                }

                // 每帧生成水能粒子特效
                CreateWaterEnergyParticles();

                return; // 结束逻辑，跳过普通模式
            }

            // 普通模式逻辑
            if (!isInUnlimitedBlockPhase)
            {
                // 检测与敌人弹幕的接触，并产生粒子特效
                for (int i = 0; i < Main.maxProjectiles && blockedProjectiles < 7; i++)
                {
                    Projectile otherProj = Main.projectile[i];
                    if (otherProj.active && otherProj.hostile && otherProj.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        // 消除敌人弹幕
                        otherProj.Kill();
                        blockedProjectiles++;

                        // 生成橙色粒子特效
                        for (int j = 0; j < 10; j++)
                        {
                            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WaterCandle, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default(Color), 1.9f);
                        }
                    }
                }

                // 如果阻挡了 x 个弹幕，则进入无限阻挡阶段
                if (blockedProjectiles >= 2)
                {
                    isInUnlimitedBlockPhase = true;
                    unlimitedBlockStartTime = (int)Main.GameUpdateCount; // 记录当前时间
                }
            }
            else
            {
                // 无限阻挡阶段，不限制阻挡数量
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile otherProj = Main.projectile[i];
                    if (otherProj.active && otherProj.hostile && otherProj.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        // 消除敌人弹幕
                        otherProj.Kill();
                        blockedProjectiles++;

                        // 生成橙色粒子特效
                        for (int j = 0; j < 10; j++)
                        {
                            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WaterCandle, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default(Color), 1.9f);
                        }
                    }
                }

                // 每帧生成水能粒子特效
                CreateWaterEnergyParticles();

                // 检查是否已超过2秒（120帧）
                if (Main.GameUpdateCount - unlimitedBlockStartTime >= 120)
                {
                    Projectile.Kill(); // 2秒后消失
                }
            }
        }

        private void CreateWaterEnergyParticles()
        {
            // 生成多个水能粒子特效
            for (int i = 0; i < 5; i++)
            {
                // 在弹幕的任意位置生成粒子
                Vector2 particlePosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2), Main.rand.NextFloat(-Projectile.height / 2, Projectile.height / 2));
                Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(1f, 2.5f); // 随机方向和速度

                // 创建深蓝色水能粒子
                Particle waterParticle = new HeavySmokeParticle(particlePosition, velocity, Color.DarkBlue, 15, 0.9f, 0.5f, 0.2f, true);
                GeneralParticleHandler.SpawnParticle(waterParticle);
            }
        }


        public override void OnKill(int timeLeft)
        {
            CreateBurstEffect();

            SoundEngine.PlaySound(new SoundStyle("CalamityThrowingSpear/Sound/潮汐突然爆"));

            // 获取玩家对象
            Player owner = Main.player[Projectile.owner];

            // 设置无敌时间
            owner.immune = true; // 激活无敌状态
            owner.immuneNoBlink = true; // 取消无敌状态闪烁
            owner.immuneTime = 60; // 设置无敌时间为60帧
        }

        private void CreateBurstEffect()
        {
            int points = 90;
            float radians = MathHelper.TwoPi / points;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            float rotRando = Main.rand.NextFloat(0.1f, 2.5f);
            for (int k = 0; k < points; k++)
            {
                Vector2 velocity = spinningPoint.RotatedBy(radians * k).RotatedBy(-0.45f * rotRando);
                LineParticle subTrail = new LineParticle(Projectile.Center + velocity * 20.5f, velocity * 15, false, 30, 1.75f, Color.Blue);
                GeneralParticleHandler.SpawnParticle(subTrail);
            }
        }

        //public override bool OnTileCollide(Vector2 oldVelocity)
        //{
        //    counter++;
        //    if (counter >= 5)
        //    {
        //        Projectile.Kill();
        //    }
        //    return false;
        //}

    }
}
