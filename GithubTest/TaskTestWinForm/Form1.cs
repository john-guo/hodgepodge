using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskTest;

namespace TaskTestWinForm
{
    public partial class Form1 : Form
    {
        JobScheduler scheduler = new JobScheduler();
        int index = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; ++i)
            {
                AddJob();
            }
            timer1.Start();
        }

        private void action(Job job)
        {
            for (int i = 0; i < 100000; ++i)
            {
                SpinWait.SpinUntil(() =>
                {
                    for (int j = 0; j < 10000; ++j) ;
                    return true;
                });
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddJob();
        }

        private void AddJob()
        {
            var job = scheduler.PenddingJob(action, index++);
            listView1.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }));
            label1.Text = index.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listView2.Items.Clear();
            listView3.Items.Clear();
            listView4.Items.Clear();
            listView5.Items.Clear();

            var doneJobs = (from j in scheduler.AllJobs
                            where j.Status == JobStatus.Done
                            select j).ToList();

            label2.Text = scheduler.AllJobs.Count.ToString();
            label3.Text = scheduler.AllTasks.Keys.Count.ToString();
            label4.Text = scheduler.PenddingQueue.Count.ToString();
            label5.Text = doneJobs.Count.ToString();

            foreach (var job in scheduler.AllJobs)
            {
                listView2.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }));
            }

            foreach (var job in scheduler.AllTasks.Keys)
            {
                listView3.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }));
            }
            foreach (var job in scheduler.PenddingQueue)
            {
                listView4.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }));
            }
            foreach (var job in doneJobs)
            {
                listView5.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }));
            }
        }
    }
}
