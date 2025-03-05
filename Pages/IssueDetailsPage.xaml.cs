using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SysMax2._1.Models;
using SysMax2._1.Services;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for IssueDetailsPage.xaml
    /// </summary>
    public partial class IssueDetailsPage : Page
    {
        private MainWindow mainWindow;
        private LoggingService loggingService = LoggingService.Instance;
        private IssueInfo currentIssue;
        private string issueType;

        public IssueDetailsPage(string issueType = "DiskSpace")
        {
            InitializeComponent();

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Store the issue type
            this.issueType = issueType;

            // Initialize with the specified issue type
            LoadIssueDetails(issueType);

            // Log page initialization
            loggingService.Log(LogLevel.Info, $"Issue Details page opened for issue type: {issueType}");

            // Show the assistant with helpful information
            if (mainWindow != null)
            {
                mainWindow.ShowAssistantMessage("This page shows detailed information about the issue and steps to fix it. Follow the recommended actions to resolve the problem.");
            }
        }

        private void LoadIssueDetails(string issueType)
        {
            // Create a mock issue based on the specified type
            switch (issueType)
            {
                case "DiskSpace":
                    currentIssue = new IssueInfo
                    {
                        Icon = "⚠️",
                        Text = "Your disk is getting full. This might slow down your computer.",
                        FixButtonText = "Fix Now",
                        FixActionTag = "DiskSpace",
                        IssueSeverity = IssueInfo.Severity.Medium,
                        Timestamp = DateTime.Now.AddHours(-3) // Detected 3 hours ago
                    };

                    // Update the UI with disk space specific details
                    IssueTitle.Text = "Disk Space Low";
                    IssueDescription.Text = "Your disk is getting full. This might slow down your computer and prevent updates from installing correctly.";
                    IssueIcon.Text = "💾";
                    IssueSeverity.Text = "Medium";
                    IssueImpact.Text = "Performance";
                    IssueDetected.Text = currentIssue.Timestamp.ToString("yyyy-MM-dd h:mm tt");

                    // No need to update troubleshooting steps - already set in XAML for disk space issue
                    break;

                case "WindowsUpdate":
                    currentIssue = new IssueInfo
                    {
                        Icon = "🔒",
                        Text = "Important security updates are available to keep your computer safe.",
                        FixButtonText = "Update Now",
                        FixActionTag = "WindowsUpdate",
                        IssueSeverity = IssueInfo.Severity.High,
                        Timestamp = DateTime.Now.AddDays(-1) // Detected 1 day ago
                    };

                    // Update the UI with Windows Update specific details
                    IssueTitle.Text = "Windows Updates Available";
                    IssueDescription.Text = "Important security updates are available to keep your computer safe from the latest threats.";
                    IssueIcon.Text = "🔄";
                    IssueSeverity.Text = "High";
                    IssueSeverity.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e74c3c"));
                    IssueImpact.Text = "Security";
                    IssueDetected.Text = currentIssue.Timestamp.ToString("yyyy-MM-dd h:mm tt");

                    // Update troubleshooting steps for Windows Update issue
                    UpdateTroubleshootingStepsForWindowsUpdate();

                    // Update additional info text
                    AdditionalInfoText.Text = "Keeping Windows updated is essential for maintaining security and performance. Updates include patches for security vulnerabilities, bug fixes, and sometimes new features.\n\n" +
                                             "Delaying updates may expose your computer to:\n" +
                                             "• Security vulnerabilities\n" +
                                             "• Compatibility issues with newer software\n" +
                                             "• Missing out on performance improvements\n" +
                                             "• Potential system instability\n\n" +
                                             "Windows updates are designed to keep your system running smoothly and securely. It's recommended to install them as soon as possible.";

                    // Update IT Pro information
                    ITProInfoText.Text = "Technical Details:\n" +
                                        "There are pending Windows updates available for installation. These include security updates and cumulative updates.\n\n" +
                                        "System Information:\n" +
                                        "• Current Windows version: Windows 10 Pro 21H2\n" +
                                        "• Last update check: " + DateTime.Now.AddHours(-12).ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
                                        "• Update KB numbers: KB5022282, KB5022845\n" +
                                        "• Update types: Security, Cumulative\n" +
                                        "• Windows Update service status: Running\n\n" +
                                        "Enterprise Solutions:\n" +
                                        "• Use WSUS or Microsoft Endpoint Manager to manage updates\n" +
                                        "• PowerShell check for updates: Get-WindowsUpdate\n" +
                                        "• PowerShell install all updates: Install-WindowsUpdate -AcceptAll\n" +
                                        "• Use Group Policy to configure update behavior: Computer Configuration > Administrative Templates > Windows Components > Windows Update";
                    break;

                case "CPUTemperature":
                    currentIssue = new IssueInfo
                    {
                        Icon = "🌡️",
                        Text = "CPU temperature is high. Make sure your computer's cooling is working properly.",
                        FixButtonText = "Learn More",
                        FixActionTag = "CPUTemperature",
                        IssueSeverity = IssueInfo.Severity.Medium,
                        Timestamp = DateTime.Now.AddMinutes(-30) // Detected 30 minutes ago
                    };

                    // Update the UI with CPU Temperature specific details
                    IssueTitle.Text = "CPU Temperature High";
                    IssueDescription.Text = "Your processor is running at a higher temperature than normal. This could lead to performance throttling or hardware damage if not addressed.";
                    IssueIcon.Text = "🌡️";
                    IssueSeverity.Text = "Medium";
                    IssueImpact.Text = "Performance, Hardware";
                    IssueDetected.Text = currentIssue.Timestamp.ToString("yyyy-MM-dd h:mm tt");

                    // Update troubleshooting steps for CPU Temperature issue
                    UpdateTroubleshootingStepsForCPUTemperature();

                    // Update additional info text
                    AdditionalInfoText.Text = "CPU temperature is an important factor in system health and performance. Modern CPUs will automatically reduce their speed (thermal throttling) when they get too hot to prevent damage.\n\n" +
                                             "High CPU temperatures can be caused by:\n" +
                                             "• Dust buildup in cooling vents or heatsink\n" +
                                             "• Poor case airflow\n" +
                                             "• Failing cooling fan\n" +
                                             "• Improper thermal paste application\n" +
                                             "• Running CPU-intensive applications\n" +
                                             "• High ambient room temperature\n\n" +
                                             "Most CPUs are designed to operate safely up to 95°C, but for optimal performance and longevity, it's best to keep temperatures below 80°C under load.";

                    // Update IT Pro information
                    ITProInfoText.Text = "Technical Details:\n" +
                                        "The CPU is operating at elevated temperatures that may lead to thermal throttling and reduced performance.\n\n" +
                                        "System Information:\n" +
                                        "• CPU Model: Intel Core i7-12700K\n" +
                                        "• Current Temperature: 87°C\n" +
                                        "• Thermal Design Power (TDP): 125W\n" +
                                        "• Thermal Throttling Threshold: 95°C\n" +
                                        "• Fan Speed: 2200 RPM (90%)\n" +
                                        "• Current CPU Usage: 76%\n\n" +
                                        "Enterprise Solutions:\n" +
                                        "• Remotely monitor temperatures with HWiNFO + SNMP/WMI\n" +
                                        "• PowerShell temperature monitoring: Get-WmiObject MSAcpi_ThermalZoneTemperature -Namespace \"root\\wmi\"\n" +
                                        "• Schedule regular maintenance for dust removal\n" +
                                        "• Consider implementing rack cooling monitoring in server environments\n" +
                                        "• Refer to Intel documentation for specific thermal guidelines for your CPU model";
                    break;

                case "HighMemory":
                    currentIssue = new IssueInfo
                    {
                        Icon = "📊",
                        Text = "RAM usage is high. Try closing some applications to improve performance.",
                        FixButtonText = "Fix Now",
                        FixActionTag = "HighMemory",
                        IssueSeverity = IssueInfo.Severity.Medium,
                        Timestamp = DateTime.Now.AddMinutes(-5) // Detected 5 minutes ago
                    };

                    // Update the UI with Memory specific details
                    IssueTitle.Text = "High Memory Usage";
                    IssueDescription.Text = "Your computer is using a large amount of RAM. This can slow down performance, especially when multitasking.";
                    IssueIcon.Text = "📊";
                    IssueSeverity.Text = "Medium";
                    IssueImpact.Text = "Performance";
                    IssueDetected.Text = currentIssue.Timestamp.ToString("yyyy-MM-dd h:mm tt");

                    // Update troubleshooting steps for Memory issue
                    UpdateTroubleshootingStepsForMemory();

                    // Update additional info text
                    AdditionalInfoText.Text = "Memory (RAM) is used to store data that active applications need for quick access. When memory usage is high, your computer may become less responsive, especially when switching between applications.\n\n" +
                                             "High memory usage can be caused by:\n" +
                                             "• Too many applications running simultaneously\n" +
                                             "• Memory leaks in applications\n" +
                                             "• Browser tabs consuming memory\n" +
                                             "• Background services and processes\n" +
                                             "• Malware or viruses\n\n" +
                                             "Windows will use the page file (virtual memory on your disk) when physical memory is full, but this is much slower than RAM and will reduce performance.";

                    // Update IT Pro information
                    ITProInfoText.Text = "Technical Details:\n" +
                                        "Physical memory utilization is exceeding optimal thresholds, potentially causing page file usage and performance degradation.\n\n" +
                                        "System Information:\n" +
                                        "• Total Physical Memory: 16 GB\n" +
                                        "• Memory Currently In Use: 13.4 GB (84%)\n" +
                                        "• Available Memory: 2.6 GB\n" +
                                        "• Page File Usage: 3.2 GB\n" +
                                        "• Top Memory Consumers:\n" +
                                        "  - Chrome.exe: 2.1 GB\n" +
                                        "  - Outlook.exe: 1.3 GB\n" +
                                        "  - Teams.exe: 0.9 GB\n\n" +
                                        "Enterprise Solutions:\n" +
                                        "• PowerShell memory assessment: Get-Process | Sort-Object -Property WS -Descending | Select-Object -First 10\n" +
                                        "• Use Resource Monitor for detailed memory analysis\n" +
                                        "• Consider implementing application memory limits via Group Policy\n" +
                                        "• For Chrome deployment, use policies to limit memory usage: ChromeMemoryLimit";
                    break;

                default:
                    // Default to disk space issue if the type is unknown
                    LoadIssueDetails("DiskSpace");
                    break;
            }
        }

        private void UpdateTroubleshootingStepsForWindowsUpdate()
        {
            // Clear existing steps
            TroubleshootingSteps.Children.Clear();

            // Add Windows Update specific steps

            // Step 1
            Border step1 = CreateTroubleshootingStep(
                "1",
                "Check for Windows Updates",
                "Open Windows Update settings to see what updates are available and install them.",
                "Check for Updates",
                "WindowsUpdate");
            TroubleshootingSteps.Children.Add(step1);

            // Step 2
            Border step2 = CreateTroubleshootingStep(
                "2",
                "Restart Your Computer",
                "Some updates require a restart to complete installation. Save your work and restart your computer.",
                "Restart Later",
                "RestartComputer");
            TroubleshootingSteps.Children.Add(step2);

            // Step 3
            Border step3 = CreateTroubleshootingStep(
                "3",
                "Troubleshoot Update Issues",
                "If updates fail to install, run the Windows Update Troubleshooter to automatically fix common problems.",
                "Run Troubleshooter",
                "UpdateTroubleshooter");
            TroubleshootingSteps.Children.Add(step3);
        }

        private void UpdateTroubleshootingStepsForCPUTemperature()
        {
            // Clear existing steps
            TroubleshootingSteps.Children.Clear();

            // Add CPU Temperature specific steps

            // Step 1
            Border step1 = CreateTroubleshootingStep(
                "1",
                "Check for Dust and Airflow",
                "Ensure your computer's vents are clear of dust and the computer has proper airflow. Consider using compressed air to clean vents and fans.",
                "View Guide",
                "CleaningGuide");
            TroubleshootingSteps.Children.Add(step1);

            // Step 2
            Border step2 = CreateTroubleshootingStep(
                "2",
                "Close Intensive Applications",
                "Check Task Manager for applications using high CPU. Close any unnecessary CPU-intensive applications.",
                "Open Task Manager",
                "TaskManager");
            TroubleshootingSteps.Children.Add(step2);

            // Step 3
            Border step3 = CreateTroubleshootingStep(
                "3",
                "Check CPU Fan Operation",
                "Make sure your CPU fan is working properly. Listen for unusual noises or check if it's spinning correctly.",
                "Run Hardware Diagnostics",
                "HardwareDiagnostics");
            TroubleshootingSteps.Children.Add(step3);
        }

        private void UpdateTroubleshootingStepsForMemory()
        {
            // Clear existing steps
            TroubleshootingSteps.Children.Clear();

            // Add Memory specific steps

            // Step 1
            Border step1 = CreateTroubleshootingStep(
                "1",
                "Close Unnecessary Applications",
                "Check Task Manager to see which applications are using the most memory and close ones you don't need.",
                "Open Task Manager",
                "TaskManager");
            TroubleshootingSteps.Children.Add(step1);

            // Step 2
            Border step2 = CreateTroubleshootingStep(
                "2",
                "Restart Your Computer",
                "Restarting your computer can clear memory and reset applications that might have memory leaks.",
                "Restart Later",
                "RestartComputer");
            TroubleshootingSteps.Children.Add(step2);

            // Step 3
            Border step3 = CreateTroubleshootingStep(
                "3",
                "Check for Memory Issues",
                "Run Windows Memory Diagnostic to check if there are hardware issues with your RAM.",
                "Run Memory Diagnostic",
                "MemoryDiagnostic");
            TroubleshootingSteps.Children.Add(step3);
        }

        private Border CreateTroubleshootingStep(string stepNumber, string title, string description, string buttonText, string actionTag)
        {
            // Create the main border
            Border stepBorder = new Border
            {
                Style = Resources["TroubleshootingStepStyle"] as Style
            };

            // Create the grid
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Create the step number border
            Border numberBorder = new Border
            {
                Width = 28,
                Height = 28,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498db")),
                CornerRadius = new CornerRadius(14),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 15, 0)
            };

            // Create the step number text
            TextBlock numberText = new TextBlock
            {
                Text = stepNumber,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };

            // Add number text to number border
            numberBorder.Child = numberText;

            // Create content stack panel
            StackPanel contentPanel = new StackPanel();

            // Create title text
            TextBlock titleText = new TextBlock
            {
                Text = title,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            };

            // Create description text
            TextBlock descriptionText = new TextBlock
            {
                Text = description,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDDDDD")),
                TextWrapping = TextWrapping.Wrap
            };

            // Create button
            Button actionButton = new Button
            {
                Content = buttonText,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 10, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a2a2a")),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(10, 5,5,5),
                Tag = actionTag
            };
            actionButton.Click += FixAction_Click;

            // Add elements to content panel
            contentPanel.Children.Add(titleText);
            contentPanel.Children.Add(descriptionText);
            contentPanel.Children.Add(actionButton);

            // Add elements to grid
            Grid.SetColumn(numberBorder, 0);
            Grid.SetColumn(contentPanel, 1);
            grid.Children.Add(numberBorder);
            grid.Children.Add(contentPanel);

            // Add grid to border
            stepBorder.Child = grid;

            return stepBorder;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the overview page
            if (mainWindow != null)
            {
                // Use reflection to access the method if it's not directly accessible
                var method = mainWindow.GetType().GetMethod("NavigateToPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(mainWindow, new object[] { "Overview" });

                    // Log navigation
                    loggingService.Log(LogLevel.Info, "User navigated back to System Overview from Issue Details");
                }
            }
        }

        private void FixAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string actionTag = button.Tag?.ToString() ?? "";

                // Log the action
                loggingService.Log(LogLevel.Info, $"User initiated fix action: {actionTag} for issue: {issueType}");

                // Perform the action based on the tag
                switch (actionTag)
                {
                    case "DiskSpace":
                        try { Process.Start("cleanmgr.exe"); }
                        catch (Exception ex) { LogError(ex, "Disk Cleanup"); }
                        break;

                    case "UninstallPrograms":
                        try { Process.Start("appwiz.cpl"); }
                        catch (Exception ex) { LogError(ex, "Programs and Features"); }
                        break;

                    case "StorageSettings":
                        try { Process.Start("ms-settings:storagesense"); }
                        catch (Exception ex) { LogError(ex, "Storage Settings"); }
                        break;

                    case "WindowsUpdate":
                        try { Process.Start("ms-settings:windowsupdate"); }
                        catch (Exception ex) { LogError(ex, "Windows Update"); }
                        break;

                    case "RestartComputer":
                        // Show confirmation dialog before restarting
                        if (MessageBox.Show("Are you sure you want to restart your computer?\n\nPlease save any unsaved work first.",
                            "Restart Computer", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            try
                            {
                                loggingService.Log(LogLevel.Info, "User initiated system restart");
                                Process.Start("shutdown.exe", "/r /t 60 /c \"System restart initiated by SysMax\"");
                                MessageBox.Show("Your computer will restart in 60 seconds. You can cancel this by running 'shutdown /a' in a command prompt.",
                                    "Restart Scheduled", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                LogError(ex, "Restart Computer");
                            }
                        }
                        break;

                    case "UpdateTroubleshooter":
                        try { Process.Start("ms-settings:troubleshoot"); }
                        catch (Exception ex) { LogError(ex, "Update Troubleshooter"); }
                        break;

                    case "TaskManager":
                        try { Process.Start("taskmgr.exe"); }
                        catch (Exception ex) { LogError(ex, "Task Manager"); }
                        break;

                    case "MemoryDiagnostic":
                        try { Process.Start("mdsched.exe"); }
                        catch (Exception ex) { LogError(ex, "Memory Diagnostic"); }
                        break;

                    case "CleaningGuide":
                        // Show a message with cleaning instructions
                        MessageBox.Show("Computer Cleaning Guide:\n\n" +
                                       "1. Shut down and unplug your computer\n" +
                                       "2. Take it to a well-ventilated area\n" +
                                       "3. Use compressed air to blow dust from vents\n" +
                                       "4. Ensure fans are spinning freely\n" +
                                       "5. Check that no vents are blocked\n\n" +
                                       "For desktop computers, consider opening the case (if under warranty, check if this is allowed) and cleaning internal components with compressed air.",
                                       "Computer Cleaning Guide", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;

                    case "HardwareDiagnostics":
                        try { Process.Start("msdt.exe", "/id DeviceDiagnostic"); }
                        catch (Exception ex) { LogError(ex, "Hardware Diagnostics"); }
                        break;

                    case "CPUTemperature":
                        // Show a message with temperature info
                        MessageBox.Show("CPU Temperature Information:\n\n" +
                                       "Ideal CPU temperatures:\n" +
                                       "• Idle: 30-45°C\n" +
                                       "• Light load: 45-60°C\n" +
                                       "• Heavy load: 60-80°C\n" +
                                       "• Concerning: Above 85°C\n" +
                                       "• Critical: Above 90°C\n\n" +
                                       "Your current CPU temperature is high. Consider cleaning your computer's cooling system or using it in a cooler environment.",
                                       "CPU Temperature Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;

                    default:
                        // Unknown action
                        loggingService.Log(LogLevel.Warning, $"Unknown fix action requested: {actionTag}");
                        break;
                }
            }
        }

        private void LogError(Exception ex, string action)
        {
            loggingService.Log(LogLevel.Error, $"Error launching {action}: {ex.Message}");
            MessageBox.Show($"Error launching {action}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}