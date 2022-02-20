#include "_prec.h"
#include "lib/TVicPort.h"
#include "lib/nvapi/nvapi.h"


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
	char FanCtrl,
		FanSpeedLo1,
		FanSpeedHi1,
		FanSpeedLo2,
		FanSpeedHi2;
} State;


//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
DWORD deax, debx, decx, dedx;
__declspec(dllexport) int ReadCpuName(char* cpuName)
{
	const DWORD id = 0x80000002;

	memset(cpuName, 0, sizeof(cpuName));

	for (DWORD t = 0; t < 3; t++) {

		const DWORD veax = id + t;

		__asm
		{
			mov eax, veax
			cpuid
			mov deax, eax
			mov debx, ebx
			mov decx, ecx
			mov dedx, edx
		}

		memcpy(cpuName + 16 * t + 0, &deax, 4);
		memcpy(cpuName + 16 * t + 4, &debx, 4);
		memcpy(cpuName + 16 * t + 8, &decx, 4);
		memcpy(cpuName + 16 * t + 12, &dedx, 4);
	}

	return 0;

}

//-------------------------------------------------------------------------
// read a byte from the embedded controller (EC) via port io
//-------------------------------------------------------------------------
int ReadByteFromEC(int offset, char* pdata)
{
	char data = -1;
	char data2 = -1;
	int iOK = false;
	int iTimeout = 100;
	int iTimeoutBuf = 1000;
	int	iTime = 0;
	int iTick = 10;

	// wait for full buffers to clear or timeout iTimeoutBuf = 1000
	for (iTime = 0; iTime < iTimeoutBuf; iTime += iTick) {
		data = (char)ReadPort(EC_CTRLPORT) & 0xff;
		if (!(data & (EC_STAT_IBF | EC_STAT_OBF))) break;
		::Sleep(iTick);
	}

	// clear OBF(output buff) if full
	if (data & EC_STAT_OBF) data2 = (char)ReadPort(EC_DATAPORT);

	// tell 'em we want to "READ"
	WritePort(EC_CTRLPORT, EC_CTRLPORT_READ);

	for (iTime = 0; iTime < iTimeout; iTime += iTick) {

		// wait for IBF and OBF to clear
		data = (char)ReadPort(EC_CTRLPORT) & 0xff;
		if (!(data & (EC_STAT_IBF | EC_STAT_OBF))) {
			iOK = true;
			break;
		}

		// try again after a moment
		::Sleep(iTick);
	}

	if (!iOK) return 0;
	iOK = false;

	// tell 'em where we want to read from
	WritePort(EC_DATAPORT, offset);

	if (!(data & EC_STAT_OBF)) {
		for (iTime = 0; iTime < iTimeout; iTime += iTick) {

			// wait for OBF to clear
			data = (char)ReadPort(EC_CTRLPORT) & 0xff;
			if ((data & EC_STAT_OBF)) {
				iOK = true;
				break;
			}

			// try again after a moment
			::Sleep(iTick);
		}
		if (!iOK) return 0;
	}

	*pdata = ReadPort(EC_DATAPORT);

	return 1;
}


//-------------------------------------------------------------------------
// write a byte to the embedded controller (EC) via port io
//-------------------------------------------------------------------------
int WriteByteToEC(int offset, char NewData)
{
	char data = -1;
	char data2 = -1;
	int iOK = false;
	int iTimeout = 100;
	int iTimeoutBuf = 1000;
	int	iTime = 0;
	int iTick = 10;

	for (iTime = 0; iTime < iTimeoutBuf; iTime += iTick) {	// wait for full buffers to clear
		data = (char)ReadPort(EC_CTRLPORT) & 0xff;			// or timeout iTimeoutBuf = 1000
		if (!(data & (EC_STAT_IBF | EC_STAT_OBF))) break;
		::Sleep(iTick);
	}

	if (data & EC_STAT_OBF) data2 = (char)ReadPort(EC_DATAPORT); //clear OBF if full

	for (iTime = 0; iTime < iTimeout; iTime += iTick) { // wait for IOBF to clear
		data = (char)ReadPort(EC_CTRLPORT) & 0xff;
		if (!(data & EC_STAT_OBF)) {
			iOK = true;
			break;
		}
		::Sleep(iTick);
	}  // try again after a moment

	if (!iOK) return 0;
	iOK = false;

	WritePort(EC_CTRLPORT, EC_CTRLPORT_WRITE);		// tell 'em we want to "WRITE"

	for (iTime = 0; iTime < iTimeout; iTime += iTick) { // wait for IBF and OBF to clear
		data = (char)ReadPort(EC_CTRLPORT) & 0xff;
		if (!(data & (EC_STAT_IBF | EC_STAT_OBF))) {
			iOK = true;
			break;
		}
		::Sleep(iTick);
	}							// try again after a moment

	if (!iOK) return 0;
	iOK = false;

	WritePort(EC_DATAPORT, offset);					// tell 'em where we want to write to

	for (iTime = 0; iTime < iTimeout; iTime += iTick) { // wait for IBF and OBF to clear
		data = (char)ReadPort(EC_CTRLPORT) & 0xff;
		if (!(data & (EC_STAT_IBF | EC_STAT_OBF))) {
			iOK = true;
			break;
		}
		::Sleep(iTick);
	}							// try again after a moment

	if (!iOK) return 0;
	iOK = false;

	WritePort(EC_DATAPORT, NewData);				// tell 'em what we want to write there

	return 1;
}


//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
int SetFanStateLevel(int fanctrl1, int fanctrl2)
{
	int ok = 0;
	int fan1_ok = 0;
	int fan2_ok = 0;

	char fanstate1 = 0;
	char fanstate2 = 0;

	for (int i = 0; i < 5; i++) {

		ok = WriteByteToEC(TP_ECOFFSET_FAN_SWITCH, TP_ECOFFSET_FAN1);
		ok = WriteByteToEC(TP_ECOFFSET_FAN, fanctrl1);

		::Sleep(100);

		ok = WriteByteToEC(TP_ECOFFSET_FAN_SWITCH, TP_ECOFFSET_FAN2);
		ok = WriteByteToEC(TP_ECOFFSET_FAN, fanctrl2);

		::Sleep(100);

		// verify completion of fan2
		// fan2_ok = ReadByteFromEC(TP_ECOFFSET_FAN, &fanstate2);

		// ::Sleep(100);

		// verify completion of fan1
		// ok = WriteByteToEC(TP_ECOFFSET_FAN_SWITCH, TP_ECOFFSET_FAN1);
		// fan1_ok = ReadByteFromEC(TP_ECOFFSET_FAN, &fanstate2);

		// ::Sleep(300);
	}

	return 0;
}


//-------------------------------------------------------------------------
//  
//-------------------------------------------------------------------------
int ReadGPUTemperture(int* gpuTemperture)
{
	NvAPI_Status nvapi_ok = NVAPI_ERROR;

	NvU32 count;
	NvPhysicalGpuHandle handle;
	NV_GPU_THERMAL_SETTINGS thermal;

	nvapi_ok = NvAPI_EnumPhysicalGPUs(&handle, &count);

	thermal.version = NV_GPU_THERMAL_SETTINGS_VER;
	nvapi_ok = NvAPI_GPU_GetThermalSettings(handle, 0, &thermal);

	if (!nvapi_ok == NVAPI_OK) {
		NvAPI_ShortString string;
		NvAPI_GetErrorMessage(nvapi_ok, string);
		printf("NVAPI NvAPI_GPU_GetThermalSettings: %s\n", string);
		return 1;
	}
 
	*gpuTemperture = static_cast<unsigned>(thermal.sensor[0].currentTemp);

	return 0;
}


//-------------------------------------------------------------------------
//  
//-------------------------------------------------------------------------
int ReadEcRaw(FCSTATE* pfcstate) {

	int ok;
	pfcstate->FanCtrl = -1;

	ok = ReadByteFromEC(TP_ECOFFSET_FAN, &pfcstate->FanCtrl);

	ok = WriteByteToEC(TP_ECOFFSET_FAN_SWITCH, TP_ECOFFSET_FAN2);

	if (ok)
		ok = ReadByteFromEC(TP_ECOFFSET_FANSPEED, &pfcstate->FanSpeedLo2);
	if (!ok) {
		printf("failed to read FanSpeedLowByte 2 from EC");
	}

	if (ok)
		ok = ReadByteFromEC(TP_ECOFFSET_FANSPEED + 1, &pfcstate->FanSpeedHi2);
	if (!ok) {
		printf("failed to read FanSpeedHighByte 2 from EC");
	}

	ok = WriteByteToEC(TP_ECOFFSET_FAN_SWITCH, TP_ECOFFSET_FAN1);

	if (ok)
		ok = ReadByteFromEC(TP_ECOFFSET_FANSPEED, &pfcstate->FanSpeedLo1);
	if (!ok) {
		printf("failed to read FanSpeedLowByte 1 from EC");
	}

	if (ok)
		ok = ReadByteFromEC(TP_ECOFFSET_FANSPEED + 1, &pfcstate->FanSpeedHi1);
	if (!ok) {
		printf("failed to read FanSpeedHighByte 1 from EC");
	}

	return 0;

}


//-------------------------------------------------------------------------
//  
//-------------------------------------------------------------------------
int ReadEcStatus(FCSTATE* pfcstate) {

	char ok = 0;

	for (int i = 0; i < 3; i++) {
		ok = ReadEcRaw(pfcstate);
		if (ok)
			break;
		::Sleep(200);
	}

	return ok;
}


int main(void){

	bool ok = false;
	NvAPI_Status nvapi_ok = NVAPI_ERROR;
	char FanCtrl = 0;

	for (int i = 0; i < 180; i++)
	{
		ok = OpenTVicPort();
		if (ok) break;

	::Sleep(1000);
	}

	for (int i = 0; i < 180; i++)
	{
		nvapi_ok = NvAPI_Initialize();

		if (nvapi_ok != NVAPI_OK) {
			NvAPI_ShortString error;
			NvAPI_GetErrorMessage(nvapi_ok, error);
			printf("NVAPI NvAPI_Initialize: %s \n", error);
		}
		else break;

		::Sleep(1000);
	}

	SetHardAccess(true);

	if (TestHardAccess())
	{

		// Get CPU Name
		char cpuName[49];
		ReadCpuName(cpuName);
		printf("%s \n", cpuName);

		// Get GPU Name
		NvU32 count;
		NvPhysicalGpuHandle handle;
		NvAPI_ShortString gpuName;
		nvapi_ok = NvAPI_EnumPhysicalGPUs(&handle, &count);
		nvapi_ok = NvAPI_GPU_GetFullName(handle, gpuName);

		printf("%s \n", gpuName);

		// manual: 0x00 - 0x07, bios auto: 0x80
		SetFanStateLevel(0x80, 0x80);

		FCSTATE state;

		while (true) {

			char cpuTemperture;
			int gpuTemperture;

			ok = ReadByteFromEC(TP_ECOFFSET_TEMP0, &cpuTemperture);
			ok = ReadGPUTemperture(&gpuTemperture);
			ok = ReadEcStatus(&state);

			// combine lo/hi byte
			short fanspeed1 = (short)(((state.FanSpeedHi1) & 0xFF) << 8 | (state.FanSpeedLo1) & 0xFF);
			short fanspeed2 = (short)(((state.FanSpeedHi2) & 0xFF) << 8 | (state.FanSpeedLo2) & 0xFF);
			printf("%d / %d / %d / %d \n", cpuTemperture, gpuTemperture, fanspeed1, fanspeed2);

			::Sleep(200);

		}

	}

	CloseTVicPort();

	return 0;
}
