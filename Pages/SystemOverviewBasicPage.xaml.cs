using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for SystemOverviewBasicPage.xaml
    /// </summary>
    public partial class SystemOverviewBasicPage : Page
    {
        private Random random = new Random();
        private bool isScanning = false;
        private MainWindow mainWindow;

        public SystemOverviewBasicPage()
        {
            InitializeComponent();

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Initialize with simulated data
            UpdateSystemHealth();

            // Show the assistant with helpful information
            if (mainWindow != null)
            {
                mainWindow.ShowAssistantMessage("Welcome to the System Overview! Here you can see the health of your computer at a glance and perform common maintenance tasks.");
            }
        }

        private void UpdateSystemHealth()
        {
            // Simulate system health data
            int diskSpace = random.Next(65, 95); // 65-95% used
            int memoryUsage = random.Next(30, 70); // 30-70% used
            bool updatesNeeded = random.Next(0, 2) == 1; // 50% chance
            int cpuTemp = random.Next(45, 85); // CPU temperature

            // Determine overall health
            string healthStatus;
            SolidColorBrush healthColor;

            if (diskSpace > 90 || cpuTemp > 80 || memoryUsage > 85 || updatesNeeded)
            {
                healthStatus = "Needs Attention";
                healthColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                OverallHealthStatus.Text = healthStatus;
                OverallHealthIndicator.Fill = healthColor;
            }
            else if (diskSpace > 80 || cpuTemp > 70 || memoryUsage > 70)
            {
                healthStatus = "Fair";
                healthColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                OverallHealthStatus.Text = healthStatus;
                OverallHealthIndicator.Fill = healthColor;
            }
            else
            {
                healthStatus = "Good";
                healthColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                OverallHealthStatus.Text = healthStatus;
                OverallHealthIndicator.Fill = healthColor;
            }

            // Determine action needed text
            if (diskSpace > 85)
            {
                ActionNeededText.Text = "You should clean your disk soon";
            }
            else if (updatesNeeded)
            {
                ActionNeededText.Text = "Install available Windows updates";
            }
            else if (cpuTemp > 75)
            {
                ActionNeededText.Text = "Your computer might be running hot";
            }
            else
            {
                ActionNeededText.Text = "Your system is running well";
            }
        }

        private void QuickAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string action = button.Name.Replace("Button", "");

                // In a real application, these would perform actual system actions
                switch (action)
                {
                    case "CheckUpdates":
                        try { Process.Start("ms-settings:windowsupdate"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("I'm checking for Windows updates. This helps keep your computer secure and working properly.");
                        }
                        break;

                    case "Cleanup":
                        try { Process.Start("cleanmgr.exe"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Disk Cleanup will help free up space by removing temporary files and emptying the Recycle Bin. This can help your computer run faster.");
                        }
                        break;

                    case "StartupApps":
                        try { Process.Start("taskmgr.exe", "/7"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("You can make your computer start faster by turning off programs you don't need when Windows starts. In the window that opens, look at the 'Startup' tab.");
                        }
                        break;

                    case "SecurityScan":
                        try { Process.Start("ms-settings:windowsdefender"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("I'm running a quick security check to make sure your computer is protected from viruses and other threats.");
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

                // In a real application, these would perform actual fixes
                switch (issueType)
                {
                    case "DiskSpace":
                        try { Process.Start("cleanmgr.exe"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("I'm launching Disk Cleanup to help free up space. This will remove temporary files and empty your Recycle Bin, which helps your computer run better.");
                        }
                        break;

                    case "WindowsUpdate":
                        try { Process.Start("ms-settings:windowsupdate"); } catch { /* Handle errors */ }
                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Installing Windows updates helps keep your computer secure and working properly. After updates are installed, you might need to restart your computer.");
                        }
                        break;
                }
            }
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

                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("I'm checking your computer. This will take a moment...");
                }

                // Simulate a scan with delay
                await Task.Delay(3000);

                // Update UI with new health metrics
                UpdateSystemHealth();

                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("Scan complete! I've updated the system health information with the latest results.");
                }
            }
            finally
            {
                RunScanButton.Content = "Check My Computer";
                RunScanButton.IsEnabled = true;
                isScanning = false;
            }
        }
    }
}