using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp1.Commands
{
    public class SortCommand: ICommand
    {
        VM parent;

        public SortCommand(VM parent)
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
            return true;
        }

        public void Execute(object parameter)
        {
            Sort(parent.ProcessQuery);
        }

        internal void Sort(BlockingCollection<SpecProcess> unsortedQuery)
        {
            IEnumerable<SpecProcess> sortedQuery;
            switch (parent.SortIndex)
            {
                case 0:
                    sortedQuery =
                                from SpecProcess in unsortedQuery
                                orderby SpecProcess.InnerProcess.ProcessName
                                select SpecProcess;
                    break;
                case 1:
                    sortedQuery =
                                from SpecProcess in unsortedQuery
                                orderby SpecProcess.InnerProcess.Id
                                select SpecProcess;
                    break;
                case 2:
                    sortedQuery =
                                from SpecProcess in unsortedQuery
                                orderby SpecProcess.InnerProcess.Responding
                                select SpecProcess;
                    break;
                case 3:
                    sortedQuery =
                                from SpecProcess in unsortedQuery
                                orderby SpecProcess.PercentCPULoad
                                select SpecProcess;
                    break;
                case 4:
                    sortedQuery =
                                from SpecProcess in unsortedQuery
                                orderby SpecProcess.PercentRAMLoad
                                select SpecProcess;
                    break;
                case 5:
                    sortedQuery =
                                from SpecProcess in unsortedQuery
                                orderby SpecProcess.InnerProcess.StartTime
                                select SpecProcess;
                    break;
                case 6:
                    sortedQuery =
                                from SpecProcess in unsortedQuery
                                orderby SpecProcess.InnerProcess.Threads.Count
                                select SpecProcess;
                    break;
                case 7:
                    sortedQuery =
                                from SpecProcess in unsortedQuery
                                orderby SpecProcess.UserName
                                select SpecProcess;
                    break;
                default:
                    sortedQuery = unsortedQuery;
                    break;
            }
            BlockingCollection<SpecProcess> processes = new BlockingCollection<SpecProcess>();
            for (int i = 0; i < unsortedQuery.Count; i++)
            {
                processes.Add(sortedQuery.ElementAt(i));
                if (sortedQuery.ElementAt(i).InnerProcess.Id == parent.selectedProcess.Id) parent.RowIndex = i;
            }
            parent.ProcessQuery = processes;
            parent.NotifyPropertyChanged("ProcessQuery");
        }
    }
}
