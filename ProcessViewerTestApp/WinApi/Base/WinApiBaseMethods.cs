using System;
using System.Runtime.InteropServices;

namespace ProcessViewerTestApp.WinApi.Base
{
    internal abstract class WinApiBaseMethods
    {
        #region Constants

        protected const int INVALID_HANDLE_VALUE = -1;
        protected const int ERROR_NO_MORE_FILES = 18;

        #endregion

        #region Flags

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

        #endregion

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern IntPtr OpenProcess(ProcessAccessFlags accessFlags, bool bInheritHandle, int processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        protected static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        protected static extern bool CloseHandle(HandleRef handle);
    }
}