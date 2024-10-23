using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.System.Memory;
using Windows.Win32.System.Threading;

namespace App.Util;

public static class DllInjector
{
    public enum InjectResult : byte
    {
        Success = 0,
        DllPathInvalid,

        FailedOpenProcess,
        FailedGetKernel32ModuleHandle,
        FailedGetLoadLibraryAddress,
        FailedAllocateRemoteMemory,
        FailedWriteRemoteMemory,
        FailedCreateRemoteThread
    }

#pragma warning disable CA1416
    public static unsafe InjectResult Inject(uint pid, string dllPath)
    {
        if (!File.Exists(dllPath))
        {
            Console.WriteLine("Dll path invalid: " + dllPath);
            return InjectResult.DllPathInvalid;
        }
        
        if (!Path.IsPathRooted(dllPath)) // Make absolute path
        {
            dllPath = Path.Combine(AppContext.BaseDirectory, dllPath);
        }
        
        var hProc = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, pid);
        if (hProc == -1) // INVALID_HANDLE_VALUE  
        {
            Console.WriteLine("OpenProcess failed: " + Marshal.GetLastWin32Error());
            return InjectResult.FailedOpenProcess;
        }
        
        var hKernel32 = NativeLibrary.Load("kernel32.dll");
        if (hKernel32 == IntPtr.Zero)
        {
            Console.WriteLine("GetModuleHandle failed: " + Marshal.GetLastWin32Error());
            PInvoke.CloseHandle(hProc);
            return InjectResult.FailedGetKernel32ModuleHandle;
        }
        
        var pLoadLibraryW =  NativeLibrary.GetExport(hKernel32, "LoadLibraryW");
        if (pLoadLibraryW == IntPtr.Zero)
        {
            Console.WriteLine("GetProcAddress failed: " + Marshal.GetLastWin32Error());
            PInvoke.CloseHandle(hProc);
            return InjectResult.FailedGetLoadLibraryAddress;
        }
     
        var pathBytes = Encoding.Unicode.GetBytes(dllPath + '\0');
        var pRemoteMem = PInvoke.VirtualAllocEx(hProc, null, (nuint)pathBytes.Length,
            VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
            PAGE_PROTECTION_FLAGS.PAGE_READWRITE);
        if (pRemoteMem == null)
        {
            Console.WriteLine("VirtualAllocEx failed: " + Marshal.GetLastWin32Error());
            PInvoke.CloseHandle(hProc);
            return InjectResult.FailedAllocateRemoteMemory;
        }
        
        fixed (byte* buffer = pathBytes)
        {
            if (!PInvoke.WriteProcessMemory(hProc, pRemoteMem, buffer, (nuint)pathBytes.Length, null))
            {
                Console.WriteLine("WriteProcessMemory failed: " + Marshal.GetLastWin32Error());
                PInvoke.VirtualFreeEx(hProc, pRemoteMem, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
                PInvoke.CloseHandle(hProc);
                return InjectResult.FailedWriteRemoteMemory;
            }
        }
      
        var hThread = PInvoke.CreateRemoteThread(hProc, null, 0,
            Marshal.GetDelegateForFunctionPointer<LPTHREAD_START_ROUTINE>(pLoadLibraryW),
            pRemoteMem, 0, null);
        if (hThread == IntPtr.Zero)
        {
            Console.WriteLine("CreateRemoteThread failed: " + Marshal.GetLastWin32Error());
            PInvoke.VirtualFreeEx(hProc, pRemoteMem, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
            PInvoke.CloseHandle(hProc);
            return InjectResult.FailedCreateRemoteThread;
        }

        PInvoke.WaitForSingleObject(hThread, 5000);
        PInvoke.VirtualFreeEx(hProc, pRemoteMem, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
        PInvoke.CloseHandle(hProc);
        return InjectResult.Success;
    }
#pragma warning restore CA1416
}