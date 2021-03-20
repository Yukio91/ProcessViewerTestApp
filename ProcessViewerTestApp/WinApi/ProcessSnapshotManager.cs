using ProcessViewerTestApp.Extensions;
using ProcessViewerTestApp.Helpers;
using ProcessViewerTestApp.Model;
using ProcessViewerTestApp.WinApi.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace ProcessViewerTestApp.WinApi
{
    internal class ProcessSnapshotManager: WinApiSnapshotBase, IDisposable
    {
        protected readonly ToolHelpHandle Handle;
        protected PROCESSENTRY32 ProcessEntry;

        protected static ToolHelpHandle CreateToolhelp32SnapshotProcesses()
        {
            return CreateToolhelp32Snapshot(SnapshotFlags.Process, 0);
        }

        public ProcessSnapshotManager()
        {
           Handle = CreateToolhelp32SnapshotProcesses();

            if (Handle.IsInvalid)
            {
                throw new Win32Exception();
            }

            ProcessEntry = new PROCESSENTRY32
            {
                dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32))
            };
        }

        public IEnumerable<ProcessInfo> GetProcessInfos(CancellationToken token)
        {
            var processList = new List<ProcessInfo>();

            if (Process32First(Handle, ref ProcessEntry) > 0)
            {
                do
                {
                    IntPtr processHandle = IntPtr.Zero;
                    try
                    {
                        if (token.IsCancellationRequested)
                        {
                            return new List<ProcessInfo>();
                        }

                        var processId = (int)ProcessEntry.th32ProcessID;
                        processHandle = NativeMethods.GetHandleByProcessId(processId);
                        if (processHandle == IntPtr.Zero)
                            continue;

                        var processFullPath = NativeMethods.GetProcessFullPathByHandle(processHandle);
                        var info = new ProcessInfo
                        {
                            Id = processId,
                            Handle = processHandle,
                            ShortName = ProcessEntry.szExeFile,
                            FullName = processFullPath,
                            Arguments = ProcessUtilities.GetCommandLine(processId),
                            AppIcon = ApplicationIconHelper.GetIconByPath(processFullPath),
                            Owner = UacManager.GetProcessOwner(processId),
                            Is64bit = NativeMethods.Is64BitChecker.IsWow64Process(processHandle),
                            IsElevated = UacManager.IsProcessElevated(processHandle),
                            IsSigned = WinTrust.VerifyEmbeddedSignature(processFullPath)
                        };

                        processList.Add(info);
                        ProcessEntry.dwSize = (uint)Marshal.SizeOf(ProcessEntry);
                    }
                    finally
                    {
                        if (processHandle != IntPtr.Zero)
                        {
                            CloseHandle(processHandle);
                        }
                    }
                }
                while (Process32Next(Handle, ref ProcessEntry) != 0);
            }

            if (Marshal.GetLastWin32Error() != ERROR_NO_MORE_FILES)
                throw new Win32Exception();

            return processList;
        }        

        #region Destructor 

        ~ProcessSnapshotManager()
        {
            Dispose(false);
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool dispose)
        {
            if (!Handle?.IsInvalid ?? false)
            {
                Handle?.Close();
            }
        }

        #endregion
    }

    
}