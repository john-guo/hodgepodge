using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BulletScreen
{
    enum BulletState
    {
        Ready,
        Flying,
        Dropped
    }

    internal static class TextRenderer {
        /// <summary>
        /// Get the required height and width of the specified text. Uses FortammedText
        /// </summary>
        public static Size MeasureTextSize(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            FormattedText ft = new FormattedText(text,
                                                 CultureInfo.CurrentCulture,
                                                 FlowDirection.LeftToRight,
                                                 new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                                 fontSize,
                                                 Brushes.Black);
            return new Size(ft.Width, ft.Height);
        }

        /// <summary>
        /// Get the required height and width of the specified text. Uses Glyph's
        /// </summary>
        public static Size MeasureText(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            Typeface typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
            GlyphTypeface glyphTypeface;

            if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
            {
                return MeasureTextSize(text, fontFamily, fontStyle, fontWeight, fontStretch, fontSize);
            }

            double totalWidth = 0;
            double height = 0;

            for (int n = 0; n < text.Length; n++)
            {
                try
                {
                    ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];

                    double width = glyphTypeface.AdvanceWidths[glyphIndex] * fontSize;

                    double glyphHeight = glyphTypeface.AdvanceHeights[glyphIndex] * fontSize;

                    if (glyphHeight > height)
                    {
                        height = glyphHeight;
                    }

                    totalWidth += width;
                }
                catch
                {
                    return MeasureTextSize(text, fontFamily, fontStyle, fontWeight, fontStretch, fontSize);
                }
            }

            return new Size(totalWidth, height);
        }
    }

    public class Font
    {
        public FontFamily FontFamily { get; set; }
        public FontStyle FontStyle { get; set; }
        public FontWeight FontWeight { get; set; }
        public FontStretch FontStretch { get; set; }
        public double FontSize { get; set; }

        public Font(FontFamily defaultFontFamily, double defaultFontSize)
        {
            FontFamily = defaultFontFamily;
            FontStyle = FontStyles.Normal;
            FontStretch = FontStretches.Normal;
            FontWeight = FontWeights.Normal;
            FontSize = defaultFontSize;
        }

        public double GetHeight()
        {
            return FontFamily.LineSpacing * FontSize;
        }
    }

    internal class Bullet
    {
        private Size size = Size.Empty;
        internal string Content { get; set; }
        internal Color Color { get; set; }
        internal Size GetSize(Font font)
        {
            if (size == Size.Empty)
                return size = TextRenderer.MeasureText(Content, font.FontFamily, font.FontStyle, font.FontWeight, font.FontStretch, font.FontSize);
            return size;
        }

        internal int X { get; set; }
        internal int StartX { get; set; }
        internal DateTime ShotTime { get; set; }
        internal double Speed { get; set; }
        internal BulletState State { get; set; }
    }

    public class BulletManager
    {
        public Rectangle Bounds { get; set; }
        int MarginRow { get; set; }
        int MarginCol { get; set; }
        int DurationTime { get; set; } //seconds
        public Font Font { get; set; }

        private List<LinkedList<Bullet>> Rows;

        private int maxRows;
        private const int defaultFontSize = 16;
        private const double millisecondsPerSeconds = 1000;

        public BulletManager(Rectangle bounds, Font font = null,
            int durationTime = 10, int marginRow = 0, int marginCol = 0)
        {
            Bounds = bounds;
            DurationTime = durationTime;
            MarginRow = marginRow;
            MarginCol = marginCol;

            if (font == null)
                font = new Font(SystemFonts.CaptionFontFamily, defaultFontSize);

            Font = font;

            Rows = new List<LinkedList<Bullet>>();
        }

        public int Width
        {
            get
            {
                return (int)Bounds.Width;
            }
        }

        public int Height
        {
            get
            {
                return (int)Bounds.Height;
            }
        }

        private Func<DateTime> playTimeFunc = delegate { return DateTime.Now; };
        public Func<DateTime> GetPlayTime
        {
            get
            {
                return playTimeFunc;
            }

            set
            {
                playTimeFunc = value;
                if (playTimeFunc == null)
                    playTimeFunc = delegate { return DateTime.Now; };
            }
        }

        public void Update(DrawingContext canvas)
        {
            var now = GetPlayTime();

            foreach (var row in Rows)
            {
                for (var bullet = row.First; bullet != null;)
                {
                    var next = bullet.Next;

                    if (bullet.Value.State == BulletState.Dropped)
                        row.Remove(bullet);

                    var b = bullet.Value;

                    var offset = (int)Math.Round(b.Speed * (now - b.ShotTime).TotalMilliseconds);

                    b.X = b.StartX - offset;
                    UpdateBullet(b);

                    bullet = next;
                }
            }

            Draw(canvas);
        }

        private void Draw(DrawingContext canvas)
        {
            for (int i = 0; i < Rows.Count; ++i)
            {
                var row = Rows[i];
                foreach (var bullet in row)
                {
                    if (bullet.State != BulletState.Flying)
                        continue;

                    var pos = new Point(bullet.X, GetRowTop(canvas, i));

                    FormattedText ft = new FormattedText(bullet.Content,
                                     CultureInfo.CurrentCulture,
                                     FlowDirection.LeftToRight,
                                     new Typeface(Font.FontFamily, Font.FontStyle, Font.FontWeight, Font.FontStretch),
                                     Font.FontSize,
                                     new SolidColorBrush(bullet.Color));

                    canvas.DrawText(ft, pos);
                }
            }
        }

        private void UpdateBullet(Bullet bullet)
        {
            switch (bullet.State)
            {
                case BulletState.Ready:
                    if (GetBulletDisplayPart(bullet) > 0)
                        bullet.State = BulletState.Flying;
                    break;
                case BulletState.Flying:
                    if (GetBulletWidth(bullet) <= 0)
                        bullet.State = BulletState.Dropped;
                    break;
                case BulletState.Dropped:
                    break;
            }
        }

        public void UpdateLayout()
        {
            CalculateMaxRows();

            Rows.Clear();
            for (int i = 0; i < maxRows; ++i)
            {
                Rows.Add(new LinkedList<Bullet>());
            }
        }

        public void UpdateLayout(DrawingContext g)
        {
            CalculateMaxRows(g);

            Rows.Clear();
            for (int i = 0; i < maxRows; ++i)
            {
                Rows.Add(new LinkedList<Bullet>());
            }
        }

        private int GetRowTop(DrawingContext g, int row)
        {
            return (int)Math.Ceiling(row * GetRowHeight(g));
        }

        private double GetRowHeight(DrawingContext g)
        {
            return Font.GetHeight() + MarginRow;
        }

        private void CalculateMaxRows(DrawingContext g)
        {
            maxRows = (int)Math.Floor(Height / GetRowHeight(g));
        }

        private double GetRowHeight()
        {
            return Font.GetHeight() + MarginRow;
        }

        private void CalculateMaxRows()
        {
            maxRows = (int)Math.Floor(Height / GetRowHeight());
        }

        public void Shot(string content, Color color, int delayMS = 0)
        {
            var b = new Bullet() { Content = content, Color = color, X = Width, StartX = Width, State = BulletState.Ready };

            Shot(b, delayMS);
        }

        private void Shot(Bullet bullet, int delayMS)
        {
            if (Rows.Count == 0)
                return;

            for (int i = 0; i < Rows.Count; ++i)
            {
                if (IsFull(i))
                    continue;

                AddBullet(Rows[i], bullet, delayMS);
                return;
            }


            var row = Rows.OrderBy(r => r.Min(b => GetBulletHidePart(b))).FirstOrDefault();
            AddBullet(row, bullet, delayMS);
        }

        private bool IsFull(int rowIndex)
        {
            if (rowIndex >= Rows.Count)
                return true;

            var last = Rows[rowIndex].LastOrDefault();
            if (last == null)
                return false;

            return GetBulletWidth(last) >= Width;
        }

        private void AddBullet(LinkedList<Bullet> row, Bullet bullet, int delayMS)
        {
            if (row.Count > 0)
            {
                bullet.StartX = bullet.X = Math.Max(Width, GetBulletWidth(row.Last()));
            }

            bullet.ShotTime = GetPlayTime();
            bullet.Speed = 1.0 * GetBulletWidth(bullet) / (DurationTime * millisecondsPerSeconds);

            if (delayMS > 0)
            {
                bullet.StartX = bullet.X += (int)Math.Round(bullet.Speed * delayMS);
            }

            row.AddLast(new LinkedListNode<Bullet>(bullet));
        }

        private int GetBulletWidth(Bullet bullet)
        {
            return bullet.X + GetBulletActualWidth(bullet);
        }

        private int GetBulletActualWidth(Bullet bullet)
        {
            return (int)bullet.GetSize(Font).Width + MarginCol;
        }

        private int GetBulletHidePart(Bullet bullet)
        {
            return Math.Max(0, GetBulletWidth(bullet) - Width);
        }

        private int GetBulletDisplayPart(Bullet bullet)
        {
            return Math.Max(0, GetBulletActualWidth(bullet) - GetBulletHidePart(bullet));
        }
    }
}

