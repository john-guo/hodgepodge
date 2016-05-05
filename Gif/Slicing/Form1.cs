using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FFmpeg;
using System.Threading;
using System.Diagnostics;
using video2gif;
using System.IO;

namespace Slicing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            FFmpegMediaInfo.InitDllDirectory();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            dynamic files = e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files)
            {
                listView1.Items.Add(file);
            }
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }


        FFmpegMediaInfo media;
        int w;
        int h;
        Stopwatch watch;
        Graphics g;

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems.OfType<ListViewItem>().FirstOrDefault();
            if (item == null)
                return;

            var filename = item.Text;

            Play(filename);
        }

        private void Play(string filename)
        {
            if (backgroundWorker1.IsBusy)
                return;

            w = pictureBox1.Width;
            h = pictureBox1.Height;

            pictureBox1.Image = new Bitmap(w, h);
            g = Graphics.FromImage(pictureBox1.Image);

            media = new FFmpegMediaInfo(filename);
            trackBar1.Maximum = (int)(media.Duration.TotalMilliseconds / 100);
            label1.Text = media.Duration.ToString();
            backgroundWorker1.RunWorkerAsync();
        }

        long start = 0;
        DateTime cur;
        bool stopped = false, seek = false;
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            cur = DateTime.Now;
            //watch = Stopwatch.StartNew();
            seek = false;
            //TimeSpan ts;
            //var state = media.BeginPlay(w, h);
            //do
            //{
            //    var bmp = media.Play(state, out ts);

            //    if (bmp == null)
            //        break;

            //    if (ts != TimeSpan.Zero)
            //    {
            //        var seconds = watch.ElapsedMilliseconds;
            //        if (seconds < ts.TotalMilliseconds)
            //        {
            //            SpinWait.SpinUntil(() => false, (int)(ts.TotalMilliseconds - seconds));
            //        }
            //    }

            //    g.DrawImage(bmp, 0, 0);
            //    bmp.Dispose();

            //    if (InvokeRequired)
            //    {
            //        Invoke((Action)delegate {
            //            pictureBox1.Invalidate();
            //            label2.Text = ts.ToString();
            //            trackBar1.Value = (int)(ts.TotalMilliseconds / 100);
            //        });
            //    }
            //    else
            //    {
            //        pictureBox1.Invalidate();
            //        label2.Text = ts.ToString();
            //        trackBar1.Value = (int)(ts.TotalMilliseconds / 100);
            //    }

            //} while (true);

            media.PlaybackFrames(TimeSpan.FromMilliseconds(start), TimeSpan.MaxValue, w, h,
            null,
            (bmp, ts, i) =>
            {

                if (ts != TimeSpan.Zero)
                {
                    var elapsed = (DateTime.Now - cur).TotalMilliseconds;

                    var seconds = start + elapsed;
                    if (seconds >= media.Duration.TotalMilliseconds)
                    {
                        stopped = true;
                    }
                   
                    if (seconds < ts.TotalMilliseconds)
                    {
                        SpinWait.SpinUntil(() => false, (int)(ts.TotalMilliseconds - seconds));
                    }
                }

                g.DrawImage(bmp, 0, 0);
                
                if (!stopped)
                {
                    if (InvokeRequired)
                    {
                        Invoke((Action)delegate
                        {
                            pictureBox1.Invalidate();
                            label2.Text = ts.ToString();
                            trackBar1.Value = (int)(ts.TotalMilliseconds / 100);
                        });
                    }
                    else
                    {
                        pictureBox1.Invalidate();
                        label2.Text = ts.ToString();
                        trackBar1.Value = (int)(ts.TotalMilliseconds / 100);
                    }
                }

                return !stopped;
            });
            //watch.Stop();
        }

        private void trackBar1_MouseCaptureChanged(object sender, EventArgs e)
        {
            seek = true;
            stopped = true;
            if (backgroundWorker1.IsBusy)
                backgroundWorker1.CancelAsync();
            start = trackBar1.Value * 100;

            //if (!backgroundWorker1.IsBusy)
            //    backgroundWorker1.RunWorkerAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stopped = false;
            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stopped = true;
            if (backgroundWorker1.IsBusy)
                backgroundWorker1.CancelAsync();
            start = trackBar1.Value * 100;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems.OfType<ListViewItem>().FirstOrDefault();
            if (item == null)
                return;

            var filename = item.Text;

            var gif = Path.ChangeExtension(filename, "gif");

            TimeSpan begin, end;
            if (!TimeSpan.TryParse(textBox1.Text, out begin))
            {
                begin = TimeSpan.Zero;
            }
            if (!TimeSpan.TryParse(textBox2.Text, out end))
            {
                end = TimeSpan.MaxValue;
            }

            int? width, height, delay;
            int w, h, d;

            if (!int.TryParse(textBox3.Text, out w))
            {
                width = null;
            }
            else
            {
                width = w;
            }

            if (!int.TryParse(textBox4.Text, out h))
            {
                height = null;
            }
            else
            {
                height = h;
            }

            if (!int.TryParse(textBox5.Text, out d))
            {
                delay = null;
            }
            else
            {
                delay = d;
            }

            Video2Gif.Process(filename, gif, begin, end, width, height, delay);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TimeSpan begin, end;
            if (!TimeSpan.TryParse(textBox1.Text, out begin))
            {
                begin = TimeSpan.Zero;
            }
            if (!TimeSpan.TryParse(textBox2.Text, out end))
            {
                end = TimeSpan.MaxValue;
            }

            int? width, height, delay;
            int w, h, d;

            if (!int.TryParse(textBox3.Text, out w))
            {
                width = null;
            }
            else
            {
                width = w;
            }

            if (!int.TryParse(textBox4.Text, out h))
            {
                height = null;
            }
            else
            {
                height = h;
            }

            if (!int.TryParse(textBox5.Text, out d))
            {
                delay = null;
            }
            else
            {
                delay = d;
            }

            foreach (var item in listView1.Items.OfType<ListViewItem>())
            {
                var filename = item.Text;

                var gif = Path.ChangeExtension(filename, "gif");

                Video2Gif.Process(filename, gif, begin, end, width, height, delay);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            stopped = false;

            if (seek)
            {
                //Thread.Sleep(500);

                //backgroundWorker1.RunWorkerAsync();
            }
        }
    }
}
