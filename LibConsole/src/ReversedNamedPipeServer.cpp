#include "ReversedNamedPipeServer.h"

#include <string>
#include <vector>

bool ReversedNamedPipeServer::create()
{
    if (mPipe != INVALID_HANDLE_VALUE)
    {
        CloseHandle(mPipe);
        mPipe = INVALID_HANDLE_VALUE;
    }

    std::string pipeName = R"(\\.\pipe\dst_console_pipe)";
    pipeName.append("_").append(std::to_string(pid));

    printf("Create reversed pipe at: %s\n", pipeName.c_str());
    mPipe = CreateNamedPipeA(
        pipeName.c_str(),
        PIPE_ACCESS_DUPLEX,
        PIPE_TYPE_BYTE | PIPE_READMODE_BYTE | PIPE_WAIT,
        1,
        6144,
        6144,
        0,
        nullptr
    );


    if (mPipe == INVALID_HANDLE_VALUE)
    {
        printf("Failed to create pipe server.\n");
        return false;
    }

    return mPipe != INVALID_HANDLE_VALUE;
}

bool ReversedNamedPipeServer::stop()
{
    if (mPipe == INVALID_HANDLE_VALUE)
        return false;

    CloseHandle(mPipe);
    mPipe = INVALID_HANDLE_VALUE;
    return true;
}

bool ReversedNamedPipeServer::sendMessage(const char* message, PipeMessageType type) const
{
    PipeMessage msg;
    msg.type = type;
    msg.pid = pid;
    msg.size = strlen(message);
    msg.data = (uint8_t*)message;

    return write(msg);
}

bool ReversedNamedPipeServer::write(const PipeMessage& message) const
{
    char buffer[6144]{};
    buffer[0] = static_cast<char>(message.type);
    memcpy(buffer + 1, &message.pid, sizeof(uint32_t));
    memcpy(buffer + 5, &message.size, sizeof(uint32_t));
    memcpy(buffer + 9, message.data, message.size);

    const DWORD size = 9 + message.size;
    return WriteFile(mPipe, buffer, size, nullptr, nullptr);
}

bool ReversedNamedPipeServer::read(PipeMessage& message)
{
    return false;
}
