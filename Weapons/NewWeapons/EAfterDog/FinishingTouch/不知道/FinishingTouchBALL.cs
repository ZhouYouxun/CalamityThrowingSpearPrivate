//using CalamityMod.Buffs.DamageOverTime;
//using CalamityMod.Events;
//using CalamityMod.World;
//using CalamityMod;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Terraria.Audio;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Terraria;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
//{
//    public class FinishingTouchBALL : ModProjectile
//    {
//        private bool hasDashed = false; // 是否已冲刺
//        private int dashStartDelay = 120; // 开始冲刺的延迟
//        private NPC targetNPC; // 追踪的目标敌人
//        public override void SetStaticDefaults()
//        {
//            Main.projFrames[Projectile.type] = 5;
//        }

//        public override void SetDefaults()
//        {
//            Projectile.width = 64;
//            Projectile.height = 66;
//            Projectile.hostile = false;
//            Projectile.friendly = true;
//            Projectile.scale = 1.5f;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = false;
//            Projectile.penetrate = 1;
//            Projectile.alpha = 50;
//            Projectile.timeLeft = 420;
//            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
//            Projectile.localNPCHitCooldown = 30; // 无敌帧冷却时间为30帧
//        }

//        public override void AI()
//        {
//            Projectile.frameCounter++;
//            if (Projectile.frameCounter > 4)
//            {
//                Projectile.frame++;
//                Projectile.frameCounter = 0;
//            }
//            if (Projectile.frame >= Main.projFrames[Projectile.type])
//                Projectile.frame = 0;

//            Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0f);

//            // 缓慢飞行并逐渐减速
//            if (!hasDashed)
//            {
//                Projectile.velocity *= 0.98f;
//            }

//            // 生成不受重力影响的火焰粒子，分布在弹幕体积内
//            if (Main.rand.NextBool(3))
//            {
//                Vector2 randomOffset = new Vector2(Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2), Main.rand.NextFloat(-Projectile.height / 2, Projectile.height / 2));
//                Dust dust = Dust.NewDustPerfect(Projectile.Center + randomOffset, DustID.OrangeTorch, Projectile.velocity * 0.3f, 0, Color.OrangeRed, 1.2f);
//                dust.noGravity = true;
//            }


//            // 计数飞行的帧数
//            Projectile.ai[0]++;

//            // 在飞行第60帧时寻找并锁定敌人
//            if (Projectile.ai[0] >= dashStartDelay && !hasDashed)
//            {
//                targetNPC = FindTarget(1000f); // 寻找最近的敌人
//                if (targetNPC != null)
//                {
//                    hasDashed = true;
//                    //// 冲刺粒子效果
//                    //for (int i = 0; i < 30; i++)
//                    //{
//                    //    Vector2 offset = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
//                    //    Dust dust = Dust.NewDustPerfect(offset, DustID.Lava, Vector2.Zero, 0, Color.OrangeRed, Main.rand.NextFloat(1f, 1.5f));
//                    //    dust.noGravity = true;
//                    //}
//                }
//            }

//            // 持续追踪目标
//            if (hasDashed && targetNPC != null && targetNPC.active)
//            {
//                Vector2 direction = Vector2.Normalize(targetNPC.Center - Projectile.Center);
//                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 20f, 0.1f); // 持续追踪
//            }
//        }

//        // 寻找最近的敌人
//        private NPC FindTarget(float range)
//        {
//            NPC closestNPC = null;
//            float closestDistance = range;

//            foreach (NPC npc in Main.npc)
//            {
//                if (npc.CanBeChasedBy(Projectile) && Projectile.Distance(npc.Center) < closestDistance)
//                {
//                    closestDistance = Projectile.Distance(npc.Center);
//                    closestNPC = npc;
//                }
//            }

//            return closestNPC;
//        }

//        public override void OnKill(int timeLeft)
//        {
//            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

//            // 生成受重力影响的火焰粒子效果
//            for (int i = 0; i < 15; i++)
//            {
//                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Lava, Main.rand.NextVector2Circular(3f, 3f), 0, Color.OrangeRed, 1.5f);
//                dust.noGravity = false; // 使粒子受重力影响
//            }

//            // 生成抛射的橙红色粒子特效
//            for (int i = 0; i < 10; i++)
//            {
//                Vector2 velocity = Main.rand.NextVector2CircularEdge(4f, 4f); // 设置粒子向周围随机方向抛射
//                Dust particle = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, velocity, 0, Color.OrangeRed, 1.2f);
//                particle.noGravity = true; // 让粒子不受重力影响
//                particle.fadeIn = 1.5f; // 使粒子效果更明显
//            }
//        }

//        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 16f * Projectile.scale, targetHitbox);

//        public override bool PreDraw(ref Color lightColor)
//        {
//            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
//            int framing = texture.Height / Main.projFrames[Projectile.type];
//            int y6 = framing * Projectile.frame;
//            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Rectangle(0, y6, texture.Width, framing), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Width / 2f, framing / 2f), Projectile.scale, SpriteEffects.None, 0);
//            return false;
//        }
//    }
//}
