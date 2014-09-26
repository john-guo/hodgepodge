using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
        List<Form> forms;
        static int index = 0;

        public Form1()
        {
            forms = new List<Form>();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var img = generateImage(ClientRectangle.Width, 60);
            var form = newFloatingForm(ClientRectangle.Width, 60, (Bitmap)img);
            forms.Add(form);
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
            form.Name = String.Format("{0}", index++);
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
            form.MouseWheel += form_MouseWheel;
            form.Tag = null;
            form.AllowTransparency = true;

            return form;
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

        void form_MouseWheel(object sender, MouseEventArgs e)
        {
            var form = sender as Form;
            if (e.Delta > 0)
                form.Opacity += 0.05;
            if (e.Delta < 0) 
                form.Opacity -= 0.05;
            if (form.Opacity < 0.2)
                form.Opacity = 0.2;
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

        private void LoadForms()
        {
            var section = ConfigurationManager.GetSection("floatingForms") as FloatingFormsSection;
            if (section == null)
                return;

            foreach (var f in section.Forms)
            {
                if (String.IsNullOrWhiteSpace(f.Name))
                    continue;

                var img = generateImage(f.Width, f.Height);
                var form = newFloatingForm(f.Width, f.Height, (Bitmap)img);
                form.BackColor = f.Lock ? Color.LightYellow : Color.White;
                form.Opacity = f.Opacity;
                form.Left = f.X;
                form.Top = f.Y;
                forms.Add(form);
                form.Show();
            }
        }

        private void SaveForms()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = new FloatingFormsSection();
            foreach (var f in forms)
            {
                var form = new FloatingFormElement()
                {
                    Name = f.Name,
                    X = f.Left,
                    Y = f.Top,
                    Width = f.Width,
                    Height = f.Height,
                    Opacity = f.Opacity,
                    Lock = f.BackColor != Color.White
                };

                section.Forms.Add(form);
            }

            if (config.Sections["floatingForms"] != null)
                config.Sections.Remove("floatingForms");

            config.Sections.Add("floatingForms", section);
            section.SectionInformation.ForceSave = true;
            config.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadForms();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveForms();
        }   
    }

    class FloatingFormsSection : ConfigurationSection
    {
        [ConfigurationProperty("forms", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(FloatingFormCollection), 
            AddItemName = "addForm", 
            ClearItemsName = "clearForms",
            RemoveItemName = "removeForm")]
        public FloatingFormCollection Forms
        {
            get
            {
                return this["forms"] as FloatingFormCollection;
            }
        }
    }

    class FloatingFormCollection : ConfigurationElementCollection, IList<FloatingFormElement>
    {
        public FloatingFormCollection()
        {
            FloatingFormElement element = (FloatingFormElement)CreateNewElement();
            Add(element);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FloatingFormElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FloatingFormElement)element).Name;
        }

        protected override string ElementName
        {
            get
            {
                return "form";
            }
        }

        public void Add(FloatingFormElement element)
        {
            BaseAdd(element, true);
        }

        public void Clear()
        {
            BaseClear();
        }

        public bool Contains(FloatingFormElement element)
        {
            return !(BaseIndexOf(element) < 0);
        }

        public void CopyTo(FloatingFormElement[] array, int index)
        {
            base.CopyTo(array, index);
        }

        public bool Remove(FloatingFormElement element)
        {
            BaseRemove(GetElementKey(element));
            return true;
        }

        bool ICollection<FloatingFormElement>.IsReadOnly
        {
            get { return IsReadOnly(); }
        }

        public int IndexOf(FloatingFormElement element)
        {
            return BaseIndexOf(element);
        }

        public void Insert(int index, FloatingFormElement element)
        {
            BaseAdd(index, element);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public FloatingFormElement this[int index]
        {
            get
            {
                return (FloatingFormElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new IEnumerator<FloatingFormElement> GetEnumerator()
        {
            return this.OfType<FloatingFormElement>().GetEnumerator();
        }
    }

    class FloatingFormElement : ConfigurationElement
    {
        public FloatingFormElement() { }


        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }
        [ConfigurationProperty("isLock", IsRequired = true)]
        public bool Lock
        {
            get
            {
                return (bool)this["isLock"];
            }
            set
            {
                this["isLock"] = value;
            }
        }
        [ConfigurationProperty("x", IsRequired = true)]
        public int X
        {
            get
            {
                return (int)this["x"];
            }
            set
            {
                this["x"] = value;
            }
        }
        [ConfigurationProperty("y", IsRequired = true)]
        public int Y
        {
            get
            {
                return (int)this["y"];
            }
            set
            {
                this["y"] = value;
            }
        }
        [ConfigurationProperty("width", IsRequired = true)]
        public int Width
        {
            get
            {
                return (int)this["width"];
            }
            set
            {
                this["width"] = value;
            }
        }
        [ConfigurationProperty("height", IsRequired = true)]
        public int Height
        {
            get
            {
                return (int)this["height"];
            }
            set
            {
                this["height"] = value;
            }
        }
        [ConfigurationProperty("opacity", IsRequired = true)]
        public double Opacity
        {
            get
            {
                return (double)this["opacity"];
            }
            set
            {
                this["opacity"] = value;
            }
        }
    }
}
