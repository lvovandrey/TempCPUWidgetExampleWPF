using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media;

using System.Text;
using System.Management;                ///////////Надо добавить библиотеку самому!!!
using System.Management.Instrumentation;    ///////////Надо добавить библиотеку самому!!!
using OpenHardwareMonitor.Hardware;

namespace WidgetExampleNS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WidgetWindow : Window
    {
        public WidgetWindow()
        {
            InitializeComponent();

            this.Left = System.Windows.SystemParameters.PrimaryScreenWidth-73;
            this.Top = 0;
         

            var timer = new DispatcherTimer() {Interval = new TimeSpan(0, 0, 1)};
            timer.Tick += (o, O) => {
                lbMain.Content = GetTemp();
            };
            timer.Start();
        }


        string GetTemp()
        {
            Computer c = new Computer();
            c.CPUEnabled = true;
            c.Open();
            string res = "";
            foreach (var hardware in c.Hardware)
            {
                if (hardware.HardwareType == HardwareType.CPU)
                {
                    hardware.Update();
                    foreach (var sensors in hardware.Sensors)
                    {
                        if (sensors.SensorType == SensorType.Temperature)
                        {
                            res += (((sensors.Max.GetValueOrDefault()*10)/10).ToString())+ " ";
                            break;
                        }
                    }
                }
            }
            return res;
        }



        #region Bottommost

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        private void ToBack()
        {
            var handle = new WindowInteropHelper(this).Handle;
            SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            ToBack();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            ToBack();
        }

        #endregion

        #region Move

        private bool CanwinDragged = false;

        private bool winDragged = false;
        private Point lmAbs = new Point();

        void Window_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!CanwinDragged) return;
            winDragged = true;
            this.lmAbs = e.GetPosition(this);
            this.lmAbs.Y = Convert.ToInt16(this.Top) + this.lmAbs.Y;
            this.lmAbs.X = Convert.ToInt16(this.Left) + this.lmAbs.X;
            Mouse.Capture(this);
        }

        void Window_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
        {
            winDragged = false;
            Mouse.Capture(null);
        }

        void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (winDragged)
            {
                Point MousePosition = e.GetPosition(this);
                Point MousePositionAbs = new Point();
                MousePositionAbs.X = Convert.ToInt16(this.Left) + MousePosition.X;
                MousePositionAbs.Y = Convert.ToInt16(this.Top) + MousePosition.Y;
                this.Left = this.Left + (MousePositionAbs.X - this.lmAbs.X);
                this.Top = this.Top + (MousePositionAbs.Y - this.lmAbs.Y);
                this.lmAbs = MousePositionAbs;
            }
        }

        private void btMove_Click(object sender, RoutedEventArgs e)
        {
            CanwinDragged = !CanwinDragged;
            if (CanwinDragged) { lbMain.Foreground = new SolidColorBrush(Colors.Blue); btExit.Visibility = Visibility.Visible; }
            else { lbMain.Foreground = new SolidColorBrush(Colors.Lime); btExit.Visibility = Visibility.Collapsed; }
        }

        private void btExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void imgExit_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
    #endregion
}

