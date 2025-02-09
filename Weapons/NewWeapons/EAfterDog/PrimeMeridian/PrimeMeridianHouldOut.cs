using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using CalamityMod;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SawBladeForkHornJav;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian.Zenith;

namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.PrimeMeridian
{
    internal class PrimeMeridianHouldOut : ModProjectile
    {
        private int chargeCounter = 0; // 计数器
        private bool isAttacking = false; // 是否正在攻击
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/PrimeMeridian/PrimeMeridian";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 计算充能强度（0 ~ 1）
            float intensity = isAttacking ? 1f : (chargeCounter / 50f);

            // 读取武器贴图
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            // **白色包边**
            float chargeOffset = 2f * intensity; // 充能越高，包边越强
            Color chargeColor = Color.White * intensity * 0.8f; // **随充能增强**
            chargeColor.A = 0; // **透明度处理，避免过亮**

            // **旋转逻辑**
            float rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            SpriteEffects direction = SpriteEffects.None;

            // **绘制白色包边**
            for (int i = 0; i < 8; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            }

            // **绘制本体**
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);

            return false;
        }

        private readonly List<int> projectileTypes = new List<int>
        {
            ModContent.ProjectileType<PM1CopperDagger>(),
            ModContent.ProjectileType<PM2Enchanted>(),
            ModContent.ProjectileType<PM3StarFury>(),
            ModContent.ProjectileType<PM4Beekeeper>(),
            ModContent.ProjectileType<PM5Seed>(),
            ModContent.ProjectileType<PM6Headless>(),
            ModContent.ProjectileType<PM7Wave>(),
            ModContent.ProjectileType<PM8Crazy>(),
            ModContent.ProjectileType<PM9Cat>(),
            ModContent.ProjectileType<PM10Terra>()
        };

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 155;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public Player Owner => Main.player[Projectile.owner];
        public override void OnSpawn(IEntitySource source)
        {


        }

        public override void AI()
        {
            // 保持弹幕旋转
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // 不断的重置剩余时间
            Projectile.timeLeft = 300;

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
            //Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 1.05f);
            Projectile.Center = Owner.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (Projectile.width * 0.5f);
            Owner.heldProj = Projectile.whoAmI;

            // 枪头位置定义
            //Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(10f, 10f);

            if (!isAttacking)
            {
                chargeCounter++;
                GenerateChargingParticles(); // **保持充能特效**

                if (chargeCounter >= 180)
                {
                    StartAttackSequence();
                    Projectile.ai[0] = 10; // **初始化等待时间**
                    Projectile.ai[1] = 0; // **初始化小弹幕发射计时器**
                }
            }

            if (isAttacking)
            {
                // **等待 10 帧后开始发射小弹幕**
                if (Projectile.ai[0] > 0)
                {
                    Projectile.ai[0]--; // 每帧减少 1
                    return;
                }

                // **小弹幕逐步发射**
                if (Projectile.ai[1] % 5 == 0 && Projectile.ai[1] / 5 < projectileTypes.Count)
                {
                    int i = (int)(Projectile.ai[1] / 5); // 计算当前要发射的弹幕索引

                    float angleOffset = 20f + i;
                    float spacingMultiplier = (i + 2) * 4f;
                    Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f + Main.rand.NextVector2Circular(10f, 10f);
                    Vector2 spawnPos = gunTip - Projectile.velocity.SafeNormalize(Vector2.Zero) * spacingMultiplier;
                    Vector2 velocityRight = Projectile.velocity.RotatedBy(MathHelper.ToRadians(angleOffset)) * 8f;
                    Vector2 velocityLeft = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-angleOffset)) * 8f;

                    // **发射左右对称小弹幕**
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocityRight, projectileTypes[i], Projectile.damage, Projectile.knockBack, Projectile.owner);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, velocityLeft, projectileTypes[i], Projectile.damage, Projectile.knockBack, Projectile.owner);

                    SoundEngine.PlaySound(SoundID.Item113, Projectile.Center);
                }

                // **更新小弹幕发射计时器**
                Projectile.ai[1]++;

                // **所有弹幕发射完毕，结束攻击**
                if (Projectile.ai[1] >= projectileTypes.Count * 5)
                {
                    isAttacking = false;
                    Projectile.ai[1] = 0; // 重置小弹幕计时器
                }
            }


            // 让玩家的双手一直朝向投射物方向，模拟握持长枪
            ManipulatePlayerArmPositions();

            // 检查玩家是否松开鼠标
            if (!Owner.channel)
            {
                Projectile.timeLeft = 50;
                Projectile.Kill(); // 立即销毁投射物
                Projectile.netUpdate = true;
            }
        }
        private void StartAttackSequence()
        {
            isAttacking = true;
            chargeCounter = 0;
            SoundEngine.PlaySound(SoundID.Item68, Projectile.Center);

            // 获取枪口位置
            Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f + Main.rand.NextVector2Circular(10f, 10f);

            // 先发射主激光
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), gunTip, Projectile.velocity * 10f, ModContent.ProjectileType<PrimeMeridianLaser>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

            // **等待 10 帧后再开始发射小弹幕**
            Projectile.ai[0] = 10; // 先等待 10 帧
        }

      
        private void GenerateChargingParticles()
        {
            Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f + Main.rand.NextVector2Circular(10f, 10f);
            for (int i = 0; i < Main.rand.Next(2, 4); i++)
            {
                Vector2 spawnPosition = gunTip + Main.rand.NextVector2Circular(3 * 16, 3 * 16);
                SquishyLightParticle exoEnergy = new SquishyLightParticle(spawnPosition, (gunTip - spawnPosition).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.4f, 1.6f), 0.28f, Color.LimeGreen, 25);
                GeneralParticleHandler.SpawnParticle(exoEnergy);
            }
        }

        public void ManipulatePlayerArmPositions()
        {
            // 让玩家的手臂方向始终朝向长枪的方向
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;

            // 计算双臂应当指向的角度，使其平行向前
            float armRotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // 设置玩家前臂（主手）和后臂（副手）的角度，使其平行前伸
            // 第1个参数设置为正确，意味着它将会使用自定义手臂，设置为错误，则不进行更改
            // 第2个参数决定了伸手臂的长度：【也就是伸出了多少，并不是指的角度】
            // Full（完全伸展，适用于拿长枪、拉弓等）
            // None（不伸展，手臂保持贴近身体）
            // Quarter（25 % 伸展，适用于轻微举起手臂）
            // ThreeQuarters（75 % 伸展，适用于半握持状态）
            // 第3个参数armRotation决定了手臂的弯曲角度，你要想让他平行向前？高举45度？还是往下放？

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRotation);
            //Owner.fullRotation = armRotation;
            //Owner.headRotation = armRotation; // 让玩家的头部旋转
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
          
        }

        public override void OnKill(int timeLeft)
        {


        }
    }
}
