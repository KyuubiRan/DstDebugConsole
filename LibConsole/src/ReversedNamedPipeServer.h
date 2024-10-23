#pragma once

#include <cstdint>
#include <Windows.h>
#include <namedpipeapi.h>

enum class PipeMessageType : uint8_t
{
    None = 0,
    Attach,
    Detach,

    Log,
    Message
};

struct PipeMessage
{
    PipeMessageType type;
    uint32_t pid;
    uint32_t size;
    uint8_t* data;
};

class ReversedNamedPipeServer
{
    HANDLE mPipe = INVALID_HANDLE_VALUE;
    uint32_t pid = 0;

public:
    explicit ReversedNamedPipeServer(const uint32_t pid) : pid(pid)
    {
    }

    bool create();
    bool stop();

    bool sendMessage(const char* message, PipeMessageType type = PipeMessageType::Message) const;

private:
    bool write(const PipeMessage &message) const;
    bool read(PipeMessage &message);
};
