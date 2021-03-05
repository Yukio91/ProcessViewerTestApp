using ProcessViewerTestApp.WinApi.Base;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ProcessViewerTestApp.WinApi
{
    /// <summary>
    /// credit goes to Simon Mourier and user3670952
    /// http://stackoverflow.com/questions/16110936/read-other-process-current-directory-in-c-sharp/
    /// </summary>
    internal class ProcessUtilities : WinApiBaseMethods
    {
        private enum PROCESSINFOCLASS : int
        {
            ProcessBasicInformation = 0,
            ProcessWow64Information = 26,
        }

        private enum PebProcessParametersMember
        {
            CurrentDirectory,
            CommandLine,
        }

        private static int GetProcessParametersOffset(bool isTargetProc64Bit)
        {
            return isTargetProc64Bit ? 0x20 : 0x10;
        }

        private static int GetProcessParametersMemberOffset(PebProcessParametersMember offsetType, bool isTargetProc64Bit)
        {
            switch (offsetType)
            {
                case PebProcessParametersMember.CurrentDirectory:
                    return isTargetProc64Bit ? 0x38 : 0x24;
                case PebProcessParametersMember.CommandLine:
                    return isTargetProc64Bit ? 0x70 : 0x40;
            }
            throw new ArgumentException("unknown PebProcessParametersMember offset type");
        }        

        public static string GetCurrentDirectory(int processId)
        {
            return GetProcessParametersString(processId, PebProcessParametersMember.CurrentDirectory);
        }

        public static string GetCurrentDirectory(IntPtr processHandle)
        {
            return GetProcessParametersString(processHandle, PebProcessParametersMember.CurrentDirectory);
        }

        public static string GetCommandLine(int processId)
        {
            return GetProcessParametersString(processId, PebProcessParametersMember.CommandLine);
        }

        public static string GetCommandLine(IntPtr processHandle)
        {
            return GetProcessParametersString(processHandle, PebProcessParametersMember.CommandLine);
        }

        private static string GetProcessParametersString(int processId, PebProcessParametersMember offsetType)
        {
            IntPtr handle = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryRead, false, processId);
            if (handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                return GetProcessParametersString(handle, offsetType);
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private static string GetProcessParametersString(IntPtr processHandle, PebProcessParametersMember offsetType)
        {
            if (processHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                bool isTargetWow64Process = NativeMethods.Is64BitChecker.IsWow64Process(processHandle);
                bool isTarget64BitProcess = Environment.Is64BitOperatingSystem && !isTargetWow64Process;

                long processParametersOffset = GetProcessParametersOffset(isTarget64BitProcess);
                long offset = GetProcessParametersMemberOffset(offsetType, isTarget64BitProcess);

                if (isTargetWow64Process)
                {
                    IntPtr peb32 = new IntPtr();

                    int hr = NtQueryInformationProcess(processHandle, (int)PROCESSINFOCLASS.ProcessWow64Information, ref peb32, IntPtr.Size, IntPtr.Zero);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    long pebAddress = peb32.ToInt64();

                    IntPtr pp = new IntPtr();
                    if (!ReadProcessMemory(processHandle, new IntPtr(pebAddress + processParametersOffset), ref pp, new IntPtr(Marshal.SizeOf(pp)), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    UNICODE_STRING_32 us = new UNICODE_STRING_32();
                    if (!ReadProcessMemory(processHandle, new IntPtr(pp.ToInt64() + offset), ref us, new IntPtr(Marshal.SizeOf(us)), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    if (us.Buffer == 0 || us.Length == 0)
                        return null;

                    string s = new string('\0', us.Length / 2);
                    if (!ReadProcessMemory(processHandle, new IntPtr(us.Buffer), s, new IntPtr(us.Length), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    return s;
                }
                else
                {
                    PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                    int hr = NtQueryInformationProcess(processHandle, (int)PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, Marshal.SizeOf(pbi), IntPtr.Zero);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    long pebAddress = pbi.PebBaseAddress.ToInt64();

                    IntPtr pp = new IntPtr();
                    if (!ReadProcessMemory(processHandle, new IntPtr(pebAddress + processParametersOffset), ref pp, new IntPtr(Marshal.SizeOf(pp)), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    UNICODE_STRING us = new UNICODE_STRING();
                    if (!ReadProcessMemory(processHandle, new IntPtr((long)pp + offset), ref us, new IntPtr(Marshal.SizeOf(us)), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    if (us.Buffer == IntPtr.Zero || us.Length == 0)
                        return null;

                    string s = new string('\0', us.Length / 2);
                    if (!ReadProcessMemory(processHandle, us.Buffer, s, new IntPtr(us.Length), IntPtr.Zero))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    return s;
                }
            }
            catch (Win32Exception)
            {
                return String.Empty;
            }
        }

        #region WinApi

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public short Length;
            public short MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING_32
        {
            public short Length;
            public short MaximumLength;
            public int Buffer;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, ref PROCESS_BASIC_INFORMATION ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, ref IntPtr ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref IntPtr lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref UNICODE_STRING lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref UNICODE_STRING_32 lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        #endregion
    }
}
