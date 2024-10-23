#include "util.h"

#include <vector>

uintptr_t util::PatternScan(HMODULE module, uintptr_t base, LPCSTR pattern)
{
    {
        static auto pattern_to_byte = [](const char* pattern)
        {
            auto bytes = std::vector<int>{};
            auto start = const_cast<char*>(pattern);
            auto end = const_cast<char*>(pattern) + strlen(pattern);
            for (auto current = start; current < end; ++current)
            {
                if (*current == '?')
                {
                    ++current;
                    if (*current == '?')
                        ++current;
                    bytes.push_back(-1);
                }
                else
                {
                    bytes.push_back(strtoul(current, &current, 16));
                }
            }
            return bytes;
        };

        if (base < (uintptr_t)module)
        {
            base = 0ul;
        }

        auto dosHeader = (PIMAGE_DOS_HEADER)module;
        auto ntHeaders = (PIMAGE_NT_HEADERS)((std::uint8_t*)module + dosHeader->e_lfanew);
        auto sizeOfImage = ntHeaders->OptionalHeader.SizeOfImage;
        auto patternBytes = pattern_to_byte(pattern);
        auto s = patternBytes.size();
        auto d = patternBytes.data();
        auto scanBytes = reinterpret_cast<std::uint8_t*>(base ? (base + s) : (uintptr_t)module);
        auto offset = base ? base - (uintptr_t)module : 0ul;

        for (auto i = 0ul; i < sizeOfImage - offset - s; ++i)
        {
            bool found = true;
            for (auto j = 0ul; j < s; ++j)
            {
                if (scanBytes[i + j] != d[j] && d[j] != -1)
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                return (uintptr_t)&scanBytes[i];
            }
        }
        return 0;
    }
}
