using System;
using System.Collections.Generic;
using CalamityMod;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.ElementalArkJav;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.FinishingTouch;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Revelation;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Sagittarius;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.StarsofDestiny;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Sunset;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.Sunset.DSeed;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.TheLastLance;
using CalamityThrowingSpear.Weapons.DeveloperWeapons.TidalMechanics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ElementalArkItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.ElementalArkJav.ElementalArkJav;
using FinishingTouchItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.FinishingTouch.FinishingTouch;
using RevelationItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.Revelation.Revelation;
using SagittariusItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.Sagittarius.Sagittarius;
using StarsOfDestinyItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.StarsofDestiny.StarsofDestiny;
using SunsetItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.Sunset.Sunset;
using TheLastLanceItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.TheLastLance.TheLastLance;
using TidalMechanicsItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.TidalMechanics.TidalMechanics;

namespace CalamityThrowingSpear.Weapons.DeveloperWeapons
{
    public enum DeveloperWeaponInfoPanelId
    {
        Guide,
        TheLastLance,
        Sagittarius,
        TidalMechanics,
        ElementalArk,
        FinishingTouch,
        Revelation,
        StarsOfDestiny,
        SunsetSeed
    }

    public static class DeveloperWeaponInfoUI
    {
        private static int PanelType => ModContent.ProjectileType<DeveloperWeaponInfoPanel>();

        public static void ApplyInfoItemDefaults(Item item)
        {
            item.useTime = 12;
            item.useAnimation = 12;
            item.useStyle = ItemUseStyleID.HoldUp;
            item.noMelee = true;
            item.autoReuse = false;
            item.shoot = PanelType;
            item.shootSpeed = 0f;
            item.UseSound = null;
        }

        public static bool CanUseInfoItem(Player player, int itemType)
        {
            return Main.myPlayer == player.whoAmI &&
                !Main.mapFullscreen &&
                !Main.blockMouse &&
                !player.mouseInterface &&
                !(Main.playerInventory && Main.HoverItem.type == itemType);
        }

        public static bool ShootInfoPanel(Player player, IEntitySource source, DeveloperWeaponInfoPanelId panelId)
        {
            if (TryCloseExistingPanel(player))
            {
                SoundEngine.PlaySound(SoundID.MenuClose with { Volume = 0.58f, Pitch = 0.05f }, player.Center);
                return false;
            }

            Projectile.NewProjectile(
                source,
                player.Center,
                Vector2.Zero,
                PanelType,
                0,
                0f,
                player.whoAmI,
                0f,
                (float)panelId);

            SoundEngine.PlaySound(SoundID.MenuOpen with { Volume = 0.68f, Pitch = 0.08f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.38f, Pitch = 0.16f }, player.Center);
            return false;
        }

        private static bool TryCloseExistingPanel(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.owner != player.whoAmI || projectile.type != PanelType)
                    continue;

                DeveloperWeaponInfoPanel.RequestClose(projectile);
                return true;
            }

            return false;
        }
    }

    internal sealed class DeveloperWeaponInfoPanel : ModProjectile, ILocalizedModType
    {
        private const int PanelWidth = 560;
        private const int PanelHeight = 356;
        private const int BorderThickness = 3;
        private const int IconFrameSize = 94;
        private const float MaxIconDrawSize = 76f;

        private Vector2 panelTopLeft;
        private bool panelPositionInitialized;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.DeveloperWeapons";

        private bool FadeOut
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }

        private DeveloperWeaponInfoPanelId PanelId => (DeveloperWeaponInfoPanelId)MathHelper.Clamp(Projectile.ai[1], 0f, Entries.Length - 1);
        private DeveloperWeaponInfoEntry Entry => Entries[(int)PanelId];
        private static Rectangle MouseRectangle => new((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 2, 2);

        private static readonly DeveloperWeaponInfoEntry[] Entries =
        {
            new(DeveloperWeaponInfoPanelId.Guide, "Guide", () => ModContent.ItemType<KaiJuXiaoZhiTiao>(), () => ModContent.ItemType<KaiJuXiaoZhiTiao>(), new Color(122, 196, 255), new Color(210, 232, 255)),
            new(DeveloperWeaponInfoPanelId.TheLastLance, "TheLastLance", () => ModContent.ItemType<XiaoZhiTiaoLL>(), () => ModContent.ItemType<TheLastLanceItem>(), new Color(126, 202, 255), new Color(214, 238, 255)),
            new(DeveloperWeaponInfoPanelId.Sagittarius, "Sagittarius", () => ModContent.ItemType<XiaoZhiTiaoSG>(), () => ModContent.ItemType<SagittariusItem>(), new Color(255, 230, 92), new Color(255, 245, 170)),
            new(DeveloperWeaponInfoPanelId.TidalMechanics, "TidalMechanics", () => ModContent.ItemType<XiaoZhiTiaoTM>(), () => ModContent.ItemType<TidalMechanicsItem>(), new Color(86, 192, 255), new Color(165, 234, 255)),
            new(DeveloperWeaponInfoPanelId.ElementalArk, "ElementalArk", () => ModContent.ItemType<XiaoZhiTiaoEA>(), () => ModContent.ItemType<ElementalArkItem>(), new Color(170, 95, 255), new Color(210, 170, 255)),
            new(DeveloperWeaponInfoPanelId.FinishingTouch, "FinishingTouch", () => ModContent.ItemType<XiaoZhiTiaoFT>(), () => ModContent.ItemType<FinishingTouchItem>(), new Color(255, 166, 72), new Color(255, 216, 150)),
            new(DeveloperWeaponInfoPanelId.Revelation, "Revelation", () => ModContent.ItemType<XiaoZhiTiaoRE>(), () => ModContent.ItemType<RevelationItem>(), new Color(158, 162, 174), new Color(224, 226, 232)),
            new(DeveloperWeaponInfoPanelId.StarsOfDestiny, "StarsOfDestiny", () => ModContent.ItemType<XiaoZhiTiaoSoD>(), () => ModContent.ItemType<StarsOfDestinyItem>(), new Color(196, 248, 255), new Color(245, 255, 255)),
            new(DeveloperWeaponInfoPanelId.SunsetSeed, "SunsetSeed", () => ModContent.ItemType<SunsetSEEDItem>(), () => ModContent.ItemType<SunsetItem>(), new Color(255, 146, 74), new Color(255, 224, 132))
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 9999999;
        }

        public override void SetDefaults()
        {
            Projectile.width = PanelWidth;
            Projectile.height = PanelHeight;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.Opacity = 0f;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            DeveloperWeaponInfoEntry entry = Entry;
            if (owner.HeldItem.type != entry.SourceItemType)
                FadeOut = true;

            if (!panelPositionInitialized && Main.myPlayer == Projectile.owner)
            {
                panelTopLeft = GetClampedPanelTopLeftFromCenter(Main.MouseScreen);
                panelPositionInitialized = true;
            }

            Vector2 panelCenter = panelTopLeft + new Vector2(PanelWidth, PanelHeight) * 0.5f;
            Projectile.Center = Main.myPlayer == Projectile.owner ? Main.screenPosition + panelCenter : owner.Center;
            Projectile.timeLeft = 2;
            Projectile.Opacity = MathHelper.Clamp(Projectile.Opacity + (FadeOut ? -0.14f : 0.18f), 0f, 1f);

            if (FadeOut && Projectile.Opacity <= 0f)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.myPlayer != Projectile.owner)
                return false;

            Player owner = Main.player[Projectile.owner];
            DeveloperWeaponInfoEntry entry = Entry;
            Rectangle panelArea = new((int)panelTopLeft.X, (int)panelTopLeft.Y, PanelWidth, PanelHeight);
            bool mouseOverPanel = panelArea.Intersects(MouseRectangle);
            bool closePressed = (Main.mouseLeft && Main.mouseLeftRelease) || (Main.mouseRight && Main.mouseRightRelease);

            DrawPanel(panelArea, entry, Projectile.Opacity);
            DrawHeader(panelArea, entry, Projectile.Opacity);
            DrawBody(panelArea, entry, Projectile.Opacity);

            if (!mouseOverPanel && !FadeOut && Projectile.Opacity >= 0.95f && closePressed)
            {
                FadeOut = true;
                SoundEngine.PlaySound(SoundID.MenuClose with { Volume = 0.58f, Pitch = 0.05f }, owner.Center);
            }

            if (mouseOverPanel)
            {
                Main.blockMouse = true;
                owner.mouseInterface = true;
            }

            return false;
        }

        public static void RequestClose(Projectile projectile)
        {
            if (projectile.ModProjectile is DeveloperWeaponInfoPanel panel)
                panel.FadeOut = true;
            else
                projectile.ai[0] = 1f;
        }

        private static Vector2 GetClampedPanelTopLeftFromCenter(Vector2 desiredCenter)
        {
            const float screenMargin = 12f;
            Vector2 desiredTopLeft = desiredCenter - new Vector2(PanelWidth, PanelHeight) * 0.5f;
            float maxX = Math.Max(screenMargin, Main.screenWidth - PanelWidth - screenMargin);
            float maxY = Math.Max(screenMargin, Main.screenHeight - PanelHeight - screenMargin);

            return new Vector2(
                MathHelper.Clamp(desiredTopLeft.X, screenMargin, maxX),
                MathHelper.Clamp(desiredTopLeft.Y, screenMargin, maxY));
        }

        private static void DrawPanel(Rectangle panelArea, DeveloperWeaponInfoEntry entry, float opacity)
        {
            DrawRectangle(panelArea, new Color(10, 12, 18, 238) * opacity);
            DrawBorder(panelArea, Color.Lerp(new Color(92, 104, 126), entry.AccentColor, 0.56f) * opacity, BorderThickness);

            Rectangle innerArea = new(
                panelArea.X + BorderThickness,
                panelArea.Y + BorderThickness,
                panelArea.Width - BorderThickness * 2,
                panelArea.Height - BorderThickness * 2);
            DrawBorder(innerArea, new Color(32, 40, 56, 224) * opacity, 1);

            Color gridColor = Color.Lerp(new Color(28, 34, 48), entry.AccentColor, 0.16f) * (opacity * 0.52f);
            for (int x = panelArea.X + 22; x < panelArea.Right - 22; x += 28)
                DrawRectangle(new Rectangle(x, panelArea.Y + 12, 1, panelArea.Height - 24), gridColor);

            for (int y = panelArea.Y + 22; y < panelArea.Bottom - 22; y += 24)
                DrawRectangle(new Rectangle(panelArea.X + 12, y, panelArea.Width - 24, 1), gridColor);
        }

        private static void DrawHeader(Rectangle panelArea, DeveloperWeaponInfoEntry entry, float opacity)
        {
            Rectangle iconFrame = new(panelArea.X + 28, panelArea.Y + 25, IconFrameSize, IconFrameSize);
            DrawRectangle(iconFrame, Color.Lerp(new Color(12, 15, 24), entry.AccentColor, 0.12f) * (opacity * 0.96f));
            DrawBorder(iconFrame, entry.AccentColor * opacity, 2);
            DrawItemIcon(entry.WeaponItemType, iconFrame.Center.ToVector2(), MaxIconDrawSize, opacity);

            Rectangle titleArea = new(panelArea.X + 142, panelArea.Y + 26, panelArea.Width - 170, 39);
            DrawFitText(Lang.GetItemNameValue(entry.WeaponItemType), titleArea, Color.White, 1.02f, 0.62f, opacity);

            Rectangle subtitleArea = new(panelArea.X + 142, panelArea.Y + 67, panelArea.Width - 170, 34);
            DrawFitText(GetLocalizedText(entry, "Subtitle"), subtitleArea, entry.SecondaryColor, 0.72f, 0.48f, opacity);

            Rectangle lineArea = new(panelArea.X + 142, panelArea.Y + 107, panelArea.Width - 170, 3);
            DrawRectangle(lineArea, entry.AccentColor * (opacity * 0.86f));
        }

        private static void DrawBody(Rectangle panelArea, DeveloperWeaponInfoEntry entry, float opacity)
        {
            Rectangle textFrame = new(panelArea.X + 28, panelArea.Y + 139, panelArea.Width - 56, panelArea.Height - 168);
            DrawRectangle(textFrame, new Color(13, 17, 26, 218) * (opacity * 0.96f));
            DrawBorder(textFrame, Color.Lerp(new Color(98, 108, 126), entry.AccentColor, 0.38f) * opacity, 2);

            string text = StripColorTags(GetLocalizedText(entry, "Acquisition"));
            Rectangle textArea = new(textFrame.X + 17, textFrame.Y + 15, textFrame.Width - 34, textFrame.Height - 30);
            float maxScale = entry.Id == DeveloperWeaponInfoPanelId.Guide ? 0.82f : 0.72f;
            float minScale = entry.Id == DeveloperWeaponInfoPanelId.Guide ? 0.56f : 0.48f;
            DrawWrappedFitText(text, textArea, new Color(226, 232, 242), maxScale, minScale, opacity);
        }

        private static void DrawItemIcon(int itemType, Vector2 center, float maxSize, float opacity)
        {
            Main.instance.LoadItem(itemType);
            Texture2D texture = TextureAssets.Item[itemType].Value;
            Rectangle source = texture.Frame();
            Vector2 sourceSize = source.Size();
            float fitScale = Math.Min(maxSize / Math.Max(1f, sourceSize.X), maxSize / Math.Max(1f, sourceSize.Y));

            Main.EntitySpriteDraw(
                texture,
                center,
                source,
                Color.White * opacity,
                0f,
                sourceSize * 0.5f,
                fitScale,
                SpriteEffects.None,
                0f);
        }

        private static string GetLocalizedText(DeveloperWeaponInfoEntry entry, string suffix)
        {
            string key = $"Mods.CalamityThrowingSpear.DeveloperWeaponPanels.{entry.LocalizationKey}.{suffix}";
            string text = Language.GetTextValue(key);
            return text == key ? string.Empty : text;
        }

        private static void DrawWrappedFitText(string text, Rectangle area, Color color, float maxScale, float minScale, float opacity)
        {
            if (string.IsNullOrWhiteSpace(text) || area.Width <= 0 || area.Height <= 0)
                return;

            string[] lines = WrapTextToArea(text, area.Width, area.Height, minScale);
            DrawMultilineFitText(lines, area, color, maxScale, minScale, opacity);
        }

        private static void DrawMultilineFitText(string[] lines, Rectangle area, Color color, float maxScale, float minScale, float opacity)
        {
            if (lines.Length == 0)
                return;

            var font = FontAssets.MouseText.Value;
            float scale = maxScale;
            float widest = 0f;

            foreach (string line in lines)
                widest = Math.Max(widest, font.MeasureString(line).X);

            if (widest * scale > area.Width)
                scale = area.Width / Math.Max(1f, widest);

            if (font.LineSpacing * scale * lines.Length > area.Height)
                scale = Math.Min(scale, area.Height / Math.Max(1f, font.LineSpacing * lines.Length));

            scale = MathHelper.Clamp(scale, minScale, maxScale);
            float lineHeight = font.LineSpacing * scale;
            Vector2 position = new(area.X, area.Y + Math.Max(0f, (area.Height - lineHeight * lines.Length) * 0.5f));

            for (int i = 0; i < lines.Length; i++)
                DrawTextWithShadow(lines[i], position + new Vector2(0f, i * lineHeight), color * opacity, scale, opacity);
        }

        private static void DrawFitText(string text, Rectangle area, Color color, float maxScale, float minScale, float opacity)
        {
            if (string.IsNullOrWhiteSpace(text) || area.Width <= 0 || area.Height <= 0)
                return;

            var font = FontAssets.MouseText.Value;
            Vector2 size = font.MeasureString(text);
            if (size.X <= 0f || size.Y <= 0f)
                return;

            float scale = maxScale;
            if (size.X * scale > area.Width)
                scale = area.Width / size.X;
            if (size.Y * scale > area.Height)
                scale = Math.Min(scale, area.Height / size.Y);

            scale = MathHelper.Clamp(scale, minScale, maxScale);
            Vector2 position = new(area.X, area.Y + Math.Max(0f, (area.Height - size.Y * scale) * 0.5f));
            DrawTextWithShadow(text, position, color * opacity, scale, opacity);
        }

        private static string[] WrapTextToArea(string text, int width, int height, float scale)
        {
            var font = FontAssets.MouseText.Value;
            int maxLines = Math.Max(1, (int)Math.Floor(height / Math.Max(1f, font.LineSpacing * scale)));
            List<string> lines = new();
            string currentLine = string.Empty;

            foreach (char character in text.Replace("\r", string.Empty))
            {
                if (character == '\n')
                {
                    AddLine(lines, ref currentLine, width, font, scale);
                    if (lines.Count >= maxLines)
                        break;
                    continue;
                }

                string candidate = currentLine + character;
                if (font.MeasureString(candidate).X * scale <= width)
                {
                    currentLine = candidate;
                    continue;
                }

                AddLine(lines, ref currentLine, width, font, scale);
                if (lines.Count >= maxLines)
                    break;

                currentLine = character.ToString();
            }

            if (!string.IsNullOrEmpty(currentLine) && lines.Count < maxLines)
                AddLine(lines, ref currentLine, width, font, scale);

            if (lines.Count == maxLines && lines.Count > 0)
                lines[^1] = TrimTextToFit(lines[^1], width, font.LineSpacing, scale);

            return lines.ToArray();
        }

        private static void AddLine(List<string> lines, ref string currentLine, int width, ReLogic.Graphics.DynamicSpriteFont font, float scale)
        {
            if (string.IsNullOrEmpty(currentLine))
            {
                lines.Add(string.Empty);
                return;
            }

            lines.Add(TrimTextToFit(currentLine.TrimEnd(), width, font.LineSpacing, scale));
            currentLine = string.Empty;
        }

        private static string TrimTextToFit(string text, int width, int height, float scale)
        {
            var font = FontAssets.MouseText.Value;
            if (font.MeasureString(text).X * scale <= width && font.MeasureString(text).Y * scale <= height)
                return text;

            const string suffix = "...";
            string trimmed = text;
            while (trimmed.Length > 0)
            {
                Vector2 size = font.MeasureString(trimmed + suffix);
                if (size.X * scale <= width && size.Y * scale <= height)
                    break;

                trimmed = trimmed[..^1];
            }

            return trimmed.Length > 0 ? trimmed + suffix : suffix;
        }

        private static string StripColorTags(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            string output = text;
            int start;
            while ((start = output.IndexOf("[c/", StringComparison.Ordinal)) >= 0)
            {
                int colon = output.IndexOf(':', start);
                if (colon < 0)
                    break;

                output = output.Remove(start, colon - start + 1);
            }

            return output.Replace("]", string.Empty);
        }

        private static void DrawTextWithShadow(string text, Vector2 position, Color color, float scale, float opacity)
        {
            CalamityUtils.DrawBorderStringEightWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                text,
                position,
                color,
                Color.Black * (0.75f * opacity),
                scale);
        }

        private static void DrawRectangle(Rectangle rectangle, Color color)
        {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, color);
        }

        private static void DrawBorder(Rectangle rectangle, Color color, int thickness)
        {
            DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
            DrawRectangle(new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
            DrawRectangle(new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
            DrawRectangle(new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
        }
    }

    internal sealed class DeveloperWeaponInfoEntry
    {
        private readonly Func<int> sourceItemType;
        private readonly Func<int> weaponItemType;

        public DeveloperWeaponInfoEntry(DeveloperWeaponInfoPanelId id, string localizationKey, Func<int> sourceItemType, Func<int> weaponItemType, Color accentColor, Color secondaryColor)
        {
            Id = id;
            LocalizationKey = localizationKey;
            this.sourceItemType = sourceItemType;
            this.weaponItemType = weaponItemType;
            AccentColor = accentColor;
            SecondaryColor = secondaryColor;
        }

        public DeveloperWeaponInfoPanelId Id { get; }
        public string LocalizationKey { get; }
        public int SourceItemType => sourceItemType();
        public int WeaponItemType => weaponItemType();
        public Color AccentColor { get; }
        public Color SecondaryColor { get; }
    }
}
