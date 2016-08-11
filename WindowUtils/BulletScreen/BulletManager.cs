using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BulletScreen
{
    enum BulletState
    {
        Ready,
        Flying,
        Dropped
    }

    public class Bullet
    {
        public string Content { get; set; }
        public Color Color { get; set; }
        public Size GetSize(Font font)
        {
            return TextRenderer.MeasureText(Content, font);
        }

        public int X { get; set; }

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
        private int speed; //pixels per 100 milliseconds
        private const int defaultFontSize = 16;
        private const double millisecondsPerSeconds = 1000;
        private const double baseTime = 100;

        public BulletManager(Rectangle bounds, Font font = null,
            int durationTime = 10, int marginRow = 0, int marginCol = 0)
        {
            Bounds = bounds;
            DurationTime = durationTime;
            MarginRow = marginRow;
            MarginCol = marginCol;

            if (font == null)
                font = new Font(SystemFonts.DefaultFont.FontFamily,
                    defaultFontSize, FontStyle.Regular, GraphicsUnit.Pixel);

            Font = font;

            Rows = new List<LinkedList<Bullet>>();
        }

        public int Width
        {
            get
            {
                return Bounds.Width;
            }
        }

        public int Height
        {
            get
            {
                return Bounds.Height;
            }
        }

        public void Update(Graphics canvas, int ms = (int)baseTime)
        {
            var currentSpeed = GetCurrentSpeed(ms);

            foreach (var row in Rows)
            {
                for (var bullet = row.First; bullet != null;)
                {
                    var next = bullet.Next;

                    if (bullet.Value.State == BulletState.Dropped)
                        row.Remove(bullet);

                    bullet.Value.X -= currentSpeed;
                    UpdateBullet(bullet.Value);

                    bullet = next;
                }
            }

            Draw(canvas);
        }

        private void Draw(Graphics canvas)
        {
            for (int i = 0; i < Rows.Count; ++i)
            {
                var row = Rows[i];
                foreach (var bullet in row)
                {
                    if (bullet.State != BulletState.Flying)
                        continue;

                    var pos = new Point(bullet.X, GetRowTop(canvas, i));

                    TextRenderer.DrawText(canvas, bullet.Content, Font, pos, bullet.Color);
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
            CalculateSpeed();
            CalculateMaxRows();

            Rows.Clear();
            for (int i = 0; i < maxRows; ++i)
            {
                Rows.Add(new LinkedList<Bullet>());
            }
        }

        public void UpdateLayout(Graphics g)
        {
            CalculateSpeed();
            CalculateMaxRows(g);

            Rows.Clear();
            for (int i = 0; i < maxRows; ++i)
            {
                Rows.Add(new LinkedList<Bullet>());
            }
        }

        private int GetRowTop(Graphics g, int row)
        {
            return (int)Math.Ceiling(row * GetRowHeight(g));
        }

        private void CalculateSpeed()
        {
            speed = (int)Math.Round(Width / (DurationTime * millisecondsPerSeconds / baseTime));
        }

        private float GetRowHeight(Graphics g)
        {
            return Font.GetHeight(g) + MarginRow;
        }

        private void CalculateMaxRows(Graphics g)
        {
            maxRows = (int)Math.Floor(Height / GetRowHeight(g));
        }

        private float GetRowHeight()
        {
            return Font.GetHeight() + MarginRow;
        }

        private void CalculateMaxRows()
        {
            maxRows = (int)Math.Floor(Height / GetRowHeight());
        }

        private int GetCurrentSpeed(int milliseconds)
        {
            return (int)Math.Round(milliseconds / baseTime * speed);
        }

        public void Shot(string content, Color color)
        {
            var b = new Bullet() { Content = content, Color = color, X = Width, State = BulletState.Ready };

            Shot(b);
        }

        private void Shot(Bullet bullet)
        {
            if (Rows.Count == 0)
                return;

            for (int i = 0; i < Rows.Count; ++i)
            {
                if (IsFull(i))
                    continue;

                AddBullet(Rows[i], bullet);
                return;
            }


            var row = Rows.OrderBy(r => r.Sum(b => GetBulletHidePart(b))).FirstOrDefault();
            AddBullet(row, bullet);
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

        private void AddBullet(LinkedList<Bullet> row, Bullet bullet)
        {
            if (row.Count > 0)
            {
                bullet.X = Math.Max(Width, GetBulletWidth(row.Last()));
            }

            row.AddLast(new LinkedListNode<Bullet>(bullet));
        }

        private int GetBulletWidth(Bullet bullet)
        {
            return bullet.X + GetBulletActualWidth(bullet);
        }

        private int GetBulletActualWidth(Bullet bullet)
        {
            return bullet.GetSize(Font).Width + MarginCol;
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
