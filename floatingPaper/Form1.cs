using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace floatingPaper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var img = generateImage(ClientRectangle.Width, 60);
            var form = newFloatingForm(ClientRectangle.Width, 60, (Bitmap)img);
            form.Show();
        }

        private Image generateImage(int w, int h)
        {
            var margin = 15;

            var width = w - margin * 2;

            var img = new Bitmap(w, h);
            var brush = new SolidBrush(Color.White);
            var pen = new Pen(brush, 2);
            using (var g = Graphics.FromImage(img))
            {
                var y = h / 2;

                g.DrawLine(pen, margin, y, w - margin, y);
                for (var i = 0; i <= 100; ++i)
                {
                    var x = margin + width * i / 100;
                    var y1 = y - 5;
                    pen.Width = 1;
                    bool text = false;
                    if (i % 10 == 0)
                    {
                        y1 -= 10;
                        pen.Width = 3;
                        text = true;
                    }
                    else if (i % 5 == 0)
                    {
                        y1 -= 5;
                        pen.Width = 2;
                        text = true;
                    }

                    g.DrawLine(pen, x, y1, x, y);
                    g.DrawLine(pen, x, y, x, y + y - y1);

                    if (text)
                    {
                        var size = g.MeasureString(i.ToString(), SystemFonts.DefaultFont);
                        var x1 = x - size.Width / 2;
                        g.DrawString(i.ToString(), SystemFonts.DefaultFont, brush, x1, y1 - size.Height);
                        g.DrawString(i.ToString(), SystemFonts.DefaultFont, brush, x1, y + size.Height);
                    }
                }

                g.FillEllipse(brush, 10, 10, 10, 10);
            }
            return img;
        }

        private Form newFloatingForm(int w, int h, Bitmap img)
        {
            var form = new Form();
            form.ControlBox = false;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            form.Width = w;
            form.Height = h;
            form.TopMost = true;
            var tran = Color.FromArgb(0);
            img.MakeTransparent(tran);
            form.Region = bmprgn(img, tran);
            form.BackColor = Color.White;
            form.MouseDown += form_MouseDown;
            form.MouseUp += form_MouseUp;
            form.MouseMove += form_MouseMove;
            form.MouseDoubleClick += form_MouseDoubleClick;
            form.Tag = null;
            form.AllowTransparency = true;

            return form;
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        class DragObj
        {
            public Point loc;
            public bool resize;

            public DragObj()
            {
                resize = false;
            }
        }

        void form_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var form = sender as Form;
            if (form.BackColor == Color.White)
                form.BackColor = Color.LightYellow;
            else
                form.BackColor = Color.White;
        }

        void form_MouseMove(object sender, MouseEventArgs e)
        {
            var form = sender as Form;
            if (form.BackColor == Color.LightYellow)
                return;

            var d = form.Tag as DragObj;
            if (d == null)
            {
                form.Cursor = Cursors.Default;
                var x = e.Location.X;
                if (x >= form.ClientRectangle.Width - 15)
                {
                    form.Cursor = Cursors.SizeWE;
                }
                return;
            }


            if (!d.resize) 
            {
                form.Left += e.X - d.loc.X;
                form.Top += e.Y - d.loc.Y;
            }
            else
            {
                form.Width += e.X - d.loc.X;
                d.loc.X = e.X;
                form.BackgroundImage = generateImage(form.Width, form.Height);
            }
            
        }

        void form_MouseUp(object sender, MouseEventArgs e)
        {
            var form = sender as Form;
            if (form.BackColor == Color.LightYellow)
                return;

            var d = form.Tag as DragObj;
            form.Tag = null;
            if (d == null)
                return;

            if (d.resize)
            {
                form.BackColor = Color.White;
                form.Opacity = 1;
                var tran = Color.FromArgb(0);
                var img = (Bitmap)form.BackgroundImage;
                img.MakeTransparent(tran);
                form.Region = bmprgn(img, tran);
                form.BackgroundImage = null;
            }
        }

        void form_MouseDown(object sender, MouseEventArgs e)
        {
            var form = sender as Form;
            if (form.BackColor == Color.LightYellow)
                return;

            var d = new DragObj();
            d.loc = e.Location;

            if (form.Cursor == Cursors.SizeWE) 
            {
                form.Region = null;
                form.Opacity = 0.8;
                form.BackColor = SystemColors.Control;
                form.BackgroundImage = generateImage(form.Width, form.Height);
                d.resize = true;
            }

            form.Tag = d;
        }

        private Region bmprgn(Bitmap picture, Color transparentcolor)
        {
            int nwidth = picture.Width;
            int nheight = picture.Height;
            Region rgn = new Region();
            rgn.MakeEmpty();
            bool istransrgn;//前一个点是否在透明区 
            Color curcolor;//当前点的颜色 
            Rectangle currect = new Rectangle();
            currect.Height = 1;
            int x = 0, y = 0;
            //逐像素扫描这个图片，找出非透明色部分区域并合并起来。 
            for (y = 0; y < nheight; ++y)
            {
                istransrgn = true;
                for (x = 0; x < nwidth; ++x)
                {
                    curcolor = picture.GetPixel(x, y);
                    if (curcolor == transparentcolor || x == nwidth - 1)//如果遇见透明色或行尾 
                    {
                        if (istransrgn == false)//退出有效区 
                        {
                            currect.Width = x - currect.X;
                            rgn.Union(currect);
                        }
                    }
                    else//非透明色 
                    {
                        if (istransrgn == true)//进入有效区 
                        {
                            currect.X = x;
                            currect.Y = y;
                        }
                    }//if curcolor 
                    istransrgn = curcolor == transparentcolor;
                }//for x 
            }//for y 
            return rgn;
        }   

    }
}
