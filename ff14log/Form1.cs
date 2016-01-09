using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ff14log
{
    public partial class Form1 : Form
    {
        List<LogBattleItem> table = new List<LogBattleItem>();
        Form2 logForm;

        public Form1()
        {
            logForm = new Form2();
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            table.Clear();

            var a = new LogAnalyzer();
            var alllogs = openFileDialog1.FileNames;


            foreach (var file in alllogs)
            {
                var logs = LogParser.Parse(file);

                foreach (var l in logs)
                {
                    try
                    {
                        var i = a.Analyse(l);
                        if (i == null)
                            continue;
                        table.Add(i);

                        if (i.type != LogBattleType.Damage)
                            continue;

                        if (String.IsNullOrWhiteSpace(i.main))
                            continue;

                        var target = i.GetName(i.main);

                        if (comboBox1.Items.Contains(target))
                            continue;

                        comboBox1.Items.Add(target);
                    }
                    catch (Exception ex)
                    {
                        logForm.textBox1.AppendText(ex.Message + Environment.NewLine);
                        logForm.Show();
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listView1.Items.Clear();

            var target = (string)comboBox1.SelectedItem;

            IEnumerable<LogBattleItem> t;
            IEnumerable<IGrouping<string, LogBattleItem>> targets;
            if (checkBox1.Checked)
            {
                t = table.Where(i => i.type == LogBattleType.Damage && i.GetName(i.target) == target);
                targets = t.GroupBy(l => l.main);
            }
            else
            {
                t = table.Where(i => i.type == LogBattleType.Damage && i.GetName(i.main) == target);
                targets = t.GroupBy(l => l.target);
            }

            var tdps = 0;
            var tdamage = 0;

            var r = targets.Select(g => new { n = g.Key, d = g.Sum(i => i.Hp), s = g.Max(i => i.time) - g.Min(i => i.time) })
                .OrderByDescending(i => i.d);

            foreach (var g in r)
            {
                var item = new ListViewItem(g.n);
                var d = g.d;
                var secs = g.s;
                if (secs <= 0) secs = 1;

                var dps = d / secs;

                item.SubItems.Add(d.ToString());
                item.SubItems.Add(dps.ToString());
                listView1.Items.Add(item);

                tdps += dps;
                tdamage += d;
            }

            textBox1.Text = tdamage.ToString();
            textBox2.Text = tdps.ToString();
            //var damage = table.Where(i => i.type == LogBattleType.Damage && i.GetName(i.main) == target).Sum(i => i.Hp);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            comboBox1_SelectedIndexChanged(sender, e);
        }
    }
}
