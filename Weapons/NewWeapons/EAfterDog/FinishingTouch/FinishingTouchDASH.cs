using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.Audio;
using CalamityMod.Particles;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Items.Weapons.Ranged;
using Terraria.ID;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Sounds;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using Terraria.DataStructures;
using Terraria.GameContent;
using CalamityMod.NPCs.Yharon;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.Buffs.DamageOverTime;
using CalamityThrowingSpear.Global;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch.FTDragon;
using Terraria.Graphics.Renderers;
using CalamityMod.Prefixes;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.FinishingTouch
{
    public class FinishingTouchDASH : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/FinishingTouch";

        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        private const int MaxChargeTime = 60;
        private const int releaseFireballInterval = 60;

        private const float DASHSpeed = 50f;

        public Vector2 IdealVelocity;

        private int fireballTimer = 0;

        private int fireballReleaseCount = 0; // 火球释放计数器

        private Vector2 lockedDirection; // 添加用于存储锁定方向的变量


        private const int MaxDashTime = 60; // 一个冲刺的最大时间
        private int dashTime = 0; // 记录冲刺已经进行的时间

        private bool isDashing = false; // 用于手动控制按下跳跃键是否被解除


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxChargeTime + 60;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 60; // 无敌帧冷却时间为60帧
        }
        private bool hasSaidPhrase = false; // 添加一个标记，确保只触发一次
        private float pentagonLightRotation = 0f; // 五边形光点旋转角度


        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 获取生成位置和速度（固定向上发射）
            Vector2 spawnPosition = Projectile.Center;
            Vector2 spawnVelocity = Vector2.UnitY * -1f * 25f;

            // 生成巨龙
            int proj = Projectile.NewProjectile(
                source,
                spawnPosition,
                spawnVelocity,
                ModContent.ProjectileType<FinishingTouchDragon>(),
                (int)(Projectile.damage * 5.0),
                Projectile.knockBack,
                Projectile.owner
            );

            // 保险设置为启用 B 方案（围绕玩家模式）
            if (proj.WithinBounds(Main.maxProjectiles))
            {
                (Main.projectile[proj].ModProjectile as FinishingTouchDragon)?.SetBPlan(true);
            }
        }
        private bool ShouldSpawnPulse(float chargeTime)
        {
            // 在蓄力 0~60 帧内：
            // 开始时每 8 帧生成一次，后期每 3 帧生成一次
            // 根据时间线性插值帧间隔
            float progress = chargeTime / 60f;
            float frameInterval = MathHelper.Lerp(8f, 3f, progress);

            return chargeTime % frameInterval < 1f;
        }
        private float pulseTimer = 0f;

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

            // 如果玩家寄了，删除弹幕
            Player owner = Main.player[Projectile.owner];
            if (owner.dead || !owner.active)
            {
                Projectile.Kill();
                return;
            }

            // 设置蓄力时间，如果 getGoodWorld 或 zenithWorld 则是 30，否则为 60
            float currentMaxChargeTime = (Main.getGoodWorld || Main.zenithWorld) ? 30f : MaxChargeTime;

            if (Projectile.velocity == Vector2.Zero)
            {
                // 禁用碰撞
                Projectile.tileCollide = false;
                // 无法对敌人造成伤害
                Projectile.friendly = false;

                isDashing = false; // 标记为非冲刺状态

                // 在蓄力期间降低玩家的移动速度 0.25
                //owner.velocity *= 0.98f;

                // 对准鼠标方向并进行蓄力
                Projectile.rotation = Projectile.AngleTo(Main.MouseWorld);
                if (Projectile.spriteDirection == -1)
                    Projectile.rotation += MathHelper.PiOver2;
                else
                    Projectile.rotation += MathHelper.PiOver4;

                Projectile.Center = owner.MountedCenter;
                owner.heldProj = Projectile.whoAmI;

                if (Projectile.ai[0] >= currentMaxChargeTime) // 因为要实时调整，所以改成 currentMaxChargeTime
                {
                    StartLunge(owner);
                }
                else
                {
                    Projectile.ai[0]++;
                }


  
                {
                    //Vector2 headPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 13f;

                    // 蓄力期间它的速度为零，因此这一段会失效用这一段
                    Vector2 direction = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Vector2 headPosition = Projectile.Center + direction * 16f * 10f;


                    pulseTimer++;
                    if (ShouldSpawnPulse(pulseTimer))
                    {
                        Particle shrinkingpulse = new DirectionalPulseRing(
                            headPosition,
                            Vector2.Zero,
                            Color.Orange,
                            new Vector2(1f, 1f),
                            Main.rand.NextFloat(8f, 12f),
                            0.05f,
                            3f,
                            15
                        );
                        GeneralParticleHandler.SpawnParticle(shrinkingpulse);
                    }

                    if (Main.GameUpdateCount % 2 == 0)
                    {
                        Vector2 smokeVelocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(2f, 4f));

                        Particle heavySmoke = new HeavySmokeParticle(
                            headPosition + Main.rand.NextVector2Circular(4f, 4f),
                            smokeVelocity,
                            new Color(255, 140, 0) * 0.8f,
                            Main.rand.Next(25, 35),
                            Main.rand.NextFloat(0.8f, 1.3f),
                            0.4f,
                            Main.rand.NextFloat(-0.03f, 0.03f),
                            false
                        );
                        GeneralParticleHandler.SpawnParticle(heavySmoke);

                        Dust fireDust = Dust.NewDustPerfect(
                            headPosition + Main.rand.NextVector2Circular(6f, 6f),
                            DustID.Torch,
                            smokeVelocity * 0.6f,
                            100,
                            Color.OrangeRed,
                            Main.rand.NextFloat(0.9f, 1.3f)
                        );
                        fireDust.noGravity = true;
                    }

                    // === 蓄力期间从随机位置向枪头位置发射 28 发 FinishingTouchDASHINV ===
                    if (Projectile.localAI[1] < 28 && Projectile.localAI[0] % 3f == 0f) // 每3帧发射1发
                    {
                        if (Projectile.owner == Main.myPlayer)
                        {
                            // 随机角度和距离围绕枪头位置生成
                            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                            float distance = 16f * 35f + Main.rand.NextFloat(-30f, 30f);
                            Vector2 spawnOffset = angle.ToRotationVector2() * distance;
                            Vector2 spawnPosition = headPosition + spawnOffset;

                            // 方向指向枪头位置
                            Vector2 toHead = (headPosition - spawnPosition).SafeNormalize(Vector2.UnitY);
                            float shootSpeed = 16f;
                            Vector2 velocity = toHead * shootSpeed;

                            // 发射 FinishingTouchDASHINV
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                spawnPosition,
                                velocity,
                                ModContent.ProjectileType<FinishingTouchDASHINV>(),
                                Projectile.damage,
                                Projectile.knockBack,
                                Projectile.owner
                            );
                        }

                        Projectile.localAI[1] += 1f; // 每次发射1发
                    }






                }




                if (!hasSaidPhrase)
                {
                    // 文本数组
                    string[] phrases = new string[]
                    {
        //"FINISH TIME!!!",
        "画龙点睛！",
                    };

                    // 对应的音效路径数组
                    string[] soundPaths = new string[]
                    {
        //"CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/FinishTIME",
        "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/FinishingTouch/TheSound/RayquazaRoar",
        
                    };

                    // 随机选择文本和对应的音效
                    int index = Main.rand.Next(phrases.Length);
                    string selectedPhrase = phrases[index];
                    string selectedSoundPath = soundPaths[index];

                    // 显示文本在玩家头顶
                    Vector2 textPosition = owner.Center - new Vector2(0, owner.height / 2 + 20f); // 玩家头顶位置
                    CombatText.NewText(new Rectangle((int)textPosition.X, (int)textPosition.Y, 1, 1), Color.Orange, selectedPhrase, false, false);

                    // 检查是否启用了独特音效播放的开关
                    if (ModContent.GetInstance<CTSConfigs>().EnableFTSound)
                    {
                        // 播放对应的音效
                        SoundEngine.PlaySound(new SoundStyle(selectedSoundPath) with { Volume = 2.5f }, owner.Center);
                    }
                    hasSaidPhrase = true; // 标记为已触发
                }

                // 如果在 Main.zenithWorld 模式下，每帧回复2生命
                /*if (Main.zenithWorld)
                {
                    owner.statLife += 2000;
                    owner.HealEffect(2000);
                }*/
            }
            else // 冲刺阶段
            {

                // 重新启用碰撞
                Projectile.tileCollide = true;
                // 可以对敌人造成伤害
                Projectile.friendly = true;

                isDashing = true; // 标记为冲刺状态

                Projectile.velocity = lockedDirection * Projectile.velocity.Length();

                // 冲刺阶段，同步玩家速度和位置
                owner.velocity = Projectile.velocity;
                owner.Center = Projectile.Center;

                // 启用无敌状态
                //owner.immune = true;
                //owner.immuneNoBlink = true; // 即便处于无敌状态，玩家也不会闪烁
                //owner.immuneTime = 60; // 设置为60帧的无敌时间（仅针对碰撞）

                // 移除玩家碰撞箱
                //owner.CollideWithNPCs(owner.getRect(), 0, 0, 0, 0);

                // 积累一定的值，当它到达最大值时停止冲刺，并且把玩家的速度停下来
                dashTime++;
                if (dashTime >= MaxDashTime)
                {
                    Projectile.velocity *= 0.75f;

                    return;
                }

                // 每隔18帧释放一次火球
                /*fireballTimer++;
                if (fireballTimer >= 10)
                {
                    ReleaseFireballs();
                    fireballTimer = 0; // 重置计时器
                }*/

                // 每 60 帧释放粒子拖尾和烟雾
                if (Projectile.ai[0] % 30 == 0)
                {
                    AddTrailParticles(); // 尖刺型粒子特效
                    AddSmokeParticles(); // 轻型烟雾粒子特效
                    AddHeavySmokeParticles(); // 重型烟雾粒子特效
                    AddPulseWaves(); // 🚩 新增冲击波效果

                    // 🚩 调用光点环绕生成
                    pentagonLightRotation += 0.05f; // 🚩 每次调用前累加角度，确保持续旋转
                    CTSLightingBoltsSystem.Spawn_FlamingPentagonOrbs(Projectile.Center, pentagonLightRotation);
                }
            }

            // 检查玩家是否按下空格键并且处于冲刺状态
            if (owner.controlJump && isDashing)
            {
                Projectile.Kill(); // 摧毁弹幕以解除冲刺
            }
        }

        private void StartLunge(Player owner) // 冲刺的具体逻辑
        {
            owner = Main.player[Projectile.owner];

            // 播放冲刺开始的音效
            SoundEngine.PlaySound(Yharon.RoarSound, owner.position);

            // 初始化冲刺方向和速度
            dashTime = 0; // 重置冲刺计时器
            lockedDirection = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero);

            // 冲刺速度调整（跟蓄力时间无关联）
            float chargeSpeed = (DASHSpeed);
            if (Main.getGoodWorld || Main.zenithWorld)
            {
                chargeSpeed *= 1; // 如果 getGoodWorld 或 zenithWorld 被启用，速度翻倍
            }
            Projectile.velocity = lockedDirection * chargeSpeed;


            // 播放冲刺音效和粒子效果
            SoundEngine.PlaySound(SoundID.Item73, Projectile.Center);
            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.OrangeRed, new Vector2(5f, 5f), Main.rand.NextFloat(6f, 10f), 0.15f, 3f, 10);
            GeneralParticleHandler.SpawnParticle(pulse);

            // 给无敌效果
            owner.immune = true;
            owner.immuneNoBlink = true; // 处于无敌状态玩家不会闪烁
            owner.immuneTime = 120; // 设置为120帧的无敌时间

            for (int i = 0; i < owner.hurtCooldowns.Length; i++)
            {
                owner.hurtCooldowns[i] = owner.immuneTime;
            }

            Projectile.netUpdate = true;
        }

        /*private void ReleaseFireballs() // 释放火球的逻辑
        {
            int fireballType = ModContent.ProjectileType<FinishingTouchBALL>();
            float baseAngle = MathHelper.TwoPi / 60;

            for (int i = 0; i < 60; i++)
            {
                Vector2 fireballVelocity = baseAngle.ToRotationVector2().RotatedBy(baseAngle * i) * 15f; // 初始速度为原来的8.5倍
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireballVelocity, fireballType, (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner);
            }
        }*/

        public override void OnKill(int timeLeft)
        {
            // 屏幕震动效果
            float shakePower = 5f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);

            //// 确保在弹幕死亡时清除无敌状态
            //Player owner = Main.player[Projectile.owner];
            //owner.immune = false;
            //owner.immuneNoBlink = false;
            Player owner = Main.player[Projectile.owner];
            owner.runAcceleration = 0f;
            owner.maxRunSpeed = 0f;
            owner.accRunSpeed = 0f;
            owner.velocity = Vector2.Zero;

            // 播放弹幕死亡的音效
            SoundEngine.PlaySound(Yharon.FireSound, owner.position);

            {

                // 🚩 获取正前方方向
                Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                // ======================== 有序部分：喷射型线性 SparkParticle ========================
                int sparkCount = 80; // 数量可微调
                float spread = MathHelper.ToRadians(30f); // ±15° 扩散
                for (int i = 0; i < sparkCount; i++)
                {
                    float angle = MathHelper.Lerp(-spread / 2, spread / 2, i / (float)sparkCount);
                    Vector2 dir = forward.RotatedBy(angle);
                    Color color = Color.Lerp(Color.Orange, Color.Yellow, Main.rand.NextFloat(0.2f, 0.8f));

                    Particle spark = new SparkParticle(
                        Projectile.Center,
                        dir * Main.rand.NextFloat(12f, 24f), // 高速喷射
                        false,
                        50, // 生命周期
                        Main.rand.NextFloat(1.0f, 1.8f),
                        color
                    );
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                // ======================== 无序部分：Dust 狂野扩散喷射（极限强化） ========================
                int dustCount = 300; // 🚩 数量翻 3 倍
                float extremeSpread = MathHelper.ToRadians(160f); // 🚩 ±80° 超广角喷发范围
                for (int i = 0; i < dustCount; i++)
                {
                    float randomAngle = Main.rand.NextFloat(-extremeSpread, extremeSpread);
                    Vector2 velocity = forward.RotatedBy(randomAngle) * Main.rand.NextFloat(30f, 80f); // 🚩 大幅提升速度（原 6~14 → 30~80）
                    int type = Main.rand.NextFloat() < 0.7f ? DustID.OrangeTorch : DustID.FlameBurst;
                    Dust d = Dust.NewDustDirect(
                        Projectile.Center,
                        0, 0,
                        type,
                        velocity.X,
                        velocity.Y,
                        20, // 更低 alpha，保证亮度
                        Color.OrangeRed,
                        Main.rand.NextFloat(1.8f, 3.5f) // 🚩 更大缩放
                    );
                    d.noGravity = true;
                }




            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Dragonfire>(), 300); // 龙焰

            // 给予敌人 5 秒钟的 CreateVictoryPEBuff
            int buffDuration = 5 * 60; // 5 秒钟，单位为帧（每秒 60 帧）
            target.AddBuff(ModContent.BuffType<CreateVictoryPEBuff>(), buffDuration);

            // 释放一些东西
            int slashCount = 1;
            for (int i = 0; i < slashCount; i++)
            {
                Vector2 randomDirection = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                int slashID = ModContent.ProjectileType<FinishingTouchDASHFuckYou>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, randomDirection, slashID, ((Projectile.damage) * 18), Projectile.knockBack, Projectile.owner);
            }


            // 播放音效
            //SoundEngine.PlaySound(CommonCalamitySounds.SwiftSliceSound with { Volume = CommonCalamitySounds.SwiftSliceSound.Volume * 0.5f }, Projectile.Center);

            // 释放橙红色粒子特效 - 击中方向一串和一个圆圈
            /*int numberOfDusts = 88;
            float rotFactor = 360f / numberOfDusts;

            // 释放一串粒子特效朝击中方向
            for (int i = 0; i < numberOfDusts / 2; ++i)
            {
                float dustSpeed = Main.rand.NextFloat(6.0f, 29.0f);
                Vector2 dustVel = new Vector2(dustSpeed, 0.0f).RotatedBy(Projectile.velocity.ToRotation());
                dustVel = dustVel.RotatedByRandom(0.18f);
                int dustID = Dust.NewDust(target.position, target.width, target.height, DustID.Torch, dustVel.X, dustVel.Y, 0, default, Main.rand.NextFloat(2.2f, 4.8f));
                Main.dust[dustID].noGravity = true;
                Main.dust[dustID].color = new Color(255, 140, 0); // 橙红色
            }

            // 释放一个圆圈粒子特效
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                Vector2 offset = new Vector2(75f, 14.5f).RotatedBy(rot);
                Vector2 velOffset = new Vector2(52f, 26.25f).RotatedBy(rot);
                int dust = Dust.NewDust(Projectile.position + offset, Projectile.width, Projectile.height, DustID.Torch, velOffset.X, velOffset.Y);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = velOffset;
                Main.dust[dust].scale = 3.8f;
                Main.dust[dust].color = new Color(255, 140, 0); // 橙红色
            }*/
        }

        private void AddTrailParticles() // 尖刺型粒子特效
        {
            Vector2 offset1 = Projectile.velocity.RotatedBy(2.3f) * 0.5f;
            Vector2 offset2 = Projectile.velocity.RotatedBy(-2.3f) * 0.5f;
            Color particleColor = Color.OrangeRed;

            PointParticle spark1 = new PointParticle(Projectile.Center - Projectile.velocity + offset1, offset1, false, 15, 1.1f, particleColor);
            PointParticle spark2 = new PointParticle(Projectile.Center - Projectile.velocity + offset2, offset2, false, 15, 1.1f, particleColor);
            GeneralParticleHandler.SpawnParticle(spark1);
            GeneralParticleHandler.SpawnParticle(spark2);
        }

        private void AddHeavySmokeParticles() // 重型烟雾粒子特效
        {
            // 烟雾粒子基本参数
            Color smokeColor = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
            float smokeSpeed = 5f; // 烟雾的初始速度
            int smokeLifetime = 30; // 烟雾粒子的生存时间

            // 获取投射物的顶端位置
            //Vector2 offset = new Vector2(Projectile.width / 2, -Projectile.height / 2).RotatedBy(Projectile.rotation);
            //Vector2 spawnPosition = Projectile.Center + offset;
            // 获取投射物的真正顶端位置
            //Vector2 offset = new Vector2(0, -Projectile.height / 2).RotatedBy(Projectile.velocity.ToRotation());
            //Vector2 spawnPosition = Projectile.Center + offset;


            // 获取投射物的顶端位置，修正倾斜45度的旋转
            Vector2 offset = new Vector2(0, -Projectile.width * 0.5f).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2);
            Vector2 spawnPosition = Projectile.Center + offset;

            // 直接在本体上生成，不去进行位移，因为位移有问题
            //Vector2 spawnPosition = Projectile.Center;

            // 计算烟雾粒子释放的基础方向（投射物的反方向）
            Vector2 baseDirection = -Projectile.velocity.SafeNormalize(Vector2.Zero);

            // 随机在 -15 度到 15 度之间变化
            float randomAngle = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f));
            Vector2 smokeVelocity = baseDirection.RotatedBy(randomAngle) * smokeSpeed;

            // 生成重型烟雾粒子
            Particle smoke = new HeavySmokeParticle(
                spawnPosition,
                smokeVelocity,
                smokeColor,
                smokeLifetime,
                Projectile.scale * Main.rand.NextFloat(0.7f, 1.3f),
                1.0f,
                MathHelper.ToRadians(2f),
                required: true
            );

            // 生成粒子
            GeneralParticleHandler.SpawnParticle(smoke);
        }

        private void AddSmokeParticles() // 轻型烟雾粒子特效
        {
            int dustCount = 4;
            float rotationSpeed = 0.3f;
            Vector2 spinningPoint = new Vector2(0, -40f);

            // 获取投射物的顶端位置
            //Vector2 offset = new Vector2(Projectile.width / 2, -Projectile.height / 2).RotatedBy(Projectile.rotation);
            //Vector2 spawnPosition = Projectile.Center + offset;
            // 直接在本体上生成，不去进行位移，因为位移有问题
            //Vector2 spawnPosition = Projectile.Center;

            // 获取投射物的顶端位置，修正倾斜45度的旋转
            Vector2 offset = new Vector2(0, -Projectile.width * 0.5f).RotatedBy(Projectile.velocity.ToRotation() + MathHelper.PiOver2);
            Vector2 spawnPosition = Projectile.Center + offset;


            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustPosition = spinningPoint.RotatedBy(rotationSpeed * i) + spawnPosition;
                Color dustColor = (i % 2 == 0) ? Color.Red : Color.Yellow;

                Particle smoke = new HeavySmokeParticle(dustPosition, Vector2.Zero, dustColor, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
        }
        private void AddPulseWaves()
        {
            int pulseCount = 6; // 🚩 增加叠加层次（更复杂）
            float baseScale = 0.25f; // 基础缩放
            float scaleStep = 0.07f; // 每层缩放递增（更明显）

            for (int i = 0; i < pulseCount; i++)
            {
                float rotation = Projectile.rotation - MathHelper.PiOver4;

                Particle pulse = new DirectionalPulseRing(
                    Projectile.Center
                    - Projectile.velocity.SafeNormalize(Vector2.UnitY) * (40f + i * 5f) // 🚩 修改为反向
                    + Main.rand.NextVector2Circular(8f, 8f), // 随机偏移
                    -Projectile.velocity.SafeNormalize(Vector2.UnitY) * (3f + i * 1.5f), // 🚩 修改为反向
                    Color.Lerp(Color.OrangeRed, Color.Yellow, i / (float)pulseCount),
                    new Vector2(1f, 2.5f + i * 0.4f),
                    rotation,
                    baseScale + i * scaleStep, // originalScale
                    0.02f,                     // finalScale
                    30
                );

                GeneralParticleHandler.SpawnParticle(pulse);
            }
        }




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

    }
}