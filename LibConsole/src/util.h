#pragma once
#include <cstdint>
#include <Windows.h>

namespace util
{
    uintptr_t PatternScan(HMODULE module, uintptr_t base, LPCSTR pattern);
}
