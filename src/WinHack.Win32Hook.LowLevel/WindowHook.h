#pragma once

#include "Windows.h"


typedef LRESULT(__stdcall* ManagedHookCallback)(int code, WPARAM wParam, LPARAM lParam);

class WindowHook
{
private:
		HHOOK hHook;
		ManagedHookCallback managedCallback;

public:

public:
		WindowHook();

private:
		LRESULT CALLBACK HookProcedure(int nCode, WPARAM wParam, LPARAM lParam);
};

