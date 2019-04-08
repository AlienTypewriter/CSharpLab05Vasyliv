using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualBasic.Devices;
namespace WpfApp1
{
    public class SpecProcess : INotifyPropertyChanged
    {
        private Process innerProcess;
        private double percentCPULoad;
        private double percentRAMLoad;
        private double totalRAM = new ComputerInfo().AvailablePhysicalMemory;
        private PerformanceCounter cpuWatch;

        public double PercentRAMLoad => percentRAMLoad;
        public double PercentCPULoad => percentCPULoad;
        public string UserName {
            get {
                if (string.IsNullOrEmpty(InnerProcess.StartInfo.UserName)) return "{Current User}";
                else return InnerProcess.StartInfo.UserName;
            }
        }

        public Process InnerProcess { get => innerProcess; set => innerProcess = value; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SpecProcess(Process p)
        {
            try
            {
                InnerProcess = p;
                cpuWatch = new PerformanceCounter("Process", "% Processor Time", InnerProcess.ProcessName);
                percentCPULoad = Math.Round(cpuWatch.NextValue() / Environment.ProcessorCount, 2);
                percentRAMLoad = Math.Round(InnerProcess.WorkingSet64 * 100 / totalRAM, 2);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void Close()
        {
            InnerProcess.Close();
        }

        public SpecProcess CallUpdate()
        {
            if (InnerProcess != null)
            {
                try
                {
                    percentCPULoad = Math.Round(cpuWatch.NextValue() / Environment.ProcessorCount, 2);
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
                percentRAMLoad = Math.Round(InnerProcess.WorkingSet64 * 100 / (double)(new ComputerInfo().AvailablePhysicalMemory), 2);
                NotifyPropertyChanged("PercentCPULoad");
                NotifyPropertyChanged("PercentRAMLoad");
                return this;
            } else
            {
                return null;
            }
        }
    }
}
