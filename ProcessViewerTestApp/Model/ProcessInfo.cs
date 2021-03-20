using ProcessViewerTestApp.Extensions;
using System;
using System.Drawing;

namespace ProcessViewerTestApp.Model
{
    public class ProcessInfo
    {
        public IntPtr Handle { get; set; }

        public Icon AppIcon { get; set; }
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Arguments { get; set; }
        public string Owner { get; set; }
        public bool Is64bit { get; set; }
        public bool IsElevated { get; set; }
        public bool IsSigned { get; set; }
    }
}