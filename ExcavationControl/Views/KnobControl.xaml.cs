using System;
using System.Collections.Generic;
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
    /// KnobControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KnobControl : UserControl
    {
        public KnobControl()
        {
            InitializeComponent();
        }
        public static bool _isPressed = false;
        private Canvas _templateCanvas = null;

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Enable moving mouse to change the value.
            _isPressed = true;
        }

        private void Ellipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Disable moving mouse to change the value.
            _isPressed = false;
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPressed)
            {
                //Find the parent canvas.
                if (_templateCanvas == null)
                {
                    _templateCanvas = MyHelper.FindParent<Canvas>(e.Source as Ellipse);
                    if (_templateCanvas == null) return;
                }
                //Canculate the current rotation angle and set the value.
                const double RADIUS = 150;
                Point newPos = e.GetPosition(_templateCanvas);
                double angle = MyHelper.GetAngleR(newPos, RADIUS);
                knob.Value = (knob.Maximum - knob.Minimum) * angle / (2 * Math.PI);

                //Console.WriteLine("각 : " + knob.Value);

                Console.WriteLine();
            }
        }

        private void knob_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Console.WriteLine("노브값 : " + knob.Value);
        }
    }
    //The converter used to convert the value to the rotation angle.
    public class ValueAngleConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter,
                      System.Globalization.CultureInfo culture)
        {
            double value = (double)values[0];
            double minimum = (double)values[1];
            double maximum = (double)values[2];

            return MyHelper.GetAngle(value, maximum, minimum);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
              System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //Convert the value to text.
    public class ValueTextConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                  System.Globalization.CultureInfo culture)
        {
            double v = (double)value;
            return String.Format("{0:F2}", v);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public static class MyHelper
    {
        //Get the parent of an item.
        public static T FindParent<T>(FrameworkElement current)
          where T : FrameworkElement
        {
            do
            {
                current = VisualTreeHelper.GetParent(current) as FrameworkElement;

                if (current is T)
                {
                    return (T)current;
                }
            }
            while (current != null);
            return null;
        }

        //Get the rotation angle from the value
        public static double GetAngle(double value, double maximum, double minimum)
        {
            double current = (value / (maximum - minimum)) * 360;
            double standard = (360 + 300) / 2;

            if (current == 300)
                current = 299.999;
            else if (standard > current && 300 < current)
            {
                current = 299.999;

                KnobControl._isPressed = false;
            }
                
            else if (standard < current && current > 0)
            {
                current = 0;

                KnobControl._isPressed = false;
            }
                

            return current;
        }

        //Get the rotation angle from the position of the mouse
        public static double GetAngleR(Point pos, double radius)
        {
            //Calculate out the distance(r) between the center and the position
            Point center = new Point(radius, radius);
            double xDiff = center.X - pos.X;
            double yDiff = center.Y - pos.Y;
            double r = Math.Sqrt(xDiff * xDiff + yDiff * yDiff);

            //Calculate the angle
            double angle = Math.Acos((center.Y - pos.Y) / r);
            Console.WriteLine("r:{0},y:{1},angle:{2}.", r, pos.Y, angle);
            if (pos.X < radius)
                angle = 2 * Math.PI - angle;
            if (Double.IsNaN(angle))
                return 0.0;
            else
                return angle;
        }
    }
}
