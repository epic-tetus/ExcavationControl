using ExcavationControl.Controllers;
using ExcavationControl.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Threading;

namespace ExcavationControl.Views
{
    /// <summary>
    /// MainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainPage : Page
    {
        #region 전역변수 정의

        SerialPort _Serial;

        private FileStream fs;
        private StreamWriter sw;

        private string WriteString;

        private SettingModel HModel;
        private SettingModel SModel;
        private SettingModel CModel;
        private SettingModel EModel;
        private SettingModel RModel;

        private DataTable dataTable;
        private DataRow sensorDataRow;

        private List<TextBox> SBoxList;
        private List<TextBox> TBoxList;
        private List<TextBox> FBoxList;

        private SaveFileDialog fileDialog;
        private OpenFileDialog openDialog;

        #endregion

        #region 생성자

        public MainPage(SerialPort serial)
        {
            InitializeComponent();

            _Serial = serial;
            _Serial.DataReceived += _Serial_DataReceived;

            WriteString = string.Empty;

            HCKnob.knob.ValueChanged += HCKnob_ValueChanged;
            SCKnob.knob.ValueChanged += SCKnob_ValueChanged;
            CBKnob.knob.ValueChanged += CBKnob_ValueChanged;
            EXKnob.knob.ValueChanged += EXKnob_ValueChanged;

            HModel = new SettingModel();
            SModel = new SettingModel();
            CModel = new SettingModel();
            EModel = new SettingModel();
            RModel = new SettingModel();

            //SValueList = new List<string>();
            //TValueList = new List<string>();
            //FValueList = new List<string>();

            SBoxList = new List<TextBox>();
            TBoxList = new List<TextBox>();
            FBoxList = new List<TextBox>();

            for (int i = 1; i < 9; i++)
            {
                var ChildBox = VisualTreeHelper.GetChild(BottomImageGrid, i) as UIElement;

                SBoxList.Add((TextBox)ChildBox);

                Debug.WriteLine(((TextBox)ChildBox).Name);
            }

            for (int i = 1; i < 3; i++)
            {
                var ChildBox = VisualTreeHelper.GetChild(BottomImageGrid, i + 8) as UIElement;

                TBoxList.Add((TextBox)ChildBox);

                Debug.WriteLine(((TextBox)ChildBox).Name);
            }

            for (int i = 1; i < 3; i++)
            {
                var ChildBox = VisualTreeHelper.GetChild(BottomImageGrid, i + 11) as UIElement;

                FBoxList.Add((TextBox)ChildBox);

                Debug.WriteLine(((TextBox)ChildBox).Name);
            }

            SBoxList = SBoxList.OrderBy(x => x.Name).ToList();
            TBoxList = TBoxList.OrderBy(x => x.Name).ToList();
            FBoxList = FBoxList.OrderBy(x => x.Name).ToList();

            dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn("TimeStamp", typeof(DateTime)));
            dataTable.Columns.Add(new DataColumn("S1", typeof(double)));
            dataTable.Columns.Add(new DataColumn("S2", typeof(double)));
            dataTable.Columns.Add(new DataColumn("S3", typeof(double)));
            dataTable.Columns.Add(new DataColumn("S4", typeof(double)));
            dataTable.Columns.Add(new DataColumn("S5", typeof(double)));
            dataTable.Columns.Add(new DataColumn("S6", typeof(double)));
            dataTable.Columns.Add(new DataColumn("S7", typeof(double)));
            dataTable.Columns.Add(new DataColumn("S8", typeof(double)));
            dataTable.Columns.Add(new DataColumn("T1", typeof(double)));
            dataTable.Columns.Add(new DataColumn("T2", typeof(double)));
            dataTable.Columns.Add(new DataColumn("F1", typeof(double)));
            dataTable.Columns.Add(new DataColumn("F2", typeof(double)));
            dataTable.Columns.Add(new DataColumn("X", typeof(double)));
            dataTable.Columns.Add(new DataColumn("Y", typeof(double)));
            dataTable.Columns.Add(new DataColumn("Z", typeof(double)));

            sensorDataRow = null;
        }

        #endregion



        #region 사용자 정의 함수

        private void CommandWrite(string command)
        {
            try
            {
                byte[] ConvertedString = Encoding.Default.GetBytes(command);

                byte[] STX = new byte[1] { 0x02 };
                byte[] ETX = new byte[1] { 0x03 };
                byte[] CR = new byte[1] { 0x0D };
                byte[] LF = new byte[1] { 0x0A };

                IEnumerable<byte> result = STX.Concat(ConvertedString).Concat(CR).Concat(ETX);

                _Serial.Write(result.ToArray(), 0, result.ToArray().Length);

                Debug.WriteLine(command + "전송 성공!");
            }
            catch
            {
                Debug.WriteLine("전송 실패!");
            }
        }

        private void CommandWriteLine(string command)
        {
            try
            {
                byte[] ConvertedString = Encoding.Default.GetBytes(command);

                byte[] STX = new byte[1] { 0x02 };
                byte[] ETX = new byte[1] { 0x03 };
                byte[] CR = new byte[1] { 0x0D };
                byte[] LF = new byte[1] { 0x0A };

                IEnumerable<byte> result = STX.Concat(ConvertedString).Concat(CR).Concat(ETX);

                _Serial.WriteLine(result.ToString());

                Debug.WriteLine(command + "라인 전송 성공!");
            }
            catch
            {
                Debug.WriteLine("전송 실패!");
            }
        }

        private int GetRoundValue(decimal d)
        {
            Array array = new int[21] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 };

            int rowValue = (int)Math.Round(d);

            int index;

            if (rowValue % 5 > (5 / 2))
                index = rowValue / 5 + 1;
            else
                index = rowValue / 5;

            if (rowValue >= 101 || rowValue < 0)
                return (int)array.GetValue(-1);

            return (int)array.GetValue(index);
        }

        /// <summary>
        /// 해당하는 텍스트 박스를 찾는 함수
        /// </summary>
        /// <param name="s"></param>

        private void FindTextBox(string s)
        {
            string indicator = s.Remove(2);
            double value = double.Parse(s.Replace(indicator, string.Empty));

            Debug.WriteLine(string.Format("indicater : {0}, value : {1}", indicator, value));

            if (indicator.Contains("S"))
            {
                foreach (var data in SBoxList)
                    if (data.Name.Equals(indicator))
                        data.Text = value.ToString();
            }

            else if (indicator.Contains("T"))
            {
                foreach (var data in TBoxList)
                    if (data.Name.Equals(indicator))
                        data.Text = value.ToString();

                Angle.Text = (double.Parse(T1.Text) + double.Parse(T2.Text)).ToString();
            }

            else if (indicator.Contains("F"))
            {
                foreach (var data in FBoxList)
                    if (data.Name.Equals(indicator))
                        data.Text = value.ToString();

                FTotal.Text = (double.Parse(F1.Text) + double.Parse(F2.Text)).ToString();
            }

            //WriteString += string.Format("{0} : {1}    ", indicator, value);

            sensorDataRow[indicator] = value;
        }

        #endregion

        #region 시리얼 이벤트 함수

        private void _Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            sensorDataRow = dataTable.NewRow();

            var receivedData = _Serial.ReadExisting();

            Debug.WriteLine("받은 데이터 : " + _Serial.ReadExisting());

            if (receivedData.Contains("HCSTART-OK"))
                Debug.WriteLine("HC 시작 성공!");

            else if (receivedData.Contains("SCSTART-OK"))
                Debug.WriteLine("SC 시작 성공!");

            else if (receivedData.Contains("CBSTART-OK"))
                Debug.WriteLine("CB 시작 성공!");

            else if (receivedData.Contains("EXSTART-OK"))
                Debug.WriteLine("EX 시작 성공!");

            else if (receivedData.Contains("EXAUTO-OK"))
                Debug.WriteLine("EX 자동 시작 성공!");

            else if (receivedData.Contains("HCSTART-OK"))
                Debug.WriteLine("EX 지침 시작 성공!");

            else if (receivedData.Contains("HCSTOP-OK"))
                Debug.WriteLine("HC 정지 성공!");

            else if (receivedData.Contains("SCSTOP-OK"))
                Debug.WriteLine("SC 정지 성공!");

            else if (receivedData.Contains("CBSTOP-OK"))
                Debug.WriteLine("CB 정지 성공!");

            else if (receivedData.Contains("EXSTOP-OK"))
                Debug.WriteLine("EX 정지 성공!");

            else if (receivedData.Contains("AASEND-OK"))
                Debug.WriteLine("AA 전송 성공!");

            //FindAngle(receivedData);

            // 아래의 비교구문들을 실행 하면 다른 것들이 안 됨! ==> Write를 하지 않고 WriteLine을 해야 문제가 없음!

            if (receivedData.Contains("SP-"))
            {
                List<string> CriteriaList = new List<string>();

                for (int i = 0; i < 8; i++)
                    CriteriaList.Add("S" + (i + 1));

                WriteString += "\r\n";

                foreach (var data in CriteriaList)
                {
                    int startIndex = receivedData.IndexOf(data);

                    string result = receivedData.Substring(startIndex, 6);

                    Debug.WriteLine("잘라낸 문자열 : " + result);

                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        FindTextBox(result);
                    }));
                }

                _Serial.DataReceived -= _Serial_DataReceived;

                CommandWriteLine("SP-OK");

                _Serial.DataReceived += _Serial_DataReceived;

                //CommandWrite("SP-OK");

            }

            if (receivedData.Contains("RS-"))
            {
                List<string> CriteriaList = new List<string>();

                for (int i = 0; i < 2; i++)
                    CriteriaList.Add("T" + (i + 1));

                WriteString += "\r\n";

                foreach (var data in CriteriaList)
                {
                    int startIndex = receivedData.IndexOf(data);

                    string result = receivedData.Substring(startIndex, 6);

                    Debug.WriteLine("잘라낸 문자열 : " + result);

                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        FindTextBox(result);
                    }));
                }

                _Serial.DataReceived -= _Serial_DataReceived;

                CommandWriteLine("RS-OK");

                _Serial.DataReceived += _Serial_DataReceived;
            }

            if (receivedData.Contains("RF-"))
            {
                List<string> CriteriaList = new List<string>();

                // 기준 문자열 리스트에 문자열은 한 개씩 추가

                for (int i = 0; i < 2; i++)
                    CriteriaList.Add("F" + (i + 1));

                // 기준 문자열 리스트에서 문자열은 한 개씩 가져온다

                foreach (var data in CriteriaList)
                {
                    int startIndex = receivedData.IndexOf(data);            // 기준 문자열 시작 인덱스 저장 

                    string result = receivedData.Substring(startIndex, 6);  // 전체 문자열에서 해당 문자열만 파싱

                    Debug.WriteLine("잘라낸 문자열 : " + result);

                    // 다른 스레드 내부에서는 UI단에 접근을 할 수 없기 떄문에 아래와 같은 구문을 사용

                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        FindTextBox(result);                                // 파싱한 값을 FindTextBox함수에 실어서 실행

                    }));
                }

                _Serial.DataReceived -= _Serial_DataReceived;               // 정보를 쓸 동안만 정보 받는 것을 정지.

                CommandWriteLine("RF-OK");                                  // CommandWrite를 하면 계속 기다리는 현상이 발생한다. 하지만, CommandWriteLine 함수를 실행시키면,
                                                                            //  내부에서 SerialPort.WriteLine 함수를 실행하여 딱 한 라인을 실행하고 응답 대기 없이 종료한다.
                _Serial.DataReceived += _Serial_DataReceived;               // 정보를 쓸 동안만 정보 받는 것을 재개.
            }

            if (receivedData.Contains("IS-"))
            {

                // 다른 스레드 내부에서는 UI단에 접근을 할 수 없기 떄문에 아래와 같은 구문을 사용

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    int startIndex = receivedData.IndexOf("X");                 // X 문자열시작 인덱스 저장

                    string x = receivedData.Substring(startIndex, 6);           // 전체 문자열에서 해당 문자열만 파싱

                    string indicator = x.Remove(1);                                     // 문자열에서 X 알파벳 파싱
                    double value = double.Parse(x.Replace(indicator, string.Empty));    // 문자열에서 X 알파벳을 제외한 부분 파싱후 value에 저장

                    sensorDataRow[indicator] = value;                           // 데이터로우에 indicator형 값에 value 저장. 

                    XAngle.Text = value.ToString();                             // XAngle 텍스트 박스에 value 값을 지정 

                    startIndex = receivedData.IndexOf("Y");                     // Y 문자열시작 인덱스 저장

                    string y = receivedData.Substring(startIndex, 6);           // 전체 문자열에서 해당 문자열만 파싱

                    indicator = y.Remove(1);                                    // 문자열에서 Z 알파벳 파싱
                    value = double.Parse(y.Replace(indicator, string.Empty));   // 문자열에서 Z 알파벳을 제외한 부분 파싱후 value에 저장

                    sensorDataRow[indicator] = value;                           // 데이터로우에 indicator형 값에 value 저장. 

                    YAngle.Text = value.ToString();                             // YAngle 텍스트 박스에 value 값을 지정 

                    startIndex = receivedData.IndexOf("Z");                     // Z 문자열시작 인덱스 저장

                    string z = receivedData.Substring(startIndex, 6);           // 전체 문자열에서 해당 문자열만 파싱

                    indicator = z.Remove(1);                                    // 문자열에서 Z 알파벳 파싱
                    value = double.Parse(z.Replace(indicator, string.Empty));   // 문자열에서 Z 알파벳을 제외한 부분 파싱후 value에 저장

                    sensorDataRow[indicator] = value;                       // 데이터로우에 indicator형 값에 value 저장. 

                    ZAngle.Text = value.ToString();                         // ZAngle 텍스트 박스에 value 값을 지정 
                }));

                _Serial.DataReceived -= _Serial_DataReceived;               // 정보를 쓸 동안만 정보 받는 것을 정지.

                CommandWriteLine("IS-OK");                                  // CommandWrite를 하면 계속 기다리는 현상이 발생한다. 하지만, CommandWriteLine 함수를 실행시키면,
                                                                            //  내부에서 SerialPort.WriteLine 함수를 실행하여 딱 한 라인을 실행하고 응답 대기 없이 종료한다.

                _Serial.DataReceived += _Serial_DataReceived;               // 정보를 쓸 동안만 정보 받는 것을 재개.

                sensorDataRow["TimeStamp"] = DateTime.Now.ToLocalTime();    // 현재 시간 저장

                dataTable.Rows.Add(sensorDataRow);      // 데이터 테이블에 열을 추가

                fs = new FileStream(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SensorResult.csv"), FileMode.OpenOrCreate, FileAccess.Write);     // 저장 될 파일의 위치

                sw = new StreamWriter(fs, Encoding.UTF8);   // 문자를 파일에 쓰기위한 StreamWriter

                string line = string.Join(",", dataTable.Columns.Cast<object>());   //컬럼 이름들을 ","로 나누고 저장.
                sw.WriteLine(line);     // StreamWriter로 파일에 라인 쓰기

                //row들을 ","로 나누고 저장.
                foreach (DataRow item in dataTable.Rows)
                {
                    line = string.Join(",", item.ItemArray.Cast<object>());     // 데이터 테이블의 열을 차례대로 불러와서 ","로 나누고 저장
                    sw.WriteLine(line);     // StreamWriter로 파일에 라인 쓰기
                }

                sw.Close();     // StreamWriter 자원 free
                fs.Close();     // FileStream 자원 free
            }

        }
        #endregion

        #region UI 이벤트 함수

        #region 노브 값 변경 이벤트 함수
        private void HCKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(HCKnob.knob.Value);

            if (value <= 120 && value >= 101)
                return;

            HCKnob.knob.Value = GetRoundValue(value);

            HText.Text = Math.Round(HCKnob.knob.Value).ToString();
        }

        private void SCKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(SCKnob.knob.Value);

            if (value <= 120 && value >= 101)
                return;

            SCKnob.knob.Value = GetRoundValue(value);

            SText.Text = Math.Round(SCKnob.knob.Value).ToString();
        }

        private void CBKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(CBKnob.knob.Value);

            if (value <= 120 && value >= 101)
                return;

            CBKnob.knob.Value = GetRoundValue(value);

            CText.Text = Math.Round(CBKnob.knob.Value).ToString();
        }

        private void EXKnob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = (int)Math.Round(EXKnob.knob.Value);

            if (value <= 120 && value >= 101)
                return;

            EXKnob.knob.Value = GetRoundValue(value);

            EText.Text = Math.Round(EXKnob.knob.Value).ToString();
        }

        #endregion

        #region Head Cutter 버튼 이벤트 함수

        private void HUp_Click(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(HText.Text);

            int destinationValue = baseValue + 5;

            if (destinationValue > 100)
            {
                MessageBoxImage boxImage = MessageBoxImage.Warning;
                MessageBoxButton boxButton = MessageBoxButton.OK;
                MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                string Title = "경고!";
                string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", destinationValue);

                MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                return;
            }

            HCKnob.knob.Value = destinationValue;

            HText.Text = destinationValue.ToString();
        }

        private void HDown_Click(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(HText.Text);

            int destinationValue = baseValue - 5;

            if (destinationValue < 0)
            {
                MessageBoxImage boxImage = MessageBoxImage.Warning;
                MessageBoxButton boxButton = MessageBoxButton.OK;
                MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                string Title = "경고!";
                string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", destinationValue);

                MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                return;
            }

            HCKnob.knob.Value = destinationValue;

            HText.Text = destinationValue.ToString();
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

                    HModel.Value = string.Format("{0:000}", int.Parse(HText.Text));

                    string checkString = string.Format("HCSTART-{0}{1}", HModel.Direction, HModel.Value);

                    CommandWrite(checkString);

                    break;
            }
        }
        #endregion

        #region Screw Conveyer 버튼 이벤트 함수

        private void SButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            int destinationValue = baseValue + 5;

            // 토글 버튼인지 버튼인지 모르기 때문에 UIElement로 일단 형 변환
            var selectedButton = sender as UIElement;

            switch (selectedButton.Uid)
            {
                case "0":
                    Debug.WriteLine("Up Button");
                    if (destinationValue > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", destinationValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    SCKnob.knob.Value = destinationValue;

                    SText.Text = (destinationValue).ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");

                    destinationValue -= 10;

                    if (destinationValue < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", destinationValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    SCKnob.knob.Value = destinationValue;

                    SText.Text = destinationValue.ToString();
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

                    SModel.Value = string.Format("{0:000}", int.Parse(SText.Text));

                    CommandWrite(string.Format("SCSTART-{0}{1}", SModel.Direction, SModel.Value));

                    break;
            }
        }
        #endregion

        #region Conveyor Belt 버튼 이벤트 함수

        private void CButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            int destinationValue = baseValue + 5;

            var selectedButton = sender as UIElement;

            switch (selectedButton.Uid)
            {
                case "0":
                    Debug.WriteLine("Up Button");

                    if (destinationValue > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", destinationValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    CBKnob.knob.Value = destinationValue;

                    CText.Text = destinationValue.ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");

                    destinationValue -= 10;

                    if (destinationValue < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", destinationValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    CBKnob.knob.Value = destinationValue;

                    CText.Text = destinationValue.ToString();
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

                    CModel.Value = string.Format("{0:000}", int.Parse(CText.Text));

                    CommandWrite(string.Format("CBSTART-{0}{1}", CModel.Direction, CModel.Value));

                    break;
            }
        }
        #endregion

        #region Excavation 버튼 이벤트 함수

        private void EButton_Clicked(object sender, RoutedEventArgs e)
        {
            int baseValue = int.Parse(SText.Text);

            int destinationValue = baseValue + 5;

            var selectedButton = sender as UIElement;

            switch (selectedButton.Uid)
            {
                case "0":
                    Debug.WriteLine("Up Button");

                    if (destinationValue > 100)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", destinationValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    EXKnob.knob.Value = destinationValue;

                    EText.Text = destinationValue.ToString();
                    break;

                case "1":
                    Debug.WriteLine("Down Button");

                    destinationValue -= 10;

                    if (destinationValue < 0)
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("선택하신 숫자는 {0}으로, \n선택 가능한 수의 범위를 넘었습니다.", destinationValue);

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        break;
                    }

                    EXKnob.knob.Value = destinationValue;

                    EText.Text = destinationValue.ToString();
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

                    EModel.Value = string.Format("{0:000}", int.Parse(EText.Text));

                    CommandWrite(string.Format("EXSTART-{0}{1}", EModel.Direction, EModel.Value));

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
            var selectedButton = sender as UIElement;

            switch (selectedButton.Uid)
            {
                case "0":
                    Debug.WriteLine("Up Button");

                    var _Parent = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(selectedButton)));

                    var angleButton = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_Parent, 2), 0), 0);

                    var downGrid = VisualTreeHelper.GetChild(((Grid)((ToggleButton)selectedButton).Parent).Parent, 1) as UIElement;

                    var downButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    var rightButton = VisualTreeHelper.GetChild(((Grid)downGrid), 0) as UIElement;

                    var leftButton = VisualTreeHelper.GetChild(((Grid)downGrid), 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + rightButton.Uid);
                    Debug.WriteLine("Changed Button's Uid :" + leftButton.Uid);
                    Debug.WriteLine("Changed Button's Uid :" + downButton.Uid);

                    ((ToggleButton)downButton).IsChecked = false;
                    ((ToggleButton)rightButton).IsChecked = false;
                    ((ToggleButton)leftButton).IsChecked = false;
                    ((ToggleButton)angleButton).IsChecked = false;

                    RModel.Direction = "U";

                    break;

                case "1":
                    Debug.WriteLine("Down Button");

                    var _Parent1 = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(selectedButton)));

                    var angleButton1 = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_Parent1, 2), 0), 0);

                    var downGrid1 = VisualTreeHelper.GetChild(((Grid)((ToggleButton)selectedButton).Parent).Parent, 1) as UIElement;

                    var upButton = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 0) as UIElement;

                    var rightButton1 = VisualTreeHelper.GetChild(((Grid)downGrid1), 0) as UIElement;

                    var leftButton1 = VisualTreeHelper.GetChild(((Grid)downGrid1), 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + rightButton1.Uid);
                    Debug.WriteLine("Changed Button's Uid :" + leftButton1.Uid);
                    Debug.WriteLine("Changed Button's Uid :" + upButton.Uid);

                    ((ToggleButton)upButton).IsChecked = false;
                    ((ToggleButton)rightButton1).IsChecked = false;
                    ((ToggleButton)leftButton1).IsChecked = false;
                    ((ToggleButton)angleButton1).IsChecked = false;

                    RModel.Direction = "D";

                    break;

                case "2":
                    Debug.WriteLine("Right Button");

                    var _Parent2 = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(selectedButton)));

                    var angleButton2 = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_Parent2, 2), 0), 0);

                    var upGrid = VisualTreeHelper.GetChild(((Grid)((ToggleButton)selectedButton).Parent).Parent, 0) as UIElement;

                    var leftButton2 = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 1) as UIElement;

                    var upButton1 = VisualTreeHelper.GetChild(((Grid)upGrid), 0) as UIElement;

                    var downButton1 = VisualTreeHelper.GetChild(((Grid)upGrid), 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + upButton1.Uid);
                    Debug.WriteLine("Changed Button's Uid :" + downButton1.Uid);
                    Debug.WriteLine("Changed Button's Uid :" + leftButton2.Uid);

                    ((ToggleButton)upButton1).IsChecked = false;
                    ((ToggleButton)downButton1).IsChecked = false;
                    ((ToggleButton)leftButton2).IsChecked = false;
                    ((ToggleButton)angleButton2).IsChecked = false;

                    RModel.Direction = "R";

                    break;

                case "3":
                    Debug.WriteLine("Left Button");

                    var _Parent3 = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(selectedButton)));

                    var angleButton3 = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(_Parent3, 2), 0), 0);

                    var upGrid1 = VisualTreeHelper.GetChild(((Grid)((ToggleButton)selectedButton).Parent).Parent, 0) as UIElement;

                    var rightButton2 = VisualTreeHelper.GetChild(((ToggleButton)selectedButton).Parent, 0) as UIElement;

                    var upButton2 = VisualTreeHelper.GetChild(((Grid)upGrid1), 0) as UIElement;

                    var downButton2 = VisualTreeHelper.GetChild(((Grid)upGrid1), 1) as UIElement;

                    Debug.WriteLine("Changed Button's Uid :" + upButton2.Uid);
                    Debug.WriteLine("Changed Button's Uid :" + downButton2.Uid);
                    Debug.WriteLine("Changed Button's Uid :" + rightButton2.Uid);

                    ((ToggleButton)upButton2).IsChecked = false;
                    ((ToggleButton)downButton2).IsChecked = false;
                    ((ToggleButton)rightButton2).IsChecked = false;
                    ((ToggleButton)angleButton3).IsChecked = false;

                    RModel.Direction = "L";

                    break;

                case "4":
                    Debug.WriteLine("Angle Button");

                    var P1 = VisualTreeHelper.GetParent(selectedButton);
                    var P2 = VisualTreeHelper.GetParent(P1);
                    var P3 = VisualTreeHelper.GetParent(P2);
                    var P4 = VisualTreeHelper.GetParent(P3);

                    Debug.WriteLine("First : " + P1);
                    Debug.WriteLine("Second : " + P2);
                    Debug.WriteLine("Third : " + P3);
                    Debug.WriteLine("Forth : " + P4);

                    Array upPanel = new UIElement[2] { VisualTreeHelper.GetChild( VisualTreeHelper.GetChild(P3,1) as UIElement,0) as UIElement,
                                                       VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(P3, 1) as UIElement, 1) as UIElement };

                    Array ToggleButtonArray = new ToggleButton[4] { (ToggleButton)VisualTreeHelper.GetChild(upPanel.GetValue(0) as UIElement , 0),
                                                                    (ToggleButton)VisualTreeHelper.GetChild(upPanel.GetValue(0) as UIElement, 1),
                                                                    (ToggleButton)VisualTreeHelper.GetChild(upPanel.GetValue(1) as UIElement, 0),
                                                                    (ToggleButton)VisualTreeHelper.GetChild(upPanel.GetValue(1) as UIElement, 1) };

                    foreach (ToggleButton data in ToggleButtonArray)
                    {
                        data.IsChecked = false;
                    }

                    RModel.Direction = "A";

                    break;

                case "5":
                    Debug.WriteLine("Send Button");

                    string sendText = "";

                    if (!AAText_1.Text.Contains("."))
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("형식에 맞게 숫자를 입력해주세요!");

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        AAText_1.Focus();

                        return;
                    }
                    else if (!AAText_2.Text.Contains("."))
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("형식에 맞게 숫자를 입력해주세요!");

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        AAText_1.Focus();

                        return;
                    }
                    else if (!AAText_3.Text.Contains("."))
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("형식에 맞게 숫자를 입력해주세요!");

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                        AAText_1.Focus();

                        return;
                    }

                    if (RModel.Direction == "U" || RModel.Direction == "D")
                        sendText = AAText_1.Text;
                    else if (RModel.Direction == "R" || RModel.Direction == "L")
                        sendText = AAText_2.Text;
                    else if (RModel.Direction == "A")
                        sendText = AAText_3.Text;

                    RModel.Value = string.Format("{0:000.0}", double.Parse(sendText.Replace("˚", string.Empty)));

                    CommandWrite(string.Format("AASEND-{0}{1}", RModel.Direction, RModel.Value));

                    break;
            }
        }
        #endregion

        #endregion

        #region 페이지 이벤트 함수

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size OldSize = e.PreviousSize;
            Size NowSize = e.NewSize;

            Debug.WriteLine("예전 메인 페이지 크기 : " + OldSize);
            Debug.WriteLine("현재 메인 페이지 크기 : " + NowSize);

            //// 사이즈가 작아짐
            //if (OldSize.Height - NowSize.Height > 0)
            //{
            //    if (FontSize - Math.Round((OldSize.Height - NowSize.Height)/100) <= 0)
            //        FontSize = 1;
            //    else
            //        FontSize = FontSize - Math.Round((OldSize.Height - NowSize.Height)/100);
            //    return;
            //}

            //// 사이즈가 커짐
            //else if (OldSize.Height - NowSize.Height < 0)
            //{
            //    if (FontSize - Math.Round((OldSize.Height - NowSize.Height)/100) <= 0)
            //        FontSize = 1;
            //    else
            //        FontSize = FontSize - Math.Round((OldSize.Height - NowSize.Height)/100);
            //    return;
            //}

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

        #endregion

        #region TextBox 관련

        #region Head Cutter Text 이벤트 함수

        private void HText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void HText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int value = int.Parse(HText.Text);

                    HCKnob.knob.Value = GetRoundValue(value);

                    HText.Text = Math.Round(HCKnob.knob.Value).ToString();
                }
                catch
                {
                    if (!HText.Text.Equals(string.Empty))
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("0 ~ 100 사이의 숫자를 입력해주세요!");

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);
                    }

                    return;
                }
            }
        }
        #endregion

        #region Screw Cutter Text 이벤트 함수

        private void SText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int value = int.Parse(HText.Text);

                    SCKnob.knob.Value = GetRoundValue(value);

                    SText.Text = Math.Round(SCKnob.knob.Value).ToString();
                }
                catch
                {
                    if (!SText.Text.Equals(string.Empty))
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("0 ~ 100 사이의 숫자를 입력해주세요!");

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                    }

                    return;
                }
            }
        }
        #endregion

        #region Conveyor Belt Text 이벤트 함수

        private void CText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int value = int.Parse(HText.Text);

                    CBKnob.knob.Value = GetRoundValue(value);

                    CText.Text = Math.Round(CBKnob.knob.Value).ToString();
                }
                catch
                {
                    if (!CText.Text.Equals(string.Empty))
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("0 ~ 100 사이의 숫자를 입력해주세요!");

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                    }

                    return;
                }
            }
        }

        #endregion

        #region Excavation Text 이벤트 함수

        private void EText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void EText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int value = int.Parse(HText.Text);

                    EXKnob.knob.Value = GetRoundValue(value);

                    EText.Text = Math.Round(EXKnob.knob.Value).ToString();
                }
                catch
                {
                    if (!EText.Text.Equals(string.Empty))
                    {
                        MessageBoxImage boxImage = MessageBoxImage.Warning;
                        MessageBoxButton boxButton = MessageBoxButton.OK;
                        MessageBoxOptions boxOptions = MessageBoxOptions.DefaultDesktopOnly;

                        string Title = "경고!";
                        string Content = string.Format("0 ~ 100 사이의 숫자를 입력해주세요!");

                        MessageBox.Show(Content, Title, boxButton, boxImage, MessageBoxResult.OK, options: boxOptions);

                    }

                    return;
                }
            }
        }
        #endregion

        #endregion


        ///// <summary>
        ///// 판넬 조정하는 버튼 (제일 상단 버튼 호버하면 보임)
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>

        //private void PanelControlButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if(PanelControlButton.Content.ToString() == "△")
        //    {
        //        RightPanel.Visibility = Visibility.Collapsed;
        //        MainGrid.Visibility = Visibility.Collapsed;
        //        PanelControlButton.Content = "▽";

        //        Grid.SetRow(BottomImageGrid, 0);
        //        Grid.SetRowSpan(BottomImageGrid, 2);

        //        BottomImage.Stretch = Stretch.Uniform;

        //        BottomImageGrid.HorizontalAlignment = HorizontalAlignment.Center;
        //        BottomImageGrid.VerticalAlignment = VerticalAlignment.Center;
        //    }
        //    else
        //    {
        //        RightPanel.Visibility = Visibility.Visible;
        //        MainGrid.Visibility = Visibility.Visible;
        //        PanelControlButton.Content = "△";
        //        Grid.SetRow(BottomImageGrid, 1);
        //        Grid.SetRowSpan(BottomImageGrid, 1);

        //        BottomImageGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
        //        BottomImageGrid.VerticalAlignment = VerticalAlignment.Stretch;
        //    }

        //}

        ///// <summary>
        ///// 마우스 호버를 위한 함수
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>

        //private void Page_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if(e.MouseDevice.GetPosition((Page)sender).Y <= 40)
        //        PanelControlButton.Visibility = Visibility.Visible;
        //    else PanelControlButton.Visibility = Visibility.Collapsed;
        //}
    }
}
