using ProcessViewerTestApp.Base;
using ProcessViewerTestApp.Helpers;
using ProcessViewerTestApp.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProcessViewerTestApp.ViewModel
{
    public class ProcessListViewModel : NotifyObjectBase
    {
        private readonly ProcessListModel _model;

        private ButtonState _buttonState = ButtonState.Update;
        private ICommand _updateCommand;

        #region Public Properties

        public ObservableCollection<ProcessInfo> ProcessList
        {
            get { return _model.ProcessInfos; }
            set
            {
                _model.ProcessInfos = value;
                ButtonState = ButtonState.Update;
                OnPropertyChanged();
            }
        }

        public ICommand UpdateCommand
        {
            get => _updateCommand ?? (_updateCommand =
                new RelayCommand(UpdateCommandExecute,
                    canExecute => UpdateCommandCanExecute));
        }

        public bool UpdateCommandCanExecute { get => _buttonState != ButtonState.Breaking; }
       
        public ButtonState ButtonState
        {
            get { return _buttonState; }
            set
            {
                _buttonState = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public ProcessListViewModel()
        {
            _model = new ProcessListModel();
            ButtonState = ButtonState.Update;

            _model.PropertyChanged += _model_PropertyChanged;
        }

        private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(_model.ProcessInfos)))
            {
                OnPropertyChanged(nameof(ProcessList));
            }
        }

        public void UpdateCommandExecute(object obj)
        {
            UpdateProcessList();
        }

        protected void UpdateProcessList()
        {
            switch (_buttonState)
            {
                case ButtonState.Update:
                    ButtonState = ButtonState.Break;
                    _model.StartUpdateOperation();
                    break;
                case ButtonState.Break:
                    ButtonState = ButtonState.Breaking;
                    _model.CancelUpdateOperation();
                    break;
                case ButtonState.Breaking:
                    break;
            }
        }
    }

    public enum ButtonState
    {
        Update,
        Break,
        Breaking
    }
}
