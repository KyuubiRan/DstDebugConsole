#include "init.h"
#include <cstdint>
#include <Windows.h>
#include "util.h"
#include <cstdio>
#include "glob.h"
#include "hooks.h"

namespace app
{
    // void (*WriteConfig)(__int64 a1, int a2, int a3, const char* a4, ...);
}

// void init_offsets()
// {
//     auto module = GetModuleHandleA(nullptr);
//     if (module == INVALID_HANDLE_VALUE)
//         return;
//
//     app::WriteConfig = (decltype(app::WriteConfig))util::PatternScan(
//         module, 0x0, "4C 89 4C 24 20 53 57 48 83 EC 78 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 50");
// }

void app::Init()
{
    glob::PipeServer->create();
    glob::PipeServer->sendMessage("Hello, world from LibConsole!");

    // init_offsets();

    hooks::InstallHooks();
}
