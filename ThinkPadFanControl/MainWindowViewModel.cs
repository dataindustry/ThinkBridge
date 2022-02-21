using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
    }

}
