using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using ElementalArkJavItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.ElementalArkJav.ElementalArkJav;
using FinishingTouchItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.FinishingTouch.FinishingTouch;
using RevelationItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.Revelation.Revelation;
using SagittariusItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.Sagittarius.Sagittarius;
using ShadowJavItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.ShadowJav.ShadowJav;
using StarsofDestinyItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.StarsofDestiny.StarsofDestiny;
using SunsetItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.Sunset.Sunset;
using TheLastLanceItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.TheLastLance.TheLastLance;
using TidalMechanicsItem = CalamityThrowingSpear.Weapons.DeveloperWeapons.TidalMechanics.TidalMechanics;

namespace CalamityThrowingSpear.LegendaryTooltipEffects
{
    public sealed class DeveloperWeaponLegendaryTooltipEffects : GlobalItem
    {
        private enum TooltipTheme
        {
            Ocean,
            DeepSeaLance,
            Starfall,
            Prism,
            DragonFire,
            RevelationTerminal,
            ShadowGlitch,
            ChronoStar,
            Sunset
        }

        private readonly struct TooltipStyle
        {
            public TooltipStyle(TooltipTheme theme, Color primary, Color secondary, Color accent, string[] glyphs)
            {
                Theme = theme;
                Primary = primary;
                Secondary = secondary;
                Accent = accent;
                Glyphs = glyphs;
            }

            public TooltipTheme Theme { get; }
            public Color Primary { get; }
            public Color Secondary { get; }
            public Color Accent { get; }
            public string[] Glyphs { get; }
        }

        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            if (!IsTooltipTextLine(line) || !TryGetStyle(item, out TooltipStyle style))
                return true;

            DrawLegendaryTooltipLine(line, style);
            return false;
        }

        private static bool IsTooltipTextLine(DrawableTooltipLine line)
        {
            return line.Name.StartsWith("Tooltip", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(StripChatTags(line.Text));
        }

        private static bool TryGetStyle(Item item, out TooltipStyle style)
        {
            if (item.type == ModContent.ItemType<TidalMechanicsItem>())
            {
                style = new TooltipStyle(TooltipTheme.Ocean, new Color(90, 210, 255), new Color(4, 22, 58), new Color(180, 248, 255), new[] { "o", "O", ".", "~" });
                return true;
            }

            if (item.type == ModContent.ItemType<TheLastLanceItem>())
            {
                style = new TooltipStyle(TooltipTheme.DeepSeaLance, new Color(126, 220, 255), new Color(7, 24, 38), new Color(218, 244, 255), new[] { "/", "\\", "V", "L", "I" });
                return true;
            }

            if (item.type == ModContent.ItemType<SagittariusItem>())
            {
                style = new TooltipStyle(TooltipTheme.Starfall, new Color(255, 232, 92), new Color(34, 22, 54), new Color(255, 255, 214), new[] { "*", "+", ".", "x" });
                return true;
            }

            if (item.type == ModContent.ItemType<ElementalArkJavItem>())
            {
                style = new TooltipStyle(TooltipTheme.Prism, new Color(180, 120, 255), new Color(18, 12, 42), new Color(120, 235, 255), new[] { "I", "II", "III", "IV" });
                return true;
            }

            if (item.type == ModContent.ItemType<FinishingTouchItem>())
            {
                style = new TooltipStyle(TooltipTheme.DragonFire, new Color(255, 118, 48), new Color(44, 10, 4), new Color(255, 221, 112), new[] { "F", "DRG", "!", "/" });
                return true;
            }

            if (item.type == ModContent.ItemType<RevelationItem>())
            {
                style = new TooltipStyle(TooltipTheme.RevelationTerminal, new Color(196, 224, 255), new Color(4, 6, 12), new Color(92, 188, 255), new[] { "SYS", "RUN", "01", "ERR" });
                return true;
            }

            if (item.type == ModContent.ItemType<ShadowJavItem>())
            {
                style = new TooltipStyle(TooltipTheme.ShadowGlitch, new Color(204, 204, 222), new Color(8, 7, 14), new Color(168, 112, 255), new[] { "INF", "NULL", "0", "1" });
                return true;
            }

            if (item.type == ModContent.ItemType<StarsofDestinyItem>())
            {
                style = new TooltipStyle(TooltipTheme.ChronoStar, new Color(200, 250, 255), new Color(12, 16, 46), new Color(255, 232, 160), new[] { "XII", "VI", "00", "*" });
                return true;
            }

            if (item.type == ModContent.ItemType<SunsetItem>())
            {
                style = new TooltipStyle(TooltipTheme.Sunset, new Color(255, 137, 86), new Color(38, 14, 44), new Color(255, 230, 150), new[] { "SUN", "END", "*", "." });
                return true;
            }

            style = default;
            return false;
        }

        private static void DrawLegendaryTooltipLine(DrawableTooltipLine line, TooltipStyle style)
        {
            string plainText = StripChatTags(line.Text);
            string[] textLines = plainText
                .Replace("\r", string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (textLines.Length == 0)
                return;

            Vector2 basePosition = new(line.X, line.Y);
            float lineHeight = line.Font.LineSpacing * line.BaseScale.Y;
            float textWidth = textLines.Max(text => line.Font.MeasureString(text).X * line.BaseScale.X);
            float textHeight = lineHeight * textLines.Length;
            Rectangle area = new(
                (int)basePosition.X - 8,
                (int)basePosition.Y - 5,
                Math.Max(24, (int)Math.Ceiling(textWidth) + 16),
                Math.Max(18, (int)Math.Ceiling(textHeight) + 10));

            float time = Main.GlobalTimeWrappedHourly;
            DrawBackdrop(area, style, time);
            DrawAmbientGlyphs(line, area, style, time);
            DrawTextLines(line, textLines, basePosition, lineHeight, style, time);
            DrawForeground(area, style, time);
        }

        private static void DrawBackdrop(Rectangle area, TooltipStyle style, float time)
        {
            DrawRectangle(area, style.Secondary * 0.92f);
            DrawRectangle(new Rectangle(area.X + 2, area.Y + 2, area.Width - 4, area.Height - 4), style.Secondary * 0.52f);

            int step = style.Theme == TooltipTheme.Ocean ? 6 : 5;
            for (int y = area.Y + 4; y < area.Bottom - 4; y += step)
            {
                float wave = MathF.Sin(time * GetWaveSpeed(style.Theme) + y * 0.08f);
                int x = area.X + 4 + (int)((wave + 1f) * 0.5f * Math.Max(1, area.Width - 12));
                int width = style.Theme == TooltipTheme.RevelationTerminal || style.Theme == TooltipTheme.ShadowGlitch ? area.Width - 8 : 5;
                DrawRectangle(new Rectangle(x, y, Math.Max(2, width), 1), style.Primary * 0.12f);
            }

            if (style.Theme == TooltipTheme.RevelationTerminal || style.Theme == TooltipTheme.ShadowGlitch)
            {
                int sweepY = area.Y + 2 + (int)((time * 48f) % Math.Max(1, area.Height - 4));
                DrawRectangle(new Rectangle(area.X + 2, sweepY, area.Width - 4, 2), style.Accent * 0.18f);
            }
        }

        private static void DrawAmbientGlyphs(DrawableTooltipLine line, Rectangle area, TooltipStyle style, float time)
        {
            int columns = Math.Clamp(area.Width / 48, 4, 14);
            float travelHeight = area.Height + 36f;
            Vector2 glyphScale = line.BaseScale * GetGlyphScale(style.Theme);

            for (int column = 0; column < columns; column++)
            {
                float x = area.X + 8f + column * (area.Width - 16f) / Math.Max(1, columns - 1);
                float speed = 16f + column % 5 * 4f + GetGlyphSpeedOffset(style.Theme);
                float y = area.Bottom + 22f - ((time * speed + column * 31f) % travelHeight);

                if (style.Theme == TooltipTheme.RevelationTerminal || style.Theme == TooltipTheme.ShadowGlitch)
                    y = area.Y - 24f + (time * speed + column * 29f) % travelHeight;

                float sway = MathF.Sin(time * 2.7f + column * 1.9f) * GetSway(style.Theme);
                string glyph = style.Glyphs[((int)(time * 7f) + column * 5) % style.Glyphs.Length];
                Color color = Color.Lerp(style.Primary, style.Accent, column / (float)Math.Max(1, columns - 1)) * 0.32f;
                Vector2 position = new(x + sway, y);

                if (position.Y < area.Y + 2f || position.Y > area.Bottom - line.Font.LineSpacing * glyphScale.Y - 2f)
                    continue;

                ChatManager.DrawColorCodedString(Main.spriteBatch, line.Font, glyph, position, color, 0f, Vector2.Zero, glyphScale);
            }
        }

        private static void DrawTextLines(DrawableTooltipLine line, string[] textLines, Vector2 basePosition, float lineHeight, TooltipStyle style, float time)
        {
            for (int row = 0; row < textLines.Length; row++)
            {
                string text = textLines[row];
                float pulse = (MathF.Sin(time * GetPulseSpeed(style.Theme) - row * 0.6f) + 1f) * 0.5f;
                Vector2 offset = GetTextOffset(style.Theme, time, row, pulse);
                Vector2 position = basePosition + Vector2.UnitY * row * lineHeight + offset;

                Color glowColor = style.Primary with { A = 0 };
                glowColor *= 0.2f + pulse * 0.18f;
                float glowRadius = 1.2f + pulse * 1.6f;
                for (int draw = 0; draw < 6; draw++)
                {
                    Vector2 glowOffset = (MathHelper.TwoPi * draw / 6f).ToRotationVector2() * glowRadius;
                    DrawText(line, text, position + glowOffset, glowColor);
                }

                if (style.Theme == TooltipTheme.ShadowGlitch || style.Theme == TooltipTheme.RevelationTerminal)
                {
                    float glitch = MathF.Sin(time * 18f + row * 5.3f);
                    if (glitch > 0.9f)
                    {
                        DrawText(line, text, position + Vector2.UnitX * 3f, style.Accent * 0.28f);
                        DrawText(line, text, position - Vector2.UnitX * 4f, style.Primary * 0.2f);
                    }
                }

                DrawText(line, text, position + new Vector2(1f, 2f), Color.Black * 0.86f);

                Color textColor = style.Theme == TooltipTheme.Prism
                    ? GetPrismColor(time, row, pulse)
                    : Color.Lerp(style.Primary, style.Accent, 0.2f + pulse * 0.55f);
                DrawText(line, text, position, textColor);
            }
        }

        private static void DrawForeground(Rectangle area, TooltipStyle style, float time)
        {
            float pulse = (MathF.Sin(time * 5.2f) + 1f) * 0.5f;
            Color edgeColor = Color.Lerp(style.Primary, style.Accent, pulse) * 0.62f;
            Color dimEdge = style.Secondary * 0.72f;

            DrawRectangle(new Rectangle(area.X, area.Y, area.Width, 1), edgeColor);
            DrawRectangle(new Rectangle(area.X, area.Bottom - 1, area.Width, 1), dimEdge);
            DrawRectangle(new Rectangle(area.X, area.Y, 1, 8), edgeColor);
            DrawRectangle(new Rectangle(area.Right - 1, area.Y, 1, 8), edgeColor);
            DrawRectangle(new Rectangle(area.X, area.Bottom - 8, 1, 8), dimEdge);
            DrawRectangle(new Rectangle(area.Right - 1, area.Bottom - 8, 1, 8), dimEdge);

            if (style.Theme == TooltipTheme.DragonFire || style.Theme == TooltipTheme.Sunset)
            {
                int glintX = area.X + 4 + (int)((time * 40f) % Math.Max(1, area.Width - 12));
                DrawRectangle(new Rectangle(glintX, area.Y, 14, 1), style.Accent * 0.82f);
            }
        }

        private static float GetWaveSpeed(TooltipTheme theme)
        {
            return theme switch
            {
                TooltipTheme.Ocean => 2.2f,
                TooltipTheme.DeepSeaLance => 1.8f,
                TooltipTheme.Starfall => 3.4f,
                TooltipTheme.Prism => 4.1f,
                TooltipTheme.DragonFire => 5.4f,
                TooltipTheme.RevelationTerminal => 7.5f,
                TooltipTheme.ShadowGlitch => 9.2f,
                TooltipTheme.ChronoStar => 2.6f,
                TooltipTheme.Sunset => 3.1f,
                _ => 2.8f
            };
        }

        private static float GetGlyphScale(TooltipTheme theme)
        {
            return theme == TooltipTheme.RevelationTerminal || theme == TooltipTheme.ShadowGlitch ? 0.46f : 0.52f;
        }

        private static float GetGlyphSpeedOffset(TooltipTheme theme)
        {
            return theme switch
            {
                TooltipTheme.DragonFire => 10f,
                TooltipTheme.RevelationTerminal => 7f,
                TooltipTheme.ShadowGlitch => 12f,
                TooltipTheme.Starfall => 6f,
                _ => 0f
            };
        }

        private static float GetSway(TooltipTheme theme)
        {
            return theme switch
            {
                TooltipTheme.Ocean => 10f,
                TooltipTheme.DeepSeaLance => 4f,
                TooltipTheme.Prism => 8f,
                TooltipTheme.Sunset => 6f,
                _ => 2f
            };
        }

        private static float GetPulseSpeed(TooltipTheme theme)
        {
            return theme switch
            {
                TooltipTheme.DragonFire => 5.8f,
                TooltipTheme.RevelationTerminal => 4.6f,
                TooltipTheme.ShadowGlitch => 8.4f,
                TooltipTheme.Starfall => 3.2f,
                TooltipTheme.ChronoStar => 2.4f,
                _ => 2.8f
            };
        }

        private static Vector2 GetTextOffset(TooltipTheme theme, float time, int row, float pulse)
        {
            return theme switch
            {
                TooltipTheme.Ocean => Vector2.UnitX * MathF.Sin(time * 2.8f + row * 0.88f) * 4.2f,
                TooltipTheme.DeepSeaLance => Vector2.UnitX * MathF.Sin(time * 1.7f + row) * 2.2f,
                TooltipTheme.Starfall => new Vector2(MathF.Sin(time * 3f + row) * 1.8f, -pulse * 0.8f),
                TooltipTheme.Prism => Vector2.UnitX * MathF.Sin(time * 5f + row * 0.7f) * 2.8f,
                TooltipTheme.DragonFire => new Vector2(MathF.Sin(time * 7f + row) * 1.4f, -pulse * 1.1f),
                TooltipTheme.RevelationTerminal => Vector2.UnitX * (MathF.Sin(time * 18f + row * 4f) > 0.93f ? 3f : 0f),
                TooltipTheme.ShadowGlitch => Vector2.UnitX * (MathF.Sin(time * 20f + row * 6f) > 0.88f ? -4f : 0f),
                TooltipTheme.ChronoStar => new Vector2(MathF.Sin(time * 2.4f + row) * 1.6f, MathF.Cos(time * 2.4f + row) * 0.9f),
                TooltipTheme.Sunset => Vector2.UnitX * MathF.Sin(time * 2.6f + row * 0.6f) * 2.6f,
                _ => Vector2.Zero
            };
        }

        private static Color GetPrismColor(float time, int row, float pulse)
        {
            float hue = (time * 0.16f + row * 0.13f + pulse * 0.08f) % 1f;
            return Main.hslToRgb(hue, 0.92f, 0.72f);
        }

        private static void DrawText(DrawableTooltipLine line, string text, Vector2 position, Color color)
        {
            ChatManager.DrawColorCodedString(
                Main.spriteBatch,
                line.Font,
                text,
                position,
                color,
                line.Rotation,
                line.Origin,
                line.BaseScale);
        }

        private static string StripChatTags(string text)
        {
            return string.Concat(ChatManager.ParseMessage(text, Color.White).Select(snippet => snippet.Text));
        }

        private static void DrawRectangle(Rectangle rectangle, Color color)
        {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rectangle, color);
        }
    }
}
