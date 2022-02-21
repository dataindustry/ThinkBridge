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
using System.Runtime.InteropServices;

namespace ThinkPadFanControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StartDevice();

            SetFanStateLevel(0x80, 0x80);

            string a = ReadCpuName();
            string b = ReadGpuName();
            int c = ReadCpuTemperture();
            int d = ReadGpuTemperture();
            int e = ReadFan1Speed();
            int f = ReadFan2Speed();

            CloseDevice();
        }

        [DllImport("ThinkBridge.dll")]
        public static extern int StartDevice();

        [DllImport("ThinkBridge.dll")]
        public static extern int CloseDevice();

        [DllImport("ThinkBridge.dll")]
        public static extern int ReadCpuTemperture();

        [DllImport("ThinkBridge.dll")]
        public static extern int ReadGpuTemperture();

        [DllImport("ThinkBridge.dll")]
        public static extern int ReadFan1Speed();

        [DllImport("ThinkBridge.dll")]
        public static extern int ReadFan2Speed();

        [DllImport("ThinkBridge.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string ReadCpuName();

        [DllImport("ThinkBridge.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string ReadGpuName();

        [DllImport("ThinkBridge.dll")]
        public static extern int SetFanStateLevel(int Fan1StateLevel, int Fan2StateLevel);
    }
}
