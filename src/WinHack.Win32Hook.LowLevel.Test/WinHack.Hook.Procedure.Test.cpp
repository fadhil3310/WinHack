// WinHack.Hook.Procedure.Test.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <string>

#include "Windows.h"
#include "WinHackHook.h"

#define LIB_PATH_64 TEXT("..\\WinHack.WindowHook.LowLevel\\WinHack.WindowHook.LowLevel64.dll");
#define LIB_PATH_32 TEXT("..\\WinHack.WindowHook.LowLevel\\WinHack.WindowHook.LowLevel32.dll");

//typedef HookData(*CreateCallWNDHookProc)(HWND hWnd, HOOKPROC callback);
//typedef bool(*RemoveCallWNDHookProc)(HookData& instance);
//typedef HookData(*CreateHookProc)(int hookID, HWND hWnd, HOOKPROC callback);
//typedef bool(*RemoveHookProc)(HookData& instance);
//
//
//void load_library(
//  BOOL is64bit, 
//  CreateHookProc& createHookProc,
//  RemoveHookProc& removeHookProc)
//{
//  LPCWSTR libraryPath;
//  if (is64bit)
//  {
//    libraryPath = LIB_PATH_64;
//  }
//  else
//  {
//    libraryPath = LIB_PATH_32;
//  }
//
//  HMODULE library = LoadLibrary(libraryPath);
//  if (library == NULL)
//    throw new std::exception("Failed loading library");
//
//
//  createHookProc = (CreateHookProc)GetProcAddress(library, "CreateHook");
//  if (createHookProc == NULL)
//    throw new std::exception("Failed getting create hook procedure");
//
//  removeHookProc = (RemoveHookProc)GetProcAddress(library, "RemoveHook");
//  if (removeHookProc == NULL)
//    throw new std::exception("Failed getting remove hook procedure");
//}
//
//
//LRESULT CALLBACK hook_callback(int nCode, WPARAM wParam, LPARAM lParam)
//{
//  std::cout << "nCode: " << nCode << "\n";
//  return 0;
//}
//
//int main()
//{
//    std::wstring windowName;
//    std::wcout << "Window name: ";
//    std::getline(std::wcin, windowName);
//
//    std::wstring className;
//    std::wcout << "Class name: ";
//    std::getline(std::wcin, className);
//
//    HWND window = FindWindow(className.c_str(), windowName.c_str());
//    if (window == NULL)
//      throw new std::exception("Failed finding window");
//
//
//    // Get thread ID of the target window
//    DWORD processId;
//    DWORD threadId = GetWindowThreadProcessId(window, &processId);
//    HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION, FALSE, processId);
//
//    BOOL is64bit = FALSE;
//    if (!IsWow64Process(hProcess, &is64bit))
//      std::cout << "Failed getting process is 64 bit: " << GetLastError() << "\n";
//    else
//      is64bit = !is64bit;
//    std::cout << "Is 64 bit process? " << is64bit << "\n";
//
//
//    CreateHookProc CreateHook;
//    RemoveHookProc RemoveHook;
//    load_library(is64bit, CreateHook, RemoveHook);
//
//
//    HookData hookData = CreateHook(WH_CALLWNDPROC, window, hook_callback);
//    std::cout << hookData.hHook;
//
//
//    while (true)
//    {
//      std::string value;
//      std::cout << "Stop? (yes)\n";
//      std::cin >> value;
//      if (value == "yes")
//        break;
//    }
//
//
//    if (!RemoveHook(hookData))
//      throw new std::exception("Failed removing hook");
//}

int main()
{
  wchar_t MAIN_PIPE_NAME[247] = L"GlobalHook";
  wchar_t pipeTypeName[50] = L"TypeName";

  wchar_t pipeName[255] = L"Global\\";
  wcscat_s(pipeName, MAIN_PIPE_NAME);
  wcscat_s(pipeName, L"\\");
  wcscat_s(pipeName, pipeTypeName);
  wcscat_s(pipeName, L"\\");
  
  std::wcout << pipeName << "\n";


  DWORD threadID = GetCurrentThreadId();
  const size_t cbThreadIDChar = sizeof(DWORD) + 1;
  wchar_t threadIDChar[cbThreadIDChar];
  swprintf(threadIDChar, cbThreadIDChar, L"%d", threadID);
  wcscat_s(pipeName, threadIDChar);

  std::wcout << pipeName << "\n";

}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
