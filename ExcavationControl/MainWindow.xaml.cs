using ExcavationControl.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ExcavationControl
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Application.Current.MainWindow.WindowState = WindowState.Maximized;

            HCKnob.knob.ValueChanged += HCKnob_ValueChanged;
            SCKnob.knob.ValueChanged += SCKnob_ValueChanged;
            CBKnob.knob.ValueChanged += CBKnob_ValueChanged;
            EXKnob.knob.ValueChanged += EXKnob_ValueChanged;
        }

        private void HCKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(HCKnob.knob.Value);

            int standard = (120 + 100) / 2;
            //노브 코드 적용 요함
            if(value == 120)

            HText.Text = Math.Round(HCKnob.knob.Value).ToString();
            
        }

        private void SCKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SText.Text = Math.Round(SCKnob.knob.Value).ToString();

        }

        private void CBKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CText.Text = Math.Round(CBKnob.knob.Value).ToString();

        }

        private void EXKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            EText.Text = Math.Round(EXKnob.knob.Value).ToString();

        }
    }
}
