using SysMax2._1;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SysMax2._1.Pages
{
    /// 
    /// Interaction logic for SystemOverviewPage.xaml
    /// 
    /// I've commented out the RAM as we are getting a Device error. I fixed it once, and it's not that difficult. Line 146, and the Method.
    public partial class SystemOverviewPage : Page
    {
        private Random random = new Random();
        private bool isScanning = false;
        private MainWindow mainWindow;

        public SystemOverviewPage()
        {
            InitializeComponent();

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Initialize with simulated data
            PopulateSystemInfo();

            // Hide no issues message initially
            NoIssuesMessage.Visibility = Visibility.Collapsed;
        }

        private void QuickAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string action = button.Name.Replace("Button", "");

                // Show a status message
                ShowStatusMessage($"Running {button.Content}...");

                // In a real application, these would perform actual system actions
                switch (action)
                {
                    case "CheckUpdates":
                        try { Process.Start("ms-settings:windowsupdate"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Checking for Windows updates. This might take a moment...");
                        }
                        break;

                    case "Cleanup":
                        try { Process.Start("cleanmgr.exe"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Disk Cleanup helps free up space by removing temporary files and emptying the Recycle Bin.");
                        }
                        break;

                    case "NetworkDiagnostics":
                        try { Process.Start("msdt.exe", "/id NetworkDiagnosticsNetworkAdapter"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Running network diagnostics to check your internet connection...");
                        }
                        break;

                    case "StartupApps":
                        try { Process.Start("taskmgr.exe", "/7"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Managing startup apps can help your computer boot faster. Disable programs you don't need to start automatically.");
                        }
                        break;

                    case "SecurityScan":
                        try { Process.Start("ms-settings:windowsdefender"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Running a quick security scan to check for threats...");
                        }
                        break;
                }
            }
        }

        private void FixIssue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string issueType = button.Tag?.ToString() ?? "";

                // In a reality, these would perform actual fixes
                switch (issueType)
                {
                    case "DiskSpace":
                        ShowStatusMessage("Launching Disk Cleanup...");
                        try { Process.Start("cleanmgr.exe"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("I'm launching Disk Cleanup to help free up space. This will remove temporary files and empty your Recycle Bin.");
                        }
                        break;

                    case "WindowsUpdate":
                        ShowStatusMessage("Launching Windows Update...");
                        try { Process.Start("ms-settings:windowsupdate"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Installing Windows updates helps keep your computer secure and running smoothly. After updates are installed, a restart will be needed.");
                        }
                        break;
                }
            }
        }

        private void ShowStatusMessage(string message)
        {
            // Update the main window's status text if available
            if (mainWindow != null)
            {
                // Use reflection to access the method if it's not directly accessible
                var method = mainWindow.GetType().GetMethod("UpdateStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(mainWindow, new object[] { message });
                }
            }
        }
        /// <summary>
        /// These are Text fillers too. We can update later
        ///
        /// </summary>
        private void PopulateSystemInfo()
        {
            try
            {
                // Operating System
                OsInfo.Text = Environment.OSVersion.VersionString;

                // CPU Info
                CpuInfo.Text = GetProcessorInfo();

                // RAM Info
                //RamInfo.Text = GetMemoryInfo();

                // GPU Info - Simulated
                GpuInfo.Text = "NVIDIA GeForce RTX 3080";

                // Storage Info
                StorageInfo.Text = GetStorageInfo();

                // Network Info - Simulated
                NetworkInfo.Text = "Intel Wireless-AC 9560 + Bluetooth 5.0";

                // Computer Name
                ComputerNameInfo.Text = Environment.MachineName;

                // Update metrics
                UpdateMetrics();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error populating system info: {ex.Message}");
            }
        }
    /// <summary>
    /// Actually returning the CPU btw
    /// 
    /// </summary>

        private string GetProcessorInfo()
        {
            try
            {
                string cpuName = Microsoft.Win32.Registry.LocalMachine
                    .OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0")?
                    .GetValue("ProcessorNameString") as string;
                return cpuName ?? "Unknown CPU";
            }
            catch
            {
                return "Unknown CPU";
            }
        }

      /*  private string GetMemoryInfo()
        {
            try
            {
                // Get total physical memory
                long totalMemoryInKB = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024;
                double totalMemoryInGB = totalMemoryInKB / 1024.0 / 1024.0;
                return $"{totalMemoryInGB:F0} GB RAM";
            }
            catch
            {
                return "Unknown Memory Configuration";
            }
        }
      */
        private string GetStorageInfo()
        {
            try
            {
                string result = "";
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        double totalSizeGB = drive.TotalSize / 1024.0 / 1024.0 / 1024.0;
                        result += $"{drive.Name} {totalSizeGB:F0}GB, ";
                    }
                }
                return result.TrimEnd(' ', ',');
            }
            catch
            {
                return "Unknown Storage Configuration";
            }
        }

        private void UpdateMetrics()
        {
            // Update CPU metrics
            int cpuUsage = random.Next(10, 80);
            int cpuTemp = random.Next(35, 75);
            CpuUsageValue.Text = $"{cpuUsage}%";
            CpuTemperature.Text = $"{cpuTemp}°C";

            // Update CPU health indicator based on temperature
            if (cpuTemp > 80)
            {
                CpuHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else if (cpuTemp > 70)
            {
                CpuHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                CpuHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }

            // Update Memory metrics
            int memoryUsage = random.Next(20, 70);
            int availableMemoryGB = random.Next(8, 24);
            MemoryUsageValue.Text = $"{memoryUsage}%";
            MemoryAvailable.Text = $"{availableMemoryGB} GB Free";

            // Update Memory health indicator
            if (memoryUsage > 90)
            {
                MemoryHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else if (memoryUsage > 80)
            {
                MemoryHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                MemoryHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }

            // Update Disk metrics
            int diskUsage = random.Next(60, 90);
            int availableDiskGB = random.Next(50, 250);
            DiskUsageValue.Text = $"{diskUsage}%";
            DiskAvailable.Text = $"{availableDiskGB} GB Free";

            // Update Disk health indicator
            if (diskUsage > 90)
            {
                DiskHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else if (diskUsage > 75)
            {
                DiskHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                DiskHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }

            // Update Network metrics
            int networkSpeed = random.Next(1, 50);
            bool networkConnected = random.Next(0, 10) < 9; // 90% chance of being connected
            NetworkSpeedValue.Text = $"{networkSpeed} MB/s";
            NetworkStatus.Text = networkConnected ? "Connected" : "Disconnected";

            // Update Network health indicator
            if (!networkConnected)
            {
                NetworkHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else if (networkSpeed < 5)
            {
                NetworkHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                NetworkHealthIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Refresh system metrics
            UpdateMetrics();
            ShowStatusMessage("System information refreshed");
        }

        private async void RunScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (isScanning)
                return;

            try
            {
                isScanning = true;
                RunScanButton.Content = "Scanning...";
                RunScanButton.IsEnabled = false;
                ShowStatusMessage("Running full system scan...");

                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("I'm running a full system scan. This might take a minute...");
                }

                // Simulate a scan with delay
                await Task.Delay(3000);

                // Update UI with new health metrics
                UpdateMetrics();
                ShowStatusMessage("System scan completed successfully");

                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("System scan complete! Everything looks good, but you should free up some disk space soon.");
                }
            }
            finally
            {
                RunScanButton.Content = "Run System Scan";
                RunScanButton.IsEnabled = true;
                isScanning = false;
            }
        }
    }
}
