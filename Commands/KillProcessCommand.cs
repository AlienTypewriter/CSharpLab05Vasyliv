using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1.Commands
{
    public class KillProcessCommand : ICommand
        {
            VM parent;

            public KillProcessCommand(VM parent)
            {
                this.parent = parent;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void RaiseCanExecuteChanged()
            {
                CommandManager.InvalidateRequerySuggested();
            }


            public bool CanExecute(object parameter)
            {
                return parent.RowIndex != -1;
            }

            public void Execute(object parameter)
            {
            try {
            parent.selectedProcess.Kill();
            } catch(Win32Exception)
            {
                MessageBox.Show("System denied access");
            }
        }
    } 
}
