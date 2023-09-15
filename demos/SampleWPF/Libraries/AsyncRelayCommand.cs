using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Aiursoft.AiurVersionControl.SampleWPF.Libraries
{
    public class AsyncRelayCommand<T> : ICommand
    {
        readonly Func<T, Task> _execute;
        readonly Predicate<T> _canExecute;

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
            try
            {
                await _execute((T)parameter);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Critical action failure", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
