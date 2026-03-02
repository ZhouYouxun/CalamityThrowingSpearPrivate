using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace CalamityThrowingSpear
{
    public static class ProjectileExtensions
    {
        public static void StickyProjAI(this Projectile projectile, int timeLeft, bool findNewNPC = false)
        {
            if (projectile.ai[0] == 1f)
            {
                int seconds = timeLeft;
                bool killProj = false;
                bool spawnDust = false;

                projectile.tileCollide = false;

                projectile.localAI[0]++;
                if (projectile.localAI[0] % 30f == 0f)
                {
                    spawnDust = true;
                }

                int npcIndex = (int)projectile.ai[1];
                NPC npc = Main.npc[npcIndex];

                if (projectile.localAI[0] >= (float)(60 * seconds))
                {
                    killProj = true;
                }
                else if (!npcIndex.WithinBounds(Main.maxNPCs))
                {
                    killProj = true;
                }
                else if (npc.active && !npc.dontTakeDamage)
                {
                    projectile.Center = npc.Center - projectile.velocity * 2f;
                    projectile.gfxOffY = npc.gfxOffY;

                    if (spawnDust)
                    {
                        npc.HitEffect(0, 1.0);
                    }
                }
                else
                {
                    killProj = true;
                }

                if (killProj)
                {
                    if (findNewNPC)
                        projectile.ai[0] = 0f;
                    else
                        projectile.Kill();
                }
            }
        }


        public static void ModifyHitNPCSticky(this Projectile projectile, int maxStick)
        {
            Player player = Main.player[projectile.owner];
            Rectangle myRect = projectile.Hitbox;

            if (projectile.owner == Main.myPlayer)
            {
                for (int npcIndex = 0; npcIndex < Main.maxNPCs; npcIndex++)
                {
                    NPC npc = Main.npc[npcIndex];

                    if (npc.active && !npc.dontTakeDamage &&
                        ((projectile.friendly && (!npc.friendly ||
                        (npc.type == NPCID.Guide && projectile.owner < Main.maxPlayers && player.killGuide) ||
                        (npc.type == NPCID.Clothier && projectile.owner < Main.maxPlayers && player.killClothier))) ||
                        (projectile.hostile && npc.friendly && !npc.dontTakeDamageFromHostiles)) &&
                        (projectile.owner < 0 || npc.immune[projectile.owner] == 0 || projectile.maxPenetrate == 1))
                    {
                        if (npc.noTileCollide || !projectile.ownerHitCheck)
                        {
                            bool stickingToNPC;

                            if (npc.type == NPCID.SolarCrawltipedeTail)
                            {
                                Rectangle rect = npc.Hitbox;
                                int crawltipedeHitboxMod = 8;
                                rect.X -= crawltipedeHitboxMod;
                                rect.Y -= crawltipedeHitboxMod;
                                rect.Width += crawltipedeHitboxMod * 2;
                                rect.Height += crawltipedeHitboxMod * 2;
                                stickingToNPC = projectile.Colliding(myRect, rect);
                            }
                            else
                            {
                                stickingToNPC = projectile.Colliding(myRect, npc.Hitbox);
                            }

                            if (stickingToNPC)
                            {
                                if (npc.reflectsProjectiles && projectile.CanBeReflected())
                                {
                                    npc.ReflectProjectile(projectile);
                                    return;
                                }

                                projectile.ai[0] = 1f;
                                projectile.ai[1] = npcIndex;

                                projectile.velocity = (npc.Center - projectile.Center) * 0.75f;
                                projectile.netUpdate = true;

                                Point[] attached = new Point[maxStick];
                                int projCount = 0;

                                for (int projIndex = 0; projIndex < Main.maxProjectiles; projIndex++)
                                {
                                    Projectile proj = Main.projectile[projIndex];

                                    if (projIndex != projectile.whoAmI &&
                                        proj.active &&
                                        proj.owner == Main.myPlayer &&
                                        proj.type == projectile.type &&
                                        proj.ai[0] == 1f &&
                                        proj.ai[1] == npcIndex)
                                    {
                                        attached[projCount++] = new Point(projIndex, proj.timeLeft);

                                        if (projCount >= attached.Length)
                                            break;
                                    }
                                }

                                if (projCount >= attached.Length)
                                {
                                    int lowestTimeIndex = 0;

                                    for (int i = 1; i < attached.Length; i++)
                                    {
                                        if (attached[i].Y < attached[lowestTimeIndex].Y)
                                            lowestTimeIndex = i;
                                    }

                                    Main.projectile[attached[lowestTimeIndex].X].Kill();
                                }
                            }
                        }
                    }
                }
            }
        }




    }
}