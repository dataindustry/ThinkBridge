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
using System.Windows.Forms;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;
using Newtonsoft.Json;
using System.IO;

namespace ThinkPadFanControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;

        private readonly String licenseKey =
            "NTg1NDA1QDMxMzkyZTM0MmUzMFRXQWxXME1naDN6MktNR2FQKzVjaWZDdGxGZ1NrdGpSSHlkdHN3Sy8ybFU9";

        private readonly string ini = System.IO.Path.Combine(Environment.CurrentDirectory, "ThinkPadFanControl.json");

        NotifyIcon icon;

        public MainWindow()
        {
            SyncfusionLicenseProvider.RegisterLicense(licenseKey);

            if (File.Exists(ini))
            {
                viewModel = JsonConvert.DeserializeObject<MainWindowViewModel>(File.ReadAllText(ini));
            }
            else
            {
                viewModel = new MainWindowViewModel
                {
                    Fan1ControlPlan = InitializeChart(),
                    Fan2ControlPlan = InitializeChart(),
                    IsECControl = true,
                    Profile = 0
                };
            }

            DataContext = viewModel;

            StartDevice();

            // enable temperture and fan speed monitoring
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

            // enable curve control
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

            // enable notify icon
            InitializeNotifyIcon();
            new Thread(new ThreadStart(delegate
            {

                try
                {

                    while (true)
                    {
                        if (icon is not null && icon.Icon is not null)
                        {
                            if (viewModel.CpuTemperture > viewModel.GpuTemperture)
                            {
                                icon.Icon = CreateIcon(viewModel.CpuTemperture, "CPU");
                            }
                            else
                            {
                                icon.Icon = CreateIcon(viewModel.GpuTemperture, "GPU");
                            }

                        }
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

            UpdateProfileEnableStatus();

            if (Math.Max(viewModel.CpuTemperture, viewModel.GpuTemperture) < 60)
            {
                viewModel.ColorMeteor = "Aquamarine";
            }
            else if (Math.Max(viewModel.CpuTemperture, viewModel.GpuTemperture) >= 80)
            {
                viewModel.ColorMeteor = "#FFFF7777";
            }
            else
            {
                viewModel.ColorMeteor = "Aquamarine";
            }

        }

        private void CurveControl()
        {
            if (viewModel.IsCurveControl == false) return;

            int fan1State = 0;
            var plan = viewModel.Fan1ControlPlan[viewModel.Profile];

            for (int i = 0; i < plan.Count - 1; i++)
            {
                if (viewModel.CpuTemperture > plan[i].Temperture)
                {
                    fan1State = plan[i + 1].FanState;
                }
            }

            int fan2State = 0;
            plan = viewModel.Fan2ControlPlan[viewModel.Profile];

            for (int i = 0; i < plan.Count - 1; i++)
            {
                if (viewModel.GpuTemperture > plan[i].Temperture)
                {
                    fan2State = plan[i + 1].FanState;
                }
            }

            SetFan1State(fan1State);
            SetFan2State(fan2State);
        }

        private static Icon CreateIcon(int temperture, string type)
        {

            Bitmap bitmap = new(96, 96);
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawString(temperture + "\n" + type, new Font("Tahoma", 13), System.Drawing.Brushes.White, new PointF(0, 0));
            Icon icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
            return icon;
        }
        private void InitializeNotifyIcon()
        {

            icon = new NotifyIcon
            {
                Icon = CreateIcon(temperture: 0, ""),
                Visible = true
            };

            ContextMenuStrip menu = new();

            ToolStripItem ECControlItem = new ToolStripMenuItem
            {
                Text = "EC Control"
            };
            ECControlItem.Click += delegate
            {
                // switch to ec control
            };

            ToolStripItem ManualControlItem = new ToolStripMenuItem
            {
                Text = "Manual Control"
            };
            ManualControlItem.Click += delegate
            {
                // switch to manual control
            };

            ToolStripItem CurveControlProfile1Item = new ToolStripMenuItem
            {
                Text = "Curve Control Profile 1"
            };
            CurveControlProfile1Item.Click += delegate
            {
                // switch to curve control
            };

            ToolStripItem CurveControlProfile2Item = new ToolStripMenuItem
            {
                Text = "Curve Control Profile 2"
            };
            CurveControlProfile2Item.Click += delegate
            {
                // switch to curve control
            };

            ToolStripItem CurveControlProfile3Item = new ToolStripMenuItem
            {
                Text = "Curve Control Profile 3"
            };
            CurveControlProfile3Item.Click += delegate
            {
                // switch to curve control
            };

            menu.Items.Add(ECControlItem);
            menu.Items.Add(ManualControlItem);
            menu.Items.Add(CurveControlProfile1Item);
            menu.Items.Add(CurveControlProfile2Item);
            menu.Items.Add(CurveControlProfile3Item);

            icon.ContextMenuStrip = menu;
            icon.MouseClick += delegate
            {
                this.Visibility = Visibility.Visible;
            };
        }

        private static ObservableCollection<ObservableCollection<FanControlPoint>> InitializeChart()
        {

            ObservableCollection<ObservableCollection<FanControlPoint>> plan = new();

            for (int i = 0; i < 3; i++)
            {
                ObservableCollection<FanControlPoint> profile = new ObservableCollection<FanControlPoint>
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

                plan.Add(profile);
            }

            return plan;

        }

        private static void SyncCurve(ObservableCollection<FanControlPoint>? source, ObservableCollection<FanControlPoint>? target)
        {
            if (source == null || target == null) return;

            for (int i = 0; i < source.Count; i++)
            {
                target[i].FanState = source[i].FanState;
            }
        }

        private void UpdateProfileEnableStatus()
        {
            bool[] setting = { true, true, true };
            setting[viewModel.Profile] = false;
            viewModel.IsProfile1Enabled = setting[0];
            viewModel.IsProfile2Enabled = setting[1];
            viewModel.IsProfile3Enabled = setting[2];
        }

        private void BtnECControl_Click(object sender, RoutedEventArgs e)
        {
            viewModel.IsECControl = true;
            viewModel.IsCurveControl = false;
            viewModel.IsManualControl = false;

            new Thread(new ThreadStart(delegate
            {

                try
                {
                    SetFan1State(0x80);
                    SetFan2State(0x80);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            })).Start();
        }

        private void ChartFan1Control_DragEnd(object sender, Syncfusion.UI.Xaml.Charts.ChartDragEndEventArgs e)
        {

            if (viewModel.IsCurveControlSync)
            {
                SyncCurve(viewModel.Fan1ControlPlan[viewModel.Profile], viewModel.Fan2ControlPlan[viewModel.Profile]);
            }
        }

        private void ChartFan2Control_DragEnd(object sender, Syncfusion.UI.Xaml.Charts.ChartDragEndEventArgs e)
        {

            if (viewModel.IsCurveControlSync)
            {
                SyncCurve(viewModel.Fan2ControlPlan[viewModel.Profile], viewModel.Fan1ControlPlan[viewModel.Profile]);
            }
        }

        private void CboFan1State_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (viewModel.IsManualControl == false) return;

            if (viewModel.IsManualControlSync)
            {
                string? newValue = viewModel.Fan1State;
                viewModel.Fan2State = newValue;
            }

            new Thread(new ThreadStart(delegate
            {

                try
                {
                    SetFan1State(Convert.ToInt32(viewModel.Fan1State, 16));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            })).Start();
        }

        private void CboFan2State_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (viewModel.IsManualControl == false) return;

            if (viewModel.IsManualControlSync)
            {
                string? newValue = viewModel.Fan2State;
                viewModel.Fan1State = newValue;
            }

            new Thread(new ThreadStart(delegate
            {

                try
                {
                    SetFan2State(Convert.ToInt32(viewModel.Fan2State, 16));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            })).Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            icon.Dispose();
            File.WriteAllText(ini, JsonConvert.SerializeObject(viewModel));
            CloseDevice();
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void BtnProfile1_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Profile = 0;
            UpdateProfileEnableStatus();
        }

        private void BtnProfile2_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Profile = 1;
            UpdateProfileEnableStatus();
        }

        private void BtnProfile3_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Profile = 2;
            UpdateProfileEnableStatus();
        }
    }
}
