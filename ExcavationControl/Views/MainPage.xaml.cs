using ExcavationControl.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        #region 전역변수 정의

        SerialPort _Serial;

        private SettingModel HModel;
        private SettingModel SModel;
        private SettingModel CModel;
        private SettingModel EModel;

        #endregion

        #region 생성자

        public MainPage(SerialPort serial)
        {
            InitializeComponent();

            _Serial = serial;
            _Serial.DataReceived += _Serial_DataReceived;

            //Application.Current.MainWindow.WindowState = WindowState.Maximized;

            HCKnob.knob.ValueChanged += HCKnob_ValueChanged;
            SCKnob.knob.ValueChanged += SCKnob_ValueChanged;
            CBKnob.knob.ValueChanged += CBKnob_ValueChanged;
            EXKnob.knob.ValueChanged += EXKnob_ValueChanged;

            HModel = new SettingModel();
            SModel = new SettingModel();
            CModel = new SettingModel();
            EModel = new SettingModel();
        }

        #endregion

        #region 사용자 정의 함수
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
            var receivedData = _Serial.ReadExisting();

            Debug.WriteLine("받은 데이터 : " + receivedData);

            if (receivedData.Equals("HCSTART-OK"))
                Debug.WriteLine("HC 시작 성공!");

            else if (receivedData.Equals("SCSTART-OK"))
                Debug.WriteLine("SC 시작 성공!");

            else if (receivedData.Equals("CBSTART-OK"))
                Debug.WriteLine("CB 시작 성공!");

            else if (receivedData.Equals("EXSTART-OK"))
                Debug.WriteLine("EX 시작 성공!");

            else if (receivedData.Equals("EXAUTO-OK"))
                Debug.WriteLine("EX 자동 시작 성공!");

            else if (receivedData.Equals("HCSTART-OK"))
                Debug.WriteLine("EX 지침 시작 성공!");

            else if (receivedData.Equals("HCSTOP-OK"))
                Debug.WriteLine("HC 정지 성공!");

            else if (receivedData.Equals("SCSTOP-OK"))
                Debug.WriteLine("SC 정지 성공!");

            else if (receivedData.Equals("CBSTOP-OK"))
                Debug.WriteLine("CB 정지 성공!");

            else if (receivedData.Equals("EXSTOP-OK"))
                Debug.WriteLine("EX 정지 성공!");

        }
        #endregion

        #region UI 이벤트 함수

        #region 노브 값 변경 이벤트 함수
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

        #endregion

        #region Head Cutter 버튼 이벤트 함수

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

            HCKnob.knob.Value = ++baseValue;
            
            HText.Text = (baseValue).ToString();
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

            HCKnob.knob.Value = --baseValue;

            HText.Text = (baseValue).ToString();
        }

        private void HButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(HText.Text);

            var selectedButton = sender as UIElement;

            switch (selectedButton.Uid)
            {
                case "2":
                    Debug.WriteLine("Right Button");

                    var leftButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + leftButton.Uid);

                    ((ToggleButton)leftButton).IsChecked = false;

                    HModel.Direction = "R";

                    break;

                case "3":
                    Debug.WriteLine("Left Button");

                    var rightButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + rightButton.Uid);

                    ((ToggleButton)rightButton).IsChecked = false;

                    HModel.Direction = "L";

                    break;

                case "4":
                    Debug.WriteLine("Stop Button");

                    var startButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + startButton.Uid);

                    ((ToggleButton)startButton).IsChecked = false;

                    CommandWrite("HCSTOP");

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

                    var stopButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + stopButton.Uid);

                    ((ToggleButton)stopButton).IsChecked = false;

                    HModel.SliderValue = string.Format("{0:000}", int.Parse(HText.Text));

                    CommandWrite(string.Format("HCSTART-{0}{1}",HModel.Direction,HModel.SliderValue));

                    break;
            }
        }
        #endregion

        #region Screw Conveyer 버튼 이벤트 함수

        private void SButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            // 토글 버튼인지 버튼인지 모르기 때문에 UIElement로 일단 형 변환
            var selectedButton = sender as UIElement;

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

                    SCKnob.knob.Value = ++baseValue;

                    SText.Text = (baseValue).ToString();
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

                    SCKnob.knob.Value = --baseValue;

                    SText.Text = (baseValue).ToString();
                    break;

                case "2":
                    Debug.WriteLine("Right Button");

                    var leftButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + leftButton.Uid);

                    ((ToggleButton)leftButton).IsChecked = false;

                    SModel.Direction = "R";

                    break;

                case "3":
                    Debug.WriteLine("Left Button");

                    var rightButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + rightButton.Uid);

                    ((ToggleButton)rightButton).IsChecked = false;

                    SModel.Direction = "L";

                    break;

                case "4":
                    Debug.WriteLine("Stop Button");

                    var startButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + startButton.Uid);

                    ((ToggleButton)startButton).IsChecked = false;

                    CommandWrite("SCSTOP");

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

                    var stopButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + stopButton.Uid);

                    ((ToggleButton)stopButton).IsChecked = false;

                    SModel.SliderValue = string.Format("{0:000}", int.Parse(SText.Text));

                    CommandWrite(string.Format("SCSTART-{0}{1}", SModel.Direction, SModel.SliderValue));

                    break;
            }
        }
        #endregion

        #region Conveyor Belt 버튼 이벤트 함수

        private void CButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            var selectedButton = sender as UIElement;

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

                    CBKnob.knob.Value = ++baseValue;

                    CText.Text = (baseValue).ToString();
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

                    CBKnob.knob.Value = --baseValue;

                    CText.Text = (baseValue).ToString();
                    break;

                case "2":
                    Debug.WriteLine("Right Button");

                    var leftButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + leftButton.Uid);

                    ((ToggleButton)leftButton).IsChecked = false;

                    CModel.Direction = "R";

                    break;

                case "3":
                    Debug.WriteLine("Left Button");

                    var rightButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + rightButton.Uid);

                    ((ToggleButton)rightButton).IsChecked = false;

                    CModel.Direction = "L";

                    break;

                case "4":
                    Debug.WriteLine("Stop Button");

                    var startButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + startButton.Uid);

                    ((ToggleButton)startButton).IsChecked = false;

                    CommandWrite("CBSTOP");

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

                    var stopButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + stopButton.Uid);

                    ((ToggleButton)stopButton).IsChecked = false;

                    CModel.SliderValue = string.Format("{0:000}", int.Parse(CText.Text));

                    CommandWrite(string.Format("CBSTART-{0}{1}", CModel.Direction, CModel.SliderValue));

                    break;
            }
        }
        #endregion

        #region Excavation 버튼 이벤트 함수

        private void EButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            var selectedButton = sender as UIElement;

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

                    EXKnob.knob.Value = ++baseValue;

                    EText.Text = (baseValue).ToString();
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

                    EXKnob.knob.Value = --baseValue;

                    EText.Text = (baseValue).ToString();
                    break;

                case "2":
                    Debug.WriteLine("Advance Button");

                    var BackButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + BackButton.Uid);

                    ((ToggleButton)BackButton).IsChecked = false;

                    EModel.Direction = "A";

                    break;

                case "3":
                    Debug.WriteLine("Back Button");

                    var rightButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + rightButton.Uid);

                    ((ToggleButton)rightButton).IsChecked = false;

                    EModel.Direction = "B";

                    break;

                case "4":
                    Debug.WriteLine("Stop Button");

                    var startButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + startButton.Uid);

                    ((ToggleButton)startButton).IsChecked = false;

                    CommandWrite("EXSTOP");

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

                    var stopButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + stopButton.Uid);

                    ((ToggleButton)stopButton).IsChecked = false;

                    EModel.SliderValue = string.Format("{0:000}", int.Parse(EText.Text));

                    CommandWrite(string.Format("EXSTART-{0}{1}", EModel.Direction, EModel.SliderValue));

                    break;

                case "6":
                    Debug.WriteLine("Manual Button");

                    var autoButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 2) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + autoButton.Uid);

                    ((ToggleButton)autoButton).IsChecked = false;

                    CommandWrite("EXMANUAL");

                    break;

                case "7":
                    Debug.WriteLine("Auto Button");

                    var manualButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + manualButton.Uid);

                    ((ToggleButton)manualButton).IsChecked = false;

                    string result = string.Format("{0:00}", int.Parse(AutoControlBox.Text));

                    CommandWrite("EXAUTO-" + result);

                    break;
            }
        }
        #endregion

        #region Right Panel 버튼 이벤트 함수

        private void RPButton_Clicked(object sender, RoutedEventArgs e)
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

        #endregion

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size OldSize = e.PreviousSize;
            Size NowSize = e.NewSize;

            Debug.WriteLine("예전 메인 페이지 크기 : " + OldSize);
            Debug.WriteLine("현재 메인 페이지 크기 : " + NowSize);

            // 사이즈가 작아짐
            if (OldSize.Height - NowSize.Height > 0)
            {
                if (FontSize - Math.Round((OldSize.Height - NowSize.Height)/100) <= 0)
                    FontSize = 1;
                else
                    FontSize = FontSize - Math.Round((OldSize.Height - NowSize.Height)/100);
                return;
            }

            // 사이즈가 커짐
            else if (OldSize.Height - NowSize.Height < 0)
            {
                if (FontSize - Math.Round((OldSize.Height - NowSize.Height)/100) <= 0)
                    FontSize = 1;
                else
                    FontSize = FontSize - Math.Round((OldSize.Height - NowSize.Height)/100);
                return;
            }

            //// 사이즈가 작아짐
            //if (OldSize.Width - NowSize.Width > 0)
            //{
            //    if (FontSize - Math.Round((OldSize.Width - NowSize.Width)/100) <= 0)
            //        FontSize = 1;
            //    else
            //        FontSize = FontSize - Math.Round((OldSize.Width - NowSize.Width))/100;
            //    return;
            //}

            //// 사이즈가 커짐
            //else if (OldSize.Width - NowSize.Width < 0)
            //{
            //    if (FontSize - Math.Round((OldSize.Width - NowSize.Width)/100) <= 0)
            //        FontSize = 1;
            //    else
            //        FontSize = FontSize - Math.Round((OldSize.Width - NowSize.Width)/100);
            //    return;
            //}
        }
    }
}
