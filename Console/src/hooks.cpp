#include "hooks.h"

#include "glob.h"
#include "init.h"
#include "HookManager.hpp"

void WINAPI OutputDebugStringAHook(LPCSTR lpOutputString)
{
    HookManager::CallOriginal(OutputDebugStringAHook, lpOutputString);
    
    if (strcmp(lpOutputString, "\n") == 0) return; // filter out empty lines
    glob::PipeServer->sendMessage(lpOutputString, PipeMessageType::Log);
}

void hooks::InstallHooks()
{
    auto proc = GetProcAddress(GetModuleHandleA("kernel32.dll"), "OutputDebugStringA");
    HookManager::InstallHook(reinterpret_cast<PVOID&>(proc), OutputDebugStringAHook);
    glob::PipeServer->sendMessage("Hooks installed", PipeMessageType::Message);
}
