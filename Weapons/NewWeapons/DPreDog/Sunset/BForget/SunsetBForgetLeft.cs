using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CalamityMod.Projectiles.Melee;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.BForget
{
    internal class SunsetBForgetLeft : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/Sunset/BForget/SunsetBForgetLeft").Value;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                // 每个拖尾切换颜色，奇数绿色，偶数黄色
                Color color = i % 2 == 0 ? Color.Yellow : Color.LightGreen;
                color *= 0.6f; // 透明度调整
                color.A = 0;

                // 计算绘制位置
                Vector2 drawPosition = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                // 绘制拖尾
                Main.EntitySpriteDraw(texture, drawPosition, null, color, Projectile.rotation, texture.Size() * 0.5f, 0.8f, SpriteEffects.None, 0);
            }

            return false;
        }

        public static class SunsetBForgetParticleManager
        {
            public static readonly int[] YellowDusts = { 169, 159, 133 };
            public static readonly int[] BlueDusts = { 80, 67, 48 };
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为35帧
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 生成绿色重型烟雾粒子
            if (Main.rand.NextFloat() < 0.6f) // 控制生成概率
            {
                Color smokeColor = Main.rand.NextBool() ? Color.Green : Color.LightGreen; // 随机选择绿色或浅绿色

                Particle smokeH = new HeavySmokeParticle(
                    Projectile.Center + new Vector2(0, -10), // 粒子生成位置，略微偏移弹幕中心
                    new Vector2(0, -1) * 5f, // 让烟雾向上飘散
                    smokeColor, // 绿色 or 浅绿色
                    15, // 生命周期缩短至 15 帧
                    Projectile.scale * 0.5f, // 缩放大小减少 0.5 倍
                    0.8f, // 适中的透明度
                    MathHelper.ToRadians(2f), // 旋转速度（更显著）
                    true // ✅ `required = true`，即重型烟雾
                );

                GeneralParticleHandler.SpawnParticle(smokeH);
            }

            // 在弹幕中心生成一串特效（黄色 & 蓝色粒子）
            if (Main.rand.NextBool(2)) // 控制频率
            {
                bool useYellow = Main.rand.NextBool(); // 随机选择黄色或蓝色阵营
                int dustType = useYellow ? Main.rand.Next(SunsetBForgetParticleManager.YellowDusts)
                                         : Main.rand.Next(SunsetBForgetParticleManager.BlueDusts);

                Dust.NewDustPerfect(
                    Projectile.Center,
                    dustType,
                    Vector2.Zero, // 静态粒子
                    100,
                    Color.White,
                    Main.rand.NextFloat(1f, 1.5f) // 轻微缩放
                );
            }
        }



        public override void OnKill(int timeLeft)
        {


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 spawnPosition = target.Center;

            // 计算随机触手数量（3~6个）
            int tentacleCount = Main.rand.Next(3, 7);

            // 计算单个触手的伤害（总伤害固定为 1.0 倍）
            int individualDamage = (int)(Projectile.damage / (float)tentacleCount);

            // 生成多个随机方向的触手
            for (int i = 0; i < tentacleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4); // 随机方向（-45° 到 45°）
                Vector2 tentacleVelocity = Vector2.UnitY.RotatedBy(randomAngle) * -4f; // 向上扩散

                SpawnGreenTentacle(tentacleVelocity, individualDamage);
            }

            // 播放撞击音效
            SoundEngine.PlaySound(SoundID.Item74, Projectile.position);

            // 施加 Debuff 给敌人
            target.AddBuff(ModContent.BuffType<SunsetBForgetEDebuff>(), 300); // 5 秒

            // 施加 Buff 给玩家
            Main.player[Projectile.owner].AddBuff(ModContent.BuffType<SunsetBForgetPBuff>(), 300); // 5 秒
        }

        // 生成绿色触手的方法
        private void SpawnGreenTentacle(Vector2 tentacleVelocity, int damage)
        {
            float kb = Projectile.knockBack;

            float ai0 = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? -1f : 1f);
            float ai1 = Main.rand.NextFloat(0.01f, 0.08f) * (Main.rand.NextBool() ? -1f : 1f);

            if (Projectile.owner == Main.myPlayer)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, tentacleVelocity, ModContent.ProjectileType<SunsetBForgetTantacle>(), damage, kb, Projectile.owner, ai0, ai1);
        }


    }
}