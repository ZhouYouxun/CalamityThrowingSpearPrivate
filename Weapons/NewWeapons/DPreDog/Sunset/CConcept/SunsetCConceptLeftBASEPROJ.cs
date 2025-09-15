//using CalamityMod;
//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;
//using CalamityMod.Particles;
//using Microsoft.Xna.Framework.Graphics;
//using Terraria.Audio;
//using Terraria.DataStructures;
//using Terraria.Graphics.Shaders;

//namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept
//{
//    internal abstract class SunsetCConceptLeftBASEPROJ : ModProjectile, ILocalizedModType
//    {
//        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";
//        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

//        private const int AttackInterval = 90;
//        private float attackSpeedMultiplier = 2.5f;
//        private float returnSpeedMultiplier = 3.0f;
//        private Vector2 idlePosition;
//        private Vector2 attackTarget = Vector2.Zero;
//        private bool isAttacking = false;
//        private bool isReturning = false;

//        private Vector2 idleOffset;
//        private Vector2 attackOffset;

//        private Vector2 attackStartPos;
//        private Vector2 returnStartPos;
//        private float attackProgress = 0f;
//        private float returnProgress = 0f;

//        private float defaultRotation; // **存储默认朝向**

//        public override void SetDefaults()
//        {
//            Projectile.width = 32;
//            Projectile.height = 32;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.DamageType = DamageClass.Melee;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = 300;
//            Projectile.light = 0.5f;
//            Projectile.ignoreWater = true;
//            Projectile.tileCollide = false;
//            Projectile.extraUpdates = 1;
//            Projectile.usesLocalNPCImmunity = true;
//            Projectile.localNPCHitCooldown = 30;
//        }
//        public static class ParticleLibrary
//        {
//            public static readonly int[] YellowDusts = { 169, 159, 133 };
//            public static readonly int[] BlueDusts = { 80, 67, 48 };
//            public static readonly int[] BlackDusts = { 240, 191, 175 };
//            public static readonly int[] RedDusts = { 5, 12, 60 };

//            public static int[] GetDustsByColor(Color color)
//            {
//                if (color == Color.Yellow) return YellowDusts;
//                if (color == Color.Blue) return BlueDusts;
//                if (color == Color.Black) return BlackDusts;
//                if (color == Color.Red) return RedDusts;
//                return YellowDusts; // **默认返回黄色**
//            }
//        }

//        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
//        {
//            base.OnSpawn(source);

//            // 计算同类弹幕数量
//            int existingCount = 0;
//            foreach (Projectile proj in Main.projectile)
//            {
//                if (proj.active && proj.type == Type && proj.owner == Projectile.owner)
//                {
//                    existingCount++;
//                }
//            }

//            // 只有在 **创建之前** 已经有 4 个弹幕时，才销毁
//            if (existingCount > 4)
//            {
//                Projectile.Kill();
//                return;
//            }

//            // **记录相对于玩家的偏移量**
//            idleOffset = GetIdleOffset();
//            attackOffset = new Vector2(Main.screenWidth - 50, 50); // **相对于屏幕的右上角偏移**

//            texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
//        }


//        public override void AI()
//        {
//            Player owner = Main.player[Projectile.owner];

//            // **始终保持相对于玩家的悬停位置**
//            idlePosition = owner.Center + idleOffset;
//            attackTarget = Main.screenPosition + attackOffset;

//            Projectile.timeLeft = 300;
//            Projectile.rotation = Projectile.AngleFrom(Main.player[Projectile.owner].Center) + MathHelper.PiOver2 + MathHelper.PiOver4;

//            if (isReturning)
//            {
//                ReturnToIdle();
//            }
//            else if (isAttacking)
//            {
//                ExecuteAttack();
//            }
//            else
//            {
//                MaintainIdlePosition();

//                if (Projectile.ai[0] % AttackInterval == 0 && Vector2.Distance(Projectile.Center, idlePosition) < 5f)
//                {
//                    isAttacking = true;
//                    attackStartPos = Projectile.Center;
//                    attackProgress = 0f;
//                }
//            }


//            Projectile.ai[0]++;
//        }

//        private void ExecuteAttack()
//        {
//            attackProgress += 0.02f; // **攻击速度曲线**
//            attackProgress = MathHelper.Clamp(attackProgress, 0f, 1f);

//            // **计算贝塞尔曲线轨迹**
//            Vector2 midPoint = (attackStartPos + attackTarget) / 2 + new Vector2(0, -100); // **攻击轨迹弧形**
//            Vector2 newPos = BezierCurve(attackStartPos, midPoint, attackTarget, attackProgress);

//            // **逐渐加速再减速**
//            float speedFactor = MathHelper.Lerp(1.5f, 0.6f, attackProgress);
//            Projectile.velocity = (newPos - Projectile.Center).SafeNormalize(Vector2.Zero) * (20f * attackSpeedMultiplier * speedFactor);
//            Projectile.Center = newPos;
//            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

//            if (attackProgress >= 1f)
//            {
//                OnReachTarget();
//            }
//        }

//        private void OnReachTarget()
//        {
//            isAttacking = false;
//            isReturning = true;
//            returnStartPos = Projectile.Center;
//            returnProgress = 0f;

//            Player owner = Main.player[Projectile.owner];
//            Projectile.NewProjectile(
//                Projectile.GetSource_FromThis(),
//                owner.Center,
//                Vector2.Zero,
//                ModContent.ProjectileType<SunsetCConceptLeftEXP>(),
//                (int)(Projectile.damage * 2.0),
//                Projectile.knockBack,
//                Projectile.owner
//            );

//            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
//        }

//        private void ReturnToIdle()
//        {
//            returnProgress += 0.015f; // **返回速度调整**
//            returnProgress = MathHelper.Clamp(returnProgress, 0f, 1f);

//            // **随机生成贝塞尔曲线的控制点**
//            float randomCurveHeight = Main.rand.Next(250, 551); // **随机弧度：250~550**
//            float randomCurveDirection = Main.rand.NextBool() ? -1f : 1f; // **随机选择向左或向右拱起**

//            Vector2 midPoint = (returnStartPos + idlePosition) / 2 + new Vector2(randomCurveDirection * Main.rand.Next(50, 151), randomCurveHeight);

//            // **计算返回路径的贝塞尔曲线**
//            Vector2 newPos = BezierCurve(returnStartPos, midPoint, idlePosition, returnProgress);

//            // **速度逐渐降低**
//            float speedFactor = MathHelper.Lerp(1.2f, 0.4f, returnProgress);
//            Projectile.velocity = (newPos - Projectile.Center).SafeNormalize(Vector2.Zero) * (15f * returnSpeedMultiplier * speedFactor);
//            Projectile.Center = newPos;
//            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

//            // **在返回过程中释放方形粒子**
//            if (Main.rand.NextBool(2)) // 50% 概率释放粒子
//            {
//                Vector2 particlePosition = Projectile.Center + Main.rand.NextVector2Circular(2 * 16, 2 * 16);
//                SquareParticle squareParticle = new SquareParticle(
//                    particlePosition,
//                    Projectile.velocity * 0.5f,
//                    false,
//                    30,
//                    1.7f + Main.rand.NextFloat(0.6f),
//                    GetColor() // **使用传递进来的颜色**
//                );

//                GeneralParticleHandler.SpawnParticle(squareParticle);
//            }

//            // **回归结束时直接归位**
//            if (returnProgress >= 1f)
//            {
//                isReturning = false;
//                Projectile.Center = idlePosition; // **直接归位**
//                Projectile.velocity = Vector2.Zero;
//            }
//        }


//        private void MaintainIdlePosition()
//        {
//            // **直接设置坐标，避免跟踪误差**
//            Vector2 hoverDestination = Main.player[Projectile.owner].Center + idleOffset;
//            Projectile.Center = Vector2.Lerp(Projectile.Center, hoverDestination, 0.04f).MoveTowards(hoverDestination, 16f);
//            Projectile.velocity *= 0.8f;

//            // **保持默认朝向**
//            //Projectile.rotation = defaultRotation;
//            Projectile.rotation = Projectile.AngleFrom(Main.player[Projectile.owner].Center) + MathHelper.PiOver4;

//            //// **调试输出**
//            //if (Main.myPlayer == Projectile.owner) // 只让本地玩家输出，避免多人模式下刷屏
//            //{
//            //    Main.NewText($"[调试] 位置: {Projectile.Center}, 目标: {hoverDestination}, 旋转: {Projectile.rotation}", Color.Cyan);
//            //}

//            // **获取当前子类的颜色，并找到对应的粒子 Dust ID**
//            int[] dusts = ParticleLibrary.GetDustsByColor(GetColor());

//            // **计算枪头位置**
//            Vector2 gunTipPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 15f;

//            // **50% 概率生成粒子**
//            if (Main.rand.NextBool(2))
//            {
//                for (int i = 0; i < 3; i++) // **在 1.5×16 范围内随机生成 3 个粒子**
//                {
//                    Vector2 particlePos = gunTipPosition + Main.rand.NextVector2Circular(1.5f * 16, 1.5f * 16);
//                    int dustType = dusts[Main.rand.Next(dusts.Length)]; // **随机选取一个 Dust ID**

//                    Dust dust = Dust.NewDustPerfect(particlePos, dustType, Vector2.Zero, 100, GetColor(), 1.2f);
//                    dust.noGravity = true;
//                    dust.velocity *= 0.1f; // **让粒子缓慢运动**
//                }
//            }
//        }

//        private Vector2 BezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, float t)
//        {
//            float u = 1 - t;
//            return (u * u) * p0 + (2 * u * t) * p1 + (t * t) * p2;
//        }

//        private Texture2D texture;
//        private int DrawFlashTimer = 0;
//        //public override void PostDraw(Color lightColor)
//        //{
//        //    if (texture == null)
//        //        return;

//        //    // 结束当前绘制
//        //    Main.spriteBatch.End();

//        //    // 以 UI 层的方式重新开启 SpriteBatch，让弹幕覆盖 UI
//        //    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.UIScaleMatrix);

//        //    // 计算居中绘制的原点
//        //    Vector2 origin = texture.Size() * 0.5f;
//        //    Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

//        //    // 绘制弹幕本体
//        //    Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None);

//        //    Main.spriteBatch.End();

//        //    // 重新开启普通绘制，让其他游戏元素不受影响
//        //    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

//        //    if (DrawFlashTimer > 0)
//        //    {
//        //        float opacity = 1f - ((27 - DrawFlashTimer) / 27f);
//        //        Main.EntitySpriteDraw(texture, drawPosition, null, Color.White * opacity, 0f, origin, 1.1f, SpriteEffects.None);
//        //        DrawFlashTimer--;
//        //    }
//        //}

//        public override bool PreDraw(ref Color lightColor)
//        {
//            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
//            Vector2 origin = texture.Size() * 0.5f;
//            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

//            // **充能光晕效果**
//            float chargeOffset = 3f;
//            Color chargeColor = GetColor() * 0.8f; // **颜色由子类提供**
//            chargeColor.A = 0;

//            float rotation = Projectile.rotation;
//            SpriteEffects direction = SpriteEffects.None;

//            // **绘制充能光晕**
//            for (int i = 0; i < 8; i++)
//            {
//                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
//                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
//            }

//            // **绘制投射物本体**
//            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

//            return false;
//        }

//        // **子类提供悬停位置**
//        protected abstract Vector2 GetIdleOffset();

//        // **子类提供粒子颜色**
//        protected abstract Color GetColor();

//        // **让子类提供默认朝向**
//        //protected abstract float GetDefaultRotation(); 

//    }
//}
