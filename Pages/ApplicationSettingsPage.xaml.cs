using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Diagnostics;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for ApplicationSettingsPage.xaml
    /// </summary>
    public partial class ApplicationSettingsPage : Page
    {
        private MainWindow mainWindow;

        public ApplicationSettingsPage()
        {
            InitializeComponent();

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Load settings (in a real app, these would be loaded from storage)
            LoadSettings();
        }

        private void LoadSettings()
        {
            // In a real app, these would be loaded from a settings file or registry
            // For now, we'll just set some default values

            StartWithWindowsCheck.IsChecked = true;
            AutoUpdateCheck.IsChecked = true;
            RefreshMediumOption.IsChecked = true;
            UseAnimationsCheck.IsChecked = true;
            SystemAlertsCheck.IsChecked = true;
            PerformanceWarningsCheck.IsChecked = true;
            UpdatesNotificationsCheck.IsChecked = true;
            UsageDataCheck.IsChecked = true;
            CrashReportsCheck.IsChecked = true;

            CpuThresholdSlider.Value = 80;
            MemoryThresholdSlider.Value = 85;
            DiskThresholdSlider.Value = 90;
            DiskSpaceThresholdSlider.Value = 15;
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would save all settings to a file or registry

            // Get values from UI controls
            bool startWithWindows = StartWithWindowsCheck.IsChecked ?? false;
            bool autoUpdate = AutoUpdateCheck.IsChecked ?? false;
            bool useAnimations = UseAnimationsCheck.IsChecked ?? false;

            // Determine selected refresh rate
            string refreshRate = "Medium";
            if (RefreshSlowOption.IsChecked ?? false) refreshRate = "Slow";
            if (RefreshFastOption.IsChecked ?? false) refreshRate = "Fast";

            // Get notification settings
            bool systemAlerts = SystemAlertsCheck.IsChecked ?? false;
            bool performanceWarnings = PerformanceWarningsCheck.IsChecked ?? false;
            bool updatesNotifications = UpdatesNotificationsCheck.IsChecked ?? false;

            // Get threshold values
            int cpuThreshold = (int)CpuThresholdSlider.Value;
            int memoryThreshold = (int)MemoryThresholdSlider.Value;
            int diskThreshold = (int)DiskThresholdSlider.Value;
            int diskSpaceThreshold = (int)DiskSpaceThresholdSlider.Value;

            // Get privacy settings
            bool usageData = UsageDataCheck.IsChecked ?? false;
            bool crashReports = CrashReportsCheck.IsChecked ?? false;

            // Show a confirmation message
            MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);

            // Update the status text in MainWindow
            if (mainWindow != null)
            {
                // Use reflection to access the method if it's not directly accessible
                var method = mainWindow.GetType().GetMethod("UpdateStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(mainWindow, new object[] { "Settings saved successfully" });
                }

                // Show assistant message
                mainWindow.ShowAssistantMessage("Your settings have been saved. The refresh rate is now set to " + refreshRate +
                                               " and system alerts are " + (systemAlerts ? "enabled" : "disabled") + ".");
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Open the URL in the default browser
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}