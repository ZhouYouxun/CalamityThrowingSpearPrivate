using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using CalamityThrowingSpear.Weapons.ChangedWeapons.EAfterDog.DragonRageC;
using Terraria.GameInput;

namespace CalamityThrowingSpear.Players
{
    public class DragonRageJavPlayer : ModPlayer
    {
        private bool hasSummonedProjectile = false;

        public override void PostUpdate()
        {
            bool holdingDragonRageJav = Player.HeldItem.type == ModContent.ItemType<DragonRageJav>();

            if (holdingDragonRageJav)
            {
                // 检查玩家是否已经拥有 DragonRageJavPROJ
                bool projectileExists = false;
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<DragonRageJavPROJ>())
                    {
                        projectileExists = true;
                        break;
                    }
                }

                // 如果没有弹幕，则创建一个新的
                if (!projectileExists)
                {
                    Projectile.NewProjectile(
                        Player.GetSource_FromThis(),
                        Player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<DragonRageJavPROJ>(),
                        Player.HeldItem.damage,
                        Player.HeldItem.knockBack,
                        Player.whoAmI
                    );
                    hasSummonedProjectile = true;
                }
            }
            else
            {
                // 如果玩家切换武器，不再手持 DragonRageJav，则移除该弹幕
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<DragonRageJavPROJ>())
                    {
                        proj.Kill();
                    }
                }
                hasSummonedProjectile = false;
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // 只有手持 DragonRageJav 时，才进行输入监听
            if (Player.HeldItem.type == ModContent.ItemType<DragonRageJav>())
            {
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<DragonRageJavPROJ>())
                    {
                        DragonRageJavPROJ javProj = proj.ModProjectile as DragonRageJavPROJ;
                        if (javProj != null)
                        {
                            if (Main.mouseLeft)
                                javProj.SetMode(DragonRageJavPROJ.Mode.Charge);
                            else if (Main.mouseRight)
                                javProj.SetMode(DragonRageJavPROJ.Mode.Attract);
                            else
                                javProj.SetMode(DragonRageJavPROJ.Mode.Return);
                        }
                    }
                }
            }
        }
    }
}
