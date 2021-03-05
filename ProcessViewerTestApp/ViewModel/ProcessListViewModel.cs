using ProcessViewerTestApp.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessViewerTestApp.ViewModel
{
    public class ProcessListViewModel : NotifyObjectBase
    {
        private ObservableCollection<ProcessInfo> _processInfos;

        #region Public Properties

        public ObservableCollection<ProcessInfo> ProcessList
        {
            get { return _processInfos;}
            set
            {
                _processInfos = value;
                OnPropertyChanged();
            }
        }

        public ICommand UpdateCommand { get; }

        #endregion
    }
}
