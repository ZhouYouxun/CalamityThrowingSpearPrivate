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
using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Sounds;
using Terraria.Audio;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Buffs.DamageOverTime;

namespace CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC
{
    public class DragonRageJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.ChangedWeapons.EAfterDog";
        private int hitCounter = 0;
        public int Time = 0;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            // 确保以贴图中心为旋转中心
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);

            // 计算绘制位置，考虑gfxOffY偏移
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            // 进行绘制，使用正确的旋转中心
            spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false; // 返回false，防止游戏默认的绘制
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 420;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 9;
        }

        public override void AI()
        {
            // 保持飞行方向不变
            Projectile.velocity *= 1.006f;

            // 添加橙色光效
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3() * 0.55f);

            // 添加高速旋转效果
            Projectile.rotation += 0.45f; // 你可以根据需求调整旋转速度，增加或减少该值

            // 粒子效果随机化释放
            if (Time % 3 == 0)
            {
                Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                particleOffset.X += Main.rand.NextFloat(-3f, 3f); // 随机左右偏移
                Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;

                // 应用 2.1 倍缩放到光环特效
                float scaleMultiplier = 2.1f;
                Particle Smear = new CircularSmearVFX(
                    particlePosition,
                    Color.OrangeRed * Main.rand.NextFloat(0.78f, 0.85f),
                    Main.rand.NextFloat(-8, 8),
                    Main.rand.NextFloat(1.2f, 1.3f) * scaleMultiplier // 应用缩放
                );
                GeneralParticleHandler.SpawnParticle(Smear);
            }


            Time++;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 龙焰

            hitCounter++;

            // 5%概率释放SparkInfernal（地狱龙卷）弹幕，但如果存在指定的弹幕则不触发
            //int numWandBolts = Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<DRSparkInfernal>()];
            //int numTornadoStarters = Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<InfernadoMarkFriendly>()];
            //int numTornadoPieces = Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<InfernadoFriendly>()];

            //if (numWandBolts + numTornadoStarters + numTornadoPieces < 1 && Main.rand.NextFloat() <= 0.05f)
            //{
                //Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<DRSparkInfernal>(), (int)(Projectile.damage * 0.95f), Projectile.knockBack, Projectile.owner);
            //}

            // 每击中5次敌人，召唤4个火球，火球从玩家屏幕边缘的随机位置召唤
            //if (hitCounter >= 5)
            //{
            //    hitCounter = 0;
            //    for (int i = 0; i < 4; i++)
            //    {
            //        // 从屏幕边缘随机生成位置
            //        Vector2 spawnPosition = Main.rand.NextVector2FromRectangle(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight));
            //        Vector2 fireballDirection = (Main.player[Projectile.owner].Center - spawnPosition).SafeNormalize(Vector2.UnitX) * 10f;

            //        // 召唤火球弹幕
            //        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, fireballDirection, ModContent.ProjectileType<DragonRageFireball>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
            //    }
            //}

            // 在原地释放FuckYou弹幕，伤害倍率为100%
            //Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<DragonRageFuckYou>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner);

            // 生成2到4个橙红色的斩击特效，并随机生成方向
            int slashCount = Main.rand.Next(4, 5); // 生成2到4个斩击特效
            for (int i = 0; i < slashCount; i++)
            {
                // 随机生成方向
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                int slashID = ModContent.ProjectileType<OrangeSLASH>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, randomDirection, slashID,(int)( Projectile.damage * 0.25f), Projectile.knockBack, Projectile.owner);
            }
            SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.5f }, Projectile.Center);

        }

        public override void OnKill(int timeLeft)
        {
            // 发射 14 个 DragonRageFireball 弹幕
            int fireballCount = 12;

            for (int i = 0; i < fireballCount; i++)
            {
                // 生成一个随机的角度（0 到 360 度之间）
                float randomAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);

                // 计算每个弹幕的方向向量
                Vector2 direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle));

                // 设定弹幕的速度和伤害
                float fireballSpeed = 10f; // 可以调整此值设置弹幕的速度
                Vector2 fireballVelocity = direction * fireballSpeed;

                // 生成 DragonRageFireball 弹幕，伤害倍率设为 0.1
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    fireballVelocity,
                    ModContent.ProjectileType<DragonRageFireball>(),
                    (int)(Projectile.damage * 0.2f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            // 播放销毁音效或其他效果（可选）
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }




    }
}
