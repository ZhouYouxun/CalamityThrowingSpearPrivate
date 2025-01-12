using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.TheLastLance
{
    public class TheLastLanceDASH : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/TheLastLance/TheLastLancePROJ";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        private const int MaxChargeTime = 30; // 蓉力时间30帧
        private const float DashSpeed = 32.5f * 0.75f; // 冲刺速度为默认速度的0.75倍
        private const int MaxDashTime = 30; // 冲刺时间30帧（0.5秒）

        private Vector2 lockedDirection; // 存储锁定的冲刺方向
        private int dashTime = 0; // 冲刺已进行的时间
        private bool isDashing = false; // 标记是否处于冲刺状态
        private int Time = 0; // 用于控制粒子效果的计时器
        private bool isSuperDash = false; // 标记是否为超级冲刺
        public void SetSuperDash()
        {
            isSuperDash = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.netImportant = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
            if (isSuperDash)
            {
                Projectile.timeLeft = 60000; // 如果是超级冲刺状态，设置为6万
                //Projectile.damage *= 6; // 超级冲刺形态造成6倍伤害
            }
            else
            {
                Projectile.timeLeft = MaxChargeTime + 30; // 维持原本的时间
                //Projectile.damage *= 2; // 正常冲刺形态仅造成2倍伤害
            }
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            //Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Player owner = Main.player[Projectile.owner];
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            //// 强制保持高速旋转，模拟孙悟空耍金箍棒的操作
            //Projectile.rotation += 0.45f; // 高速旋转

            if (Projectile.velocity == Vector2.Zero) // 蓄力阶段
            {
                Projectile.tileCollide = false;
                Projectile.friendly = false;
                isDashing = false;

                // 强制保持高速旋转，模拟孙悟空耍金箍棒的操作
                Projectile.rotation += 0.45f; // 高速旋转
                //Projectile.rotation = Projectile.AngleTo(Main.MouseWorld);
                //if (Projectile.spriteDirection == -1)
                //    Projectile.rotation += MathHelper.PiOver2;
                //else
                //    Projectile.rotation += MathHelper.PiOver4;


                Projectile.Center = owner.MountedCenter;
                owner.heldProj = Projectile.whoAmI;

                if (Time % 3 == 0)
                {
                    Vector2 particleOffset = new Vector2(13.5f * Projectile.direction, 0);
                    particleOffset.X += Main.rand.NextFloat(-3f, 3f); // 随机左右偏移
                    Vector2 particlePosition = Projectile.Center + particleOffset + Projectile.velocity * 0.5f;
                    Particle Smear = new CircularSmearVFX(particlePosition, Color.CadetBlue * Main.rand.NextFloat(0.9f, 1.0f), Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(1.2f, 1.3f));
                    GeneralParticleHandler.SpawnParticle(Smear);
                }

                Time++;

                if (Projectile.ai[0] >= MaxChargeTime)
                {
                    StartLunge(owner); // 开始冲刺
                }
                else
                {
                    Projectile.ai[0]++;
                }
            }
            else // 冲刺阶段
            {
                // 在冲刺阶段将长枪的旋转方向固定为开始冲刺的方向
                //Projectile.rotation = lockedDirection.ToRotation() + MathHelper.PiOver4;


                // 不断的更新弹幕的旋转方向，让它永远对准鼠标还是调整
                Vector2 currentDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.rotation = currentDirection.ToRotation() + MathHelper.PiOver4;
                // 更新弹幕的 velocity 以使其持续朝向鼠标
                Projectile.velocity = currentDirection * Projectile.velocity.Length();


                if (isSuperDash)
                {
                    Projectile.tileCollide = false; // 超级冲刺状态下会穿墙
                }
                else
                {
                    Projectile.tileCollide = true; // 普通冲刺不会穿墙
                }

                Projectile.friendly = true;
                isDashing = true;

                //Projectile.velocity = lockedDirection * Projectile.velocity.Length();
                owner.velocity = Projectile.velocity;
                owner.Center = Projectile.Center;

                dashTime++;
                if (!isSuperDash && dashTime >= MaxDashTime) // 仅在不是超级冲刺时才执行减速和停止逻辑
                {
                    Projectile.velocity *= 0.35f;
                    return;
                }

                // 在枪头位置生成深蓝色烟雾粒子特效
                AddSmokeParticles();

                // 每帧释放深蓝色气泡特效
                if (Projectile.ai[0] % 3 == 0)
                {
                    ReleaseBubbles();
                }

                // 在身体上留下蓝色重型烟雾粒子特效，代表海洋的力量
                Color smokeColor = Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f); // 使用深蓝色和浅蓝色渐变
                Particle smoke = new HeavySmokeParticle(Projectile.Center, Projectile.velocity * 0.5f, smokeColor, 30, Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f), 1.0f, MathHelper.ToRadians(2f), required: true);
                GeneralParticleHandler.SpawnParticle(smoke);

                // 如果是超级冲刺且接触到液体，则销毁投射物
                if (isSuperDash && Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height))
                {
                    Projectile.Kill();
                }
            }
        }

        private void StartLunge(Player owner) // 冲刺的具体逻辑
        {
            owner = Main.player[Projectile.owner];

            dashTime = 0;
            lockedDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.velocity = lockedDirection * DashSpeed;

            // 如果是超级冲刺，设置冲刺时间和生存时间无限制
            if (isSuperDash)
            {
                dashTime = 0;
                Projectile.timeLeft = MaxChargeTime + 60000;
                Projectile.tileCollide = false;
            }

            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);
            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.DarkBlue, new Vector2(1f, 1f), Main.rand.NextFloat(6f, 10f), 0.15f, 3f, 10);
            GeneralParticleHandler.SpawnParticle(pulse);

            owner.immune = true;
            owner.immuneNoBlink = true;
            owner.immuneTime = 30;

            for (int i = 0; i < owner.hurtCooldowns.Length; i++)
            {
                owner.hurtCooldowns[i] = owner.immuneTime;
            }

            Projectile.netUpdate = true;
        }

        private void AddSmokeParticles() // 在枪头位置生成深蓝色烟雾粒子特效
        {
            Vector2 offset = new Vector2(0, -Projectile.width * 0.5f).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2);
            Vector2 spawnPosition = Projectile.Center + offset;
            Color smokeColor = Color.DarkBlue; // 深蓝色
            Particle smoke = new HeavySmokeParticle(spawnPosition, Vector2.Zero, smokeColor, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
            GeneralParticleHandler.SpawnParticle(smoke);
        }

        private void ReleaseBubbles() // 每帧释放深蓝色气泡特效
        {
            Player player = Main.player[Projectile.owner]; // 获取玩家对象
            Vector2 playerPosition = player.Center; // 获取玩家的中心位置

            // 将位置参数修改为玩家的位置
            Gore bubble = Gore.NewGorePerfect(Projectile.GetSource_FromAI(), playerPosition, Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 411);
            bubble.timeLeft = 8 + Main.rand.Next(6);
            bubble.scale = Main.rand.NextFloat(0.6f, 1f) * (1 + Projectile.timeLeft / (float)Projectile.timeLeft);
            bubble.type = Main.rand.NextBool(3) ? 412 : 411;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int freezeDuration = 180; // 冻结持续时间，单位为帧
            target.AddBuff(ModContent.BuffType<GlacialState>(), freezeDuration); // 冰河时代
            target.AddBuff(BuffID.Frostburn, freezeDuration); // 原版的霜火效果
            target.AddBuff(BuffID.Chilled, freezeDuration); // 原版的寒冷效果

            // 检查玩家是否处于海洋群系，以便造成额外的两倍伤害
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.ZoneBeach)
            {
                Projectile.damage = (int)(Projectile.damage * 2.0f); // 造成两倍伤害
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor) // 确保贴图的中心点为绘制的中心点
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

            spriteBatch.Draw(texture, drawPosition, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
