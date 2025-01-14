using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav
{
    public class SawBladeForkHornJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/SawBladeForkHornJav/SawBladeForkHornJav";

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

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 35;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        internal Color ColorFunction(float completionRatio)
        {
            float fadeOpacity = Utils.GetLerpValue(0.94f, 0.54f, completionRatio, true) * Projectile.Opacity;
            return Color.Lerp(Color.Black, Color.LightGray, 0.4f) * fadeOpacity;
        }

        internal float WidthFunction(float completionRatio)
        {
            float expansionCompletion = 1f - (float)Math.Pow(1f - Utils.GetLerpValue(0f, 0.3f, completionRatio, true), 2D);
            return MathHelper.Lerp(0f, 12f * Projectile.Opacity, expansionCompletion);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            GameShaders.Misc["CalamityMod:TrailStreak"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(WidthFunction, ColorFunction, (_) => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:TrailStreak"]), 60);
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            return false;
        }


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public Player Owner => Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[1];
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 遍历所有活跃的弹幕
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<SawBladeForkHornJavRIGHT>())
                {
                    proj.Kill(); // 销毁所有活跃的 SawBladeForkHornJavRIGHT 弹幕
                }
            }
        }

        public override void AI()
        {
            // 检查弹幕与玩家的距离
            float distanceToPlayer = Vector2.Distance(Projectile.Center, Owner.Center);
            if (distanceToPlayer > 5000f)
            {
                Projectile.Kill(); // 如果距离超过5000像素，销毁弹幕
                return; // 结束AI逻辑
            }

            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                case BehaviorState.Fire:
                    DoBehavior_Fire();
                    break;
            }
            Time++;
            // 更新本地计时器，用于特效释放
            Projectile.localAI[0]++;
        }

        public void DoBehavior_Aim()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 不断的重置剩余时间
            Projectile.timeLeft = 480;

            // 设置穿透次数为 -1
            Projectile.penetrate = -1;

            // 不断的让它可以穿透方块
            Projectile.tileCollide = false;

            // 使投射物与玩家保持一致并瞄准鼠标位置
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 aimDirection = Owner.SafeDirectionTo(Main.MouseWorld);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, aimDirection, 0.1f);
            }

            // 将投射物位置与玩家中心对齐，模拟持握效果
            // Projectile.Center = Owner.Center;
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width / 1);
            Owner.heldProj = Projectile.whoAmI;

            // 每 60 帧生成一次黑色冲击波
            if (Projectile.localAI[0] % 30 == 0)
            {
                Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Black, new Vector2(1f, 1f), Main.rand.NextFloat(6f, 10f), 0.15f, 3f, 10);
                GeneralParticleHandler.SpawnParticle(pulse);
            }

            // 检查玩家是否松开鼠标
            if (!Owner.channel)
            {
                // 切换至发射状态
                CurrentState = BehaviorState.Fire;
                Time = 0f;
                Projectile.netUpdate = true;
            }
        }

        public void DoBehavior_Fire()
        {
            // 设置穿透次数为 1
            Projectile.penetrate = -1;

            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 现在不再能穿透方块了
            Projectile.tileCollide = true;

            // 重置速度的逻辑
            {
                float initialSpeed = 30f; // 设定初始速度值，可根据需求替换具体值
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * initialSpeed;
            }

            // 粒子效果保持不变
            int particleCount = Main.rand.Next(3, 6);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 offset = Projectile.velocity.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(0.5f, 1.5f);
                Vector2 particleVelocity = offset * 0.2f;
                int dustID = Dust.NewDust(Projectile.Center + offset, 0, 0, DustID.Smoke, particleVelocity.X, particleVelocity.Y, 100, Color.Black, 1.5f);
                Main.dust[dustID].noGravity = true;
                Main.dust[dustID].scale = Main.rand.NextFloat(0.7f, 1.2f);
            }


            // 每两帧生成黑色烟雾粒子
            if (Projectile.localAI[0] % 2 == 0)
            {
                Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(0.3f, 0.8f);
                Particle blackSmokeParticle = new HeavySmokeParticle(Projectile.Center, velocity, Color.Black, 15, 0.9f, 0.5f, 0.2f, true);
                GeneralParticleHandler.SpawnParticle(blackSmokeParticle);
            }
        }

        public override bool? CanDamage()
        {
            // 只有在发射阶段才能造成伤害
            return CurrentState == BehaviorState.Fire ? true : false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 给予敌人 SawBladeForkHornEDebuff
            target.AddBuff(ModContent.BuffType<SawBladeForkHornEDebuff>(), 300); // 5秒持续时间

            // 给予敌人 MarkedforDeath死亡标记
            target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 300); // 5秒持续时间

            // 给予敌人 Crumbling粉碎
            target.AddBuff(ModContent.BuffType<Crumbling>(), 300); // 5秒持续时间

            // 给予玩家可以堆叠的 SawBladeForkHornPBuff
            var player = Main.player[Projectile.owner].GetModPlayer<SawBladeForkHornPlayer>();
            player.IncreaseStackCount(); // 每次击中敌人时增加堆叠

            // 播放手榴弹的音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // 在敌人周围生成碎石和泥土粒子
            for (int i = 0; i < 10; i++)
            {
                int dustType = Main.rand.Next(new int[] { DustID.Dirt, DustID.Stone, DustID.AmberBolt });
                Vector2 dustPosition = target.Center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 dustVelocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(0.5f, 1.5f);
                Dust.NewDustPerfect(dustPosition, dustType, dustVelocity, 100, default, Main.rand.NextFloat(0.8f, 1.2f));
            }

            // 屏幕震动效果
            float shakePower = 10f; // 设置震动强度
            float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true); // 距离衰减
            Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(Main.LocalPlayer.Calamity().GeneralScreenShakePower, shakePower * distanceFactor);
        }

        public override void OnKill(int timeLeft)
        {
            float pulseCompletionRatio = 1f; // 在死亡时模拟震动的完成比例
            float screenShakePower = CalamityUtils.Convert01To010(pulseCompletionRatio) * 16f;

            // 如果当前的震动强度小于需要的震动强度，则更新为更强的震动
            if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < screenShakePower)
            {
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = screenShakePower;
            }
        }
    }
}
