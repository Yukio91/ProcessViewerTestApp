using Microsoft.Win32.SafeHandles;
using ProcessViewerTestApp.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessViewerTestApp.WinApi
{
    internal static class NativeMethods
    {
        private const int INVALID_HANDLE_VALUE = -1;
        public const int ERROR_NO_MORE_FILES = 18;

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        private static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        private static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

        internal static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size > 4 ?
                GetClassLongPtr64(hWnd, nIndex) :
                new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        //[DllImport("kernel32.dll", SetLastError = true)]
        //static extern IntPtr OpenProcess(ProcessAccessFlags accessFlag/*UInt32 dwDesiredAccess*/, Int32 bInheritHandle, Int32 dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags accessFlags/*uint processAccess*/, bool bInheritHandle, int processId);

        public static IntPtr OpenProcess(ProcessAccessFlags flags, int processId)
        {
            return OpenProcess(flags, false, processId);
        }

        internal static IntPtr GetHandleByProcessId(int processId)
        {
            return OpenProcess(ProcessAccessFlags.QueryLimitedInformation | ProcessAccessFlags.VirtualMemoryOperation, processId);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool CloseHandle(HandleRef handle);


        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

        public static bool Is64Bit(IntPtr handle)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;

            if (!IsWow64Process(handle, out bool isWow64))
                throw new Win32Exception();

            return isWow64;
        }

        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(IntPtr handle, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                String.Empty;
        }
    }

    public static class ProcessSnapshotManager
    {
        [Flags]
        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern ToolHelpHandle CreateToolhelp32Snapshot(SnapshotFlags dwFlags, int processId);

        internal static ToolHelpHandle CreateToolhelp32SnapshotProcesses()
        {
            return CreateToolhelp32Snapshot(SnapshotFlags.Process, 0);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int Process32First(ToolHelpHandle/*IntPtr*/ hSnapshot,
                                 ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int Process32Next(ToolHelpHandle/*IntPtr*/ hSnapshot,
                                        ref PROCESSENTRY32 lppe);

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        };

        public class ToolHelpHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private ToolHelpHandle()
                : base(true)
            {
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
            protected override bool ReleaseHandle()
            {
                return NativeMethods.CloseHandle(handle);
            }
        }
    }

    public static class ApplicationIconManager
    {
        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int WM_GETICON = 0x7F;

        public static Icon GetIconByPath(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return null;

            return System.Drawing.Icon.ExtractAssociatedIcon(filePath);
        }

        public static Icon GetAppIcon(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return null;

            IntPtr iconHandle = NativeMethods.SendMessage(hwnd, WM_GETICON, ICON_SMALL2, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.SendMessage(hwnd, WM_GETICON, ICON_SMALL, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.GetClassLongPtr(hwnd, GCL_HICON);
            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.GetClassLongPtr(hwnd, GCL_HICONSM);

            if (iconHandle == IntPtr.Zero)
                return null;

            return Icon.FromHandle(iconHandle);
        }

        public static Icon GetIconByProcessId(int processId)
        {
            var hwnds = GetProcessWindows(processId);
            foreach (var hwnd in hwnds)
            {
                try
                {
                    var icon = GetAppIcon(hwnd);
                    if (icon != null)
                        return icon;
                }
                finally
                {

                }
            }

            return null;
        }

        public static Dictionary<int, Icon> GetIconsByProcessIds(List<int> processIds)
        {
            var iconsDict = new Dictionary<int, Icon>();
            var dict = GetProcessWindows(processIds);
            foreach (var kvp in dict)
            {
                try
                {
                    foreach (var hwnd in kvp.Value)
                    {
                        var icon = GetAppIcon(hwnd);
                        if (icon != null)
                        {
                            iconsDict.Add(kvp.Key, icon);
                            break;
                        }
                            
                    }
                }
                finally
                {

                }
            }

            return iconsDict;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr parentWindow, IntPtr previousChildWindow, string windowClass, string windowTitle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr window, out int process);

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

        private static Dictionary<int, List<IntPtr>> GetProcessWindows(List<int> processIds)
        {
            var dict = new Dictionary<int, List<IntPtr>>();
            IntPtr pLast = IntPtr.Zero;
            do
            {
                pLast = FindWindowEx(IntPtr.Zero, pLast, null, null);
                var currentHandle = GetWindowThreadProcessId(pLast, out int iProcess_);

                if (currentHandle == IntPtr.Zero)
                    break;

                if (processIds.Contains(iProcess_))
                {
                    if (!dict.ContainsKey(iProcess_))
                    {
                        dict.Add(iProcess_, new List<IntPtr>());
                    }

                    dict[iProcess_].Add(pLast);
                }

            } while (pLast != IntPtr.Zero);

            return dict;
        }

        public static List<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            List<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
            List<IntPtr> dsProcRootWindows = new List<IntPtr>();
            foreach (IntPtr hWnd in rootWindows)
            {
                /*WindowsInterop.User32.*/GetWindowThreadProcessId(hWnd, out uint lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                /*WindowsInterop.*/Win32Callback childProc = new /*WindowsInterop.*/Win32Callback(EnumWindow);
                /*WindowsInterop.User32.*/EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);
    }

    
}