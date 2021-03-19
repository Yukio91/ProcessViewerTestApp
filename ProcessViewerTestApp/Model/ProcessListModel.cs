using ProcessViewerTestApp.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessViewerTestApp.Model
{
    public class ProcessListModel : NotifyObjectBase
    {
        #region Private Fields

        private readonly ProcessManager _manager;
        private ObservableCollection<ProcessInfo> _processInfos;

        #endregion

        public ObservableCollection<ProcessInfo> ProcessInfos
        {
            get
            {
                return _processInfos;
            }

            set
            {
                _processInfos = value;
                OnPropertyChanged();
            }
        }

        public ProcessListModel()
        {
            _manager = new ProcessManager();
            _manager.OnProcessListCompleted += _manager_OnProcessListCompleted;
        }

        private void _manager_OnProcessListCompleted(object sender, ProcessListCompletedEventArgs e)
        {
            ProcessInfos = new ObservableCollection<ProcessInfo>(e.ProcessInfos);
        }

        public void StartUpdateOperation()
        {
            _manager.Start();
        }

        public void CancelUpdateOperation()
        {
            _manager.Cancel();
        }
    }
}
