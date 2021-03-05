using ProcessViewerTestApp.Base;

namespace ProcessViewerTestApp.ViewModel
{
    public class MainWindowViemModel: NotifyObjectBase
    {
        public ProcessListViewModel ProcessListViewModel { get; }

        public MainWindowViemModel()
        {
            ProcessListViewModel = new ProcessListViewModel();
        }
    }
}