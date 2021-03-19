using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ProcessViewerTestApp.WinApi.Base
{
    internal abstract class NativeMethodsBase : WinApiBaseMethods
    {
        #region WinApi Methods

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        protected static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        protected static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

        protected static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            return IntPtr.Size > 4 ?
                GetClassLongPtr64(hWnd, nIndex) :
                new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        }

        #region Get Process FullName

        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        protected static string GetMainModuleFileName(IntPtr handle, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(handle, 0, fileNameBuilder, ref bufferLength) ?
                fileNameBuilder.ToString() :
                string.Empty;
        }

        #endregion

        [DllImport("user32.dll")]
        protected static extern IntPtr FindWindowEx(IntPtr parentWindow, IntPtr previousChildWindow, string windowClass, string windowTitle);

        [DllImport("user32.dll")]
        protected static extern IntPtr GetWindowThreadProcessId(IntPtr window, out int process);

        [DllImport("gdi32.dll", SetLastError = true)]
        protected static extern bool DeleteObject(IntPtr hObject);

        #region Get Application Icon

        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int WM_GETICON = 0x7F;

        protected static Icon GetAppIcon(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return null;

            IntPtr iconHandle = SendMessage(hwnd, WM_GETICON, ICON_SMALL2, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd, WM_GETICON, ICON_SMALL, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICON);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICONSM);

            if (iconHandle == IntPtr.Zero)
                return null;

            return Icon.FromHandle(iconHandle);
        }

        #endregion

        #endregion

    }
}