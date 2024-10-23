#include "glob.h"

namespace glob
{
    ReversedNamedPipeServer* PipeServer = new ReversedNamedPipeServer(GetCurrentProcessId());
}
