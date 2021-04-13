using System;
using System.Windows;
using System.Windows.Input;

namespace AiurVersionControl.SampleWPF.Libraries
{
    public class RelayCommand<T> : ICommand
    {
        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
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

        public void Execute(object parameter)
        {
            try
            {
                _execute((T)parameter);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Critical action failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}