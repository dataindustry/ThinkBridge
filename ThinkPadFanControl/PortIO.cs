using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ThinkPadFanControl
{
    internal class PortIO
    {
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
