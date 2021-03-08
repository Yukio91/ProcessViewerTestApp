using ProcessViewerTestApp.Model;
using ProcessViewerTestApp.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ProcessViewerTestApp
{
    public class ProcessManager
    {
		public IEnumerable<ProcessInfo> GetProcessInfos()
		{
			using (var processSnapshot = new ProcessSnapshotManager())
			{
				return processSnapshot.GetProcessInfos();
			}			
		}
    }
}
