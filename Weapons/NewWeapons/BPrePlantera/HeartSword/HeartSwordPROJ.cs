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
using CalamityMod.Balancing;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;



namespace CalamityThrowingSpear.Weapons.NewWeapons.BPrePlantera.HeartSword
{
    public class HeartSwordPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/BPrePlantera/HeartSword/HeartSword";
        public new string LocalizationCategory => "Projectiles.NewWeapons.BPrePlantera";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            // 添加刀刃亮光效果
            Texture2D shineTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            Vector2 shineScale = new Vector2(1.67f, 3f) * Projectile.scale;
            shineScale *= MathHelper.Lerp(0.9f, 1.1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.4f + Projectile.identity) * 0.5f + 0.5f);

            // 设置亮光的位置为弹幕的中心
            //Vector2 lensFlareWorldPosition = Projectile.Center; // 移除偏移，直接使用弹幕中心
            Vector2 lensFlareWorldPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 0.7f); // 改为弹幕前端

            // 亮光颜色为红色和橙色渐变
            Color lensFlareColor = Color.Lerp(Color.Red, Color.Orange, 0.23f) with { A = 0 };
            Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, 0f, shineTex.Size() * 0.5f, shineScale * 0.6f, 0, 0);
            Main.EntitySpriteDraw(shineTex, lensFlareWorldPosition - Main.screenPosition, null, lensFlareColor, MathHelper.PiOver2, shineTex.Size() * 0.5f, shineScale, 0, 0);
            return false;
        }


        public enum BehaviorState
        {
            Aim,
            Fire
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 60; // 无敌帧冷却时间
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 创建三个 HeartSwordPROJExtra
            float radius = 10 * 16f;
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi / 3 * i; // 每120度一个弹幕
                Vector2 spawnPosition = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Projectile.NewProjectile(source, spawnPosition, Vector2.Zero, ModContent.ProjectileType<HeartSwordPROJExtra>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: angle);
            }
        }

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Lighting - 添加深红色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.55f);

            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Fire:
                    DoBehavior_Fire();
                    break;
            }
        }
        public Player Owner => Main.player[Projectile.owner];

        private void DoBehavior_Aim()
        {
            // 不断的重置剩余时间
            Projectile.timeLeft = 240;

            // 设置穿透次数为 -1
            Projectile.penetrate = -1;

            // 不断的让它可以穿透方块
            Projectile.tileCollide = false;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.Center = Main.player[Projectile.owner].Center;
            if (!Main.player[Projectile.owner].channel)
            {
                CurrentState = BehaviorState.Fire;
                Projectile.netUpdate = true;
            }

            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 将投射物位置与玩家中心对齐，模拟持握效果
            //Projectile.Center = Owner.Center;

            // 弹幕末端对齐玩家中心
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 2);
            Owner.heldProj = Projectile.whoAmI;
        }

        private void DoBehavior_Fire()
        {
            // 设置穿透次数为 1
            Projectile.penetrate = 1;

            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 现在不再能穿透方块了
            Projectile.tileCollide = true;

            float speed = 30f;
            Projectile.velocity = Vector2.Normalize(Projectile.velocity) * speed;

            // 弹幕保持直线运动并逐渐加速
            Projectile.velocity *= 1.01f;

            // 生成血红色烟雾特效
            int Dusts = 2;
            float radians = MathHelper.TwoPi / Dusts;
            Vector2 spinningPoint = Vector2.Normalize(new Vector2(-1f, -1f));
            for (int i = 0; i < Dusts; i++)
            {
                Vector2 dustVelocity = spinningPoint.RotatedBy(radians * i).RotatedBy(0.5f) * 6.5f;
                Particle smoke = new HeavySmokeParticle(Projectile.Center, dustVelocity * Main.rand.NextFloat(1f, 2.6f), Color.DarkRed, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }


            // === 1. 心跳波线拖尾 ===
            if (Projectile.localAI[0] % 3 == 0) // 每3帧一个点
            {
                float wave = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 6f; // 数学心跳波
                Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2) * wave;

                LineParticle heartbeatTrail = new LineParticle(
                    Projectile.Center + offset,
                    Projectile.velocity * 0.15f,
                    false,
                    12,
                    1.1f,
                    Color.Lerp(Color.DarkRed, Color.Red, 0.6f)
                );
                GeneralParticleHandler.SpawnParticle(heartbeatTrail);
            }

            // === DNA 双螺旋轨迹 ===
            float time = Projectile.localAI[0] * 0.2f; // 时间推进
            float radius = 18f; // 缠绕半径
            int segments = 2;   // 两股

            for (int i = 0; i < segments; i++)
            {
                // 每股相差 180°
                float angle = time + i * MathHelper.Pi;

                // 核心：cos+sin 绕中心点转圈
                Vector2 spiralOffset = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );

                // 叠加到主弹幕中心
                Vector2 spawnPos = Projectile.Center + spiralOffset;

                GlowOrbParticle spiral = new GlowOrbParticle(
                    spawnPos,
                    Vector2.Zero,
                    false,
                    35,    // 生命周期：要够长，能连成丝带
                    1.1f,  // 缩放大一些，视觉更明显
                    Color.Lerp(Color.DarkRed, Color.OrangeRed, (float)Math.Sin(angle) * 0.5f + 0.5f),
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(spiral);
            }

            Projectile.localAI[0]++;




            // === 3. 血色流星碎片 ===
            if (Main.rand.NextBool(3))
            {
                Vector2 scatter = (-Projectile.velocity).RotatedByRandom(MathHelper.ToRadians(15f)) * 0.3f;
                PointParticle shard = new PointParticle(
                    Projectile.Center,
                    scatter,
                    false,
                    20,
                    0.9f,
                    Color.OrangeRed
                );
                GeneralParticleHandler.SpawnParticle(shard);
            }


        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            // 回复玩家生命值
            int healAmount = (int)(damageDone * 0.05f);
            player.statLife += healAmount;
            player.HealEffect(healAmount);

      

            // 根据生命值百分比触发超级效果
            float healthPercent = (float)player.statLife / player.statLifeMax2;
            float chance = MathHelper.Lerp(0.04f, 0.004f, healthPercent);
            if (Main.rand.NextFloat() <= chance)
            {
                float radius = 15 * 16f;
                for (int i = 0; i < (player.statLifeMax2 - player.statLife) / 20 * 2; i++)
                {
                    Item.NewItem(Projectile.GetSource_OnHit(target), target.Center, ItemID.Heart);
                }
            }
        }

        // 创建治疗魔法阵特效
        private void CreateHealingCircle(Vector2 center)
        {
            int particleCount = 36; // 粒子数量，生成心形图案的密度
            float expansionSpeed = 2f; // 粒子扩散的速度
            float particleScale = 1.5f; // 粒子的初始大小

            for (int i = 0; i < particleCount; i++)
            {
                // 使用极坐标生成心形图案
                float angle = MathHelper.TwoPi * i / particleCount;
                float x = 16 * (float)Math.Pow(Math.Sin(angle), 3);
                float y = 13 * (float)Math.Cos(angle) - 5 * (float)Math.Cos(2 * angle) - 2 * (float)Math.Cos(3 * angle) - (float)Math.Cos(4 * angle);

                // 计算粒子位置和速度
                Vector2 offset = new Vector2(x, -y) * 0.5f; // 调整比例和方向
                Vector2 velocity = offset * expansionSpeed;

                // 创建红宝石粒子
                Dust rubyDust = Dust.NewDustPerfect(center + offset, DustID.GemRuby, velocity, 100, Color.Red, particleScale);
                rubyDust.noGravity = true; // 无重力粒子
                rubyDust.fadeIn = 1.5f;

                // 创建生命水晶粒子
                Dust lifeCrystalDust = Dust.NewDustPerfect(center + offset, DustID.LifeCrystal, velocity, 100, Color.Pink, particleScale);
                lifeCrystalDust.noGravity = true;
                lifeCrystalDust.fadeIn = 1.5f;
            }
        }


        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item4, Projectile.Center);
            // 添加治疗魔法阵特效
            CreateHealingCircle(Projectile.Center);
        }
    }
}