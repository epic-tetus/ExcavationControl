using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
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
        SerialPort _Serial;
        public MainPage(SerialPort serial)
        {
            InitializeComponent();

            _Serial = serial;
            _Serial.DataReceived += _Serial_DataReceived;

            Application.Current.MainWindow.WindowState = WindowState.Maximized;

            HCKnob.knob.ValueChanged += HCKnob_ValueChanged;
            SCKnob.knob.ValueChanged += SCKnob_ValueChanged;
            CBKnob.knob.ValueChanged += CBKnob_ValueChanged;
            EXKnob.knob.ValueChanged += EXKnob_ValueChanged;
        }

        #region 사용자 지정 함수
        private void CommandWrite(string command)
        {
            try
            {
                byte[] ConvertedString = Encoding.Default.GetBytes(command + "\r");

                byte[] STX = new byte[1] { 0x02 };
                byte[] ETX = new byte[1] { 0x03 };
                byte[] CR = new byte[1] { 0x0D };
                byte[] LF = new byte[1] { 0x0A };

                IEnumerable<byte> result = STX.Concat(ConvertedString).Concat(ETX);

                _Serial.Write(result.ToArray(), 0, result.ToArray().Length);

                Debug.WriteLine("전송 성공!");
            }
            catch
            {
                Debug.WriteLine("전송 실패!");
            }
        }
        #endregion

        #region 시리얼 이벤트 함수
        private void _Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Debug.WriteLine("받은 데이터 : " + _Serial.ReadExisting());
        }
        #endregion

        #region UI 이벤트 함수

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

            if ((baseValue + 1) > 100)
            {
                MessageBoxImage boxImage = MessageBoxImage.Warning;
                MessageBoxButton boxButton = MessageBoxButton.OK;
                MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                string Title = "경고!";
                string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue + 1);

                MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                return;
            }

            HText.Text = (++baseValue).ToString();
        }

        private void HDown_Click(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(HText.Text);

            if ((baseValue - 1) < 0)
            {
                MessageBoxImage boxImage = MessageBoxImage.Warning;
                MessageBoxButton boxButton = MessageBoxButton.OK;
                MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                string Title = "경고!";
                string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue - 1);

                MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                return;
            }

            HText.Text = (--baseValue).ToString();
        }

        private void HButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(HText.Text);

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

                    if (baseValue > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    else if (baseValue < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }



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
                    if( (baseValue+1) > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue + 1);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK ,options: boxOptions);

                        break;
                    }
                        
                    SText.Text = (++baseValue).ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");

                    if ((baseValue - 1) < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue - 1);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

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

                    if (baseValue > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    else if (baseValue < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

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

                    if ((baseValue + 1) > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue + 1);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    CText.Text = (++baseValue).ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");

                    if ((baseValue - 1) < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue - 1);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

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

                    if (baseValue > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    else if (baseValue < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

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

                    if ((baseValue + 1) > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue + 1);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    EText.Text = (++baseValue).ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");

                    if ((baseValue - 1) < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue - 1);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

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

                    if (baseValue > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    else if (baseValue < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", baseValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

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
        #endregion
    }
}
