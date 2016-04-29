using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace floatingPaper
{
    public partial class Form1 : Form
    {
        List<Form> forms;
        static int index = 0;

        [DllImport("user32", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static Dictionary<int, int> keys = new Dictionary<int, int>();
        private const int KEYCLEAR = 1;
        private const int KEYUNLOCK = 2;


        public Form1()
        {
            forms = new List<Form>();
            InitializeComponent();
        }


        private void RegisterHotkeys()
        {
            var id = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
            var result = RegisterHotKey(this.Handle, id, (uint)KeyModifiers.Alt, (uint)Keys.Pause);
            if (!result)
                throw new Exception();

            keys[KEYCLEAR] = id;

            id = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);
            result = RegisterHotKey(this.Handle, id, (uint)(KeyModifiers.Alt), (uint)Keys.Scroll);
            if (!result)
                throw new Exception();

            keys[KEYUNLOCK] = id;
        }

        private void UnregisterHotkeys()
        {
            UnregisterHotKey(this.Handle, keys[KEYCLEAR]);
            UnregisterHotKey(this.Handle, keys[KEYUNLOCK]);
        }

        private void ProcessHotkey(int keyId) //按下设定的键时调用该函数
        {
            if (keys[KEYCLEAR] == keyId)
            {
                forms.ForEach(f => f.Visible = !f.Visible);
            }
            else if (keys[KEYUNLOCK] == keyId)
            {
                forms.ForEach(f => {
                    var uf = (UntouchableForm)f;
                    uf.Lock = !uf.Lock;
                });
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;//如果m.Msg的值为0x0312那么表示用户按下了热键
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    ProcessHotkey((int)m.WParam);//按下热键时调用ProcessHotkey()函数
                    break;
            }
            base.WndProc(ref m); //将系统消息传递自父类的WndProc
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
            var pen = new Pen(brush, 1);
            using (var g = Graphics.FromImage(img))
            {
                var y = h / 2;

                //g.DrawLine(pen, margin, y, w - margin, y);
                for (var i = 0; i <= 100; ++i)
                {
                    var x = margin + width * i / 100;
                    var y1 = y - 5;
                    pen.Width = 1;
                    bool text = false;
                    if (i % 10 == 0)
                    {
                        y1 -= 10;
                        //pen.Width = 3;
                        text = true;
                    }
                    else if (i % 5 == 0)
                    {
                        y1 -= 5;
                        //pen.Width = 2;
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

        private Form newFloatingForm(int w, int h, Bitmap img, bool islock = false)
        {
            var form = new UntouchableForm();
            form.Name = String.Format("{0}", index++);
            form.ControlBox = false;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            form.Width = w;
            form.Height = h;
            form.TopMost = true;
            var tran = Color.FromArgb(0);
            img.MakeTransparent(tran);
            form.Region = bmprgn(img, tran);
            form.MouseDown += form_MouseDown;
            form.MouseUp += form_MouseUp;
            form.MouseMove += form_MouseMove;
            form.MouseDoubleClick += form_MouseDoubleClick;
            form.MouseWheel += form_MouseWheel;
            form.KeyDown += form_KeyDown;
            form.KeyUp += form_KeyUp;
            form.Tag = null;
            form.AllowTransparency = true;
            form.StartPosition = FormStartPosition.Manual;
            form.Lock = islock;
            form.ShowInTaskbar = false;

            return form;
        }

        void form_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            var form = sender as UntouchableForm;
            if (form.Lock)
                return;

            if (!e.Shift)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        form.Top--;
                        break;
                    case Keys.Down:
                        form.Top++;
                        break;
                    case Keys.Left:
                        form.Left--;
                        break;
                    case Keys.Right:
                        form.Left++;
                        break;
                    default:
                        return;
                }
            }
            else
            {
                if (form.BackColor != SystemColors.ControlDarkDark)
                {
                    form.Region = null;
                    if (form.Opacity >= 1)
                        form.Opacity = 0.8;
                    form.BackColor = SystemColors.ControlDarkDark;
                    form.BackgroundImage = generateImage(form.Width, form.Height);
                }
                switch (e.KeyCode)
                {
                    case Keys.Left:
                        form.Width--;
                        break;
                    case Keys.Right:
                        form.Width++;
                        break;
                    default:
                        return;
                }
                form.BackgroundImage = generateImage(form.Width, form.Height);
            }
        }

        void form_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            var form = sender as UntouchableForm;
            if (form.Lock)
                return;

            if (e.Shift || e.KeyCode == Keys.ShiftKey)
            {
                var img = (Bitmap)form.BackgroundImage;
                if (img == null) return;
                form.BackColor = Color.White;
                var tran = Color.FromArgb(0);
                img.MakeTransparent(tran);
                form.Region = bmprgn(img, tran);
                form.BackgroundImage = null;
            }
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
            if (form.Opacity < 0.1)
                form.Opacity = 0.1;
        }

        void form_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var form = sender as UntouchableForm;
            form.Lock = !form.Lock;
        }

        void form_MouseMove(object sender, MouseEventArgs e)
        {
            var form = sender as UntouchableForm;
            if (form.Lock)
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
            var form = sender as UntouchableForm;
            if (form.Lock)
                return;

            var d = form.Tag as DragObj;
            form.Tag = null;
            if (d == null)
                return;

            if (d.resize)
            {
                form.BackColor = Color.White;
                var tran = Color.FromArgb(0);
                var img = (Bitmap)form.BackgroundImage;
                img.MakeTransparent(tran);
                form.Region = bmprgn(img, tran);
                form.BackgroundImage = null;
            }
        }

        void form_MouseDown(object sender, MouseEventArgs e)
        {
            var form = sender as UntouchableForm;
            if (form.Lock)
                return;

            var d = new DragObj();
            d.loc = e.Location;

            if (form.Cursor == Cursors.SizeWE) 
            {
                form.Region = null;
                if (form.Opacity >= 1)
                    form.Opacity = 0.8;
                form.BackColor = SystemColors.ControlDarkDark;
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
                var form = newFloatingForm(f.Width, f.Height, (Bitmap)img, f.Lock);
                form.Opacity = f.Opacity;
                form.Left = f.X;
                form.Top = f.Y;
                forms.Add(form);
                form.Show();
                //(form as UntouchableForm).Lock = true;
            }
        }

        private void SaveForms()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = new FloatingFormsSection();
            foreach (var f in forms.OfType<UntouchableForm>())
            {
                var form = new FloatingFormElement()
                {
                    Name = f.Name,
                    X = f.Left,
                    Y = f.Top,
                    Width = f.Width,
                    Height = f.Height,
                    Opacity = f.Opacity,
                    Lock = f.Lock,
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
            RegisterHotkeys();
            LoadForms();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveForms();
            UnregisterHotkeys();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveForms();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (forms.Count <= 1)
                return;
            forms.Last().Close();
            forms.RemoveAt(forms.Count - 1);
        }   
    }


    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }

    public class UntouchableForm : Form 
    {
        private const int WM_MOUSEACTIVATE = 0x21;
        private const int MA_NOACTIVATEANDEAT = 4;
        private const int WM_NCHITTEST = 0x0084;

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr opt);
        [DllImport("user32.dll")]
        private static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetWindowLong(IntPtr Handle, int nIndex, uint dwNewLong);


        private static void forceSetForegroundWindow(IntPtr hWnd)
        {
            var mainThreadId = GetCurrentThreadId();
            IntPtr foregroundThreadID = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            if (foregroundThreadID != mainThreadId)
            {
                AttachThreadInput(foregroundThreadID, mainThreadId, true);
                SetForegroundWindow(hWnd);
                AttachThreadInput(foregroundThreadID, mainThreadId, false);
            }
            else
                SetForegroundWindow(hWnd);
        }

        private bool _lock;
        public bool Lock 
        { 
            get { return _lock; }
            set
            {
                _lock = value;
                if (_lock)
                {
                    BackColor = Color.OrangeRed;
                    var wl = GetWindowLong(Handle, -20);
                    wl |= 0x20;
                    SetWindowLong(Handle, -20, wl);
                }
                else
                {
                    var wl = GetWindowLong(Handle, -20);
                    wl ^= 0x20;
                    SetWindowLong(Handle, -20, wl);
                    BackColor = Color.White;
                }
            }
        }

        public UntouchableForm()
            : base()
        {
            Lock = false;
        }

        protected override void WndProc(ref Message m)
        {
            if (Lock)
            {
                if (m.Msg == WM_MOUSEACTIVATE)
                {
                    m.Result = new IntPtr(MA_NOACTIVATEANDEAT);
                    return;
                }
            }
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
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
