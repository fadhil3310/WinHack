#include <Windows.h>
#include <iostream>
#include <string>

#define FULL_REQ_BUFFER_SIZE sizeof(char) + sizeof(int) + sizeof(int)
#define FULL_ANSWER_BUFFER_SIZE 5120

#define REQ_CREATE_HOOK 0
#define REQ_REMOVE_HOOK 1


std::wstring HOOK_PIPE_NAME; // The hook pipe name.
std::wstring SURROGATE_PIPE_NAME; // The surrogate's pipe server name.
std::wstring DLL_PATH; // The WindowHook.LowLevel 32-bit dll path.

HANDLE pipeServer; // The pipe server's handle.

// DLL Procedures.
typedef bool (*DLL_Initialize)(const wchar_t*);
DLL_Initialize dll_Initialize;
typedef int (*DLL_CreateHook)(int, DWORD);
DLL_CreateHook dll_CreateHook;
typedef bool (*DLL_RemoveHook)(int);
DLL_RemoveHook dll_RemoveHook;


bool answer_remove_hook(int hookId) 
{
  std::cout << "Answer remove hook\n";

  bool success = dll_RemoveHook(hookId);

  // Unused.
  DWORD cbWritten;

  std::cout << "Sending remove hook status...\n";
  if (!WriteFile(
    pipeServer,
    &success,
    sizeof(bool),
    &cbWritten,
    NULL))
  {
    std::cout << "Failed sending remove hook status to client.\n";
    return false;
  }

  return true;
}

bool answer_create_hook(int hookType, DWORD threadId)
{
  std::cout << "Answer create hook\n";

  int hookId = dll_CreateHook(hookType, threadId);
  
  // Unused.
  DWORD cbWritten;

  std::cout << "Sending hook id...\n";
  if (!WriteFile(
    pipeServer,
    &hookId,
    sizeof(int),
    &cbWritten,
    NULL))
  {
    std::cout << "Failed sending hook id to client.\n";
    return false;
  }

  return true;
}

bool process_messages()
{
  std::cout << "Start processing messages...\n";

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

    std::cout << "Got message, type: " << requestType << ", message1: " << requestMessage1 << ", message2: " << requestMessage2 << "\n";

    switch (requestType)
    {
    case REQ_CREATE_HOOK:
      answer_create_hook(requestMessage1, requestMessage2);
      break;
    case REQ_REMOVE_HOOK:
      answer_remove_hook(requestMessage1);
      break;
    }
  }
}

bool wait_client_connected()
{
  std::cout << "Waiting for client to connect...\n";

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
  std::cout << "Creating pipe server...\n";

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
  std::cout << "Loading library...\n";

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

  dll_CreateHook = (DLL_CreateHook)GetProcAddress(library, "CreateHook");
  if (dll_CreateHook == NULL)
  {
    std::cout << "Failed getting 'CreateHook' proc address.\n";
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