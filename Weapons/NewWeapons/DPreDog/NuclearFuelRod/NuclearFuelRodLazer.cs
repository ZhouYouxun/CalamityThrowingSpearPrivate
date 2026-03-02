using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Projectiles.BaseProjectiles;
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
using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.NuclearFuelRod
{
    public class NuclearFuelRodLazer : BaseLaserbeamProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.DPreDog";

        public int OwnerIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override float MaxScale => 1.2f;
        public override float MaxLaserLength => 2000f;
        public override float Lifetime => 180; // 持续 X 帧
        public override Color LaserOverlayColor => new Color(150, 255, 150, 100); // 荧光绿色
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
            Projectile.timeLeft = 180;
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 1; // 无敌帧冷却时间为1帧
        }



        // 修改大激光锚点跟随弹幕位置保持同步
        public override void AttachToSomething()
        {
            if (OwnerIndex.WithinBounds(Main.maxProjectiles))
            {
                Projectile ownerProj = Main.projectile[OwnerIndex];
                if (ownerProj.active && ownerProj.type == ModContent.ProjectileType<NuclearFuelRodPROJ>())
                {
                    // 精确绑定枪头位置
                    Projectile.Center = ownerProj.Center + (ownerProj.rotation - MathHelper.PiOver4).ToRotationVector2() * 48f;
                    // 强制同步方向
                    Projectile.rotation = (ownerProj.rotation - MathHelper.PiOver4); // 完全匹配父弹幕枪头方向
                }
                else
                {
                    Projectile.Kill(); // 父弹幕消失自动移除激光
                }
            }
            else
            {
                Projectile.Kill(); // 防止越界
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



        public float LaserWidthFunction(float completionRatio, Vector2 vertexPos) => Projectile.scale * Projectile.width + 180;

        public static Color LaserColorFunction(float completionRatio, Vector2 vertexPos)
        {
            float colorInterpolant = (float)Math.Sin(Main.GlobalTimeWrappedHourly * -3.2f + completionRatio * 23f) * 0.5f + 0.5f;
            return Color.Lerp(Color.LimeGreen, Color.Green, colorInterpolant * 0.67f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity == Vector2.Zero)
                return false;

            Vector2 laserEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * LaserLength;
            Vector2[] baseDrawPoints = new Vector2[8];
            for (int i = 0; i < baseDrawPoints.Length; i++)
                baseDrawPoints[i] = Vector2.Lerp(Projectile.Center, laserEnd, i / (float)(baseDrawPoints.Length - 1f));

            GameShaders.Misc["CalamityMod:ArtemisLaser"].UseColor(Color.LimeGreen);
            GameShaders.Misc["CalamityMod:ArtemisLaser"].UseImage1("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ArtemisLaser"].UseImage2("Images/Misc/Perlin");

            PrimitiveRenderer.RenderTrail(baseDrawPoints, new(LaserWidthFunction, LaserColorFunction, shader: GameShaders.Misc["CalamityMod:ArtemisLaser"]), 64);
            return false;
        }


    }
}
