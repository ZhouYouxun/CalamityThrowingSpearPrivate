using System;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Weapons.NewSpears
{
    public abstract class BaseSpearProjChannel : ModProjectile
    {
        public Player Owner => Main.player[Projectile.owner]; // 获取投射物拥有者

        // 定义长枪的行为状态
        public enum BehaviorState
        {
            Extending,  // 前进
            Retracting, // 回收
            FinalMove   // 最后一次前进回收，用于玩家松开鼠标后
        }

        public virtual float InitialSpeed => 3f; // 参考 AstralPikeProj
        public virtual float ReelbackSpeed => 2.4f; // 参考 AstralPikeProj
        public virtual float ForwardSpeed => 0.8f; // 参考 AstralPikeProj

        // 让 MaxExtendDistance 由 ForwardSpeed 计算，保持平衡
        public virtual float MaxExtendDistance => ForwardSpeed * 90f; // 90 帧左右

        public virtual SoundStyle SpearSound => SoundID.Item1; // 需要子类指定的音效
        public virtual Action<Projectile> EffectAtExtendEnd => null; // 前进结束时触发的攻击效果
        public BehaviorState CurrentState = BehaviorState.Extending; // 当前状态
        private int stateTimer = 0; // 计时器

        public virtual int LocalNPCHitCooldown => 25; // 默认无敌帧 25，可由子类覆盖
        public virtual int ProjectileSize => 15; // 默认尺寸 15，可由子类覆盖


        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = ProjectileSize; // 让子类决定尺寸
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.hide = true;
            Projectile.timeLeft = int.MaxValue; // 让投掷物永远存在
            Projectile.usesLocalNPCImmunity = true; // 始终为 true
            Projectile.localNPCHitCooldown = LocalNPCHitCooldown; // 可由子类覆盖
        }


        public override void AI()
        {
            // 持续刷新 timeLeft，防止弹幕消失
            Projectile.timeLeft = int.MaxValue;

            // 让玩家手臂保持正确方向
            ManipulatePlayerArmPositions();

            // **切换行为状态**
            switch (CurrentState)
            {
                case BehaviorState.Extending:
                    Extend();
                    break;
                case BehaviorState.Retracting:
                    Retract();
                    break;
                case BehaviorState.FinalMove:
                    FinalMovement();
                    break;
            }

            // **检测玩家是否松开鼠标**
            if (!Owner.channel && CurrentState != BehaviorState.FinalMove)
            {
                CurrentState = BehaviorState.FinalMove; // 进入最终回收阶段
            }

            // **调整旋转角度**
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            if (Projectile.spriteDirection == -1)
                Projectile.rotation -= MathHelper.PiOver2;
        }

        // 处理前进逻辑
        private Vector2 movementLineStart; // 运动线的起点（玩家位置）
        private Vector2 movementLineDirection; // 运动线方向
        private float fixedRotation; // 固定的旋转角度
        private void Extend()
        {
            if (Projectile.ai[0] == 0) // **确保初速度不是 0**
            {
                Projectile.ai[0] = InitialSpeed * 3f; 
                movementLineDirection = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.Zero);
                fixedRotation = movementLineDirection.ToRotation() + MathHelper.PiOver4;
                Projectile.netUpdate = true;
            }

            // **让 `ai[0]` 直接受到 `ForwardSpeed` 影响**
            Projectile.ai[0] += ForwardSpeed;
            Projectile.Center = Owner.Center + movementLineDirection * Projectile.ai[0];

            Projectile.rotation = fixedRotation;

            PlaySpearSound();

            if (Projectile.ai[0] >= MaxExtendDistance)
            {
                EffectAtExtendEnd?.Invoke(Projectile);
                CurrentState = BehaviorState.Retracting;
            }
        }


        // 处理回收逻辑
        private void Retract()
        {
            // **直接减少 `ai[0]`，确保回收符合 `BaseSpearProjectile`**
            Projectile.ai[0] -= ReelbackSpeed * 3f;
            Projectile.Center = Owner.Center + movementLineDirection * Projectile.ai[0];

            Projectile.rotation = fixedRotation;

            PlaySpearSound();

            if (Projectile.ai[0] <= 0)
            {
                Projectile.Kill();
            }
        }

        // 处理玩家松开鼠标后的最终前进回收
        private void FinalMovement()
        {
            if (CurrentState != BehaviorState.Retracting)
            {
                CurrentState = BehaviorState.Retracting;
            }

            // **最后一轮回收速度 `ReelbackSpeed * 3`**
            Projectile.ai[0] -= ReelbackSpeed * 3f;
            Projectile.Center = Owner.Center + movementLineDirection * Projectile.ai[0];

            PlaySpearSound();

            if (Projectile.ai[0] <= 0)
            {
                Projectile.Kill();
            }
        }



        // 让玩家手臂方向始终指向长枪
        public void ManipulatePlayerArmPositions()
        {
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;

            float armRotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);
        }

        // 播放长枪的音效
        private void PlaySpearSound()
        {
            if (SpearSound != null)
            {
                SoundEngine.PlaySound(SpearSound, Projectile.position);
            }
        }

        // 允许子类覆盖默认绘制，修正偏移问题
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * (texture.Height / 2f);
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f); // 修正偏移
            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, 0, 0);
            return false;
        }
    }
}
