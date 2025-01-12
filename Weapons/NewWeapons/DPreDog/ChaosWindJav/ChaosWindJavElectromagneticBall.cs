using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Projectiles.Ranged;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.ChaosWindJav
{
    public class ChaosWindJavElectromagneticBall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override string Texture => "Terraria/Images/Projectile_465"; // 使用原版的 Projectile_465 贴图（教徒闪电球）

        public int time = 0;
        public bool doDamage = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void AI()
        {
            //if (time == 0)
            //{
            //    Projectile.scale = Main.rand.NextFloat(0.35f, 0.55f);
            //}

            if (time == 0)
            {
                Projectile.scale = Projectile.ai[0] == 1 ? 3f : 1f; // 这里调整的是本体电磁球的大小
            }

            if (Projectile.ai[0] == 1)
            {
                //// 强逻辑时的屏幕震动效果
                //float shakePower = 5f;
                //float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                //Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
            }

            time++;

            // 每隔一定时间生成粒子效果
            if (time >= 25 && time % 2 == 0 && Projectile.timeLeft > 20)
            {
                Particle bolt = new CrackParticle(Projectile.Center, new Vector2(8, 8).RotatedByRandom(100), Color.Aqua * 0.65f, Vector2.One, 0, 0, Main.rand.NextFloat(0.4f, 0.65f), 11);
                GeneralParticleHandler.SpawnParticle(bolt);
            }
            // 亮黄色冲击波效果
            if (Projectile.timeLeft % 30 == 0)
            {
                if (Projectile.ai[0] == 1) // 强逻辑
                {
                    // 将 OriginalScale 调整为原来的三倍，FinalScale 保持不变
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.BlueViolet, new Vector2(1.5f), Projectile.rotation, 4.5f, 1.1f, 30);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
                else
                {
                    // 更弱的视觉效果
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.BlueViolet, new Vector2(1.5f), Projectile.rotation, 2.5f, 0.1f, 30);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }

                //// 随机释放一道没有伤害的视觉效果闪电
                //{
                //    // 生成一个随机角度，范围在0到360度
                //    float randomAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                //    Vector2 velocity = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)) * 10f; // 控制速度大小，可以根据需要调整

                //    // 创建闪电弹幕
                //    int lightningProjectile = Projectile.NewProjectile(
                //        Projectile.GetSource_FromThis(),
                //        Projectile.Center,
                //        velocity,
                //        ProjectileID.CultistBossLightningOrbArc, // 使用的弹幕类型
                //        0, // 设置伤害为0
                //        0f,
                //        Projectile.owner
                //    );

                //    // 设置闪电属性
                //    Projectile proj = Main.projectile[lightningProjectile];
                //    proj.friendly = true;
                //    proj.hostile = false;
                //    proj.penetrate = -1;
                //    proj.localNPCHitCooldown = 60;
                //    proj.usesLocalNPCImmunity = true;
                //}
                
            }
            // 控制闪电球动画
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3) 
                {
                    Projectile.frame = 0;
                }
            }

            time++;
        }

        public override void OnKill(int timeLeft)
        {
            //Projectile.netUpdate = true;
            //doDamage = true;


         
            if (Projectile.ai[0] == 1) // 强逻辑
            {
                // 强逻辑时的屏幕震动效果
                float shakePower = 5f;
                float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

                //Projectile.ExpandHitboxBy(4000);
                //Projectile.Damage();

                // 生成强化的 ChaosWindJavAirburst
                for (int i = 0; i < 7; i++)
                {
                    Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ChaosWindJavAirburst>(), (int)(Projectile.damage * 0.5), Projectile.knockBack, Projectile.owner);
                    if (explosion.whoAmI.WithinBounds(Main.maxProjectiles))
                    {
                        explosion.ai[1] = Main.rand.NextFloat(256f, 696f) + i * 45f; // Randomize the maximum radius.
                        explosion.localAI[1] = Main.rand.NextFloat(0.08f, 0.25f); // And the interpolation step.
                        explosion.Opacity = MathHelper.Lerp(0.18f, 0.6f, i / 7f) + Main.rand.NextFloat(-0.08f, 0.08f);
                        explosion.netUpdate = true;
                    }
                }
            }
            else // 弱逻辑
            {
                //Projectile.ExpandHitboxBy(400);
                //Projectile.Damage();

                // 生成弱化的 ChaosWindJavAirburst
                for (int i = 0; i < 2; i++)
                {
                    Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ChaosWindJavAirburst>(), (int)(Projectile.damage * 0.05), 0f, Projectile.owner);
                    explosion.ai[1] = Main.rand.NextFloat(64f, 174f) + i * 20f; // Randomize the maximum radius.
                    explosion.localAI[1] = Main.rand.NextFloat(0.18f, 0.3f); // And the interpolation step.
                    explosion.netUpdate = true;
                }
            }
            // 生成特效
            int effectCount = Projectile.ai[0] == 1 ? 100 : 40;
            for (int k = 0; k < effectCount; k++)
            {
                Dust dust2 = Dust.NewDustPerfect(Projectile.Center, 226, new Vector2(25, 25).RotatedByRandom(100) * Main.rand.NextFloat(0.05f, 0.8f));
                dust2.scale = Projectile.ai[0] == 1 ? Main.rand.NextFloat(1.2f, 2.0f) : Main.rand.NextFloat(0.65f, 1.15f);
                dust2.noGravity = true;
            }

            SoundStyle fire = new("CalamityMod/Sounds/Item/AuricBulletHit");
            SoundEngine.PlaySound(fire with { Volume = Projectile.ai[0] == 1 ? 1.0f : 0.4f, Pitch = 0f }, Projectile.Center);

            Particle bolt = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Aqua, "CalamityMod/Particles/HighResFoggyCircleHardEdge", Vector2.One * (Projectile.ai[0] == 1 ? 1.5f : 1f), Main.rand.NextFloat(-10f, 10f), 0.03f, 0.16f, 16);
            GeneralParticleHandler.SpawnParticle(bolt);

            // 生成闪电
            int lightningCount = Projectile.ai[0] == 1 ? 9 : 2;
            SpawnLightningBolts(lightningCount);
        }

        private void SpawnLightningBolts(int count)
        {
            for (int i = 0; i < count; i++)
            {
                // 闪电生成位置：电磁球上方 50 个方块处，以该点为圆心，半径 7 个方块的随机位置
                Vector2 spawnPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-7f, 7f) * 16f, -50f * 16f);
                Vector2 velocity = Vector2.UnitY * 7f; // 与 ChaosWindJavLightningCloud 中的速度相同

                // 生成弹幕
                int lightningProjectile = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, ProjectileID.CultistBossLightningOrbArc, (int)((int)(Projectile.damage)*2.1), 0f, Projectile.owner, MathHelper.PiOver2, Main.rand.Next(100));

                // 设置属性
                Projectile proj = Main.projectile[lightningProjectile];
                proj.friendly = true;
                proj.hostile = false;
                proj.penetrate = -1;
                proj.localNPCHitCooldown = 60;
                proj.usesLocalNPCImmunity = true;
            }
        }


        //public override bool? CanDamage() => true;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int yPos = frameHeight * Projectile.frame;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle(0, yPos, texture.Width, frameHeight), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Width / 2f, frameHeight / 2f), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
