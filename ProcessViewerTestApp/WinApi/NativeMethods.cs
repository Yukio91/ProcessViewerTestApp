using ProcessViewerTestApp.WinApi.Base;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ProcessViewerTestApp.WinApi
{
    internal class NativeMethods : NativeMethodsBase
    {
        protected static IntPtr OpenProcess(ProcessAccessFlags flags, int processId)
        {
            return OpenProcess(flags, false, processId);
        }

        internal static IntPtr GetHandleByProcessId(int processId)
        {
            return OpenProcess(ProcessAccessFlags.QueryLimitedInformation | ProcessAccessFlags.VirtualMemoryOperation, processId);
        }

        internal static string GetProcessFullPathByHandle(IntPtr handle)
        {
            return GetMainModuleFileName(handle);
        }        

        #region Get Icon

        internal static Icon GetIconByProcessId(int processId)
        {
            var hwnds = GetProcessWindows(processId);
            foreach (var hwnd in hwnds)
            {
                var icon = GetAppIcon(hwnd);
                if (icon != null)
                    return icon;
            }

            return null;
        }          

        private static IntPtr[] GetProcessWindows(int processId)
        {
            IntPtr[] apRet = (new IntPtr[256]);
            int iCount = 0;
            IntPtr pLast = IntPtr.Zero;
            do
            {
                pLast = FindWindowEx(IntPtr.Zero, pLast, null, null);
                var currentHandle = GetWindowThreadProcessId(pLast, out int iProcess_);

                if (currentHandle == IntPtr.Zero)
                    break;

                if (iProcess_ == processId)
                    apRet[iCount++] = pLast;

            } while (pLast != IntPtr.Zero);

            System.Array.Resize(ref apRet, iCount);
            return apRet;
        }

        #endregion

        public class Is64BitChecker
        {
            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool wow64Process);

            public static bool IsWow64Process(IntPtr hProcess)
            {
                if (Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1 ||
                    Environment.OSVersion.Version.Major >= 6)
                {
                    if (!IsWow64Process(hProcess, out bool retVal))
                    {
                        return false;
                    }

                    return retVal;
                }
                else
                {
                    return false;
                }
            }

            public static bool IsWow64Process(int pid)
            {
                IntPtr handle = OpenProcess(ProcessAccessFlags.QueryInformation, false, pid);
                if (handle == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                try
                {
                    return IsWow64Process(handle);
                }
                finally
                {
                    CloseHandle(handle);
                }
            }
        }
    }    
}