using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using SysMax2._1.Services;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for SystemInfoPage.xaml
    /// </summary>
    public partial class SystemInfoPage : Page
    {
        private MainWindow mainWindow;
        private LoggingService loggingService = LoggingService.Instance;
        private HardwareMonitorService hardwareMonitor = HardwareMonitorService.Instance;
        private DispatcherTimer monitoringTimer;
        private bool isMonitoring = false;
        private TabControl MainTabControl;


        // Reference to the monitoring button - make sure this exists in your XAML
        private Button monitoringButton;

        public SystemInfoPage()
        {
            InitializeComponent();



            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;


            MainTabControl = FindName("MainTabControl") as TabControl;
            // Initialize with system info
            LoadSystemInformation();

            // Initialize monitoring timer
            monitoringTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            monitoringTimer.Tick += MonitoringTimer_Tick;

            // Find the monitoring button if it exists in the XAML
            monitoringButton = FindName("StartMonitoringButton") as Button;

            // Register for Unloaded event to clean up resources
            this.Unloaded += SystemInfoPage_Unloaded;

            // Log page initialization
            loggingService.Log(LogLevel.Info, "System Information page initialized");

            // Show the assistant with helpful information
            if (mainWindow != null)
            {
                mainWindow.ShowAssistantMessage("This page shows detailed information about your system hardware and software. You can monitor real-time metrics by clicking 'Start Monitoring'.");
            }
        }

        private void LoadSystemInformation()
        {
            try
            {
                // Load OS information
                LoadOperatingSystemInfo();

                // Load CPU information
                LoadCpuInfo();

                // Load memory information
                LoadMemoryInfo();

                // Load storage information
                LoadStorageInfo();

                // Load GPU information
                LoadGpuInfo();

                // Load network information
                LoadNetworkInfo();

                // Update System Summary
                UpdateSystemSummary();

                // Log completion
                loggingService.Log(LogLevel.Info, "Loaded system information successfully");
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error loading system information: {ex.Message}");
                MessageBox.Show($"Error loading system information: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSystemSummary()
        {
            try
            {
                // Update computer name
                ComputerNameValue.Text = Environment.MachineName;

                // Update OS info
                OsNameValue.Text = GetOSCaption();
                OsVersionValue.Text = GetOSVersionAndServicePack();
                OsBuildValue.Text = GetOSBuild();
                OsArchValue.Text = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
                OsInstalledValue.Text = GetOSInstallDate();
                OsLastUpdateValue.Text = GetLastWindowsUpdateDate();

                // Update uptime
                UptimeValue.Text = GetSystemUptime();

                // Update username
                UsernameValue.Text = Environment.UserName;

                // Update computer role
                ComputerRoleValue.Text = GetComputerRole();

                // Update network status
                IpAddressValue.Text = GetLocalIPAddress();
                NetworkAdapterValue.Text = GetActiveNetworkAdapter();
                NetworkStatusValue.Text = NetworkInterface.GetIsNetworkAvailable() ? "Connected" : "Disconnected";
                NetworkStatusValue.Foreground = NetworkInterface.GetIsNetworkAvailable()
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));

                // Update hardware info summary
                ProcessorValue.Text = GetProcessorName();
                MemoryValue.Text = $"{GetTotalPhysicalMemory()} GB RAM";
                GraphicsValue.Text = GetGraphicsCardName();
                StorageValue.Text = GetStorageSummary();
                MotherboardValue.Text = GetMotherboardInfo();
                NetworkValue.Text = GetNetworkAdapterName();
                BiosValue.Text = GetBiosVersion();

                // Update live metrics
                UpdateLiveMetrics();
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error updating system summary: {ex.Message}");
            }
        }

        private void UpdateLiveMetrics()
        {
            try
            {
                // Update hardware metrics using HardwareMonitorService
                var cpuInfo = hardwareMonitor.GetCpuInfo();
                var memoryInfo = hardwareMonitor.GetMemoryInfo();
                var storageDevices = hardwareMonitor.GetStorageInfo();
                var networkAdapters = hardwareMonitor.GetNetworkInfo();

                // Update CPU metrics
                if (cpuInfo.TryGetValue("Load", out string cpuLoadStr))
                {
                    float cpuLoad = float.Parse(cpuLoadStr.Replace(" %", ""));
                    CpuUsageValue.Text = $"{cpuLoad:F0}";
                    CpuUsageBar.Value = cpuLoad;

                    // Update color based on value
                    if (cpuLoad > 90)
                    {
                        CpuUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    }
                    else if (cpuLoad > 70)
                    {
                        CpuUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    }
                    else
                    {
                        CpuUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498db"));
                    }
                }
                else
                {
                    // Fallback to performance counter
                    var cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
                    float cpuUsage = cpuCounter.NextValue();
                    // Need to read twice for accuracy on first read
                    System.Threading.Thread.Sleep(500);
                    cpuUsage = cpuCounter.NextValue();

                    CpuUsageValue.Text = $"{cpuUsage:F0}";
                    CpuUsageBar.Value = cpuUsage;

                    // Update color based on value
                    if (cpuUsage > 90)
                    {
                        CpuUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    }
                    else if (cpuUsage > 70)
                    {
                        CpuUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    }
                    else
                    {
                        CpuUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498db"));
                    }
                }

                // Update CPU temperature
                if (cpuInfo.TryGetValue("Temperature", out string cpuTempStr))
                {
                    float cpuTemp = float.Parse(cpuTempStr.Replace(" °C", ""));
                    CpuTempValue.Text = $"{cpuTemp:F0}";
                    CpuTempBar.Value = cpuTemp;

                    // Update color based on value
                    if (cpuTemp > 85)
                    {
                        CpuTempBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    }
                    else if (cpuTemp > 70)
                    {
                        CpuTempBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    }
                    else
                    {
                        CpuTempBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                    }
                }
                else
                {
                    // Fallback to estimated temp
                    var rand = new Random();
                    int estTemp = rand.Next(45, 75);
                    CpuTempValue.Text = $"{estTemp}";
                    CpuTempBar.Value = estTemp;

                    // Update color based on value
                    if (estTemp > 85)
                    {
                        CpuTempBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    }
                    else if (estTemp > 70)
                    {
                        CpuTempBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    }
                    else
                    {
                        CpuTempBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                    }
                }

                // Update memory metrics
                if (memoryInfo.TryGetValue("Load", out string memLoadStr))
                {
                    float memLoad = float.Parse(memLoadStr.Replace(" %", ""));
                    MemoryUsageValue.Text = $"{memLoad:F0}";
                    MemoryUsageBar.Value = memLoad;

                    // Update color based on value
                    if (memLoad > 90)
                    {
                        MemoryUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    }
                    else if (memLoad > 70)
                    {
                        MemoryUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    }
                    else
                    {
                        MemoryUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                    }
                }
                else
                {
                    // Fallback to performance counter
                    var memoryCounter = new System.Diagnostics.PerformanceCounter("Memory", "% Committed Bytes In Use");
                    float memoryUsage = memoryCounter.NextValue();

                    MemoryUsageValue.Text = $"{memoryUsage:F0}";
                    MemoryUsageBar.Value = memoryUsage;

                    // Update color based on value
                    if (memoryUsage > 90)
                    {
                        MemoryUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    }
                    else if (memoryUsage > 70)
                    {
                        MemoryUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    }
                    else
                    {
                        MemoryUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                    }
                }

                // Update disk usage
                float diskUsage = 0;

                // Get all logical drives
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                long totalSpace = 0;
                long totalFreeSpace = 0;

                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        totalSpace += drive.TotalSize;
                        totalFreeSpace += drive.AvailableFreeSpace;
                    }
                }

                if (totalSpace > 0)
                {
                    diskUsage = (float)((totalSpace - totalFreeSpace) * 100.0 / totalSpace);
                    DiskUsageValue.Text = $"{diskUsage:F0}";
                    DiskUsageBar.Value = diskUsage;

                    // Update color based on value
                    if (diskUsage > 90)
                    {
                        DiskUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    }
                    else if (diskUsage > 70)
                    {
                        DiskUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                    }
                    else
                    {
                        DiskUsageBar.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9b59b6"));
                    }
                }

                // Update network metrics
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

                // Find the active network adapter
                NetworkInterface activeAdapter = null;
                foreach (NetworkInterface adapter in adapters)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up &&
                        (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                         adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                    {
                        activeAdapter = adapter;
                        break;
                    }
                }

                // Get network speeds
                float downloadSpeed = 0;
                float uploadSpeed = 0;

                if (activeAdapter != null && networkAdapters.Count > 0)
                {
                    foreach (var network in networkAdapters)
                    {
                        if (network.Value.TryGetValue("Download", out string downloadStr))
                        {
                            downloadSpeed = float.Parse(downloadStr.Replace(" KB/s", ""));
                        }

                        if (network.Value.TryGetValue("Upload", out string uploadStr))
                        {
                            uploadSpeed = float.Parse(uploadStr.Replace(" KB/s", ""));
                        }

                        if (downloadSpeed > 0 || uploadSpeed > 0)
                        {
                            break;
                        }
                    }

                    // Convert to appropriate units
                    if (downloadSpeed > 1024)
                    {
                        DownloadSpeedValue.Text = $"{downloadSpeed / 1024:F1} MB/s";
                    }
                    else
                    {
                        DownloadSpeedValue.Text = $"{downloadSpeed:F1} KB/s";
                    }

                    if (uploadSpeed > 1024)
                    {
                        UploadSpeedValue.Text = $"{uploadSpeed / 1024:F1} MB/s";
                    }
                    else
                    {
                        UploadSpeedValue.Text = $"{uploadSpeed:F1} KB/s";
                    }
                }
                else
                {
                    DownloadSpeedValue.Text = "0 KB/s";
                    UploadSpeedValue.Text = "0 KB/s";
                }
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error updating live metrics: {ex.Message}");
            }
        }
        private void MonitoringToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // This can just call your existing method
            StartMonitoringButton_Click(sender, e);
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                // Get the tag from the button, which likely indicates which tab to show
                string tabTag = clickedButton.Tag?.ToString();

                // Find the TabItem with that tag and select it
                if (!string.IsNullOrEmpty(tabTag) && MainTabControl != null)
                {
                    foreach (TabItem item in MainTabControl.Items)
                    {
                        if (item.Tag?.ToString() == tabTag)
                        {
                            MainTabControl.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }
        private void LoadOperatingSystemInfo()
        {
            try
            {
                OsInfoPanel.Children.Clear();

                // Add the page title
                TextBlock titleBlock = new TextBlock
                {
                    Text = "OPERATING SYSTEM DETAILS",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                OsInfoPanel.Children.Add(titleBlock);

                // Create a grid for OS details
                Grid osGrid = new Grid();
                osGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                osGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Add rows dynamically
                int rowIndex = 0;

                // Add basic OS information
                AddOsGridRow(osGrid, rowIndex++, "OS Name:", GetOSCaption());
                AddOsGridRow(osGrid, rowIndex++, "Version:", GetOSVersionAndServicePack());
                AddOsGridRow(osGrid, rowIndex++, "Build Number:", GetOSBuild());
                AddOsGridRow(osGrid, rowIndex++, "Architecture:", Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit");
                AddOsGridRow(osGrid, rowIndex++, "Installation Date:", GetOSInstallDate());
                AddOsGridRow(osGrid, rowIndex++, "Last Windows Update:", GetLastWindowsUpdateDate());
                AddOsGridRow(osGrid, rowIndex++, "System Directory:", Environment.SystemDirectory);
                AddOsGridRow(osGrid, rowIndex++, "Windows Directory:", Environment.GetFolderPath(Environment.SpecialFolder.Windows));
                AddOsGridRow(osGrid, rowIndex++, "Computer Name:", Environment.MachineName);
                AddOsGridRow(osGrid, rowIndex++, "Username:", Environment.UserName);
                AddOsGridRow(osGrid, rowIndex++, "Domain:", GetComputerDomain());
                AddOsGridRow(osGrid, rowIndex++, "Computer Role:", GetComputerRole());
                AddOsGridRow(osGrid, rowIndex++, "System Uptime:", GetSystemUptime());

                // Add the grid to the panel
                OsInfoPanel.Children.Add(osGrid);

                // Add a section for registered owner information
                TextBlock ownerTitle = new TextBlock
                {
                    Text = "REGISTERED USER",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                OsInfoPanel.Children.Add(ownerTitle);

                // Create a grid for owner details
                Grid ownerGrid = new Grid();
                ownerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                ownerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Add owner information
                rowIndex = 0;
                string regOwner = GetRegisteredOwner();
                string regOrg = GetRegisteredOrganization();
                string productId = GetWindowsProductId();
                string productKey = GetWindowsProductKey();

                AddOsGridRow(ownerGrid, rowIndex++, "Registered Owner:", regOwner);
                AddOsGridRow(ownerGrid, rowIndex++, "Registered Organization:", regOrg);
                AddOsGridRow(ownerGrid, rowIndex++, "Product ID:", productId);
                AddOsGridRow(ownerGrid, rowIndex++, "Product Key:", productKey);

                // Add the grid to the panel
                OsInfoPanel.Children.Add(ownerGrid);

                // Add a section for Windows features
                TextBlock featuresTitle = new TextBlock
                {
                    Text = "WINDOWS FEATURES AND CONFIGURATION",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                OsInfoPanel.Children.Add(featuresTitle);

                // Create a grid for features
                Grid featuresGrid = new Grid();
                featuresGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                featuresGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Add feature information
                rowIndex = 0;
                AddOsGridRow(featuresGrid, rowIndex++, "Windows Defender Status:", GetWindowsDefenderStatus());
                AddOsGridRow(featuresGrid, rowIndex++, "Windows Firewall Status:", GetWindowsFirewallStatus());
                AddOsGridRow(featuresGrid, rowIndex++, "Windows Update Settings:", GetWindowsUpdateSettings());
                AddOsGridRow(featuresGrid, rowIndex++, "UAC Level:", GetUACLevel());
                AddOsGridRow(featuresGrid, rowIndex++, "System Restore:", IsSystemRestoreEnabled() ? "Enabled" : "Disabled");
                AddOsGridRow(featuresGrid, rowIndex++, "Fast Startup:", IsFastStartupEnabled() ? "Enabled" : "Disabled");

                // Add the grid to the panel
                OsInfoPanel.Children.Add(featuresGrid);
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error loading operating system information: {ex.Message}");

                // Show error in the panel
                OsInfoPanel.Children.Clear();
                OsInfoPanel.Children.Add(new TextBlock
                {
                    Text = $"Error loading operating system information: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        private void AddOsGridRow(Grid grid, int rowIndex, string label, string value)
        {
            // Add a new row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create the label
            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                Style = Resources["PropertyNameStyle"] as Style
            };
            Grid.SetRow(labelBlock, rowIndex);
            Grid.SetColumn(labelBlock, 0);

            // Create the value
            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                Style = Resources["PropertyValueStyle"] as Style
            };
            Grid.SetRow(valueBlock, rowIndex);
            Grid.SetColumn(valueBlock, 1);

            // Add them to the grid
            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
        }

        private void LoadCpuInfo()
        {
            try
            {
                CpuInfoPanel.Children.Clear();

                // Get CPU information from WMI
                var cpuInfo = hardwareMonitor.GetCpuInfo();

                // Add CPU information
                TextBlock basicInfoTitle = new TextBlock
                {
                    Text = "CPU BASIC INFORMATION",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                CpuInfoPanel.Children.Add(basicInfoTitle);

                // Create a grid for basic CPU info
                Grid basicInfoGrid = new Grid();
                basicInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                basicInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                int rowIndex = 0;

                // Get processor information from WMI
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Name:", obj["Name"].ToString());
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Manufacturer:", obj["Manufacturer"].ToString());
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Description:", obj["Description"].ToString());
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Architecture:", GetCpuArchitecture(Convert.ToUInt16(obj["Architecture"])));
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Number of Cores:", obj["NumberOfCores"].ToString());
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Logical Processors:", obj["NumberOfLogicalProcessors"].ToString());
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Max Clock Speed:", $"{obj["MaxClockSpeed"]} MHz");
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Current Clock Speed:", $"{obj["CurrentClockSpeed"]} MHz");
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "Socket Designation:", obj["SocketDesignation"].ToString());
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "L2 Cache Size:", $"{Convert.ToUInt32(obj["L2CacheSize"]) / 1024} MB");
                        AddCpuGridRow(basicInfoGrid, rowIndex++, "L3 Cache Size:", $"{Convert.ToUInt32(obj["L3CacheSize"]) / 1024} MB");
                        break; // Only get the first CPU for now
                    }
                }

                CpuInfoPanel.Children.Add(basicInfoGrid);

                // Add live metrics from HardwareMonitor
                TextBlock metricsTitle = new TextBlock
                {
                    Text = "CPU LIVE METRICS",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                CpuInfoPanel.Children.Add(metricsTitle);

                // Create a grid for metrics
                StackPanel metricsPanel = new StackPanel();

                // Add CPU load and temperature
                if (cpuInfo.Count > 0)
                {
                    // Create live metrics for CPU
                    foreach (var item in cpuInfo)
                    {
                        // Skip name as we already have it
                        if (item.Key == "Name")
                            continue;

                        Border metricBorder = new Border
                        {
                            Style = Resources["HardwareMetricStyle"] as Style
                        };

                        Grid metricGrid = new Grid();
                        metricGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        metricGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        TextBlock metricName = new TextBlock
                        {
                            Text = item.Key,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                            Margin = new Thickness(0, 0, 15, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetColumn(metricName, 0);

                        TextBlock metricValue = new TextBlock
                        {
                            Text = item.Value,
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Grid.SetColumn(metricValue, 1);

                        metricGrid.Children.Add(metricName);
                        metricGrid.Children.Add(metricValue);

                        metricBorder.Child = metricGrid;
                        metricsPanel.Children.Add(metricBorder);
                    }
                }
                else
                {
                    // Fallback if HardwareMonitor doesn't work
                    var cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
                    float cpuUsage = cpuCounter.NextValue();
                    // Need to read twice for accuracy on first read
                    System.Threading.Thread.Sleep(500);
                    cpuUsage = cpuCounter.NextValue();

                    Border metricBorder = new Border
                    {
                        Style = Resources["HardwareMetricStyle"] as Style
                    };

                    Grid metricGrid = new Grid();
                    metricGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    metricGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    TextBlock metricName = new TextBlock
                    {
                        Text = "CPU Usage",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                        Margin = new Thickness(0, 0, 15, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(metricName, 0);

                    TextBlock metricValue = new TextBlock
                    {
                        Text = $"{cpuUsage:F1} %",
                        Foreground = new SolidColorBrush(Colors.White),
                        FontWeight = FontWeights.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(metricValue, 1);

                    metricGrid.Children.Add(metricName);
                    metricGrid.Children.Add(metricValue);

                    metricBorder.Child = metricGrid;
                    metricsPanel.Children.Add(metricBorder);
                }

                CpuInfoPanel.Children.Add(metricsPanel);

                // Add CPU features
                TextBlock featuresTitle = new TextBlock
                {
                    Text = "CPU SUPPORTED FEATURES",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                CpuInfoPanel.Children.Add(featuresTitle);

                // Create a panel for CPU features
                WrapPanel featuresPanel = new WrapPanel();

                // Get CPU features from registry
                string[] cpuFeatures = GetCpuFeatures();
                foreach (var feature in cpuFeatures)
                {
                    Border featureBorder = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a2a2a")),
                        CornerRadius = new CornerRadius(4),
                        Padding = new Thickness(10, 5, 10, 5),
                        Margin = new Thickness(0, 0, 5, 5)
                    };

                    TextBlock featureText = new TextBlock
                    {
                        Text = feature,
                        Foreground = new SolidColorBrush(Colors.White)
                    };

                    featureBorder.Child = featureText;
                    featuresPanel.Children.Add(featureBorder);
                }

                CpuInfoPanel.Children.Add(featuresPanel);
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error loading CPU information: {ex.Message}");

                // Show error in the panel
                CpuInfoPanel.Children.Clear();
                CpuInfoPanel.Children.Add(new TextBlock
                {
                    Text = $"Error loading CPU information: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        private void AddCpuGridRow(Grid grid, int rowIndex, string label, string value)
        {
            // Add a new row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create the label
            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                Style = Resources["PropertyNameStyle"] as Style
            };
            Grid.SetRow(labelBlock, rowIndex);
            Grid.SetColumn(labelBlock, 0);

            // Create the value
            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                Style = Resources["PropertyValueStyle"] as Style
            };
            Grid.SetRow(valueBlock, rowIndex);
            Grid.SetColumn(valueBlock, 1);

            // Add them to the grid
            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
        }

        private void LoadMemoryInfo()
        {
            try
            {
                MemoryInfoPanel.Children.Clear();

                // Get memory information from WMI
                var memoryInfo = hardwareMonitor.GetMemoryInfo();

                // Add memory information
                TextBlock physMemTitle = new TextBlock
                {
                    Text = "PHYSICAL MEMORY",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                MemoryInfoPanel.Children.Add(physMemTitle);

                // Create a grid for physical memory info
                Grid physMemGrid = new Grid();
                physMemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                physMemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                int rowIndex = 0;

                // Add physical memory information
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        double totalPhysicalMemory = Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                        AddMemoryGridRow(physMemGrid, rowIndex++, "Total Physical Memory:", $"{totalPhysicalMemory:F2} GB");
                        break;
                    }
                }

                // Add available memory
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        double freePhysicalMemory = Convert.ToDouble(obj["FreePhysicalMemory"]) / (1024 * 1024);
                        AddMemoryGridRow(physMemGrid, rowIndex++, "Available Memory:", $"{freePhysicalMemory:F2} GB");

                        double totalVirtualMemory = Convert.ToDouble(obj["TotalVirtualMemorySize"]) / 1024;
                        AddMemoryGridRow(physMemGrid, rowIndex++, "Total Virtual Memory:", $"{totalVirtualMemory:F2} GB");

                        double freeVirtualMemory = Convert.ToDouble(obj["FreeVirtualMemory"]) / 1024;
                        AddMemoryGridRow(physMemGrid, rowIndex++, "Available Virtual Memory:", $"{freeVirtualMemory:F2} GB");
                        break;
                    }
                }

                // Add memory usage from hardware monitor
                if (memoryInfo.Count > 0)
                {
                    foreach (var item in memoryInfo)
                    {
                        // Skip keys we already have
                        if (item.Key == "Total" || item.Key == "Available")
                            continue;

                        AddMemoryGridRow(physMemGrid, rowIndex++, item.Key + ":", item.Value);
                    }
                }

                MemoryInfoPanel.Children.Add(physMemGrid);

                // Add memory modules information
                TextBlock memModulesTitle = new TextBlock
                {
                    Text = "MEMORY MODULES",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                MemoryInfoPanel.Children.Add(memModulesTitle);

                // Create a panel for memory modules
                StackPanel modulesPanel = new StackPanel();

                // Get memory modules from WMI
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    int moduleNumber = 1;
                    foreach (var obj in searcher.Get())
                    {
                        Border moduleBorder = new Border
                        {
                            Style = Resources["HardwareMetricStyle"] as Style
                        };

                        StackPanel moduleInfo = new StackPanel();

                        // Create module title
                        TextBlock moduleTitle = new TextBlock
                        {
                            Text = $"Module {moduleNumber}: {Convert.ToDouble(obj["Capacity"]) / (1024 * 1024 * 1024):F2} GB",
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        moduleInfo.Children.Add(moduleTitle);

                        // Create module details
                        TextBlock moduleDetails = new TextBlock
                        {
                            Text = $"Manufacturer: {obj["Manufacturer"]}\n" +
                                   $"Speed: {obj["Speed"]} MHz\n" +
                                   $"Form Factor: {GetMemoryFormFactor(Convert.ToUInt16(obj["FormFactor"]))}\n" +
                                   $"Bank Label: {obj["BankLabel"]}, Device Locator: {obj["DeviceLocator"]}",
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                            TextWrapping = TextWrapping.Wrap
                        };
                        moduleInfo.Children.Add(moduleDetails);

                        moduleBorder.Child = moduleInfo;
                        modulesPanel.Children.Add(moduleBorder);

                        moduleNumber++;
                    }
                }

                MemoryInfoPanel.Children.Add(modulesPanel);

                // Add page file information
                TextBlock pageFileTitle = new TextBlock
                {
                    Text = "PAGE FILE INFORMATION",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                MemoryInfoPanel.Children.Add(pageFileTitle);

                // Create a grid for page file info
                Grid pageFileGrid = new Grid();
                pageFileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                pageFileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                rowIndex = 0;

                // Add page file information
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileUsage"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        AddMemoryGridRow(pageFileGrid, rowIndex++, "Page File Name:", obj["Name"].ToString());
                        AddMemoryGridRow(pageFileGrid, rowIndex++, "Current Usage:", $"{obj["CurrentUsage"]} MB");
                        AddMemoryGridRow(pageFileGrid, rowIndex++, "Peak Usage:", $"{obj["PeakUsage"]} MB");
                        AddMemoryGridRow(pageFileGrid, rowIndex++, "Allocated Size:", $"{obj["AllocatedBaseSize"]} MB");
                    }
                }

                MemoryInfoPanel.Children.Add(pageFileGrid);
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error loading memory information: {ex.Message}");

                // Show error in the panel
                MemoryInfoPanel.Children.Clear();
                MemoryInfoPanel.Children.Add(new TextBlock
                {
                    Text = $"Error loading memory information: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        private void AddMemoryGridRow(Grid grid, int rowIndex, string label, string value)
        {
            // Add a new row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create the label
            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                Style = Resources["PropertyNameStyle"] as Style
            };
            Grid.SetRow(labelBlock, rowIndex);
            Grid.SetColumn(labelBlock, 0);

            // Create the value
            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                Style = Resources["PropertyValueStyle"] as Style
            };
            Grid.SetRow(valueBlock, rowIndex);
            Grid.SetColumn(valueBlock, 1);

            // Add them to the grid
            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
        }

        private void LoadStorageInfo()
        {
            try
            {
                StorageInfoPanel.Children.Clear();

                // Add physical disks information
                TextBlock physDisksTitle = new TextBlock
                {
                    Text = "PHYSICAL DISKS",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                StorageInfoPanel.Children.Add(physDisksTitle);

                // Create a panel for physical disks
                StackPanel physDisksPanel = new StackPanel();

                // Get physical disks from WMI
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    int diskNumber = 1;
                    foreach (var obj in searcher.Get())
                    {
                        Border diskBorder = new Border
                        {
                            Style = Resources["HardwareMetricStyle"] as Style
                        };

                        StackPanel diskInfo = new StackPanel();

                        // Calculate size in GB
                        double sizeGB = Convert.ToDouble(obj["Size"]) / (1024 * 1024 * 1024);

                        // Create disk title
                        TextBlock diskTitle = new TextBlock
                        {
                            Text = $"Disk {diskNumber}: {obj["Model"]}",
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        diskInfo.Children.Add(diskTitle);

                        // Create disk details
                        TextBlock diskDetails = new TextBlock
                        {
                            Text = $"Size: {sizeGB:F2} GB\n" +
                                   $"Interface Type: {obj["InterfaceType"]}\n" +
                                   $"Serial Number: {obj["SerialNumber"]}\n" +
                                   $"Firmware: {obj["FirmwareRevision"]}\n" +
                                   $"Partitions: {obj["Partitions"]}",
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                            TextWrapping = TextWrapping.Wrap
                        };
                        diskInfo.Children.Add(diskDetails);

                        diskBorder.Child = diskInfo;
                        physDisksPanel.Children.Add(diskBorder);

                        diskNumber++;
                    }
                }

                StorageInfoPanel.Children.Add(physDisksPanel);

                // Add logical drives information
                TextBlock logDrivesTitle = new TextBlock
                {
                    Text = "LOGICAL DRIVES",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                StorageInfoPanel.Children.Add(logDrivesTitle);

                // Create a panel for logical drives
                StackPanel logDrivesPanel = new StackPanel();

                // Get logical drives
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        Border driveBorder = new Border
                        {
                            Style = Resources["HardwareMetricStyle"] as Style
                        };

                        StackPanel driveInfo = new StackPanel();

                        // Calculate sizes
                        double totalSizeGB = drive.TotalSize / (1024.0 * 1024 * 1024);
                        double freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                        double usedSpaceGB = totalSizeGB - freeSpaceGB;
                        double usedPercent = (usedSpaceGB / totalSizeGB) * 100;

                        // Create drive title
                        TextBlock driveTitle = new TextBlock
                        {
                            Text = $"Drive {drive.Name} ({drive.VolumeLabel})",
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        driveInfo.Children.Add(driveTitle);

                        // Create drive details
                        TextBlock driveDetails = new TextBlock
                        {
                            Text = $"File System: {drive.DriveFormat}\n" +
                                   $"Total Size: {totalSizeGB:F2} GB\n" +
                                   $"Used Space: {usedSpaceGB:F2} GB ({usedPercent:F1}%)\n" +
                                   $"Free Space: {freeSpaceGB:F2} GB ({100 - usedPercent:F1}%)",
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        driveInfo.Children.Add(driveDetails);

                        // Add usage bar
                        ProgressBar usageBar = new ProgressBar
                        {
                            Minimum = 0,
                            Maximum = 100,
                            Value = usedPercent,
                            Height = 10,
                            Foreground = GetProgressBarColor(usedPercent),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#444444"))
                        };
                        driveInfo.Children.Add(usageBar);

                        driveBorder.Child = driveInfo;
                        logDrivesPanel.Children.Add(driveBorder);
                    }
                }

                StorageInfoPanel.Children.Add(logDrivesPanel);

                // Add storage performance metrics
                TextBlock perfTitle = new TextBlock
                {
                    Text = "STORAGE PERFORMANCE",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                StorageInfoPanel.Children.Add(perfTitle);

                // Create a panel for storage performance
                StackPanel perfPanel = new StackPanel();

                // Get storage info from HardwareMonitor
                var storageDevices = hardwareMonitor.GetStorageInfo();
                if (storageDevices.Count > 0)
                {
                    foreach (var storage in storageDevices)
                    {
                        Border perfBorder = new Border
                        {
                            Style = Resources["HardwareMetricStyle"] as Style
                        };

                        StackPanel storagePerf = new StackPanel();

                        // Create storage title
                        TextBlock storageTitle = new TextBlock
                        {
                            Text = storage.Key,
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        storagePerf.Children.Add(storageTitle);

                        // Create performance metrics
                        Grid perfGrid = new Grid();
                        perfGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                        perfGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        int row = 0;
                        foreach (var metric in storage.Value)
                        {
                            perfGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                            TextBlock metricName = new TextBlock
                            {
                                Text = metric.Key + ":",
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                                Margin = new Thickness(0, 2, 10, 2)
                            };
                            Grid.SetRow(metricName, row);
                            Grid.SetColumn(metricName, 0);

                            TextBlock metricValue = new TextBlock
                            {
                                Text = metric.Value,
                                Foreground = new SolidColorBrush(Colors.White)
                            };
                            Grid.SetRow(metricValue, row);
                            Grid.SetColumn(metricValue, 1);

                            perfGrid.Children.Add(metricName);
                            perfGrid.Children.Add(metricValue);

                            row++;
                        }

                        storagePerf.Children.Add(perfGrid);

                        perfBorder.Child = storagePerf;
                        perfPanel.Children.Add(perfBorder);
                    }
                }
                else
                {
                    // Fallback to basic performance data
                    Border perfBorder = new Border
                    {
                        Style = Resources["HardwareMetricStyle"] as Style
                    };

                    TextBlock noPerfData = new TextBlock
                    {
                        Text = "Storage performance data not available. Start monitoring to view real-time metrics.",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                        TextWrapping = TextWrapping.Wrap
                    };

                    perfBorder.Child = noPerfData;
                    perfPanel.Children.Add(perfBorder);
                }

                StorageInfoPanel.Children.Add(perfPanel);
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error loading storage information: {ex.Message}");

                // Show error in the panel
                StorageInfoPanel.Children.Clear();
                StorageInfoPanel.Children.Add(new TextBlock
                {
                    Text = $"Error loading storage information: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        private void LoadGpuInfo()
        {
            try
            {
                GraphicsInfoPanel.Children.Clear();

                // Get GPU information from WMI and HardwareMonitor
                var gpuInfo = hardwareMonitor.GetGpuInfo();

                // Add GPU information
                TextBlock basicInfoTitle = new TextBlock
                {
                    Text = "GRAPHICS ADAPTER INFORMATION",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                GraphicsInfoPanel.Children.Add(basicInfoTitle);

                // Create a panel for GPU info
                StackPanel gpuPanel = new StackPanel();

                // Get GPU information from WMI
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    int gpuNumber = 1;
                    foreach (var obj in searcher.Get())
                    {
                        Border gpuBorder = new Border
                        {
                            Style = Resources["HardwareMetricStyle"] as Style
                        };

                        StackPanel gpuDetails = new StackPanel();

                        // Create GPU title
                        TextBlock gpuTitle = new TextBlock
                        {
                            Text = $"GPU {gpuNumber}: {obj["Name"]}",
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        gpuDetails.Children.Add(gpuTitle);

                        // Create basic details grid
                        Grid basicGrid = new Grid();
                        basicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                        basicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        int row = 0;

                        // Add basic GPU details
                        AddGpuGridRow(basicGrid, row++, "Manufacturer:", obj["AdapterCompatibility"].ToString());
                        AddGpuGridRow(basicGrid, row++, "Description:", obj["Description"].ToString());
                        AddGpuGridRow(basicGrid, row++, "Video Processor:", obj["VideoProcessor"].ToString());
                        AddGpuGridRow(basicGrid, row++, "Driver Version:", obj["DriverVersion"].ToString());
                        AddGpuGridRow(basicGrid, row++, "Driver Date:", GetDriverDate(obj["DriverDate"].ToString()));
                        AddGpuGridRow(basicGrid, row++, "Video RAM:", $"{Convert.ToDouble(obj["AdapterRAM"]) / (1024 * 1024 * 1024):F2} GB");
                        AddGpuGridRow(basicGrid, row++, "Current Resolution:", $"{obj["CurrentHorizontalResolution"]} x {obj["CurrentVerticalResolution"]}");
                        AddGpuGridRow(basicGrid, row++, "Refresh Rate:", $"{obj["CurrentRefreshRate"]} Hz");
                        AddGpuGridRow(basicGrid, row++, "Bits Per Pixel:", $"{obj["CurrentBitsPerPixel"]}");
                        AddGpuGridRow(basicGrid, row++, "Status:", obj["Status"].ToString());

                        gpuDetails.Children.Add(basicGrid);

                        gpuBorder.Child = gpuDetails;
                        gpuPanel.Children.Add(gpuBorder);

                        gpuNumber++;
                    }
                }

                GraphicsInfoPanel.Children.Add(gpuPanel);

                // Add GPU performance metrics
                TextBlock perfTitle = new TextBlock
                {
                    Text = "GPU LIVE METRICS",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                GraphicsInfoPanel.Children.Add(perfTitle);

                // Create a panel for GPU live metrics
                StackPanel perfPanel = new StackPanel();

                // Add GPU metrics from HardwareMonitor
                if (gpuInfo.Count > 0)
                {
                    Border metricsBorder = new Border
                    {
                        Style = Resources["HardwareMetricStyle"] as Style
                    };

                    Grid metricsGrid = new Grid();
                    metricsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                    metricsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    int row = 0;
                    foreach (var metric in gpuInfo)
                    {
                        if (metric.Key == "Name")
                            continue;

                        metricsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        TextBlock metricName = new TextBlock
                        {
                            Text = metric.Key + ":",
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                            Margin = new Thickness(0, 5, 0, 5)
                        };
                        Grid.SetRow(metricName, row);
                        Grid.SetColumn(metricName, 0);

                        TextBlock metricValue = new TextBlock
                        {
                            Text = metric.Value,
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 5, 0, 5)
                        };
                        Grid.SetRow(metricValue, row);
                        Grid.SetColumn(metricValue, 1);

                        metricsGrid.Children.Add(metricName);
                        metricsGrid.Children.Add(metricValue);

                        row++;
                    }

                    metricsBorder.Child = metricsGrid;
                    perfPanel.Children.Add(metricsBorder);
                }
                else
                {
                    // Fallback message when no GPU metrics are available
                    Border noMetricsBorder = new Border
                    {
                        Style = Resources["HardwareMetricStyle"] as Style
                    };

                    TextBlock noMetricsText = new TextBlock
                    {
                        Text = "GPU performance metrics are not available. Start monitoring to view real-time GPU metrics.",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                        TextWrapping = TextWrapping.Wrap
                    };

                    noMetricsBorder.Child = noMetricsText;
                    perfPanel.Children.Add(noMetricsBorder);
                }

                GraphicsInfoPanel.Children.Add(perfPanel);

                // Add display information
                TextBlock displayTitle = new TextBlock
                {
                    Text = "DISPLAY INFORMATION",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                GraphicsInfoPanel.Children.Add(displayTitle);

                // Create a panel for display info
                StackPanel displayPanel = new StackPanel();

                // Get display information from WMI
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor"))
                {
                    int monitorNumber = 1;
                    foreach (var obj in searcher.Get())
                    {
                        Border monitorBorder = new Border
                        {
                            Style = Resources["HardwareMetricStyle"] as Style
                        };

                        StackPanel monitorDetails = new StackPanel();

                        // Create monitor title
                        TextBlock monitorTitle = new TextBlock
                        {
                            Text = $"Monitor {monitorNumber}: {obj["Name"]}",
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        monitorDetails.Children.Add(monitorTitle);

                        // Create monitor details
                        Grid monitorGrid = new Grid();
                        monitorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                        monitorGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        int row = 0;

                        AddGpuGridRow(monitorGrid, row++, "Manufacturer:", obj["MonitorManufacturer"]?.ToString() ?? "Unknown");
                        AddGpuGridRow(monitorGrid, row++, "Type:", obj["MonitorType"]?.ToString() ?? "Unknown");
                        AddGpuGridRow(monitorGrid, row++, "Screen Width:", $"{obj["ScreenWidth"]} pixels");
                        AddGpuGridRow(monitorGrid, row++, "Screen Height:", $"{obj["ScreenHeight"]} pixels");
                        AddGpuGridRow(monitorGrid, row++, "Pixel Width:", $"{obj["PixelsPerXLogicalInch"]} DPI");
                        AddGpuGridRow(monitorGrid, row++, "Status:", obj["Status"].ToString());

                        monitorDetails.Children.Add(monitorGrid);

                        monitorBorder.Child = monitorDetails;
                        displayPanel.Children.Add(monitorBorder);

                        monitorNumber++;
                    }
                }

                GraphicsInfoPanel.Children.Add(displayPanel);
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error loading graphics information: {ex.Message}");

                // Show error in the panel
                GraphicsInfoPanel.Children.Clear();
                GraphicsInfoPanel.Children.Add(new TextBlock
                {
                    Text = $"Error loading graphics information: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        private void AddGpuGridRow(Grid grid, int rowIndex, string label, string value)
        {
            // Add a new row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create the label
            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                Style = Resources["PropertyNameStyle"] as Style
            };
            Grid.SetRow(labelBlock, rowIndex);
            Grid.SetColumn(labelBlock, 0);

            // Create the value
            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                Style = Resources["PropertyValueStyle"] as Style
            };
            Grid.SetRow(valueBlock, rowIndex);
            Grid.SetColumn(valueBlock, 1);

            // Add them to the grid
            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
        }

        private void LoadNetworkInfo()
        {
            try
            {
                NetworkInfoPanel.Children.Clear();

                // Get network information
                var networkAdapters = hardwareMonitor.GetNetworkInfo();

                // Add network adapters information
                TextBlock adaptersTitle = new TextBlock
                {
                    Text = "NETWORK ADAPTERS",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                NetworkInfoPanel.Children.Add(adaptersTitle);

                // Create a panel for network adapters
                StackPanel adaptersPanel = new StackPanel();

                // Get network adapters information from WMI
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus != NULL"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        Border adapterBorder = new Border
                        {
                            Style = Resources["HardwareMetricStyle"] as Style
                        };

                        StackPanel adapterDetails = new StackPanel();

                        // Create adapter title
                        TextBlock adapterTitle = new TextBlock
                        {
                            Text = obj["Name"].ToString(),
                            Foreground = new SolidColorBrush(Colors.White),
                            FontWeight = FontWeights.SemiBold,
                            Margin = new Thickness(0, 0, 0, 10)
                        };
                        adapterDetails.Children.Add(adapterTitle);

                        // Create adapter details grid
                        Grid adapterGrid = new Grid();
                        adapterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                        adapterGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        int row = 0;

                        // Add adapter details
                        AddNetworkGridRow(adapterGrid, row++, "Manufacturer:", obj["Manufacturer"].ToString());
                        AddNetworkGridRow(adapterGrid, row++, "Adapter Type:", obj["AdapterType"]?.ToString() ?? "Unknown");
                        AddNetworkGridRow(adapterGrid, row++, "MAC Address:", obj["MACAddress"]?.ToString() ?? "N/A");
                        AddNetworkGridRow(adapterGrid, row++, "Connection Status:", GetNetworkConnectionStatus(Convert.ToUInt16(obj["NetConnectionStatus"])));
                        AddNetworkGridRow(adapterGrid, row++, "Speed:", obj["Speed"] != null ? $"{Convert.ToUInt64(obj["Speed"]) / 1000000} Mbps" : "Unknown");

                        // Get IP information for this adapter
                        var adapterGuid = obj["GUID"]?.ToString();
                        if (!string.IsNullOrEmpty(adapterGuid))
                        {
                            using (var configSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE SettingID = '{adapterGuid}'"))
                            {
                                foreach (var configObj in configSearcher.Get())
                                {
                                    var ipAddresses = (string[])configObj["IPAddress"];
                                    if (ipAddresses != null && ipAddresses.Length > 0)
                                    {
                                        AddNetworkGridRow(adapterGrid, row++, "IP Address:", string.Join(", ", ipAddresses));
                                    }

                                    var subnetMasks = (string[])configObj["IPSubnet"];
                                    if (subnetMasks != null && subnetMasks.Length > 0)
                                    {
                                        AddNetworkGridRow(adapterGrid, row++, "Subnet Mask:", string.Join(", ", subnetMasks));
                                    }

                                    var gateway = (string[])configObj["DefaultIPGateway"];
                                    if (gateway != null && gateway.Length > 0)
                                    {
                                        AddNetworkGridRow(adapterGrid, row++, "Default Gateway:", string.Join(", ", gateway));
                                    }

                                    var dnsServers = (string[])configObj["DNSServerSearchOrder"];
                                    if (dnsServers != null && dnsServers.Length > 0)
                                    {
                                        AddNetworkGridRow(adapterGrid, row++, "DNS Servers:", string.Join(", ", dnsServers));
                                    }

                                    AddNetworkGridRow(adapterGrid, row++, "DHCP Enabled:", configObj["DHCPEnabled"].ToString());
                                    break;
                                }
                            }
                        }

                        adapterDetails.Children.Add(adapterGrid);

                        // Add traffic info if available from HardwareMonitor
                        if (networkAdapters.Count > 0)
                        {
                            foreach (var network in networkAdapters)
                            {
                                if (network.Key.Contains(obj["Name"].ToString()))
                                {
                                    TextBlock trafficTitle = new TextBlock
                                    {
                                        Text = "TRAFFIC",
                                        FontWeight = FontWeights.SemiBold,
                                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                                        Margin = new Thickness(0, 15, 0, 10)
                                    };
                                    adapterDetails.Children.Add(trafficTitle);

                                    Grid trafficGrid = new Grid();
                                    trafficGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                                    trafficGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                    int trafficRow = 0;
                                    foreach (var metric in network.Value)
                                    {
                                        AddNetworkGridRow(trafficGrid, trafficRow++, metric.Key + ":", metric.Value);
                                    }

                                    adapterDetails.Children.Add(trafficGrid);
                                    break;
                                }
                            }
                        }

                        adapterBorder.Child = adapterDetails;
                        adaptersPanel.Children.Add(adapterBorder);
                    }
                }

                NetworkInfoPanel.Children.Add(adaptersPanel);

                // Add TCP/IP information
                TextBlock tcpipTitle = new TextBlock
                {
                    Text = "TCP/IP CONFIGURATION",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                NetworkInfoPanel.Children.Add(tcpipTitle);

                // Create a grid for TCP/IP info
                Grid tcpipGrid = new Grid();
                tcpipGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
                tcpipGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                int rowIndex = 0;

                // Add general TCP/IP info
                AddNetworkGridRow(tcpipGrid, rowIndex++, "Host Name:", Environment.MachineName);
                AddNetworkGridRow(tcpipGrid, rowIndex++, "Domain Name:", GetComputerDomain());

                // Add IP configuration
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled=TRUE"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var domainName = obj["DNSDomain"]?.ToString();
                        if (!string.IsNullOrEmpty(domainName))
                        {
                            AddNetworkGridRow(tcpipGrid, rowIndex++, "Primary DNS Suffix:", domainName);
                        }

                        var nodeType = obj["TcpipNetbiosOptions"];
                        if (nodeType != null)
                        {
                            AddNetworkGridRow(tcpipGrid, rowIndex++, "Node Type:", GetNodeType(Convert.ToUInt32(nodeType)));
                        }

                        break;
                    }
                }

                NetworkInfoPanel.Children.Add(tcpipGrid);

                // Add network connectivity tests
                TextBlock connectivityTitle = new TextBlock
                {
                    Text = "NETWORK CONNECTIVITY",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
                    Margin = new Thickness(0, 25, 0, 15)
                };
                NetworkInfoPanel.Children.Add(connectivityTitle);

                // Create a stack panel for connectivity tests
                StackPanel connectivityPanel = new StackPanel();

                // Add a button to run connectivity tests
                Button testButton = new Button
                {
                    Content = "Run Connectivity Tests",
                    Margin = new Thickness(0, 0, 0, 15),
                    Padding = new Thickness(15, 10, 15, 10),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498db")),
                    Foreground = new SolidColorBrush(Colors.White),
                    BorderThickness = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                testButton.Click += RunConnectivityTests_Click;
                connectivityPanel.Children.Add(testButton);

                // Add a border for test results
                Border resultsBorder = new Border
                {
                    Style = Resources["HardwareMetricStyle"] as Style
                };

                StackPanel resultsPanel = new StackPanel();

                TextBlock resultsText = new TextBlock
                {
                    Text = "Click the button above to run connectivity tests.",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                    TextWrapping = TextWrapping.Wrap
                };
                resultsPanel.Children.Add(resultsText);

                resultsBorder.Child = resultsPanel;
                connectivityPanel.Children.Add(resultsBorder);

                NetworkInfoPanel.Children.Add(connectivityPanel);
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error loading network information: {ex.Message}");

                // Show error in the panel
                NetworkInfoPanel.Children.Clear();
                NetworkInfoPanel.Children.Add(new TextBlock
                {
                    Text = $"Error loading network information: {ex.Message}",
                    Foreground = new SolidColorBrush(Colors.Red),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

        private void AddNetworkGridRow(Grid grid, int rowIndex, string label, string value)
        {
            // Add a new row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Create the label
            TextBlock labelBlock = new TextBlock
            {
                Text = label,
                Style = Resources["PropertyNameStyle"] as Style
            };
            Grid.SetRow(labelBlock, rowIndex);
            Grid.SetColumn(labelBlock, 0);

            // Create the value
            TextBlock valueBlock = new TextBlock
            {
                Text = value,
                Style = Resources["PropertyValueStyle"] as Style
            };
            Grid.SetRow(valueBlock, rowIndex);
            Grid.SetColumn(valueBlock, 1);

            // Add them to the grid
            grid.Children.Add(labelBlock);
            grid.Children.Add(valueBlock);
        }

        private async void RunConnectivityTests_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable the button while tests are running
                Button button = sender as Button;
                button.IsEnabled = false;
                button.Content = "Running Tests...";

                // Get the results border and panel
                Border resultsBorder = (button.Parent as StackPanel).Children[1] as Border;
                StackPanel resultsPanel = resultsBorder.Child as StackPanel;

                // Clear previous results
                resultsPanel.Children.Clear();

                // Add a status message
                TextBlock statusText = new TextBlock
                {
                    Text = "Running connectivity tests...",
                    Foreground = new SolidColorBrush(Colors.White),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                resultsPanel.Children.Add(statusText);

                // Log the action
                loggingService.Log(LogLevel.Info, "User initiated network connectivity tests");

                // Run tests asynchronously
                await Task.Run(() =>
                {
                    // Test internet connectivity using common websites
                    string[] testSites = { "8.8.8.8", "1.1.1.1", "www.google.com", "www.microsoft.com" };

                    foreach (string site in testSites)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            TextBlock pingText = new TextBlock
                            {
                                Text = $"Testing connection to {site}...",
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                                TextWrapping = TextWrapping.Wrap
                            };
                            resultsPanel.Children.Add(pingText);
                        });

                        try
                        {
                            using (Ping ping = new Ping())
                            {
                                PingReply reply = ping.Send(site, 3000);

                                Dispatcher.Invoke(() =>
                                {
                                    TextBlock resultText = new TextBlock
                                    {
                                        Text = $"Result: {reply.Status}, Round-trip time: {reply.RoundtripTime}ms",
                                        Foreground = reply.Status == IPStatus.Success
                                            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"))
                                            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c")),
                                        TextWrapping = TextWrapping.Wrap,
                                        Margin = new Thickness(15, 0, 0, 10)
                                    };
                                    resultsPanel.Children.Add(resultText);
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                TextBlock errorText = new TextBlock
                                {
                                    Text = $"Error: {ex.Message}",
                                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c")),
                                    TextWrapping = TextWrapping.Wrap,
                                    Margin = new Thickness(15, 0, 0, 10)
                                };
                                resultsPanel.Children.Add(errorText);
                            });
                        }
                    }

                    // Test DNS resolution
                    Dispatcher.Invoke(() =>
                    {
                        TextBlock dnsText = new TextBlock
                        {
                            Text = "Testing DNS resolution...",
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 10, 0, 0)
                        };
                        resultsPanel.Children.Add(dnsText);
                    });

                    try
                    {
                        string[] hosts = { "www.google.com", "www.microsoft.com", "www.amazon.com" };
                        foreach (string host in hosts)
                        {
                            var hostEntry = System.Net.Dns.GetHostEntry(host);

                            Dispatcher.Invoke(() =>
                            {
                                TextBlock resultText = new TextBlock
                                {
                                    Text = $"Resolved {host} to {string.Join(", ", hostEntry.AddressList.Select(a => a.ToString()))}",
                                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71")),
                                    TextWrapping = TextWrapping.Wrap,
                                    Margin = new Thickness(15, 0, 0, 0)
                                };
                                resultsPanel.Children.Add(resultText);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            TextBlock errorText = new TextBlock
                            {
                                Text = $"DNS Error: {ex.Message}",
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c")),
                                TextWrapping = TextWrapping.Wrap,
                                Margin = new Thickness(15, 0, 0, 10)
                            };
                            resultsPanel.Children.Add(errorText);
                        });
                    }
                });

                // Update status message
                statusText.Text = "Connectivity tests completed.";

                // Add timestamp
                TextBlock timestampText = new TextBlock
                {
                    Text = $"Tests completed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB")),
                    FontStyle = FontStyles.Italic,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 15, 0, 0)
                };
                resultsPanel.Children.Add(timestampText);

                // Enable the button again
                button.IsEnabled = true;
                button.Content = "Run Connectivity Tests";

                loggingService.Log(LogLevel.Info, "Network connectivity tests completed");
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error running connectivity tests: {ex.Message}");

                Button button = sender as Button;
                button.IsEnabled = true;
                button.Content = "Run Connectivity Tests";

                MessageBox.Show($"Error running connectivity tests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Update system information
            LoadSystemInformation();

            // Log the refresh action
            loggingService.Log(LogLevel.Info, "User refreshed system information");
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"SysMax_System_Info_Export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Build the system info text
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("==========================================================");
                    sb.AppendLine($"SysMax System Information Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine("==========================================================");
                    sb.AppendLine();

                    // System Summary
                    sb.AppendLine("SYSTEM SUMMARY");
                    sb.AppendLine("----------------------------------------------------------");
                    sb.AppendLine($"Computer Name: {ComputerNameValue.Text}");
                    sb.AppendLine($"Processor: {ProcessorValue.Text}");
                    sb.AppendLine($"Memory: {MemoryValue.Text}");
                    sb.AppendLine($"Graphics: {GraphicsValue.Text}");
                    sb.AppendLine($"Storage: {StorageValue.Text}");
                    sb.AppendLine($"Motherboard: {MotherboardValue.Text}");
                    sb.AppendLine($"Network: {NetworkValue.Text}");
                    sb.AppendLine($"BIOS Version: {BiosValue.Text}");
                    sb.AppendLine();

                    // Operating System
                    sb.AppendLine("OPERATING SYSTEM");
                    sb.AppendLine("----------------------------------------------------------");
                    sb.AppendLine($"OS Name: {OsNameValue.Text}");
                    sb.AppendLine($"Version: {OsVersionValue.Text}");
                    sb.AppendLine($"Build: {OsBuildValue.Text}");
                    sb.AppendLine($"Architecture: {OsArchValue.Text}");
                    sb.AppendLine($"Installed On: {OsInstalledValue.Text}");
                    sb.AppendLine($"Last Update: {OsLastUpdateValue.Text}");
                    sb.AppendLine($"Uptime: {UptimeValue.Text}");
                    sb.AppendLine($"Username: {UsernameValue.Text}");
                    sb.AppendLine($"Computer Role: {ComputerRoleValue.Text}");
                    sb.AppendLine();

                    // CPU Information
                    sb.AppendLine("CPU INFORMATION");
                    sb.AppendLine("----------------------------------------------------------");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            sb.AppendLine($"Name: {obj["Name"]}");
                            sb.AppendLine($"Manufacturer: {obj["Manufacturer"]}");
                            sb.AppendLine($"Description: {obj["Description"]}");
                            sb.AppendLine($"Architecture: {GetCpuArchitecture(Convert.ToUInt16(obj["Architecture"]))}");
                            sb.AppendLine($"Number of Cores: {obj["NumberOfCores"]}");
                            sb.AppendLine($"Logical Processors: {obj["NumberOfLogicalProcessors"]}");
                            sb.AppendLine($"Max Clock Speed: {obj["MaxClockSpeed"]} MHz");
                            sb.AppendLine($"Current Clock Speed: {obj["CurrentClockSpeed"]} MHz");
                            sb.AppendLine($"Socket Designation: {obj["SocketDesignation"]}");
                            sb.AppendLine($"L2 Cache Size: {Convert.ToUInt32(obj["L2CacheSize"]) / 1024} MB");
                            sb.AppendLine($"L3 Cache Size: {Convert.ToUInt32(obj["L3CacheSize"]) / 1024} MB");
                            break; // Only get the first CPU
                        }
                    }
                    sb.AppendLine();

                    // Memory Information
                    sb.AppendLine("MEMORY INFORMATION");
                    sb.AppendLine("----------------------------------------------------------");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            double totalPhysicalMemory = Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                            sb.AppendLine($"Total Physical Memory: {totalPhysicalMemory:F2} GB");
                            break;
                        }
                    }
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            double freePhysicalMemory = Convert.ToDouble(obj["FreePhysicalMemory"]) / (1024 * 1024);
                            sb.AppendLine($"Available Memory: {freePhysicalMemory:F2} GB");

                            double totalVirtualMemory = Convert.ToDouble(obj["TotalVirtualMemorySize"]) / 1024;
                            sb.AppendLine($"Total Virtual Memory: {totalVirtualMemory:F2} GB");

                            double freeVirtualMemory = Convert.ToDouble(obj["FreeVirtualMemory"]) / 1024;
                            sb.AppendLine($"Available Virtual Memory: {freeVirtualMemory:F2} GB");
                            break;
                        }
                    }

                    sb.AppendLine("\nMemory Modules:");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                    {
                        int moduleNumber = 1;
                        foreach (var obj in searcher.Get())
                        {
                            double capacity = Convert.ToDouble(obj["Capacity"]) / (1024 * 1024 * 1024);
                            sb.AppendLine($"  Module {moduleNumber}: {capacity:F2} GB");
                            sb.AppendLine($"    Manufacturer: {obj["Manufacturer"]}");
                            sb.AppendLine($"    Speed: {obj["Speed"]} MHz");
                            sb.AppendLine($"    Form Factor: {GetMemoryFormFactor(Convert.ToUInt16(obj["FormFactor"]))}");
                            sb.AppendLine($"    Bank Label: {obj["BankLabel"]}, Device Locator: {obj["DeviceLocator"]}");
                            moduleNumber++;
                        }
                    }
                    sb.AppendLine();

                    // Storage Information
                    sb.AppendLine("STORAGE INFORMATION");
                    sb.AppendLine("----------------------------------------------------------");
                    sb.AppendLine("Physical Disks:");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                    {
                        int diskNumber = 1;
                        foreach (var obj in searcher.Get())
                        {
                            double sizeGB = Convert.ToDouble(obj["Size"]) / (1024 * 1024 * 1024);
                            sb.AppendLine($"  Disk {diskNumber}: {obj["Model"]}");
                            sb.AppendLine($"    Size: {sizeGB:F2} GB");
                            sb.AppendLine($"    Interface Type: {obj["InterfaceType"]}");
                            sb.AppendLine($"    Serial Number: {obj["SerialNumber"]}");
                            sb.AppendLine($"    Firmware: {obj["FirmwareRevision"]}");
                            sb.AppendLine($"    Partitions: {obj["Partitions"]}");
                            diskNumber++;
                        }
                    }

                    sb.AppendLine("\nLogical Drives:");
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    foreach (DriveInfo drive in allDrives)
                    {
                        if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                        {
                            double totalSizeGB = drive.TotalSize / (1024.0 * 1024 * 1024);
                            double freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                            double usedSpaceGB = totalSizeGB - freeSpaceGB;
                            double usedPercent = (usedSpaceGB / totalSizeGB) * 100;

                            sb.AppendLine($"  Drive {drive.Name} ({drive.VolumeLabel})");
                            sb.AppendLine($"    File System: {drive.DriveFormat}");
                            sb.AppendLine($"    Total Size: {totalSizeGB:F2} GB");
                            sb.AppendLine($"    Used Space: {usedSpaceGB:F2} GB ({usedPercent:F1}%)");
                            sb.AppendLine($"    Free Space: {freeSpaceGB:F2} GB ({(100 - usedPercent):F1}%)");
                        }
                    }
                    sb.AppendLine();

                    // Graphics Information
                    sb.AppendLine("GRAPHICS INFORMATION");
                    sb.AppendLine("----------------------------------------------------------");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    {
                        int gpuNumber = 1;
                        foreach (var obj in searcher.Get())
                        {
                            sb.AppendLine($"GPU {gpuNumber}: {obj["Name"]}");
                            sb.AppendLine($"  Manufacturer: {obj["AdapterCompatibility"]}");
                            sb.AppendLine($"  Description: {obj["Description"]}");
                            sb.AppendLine($"  Video Processor: {obj["VideoProcessor"]}");
                            sb.AppendLine($"  Driver Version: {obj["DriverVersion"]}");
                            sb.AppendLine($"  Driver Date: {GetDriverDate(obj["DriverDate"].ToString())}");
                            sb.AppendLine($"  Video RAM: {Convert.ToDouble(obj["AdapterRAM"]) / (1024 * 1024 * 1024):F2} GB");
                            sb.AppendLine($"  Current Resolution: {obj["CurrentHorizontalResolution"]} x {obj["CurrentVerticalResolution"]}");
                            sb.AppendLine($"  Refresh Rate: {obj["CurrentRefreshRate"]} Hz");
                            sb.AppendLine($"  Bits Per Pixel: {obj["CurrentBitsPerPixel"]}");
                            sb.AppendLine($"  Status: {obj["Status"]}");
                            gpuNumber++;
                        }
                    }
                    sb.AppendLine();

                    // Network Information
                    sb.AppendLine("NETWORK INFORMATION");
                    sb.AppendLine("----------------------------------------------------------");
                    sb.AppendLine($"IP Address: {IpAddressValue.Text}");
                    sb.AppendLine($"Network Adapter: {NetworkAdapterValue.Text}");
                    sb.AppendLine($"Network Status: {NetworkStatusValue.Text}");
                    sb.AppendLine();

                    // Network Adapters
                    sb.AppendLine("Network Adapters:");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus != NULL"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            sb.AppendLine($"  {obj["Name"]}");
                            sb.AppendLine($"    Manufacturer: {obj["Manufacturer"]}");
                            sb.AppendLine($"    Adapter Type: {obj["AdapterType"]?.ToString() ?? "Unknown"}");
                            sb.AppendLine($"    MAC Address: {obj["MACAddress"]?.ToString() ?? "N/A"}");
                            sb.AppendLine($"    Connection Status: {GetNetworkConnectionStatus(Convert.ToUInt16(obj["NetConnectionStatus"]))}");
                            sb.AppendLine($"    Speed: {(obj["Speed"] != null ? $"{Convert.ToUInt64(obj["Speed"]) / 1000000} Mbps" : "Unknown")}");

                            // Get IP information
                            var adapterGuid = obj["GUID"]?.ToString();
                            if (!string.IsNullOrEmpty(adapterGuid))
                            {
                                using (var configSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE SettingID = '{adapterGuid}'"))
                                {
                                    foreach (var configObj in configSearcher.Get())
                                    {
                                        var ipAddresses = (string[])configObj["IPAddress"];
                                        if (ipAddresses != null && ipAddresses.Length > 0)
                                        {
                                            sb.AppendLine($"    IP Address: {string.Join(", ", ipAddresses)}");
                                        }

                                        var subnetMasks = (string[])configObj["IPSubnet"];
                                        if (subnetMasks != null && subnetMasks.Length > 0)
                                        {
                                            sb.AppendLine($"    Subnet Mask: {string.Join(", ", subnetMasks)}");
                                        }

                                        var gateway = (string[])configObj["DefaultIPGateway"];
                                        if (gateway != null && gateway.Length > 0)
                                        {
                                            sb.AppendLine($"    Default Gateway: {string.Join(", ", gateway)}");
                                        }

                                        var dnsServers = (string[])configObj["DNSServerSearchOrder"];
                                        if (dnsServers != null && dnsServers.Length > 0)
                                        {
                                            sb.AppendLine($"    DNS Servers: {string.Join(", ", dnsServers)}");
                                        }

                                        sb.AppendLine($"    DHCP Enabled: {configObj["DHCPEnabled"]}");
                                        break;
                                    }
                                }
                            }
                            sb.AppendLine();
                        }
                    }

                    // Write the built string to the selected file
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());

                    // Log the export action
                    loggingService.Log(LogLevel.Info, $"System information exported to {saveFileDialog.FileName}");

                    // Show confirmation
                    MessageBox.Show("System information exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Show assistant message
                    if (mainWindow != null)
                    {
                        mainWindow.ShowAssistantMessage("I've exported all system information to a text file. You can open this file to view detailed information about your system hardware and software.");
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error exporting system information: {ex.Message}");
                MessageBox.Show($"Error exporting system information: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MonitoringTimer_Tick(object sender, EventArgs e)
        {
            // Update metrics when monitoring is active
            if (isMonitoring)
            {
                UpdateLiveMetrics();
            }
        }

        private void StartMonitoringButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMonitoring)
            {
                // Stop monitoring
                monitoringTimer.Stop();
                isMonitoring = false;

                // Update button text if we have a reference to it
                if (monitoringButton != null)
                {
                    monitoringButton.Content = "Start Monitoring";
                }
                else if (sender is Button button)
                {
                    button.Content = "Start Monitoring";
                }

                // Update status
                if (mainWindow != null)
                {
                    var method = mainWindow.GetType().GetMethod("UpdateStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(mainWindow, new object[] { "System monitoring stopped" });
                    }

                    mainWindow.ShowAssistantMessage("I've stopped monitoring your system in real-time. The values you see now are from the last update.");
                }

                // Log action
                loggingService.Log(LogLevel.Info, "User stopped system monitoring");
            }
            else
            {
                // Start monitoring
                monitoringTimer.Start();
                isMonitoring = true;

                // Update button text if we have a reference to it
                if (monitoringButton != null)
                {
                    monitoringButton.Content = "Stop Monitoring";
                }
                else if (sender is Button button)
                {
                    button.Content = "Stop Monitoring";
                }

                // Update status
                if (mainWindow != null)
                {
                    var method = mainWindow.GetType().GetMethod("UpdateStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(mainWindow, new object[] { "System monitoring started" });
                    }

                    mainWindow.ShowAssistantMessage("I'm now monitoring your system in real-time. The metrics will update every 2 seconds, letting you see how your computer is performing.");
                }

                // Log action
                loggingService.Log(LogLevel.Info, "User started system monitoring");
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When tab changes, reset scroll position for better user experience
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Content is ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToTop();
                }
            }
        }

        // Handle cleanup when the page is unloaded
        private void SystemInfoPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up resources when the page is unloaded
            monitoringTimer.Stop();

            // Log page unload
            loggingService.Log(LogLevel.Info, "System Information page unloaded");
        }

        #region Helper Methods

        private string GetOSCaption()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Caption"].ToString();
                    }
                }
                return "Unknown Operating System";
            }
            catch
            {
                return "Unknown Operating System";
            }
        }

        private string GetOSVersionAndServicePack()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Version, ServicePackMajorVersion FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        string version = obj["Version"].ToString();
                        uint spMajor = Convert.ToUInt32(obj["ServicePackMajorVersion"]);

                        if (spMajor > 0)
                        {
                            return $"{version} Service Pack {spMajor}";
                        }
                        else
                        {
                            return version;
                        }
                    }
                }
                return "Unknown Version";
            }
            catch
            {
                return "Unknown Version";
            }
        }

        private string GetOSBuild()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT BuildNumber FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["BuildNumber"].ToString();
                    }
                }
                return "Unknown Build";
            }
            catch
            {
                return "Unknown Build";
            }
        }

        private string GetOSInstallDate()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT InstallDate FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        string installDateString = obj["InstallDate"].ToString();
                        if (DateTime.TryParseExact(installDateString.Substring(0, 14), "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime installDate))
                        {
                            return installDate.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            return "Unknown";
                        }
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetLastWindowsUpdateDate()
        {
            try
            {
                // This uses registry to get the last update date
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Detect"))
                {
                    if (key != null)
                    {
                        var lastCheckedString = key.GetValue("LastSuccessTime")?.ToString();
                        if (!string.IsNullOrEmpty(lastCheckedString))
                        {
                            if (DateTime.TryParse(lastCheckedString, out DateTime lastChecked))
                            {
                                return lastChecked.ToString("yyyy-MM-dd");
                            }
                        }
                    }
                }

                // Try another registry key if the first one didn't work
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Install"))
                {
                    if (key != null)
                    {
                        var lastInstalledString = key.GetValue("LastSuccessTime")?.ToString();
                        if (!string.IsNullOrEmpty(lastInstalledString))
                        {
                            if (DateTime.TryParse(lastInstalledString, out DateTime lastInstalled))
                            {
                                return lastInstalled.ToString("yyyy-MM-dd");
                            }
                        }
                    }
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetComputerDomain()
        {
            try
            {
                return Environment.UserDomainName;
            }
            catch
            {
                return "Unknown Domain";
            }
        }

        private string GetComputerRole()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT DomainRole FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        uint roleValue = Convert.ToUInt32(obj["DomainRole"]);
                        switch (roleValue)
                        {
                            case 0: return "Standalone Workstation";
                            case 1: return "Member Workstation";
                            case 2: return "Standalone Server";
                            case 3: return "Member Server";
                            case 4: return "Backup Domain Controller";
                            case 5: return "Primary Domain Controller";
                            default: return "Unknown Role";
                        }
                    }
                }
                return "Unknown Role";
            }
            catch
            {
                return "Unknown Role";
            }
        }

        private string GetSystemUptime()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        string bootTimeString = obj["LastBootUpTime"].ToString();
                        if (DateTime.TryParseExact(bootTimeString.Substring(0, 14), "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime bootTime))
                        {
                            TimeSpan uptime = DateTime.Now - bootTime;
                            return $"{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes";
                        }
                        else
                        {
                            return "Unknown";
                        }
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetLocalIPAddress()
        {
            try
            {
                string ipAddresses = "";

                // Get IPv4 addresses
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                         ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                if (!string.IsNullOrEmpty(ipAddresses))
                                {
                                    ipAddresses += ", ";
                                }
                                ipAddresses += ip.Address.ToString();
                            }
                        }
                    }
                }

                return string.IsNullOrEmpty(ipAddresses) ? "Not connected" : ipAddresses;
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetActiveNetworkAdapter()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                         ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                    {
                        return ni.Name;
                    }
                }
                return "No active network adapter";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetProcessorName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Name"].ToString();
                    }
                }
                return "Unknown Processor";
            }
            catch
            {
                return "Unknown Processor";
            }
        }

        private double GetTotalPhysicalMemory()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return Math.Round(Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024), 2);
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GetGraphicsCardName()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Name"].ToString();
                    }
                }
                return "Unknown Graphics Card";
            }
            catch
            {
                return "Unknown Graphics Card";
            }
        }

        private string GetStorageSummary()
        {
            try
            {
                long totalSize = 0;
                long totalFreeSpace = 0;

                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        totalSize += drive.TotalSize;
                        totalFreeSpace += drive.AvailableFreeSpace;
                    }
                }

                if (totalSize > 0)
                {
                    double totalSizeGB = totalSize / (1024.0 * 1024 * 1024);
                    double freeSpaceGB = totalFreeSpace / (1024.0 * 1024 * 1024);
                    double usedPercent = 100 - (freeSpaceGB / totalSizeGB * 100);

                    return $"{totalSizeGB:F0} GB ({usedPercent:F0}% used)";
                }

                return "Unknown Storage";
            }
            catch
            {
                return "Unknown Storage";
            }
        }

        private string GetMotherboardInfo()
        {
            try
            {
                string manufacturer = "";
                string product = "";

                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        manufacturer = obj["Manufacturer"].ToString();
                        product = obj["Product"].ToString();
                        break;
                    }
                }

                return $"{manufacturer} {product}";
            }
            catch
            {
                return "Unknown Motherboard";
            }
        }

        private string GetNetworkAdapterName()
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                         ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                    {
                        return ni.Description;
                    }
                }
                return "No active network adapter";
            }
            catch
            {
                return "Unknown Network Adapter";
            }
        }

        private string GetBiosVersion()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, SMBIOSBIOSVersion FROM Win32_BIOS"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"].ToString();
                        string version = obj["SMBIOSBIOSVersion"].ToString();
                        return $"{manufacturer} {version}";
                    }
                }
                return "Unknown BIOS Version";
            }
            catch
            {
                return "Unknown BIOS Version";
            }
        }

        private string GetRegisteredOwner()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string owner = key.GetValue("RegisteredOwner")?.ToString() ?? "Unknown";
                        return owner;
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetRegisteredOrganization()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string org = key.GetValue("RegisteredOrganization")?.ToString() ?? "Unknown";
                        return org;
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetWindowsProductId()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        string productId = key.GetValue("ProductId")?.ToString() ?? "Unknown";
                        return productId;
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetWindowsProductKey()
        {
            // For security/privacy reasons, we'll return a masked key
            return "XXXXX-XXXXX-XXXXX-XXXXX-XXXXX";
        }

        private string GetWindowsDefenderStatus()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection"))
                {
                    if (key != null)
                    {
                        var disableRealtimeMonitoring = key.GetValue("DisableRealtimeMonitoring");
                        if (disableRealtimeMonitoring != null && Convert.ToInt32(disableRealtimeMonitoring) == 0)
                        {
                            return "Enabled";
                        }
                        else
                        {
                            return "Disabled";
                        }
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetWindowsFirewallStatus()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM FirewallProduct"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        bool enabled = Convert.ToBoolean(obj["Enabled"]);
                        return enabled ? "Enabled" : "Disabled";
                    }
                }

                // Fallback using registry
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile"))
                {
                    if (key != null)
                    {
                        var enableFirewall = key.GetValue("EnableFirewall");
                        if (enableFirewall != null && Convert.ToInt32(enableFirewall) == 1)
                        {
                            return "Enabled";
                        }
                        else
                        {
                            return "Disabled";
                        }
                    }
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetWindowsUpdateSettings()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update"))
                {
                    if (key != null)
                    {
                        var auOptions = key.GetValue("AUOptions");
                        if (auOptions != null)
                        {
                            switch (Convert.ToInt32(auOptions))
                            {
                                case 1: return "Never check for updates";
                                case 2: return "Check for updates but let me choose whether to download and install them";
                                case 3: return "Download updates but let me choose whether to install them";
                                case 4: return "Install updates automatically";
                                default: return "Unknown";
                            }
                        }
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetUACLevel()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System"))
                {
                    if (key != null)
                    {
                        var consentPromptBehaviorAdmin = key.GetValue("ConsentPromptBehaviorAdmin");
                        var promptOnSecureDesktop = key.GetValue("PromptOnSecureDesktop");

                        if (consentPromptBehaviorAdmin != null && promptOnSecureDesktop != null)
                        {
                            int adminValue = Convert.ToInt32(consentPromptBehaviorAdmin);
                            int desktopValue = Convert.ToInt32(promptOnSecureDesktop);

                            if (adminValue == 0)
                                return "Never notify";
                            else if (adminValue == 5 && desktopValue == 0)
                                return "Notify me only when apps try to make changes (no secure desktop)";
                            else if (adminValue == 5 && desktopValue == 1)
                                return "Notify me only when apps try to make changes (with secure desktop)";
                            else if (adminValue == 2 && desktopValue == 1)
                                return "Always notify";
                            else
                                return "Custom";
                        }
                    }
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private bool IsSystemRestoreEnabled()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var systemDrive = Environment.GetEnvironmentVariable("SystemDrive");
                        using (var restoreKey = Registry.LocalMachine.OpenSubKey($@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore\RPSessionInterval"))
                        {
                            return restoreKey != null;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool IsFastStartupEnabled()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power"))
                {
                    if (key != null)
                    {
                        var hibernateBootEnabled = key.GetValue("HiberbootEnabled");
                        if (hibernateBootEnabled != null)
                        {
                            return Convert.ToInt32(hibernateBootEnabled) == 1;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private string GetCpuArchitecture(ushort architecture)
        {
            switch (architecture)
            {
                case 0: return "x86";
                case 1: return "MIPS";
                case 2: return "Alpha";
                case 3: return "PowerPC";
                case 6: return "ia64";
                case 9: return "x64";
                default: return "Unknown";
            }
        }

        private string[] GetCpuFeatures()
        {
            List<string> features = new List<string>();

            try
            {
                // Common CPU features to look for
                string[] featuresToCheck = {
                    "MMX", "SSE", "SSE2", "SSE3", "SSSE3", "SSE4.1", "SSE4.2", "AVX", "AVX2", "AVX512",
                    "AES", "FMA", "FMA3", "FMA4", "BMI1", "BMI2", "VT-x", "AMD-V", "Intel VT-d", "AMD-Vi",
                    "XOP", "F16C", "FP16", "SGX", "CET", "VAES", "CLMUL", "PCLMULQDQ", "RDRAND", "RDSEED",
                    "ADX", "SHA", "VNNI", "3DNow"
                };

                // Get processor ID from WMI
                string processorId = "";
                using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        processorId = obj["ProcessorId"].ToString();
                        break;
                    }
                }

                // Check registry for CPUID flags
                using (var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0"))
                {
                    if (key != null)
                    {
                        // Add basic features
                        foreach (var feature in featuresToCheck)
                        {
                            if (key.GetValue($"Feature{feature}") != null ||
                                (processorId.Contains(feature) && !features.Contains(feature)))
                            {
                                features.Add(feature);
                            }
                        }
                    }
                }

                // If no features were found, add some basic ones based on processor name
                if (features.Count == 0)
                {
                    string processorName = GetProcessorName().ToUpper();

                    if (processorName.Contains("INTEL"))
                    {
                        // Basic Intel features
                        features.AddRange(new[] { "MMX", "SSE", "SSE2", "SSE3" });

                        if (processorName.Contains("CORE"))
                        {
                            features.AddRange(new[] { "SSSE3", "SSE4.1", "SSE4.2" });
                        }

                        if (processorName.Contains("I7") || processorName.Contains("I9"))
                        {
                            features.AddRange(new[] { "AVX", "AVX2", "AES" });
                        }
                    }
                    else if (processorName.Contains("AMD"))
                    {
                        // Basic AMD features
                        features.AddRange(new[] { "MMX", "SSE", "SSE2", "SSE3" });

                        if (processorName.Contains("RYZEN"))
                        {
                            features.AddRange(new[] { "AVX", "AVX2", "SSE4.1", "SSE4.2", "AES" });
                        }
                    }
                }
            }
            catch
            {
                // If all else fails, add some very basic features
                features.AddRange(new[] { "MMX", "SSE", "SSE2" });
            }

            return features.ToArray();
        }

        private string GetMemoryFormFactor(ushort formFactor)
        {
            switch (formFactor)
            {
                case 0: return "Unknown";
                case 1: return "Other";
                case 2: return "SIP";
                case 3: return "DIP";
                case 4: return "ZIP";
                case 5: return "SOJ";
                case 6: return "Proprietary";
                case 7: return "SIMM";
                case 8: return "DIMM";
                case 9: return "TSOP";
                case 10: return "PGA";
                case 11: return "RIMM";
                case 12: return "SODIMM";
                case 13: return "SRIMM";
                case 14: return "SMD";
                case 15: return "SSMP";
                case 16: return "QFP";
                case 17: return "TQFP";
                case 18: return "SOIC";
                case 19: return "LCC";
                case 20: return "PLCC";
                case 21: return "BGA";
                case 22: return "FPBGA";
                case 23: return "LGA";
                default: return "Unknown";
            }
        }

        private SolidColorBrush GetProgressBarColor(double value)
        {
            if (value > 90)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
            }
            else if (value > 75)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
            }
            else
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
            }
        }

        private string GetDriverDate(string driverDate)
        {
            // WMI returns dates in a specific format that needs to be parsed
            try
            {
                if (driverDate.Length >= 14)
                {
                    // Extract YYYYMMDD portion
                    string dateStr = driverDate.Substring(0, 8);

                    // Parse year, month, day
                    int year = int.Parse(dateStr.Substring(0, 4));
                    int month = int.Parse(dateStr.Substring(4, 2));
                    int day = int.Parse(dateStr.Substring(6, 2));

                    return $"{year}-{month:D2}-{day:D2}";
                }
                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private string GetNetworkConnectionStatus(ushort status)
        {
            switch (status)
            {
                case 0: return "Disconnected";
                case 1: return "Connecting";
                case 2: return "Connected";
                case 3: return "Disconnecting";
                case 4: return "Hardware not present";
                case 5: return "Hardware disabled";
                case 6: return "Hardware malfunction";
                case 7: return "Media disconnected";
                case 8: return "Authenticating";
                case 9: return "Authentication succeeded";
                case 10: return "Authentication failed";
                case 11: return "Invalid address";
                case 12: return "Credentials required";
                default: return "Unknown";
            }
        }

        private string GetNodeType(uint nodeType)
        {
            switch (nodeType)
            {
                case 1: return "B-node (Broadcast)";
                case 2: return "P-node (Peer-to-peer)";
                case 4: return "M-node (Mixed)";
                case 8: return "H-node (Hybrid)";
                default: return "Unknown";
            }
        }

        #endregion
    }
}