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

namespace ExcavationControl.Views
{
    /// <summary>
    /// MainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            HCKnob.knob.ValueChanged += HCKnob_ValueChanged;
            SCKnob.knob.ValueChanged += SCKnob_ValueChanged;
            CBKnob.knob.ValueChanged += CBKnob_ValueChanged;
            EXKnob.knob.ValueChanged += EXKnob_ValueChanged;
        }

        private void HCKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(HCKnob.knob.Value);

            if (value <= 120 && value >= 101)
                return;

            HText.Text = Math.Round(HCKnob.knob.Value).ToString();

        }

        private void SCKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(SCKnob.knob.Value);

            if (value <= 120 && value >= 101)
                return;

            SText.Text = Math.Round(SCKnob.knob.Value).ToString();
        }

        private void CBKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(CBKnob.knob.Value);

            if (value <= 120 && value >= 101)
                return;

            CText.Text = Math.Round(CBKnob.knob.Value).ToString();
        }

        private void EXKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(EXKnob.knob.Value);

            if (value <= 120 && value >= 101)
                return;

            EText.Text = Math.Round(EXKnob.knob.Value).ToString();
        }

        private void HUp_Click(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(HText.Text);

            HText.Text = (++baseValue).ToString();
        }

        private void HDown_Click(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(HText.Text);

            HText.Text = (--baseValue).ToString();
        }

        private void HButton_Clicked(object sender, RoutedEventArgs e)
        {
            Button selectedButton = (Button)sender;
            switch (selectedButton.Uid)
            {
                case "2":
                    Debug.WriteLine("Right Button");
                    break;

                case "3":
                    Debug.WriteLine("Left Button");
                    break;

                case "4":
                    Debug.WriteLine("Stop Button");
                    break;

                case "5":
                    Debug.WriteLine("Start Button");
                    break;
            }
        }

        private void SButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            Button selectedButton = (Button)sender;
            switch (selectedButton.Uid)
            {
                case "0":
                    Debug.WriteLine("Up Button");
                    SText.Text = (++baseValue).ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");
                    SText.Text = (--baseValue).ToString();
                    break;

                case "2":
                    Debug.WriteLine("Right Button");
                    break;

                case "3":
                    Debug.WriteLine("Left Button");
                    break;

                case "4":
                    Debug.WriteLine("Stop Button");
                    break;

                case "5":
                    Debug.WriteLine("Start Button");
                    break;
            }
        }

        private void CButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            Button selectedButton = (Button)sender;
            switch (selectedButton.Uid)
            {
                case "0":
                    Debug.WriteLine("Up Button");
                    CText.Text = (++baseValue).ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");
                    CText.Text = (--baseValue).ToString();
                    break;

                case "2":
                    Debug.WriteLine("Right Button");
                    break;

                case "3":
                    Debug.WriteLine("Left Button");
                    break;

                case "4":
                    Debug.WriteLine("Stop Button");
                    break;

                case "5":
                    Debug.WriteLine("Start Button");
                    break;
            }
        }

        private void EButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            Button selectedButton = (Button)sender;
            switch (selectedButton.Uid)
            {
                case "0":
                    Debug.WriteLine("Up Button");
                    EText.Text = (++baseValue).ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");
                    EText.Text = (--baseValue).ToString();
                    break;

                case "2":
                    Debug.WriteLine("Right Button");
                    break;

                case "3":
                    Debug.WriteLine("Left Button");
                    break;

                case "4":
                    Debug.WriteLine("Stop Button");
                    break;

                case "5":
                    Debug.WriteLine("Start Button");
                    break;
            }
        }

        private void LPButton_Clicked(object sender, RoutedEventArgs e)
        {
            Button selectedButton = (Button)sender;
            switch (selectedButton.Uid)
            {
                case "0":
                    Debug.WriteLine("Up Button");
                    break;

                case "1":
                    Debug.WriteLine("Down Button");
                    break;

                case "2":
                    Debug.WriteLine("Right Button");
                    break;

                case "3":
                    Debug.WriteLine("Left Button");
                    break;

                case "4":
                    Debug.WriteLine("Angle Button");
                    break;

                case "5":
                    Debug.WriteLine("Send Button");
                    break;
            }
        }
    }
}
