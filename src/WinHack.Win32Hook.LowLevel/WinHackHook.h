#pragma once

#ifdef WINHACKHOOK_API
#define WINHACKHOOK_API __declspec(dllexport)
#else
#define WINHACKHOOK_API __declspec(dllimport)
#endif

#include <Windows.h>


extern HMODULE CURRENT_HMODULE;

/// <summary>
/// Initialize
/// </summary>
/// <param name="mainPipeName">The main name to be used for the pipe connection</param>
/// <returns></returns>
extern "C" WINHACKHOOK_API bool Initialize(const wchar_t* mainPipeName);

/// <summary>
/// Create hook
/// </summary>
/// <param name="threadId"></param>
/// <returns></returns>
extern "C" WINHACKHOOK_API HHOOK CreateLocalHook(int hookId, DWORD threadId);

///// <summary>
///// Create CallWnd hook
///// </summary>
///// <param name="threadId"></param>
///// <returns></returns>
//extern "C" WINHACKHOOK_API HHOOK CreateCallWndHook(DWORD threadId);

///// <summary>
///// Create global hook
///// </summary>
///// <param name="idHook"></param>
///// <param name="callback"></param>
///// <returns></returns>
//extern "C" WINHACKHOOK_API HHOOK CreateGlobalHook(int hookId, HOOKPROC callback);

/// <summary>
/// Remove hook
/// </summary>
/// <param name="instance"></param>
/// <returns></returns>
extern "C" WINHACKHOOK_API bool RemoveHook(HHOOK& hHook);