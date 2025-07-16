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
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Audio;
using CalamityMod.Particles;


namespace CalamityThrowingSpear.Weapons.NewWeapons.EAfterDog.SoulSeekerJav
{
    public class SoulSeekerJavPROJ : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.NewWeapons.EAfterDog";
        public override string Texture => "CalamityThrowingSpear/Weapons/NewWeapons/EAfterDog/SoulSeekerJav/SoulSeekerJav";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 原有的拖尾效果
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);

            // 获取 Sparkle2 贴图
            Texture2D sparkleTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HealingPlus").Value;

            // 枪头与枪尾位置
            Vector2 gunTip = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;
            Vector2 gunTail = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f * 3f;

            // 设定粒子数量（8 ~ 12）
            int particleCount = 10;

            // 逐步在枪身上生成粒子
            for (int i = 0; i < particleCount; i++)
            {
                // 计算插值位置，使粒子均匀分布在枪头和枪尾之间
                float t = i / (float)(particleCount - 1);
                Vector2 position = Vector2.Lerp(gunTail, gunTip, t);

                // **增加左右随机摆动**
                float wiggleOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + i) * 4f; // 左右摆动幅度
                position += Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * wiggleOffset;

                // **设置颜色**（血红色+透明度）
                float alphaFactor = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + i) * 0.2f + 0.8f; // 呼吸灯透明度
                Color sparkleColor = Color.Red * alphaFactor;
                sparkleColor.A = (byte)(sparkleColor.A * 0.6f); // 透明度降低

                // **呼吸灯缩放**
                float scaleFactor = 1f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f + i);

                // **绘制粒子**
                Main.EntitySpriteDraw(
                    sparkleTex,
                    position - Main.screenPosition,
                    null,
                    sparkleColor,
                    0f, // 不需要旋转
                    sparkleTex.Size() * 0.5f,
                    scaleFactor * 0.5f, // 适当缩小
                    SpriteEffects.None,
                    0
                );
            }

            return false; // 由于已经手动绘制，不需要默认绘制
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            // 遍历所有小鸟，发送开火通知
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Projectile.owner && proj.type == ModContent.ProjectileType<SoulSeekerJavBRID>())
                {
                    // 调用小鸟的接收命令函数
                    (proj.ModProjectile as SoulSeekerJavBRID)?.ReceiveFireOrder(Projectile.damage);
                }
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3; // 只允许一次伤害
            Projectile.timeLeft = 360;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true; 
            Projectile.extraUpdates = 2; // 额外更新次数
            Projectile.usesLocalNPCImmunity = true; // 弹幕使用本地无敌帧
            Projectile.localNPCHitCooldown = 14; // 无敌帧冷却时间为14帧
        }
        private HashSet<Point> visitedTiles = new();

        public override void AI()
        {
            // 保持弹幕旋转（对于倾斜走向的弹幕而言）
            if (isInSpiralMode)
            {
                // 蚊香状态，按公转方向 + 90度旋转
                Vector2 offset = spiralAngle.ToRotationVector2() * spiralRadius;
                Projectile.rotation = offset.ToRotation() + MathHelper.PiOver4 + MathHelper.PiOver4 + MathHelper.PiOver4;
            }
            else
            {
                // 正常状态，按速度方向旋转
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }

            // Lighting - 添加红色光源，光照强度为 0.55
            Lighting.AddLight(Projectile.Center, Color.Red.ToVector3() * 0.55f);


            // 飞行过程中的粒子特效
            {

                // 递减计时器
                if (triangleTimer > 0) triangleTimer--;
                if (squareTimer > 0) squareTimer--;
                if (arrowTimer > 0) arrowTimer--;

                // 触发特效：三角形（90 帧）
                if (triangleTimer <= 0)
                {
                    CreateShapeEffect(Projectile.Center, DustID.RedTorch, Color.Red, 12f, "Triangle");
                    triangleTimer = 9;
                }

                // 触发特效：方形（60 帧）
                if (squareTimer <= 0)
                {
                    CreateShapeEffect(Projectile.Center, 105, Color.Orange, 16f, "Square");
                    squareTimer = 6;
                }

                // 触发特效：箭头（30 帧）
                if (arrowTimer <= 0)
                {
                    CreateShapeEffect(Projectile.Center, DustID.HealingPlus, Color.Yellow, 18f, "Arrow");
                    arrowTimer = 3;
                }
            }



            // 触发蚊香型扩散
            if (isInSpiralMode && trackedNPC >= 0 && Main.npc[trackedNPC].active && !Main.npc[trackedNPC].dontTakeDamage)
            {
                NPC target = Main.npc[trackedNPC];

                // 蚊香角度递增（加速旋转）
                spiralAngle += 0.1f + spiralAngle * 0.002f;

                // 蚊香半径扩大（缓慢外扩）
                spiralRadius += 5f;

                // 计算位置
                Vector2 offset = spiralAngle.ToRotationVector2() * spiralRadius;
                Projectile.Center = target.Center + offset;

                // 保持不动
                Projectile.velocity = Vector2.Zero;

                // 发射弹幕计时器逻辑
                fireTimer++;
                if (fireTimer >= fireInterval)
                {
                    fireTimer = 0;

                    // 发射子弹
                    Vector2 fireVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, fireVel,
                        ModContent.ProjectileType<SoulSeekerJavBRIDFire>(), (int)(Projectile.damage * 0.33), 0f, Projectile.owner);

                    SoundEngine.PlaySound(SoundID.NPCDeath26, Projectile.Center);

                    // 冷却时间逐步缩短，最短为6帧
                    fireInterval = Math.Max(fireInterval - fireIntervalDecrement, 6);
                }

            }

            //// ✅ 检测当前中心是否在实心方块内
            //Point tileCoords = Projectile.Center.ToTileCoordinates();
            //Tile tile = Framing.GetTileSafely(tileCoords.X, tileCoords.Y);

            //if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
            //{
            //    // ✅ 在当前中心位置生成一个熔岩粒子
            //    RancorLavaMetaball.SpawnParticle(Projectile.Center, Main.rand.NextFloat(70f, 100f));
            //}

            // ✅ 获取当前位置对应的 tile 坐标
            Point tileCoords = Projectile.Center.ToTileCoordinates();

            // ✅ 如果没访问过
            if (!visitedTiles.Contains(tileCoords))
            {
                Tile tile = Framing.GetTileSafely(tileCoords.X, tileCoords.Y);

                // ✅ 如果是合法实心方块
                if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    visitedTiles.Add(tileCoords); // 记录访问，避免重复

                    // ✅ 在该 tile 的中心生成 2~3 个 RancorLavaMetaball
                    for (int i = 0; i < Main.rand.Next(2, 4); i++)
                    {
                        Vector2 pos = tileCoords.ToVector2() * 16f + Main.rand.NextVector2Circular(4f, 4f);
                        float size = Main.rand.NextFloat(60f, 100f);
                        RancorLavaMetaball.SpawnParticle(pos, size);
                    }
                }
            }

        }
        // 计时器变量
        private int triangleTimer = 0;
        private int squareTimer = 0;
        private int arrowTimer = 0;

        // 形态切换变量
        private bool isInSpiralMode = false;
        private int trackedNPC = -1;
        private float spiralAngle = 0f;
        private float spiralRadius = 24f;
        private int fireTimer = 0;

        // 蚊香形态攻击的参数
        private int fireInterval = 30; // 初始冷却时间（可调整）
        private int fireIntervalDecrement = 4; // 每次减少的冷却时间（可调整）


        private void CreateShapeEffect(Vector2 center, int dustType, Color color, float size, string shape)
        {
            int numParticles = 8; // 形状的点数
            for (int i = 0; i < numParticles; i++)
            {
                float angle = 0;
                switch (shape)
                {
                    case "Triangle":
                        angle = MathHelper.PiOver2 + i * MathHelper.TwoPi / 3f; // 120°间隔
                        break;
                    case "Square":
                        angle = i * MathHelper.PiOver2; // 90°间隔
                        break;
                    case "Arrow":
                        angle = i == 0 ? 0 : (i % 2 == 0 ? MathHelper.PiOver4 : -MathHelper.PiOver4); // 箭头形
                        break;
                }

                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * size;
                Dust dust = Dust.NewDustPerfect(center + offset, dustType, Vector2.Zero, 0, color);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.95f, 1.35f);
                dust.velocity = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f);
            }
        }


        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            SoundEngine.PlaySound(SoundID.Item113 with { Volume = 1.2f, Pitch = -0.0f }, Projectile.Center);
            // 检查当前小鸟数量
            int currentBirdCount = Main.projectile.Count(p => p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<SoulSeekerJavBRID>());
            if (currentBirdCount >= 10)
            {
                // 小鸟数量达到上限时，增加x倍伤害
                modifiers.FinalDamage *= 2.22f;
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            {
                //// 目标切换：寻找最近敌人
                //NPC closestNPC = Main.npc
                //    .Where(npc => npc.active && !npc.friendly && npc.life > 0 && npc.whoAmI != target.whoAmI)
                //    .Where(npc => Vector2.Distance(npc.Center, Projectile.Center) > 15 * 16 && Vector2.Distance(npc.Center, Projectile.Center) < 150 * 16)
                //    .OrderBy(npc => Vector2.Distance(npc.Center, Projectile.Center))
                //    .FirstOrDefault();

                //if (closestNPC != null)
                //{
                //    Vector2 direction = closestNPC.Center - Projectile.Center;
                //    Projectile.velocity = Vector2.Normalize(direction) * Projectile.velocity.Length();
                //}
            }

            // 在首次命中敌人的时候触发蚊香形状
            Projectile.friendly = false; // 禁用伤害
            Projectile.timeLeft = 200; // 设定一个倒计时

            trackedNPC = target.whoAmI; // 自定义字段记录目标
            spiralAngle = 0f;
            spiralRadius = 24f;
            fireTimer = 0;
            isInSpiralMode = true;



            {
                // 计算当前小鸟数量
                int existingBirdCount = Main.projectile.Count(p => p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<SoulSeekerJavBRID>());

                if (existingBirdCount < 10)
                {
                    // 生成新小鸟
                    Vector2 spawnPosition = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * (3 * 16);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, -Projectile.velocity * 1f, ModContent.ProjectileType<SoulSeekerJavBRID>(), Projectile.damage, 0f, Projectile.owner);

                }
                // ✦ 保留的 else 分支，未来扩展用
                else
                {
                    // hit.damage *= 2.22f;
                }



                // ✦ 屏幕震动效果，根据当前已有小鸟数量线性增强（不管有没有生成新的）
                {
                    float baseShake = 2f; // 最低震动
                    float shakePower = baseShake + existingBirdCount * 0.6f;
                    float distanceFactor = Utils.GetLerpValue(1000f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
                    Main.LocalPlayer.Calamity().GeneralScreenShakePower = Math.Max(
                        Main.LocalPlayer.Calamity().GeneralScreenShakePower,
                        shakePower * distanceFactor
                    );
                }

                // 正/倒三角形特效
                CreateTriangleMetaballEffect(target.Center, existingBirdCount < 10);
                CreateTriangleParticleEffect(target.Center, existingBirdCount < 10);
            }
        }
        private void CreateTriangleParticleEffect(Vector2 center, bool isNormalTriangle)
        {
            int numParticles = 3; // 三角形的顶点数
            float baseAngle = isNormalTriangle ? MathHelper.PiOver2 : -MathHelper.PiOver2;

            for (int i = 0; i < numParticles; i++)
            {
                float angle = baseAngle + i * MathHelper.TwoPi / 3f;
                Vector2 start = angle.ToRotationVector2();
                Vector2 next = (angle + MathHelper.TwoPi / 3f).ToRotationVector2();

                for (int j = 0; j < 40; j++)
                {
                    Dust triangleDust = Dust.NewDustPerfect(center, 267);
                    triangleDust.scale = 2.5f;
                    triangleDust.velocity = Vector2.Lerp(start, next, j / 40f) * 16f;
                    triangleDust.color = Color.Crimson;
                    triangleDust.noGravity = true;
                }
            }
        }
        private void CreateTriangleMetaballEffect(Vector2 center, bool isNormalTriangle)
        {
            int numParticlesPerEdge = 15; // 每条边 15 个粒子
            float baseAngle = isNormalTriangle ? MathHelper.PiOver2 : -MathHelper.PiOver2;

            for (int i = 0; i < 3; i++)
            {
                float angle1 = baseAngle + i * MathHelper.TwoPi / 3f;
                float angle2 = baseAngle + (i + 1) * MathHelper.TwoPi / 3f;

                Vector2 start = angle1.ToRotationVector2() * 30f;
                Vector2 end = angle2.ToRotationVector2() * 30f;

                for (int j = 0; j < numParticlesPerEdge; j++)
                {
                    Vector2 position = Vector2.Lerp(start, end, j / (float)(numParticlesPerEdge - 1)) + center;
                    Vector2 velocity = (position - center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);
                    float radius = Main.rand.NextFloat(20f, 40f);

                    GruesomeMetaball.SpawnParticle(position, velocity, radius);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            int particleCount = 40;
            float baseAngle = Projectile.velocity.ToRotation();
            float spreadAngle = MathHelper.ToRadians(45f); // 45度扇形

            for (int i = 0; i < particleCount; i++)
            {
                float randomAngle = Main.rand.NextFloat(-spreadAngle, spreadAngle);
                Vector2 direction = (baseAngle + randomAngle).ToRotationVector2();
                Vector2 velocity = -direction * Main.rand.NextFloat(2f, 6f); // 朝向自己
                Vector2 spawnPosition = Projectile.Center + direction * Main.rand.NextFloat(20f, 60f);
                float radius = Main.rand.NextFloat(40f, 80f);
                GruesomeMetaball.SpawnParticle(spawnPosition, velocity, radius);
            }

            for (int i = 0; i < 16; i++)
            {
                Vector2 spawnOffset = Main.rand.NextVector2Circular(32f, 32f);
                float radius = Main.rand.NextFloat(60f, 100f);

                RancorLavaMetaball.SpawnParticle(
                    Projectile.Center + spawnOffset,
                    radius
                );
            }


        }


        //public override bool OnTileCollide(Vector2 oldVelocity)
        //{


        //}


    }
}