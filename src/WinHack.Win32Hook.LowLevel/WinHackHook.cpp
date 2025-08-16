#include "pch.h"
#include "WinHackHook.h"

#include <unordered_map>
#include <mutex>
#include <fstream>
#include <string>

#pragma data_seg("SHARED")
bool IS_INITIALIZED = false;
wchar_t MAIN_PIPE_NAME[247] = L"";
#pragma data_seg()
#pragma comment(linker, "/section:SHARED,RWS")



// ============================ Static Variables ============================

static HANDLE hPipe; // Pipe client instance

// ============================ End Static Variables ============================


/// <summary>
/// Create a pipe client instance if it hasn't been created for the current thread.
/// The server is created by the WinHack.WindowHook managed code
/// </summary>
/// <param name="file"></param>
/// <param name="pipeTypeName"></param>
/// <returns></returns>
bool setup_pipe(std::wofstream& file, DWORD hookId)
{
  if (hPipe == NULL)
  {
    file << "Pipe is null, creating...\n";
    file << "Is Intialized: " << IS_INITIALIZED << ", Main pipe name: " << MAIN_PIPE_NAME << "\n";

    // The name of the pipe (Global\[MAIN_PIPE_NAME][Pipe Type: CallWnd/CBT/etc]\[Thread Id]).
    wchar_t pipeName[255];
    //wcscat_s(pipeName, MAIN_PIPE_NAME);
    //wcscat_s(pipeName, hookId);
    //wcscat_s(pipeName, L"\\");

    DWORD threadId = GetCurrentThreadId();
    //swprintf_s(threadIDChar, cbThreadIDChar, L"%d", threadID);
    //wcscat_s(pipeName, threadIDChar);

    swprintf_s(pipeName, _countof(pipeName), L"\\\\.\\pipe\\%ls%u\\%d", MAIN_PIPE_NAME, hookId, threadId);

    file << "Pipe name: " << pipeName << "\n";

    // Create the pipe client instance.
    hPipe = CreateFileW(
      pipeName,
      GENERIC_WRITE |
      GENERIC_READ,
      0,
      NULL,
      OPEN_EXISTING,
      0,
      NULL);

    if (hPipe != INVALID_HANDLE_VALUE)
    {
      file << "Success connecting to pipe server\n";
      // Connect to the pipe server.
     /* if (!WaitNamedPipe(pipeName, 5000))
      {
        file << "Connecting pipe timed out: " << GetLastError() << "\n";
        hPipe = NULL;
        return false;
      }*/

      // Set the type of the pipe.
      DWORD dwMode = PIPE_READMODE_BYTE;
      if (!SetNamedPipeHandleState(
        hPipe,
        &dwMode,
        NULL,
        NULL
      ))
      {
        file << "Failed setting pipe handle state\n";
        hPipe = NULL;
        return false;
      }

      file << "Success setting pipe client state\n";
    }
    else
    {
      file << "Failed creating pipe, INVALID_HANDLE_VALUE: " << GetLastError() << "\n";
      //if (!WaitNamedPipe(pipeName, 5000))
      //{
      //  file << "Connecting pipe timed out: " << GetLastError() << "\n";
      //  hPipe = NULL;
      //  return false;
      //}
      hPipe = NULL;
      return false;
    }
  }

  file << "Pipe not null\n";
  return true;
}

/// <summary>
/// Call the creator of the hook everytime the hook received a message,
/// and then wait for a response to be returned to the hook
/// </summary>
/// <param name="file"></param>
/// <param name="lParam"></param>
/// <param name="cblParam"></param>
/// <returns></returns>
int call_hook_callback(std::wofstream& file, int nCode, LPARAM lParam, size_t cblParam)
{
  file << "cblParam: " << cblParam << "\n";

  // Write a message to be sent to the hook creator.
  size_t cbMessage = sizeof(int32_t) + sizeof(uint32_t) + cblParam;
  char* message = (char*)malloc(cbMessage);
  // Check if failed to allocate memory for message.
  if (message)
  {
    memset(message, 0, cbMessage);
    // The nCode.
    //memset(message, nCode, sizeof(int32_t));
    *(message) = nCode;
    // The size of the lParam.
    *(message + sizeof(int32_t)) = cblParam;
    //memset(message + sizeof(int32_t), cblParam, sizeof(uint32_t));
    // Serialize the lParam.
    memcpy(message + sizeof(int32_t) + sizeof(uint32_t), (void*)lParam, cblParam);
  }
  else
  {
    return -1;
  }
 
  file << "Attempt to write message: " << cbMessage << "\n";

  // Send the message.
  DWORD cbWritten;
  if (!WriteFile(
    hPipe,
    message,
    cbMessage,
    &cbWritten,
    NULL
  ))
  {
    file << "Failed writing message: " << GetLastError() << "\n";
    free(message);
    return -1;
  }
  file << "Finish writing message\n";
  free(message);

  // Wait for a response,
  // the response should be the code to be returned to the hook.
  char messageRead;
  size_t cbMessageRead = sizeof(char);
  DWORD cbBytesRead;
  if (!ReadFile(
    hPipe,
    &messageRead,
    cbMessageRead,
    &cbBytesRead,
    NULL
  ))
  {
    file << "Error reading message: " << GetLastError() << ", " << cbBytesRead << "\n";
    return -1;
  }

  file << "Message read: " << messageRead << "\n";
  return messageRead;
}

// ============================ Hook Procedures ============================


/// <summary>
/// Hook procedure for CallWnd hook
/// </summary>
/// <param name="nCode"></param>
/// <param name="wParam"></param>
/// <param name="lParam"></param>
/// <returns></returns>
LRESULT CALLBACK callwnd_hook_proc(int nCode, WPARAM wParam, LPARAM lParam)
{
  std::wofstream file("C:\\Users\\Fadhil\\Desktop\\test.txt", std::ios_base::app);
  file << "Called! " << nCode << "\n";
  file << "Try setup pipe\n";

  if (setup_pipe(file, WH_CALLWNDPROC))
  {
    int result = call_hook_callback(file, nCode, lParam, sizeof(CWPSTRUCT));
  }
  else
  {
    file << "Failed creating pipe client\n";
  }

  file.close();
  return CallNextHookEx(NULL, nCode, wParam, lParam);
}

// ============================ End Hook Procedures ============================


// ============================ Public Functions ============================

bool Initialize(const wchar_t* mainPipeName)
{
  if (IS_INITIALIZED)
    return false;

  std::wofstream file("C:\\Users\\Fadhil\\Desktop\\test.txt", std::ios_base::app);
  file << "Initialize: " << mainPipeName << "\n";
  file.close();

  wcscpy_s(MAIN_PIPE_NAME, mainPipeName);

  IS_INITIALIZED = true;
  return true;
}

HHOOK CreateLocalHook(int hookId, DWORD threadId)
{
  std::wofstream file("C:\\Users\\Fadhil\\Desktop\\test.txt", std::ios_base::app);

  // Return early if hasn't been initialized.
  if (!IS_INITIALIZED)
  {
    file << "CreateHook! Hasn't been initialized!\n";
    file.close();
    return {};
  }
  file << "CreateHook!" << " Hook Id: " << hookId << ", Thread Id: " << threadId << ", Main pipe name: " << MAIN_PIPE_NAME << "\n";

  // Get the appropriate hook procedure.
  HOOKPROC hookProc = nullptr;
  switch (hookId)
  {
  case WH_CALLWNDPROC:
    hookProc = reinterpret_cast<HOOKPROC>(callwnd_hook_proc);
    break;
  // Return an empty struct if there's no procedure created 
  // for the requested hook type yet.
  default:
    return {};
  }

  // Create the hook.
  HHOOK hHook = SetWindowsHookEx(WH_CALLWNDPROC, reinterpret_cast<HOOKPROC>(hookProc), CURRENT_HMODULE, threadId);
  file << "Finish creating callwnd hook\n";

  // If failed creating hook, return an empty struct,
  // if not, return the created hook's handle.
  if (!hHook)
  {
    file << "Error: " << GetLastError() << "\n";
    file.close();
    return {};
  }

  file.close();
  return hHook;
}

HHOOK CreateGlobalHook(int idHook, HOOKPROC callback)
{
  return {};
}

bool RemoveHook(HHOOK& hHook) {
  if (!UnhookWindowsHookEx(hHook))
    return false;
  return true;
}

// ============================ End Public Functions ============================

