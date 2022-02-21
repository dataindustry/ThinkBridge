#include "_prec.h"
#include "lib/TVicPort.h"
#include "lib/nvapi/nvapi.h"
#include "ThinkBridge.h"
#include <atlconv.h>


//-------------------------------------------------------------------------
// 
//-------------------------------------------------------------------------
BSTR convertCharToBSTR(char* input) {

	USES_CONVERSION;
	const OLECHAR* pOleChar = A2CW(input);
	BSTR str = SysAllocString(pOleChar);
	SysFreeString(str);

	return str;
}


//-------------------------------------------------------------------------
// built-in. read a byte from the embedded controller (EC) via port io
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
// built-in. write a byte to the embedded controller (EC) via port io
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
// built-in.
//-------------------------------------------------------------------------
int ReadEcRaw(FCSTATE* pfcstate) {

	int ok;

	// fan 1
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
	ok = ReadByteFromEC(TP_ECOFFSET_FAN, &pfcstate->Fan1StateLevel);

	// fan 2
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
	ok = ReadByteFromEC(TP_ECOFFSET_FAN, &pfcstate->Fan2StateLevel);

	return 0;

}


//-------------------------------------------------------------------------
// built-in.
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


//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
int StartDevice()
{
	bool ok = false;
	NvAPI_Status nvapi_ok = NVAPI_ERROR;

	for (int i = 0; i < 180; i++)
	{
		ok = OpenTVicPort();
		if (ok) break;

		::Sleep(1000);
	}

	for (int i = 0; i < 180; i++)
	{
		nvapi_ok = NvAPI_Initialize();
		if (nvapi_ok == NVAPI_OK) break;

		::Sleep(1000);
	}

	SetHardAccess(true);

	return 0;
}


//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
int CloseDevice()
{
	SetHardAccess(false);
	CloseTVicPort();
	
	return 0;
}


//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
BSTR ReadCpuName() {

	char _cpuName[48] = { 0 };

	const DWORD id = 0x80000002;

	memset(_cpuName, 0, sizeof(_cpuName));

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

		memcpy(_cpuName + 16 * t + 0, &deax, 4);
		memcpy(_cpuName + 16 * t + 4, &debx, 4);
		memcpy(_cpuName + 16 * t + 8, &decx, 4);
		memcpy(_cpuName + 16 * t + 12, &dedx, 4);
	}


	return convertCharToBSTR(_cpuName);
}

//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
BSTR ReadGpuName()
{
	NvAPI_Status nvapi_ok = NVAPI_ERROR;

	// Get GPU Name
	NvU32 count;
	NvPhysicalGpuHandle handle;

	NvAPI_ShortString _gpuName;

	std::string a = "";

	nvapi_ok = NvAPI_EnumPhysicalGPUs(&handle, &count);
	if (nvapi_ok != NVAPI_OK) return NULL;

	nvapi_ok = NvAPI_GPU_GetFullName(handle, _gpuName);
	if (nvapi_ok != NVAPI_OK) return NULL;

	return convertCharToBSTR(_gpuName);

}


//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
int SetFanStateLevel(int fan1statelevel, int fan2statelevel)
{
	int ok = 0;

	// int fan1_ok = 0;
	// int fan2_ok = 0;

	// char fanstate1 = 0;
	// char fanstate2 = 0;

	for (int i = 0; i < 5; i++) {

		ok = WriteByteToEC(TP_ECOFFSET_FAN_SWITCH, TP_ECOFFSET_FAN1);
		ok = WriteByteToEC(TP_ECOFFSET_FAN, fan1statelevel);

		::Sleep(100);

		ok = WriteByteToEC(TP_ECOFFSET_FAN_SWITCH, TP_ECOFFSET_FAN2);
		ok = WriteByteToEC(TP_ECOFFSET_FAN, fan2statelevel);

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
int ReadCpuTemperture(int* cpuTemperture)
{
	char value;
	ReadByteFromEC(TP_ECOFFSET_TEMP0, &value);

	*cpuTemperture = (int)value;

	return 0;
}


//-------------------------------------------------------------------------
//  
//-------------------------------------------------------------------------
int ReadGpuTemperture(int* gpuTemperture)
{
	NvAPI_Status nvapi_ok = NVAPI_ERROR;

	NvU32 count;
	NvPhysicalGpuHandle handle;
	NV_GPU_THERMAL_SETTINGS thermal;

	nvapi_ok = NvAPI_EnumPhysicalGPUs(&handle, &count);

	thermal.version = NV_GPU_THERMAL_SETTINGS_VER;
	nvapi_ok = NvAPI_GPU_GetThermalSettings(handle, 0, &thermal);

	if (nvapi_ok != NVAPI_OK) return 1;
 
	*gpuTemperture = static_cast<unsigned>(thermal.sensor[0].currentTemp);

	return 0;
}


//-------------------------------------------------------------------------
//  
//-------------------------------------------------------------------------
int ReadFanState(FCSTATE* state)
{
	int ok = 1;
	ok = ReadEcStatus(state);

	// combine lo/hi byte
	state->Fan1Speed = (short)(((state->FanSpeedHi1) & 0xFF) << 8 | (state->FanSpeedLo1) & 0xFF);
	state->Fan2Speed = (short)(((state->FanSpeedHi2) & 0xFF) << 8 | (state->FanSpeedLo2) & 0xFF);

	return 0;
}


int main(void)
{
	return 0;
}


// interop warpper function
int ReadCpuTemperture()
{
	int cpuTemperture;
	ReadCpuTemperture(&cpuTemperture);
	return cpuTemperture;
}


int ReadGpuTemperture()
{
	int gpuTemperture;
	ReadGpuTemperture(&gpuTemperture);
	return gpuTemperture;
}


int ReadFan1Speed() {
	FCSTATE state;
	ReadFanState(&state);
	return state.Fan1Speed;
}


int ReadFan2Speed() {
	FCSTATE state;
	ReadFanState(&state);
	return state.Fan2Speed;
}


