using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace CalamityThrowingSpear.Weapons.NewSpears.APreHardMode.TheBrokenSpear
{
    internal class TheBrokenSpearHoldOut : ModProjectile
    {
        private const float SWINGRANGE = 1.67f * (float)Math.PI; // 挥舞角度范围 (300度)
        private const float FIRSTHALFSWING = 0.45f; // 挥舞前半部分的角度比例（相对于 SWINGRANGE）
        private const float SPINRANGE = 3.5f * (float)Math.PI; // 旋转攻击的角度范围 (630度)
        private const float WINDUP = 0.15f; // 挥舞前摇的角度比例（相对于 SWINGRANGE）
        private const float UNWIND = 0.4f; // 挥舞后摇的时间比例
        private const float SPINTIME = 2.5f; // 旋转攻击的时间倍率

        private enum AttackType
        {
            Swing, // 普通挥舞攻击
            Spin,  // 旋转攻击
        }

        // 攻击阶段
        private enum AttackStage
        {
            Prepare, // 预备阶段
            Execute, // 执行阶段
            Unwind   // 收尾阶段
        }

        // 访问 Projectile 的 AI 变量，用于存储攻击状态
        private AttackType CurrentAttack
        {
            get => (AttackType)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private AttackStage CurrentStage
        {
            get => (AttackStage)Projectile.localAI[0];
            set
            {
                Projectile.localAI[0] = (float)value;
                Timer = 0; // 阶段转换时重置计时器
            }
        }

        // 运行时变量
        private ref float InitialAngle => ref Projectile.ai[1]; // 初始角度
        private ref float Timer => ref Projectile.ai[2]; // 计时器
        private ref float Progress => ref Projectile.localAI[1]; // 挥舞进度
        private ref float Size => ref Projectile.localAI[2]; // 武器大小

        // 计算各阶段所需时间，受近战攻击速度影响
        private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float execTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
        private float hideTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);


        private Player Owner => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 68; // 弹幕的高度
            Projectile.friendly = true; // 伤害敌人
            Projectile.timeLeft = 10000; // 存活时间
            Projectile.penetrate = -1; // 无限穿透
            Projectile.tileCollide = false; // 不与地形碰撞
            Projectile.usesLocalNPCImmunity = true; // 使用本地NPC免疫
            Projectile.localNPCHitCooldown = -1; // 避免多次命中
            Projectile.ownerHitCheck = true; // 不能穿墙攻击
            Projectile.DamageType = DamageClass.Melee; // 近战类型
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 设置弹幕的方向，如果鼠标在玩家右侧，则向右，否则向左
            Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;

            // 计算目标角度，即鼠标位置相对于玩家中心的位置角度
            float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

            if (CurrentAttack == AttackType.Spin)
            {
                // 如果是旋转攻击，设置初始角度，使其从固定角度开始旋转
                InitialAngle = (float)(-Math.PI / 2 - Math.PI * 1 / 3 * Projectile.spriteDirection);
            }
            else
            {
                if (Projectile.spriteDirection == 1)
                {
                    // 限制向右挥舞时的角度范围，防止武器看起来太奇怪
                    targetAngle = MathHelper.Clamp(targetAngle, (float)-Math.PI * 1 / 3, (float)Math.PI * 1 / 6);
                }
                else
                {
                    if (targetAngle < 0)
                    {
                        // 使角度保持连续，避免负角度影响计算
                        targetAngle += 2 * (float)Math.PI;
                    }

                    // 限制向左挥舞时的角度范围
                    targetAngle = MathHelper.Clamp(targetAngle, (float)Math.PI * 5 / 6, (float)Math.PI * 4 / 3);
                }

                // 计算初始角度，使武器从指定角度开始挥舞
                InitialAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection;
            }
        }

        public override void AI()
        {
            // 持续保持武器的使用动画，直到弹幕消失
            Owner.itemAnimation = 2;
            Owner.itemTime = 2;

            // 如果玩家死亡、无法使用物品或被控制，则销毁弹幕
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            // 根据当前的攻击阶段执行不同的逻辑
            switch (CurrentStage)
            {
                case AttackStage.Prepare:
                    // 预备阶段，武器准备挥舞
                    PrepareStrike();
                    break;
                case AttackStage.Execute:
                    // 执行阶段，武器进行挥舞
                    ExecuteStrike();
                    break;
                default:
                    // 收尾阶段，武器回收或消失
                    UnwindStrike();
                    break;
            }

            // 更新武器的位置，使其跟随玩家手臂进行挥舞
            SetSwordPosition();

            // 计时器递增，控制每个阶段的进度
            Timer++;
        }

        // 设定弹幕的位置以及角色手臂的动画
        public void SetSwordPosition()
        {
            // **让武器完全固定在玩家中心**
            Projectile.Center = Owner.MountedCenter;

            // 计算当前武器旋转角度
            Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress;

            // 让玩家的手臂依然跟随武器
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                Projectile.rotation - MathHelper.ToRadians(90f));

            // 调整武器大小，考虑近战武器尺寸调整
            Projectile.scale = Size * 1.2f * Owner.GetAdjustedItemScale(Owner.HeldItem);

            // 让玩家的 "heldProj" 变量指向当前武器，使其可见
            Owner.heldProj = Projectile.whoAmI;
        }


        // 预备攻击阶段（举起武器准备攻击）
        private void PrepareStrike()
        {
            // 计算挥舞起手角度，使武器从后撤的角度开始
            Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime);

            // 逐渐增大武器尺寸，使其缓慢显现
            Size = MathHelper.SmoothStep(0, 1, Timer / prepTime);

            // 当达到预备时间，进入攻击阶段
            if (Timer >= prepTime)
            {
                SoundEngine.PlaySound(SoundID.Item1); // 播放挥舞武器的音效
                CurrentStage = AttackStage.Execute; // 进入攻击执行阶段
            }
        }

        // 处理攻击执行阶段（挥舞的前半段）
        private void ExecuteStrike()
        {
            if (CurrentAttack == AttackType.Swing)
            {
                // 计算挥舞的角度进度，使其平滑过渡
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);

                // 当执行时间达到设定值，进入收尾阶段
                if (Timer >= execTime)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }
            else
            {
                // 旋转攻击的角度过渡，同样使用平滑插值计算
                Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) * Timer / (execTime * SPINTIME));

                // 在旋转攻击进行到 3/4 处时，播放武器音效并重置 NPC 免疫时间
                if (Timer == (int)(execTime * SPINTIME * 3 / 4))
                {
                    SoundEngine.PlaySound(SoundID.Item1); // 播放武器音效
                    Projectile.ResetLocalNPCHitImmunity(); // 重置 NPC 受击免疫，以便旋转攻击能再次命中
                }

                // 旋转攻击时间到达设定值后，进入收尾阶段
                if (Timer >= execTime * SPINTIME)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }
        }

        // 处理攻击的收尾阶段（挥舞的后半段，武器逐渐消失）
        private void UnwindStrike()
        {
            if (CurrentAttack == AttackType.Swing)
            {
                // 计算后半段的挥舞角度进度，使其平滑过渡
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);

                // 逐渐缩小武器尺寸，使其平滑地消失
                Size = 1f - MathHelper.SmoothStep(0, 1, Timer / hideTime);

                // 当收尾时间达到设定值，销毁弹幕
                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                // 旋转攻击的收尾阶段，同样进行平滑角度过渡
                Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) + UNWIND / 2 * Timer / (hideTime * SPINTIME / 2));

                // 逐渐缩小武器尺寸，使其平滑地消失
                Size = 1f - MathHelper.SmoothStep(0, 1, Timer / (hideTime * SPINTIME / 2));

                // 当旋转攻击的收尾时间达到设定值，销毁弹幕
                if (Timer >= hideTime * SPINTIME / 2)
                {
                    Projectile.Kill();
                }
            }
        }


        //public override bool PreDraw(ref Color lightColor)
        //{
        //    // 计算武器的原点（剑柄位置），并根据朝向调整旋转角度
        //    Vector2 origin;
        //    float rotationOffset;
        //    SpriteEffects effects;

        //    if (Projectile.spriteDirection > 0)
        //    {
        //        // 当武器朝向右方时，设置原点在左下角，并调整旋转角度
        //        origin = new Vector2(0, Projectile.height);
        //        rotationOffset = MathHelper.ToRadians(45f);
        //        effects = SpriteEffects.None;
        //    }
        //    else
        //    {
        //        // 当武器朝向左方时，设置原点在右下角，并调整旋转角度
        //        origin = new Vector2(Projectile.width, Projectile.height);
        //        rotationOffset = MathHelper.ToRadians(135f);
        //        effects = SpriteEffects.FlipHorizontally;
        //    }

        //    // 获取当前弹幕的纹理
        //    Texture2D texture = TextureAssets.Projectile[Type].Value;

        //    // 绘制武器，应用旋转、缩放等参数
        //    Main.spriteBatch.Draw(texture,
        //        Projectile.Center - Main.screenPosition,
        //        default,
        //        lightColor * Projectile.Opacity,
        //        Projectile.rotation + rotationOffset,
        //        origin,
        //        Projectile.scale,
        //        effects,
        //        0);

        //    // 由于已经自定义绘制，不让原本的绘制逻辑执行
        //    return false;
        //}

        public override bool PreDraw(ref Color lightColor)
        {
            // 获取当前弹幕的纹理
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            // **让旋转中心与玩家中心对齐**
            Vector2 origin = texture.Size() / 2f; // 让旋转点在武器中心，而不是剑柄位置

            // **让武器的绘制位置对齐玩家中心**
            Vector2 drawPosition = Owner.MountedCenter - Main.screenPosition;

            // **调整旋转角度的偏移**
            float rotationOffset = Projectile.spriteDirection > 0 ? MathHelper.ToRadians(45f) : MathHelper.ToRadians(135f);
            SpriteEffects effects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            // **绘制武器**
            Main.spriteBatch.Draw(texture,
                drawPosition,
                null,
                lightColor * Projectile.Opacity,
                Projectile.rotation + rotationOffset,
                origin,
                Projectile.scale,
                effects,
                0);

            // 由于已经自定义绘制，不让原本的绘制逻辑执行
            return false;
        }


        // 计算武器的起点和终点，使用线性碰撞检测敌人
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + Projectile.rotation.ToRotationVector2() * ((Projectile.Size.Length()) * Projectile.scale);
            float collisionPoint = 0f;

            // 检测敌人碰撞，如果敌人的 hitbox 与武器的挥舞轨迹相交，则判定命中
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
        }







        // 只有在执行和收尾阶段，武器才可以造成伤害
        public override bool? CanDamage()
        {
            if (CurrentStage == AttackStage.Prepare)
                return false; // 预备阶段不造成伤害
            return base.CanDamage();
        }


        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 使击退方向远离玩家
            modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;

            // 如果是旋转攻击，增加额外的击退力度
            if (CurrentAttack == AttackType.Spin)
                modifiers.Knockback += 1;
        }


    }
}