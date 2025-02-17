using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.ChangedWeapons.CPreMoodLord.TenebreusTidesC;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;

namespace CalamityThrowingSpear.Weapons.NewWeapons.ZOthers.GlobalStorm
{
    internal class GlobalStormRightHoldOut : ModProjectile
    {
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/ZOthers/GlobalStorm/GlobalStorm";

        public enum BehaviorState
        {
            Aim,
            //Dash
        }

        public BehaviorState CurrentState
        {
            get => (BehaviorState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1500;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 14;
        }
        public Player Owner => Main.player[Projectile.owner];

        public override void AI()
        {
            switch (CurrentState)
            {
                case BehaviorState.Aim:
                    DoBehavior_Aim();
                    break;
                    //case BehaviorState.Dash:
                    //    DoBehavior_Dash();
                    //    break;
            }
        }
        //public override void OnSpawn(IEntitySource source)
        //{
        //    if (Main.myPlayer == Projectile.owner)
        //    {
        //        Vector2 spawnPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(5f, 5f);
        //        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, Vector2.Zero, ModContent.ProjectileType<GlobalStormRightMagic>(), 0, 0, Projectile.owner);
        //    }
        //}

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

            // 生成枪头烟雾
            Vector2 smokePosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 4.5f + Main.rand.NextVector2Circular(5f, 5f);
            Particle smoke = new HeavySmokeParticle(
                smokePosition,
                Vector2.UnitY * -1 * Main.rand.NextFloat(3f, 7f),
                Color.Lerp(Color.DarkBlue, Color.CadetBlue, 0.5f),
                Main.rand.Next(30, 60),
                Main.rand.NextFloat(0.25f, 0.5f),
                1.0f,
                MathHelper.ToRadians(Main.rand.NextFloat(-5f, 5f)),
                true
            );
            GeneralParticleHandler.SpawnParticle(smoke);


            if (lightBallCounter < 9 && Projectile.ai[1] % 20 == 0) // 每 20 帧生成一个光球
            {
                GenerateLightBall();
            }

            // 检测松手
            Player player = Main.player[Projectile.owner];
            if (!player.Calamity().mouseRight)
            {
                Projectile.netUpdate = true;
                Projectile.Kill();
                SoundEngine.PlaySound(SoundID.Item79, Projectile.position);
            }
        }

        private void GenerateLightBall()
        {
            if (lightBallCounter >= 9) return; // 最多生成 9 个

            float baseAngle = 90f;
            float angleStep = 10f * (lightBallCounter % 2 == 0 ? 1 : -1);

            float angle = MathHelper.ToRadians(baseAngle + (lightBallCounter / 2) * angleStep);
            Vector2 spawnPos = Owner.Center + new Vector2(8 * 16 * (float)Math.Cos(angle), 8 * 16 * (float)Math.Sin(angle));

            int proj = Projectile.NewProjectile(Owner.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<GlobalStormLightBall>(), 0, 0, Owner.whoAmI);

            if (proj >= 0 && proj < Main.maxProjectiles && Main.projectile[proj].active)
            {
                Main.projectile[proj].velocity = Vector2.Zero; // 确保初始速度为零
            }

            SoundEngine.PlaySound(SoundID.Item9, spawnPos);
            lightBallCounter++;

            if (lightBallCounter == 9)
            {
                // 触发特殊特效（屏幕闪烁 + 震动）
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = 5f;
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustPos = Owner.Center + Main.rand.NextVector2Circular(8 * 16, 8 * 16);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.Electric, Vector2.Zero, 100, Color.Cyan, 2f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item92, Owner.Center);
            }
        }

        private int lightBallCounter = 0;


        public override void OnKill(int timeLeft)
        {

        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }
    }
}