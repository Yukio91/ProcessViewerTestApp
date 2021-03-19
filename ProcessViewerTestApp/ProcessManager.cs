using ProcessViewerTestApp.Model;
using ProcessViewerTestApp.WinApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace ProcessViewerTestApp
{
    public class ProcessManager: IDisposable
    {
        private readonly BackgroundWorker _worker;

        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;

        private OperationState _operationState = OperationState.None;
        private IEnumerable<ProcessInfo> _processInfos;

        public ProcessManager()
        {
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;

            _worker = new BackgroundWorker();
            _worker.DoWork += _worker_DoWork;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;
        }

        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _processInfos = GetProcessInfos(_token);
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CompleteProcessList(_processInfos);
            _operationState = OperationState.Completed;
        }

        public IEnumerable<ProcessInfo> GetProcessInfos(CancellationToken token)
		{
			using (var processSnapshot = new ProcessSnapshotManager())
			{
				return processSnapshot.GetProcessInfos(token);
			}			
		}
        
        public void Start()
        {
            try
            {
                _operationState = OperationState.DoWorking;

                _worker.RunWorkerAsync();
            }
            catch (Exception)
            {
                _operationState = OperationState.Error;
            }
        }

        public bool Cancel()
        {
            if (_operationState != OperationState.DoWorking)
                return false;

            _tokenSource.Cancel();
            _operationState = OperationState.Canceled;
            return true;
        }

        private void CompleteProcessList(IEnumerable<ProcessInfo> processInfos)
        {
            if (OnProcessListCompleted == null) return;

            OnProcessListCompleted(this, new ProcessListCompletedEventArgs(processInfos));
        }

        public delegate void ProcessListCompletedHandler(object sender, ProcessListCompletedEventArgs e);
        public event ProcessListCompletedHandler OnProcessListCompleted;

        #region Destructor 

        ~ProcessManager()
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
            _worker?.Dispose();
        }

        #endregion
    }

    public class ProcessListCompletedEventArgs : EventArgs
    {
        public IEnumerable<ProcessInfo> ProcessInfos { get; private set; }

        public ProcessListCompletedEventArgs(IEnumerable<ProcessInfo> processInfos)
        {
            ProcessInfos = processInfos;
        }
    }

    public enum OperationState
    {
        None,
        DoWorking,
        Completed,
        Canceled,
        Error
    }
}
