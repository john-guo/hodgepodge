﻿using System;
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
            for (int i = 0; i < 1; ++i)
            {
                AddJob();
            }
            timer1.Start();
        }

        private void action(Job job)
        {
            var sp = job.GetSavePoint<Tuple<int, int>>();

            if (sp == null)
                sp = new Tuple<int, int>(0, 0);

            for (int i = sp.Item1; i < 100000; ++i)
            {
                SpinWait.SpinUntil(() =>
                {
                    for (int j = sp.Item2; j < 10000; ++j)
                    {
                        job.CancelProcess(nj => new Tuple<int, int>(i, j));
                    }
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
            listView6.Items.Clear();

            var doneJobs = (from j in scheduler.AllJobs
                            where j.Status == JobStatus.Done
                            select j).ToList();

            label2.Text = scheduler.AllJobs.Count().ToString();
            label3.Text = scheduler.AllTasks.Count().ToString();
            label4.Text = scheduler.PenddingQueue.Count().ToString();
            label5.Text = doneJobs.Count.ToString();
            label6.Text = scheduler.StopJobs.Count().ToString();

            foreach (var job in scheduler.AllJobs)
            {
                listView2.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }));
            }
            foreach (var job in scheduler.AllTasks)
            {
                var sp = job.GetSavePoint<Tuple<int, int>>();
                if (sp == null)
                    listView3.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }) { Tag = job });
                else
                    listView3.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), String.Format("{0} {1}", sp.Item1, sp.Item2) }) { Tag = job });
            }
            foreach (var job in scheduler.PenddingQueue)
            {
                listView4.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }));
            }
            foreach (var job in doneJobs)
            {
                listView5.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }));
            }
            foreach (var job in scheduler.StopJobs)
            {
                var sp = job.GetSavePoint<Tuple<int, int>>();
                if (sp == null)
                    listView6.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), job.Status.ToString() }) { Tag = job });
                else
                    listView6.Items.Add(new ListViewItem(new string[] { job.GetTag<int>().ToString(), String.Format("{0} {1}", sp.Item1, sp.Item2) }) { Tag = job });
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count <= 0)
                return;

            var item = listView3.SelectedItems[0];

            var job = item.Tag as Job;

            scheduler.StopJob(job);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView6.SelectedItems.Count <= 0)
                return;

            var item = listView6.SelectedItems[0];

            var job = item.Tag as Job;

            scheduler.RestartJob(job);
        }
    }
}
