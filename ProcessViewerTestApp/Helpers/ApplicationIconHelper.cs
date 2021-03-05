using System.Drawing;
using System.IO;

namespace ProcessViewerTestApp.Helpers
{
    public class ApplicationIconHelper
    {
        public static Icon GetIconByPath(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            return Icon.ExtractAssociatedIcon(filePath);
        }
    }
}