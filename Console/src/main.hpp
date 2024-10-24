#pragma once

#include <cstdio>
#include <Windows.h>

#include "glob.h"
#include "HookManager.hpp"
#include "init.h"

inline void Run(HMODULE* hModule)
{
    Sleep(3000);
    // FreeConsole();
    // AllocConsole();
    // SetConsoleTitle(L"LibConsole.dll");
    // // SetConsoleOutputCP(65001);
    // // reset console output
    // freopen_s(reinterpret_cast<FILE**>(stdout), "CONOUT$", "w", stdout);
    // freopen_s(reinterpret_cast<FILE**>(stdin), "CONIN$", "r", stdin);
    // freopen_s(reinterpret_cast<FILE**>(stderr), "CONOUT$", "w", stderr);
    
    app::Init();
}

inline void Stop()
{
    // HookManager::UninstallAll();
    glob::PipeServer->stop();
}