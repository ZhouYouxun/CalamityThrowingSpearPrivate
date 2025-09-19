using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Particles;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.Laser
{
    internal class TEM00LeftSuperLazer : BaseLaserbeamProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";

        public int OwnerIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override float MaxScale => 1.2f;
        public override float MaxLaserLength => 2000f;
        public override float Lifetime => 180; // 持续 X 帧
        public override Color LaserOverlayColor => new Color(90, 200, 255, 130); // 科技蓝(带Alpha)
        public override Color LightCastColor => Color.White;
        public override Texture2D LaserBeginTexture => Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        public override Texture2D LaserMiddleTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/AresLaserBeamMiddle", AssetRequestMode.ImmediateLoad).Value;
        public override Texture2D LaserEndTexture => ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/AresLaserBeamEnd", AssetRequestMode.ImmediateLoad).Value;
        public override string Texture => "CalamityMod/Projectiles/Boss/AresLaserBeamStart";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 10000;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180000;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 1; // 无敌帧冷却时间为1帧
        }



        // 修改大激光锚点跟随弹幕位置保持同步
        public override void AttachToSomething()
        {
            // ai[0] 存放了父弹幕索引（我们在生成时写入了 this.whoAmI）
            int ownerIndex = OwnerIndex; // 读取 ai[0]
            if (!ownerIndex.WithinBounds(Main.maxProjectiles))
            {
                Projectile.Kill();
                return;
            }

            Projectile ownerProj = Main.projectile[ownerIndex];
            // 兼容原先的 NuclearFuelRodPROJ，也兼容新的父弹幕 TEM00Left
            int nuclearType = ModContent.ProjectileType<NuclearFuelRodPROJ>();
            int leftType = ModContent.ProjectileType<CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.TEM00.TEM00Left>(); // 注意命名空间/类名需与项目一致

            // 如果父弹幕存在且是允许的类型 → 绑定位置与方向
            if (ownerProj != null && ownerProj.active && (ownerProj.type == nuclearType || ownerProj.type == leftType))
            {
                // 这里把激光顶点贴到父弹幕的“枪口”位置
                // 你的父弹幕使用 rotation + PiOver4 偏移作为枪头方向，上面的实现参考了你原始实现
                Projectile.Center = ownerProj.Center + (ownerProj.rotation - MathHelper.PiOver4).ToRotationVector2() * 48f;
                Projectile.rotation = (ownerProj.rotation - MathHelper.PiOver4); // 完全匹配父弹幕枪头方向
            }
            else
            {
                // 父弹幕不存在/类型不匹配 → 激光自毁，确保不会漂浮在空中
                Projectile.Kill();
            }
        }



        public override void UpdateLaserMotion()
        {
            // 找到最近的敌人
            NPC target = Projectile.Center.ClosestNPCAt(1500f);
            if (target != null)
            {
                // 计算目标方向
                float targetRotation = (target.Center - Projectile.Center).ToRotation();
                float maxTurnSpeed = MathHelper.ToRadians(3f); // 限制每帧最大旋转 3°

                // **平滑转向目标**
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, maxTurnSpeed);
                Projectile.velocity = Projectile.rotation.ToRotationVector2(); // 让激光朝向目标
            }
        }





        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 120);
        }

        public override void ExtraBehavior()
        {
            // ===== 科技蓝主光照 =====
            Lighting.AddLight(Projectile.Center, 0.10f, 0.28f, 0.55f); // 柔和科技蓝环境光

            // ===== 预计算束体信息 =====
            if (Projectile.velocity == Vector2.Zero)
                return; // 安全保护：没有方向时不生成粒子

            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 nrm = dir.RotatedBy(MathHelper.PiOver2); // 右手法线（±就是两侧边缘）
            float halfWidth = (Projectile.scale * Projectile.width + 180f) * 0.5f; // 与 LaserWidthFunction 对齐（你的宽函数）

            // 沿着光束取样的步长（越小越密）
            float step = 56f; // 经验值：适中密度（可 40~72 调整）
            int sampleCount = (int)(LaserLength / step);
            sampleCount = Utils.Clamp(sampleCount, 3, 24); // 防失控

            // 末端位置（便于在端点加额外亮点）
            Vector2 endPos = Projectile.Center + dir * LaserLength;

            // ===== 1) 光束“方边”——SquareParticle 落在两侧边缘 =====
            for (int i = 1; i <= sampleCount; i++)
            {
                if (!Main.rand.NextBool(5)) // 1/5 概率生成（节流）
                    continue;

                float t = i / (float)(sampleCount + 1);
                Vector2 basePos = Vector2.Lerp(Projectile.Center, endPos, t);

                // 在 ±法线方向偏移到“边缘”，做几何轮廓
                Vector2 leftEdge = basePos - nrm * halfWidth * Main.rand.NextFloat(0.88f, 1.08f);
                Vector2 rightEdge = basePos + nrm * halfWidth * Main.rand.NextFloat(0.88f, 1.08f);

                // 让方块粒子沿着“边缘切线”轻漂，像是能量流经棱边
                Vector2 glideVelL = dir * Main.rand.NextFloat(0.4f, 1.0f) + nrm * Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 glideVelR = dir * Main.rand.NextFloat(0.4f, 1.0f) + nrm * Main.rand.NextFloat(-0.3f, 0.3f);

                // 左侧
                SquareParticle sqL = new SquareParticle(
                    leftEdge,
                    glideVelL,
                    false,
                    24 + Main.rand.Next(8),             // 24~31 帧
                    1.6f + Main.rand.NextFloat(0.7f),   // 1.6~2.3
                    new Color(90, 200, 255) * 1.35f     // 科技蓝，略增亮
                );
                GeneralParticleHandler.SpawnParticle(sqL);

                // 右侧
                if (Main.rand.NextBool()) // 1/2 再补一侧，避免过密
                {
                    SquareParticle sqR = new SquareParticle(
                        rightEdge,
                        glideVelR,
                        false,
                        24 + Main.rand.Next(8),
                        1.6f + Main.rand.NextFloat(0.7f),
                        new Color(90, 200, 255) * 1.35f
                    );
                    GeneralParticleHandler.SpawnParticle(sqR);
                }
            }

            // ===== 2) 光束“脉冲核”——GlowOrb 沿中心线/端点闪烁 =====
            // 中轴脉冲：稀疏而快消，体现能量跳动
            if (Main.rand.NextBool(3))
            {
                float centerT = Main.rand.NextFloat(0.08f, 0.92f);
                Vector2 pulsePos = Vector2.Lerp(Projectile.Center, endPos, centerT);
                GlowOrbParticle orb = new GlowOrbParticle(
                    pulsePos,
                    Vector2.Zero,
                    false,
                    5,                    // 快速消散
                    0.95f + Main.rand.NextFloat(0.25f),
                    new Color(170, 235, 255), // 偏白的浅青蓝
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(orb);
            }

            // 端点高亮：每隔几帧给端点加一个“爆点”，强调终端能量
            if (Main.rand.NextBool(4))
            {
                GlowOrbParticle tip = new GlowOrbParticle(
                    endPos,
                    -dir * Main.rand.NextFloat(0.25f, 0.6f), // 轻微回喷
                    false,
                    7,
                    1.15f + Main.rand.NextFloat(0.25f),
                    new Color(120, 210, 255),
                    true,
                    false,
                    true
                );
                GeneralParticleHandler.SpawnParticle(tip);
            }
        }


        public float LaserWidthFunction(float _) => Projectile.scale * Projectile.width + 180;

        public static Color LaserColorFunction(float completionRatio)
        {
            // 轻呼吸 + 扭曲，色相在浅青(靠近白) 与 天蓝之间摆动
            float osc = (float)Math.Sin(Main.GlobalTimeWrappedHourly * -3.2f + completionRatio * 23f) * 0.5f + 0.5f;
            Color c1 = new Color(170, 235, 255);  // 浅青蓝（近白的科技光）
            Color c2 = new Color(70, 160, 255);   // 天蓝（偏冷，金属感）
            return Color.Lerp(c1, c2, osc * 0.75f);
        }


        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity == Vector2.Zero)
                return false;

            Vector2 laserEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(Projectile.Center, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            GameShaders.Misc["CalamityMod:ArtemisLaser"].UseColor(new Color(90, 200, 255));
            GameShaders.Misc["CalamityMod:ArtemisLaser"].UseImage1("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ArtemisLaser"].UseImage2("Images/Misc/Perlin");

            PrimitiveRenderer.RenderTrail(baseDrawPoints, new(LaserWidthFunction, LaserColorFunction, shader: GameShaders.Misc["CalamityMod:ArtemisLaser"]), 64);
            return false;
        }


    }
}
