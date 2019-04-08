using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfApp1.Commands;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;

namespace WpfApp1
{
    public class VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        SortCommand sortCommand;
        KillProcessCommand killProcessCommand;
        OpenFolderCommand openFolderCommand;
        private int rowIndex = 0;
        private int sortIndex = 0;
        private int methodIndex = 0;
        internal Process selectedProcess = Process.GetProcessById(0);
        private BlockingCollection<SpecProcess> processQuery;
        public string[] sortList = {"Name","ID","Responding","CPU Load", "RAM Load", "Started at", "Thread count", "User name" };
        public int RowIndex { get => rowIndex; set
            {
                rowIndex = value;
                try
                {
                    selectedProcess = Process.GetProcessById(ProcessQuery.ElementAt(RowIndex).InnerProcess.Id);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return;
                }
                NotifyPropertyChanged("ProcessThreads");
                NotifyPropertyChanged("ProcessModules");
            }
        }
        public int SortIndex { get => sortIndex; set => sortIndex = value; }
        public int MethodIndex { get => methodIndex; set => methodIndex = value; }
        public SortCommand SortCommand { get => sortCommand; set => sortCommand = value; }
        public KillProcessCommand KillProcessCommand { get => killProcessCommand; set => killProcessCommand = value; }
        public OpenFolderCommand OpenFolderCommand { get => openFolderCommand; set => openFolderCommand = value; }
        public BlockingCollection<SpecProcess> ProcessQuery { get =>processQuery; set => processQuery=value; }
        public ProcessThreadCollection ProcessThreads { get
            {
                try
                {
                    return selectedProcess.Threads;
                }
                catch (Win32Exception)
                {
                    return null;
                }
            } }
        public ProcessModuleCollection ProcessModules
        {
            get
            {
                try
                {
                    return selectedProcess.Modules;
                }
                catch (Win32Exception)
                {
                    return null;
                }
            }
        }


        public VM()
        {
            Process[] processes = Process.GetProcesses();
            ProcessQuery = new BlockingCollection<SpecProcess>();
            foreach(Process p in processes)
            {
                ProcessQuery.Add(new SpecProcess(p));
            }
            KillProcessCommand = new KillProcessCommand(this);
            SortCommand = new SortCommand(this);
            LaunchConstantUpdate();
            LaunchConstantRequery();
        }

        private int msToNext = 1825;
        private int slept = 2000;
        private BlockingCollection<SpecProcess> phQuery;
        private async void LaunchConstantRequery()
        {
            await Task.Run(async () => {
                for (; ; )
                {
                    SpecProcess[] copy = new SpecProcess[ProcessQuery.Count];
                    for (int i = 0; i < ProcessQuery.Count; i++)
                    {
                        copy[i] = ProcessQuery.Take();
                    }
                    phQuery = new BlockingCollection<SpecProcess>();
                    foreach (Process p in Process.GetProcesses())
                    {
                        try
                        {
                            SpecProcess spec = copy.Where(x => x != null && x.InnerProcess.Id == p.Id).Single();
                            phQuery.Add(spec);
                        }
                        catch (InvalidOperationException)
                        {
                            phQuery.Add(new SpecProcess(p));
                        }
                    }
                    Thread.Sleep(slept);
                    SortCommand.Sort(phQuery);
                    int toSleep;
                    toSleep = (3 < msToNext / 500) ? 2000 : 2500;
                    slept = 5000 - toSleep;
                    Thread.Sleep(toSleep);
                }
            });        
        }

        private async void LaunchConstantUpdate()
        {
            await Task.Run(() => {
                for (; ; ){
                    foreach (SpecProcess s in ProcessQuery)
                    {
                        s.CallUpdate();
                    }
                    msToNext += 1825;
                    msToNext %= 5000;
                    Thread.Sleep(1825);
                    }
            });
        }
    }
}
