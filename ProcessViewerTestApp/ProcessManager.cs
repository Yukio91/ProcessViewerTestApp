using ProcessViewerTestApp.ViewModel;
using ProcessViewerTestApp.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ProcessViewerTestApp.WinApi.NativeMethods;
using static ProcessViewerTestApp.WinApi.ProcessSnapshotManager;

namespace ProcessViewerTestApp
{
    public class ProcessManager: IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

		public IEnumerable<ProcessInfo> GetProcessInfos()
		{
			ToolHelpHandle ptr = null;
			var list = new List<ProcessInfo>();
			try
			{
				ptr = CreateToolhelp32SnapshotProcesses();
				if (ptr.IsInvalid)
				{
					throw new Win32Exception();
				}
				var info = new PROCESSENTRY32();
				info.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));

				if (Process32First(ptr, ref info) > 0)
				{
					do
					{
						IntPtr _handle = IntPtr.Zero;
						try
						{
							var _id = (int)info.th32ProcessID;
							_handle = NativeMethods.GetHandleByProcessId(_id);
							if (_handle == IntPtr.Zero)
								continue;

							var _fullPath = NativeMethods.GetMainModuleFileName(_handle);
							var procinfo = new ViewModel.ProcessInfo
							{
								Id = _id,
								Handle = _handle,
								ShortName = info.szExeFile,
								FullName = _fullPath,
								Arguments = DangerousPebHack.GetCommandLine(_id),
								AppIcon = ApplicationIconManager.GetIconByPath(_fullPath),
								//AppIcon = _id > 0 ? ApplicationIconManager.GetIconByProcessId(_id) : null,
								Owner = UacHelper.GetProcessOwner(_id),
								Is64bit = NativeMethods.Is64Bit(_handle),
								IsElevated = UacHelper.IsProcessElevated(_handle),
								IsSigned = WinTrust.VerifyEmbeddedSignature(_fullPath)
							};
							list.Add(procinfo);
							info.dwSize = (uint)Marshal.SizeOf(info);
						}
						finally
						{
							if (_handle != IntPtr.Zero)
							{
								NativeMethods.CloseHandle(_handle);
								_handle = IntPtr.Zero;
							}
						}
					}
					while (Process32Next(ptr, ref info) != 0);
				}

				if (Marshal.GetLastWin32Error() != ERROR_NO_MORE_FILES)
					throw new Win32Exception();

				//var processIds = list.Select(pinfo => pinfo.Id).ToList();
				//var dict = ApplicationIconManager.GetIconsByProcessIds(processIds);
				//foreach(var procinfo in list)
    //            {
				//	if (dict.TryGetValue(procinfo.Id, out var icon))
				//		procinfo.AppIcon = icon;
    //            }
			}
			finally
			{
				if (!ptr?.IsInvalid ?? false)
				{
					ptr.Close();
				}
			}
			return list;
		}


		//public void Dispose()
		//{
		//	Dispose(true);
		//	GC.SuppressFinalize(this);
		//}
		//~ProcessMemory()
		//{
		//	Dispose(false);
		//}
		//protected virtual void Dispose(bool dispose)
		//{
		//	if (Handle != IntPtr.Zero)
		//	{
		//		CloseHandle(Handle);
		//		Handle = IntPtr.Zero;
		//	}
		//}
	}
}
