using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.CConcept;

namespace CalamityThrowingSpear.Weapons.NewWeapons.DPreDog.Sunset.PPlayer
{
    internal class ConceptGNSpitCtrl : ModPlayer
    {
        // —— 生成节奏与总数 —— //
        private const int BladeTotal = 10;  // 一共 10 个
        private const int SpawnInterval = 8;  // 每两个之间相隔 X 帧

        // —— 运行时计数 —— //
        private int spawnTimer = 0;

        public override void PostUpdate()
        {
            // 只在：手持 Sunset 且 Sunset 处于 C 形态 时触发
            bool holdingC = false;
            if (Player.HeldItem?.ModItem is Sunset sunsetItem) // Sunset 来自你的 ModItem
            {
                // 必须是这把武器 & 必须是 C 形态
                holdingC = sunsetItem.IsCMode;
            }

            if (holdingC)
            {
                // 🚩 检查是否已经有魔法阵存在
                bool hasMagic = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active &&
                        proj.type == ModContent.ProjectileType<SunsetCConceptLeftMagic>() &&
                        proj.owner == Player.whoAmI)
                    {
                        hasMagic = true;
                        break;
                    }
                }

                if (!hasMagic) // 🚩 只有在没有魔法阵时才生成刀片
                {
                    int ownedCount = 0;
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.owner == Player.whoAmI &&
                            proj.type == ModContent.ProjectileType<SunsetCConceptLeftNoDamage>() &&
                            proj.ai[0] == -1) // -1 表示玩家跟随模式
                        {
                            ownedCount++;
                        }
                    }

                    if (ownedCount < BladeTotal)
                    {
                        spawnTimer++;
                        if (spawnTimer >= SpawnInterval)
                        {
                            spawnTimer = 0;

                            int index = ownedCount;
                            float angle = MathHelper.TwoPi * index / BladeTotal;
                            Vector2 backCenter = Player.Center - new Vector2(Player.direction, 0f) * 48f;
                            Vector2 offset = angle.ToRotationVector2() * 60f;

                            int projID = Projectile.NewProjectile(
                                Player.GetSource_Misc("SunsetNoDamageBlades"),
                                backCenter + offset,
                                Vector2.Zero,
                                ModContent.ProjectileType<SunsetCConceptLeftNoDamage>(),
                                0, 0f, Player.whoAmI,
                                -1,
                                index
                            );

                            if (projID >= 0)
                            {
                                Projectile child = Main.projectile[projID];
                                child.ai[0] = -1;
                                child.ai[1] = index;
                                child.netUpdate = true;
                            }
                        }
                    }
                    else
                    {
                        spawnTimer = 0;
                    }
                }
            }
            else
            {
                // 不是 C 形态 或 没手持 Sunset → 清理“玩家跟随模式”的刀片
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == Player.whoAmI &&
                        proj.type == ModContent.ProjectileType<SunsetCConceptLeftNoDamage>() &&
                        proj.ai[0] == -1) // 仅清理跟随玩家的这批，不动以后要公转魔法阵的那批
                    {
                        proj.Kill();
                    }
                }
                // 重置生成节奏
                spawnTimer = 0;
            }
        }
    }
}
