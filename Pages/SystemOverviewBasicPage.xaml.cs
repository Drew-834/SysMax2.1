using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Text;
using SysMax2._1.Services;
using SysMax2._1.Models;

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
        private LoggingService loggingService = LoggingService.Instance;
        private List<IssueInfo> currentIssues = new List<IssueInfo>();

        public SystemOverviewBasicPage()
        {
            InitializeComponent();

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Initialize the current issues
            InitializeCurrentIssues();

            // Initialize with simulated data
            UpdateSystemHealth();

            // Log page initialization
            loggingService.Log(LogLevel.Info, "System Overview (Basic) page initialized");

            // Show the assistant with helpful information
            if (mainWindow != null)
            {
                mainWindow.ShowAssistantMessage("Welcome to the System Overview! Here you can see the health of your computer at a glance and perform common maintenance tasks.");
            }
        }

        private void InitializeCurrentIssues()
        {
            // Add predefined issues
            currentIssues.Add(new IssueInfo
            {
                Icon = "⚠️",
                Text = "Your disk is getting full. This might slow down your computer.",
                FixButtonText = "Fix Now",
                FixActionTag = "DiskSpace",
                IssueSeverity = IssueInfo.Severity.Medium,
                Timestamp = DateTime.Now
            });

            currentIssues.Add(new IssueInfo
            {
                Icon = "🔒",
                Text = "Important security updates are available to keep your computer safe.",
                FixButtonText = "Update Now",
                FixActionTag = "WindowsUpdate",
                IssueSeverity = IssueInfo.Severity.High,
                Timestamp = DateTime.Now
            });

            // Update the issue count
            UpdateIssueCount();
        }

        private void UpdateIssueCount()
        {
            IssueCount.Text = $"{currentIssues.Count} issues found";

            // Set text color based on issue count
            if (currentIssues.Count == 0)
            {
                IssueCount.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                NoIssuesMessage.Visibility = Visibility.Visible;
                CopyAllIssuesButton.IsEnabled = false;
            }
            else
            {
                // Yellow for 1-2 issues, red for 3+
                if (currentIssues.Count > 2)
                {
                    IssueCount.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                }
                else
                {
                    IssueCount.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                }
                NoIssuesMessage.Visibility = Visibility.Collapsed;
                CopyAllIssuesButton.IsEnabled = true;
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

                // Log health status
                loggingService.Log(LogLevel.Warning, $"System health: {healthStatus} - Disk: {diskSpace}%, CPU Temp: {cpuTemp}°C, Memory: {memoryUsage}%");
            }
            else if (diskSpace > 80 || cpuTemp > 70 || memoryUsage > 70)
            {
                healthStatus = "Fair";
                healthColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f39c12"));
                OverallHealthStatus.Text = healthStatus;
                OverallHealthIndicator.Fill = healthColor;

                // Log health status
                loggingService.Log(LogLevel.Info, $"System health: {healthStatus} - Disk: {diskSpace}%, CPU Temp: {cpuTemp}°C, Memory: {memoryUsage}%");
            }
            else
            {
                healthStatus = "Good";
                healthColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ecc71"));
                OverallHealthStatus.Text = healthStatus;
                OverallHealthIndicator.Fill = healthColor;

                // Log health status
                loggingService.Log(LogLevel.Info, $"System health: {healthStatus} - Disk: {diskSpace}%, CPU Temp: {cpuTemp}°C, Memory: {memoryUsage}%");
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
                        try
                        {
                            Process.Start("ms-settings:windowsupdate");
                            loggingService.Log(LogLevel.Info, "User initiated Windows Update check");
                        }
                        catch (Exception ex)
                        {
                            loggingService.Log(LogLevel.Error, $"Error launching Windows Update: {ex.Message}");
                        }

                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("I'm checking for Windows updates. This helps keep your computer secure and working properly.");
                        }
                        break;

                    case "Cleanup":
                        try
                        {
                            Process.Start("cleanmgr.exe");
                            loggingService.Log(LogLevel.Info, "User initiated Disk Cleanup");
                        }
                        catch (Exception ex)
                        {
                            loggingService.Log(LogLevel.Error, $"Error launching Disk Cleanup: {ex.Message}");
                        }

                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("Disk Cleanup will help free up space by removing temporary files and emptying the Recycle Bin. This can help your computer run faster.");
                        }
                        break;

                    case "StartupApps":
                        try
                        {
                            Process.Start("taskmgr.exe", "/7");
                            loggingService.Log(LogLevel.Info, "User accessed Startup Apps management");
                        }
                        catch (Exception ex)
                        {
                            loggingService.Log(LogLevel.Error, $"Error launching Task Manager: {ex.Message}");
                        }

                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("You can make your computer start faster by turning off programs you don't need when Windows starts. In the window that opens, look at the 'Startup' tab.");
                        }
                        break;

                    case "SecurityScan":
                        try
                        {
                            Process.Start("ms-settings:windowsdefender");
                            loggingService.Log(LogLevel.Info, "User initiated Security Scan");
                        }
                        catch (Exception ex)
                        {
                            loggingService.Log(LogLevel.Error, $"Error launching Windows Defender: {ex.Message}");
                        }

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
                        try
                        {
                            Process.Start("cleanmgr.exe");
                            loggingService.Log(LogLevel.Info, "User initiated fix for disk space issue");
                        }
                        catch (Exception ex)
                        {
                            loggingService.Log(LogLevel.Error, $"Error launching Disk Cleanup: {ex.Message}");
                        }

                        if (mainWindow != null)
                        {
                            mainWindow.ShowAssistantMessage("I'm launching Disk Cleanup to help free up space. This will remove temporary files and empty your Recycle Bin, which helps your computer run better.");
                        }
                        break;

                    case "WindowsUpdate":
                        try
                        {
                            Process.Start("ms-settings:windowsupdate");
                            loggingService.Log(LogLevel.Info, "User initiated fix for Windows Update issue");
                        }
                        catch (Exception ex)
                        {
                            loggingService.Log(LogLevel.Error, $"Error launching Windows Update: {ex.Message}");
                        }

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

                loggingService.Log(LogLevel.Info, "User initiated system scan");

                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("I'm checking your computer. This will take a moment...");
                }

                // Simulate a scan with delay
                await Task.Delay(3000);

                // Update UI with new health metrics
                UpdateSystemHealth();

                // Simulate finding new issues based on random conditions
                if (random.Next(0, 3) == 1)  // 1 in 3 chance of finding a new issue
                {
                    AddRandomIssue();
                }

                loggingService.Log(LogLevel.Info, "System scan completed");

                if (mainWindow != null)
                {
                    mainWindow.ShowAssistantMessage("Scan complete! I've updated the system health information with the latest results.");
                }
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error during system scan: {ex.Message}");
            }
            finally
            {
                RunScanButton.Content = "Check My Computer";
                RunScanButton.IsEnabled = true;
                isScanning = false;
            }
        }

        private void AddRandomIssue()
        {
            // Create a few possible random issues
            List<IssueInfo> possibleIssues = new List<IssueInfo>
            {
                new IssueInfo
                {
                    Icon = "🌡️",
                    Text = "CPU temperature is high. Make sure your computer's cooling is working properly.",
                    FixButtonText = "Learn More",
                    FixActionTag = "CPUTemperature",
                    IssueSeverity = IssueInfo.Severity.Medium
                },
                new IssueInfo
                {
                    Icon = "📊",
                    Text = "RAM usage is high. Try closing some applications to improve performance.",
                    FixButtonText = "Fix Now",
                    FixActionTag = "HighMemory",
                    IssueSeverity = IssueInfo.Severity.Medium
                },
                new IssueInfo
                {
                    Icon = "🛡️",
                    Text = "Windows Firewall is disabled. This could put your computer at risk.",
                    FixButtonText = "Enable Firewall",
                    FixActionTag = "Firewall",
                    IssueSeverity = IssueInfo.Severity.High
                },
                new IssueInfo
                {
                    Icon = "🔋",
                    Text = "Your power plan is set to high performance. This might reduce battery life.",
                    FixButtonText = "Change Plan",
                    FixActionTag = "PowerPlan",
                    IssueSeverity = IssueInfo.Severity.Low
                }
            };

            // Select a random issue
            IssueInfo newIssue = possibleIssues[random.Next(possibleIssues.Count)];
            newIssue.Timestamp = DateTime.Now;

            // Don't add duplicate issues
            bool isDuplicate = false;
            foreach (var issue in currentIssues)
            {
                if (issue.Text == newIssue.Text)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                // Add to the current issues list
                currentIssues.Add(newIssue);

                // Log the new issue
                loggingService.Log(LogLevel.Warning, $"New issue detected: {newIssue.Text}");

                // Update the UI
                AddIssueToUI(newIssue);

                // Update the issue count
                UpdateIssueCount();
            }
        }

        private void AddIssueToUI(IssueInfo issue)
        {
            // Create a new issue border
            Border issueBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a2a2a")),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Create the grid
            Grid issueGrid = new Grid();
            issueGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            issueGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            issueGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            issueGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Create icon
            TextBlock iconTextBlock = new TextBlock
            {
                Text = issue.Icon,
                FontSize = 24,
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Create text
            TextBlock textTextBlock = new TextBlock
            {
                Text = issue.Text,
                Foreground = new SolidColorBrush(Colors.White),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = $"Issue{currentIssues.Count}"  // Tag for copy functionality
            };

            // Create copy button
            Button copyButton = new Button
            {
                Content = "📋",
                FontSize = 16,
                Margin = new Thickness(10, 0, 10, 0),
                Padding = new Thickness(5, 0, 5, 0),
                Style = Resources["CopyButtonStyle"] as Style,
                Tag = $"Issue{currentIssues.Count}",  // Tag for copy functionality
                ToolTip = "Copy this issue to clipboard"
            };
            copyButton.Click += CopyIssueButton_Click;

            // Create fix button
            Button fixButton = new Button
            {
                Content = issue.FixButtonText,
                Style = Resources["FixButtonStyle"] as Style,
                Tag = issue.FixActionTag
            };
            fixButton.Click += FixIssue_Click;

            // Add all elements to the grid
            Grid.SetColumn(iconTextBlock, 0);
            Grid.SetColumn(textTextBlock, 1);
            Grid.SetColumn(copyButton, 2);
            Grid.SetColumn(fixButton, 3);

            issueGrid.Children.Add(iconTextBlock);
            issueGrid.Children.Add(textTextBlock);
            issueGrid.Children.Add(copyButton);
            issueGrid.Children.Add(fixButton);

            // Add the grid to the border
            issueBorder.Child = issueGrid;

            // Add the border to the issues list
            IssuesList.Children.Add(issueBorder);
        }

        private void CopyIssueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button)
                {
                    string issueTag = button.Tag?.ToString() ?? "";
                    string issueText = "";

                    // Find the corresponding text block
                    switch (issueTag)
                    {
                        case "Issue1":
                            issueText = Issue1Text.Text;
                            break;
                        case "Issue2":
                            issueText = Issue2Text.Text;
                            break;
                        default:
                            // For dynamically added issues
                            foreach (var child in IssuesList.Children)
                            {
                                if (child is Border border && border.Child is Grid grid)
                                {
                                    foreach (var gridChild in grid.Children)
                                    {
                                        if (gridChild is TextBlock textBlock && textBlock.Tag != null && textBlock.Tag.ToString() == issueTag)
                                        {
                                            issueText = textBlock.Text;
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                    }

                    if (!string.IsNullOrEmpty(issueText))
                    {
                        // Format the text
                        string formattedText = $"SysMax Issue Report - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                              $"Issue: {issueText}\n" +
                                              $"Reported by: SysMax System Health Monitor";

                        // Copy to clipboard
                        Clipboard.SetText(formattedText);

                        // Log
                        loggingService.Log(LogLevel.Info, $"User copied issue to clipboard: {issueText}");

                        // Show confirmation
                        MessageBox.Show("Issue copied to clipboard!", "Copy Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error copying issue to clipboard: {ex.Message}");
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyAllIssuesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentIssues.Count > 0)
                {
                    // Build text for all issues
                    StringBuilder allIssuesText = new StringBuilder();
                    allIssuesText.AppendLine($"SysMax System Health Report - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    allIssuesText.AppendLine($"System Health: {OverallHealthStatus.Text}");
                    allIssuesText.AppendLine($"Issues Detected: {currentIssues.Count}");
                    allIssuesText.AppendLine("--------------------------------------------------------");

                    for (int i = 0; i < currentIssues.Count; i++)
                    {
                        allIssuesText.AppendLine($"Issue {i + 1}: {currentIssues[i].Text}");
                        allIssuesText.AppendLine($"Severity: {currentIssues[i].IssueSeverity}");
                        allIssuesText.AppendLine($"Detected: {currentIssues[i].Timestamp:yyyy-MM-dd HH:mm:ss}");
                        allIssuesText.AppendLine($"Recommended Action: {currentIssues[i].FixButtonText} ({currentIssues[i].FixActionTag})");
                        allIssuesText.AppendLine();
                    }

                    allIssuesText.AppendLine("--------------------------------------------------------");
                    allIssuesText.AppendLine("Generated by SysMax System Health Monitor");

                    // Copy to clipboard
                    Clipboard.SetText(allIssuesText.ToString());

                    // Log
                    loggingService.Log(LogLevel.Info, $"User copied all issues to clipboard ({currentIssues.Count} issues)");

                    // Show confirmation
                    MessageBox.Show("All issues copied to clipboard!", "Copy Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error copying all issues to clipboard: {ex.Message}");
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}