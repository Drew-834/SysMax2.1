using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using SysMax2._1.Models;
using SysMax2._1.Services;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for SystemInfoPage.xaml
    /// </summary>
    public partial class SystemInfoPage : Page
    {
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private readonly EnhancedHardwareMonitorService _hardwareMonitor;
        private DispatcherTimer _updateTimer;
        private MainWindow? mainWindow;

        public class StorageDriveInfo
        {
            public string DriveName { get; set; } = "";
            public string DriveType { get; set; } = "";
            public string UsageText { get; set; } = "";
            public string UsagePercentage { get; set; } = "0%"; // Width percentage for the progress bar
            public SolidColorBrush UsageColor { get; set; } = new SolidColorBrush(Colors.Green);
        }

        public class NetworkInterfaceInfo
        {
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public string AddressInfo { get; set; } = "";
            public bool IsConnected { get; set; }
        }

        public ObservableCollection<StorageDriveInfo> StorageDrives { get; set; } = new ObservableCollection<StorageDriveInfo>();
        public ObservableCollection<NetworkInterfaceInfo> NetworkInterfaces { get; set; } = new ObservableCollection<NetworkInterfaceInfo>();

        public SystemInfoPage()
        {
            InitializeComponent();

            // Get hardware monitor service
            _hardwareMonitor = EnhancedHardwareMonitorService.Instance;

            // Find main window
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Set data sources
            StorageListView.ItemsSource = StorageDrives;
            NetworkListView.ItemsSource = NetworkInterfaces;

            // Start hardware monitoring if not already running
            if (!_hardwareMonitor.IsMonitoring)
            {
                _hardwareMonitor.StartMonitoring();
            }

            // Load system information
            LoadSystemInformation();

            // Set up update timer for dynamic information
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // Log page navigation
            _loggingService.Log(LogLevel.Info, "Navigated to System Information page");
        }

        private void LoadSystemInformation()
        {
            try
            {
                // Show loading status
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Loading system information...");
                }

                // Load basic system information
                LoadBasicSystemInfo();

                // Load hardware information
                LoadHardwareInfo();

                // Load storage information
                LoadStorageInfo();

                // Load network information
                LoadNetworkInfo();

                // Update status
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("System information loaded");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading system information: {ex.Message}");

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Error loading system information");
                    mainWindow.ShowAssistantMessage("There was a problem loading your system information. Some data may not be displayed correctly.");
                }
            }
        }

        private void LoadBasicSystemInfo()
        {
            try
            {
                // Get operating system information
                OSValue.Text = Environment.OSVersion.Platform.ToString();

                // Get OS version
                OSVersionValue.Text = GetWindowsVersionInfo();

                // Get computer name
                ComputerNameValue.Text = Environment.MachineName;

                // Get username
                UsernameValue.Text = Environment.UserName;

                // Get uptime information
                UpdateUptimeInfo();
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading basic system info: {ex.Message}");

                // Set default values if loading fails
                OSValue.Text = "Information Unavailable";
                OSVersionValue.Text = "Information Unavailable";
                ComputerNameValue.Text = "Information Unavailable";
                UsernameValue.Text = "Information Unavailable";
                UptimeValue.Text = "Information Unavailable";
                LastBootValue.Text = "Information Unavailable";
            }
        }

        private string GetWindowsVersionInfo()
        {
            try
            {
                // Try to get more detailed Windows version information from registry
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string? productName = key.GetValue("ProductName") as string;
                        string? releaseId = key.GetValue("ReleaseId") as string;
                        string? buildNumber = key.GetValue("CurrentBuildNumber") as string;
                        string? displayVersion = key.GetValue("DisplayVersion") as string;

                        if (!string.IsNullOrEmpty(productName))
                        {
                            return $"{productName} {displayVersion ?? releaseId ?? ""} (Build {buildNumber ?? Environment.OSVersion.Version.Build.ToString()})";
                        }
                    }
                }

                // Fallback to Environment.OSVersion
                return Environment.OSVersion.VersionString;
            }
            catch
            {
                // If registry access fails, use Environment.OSVersion
                return Environment.OSVersion.VersionString;
            }
        }


        private void UpdateUptimeInfo()
        {
            try
            {
                // Use performance counter to get system uptime
                TimeSpan uptime;
                DateTime bootTime;

                using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                {
                    ManagementObject? mo = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        string? bootTimeStr = mo["LastBootUpTime"] as string;
                        if (!string.IsNullOrEmpty(bootTimeStr))
                        {
                            bootTime = ManagementDateTimeConverter.ToDateTime(bootTimeStr);
                            uptime = DateTime.Now - bootTime;

                            // Update UI
                            UptimeValue.Text = $"{uptime.Days} days, {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
                            LastBootValue.Text = bootTime.ToString("g"); // Short date and time pattern
                            return;
                        }
                    }
                }

                // Fallback method using Environment.TickCount (less accurate for long uptimes)
                int tickCount = Environment.TickCount;
                uptime = TimeSpan.FromMilliseconds(tickCount);
                bootTime = DateTime.Now - uptime;

                UptimeValue.Text = $"{uptime.Days} days, {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
                LastBootValue.Text = bootTime.ToString("g"); // Short date and time pattern
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error getting uptime info: {ex.Message}");

                UptimeValue.Text = "Information Unavailable";
                LastBootValue.Text = "Information Unavailable";
            }
        }

        private void LoadHardwareInfo()
        {
            try
            {
                // Get hardware information from the hardware monitor service
                var systemInfo = _hardwareMonitor.GetSystemInformation();

                // CPU information
                CPUValue.Text = systemInfo.TryGetValue("ProcessorName", out string? cpuName)
                    ? $"{cpuName} ({systemInfo.GetValueOrDefault("CPUCores", "Unknown")} cores)"
                    : "Information Unavailable";

                // RAM information
                RAMValue.Text = systemInfo.TryGetValue("TotalRAM", out string? ramSize)
                    ? ramSize
                    : "Information Unavailable";

                // GPU information
                GPUValue.Text = systemInfo.TryGetValue("GPUName", out string? gpuName)
                    ? gpuName
                    : "Information Unavailable";

                // Storage summary
                StorageValue.Text = systemInfo.TryGetValue("StorageSummary", out string? storageSummary)
                    ? storageSummary
                    : "Information Unavailable";
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading hardware info: {ex.Message}");

                // Set default values if loading fails
                CPUValue.Text = "Information Unavailable";
                RAMValue.Text = "Information Unavailable";
                GPUValue.Text = "Information Unavailable";
                StorageValue.Text = "Information Unavailable";
            }
        }

        private void LoadStorageInfo()
        {
            try
            {
                // Clear existing storage drives
                StorageDrives.Clear();

                // Get all drives
                DriveInfo[] drives = DriveInfo.GetDrives();

                foreach (DriveInfo drive in drives)
                {
                    // Skip drives that aren't ready
                    if (!drive.IsReady)
                        continue;

                    // Calculate usage percentage
                    double usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                    double usagePercentage = (usedSpace / drive.TotalSize) * 100;

                    // Determine usage color based on percentage
                    Color usageColor;
                    if (usagePercentage > 90)
                        usageColor = (Color)ColorConverter.ConvertFromString("#e74c3c"); // Red
                    else if (usagePercentage > 75)
                        usageColor = (Color)ColorConverter.ConvertFromString("#f39c12"); // Orange
                    else
                        usageColor = (Color)ColorConverter.ConvertFromString("#2ecc71"); // Green

                    // Create storage drive info
                    StorageDriveInfo driveInfo = new StorageDriveInfo
                    {
                        DriveName = $"{drive.Name} {(string.IsNullOrEmpty(drive.VolumeLabel) ? "" : $"({drive.VolumeLabel})")}",
                        DriveType = drive.DriveType.ToString(),
                        UsageText = $"{drive.AvailableFreeSpace / (1024.0 * 1024 * 1024):F1} GB free of {drive.TotalSize / (1024.0 * 1024 * 1024):F1} GB",
                        UsagePercentage = $"{usagePercentage:F0}%",
                        UsageColor = new SolidColorBrush(usageColor)
                    };

                    // Add to collection
                    StorageDrives.Add(driveInfo);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading storage info: {ex.Message}");
            }
        }

        private void LoadNetworkInfo()
        {
            try
            {
                // Clear existing network interfaces
                NetworkInterfaces.Clear();

                // Get all network interfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

                foreach (NetworkInterface ni in interfaces)
                {
                    // Skip loopback, tunnel adapters, or non-operational interfaces
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.Description.ToLower().Contains("tunnel") ||
                        ni.OperationalStatus == OperationalStatus.Unknown)
                        continue;

                    // Get IP addresses
                    string addressInfo = "No IP Address";
                    try
                    {
                        IPInterfaceProperties ipProps = ni.GetIPProperties();

                        var ipv4Addresses = ipProps.UnicastAddresses
                            .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .Select(addr => addr.Address.ToString());

                        if (ipv4Addresses.Any())
                        {
                            addressInfo = string.Join(", ", ipv4Addresses);
                        }
                    }
                    catch
                    {
                        // If we can't get IP addresses, just show connection status
                        addressInfo = $"Status: {ni.OperationalStatus}";
                    }

                    // Create network interface info
                    NetworkInterfaceInfo nicInfo = new NetworkInterfaceInfo
                    {
                        Name = ni.Name,
                        Description = ni.Description,
                        AddressInfo = addressInfo,
                        IsConnected = ni.OperationalStatus == OperationalStatus.Up
                    };

                    // Add to collection
                    NetworkInterfaces.Add(nicInfo);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error loading network info: {ex.Message}");
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Update dynamic information
            UpdateUptimeInfo();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Log the action
            _loggingService.Log(LogLevel.Info, "User refreshed system information");

            // Reload system information
            LoadSystemInformation();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create save file dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|HTML files (*.html)|*.html|All files (*.*)|*.*",
                    DefaultExt = ".txt",
                    Title = "Export System Information"
                };

                // Show save file dialog
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Determine export format based on file extension
                    string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();

                    if (extension == ".html")
                    {
                        ExportSystemInfoAsHtml(saveFileDialog.FileName);
                    }
                    else
                    {
                        ExportSystemInfoAsText(saveFileDialog.FileName);
                    }

                    // Show success message
                    if (mainWindow != null)
                    {
                        mainWindow.UpdateStatus($"System information exported to {Path.GetFileName(saveFileDialog.FileName)}");
                        mainWindow.ShowAssistantMessage($"Successfully exported system information to {Path.GetFileName(saveFileDialog.FileName)}");
                    }

                    // Log the action
                    _loggingService.Log(LogLevel.Info, $"User exported system information to {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error exporting system information: {ex.Message}");

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Error exporting system information");
                    mainWindow.ShowAssistantMessage($"There was a problem exporting the system information: {ex.Message}");
                }
            }
        }

        private void ExportSystemInfoAsText(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("SYSTEM INFORMATION REPORT");
                writer.WriteLine($"Generated on: {DateTime.Now:F}");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine();

                // System Overview
                writer.WriteLine("SYSTEM OVERVIEW");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine($"OS:             {OSValue.Text}");
                writer.WriteLine($"Version:        {OSVersionValue.Text}");
                writer.WriteLine($"Computer Name:  {ComputerNameValue.Text}");
                writer.WriteLine($"Username:       {UsernameValue.Text}");
                writer.WriteLine($"System Uptime:  {UptimeValue.Text}");
                writer.WriteLine($"Last Boot:      {LastBootValue.Text}");
                writer.WriteLine();

                // Hardware
                writer.WriteLine("HARDWARE");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine($"Processor:      {CPUValue.Text}");
                writer.WriteLine($"Memory:         {RAMValue.Text}");
                writer.WriteLine($"Graphics:       {GPUValue.Text}");
                writer.WriteLine($"Storage:        {StorageValue.Text}");
                writer.WriteLine();

                // Storage Drives
                writer.WriteLine("STORAGE DRIVES");
                writer.WriteLine(new string('-', 80));
                foreach (var drive in StorageDrives)
                {
                    writer.WriteLine($"{drive.DriveName} ({drive.DriveType})");
                    writer.WriteLine($"    {drive.UsageText}");
                    writer.WriteLine();
                }

                // Network Interfaces
                writer.WriteLine("NETWORK INTERFACES");
                writer.WriteLine(new string('-', 80));
                foreach (var iface in NetworkInterfaces)
                {
                    writer.WriteLine($"{iface.Name} - {(iface.IsConnected ? "Connected" : "Disconnected")}");
                    writer.WriteLine($"    {iface.Description}");
                    writer.WriteLine($"    {iface.AddressInfo}");
                    writer.WriteLine();
                }

                // System metrics
                writer.WriteLine("CURRENT SYSTEM METRICS");
                writer.WriteLine(new string('-', 80));
                writer.WriteLine($"CPU Usage:      {_hardwareMonitor.CpuUsage:F1}%");
                writer.WriteLine($"CPU Temperature: {_hardwareMonitor.CpuTemperature:F1}°C");
                writer.WriteLine($"Memory Usage:   {_hardwareMonitor.MemoryUsage:F1}%");
                writer.WriteLine($"Disk Usage:     {_hardwareMonitor.DiskUsage:F1}%");
                writer.WriteLine();

                // Footer
                writer.WriteLine(new string('-', 80));
                writer.WriteLine("Generated by SysMax System Health Monitor");
            }
        }

        private void ExportSystemInfoAsHtml(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine("<title>System Information Report</title>");
                writer.WriteLine("<style>");
                writer.WriteLine("body { font-family: Arial, sans-serif; margin: 40px; color: #333; }");
                writer.WriteLine("h1 { color: #3498db; }");
                writer.WriteLine("h2 { color: #2980b9; margin-top: 30px; border-bottom: 1px solid #eee; padding-bottom: 5px; }");
                writer.WriteLine(".timestamp { color: #888; font-style: italic; margin-bottom: 30px; }");
                writer.WriteLine("table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }");
                writer.WriteLine("th, td { text-align: left; padding: 8px; border-bottom: 1px solid #ddd; }");
                writer.WriteLine("th { background-color: #f2f2f2; }");
                writer.WriteLine(".connected { color: #2ecc71; font-weight: bold; }");
                writer.WriteLine(".disconnected { color: #e74c3c; font-weight: bold; }");
                writer.WriteLine(".footer { margin-top: 50px; text-align: center; color: #888; font-size: 12px; }");
                writer.WriteLine("</style>");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");

                writer.WriteLine("<h1>System Information Report</h1>");
                writer.WriteLine($"<p class=\"timestamp\">Generated on: {DateTime.Now:F}</p>");

                // System Overview
                writer.WriteLine("<h2>System Overview</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Property</th><th>Value</th></tr>");
                writer.WriteLine($"<tr><td>OS</td><td>{OSValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Version</td><td>{OSVersionValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Computer Name</td><td>{ComputerNameValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Username</td><td>{UsernameValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>System Uptime</td><td>{UptimeValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Last Boot</td><td>{LastBootValue.Text}</td></tr>");
                writer.WriteLine("</table>");

                // Hardware
                writer.WriteLine("<h2>Hardware</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Component</th><th>Details</th></tr>");
                writer.WriteLine($"<tr><td>Processor</td><td>{CPUValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Memory</td><td>{RAMValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Graphics</td><td>{GPUValue.Text}</td></tr>");
                writer.WriteLine($"<tr><td>Storage</td><td>{StorageValue.Text}</td></tr>");
                writer.WriteLine("</table>");

                // Storage Drives
                writer.WriteLine("<h2>Storage Drives</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Drive</th><th>Type</th><th>Usage</th></tr>");
                foreach (var drive in StorageDrives)
                {
                    writer.WriteLine($"<tr><td>{drive.DriveName}</td><td>{drive.DriveType}</td><td>{drive.UsageText}</td></tr>");
                }
                writer.WriteLine("</table>");

                // Network Interfaces
                writer.WriteLine("<h2>Network Interfaces</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Name</th><th>Description</th><th>Status</th><th>Address Info</th></tr>");
                foreach (var iface in NetworkInterfaces)
                {
                    writer.WriteLine($"<tr><td>{iface.Name}</td><td>{iface.Description}</td>" +
                                    $"<td class=\"{(iface.IsConnected ? "connected" : "disconnected")}\">{(iface.IsConnected ? "Connected" : "Disconnected")}</td>" +
                                    $"<td>{iface.AddressInfo}</td></tr>");
                }
                writer.WriteLine("</table>");

                // System metrics
                writer.WriteLine("<h2>Current System Metrics</h2>");
                writer.WriteLine("<table>");
                writer.WriteLine("<tr><th>Metric</th><th>Value</th></tr>");
                writer.WriteLine($"<tr><td>CPU Usage</td><td>{_hardwareMonitor.CpuUsage:F1}%</td></tr>");
                writer.WriteLine($"<tr><td>CPU Temperature</td><td>{_hardwareMonitor.CpuTemperature:F1}°C</td></tr>");
                writer.WriteLine($"<tr><td>Memory Usage</td><td>{_hardwareMonitor.MemoryUsage:F1}%</td></tr>");
                writer.WriteLine($"<tr><td>Disk Usage</td><td>{_hardwareMonitor.DiskUsage:F1}%</td></tr>");
                writer.WriteLine("</table>");

                // Footer
                writer.WriteLine("<div class=\"footer\">Generated by SysMax System Health Monitor</div>");

                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Stop the update timer
            _updateTimer?.Stop();
        }
    }
}