// EnableSurround.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "nvapi\include\nvapi.h"


int main()
{
	NvAPI_Status ret = NVAPI_OK;

	ret = NvAPI_Initialize();
	if (ret != NVAPI_OK)
	{
		printf("NvAPI_Initialize() failed = 0x%x\r\n", ret);
		return 1; // Initialization failed
	}

	ret = NvAPI_EnableCurrentMosaicTopology(NV_TRUE);
	if (ret != NVAPI_OK) {
		NvAPI_ShortString desc;
		NvAPI_GetErrorMessage(ret, desc);
		printf("Enable failed: %s\r\n", desc);
		return 1;
	}

	NvAPI_Unload();
    return 0;
}

