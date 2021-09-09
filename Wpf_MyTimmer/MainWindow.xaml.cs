using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Wpf_MyTimmer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer1 = null;
        DispatcherTimer timer = null;
        TimeSpan tspan;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += image_Loaded;
            this.Loaded += Window_Loaded;
            this.btn_StartTimmer.Visibility = Visibility.Hidden;
            timer1 = new DispatcherTimer();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1); //创建时分秒
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void rdoBtnclock_Click( object sender, RoutedEventArgs e )
        {
            if (timer1.IsEnabled)
            {
                timer1.Stop();
            }
            timer.Start();
            this.btn_StartTimmer.Visibility = Visibility.Hidden;
            this.tbh.Visibility = Visibility.Hidden;
            this.tbm.Visibility = Visibility.Hidden;
            this.tbs.Visibility = Visibility.Hidden;
        }

        private void rdoBtnTimer_Click( object sender, RoutedEventArgs e )
        {
            timer.Stop();

            this.btn_StartTimmer.Visibility = Visibility.Visible;
            this.tbh.Visibility = Visibility.Visible;
            this.tbm.Visibility = Visibility.Visible;
            this.tbs.Visibility = Visibility.Visible;

        }
        private void btn_StartTimmer_Click( object sender, RoutedEventArgs e )
        {
            //this.Hour.Content = tbh.Text;
            //this.Minute.Content = tbm.Text;
            //this.second.Content = tbs.Text;

            this.tbh.Visibility = Visibility.Hidden;
            this.tbm.Visibility = Visibility.Hidden;
            this.tbs.Visibility = Visibility.Hidden;

            tspan = new TimeSpan(int.Parse(tbh.Text), int.Parse(tbm.Text), int.Parse(tbs.Text));

            timer1.Interval = new TimeSpan(0, 0, 1);
            timer1.Tick += new EventHandler(Timer1_Tick);
            timer1.Start();
        }

        private void btn_StopTimmer_Click( object sender, RoutedEventArgs e )
        {
            timer1.Stop();
        }

        private void Timer1_Tick( object sender, EventArgs e )
        {
            this.Hour.Content = tspan.Hours;
            this.Minute.Content = tspan.Minutes;
            this.second.Content = tspan.Seconds;

            tspan = tspan.Subtract( new TimeSpan(0,0,1));

            if (tspan.TotalSeconds == 0)
            {
                timer1.Stop();
            }
        }


        private void Timer_Tick( object sender, EventArgs e )
        {
            DateTime now = DateTime.Now;
            this.Hour.Content = now.Hour.ToString();
            this.Minute.Content = now.Minute.ToString();
            this.second.Content = now.Second.ToString();
        }

        private void image_Loaded( object sender, RoutedEventArgs e )
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;

            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
        public static IntPtr SetWindowLong( IntPtr hWnd, int nIndex, IntPtr dwNewLong )
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }
        private static int IntPtrToInt32( IntPtr intPtr )
        {
            return unchecked((int)intPtr.ToInt64());
        }
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr( IntPtr hWnd, int nIndex, IntPtr dwNewLong );

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong( IntPtr hWnd, int nIndex, Int32 dwNewLong );


        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError( int dwErrorCode );

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong( IntPtr hWnd, int nIndex );
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        private void Window_Loaded( object sender, RoutedEventArgs e )
        {
            SetParent(new WindowInteropHelper(this).Handle, GetDesktopPtr());

        }
        /// <summary>
        /// 将程序嵌入桌面
        /// </summary>
        /// <returns></returns>
        private IntPtr GetDesktopPtr()//寻找桌面的句柄
        {
            // 情况一
            IntPtr hwndWorkerW = IntPtr.Zero;
            IntPtr hShellDefView = IntPtr.Zero;
            IntPtr hwndDesktop = IntPtr.Zero;
            IntPtr hProgMan = FindWindow("Progman", "Program Manager");
            if (hProgMan != IntPtr.Zero)
            {
                hShellDefView = FindWindowEx(hProgMan, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (hShellDefView != IntPtr.Zero)
                {
                    hwndDesktop = FindWindowEx(hShellDefView, IntPtr.Zero, "SysListView32", null);
                }
            }
            if (hwndDesktop != IntPtr.Zero) return hwndDesktop;

            // 情况二
            //必须存在桌面窗口层次
            while (hwndDesktop == IntPtr.Zero)
            {
                //获得WorkerW类的窗口
                hwndWorkerW = FindWindowEx(IntPtr.Zero, hwndWorkerW, "WorkerW", null);
                if (hwndWorkerW == IntPtr.Zero) break;
                hShellDefView = FindWindowEx(hwndWorkerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (hShellDefView == IntPtr.Zero) continue;
                hwndDesktop = FindWindowEx(hShellDefView, IntPtr.Zero, "SysListView32", null);
            }
            return hwndDesktop;
        }
        [DllImport("user32")]
        private static extern IntPtr FindWindow( string lpClassName, string lpWindowName );
        [DllImport("user32")]
        private static extern IntPtr FindWindowEx( IntPtr par1, IntPtr par2, String par3, String par4 );
        [DllImport("user32")]
        private static extern IntPtr SetParent( IntPtr hWndChild, IntPtr hWndNewParent );

        private void backGround_MouseMove( object sender, MouseEventArgs e )
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
            catch { }
        }

    }
}
