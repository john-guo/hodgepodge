using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace dohnadohna
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Action<object> OnExecute = delegate { };
        public Func<object, bool> OnCanExecute = parameter => true;
        public bool CanExecute(object parameter)
        {
            return OnCanExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            OnExecute?.Invoke(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

}
