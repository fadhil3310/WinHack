#include "pch.h"
#include "WindowHook.h"

HMODULE CURRENT_HMODULE;


WindowHook::WindowHook(int idHook, DWORD threadId)
{
		std::ofstream file("C:\\Users\\Fadhil\\Desktop\\test.txt", std::ios_base::app);
		file << "Start! " << windowThreadProcessId << "\n";

		managedCallback = callback;

		hHook = SetWindowsHookExA(idHook, HookProcedure, CURRENT_HMODULE, windowThreadProcessId);
		file << "Error: " << GetLastError() << "\n";
		file.close();
}
