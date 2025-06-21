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
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Items.Weapons.Magic;
using Terraria.GameContent;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.BloodstoneJav
{
    public class BloodstoneJavPROJ : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/DPreDog/BloodstoneJav/BloodstoneJav";

        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
        }

        public enum BehaviorState
        {
            Aim,
            Dash
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        private int chargeLevel = 0; // 当前蓄力等级
        private int hitCounter = 0; // 命中计数器
        private const int MaxChargeLevel = 15;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1; // 只允许一次伤害
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; // 允许与方块碰撞
            Projectile.extraUpdates = 1; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            // 添加深红色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.DarkRed.ToVector3() * 0.55f);
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Dash:
                    DoBehavior_Dash();
                    break;
            }
        }

        private void DoBehavior_Aim()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 对齐到玩家中心
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;

            // 每45帧提升蓄力等级
            if (Projectile.localAI[0] % 45 == 0 && chargeLevel < MaxChargeLevel)
            {
                chargeLevel++;
                CreateChargeEffect();
                InflictChargePenalty();
            }

            // 检测松手
            if (!Owner.channel)
            {
                CurrentState = BehaviorState.Dash;
                Projectile.netUpdate = true;
                Projectile.penetrate = 1 + chargeLevel; // 根据等级设置穿透次数
                float speedBoost = 14f + chargeLevel * 0.2f; // 飞行速度提升
                Projectile.velocity *= speedBoost;
            }

            Projectile.localAI[0]++;
        }

        private void CreateChargeEffect()
        {
            Vector2 center = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 2f;
            Color electricColor = Color.Red;

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.ToRadians(60 * i);
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                Particle electricParticle = new SparkParticle(
                    center,
                    direction * 2f,
                    false,
                    60,
                    Main.rand.NextFloat(1.5f, 2.0f),
                    electricColor
                );

                GeneralParticleHandler.SpawnParticle(electricParticle);
            }
            SoundEngine.PlaySound(SoundID.Item30, Projectile.position);
        }

        private void InflictChargePenalty()
        {
            int damagePenalty = 8; // 每级扣除X点血量
            Owner.statLife -= damagePenalty;
            CombatText.NewText(Owner.getRect(), Color.Lime, -damagePenalty); // 显示绿色负值

            if (Owner.statLife <= 0)
            {
                Owner.KillMe(PlayerDeathReason.ByCustomReason($"{Owner.name} 把自己榨干了"), damagePenalty, 0);
            }
        }

        private void DoBehavior_Dash()
        {

            // 重置速度的逻辑
            {
                float initialSpeed = 18f; // 设定初始速度值，可根据需求替换具体值
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            }


            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.tileCollide = true;

            // 飞行期间的粒子效果
            Color bloodColor = Color.Red;
            float scaleBoost = MathHelper.Clamp(chargeLevel * 0.005f, 0f, 2f);
            float outerSparkScale = 1.5f + scaleBoost;
            SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, bloodColor);
            GeneralParticleHandler.SpawnParticle(spark);

            // 每10帧生成血红色圆圈
            if (Projectile.localAI[0] % 5 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Particle pulse = new DirectionalPulseRing(Projectile.Center, Projectile.velocity * 0.75f, bloodColor, new Vector2(1f, 2.5f), Projectile.rotation - MathHelper.PiOver4, 0.2f, 0.03f, 20);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }

            // 添加线性粒子特效
            if (Projectile.localAI[0] % 5 == 0) // 每隔 5 帧生成一次
            {
                Vector2 particleVelocity = Projectile.velocity * 0.8f; // 粒子速度基于弹幕速度
                Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f); // 粒子生成位置略有随机偏移

                // 生成粒子
                LineParticle bloodTrail = new LineParticle(
                    particlePosition,
                    particleVelocity,
                    false,
                    30, // 粒子存活时间
                    0.5f, // 粒子缩放大小
                    Color.DarkRed // 粒子颜色为血红色
                );

                GeneralParticleHandler.SpawnParticle(bloodTrail);
            }


            Projectile.localAI[0]++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 恢复玩家生命值
            Player player = Main.player[Projectile.owner];
            float healMultiplier = 0.025f + chargeLevel * 0.001f; // 每级多增加0.1%回复
            int healAmount = (int)(damageDone * healMultiplier);
            player.statLife += healAmount;
            player.HealEffect(healAmount);

            // 每命中4次触发特效
            hitCounter++;
            if (hitCounter % 4 == 0 && chargeLevel > 0)
            {
                CreateImpactEffects();
            }
        }

        private void CreateImpactEffects()
        {
            // 血雾
            for (int i = 0; i < 3; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                Particle bloodFog = new HeavySmokeParticle(Projectile.Center, velocity, Color.Red, 18, Main.rand.NextFloat(0.9f, 1.6f), 0.35f, Main.rand.NextFloat(-1, 1), true);
                GeneralParticleHandler.SpawnParticle(bloodFog);
            }

            // Visceral爆炸
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<VisceraBoom>(),
                (int)(Projectile.damage * 0.75f),
                Projectile.knockBack * 4,
                Projectile.owner
            );

            // 血液爆炸冲击波
            Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkRed, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, 40, false);
            GeneralParticleHandler.SpawnParticle(bloodsplosion);
            Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, new Color(255, 32, 32), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f, 0.155f, 40);
            GeneralParticleHandler.SpawnParticle(bloodsplosion2);
        }
        public override bool? CanDamage()
        {
            // 如果是 Zenith World 天顶世界，无论何时都允许造成伤害
            if (Main.zenithWorld)
            {
                return true;
            }

            // 如果是正常世界，那么蓄力状态下不造成伤害
            if (CurrentState == BehaviorState.Aim)
            {
                return false;
            }

            // 如果当前状态是冲刺状态，允许造成伤害
            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (CurrentState == BehaviorState.Dash)
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            }
            else
            {
                Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            }
            return false;
        }
      
    }
}

