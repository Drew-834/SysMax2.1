using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Management;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// 
    /// </summary>
    public partial class OptimizationPage : Page
    {
        public OptimizationPage()
        {
            InitializeComponent();
            LoadSystemInfo();
        }

        private void LoadSystemInfo()
        {
            CpuUsageBar.Value = GetCpuUsage();
            CpuUsageText.Text = $"{CpuUsageBar.Value}%";

            RamUsageBar.Value = GetRamUsage();
            RamUsageText.Text = $"{RamUsageBar.Value}%";

            LoadStartupApplications();
            LoadRunningProcesses(); // ✅ FIX: Ensured this method exists
        }

        private double GetCpuUsage()
        {
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(1000);
            return Math.Round(cpuCounter.NextValue(), 2);
        }

        private double GetRamUsage()
        {
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            ulong totalRam = GetTotalPhysicalMemory(); // ✅ FIX: Replaced VisualBasic reference with WMI
            double usedRam = totalRam - (ramCounter.NextValue() * 1024 * 1024); // Convert MB to Bytes
            return Math.Round((usedRam / totalRam) * 100, 2);
        }

        private ulong GetTotalPhysicalMemory()
        {
            ulong totalRam = 0;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");

                foreach (ManagementObject obj in searcher.Get())
                {
                    totalRam = (ulong)obj["TotalPhysicalMemory"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving total physical memory: " + ex.Message);
            }
            return totalRam;
        }

        private void LoadStartupApplications()
        {
            StartupList.Items.Clear();
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    foreach (string appName in key.GetValueNames())
                    {
                        StartupList.Items.Add(appName);
                    }
                }
            }
        }

        private void LoadRunningProcesses() // ✅ FIX: Added missing method
        {
            ProcessList.Items.Clear();
            var processes = Process.GetProcesses()
                                   .Select(p => p.ProcessName)
                                   .Distinct()
                                   .OrderBy(p => p);

            foreach (var process in processes)
            {
                ProcessList.Items.Add(process);
            }
        }

        private void DisableStartup_Click(object sender, RoutedEventArgs e)
        {
            if (StartupList.SelectedItem == null) return;

            string selectedApp = StartupList.SelectedItem.ToString();
            string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath, true))
            {
                if (key != null)
                {
                    key.DeleteValue(selectedApp, false);
                }
            }

            LoadStartupApplications();
        }

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessList.SelectedItem == null) return;

            string selectedProcess = ProcessList.SelectedItem.ToString();
            try
            {
                foreach (var process in Process.GetProcessesByName(selectedProcess))
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error terminating process: {ex.Message}");
            }

            LoadRunningProcesses();
        }

        private void CheckPing_Click(object sender, RoutedEventArgs e)
        {
            string host = PingInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(host)) return;

            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(host, 1000);
                PingResultText.Text = reply.Status == IPStatus.Success
                    ? $"Ping: {reply.RoundtripTime} ms"
                    : "Ping failed.";
            }
            catch (Exception ex)
            {
                PingResultText.Text = $"Error: {ex.Message}";
            }
        }
    }
}
  