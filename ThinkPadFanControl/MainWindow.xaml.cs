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
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace ThinkPadFanControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel;
        public MainWindow()
        {
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            // SetFanStateLevel(0x80, 0x80);

            new Thread(new ThreadStart(delegate
            {

                try
                {
                    StartDevice();

                    while (true)
                    {
                        UpdateViewModel();
                        // Thread.Sleep(100);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally {

                    CloseDevice();
                }


            })).Start();

            InitializeComponent();

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

        private void UpdateViewModel() {

            viewModel.CpuName = ReadCpuName();
            viewModel.GpuName = ReadGpuName();
            viewModel.CpuTemperture = ReadCpuTemperture();
            viewModel.GpuTemperture = ReadGpuTemperture();
            viewModel.Fan1Speed = ReadFan1Speed();
            viewModel.Fan2Speed = ReadFan2Speed();

        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {

            new Thread(new ThreadStart(delegate
            {

                try
                {
                    SetFanStateLevel(
                        Convert.ToInt32(viewModel.Fan1State, 16),
                        Convert.ToInt32(viewModel.Fan2State, 16));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            })).Start();
        }
    }
}
