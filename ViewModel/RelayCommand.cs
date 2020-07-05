using System;
using System.Windows.Input;

namespace Levitate.ViewModel
{
    public class RelayCommand : ICommand
    {
        #region Declarations

        public delegate void ExecuteDelegate(object parameter);
        private readonly Func<bool> _canExecute;
        private readonly ExecuteDelegate _execute;

        #endregion


        #region Constructors

        public RelayCommand(ExecuteDelegate execute) : this(execute, null)
        {
        }

        public RelayCommand(ExecuteDelegate execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion


        #region ICommand

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute.Invoke();
        }
        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }

        #endregion
    }
}
