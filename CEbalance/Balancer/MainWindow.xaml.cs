using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CEbalance.Symbol;
using CEbalance.Math;
using CEbalance;

namespace Balancer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var equ = new Equation(textBox.Text);
                equ.Trim();
                label.Content = equ.ToString();
            }
            catch (Exception ex)
            {
                label.Content = "Equation Invalid.";
            }
        }
    }
}
