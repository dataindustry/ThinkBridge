#include <string>
using namespace std;
#include <comutil.h>


DWORD deax, debx, decx, dedx;


// Registers of the embedded controller
#define EC_DATAPORT	0x1600	// EC data io-port 0x62
#define EC_CTRLPORT	0x1604	// EC control io-port 0x66


// Embedded controller status register bits
#define EC_STAT_OBF	 0x01 // Output buffer full 
#define EC_STAT_IBF	 0x02 // Input buffer full 
#define EC_STAT_CMD	 0x08 // Last write was a command write (0=data) 


// Embedded controller commands
// (write to EC_CTRLPORT to initiate read/write operation)
#define EC_CTRLPORT_READ	 (char)0x80	
#define EC_CTRLPORT_WRITE	 (char)0x81
#define EC_CTRLPORT_QUERY	 (char)0x84


#define TP_ECOFFSET_FAN		 (char)0x2F	// 1 byte (binary xyzz zzz)
#define TP_ECOFFSET_FANSPEED (char)0x84 // 16 bit word, lo/hi byte
#define TP_ECOFFSET_TEMP0    (char)0x78	// 8 temp sensor bytes from here
#define TP_ECOFFSET_TEMP1    (char)0xC0 // 4 temp sensor bytes from here
#define TP_ECOFFSET_FAN1	 (char)0x0000
#define TP_ECOFFSET_FAN2	 (char)0x0001
#define TP_ECOFFSET_FAN_SWITCH (char)0x31


struct FCSTATE {
	char Fan1StateLevel,
		Fan2StateLevel,
		FanSpeedLo1,
		FanSpeedHi1,
		FanSpeedLo2,
		FanSpeedHi2;
	short Fan1Speed,
		Fan2Speed;
} State;

extern "C" __declspec(dllexport) int StartDevice();

extern "C" __declspec(dllexport) int CloseDevice();

extern "C" __declspec(dllexport) BSTR ReadCpuName();

extern "C" __declspec(dllexport) BSTR ReadGpuName();

extern "C" __declspec(dllexport) int ReadCpuTemperture();

extern "C" __declspec(dllexport) int ReadGpuTemperture();

extern "C" __declspec(dllexport) int ReadFan1Speed();

extern "C" __declspec(dllexport) int ReadFan2Speed();

extern "C" __declspec(dllexport) int SetFan1State(int fanState);

extern "C" __declspec(dllexport) int SetFan2State(int fanState);