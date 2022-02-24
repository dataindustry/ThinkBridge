using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Globalization;

namespace ThinkPadFanControl
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private string? cpuName;
        public string? CpuName
        {
            get => cpuName;
            set => SetProperty(ref cpuName, value);
        }

        private string? gpuName;
        public string? GpuName
        {
            get => gpuName;
            set => SetProperty(ref gpuName, value);
        }

        private int gpuTemperture;
        public int GpuTemperture
        {
            get => gpuTemperture;
            set => SetProperty(ref gpuTemperture, value);
        }

        private int cpuTemperture;
        public int CpuTemperture
        {
            get => cpuTemperture;
            set => SetProperty(ref cpuTemperture, value);
        }

        private int fan1Speed;
        public int Fan1Speed
        {
            get => fan1Speed;
            set => SetProperty(ref fan1Speed, value);
        }

        private int fan2Speed;
        public int Fan2Speed
        {
            get => fan2Speed;
            set => SetProperty(ref fan2Speed, value);
        }

        private string? colorMeteor;
        public string? ColorMeteor
        {
            get => colorMeteor;
            set => SetProperty(ref colorMeteor, value);
        }


        private string? fan1State;
        public string? Fan1State
        {
            get => fan1State;
            set => SetProperty(ref fan1State, value);
        }

        private string? fan2State;
        public string? Fan2State
        {
            get => fan2State;
            set => SetProperty(ref fan2State, value);
        }

        private bool isECControl;
        public bool IsECControl
        {
            get => isECControl;
            set => SetProperty(ref isECControl, value);
        }

        private bool isManualControl;
        public bool IsManualControl
        {
            get => isManualControl;
            set => SetProperty(ref isManualControl, value);
        }

        private bool isCurveControl;
        public bool IsCurveControl
        {
            get => isCurveControl;
            set => SetProperty(ref isCurveControl, value);
        }

        private bool isManualControlSync;
        public bool IsManualControlSync
        {
            get => isManualControlSync;
            set => SetProperty(ref isManualControlSync, value);
        }

        private bool isCurveControlSync;
        public bool IsCurveControlSync
        {
            get => isCurveControlSync;
            set => SetProperty(ref isCurveControlSync, value);
        }

        private int profile;
        public int Profile
        {
            get => profile;
            set => SetProperty(ref profile, value);
        }

        private bool isProfile1Enabled;
        public bool IsProfile1Enabled
        {
            get => isProfile1Enabled;
            set => SetProperty(ref isProfile1Enabled, value);
        }

        private bool isProfile2Enabled;
        public bool IsProfile2Enabled
        {
            get => isProfile2Enabled;
            set => SetProperty(ref isProfile2Enabled, value);
        }

        private bool isProfile3Enabled;
        public bool IsProfile3Enabled
        {
            get => isProfile3Enabled;
            set => SetProperty(ref isProfile3Enabled, value);
        }

        private ObservableCollection<ObservableCollection<FanControlPoint>>? fan1ControlPlan;
        public ObservableCollection<ObservableCollection<FanControlPoint>>? Fan1ControlPlan
        {
            get => fan1ControlPlan;
            set => SetProperty(ref fan1ControlPlan, value);
        }

        private ObservableCollection<ObservableCollection<FanControlPoint>>? fan2ControlPlan;
        public ObservableCollection<ObservableCollection<FanControlPoint>>? Fan2ControlPlan
        {
            get => fan2ControlPlan;
            set => SetProperty(ref fan2ControlPlan, value);
        }

    }

    internal class FanControlPoint : INotifyPropertyChanged

    {

        public int temperture;

        public int Temperture
        {
            get => temperture;
            set { temperture = value; OnPropertyChanged(nameof(Temperture)); }
        }

        public int fanState;

        public int FanState
        {
            get => fanState;
            set { fanState = value; OnPropertyChanged(nameof(FanState)); }
        }
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

    }

    internal class ChartProfileConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var plan = (ObservableCollection<ObservableCollection<FanControlPoint>>)values[0];
            int profile = (int)values[1];
            return plan[profile];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
