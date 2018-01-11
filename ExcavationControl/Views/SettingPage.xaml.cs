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
    /// SettingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingPage : Page
    {
        private SerialPort serial;

        public string _PortName { get; set; }
        public int _BaudRate { get; set; }
        public int _DataBits { get; set; }
        public Parity _Parity { get; set; }
        public StopBits _StopBits { get; set; }
        public Handshake _FlowControl { get; set; }

        public SettingPage()
        {

            InitializeComponent();

            serial = new SerialPort();

            BaudRateCombo.SelectedIndex = 7;
            DataBitsCombo.SelectedIndex = 3;
            ParityCombo.SelectedIndex = 0;
            StopBitsCombo.SelectedIndex = 0;
            FlowCtrlCombo.SelectedIndex = 0;
        }

        public SettingPage(SerialPort serial)
        {
            InitializeComponent();

            this.serial = serial;

            PortCombo.SelectedIndex = 0;
            BaudRateCombo.SelectedIndex = 7;
            DataBitsCombo.SelectedIndex = 3;
            ParityCombo.SelectedIndex = 0;
            StopBitsCombo.SelectedIndex = 0;
            FlowCtrlCombo.SelectedIndex = 0;
        }

        private void SerialInit()
        {
            try
            {
                serial.PortName = _PortName;
                serial.BaudRate = _BaudRate;
                serial.DataBits = _DataBits;
                serial.Parity = _Parity;
                serial.StopBits = _StopBits;
                serial.Handshake = _FlowControl;

                serial.Open();

                serial.DataReceived += Serial_DataReceived;

                Debug.WriteLine("포트열기 성공!");

                MainPage mainPage = new MainPage(serial);

                NavigationService.Navigate(mainPage);

                Application.Current.MainWindow.MinWidth = 1436;

                Application.Current.MainWindow.MinHeight = 1005;

                Application.Current.MainWindow.Width = 1446;

                Application.Current.MainWindow.Height = 1015;

                //Application.Current.MainWindow.MaxWidth = 1406;

                //Application.Current.MainWindow.MaxHeight = 980;

                //MainWindow._Serial.PortName = serial.PortName;
                //MainWindow._Serial.BaudRate = serial.BaudRate;
                //MainWindow._Serial.DataBits = serial.DataBits;
                //MainWindow._Serial.Parity = serial.Parity;
                //MainWindow._Serial.StopBits = serial.StopBits;
                //MainWindow._Serial.Handshake = serial.Handshake;
            }
            catch
            {
                string Message = "포트 연결에 실패 했습니다.";
                string Title = "경고!";
                MessageBoxImage image = MessageBoxImage.Warning;
                MessageBoxButton button = MessageBoxButton.OK;

                MessageBox.Show(App.Current.MainWindow, Message, Title, button, image);

                Debug.WriteLine("포트 열기 실패!");

                //MainWindow.flag = false;
            }
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //string recievedData;
            //recievedData = serial.ReadExisting();
            //Debug.WriteLine("받은 데이터 : " + recievedData);
        }

        private void PortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                _PortName = PortCombo.SelectedItem.ToString();
            }
            catch (NullReferenceException nre)
            {
                PortCombo.SelectedIndex = 0;
            }

        }

        private void BaudRateCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _BaudRate = int.Parse(((ComboBoxItem)BaudRateCombo.SelectedValue).Content.ToString());
            Debug.WriteLine("_BaudRate : " + _BaudRate);
        }

        private void DataBitsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _DataBits = int.Parse(((ComboBoxItem)DataBitsCombo.SelectedValue).Content.ToString());
            Debug.WriteLine("_DataBits : " + _DataBits);
        }

        private void ParityCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ParityCombo.SelectedIndex)
            {
                case 0:
                    _Parity = Parity.None;
                    Debug.WriteLine("_Parity None");
                    break;

                case 1:
                    _Parity = Parity.Odd;
                    Debug.WriteLine("_Parity Odd");
                    break;

                case 2:
                    _Parity = Parity.Even;
                    Debug.WriteLine("_Parity Even");
                    break;
            }
        }

        private void StopBitsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (StopBitsCombo.SelectedIndex)
            {
                case 0:
                    _StopBits = StopBits.One;
                    Debug.WriteLine("_StopBits One");
                    break;

                case 1:
                    _StopBits = StopBits.OnePointFive;
                    Debug.WriteLine("_StopBits OnePointFive");
                    break;

                case 2:
                    _StopBits = StopBits.Two;
                    Debug.WriteLine("_StopBits Two");
                    break;
            }
        }

        private void FlowCtrlCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (FlowCtrlCombo.SelectedIndex)
            {
                case 0:
                    _FlowControl = Handshake.None;
                    Debug.WriteLine("_FlowControl None");
                    break;

                case 1:
                    _FlowControl = Handshake.XOnXOff;
                    Debug.WriteLine("_FlowControl XonXoff");
                    break;

                case 2:
                    _FlowControl = Handshake.RequestToSend;
                    Debug.WriteLine("_FlowControl RTS");
                    break;
            }
        }

        private void PortCombo_DropDownOpened(object sender, EventArgs e)
        {
            Debug.WriteLine("Clicked");

            string[] AvailablePorts = SerialPort.GetPortNames();

            PortCombo.Items.Clear();

            foreach (var data in AvailablePorts)
            {
                if (!GetOpenedPortNames().Contains(data))
                {
                    Debug.WriteLine("Available Port : " + data);
                    PortCombo.Items.Add(data);
                }
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            SerialInit();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private List<string> GetOpenedPortNames()
        {
            List<string> list = new List<string>();

            foreach (var data in SerialPort.GetPortNames())
            {
                try
                {
                    SerialPort testSerial = new SerialPort(data);

                    testSerial.Open();

                    testSerial.Close();
                }
                catch
                {
                    Debug.WriteLine("Not Available Port : " + data);

                    list.Add(data);
                }
            }

            return list;
        }
    }
}
