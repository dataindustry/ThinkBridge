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
using Syncfusion.Licensing;
using System.Collections.ObjectModel;

using static ThinkPadFanControl.PortIO;

namespace ThinkPadFanControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel viewModel;

        private readonly String licenseKey =
            "NTg1NDA1QDMxMzkyZTM0MmUzMFRXQWxXME1naDN6MktNR2FQKzVjaWZDdGxGZ1NrdGpSSHlkdHN3Sy8ybFU9";

        public MainWindow()
        {
            SyncfusionLicenseProvider.RegisterLicense(licenseKey);

            viewModel = new MainWindowViewModel();
            viewModel.Fan1ControlPlan = InitializeChart();
            viewModel.Fan2ControlPlan = InitializeChart();
            DataContext = viewModel;

            StartDevice();

            new Thread(new ThreadStart(delegate
            {

                try
                {

                    while (true)
                    {
                        UpdateViewModel();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            })).Start();

            new Thread(new ThreadStart(delegate
            {

                try
                {

                    while (true)
                    {
                        CurveControl();
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            })).Start();

            InitializeComponent();

        }

        private void UpdateViewModel()
        {

            viewModel.CpuName = ReadCpuName();
            viewModel.GpuName = ReadGpuName();
            viewModel.CpuTemperture = ReadCpuTemperture();
            viewModel.GpuTemperture = ReadGpuTemperture();
            viewModel.Fan1Speed = ReadFan1Speed();
            viewModel.Fan2Speed = ReadFan2Speed();

        }

        private void CurveControl()
        {
            int fan1State = 0;
            for (int i = 0; i < viewModel.fan1ControlPlan.Count - 1; i++)
            {
                if (viewModel.CpuTemperture > viewModel.fan1ControlPlan[i].Temperture) {
                    fan1State = viewModel.fan1ControlPlan[i + 1].FanState;
                }
            }

            int fan2State = 0;
            for (int i = 0; i < viewModel.fan2ControlPlan.Count - 1; i++)
            {
                if (viewModel.GpuTemperture > viewModel.fan2ControlPlan[i].Temperture)
                {
                    fan2State = viewModel.fan2ControlPlan[i + 1].FanState;
                }
            }

            SetFanStateLevel(fan1State, fan2State);
        }

        private void ManualControl()
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

        private static ObservableCollection<FanControlPoint> InitializeChart()
        {
            return _ = new ObservableCollection<FanControlPoint>
            {
                new FanControlPoint() {Temperture=0, FanState=0x00},
                new FanControlPoint() {Temperture=10, FanState=0x00},
                new FanControlPoint() {Temperture=20, FanState=0x01},
                new FanControlPoint() {Temperture=30, FanState=0x02},
                new FanControlPoint() {Temperture=40, FanState=0x03},
                new FanControlPoint() {Temperture=50, FanState=0x04},
                new FanControlPoint() {Temperture=60, FanState=0x05},
                new FanControlPoint() {Temperture=70, FanState=0x06},
                new FanControlPoint() {Temperture=80, FanState=0x07},
                new FanControlPoint() {Temperture=90, FanState=0x07},
                new FanControlPoint() {Temperture=100, FanState=0x07}
            };
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

        private void ChartFan1Control_DragEnd(object sender, Syncfusion.UI.Xaml.Charts.ChartDragEndEventArgs e)
        {
            for (int i = viewModel.fan1ControlPlan.Count - 1; i >= 0; i--)
            {
                if (i > 0 && viewModel.fan1ControlPlan[i].FanState < viewModel.fan1ControlPlan[i - 1].FanState)
                {
                    viewModel.fan1ControlPlan[i].FanState = viewModel.fan1ControlPlan[i - 1].FanState;
                }
            }
        }

        private void ChartFan2Control_DragEnd(object sender, Syncfusion.UI.Xaml.Charts.ChartDragEndEventArgs e)
        {
            for (int i = viewModel.fan2ControlPlan.Count - 1; i >= 0; i--)
            {
                if (i > 0 && viewModel.fan2ControlPlan[i].FanState < viewModel.fan2ControlPlan[i - 1].FanState)
                {
                    viewModel.fan2ControlPlan[i].FanState = viewModel.fan2ControlPlan[i - 1].FanState;
                }
            }
        }
    }
}
