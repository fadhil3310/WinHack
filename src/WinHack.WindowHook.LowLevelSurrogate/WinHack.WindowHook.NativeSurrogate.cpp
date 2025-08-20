#include <Windows.h>
#include <iostream>
#include <string>

#define FULL_REQ_BUFFER_SIZE sizeof(char) + sizeof(int) + sizeof(int)
#define FULL_ANSWER_BUFFER_SIZE 5120

#define REQ_CREATE_LOCAL_HOOK 0
#define REQ_CREATE_GLOBAL_HOOK 1
#define REQ_REMOVE_HOOK 2


std::wstring HOOK_PIPE_NAME; // The hook pipe name.
std::wstring SURROGATE_PIPE_NAME; // The surrogate's pipe server name.
std::wstring DLL_PATH; // The WindowHook.LowLevel 32-bit dll path.

HANDLE pipeServer; // The pipe server's handle.

// DLL Procedures.
typedef bool (*DLL_Initialize)(const wchar_t*);
DLL_Initialize dll_Initialize;
typedef HHOOK (*DLL_CreateLocalHook)(int, DWORD);
DLL_CreateLocalHook dll_CreateLocalHook;
typedef bool (*DLL_RemoveHook)(HHOOK&);
DLL_RemoveHook dll_RemoveHook;



bool answer_create_local_hook(int hookId, DWORD threadId)
{
  HHOOK hHook = dll_CreateLocalHook(hookId, threadId);

  // Unused.
  DWORD cbWritten;

  // Send the HHOOK size, or 0 if failed creating the hook.
  DWORD cbHHook = hHook == NULL ? 0 : sizeof(hHook);
  if (!WriteFile(
    pipeServer,
    &cbHHook,
    sizeof(DWORD),
    &cbWritten,
    NULL))
  {
    std::cout << "Failed sending hHook size to client.\n";
    return false;
  }

  // Send the HHOOK.
  if (hHook != NULL)
  {
    char* hHookBuffer = (char*)malloc(cbHHook);
    memcpy(hHookBuffer, hHook, cbHHook);

    if (!WriteFile(
      pipeServer,
      &hHookBuffer,
      cbHHook,
      &cbWritten,
      NULL))
    {
      std::cout << "Failed sending hHook to client.\n";
      return false;
    }
  }

  return true;
}

bool process_messages()
{
  while (true)
  {
    char requestType;
    int requestMessage1;
    unsigned int requestMessage2;
    DWORD cbBytesRead; // not needed.

    // The type of the request.
    bool typeSuccess = ReadFile(
      pipeServer,
      &requestType,
      sizeof(char),
      &cbBytesRead,
      NULL);
    // The first message of the request.
    bool message1Success = ReadFile(
      pipeServer,
      &requestMessage1,
      sizeof(int),
      &cbBytesRead,
      NULL);
    // The second message of the request.
    bool message2Success = ReadFile(
      pipeServer,
      &requestMessage2,
      sizeof(unsigned int),
      &cbBytesRead,
      NULL);

    if (!typeSuccess || !message1Success || !message2Success)
    {
      std::cout << "Failed reading request.\n";
      return false;
    }

    switch (requestType)
    {
    case REQ_CREATE_LOCAL_HOOK:
      answer_create_local_hook(requestMessage1, requestMessage2);
      break;
    }
  }
}

bool wait_client_connected()
{
  bool isConnected = ConnectNamedPipe(pipeServer, NULL) ?
    true : (GetLastError() == ERROR_PIPE_CONNECTED);

  if (!isConnected)
  {
    std::cout << "Client not connected.\n";
    CloseHandle(pipeServer);
    return false;
  }

  return true;
}

bool create_pipe_server()
{
  wchar_t fullPipeName[255] = L"\\\\.\\pipe\\";
  wcscat_s(fullPipeName, SURROGATE_PIPE_NAME.c_str());

  pipeServer = CreateNamedPipeW(
    fullPipeName,
    PIPE_ACCESS_DUPLEX,
    PIPE_TYPE_MESSAGE |
    PIPE_READMODE_MESSAGE |
    PIPE_WAIT,
    1,
    FULL_ANSWER_BUFFER_SIZE,
    FULL_REQ_BUFFER_SIZE,
    0,
    NULL);

  if (pipeServer == INVALID_HANDLE_VALUE)
  {
    std::cout << "Failed creating pipe server (invalid handle value), error code: " << GetLastError() << ".\n";
    return false;
  }

  return true;
}

bool load_library()
{
  HMODULE library = LoadLibrary(DLL_PATH.c_str());
  if (library == NULL)
  {
    std::cout << "Failed loading library (wrong dll path?), error code: " << GetLastError() << ".\n";
    return false;
  }

  dll_Initialize = (DLL_Initialize)GetProcAddress(library, "Initialize");
  if (dll_Initialize == NULL)
  {
    std::cout << "Failed getting 'Initialize' proc address.\n";
    return false;
  }

  dll_CreateLocalHook = (DLL_CreateLocalHook)GetProcAddress(library, "CreateLocalHook");
  if (dll_CreateLocalHook == NULL)
  {
    std::cout << "Failed getting 'CreateLocalHook' proc address.\n";
    return false;
  }

  dll_RemoveHook = (DLL_RemoveHook)GetProcAddress(library, "RemoveHook");
  if (dll_RemoveHook == NULL)
  {
    std::cout << "Failed getting 'RemoveHook' proc address.\n";
    return false;
  }

  // Initialize the hook dll.
  if (!dll_Initialize(HOOK_PIPE_NAME.c_str()))
    return false;

  return true;
}

int wmain(int argc, wchar_t *argv[])
{
  if (argc <= 3)
    return 1;

  HOOK_PIPE_NAME = argv[1];
  SURROGATE_PIPE_NAME = argv[2];
  DLL_PATH = argv[3];

  // Check if PIPE_NAME or DLL_PATH is empty.
  if (HOOK_PIPE_NAME.empty() || SURROGATE_PIPE_NAME.empty() || DLL_PATH.empty())
    return 1;

  // Load dll.
  if (!load_library())
    return 2;

  // Create pipe server.
  if (!create_pipe_server())
    return 3;

  // Wait for client to connect to the pipe server.
  if (!wait_client_connected())
    return 4;

  // Process any incoming messages.
  if (!process_messages())
    return 5;
}