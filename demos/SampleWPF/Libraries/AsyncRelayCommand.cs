using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.ViewModels.MVVM
{
    public class AsyncRelayCommand<T> : ICommand
    {
        readonly Func<T, Task> _execute = null;
        readonly Predicate<T> _canExecute = null;

        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }

        public async void Execute(object parameter)
        {
            await _execute((T)parameter);
        }
    }
}
