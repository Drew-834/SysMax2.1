using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SysMax2._1.Models;
using SysMax2._1.Services;

namespace SysMax2._1.Controls
{
    /// <summary>
    /// Active Issues Dashboard Component that can be reused across the application
    /// </summary>
    public partial class ActiveIssuesDashboard : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<IssueInfo> _activeIssues = new ObservableCollection<IssueInfo>();
        private readonly LoggingService _loggingService = LoggingService.Instance;
        private readonly EnhancedHardwareMonitorService _hardwareMonitor;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Event for when an issue fix button is clicked
        public event EventHandler<IssueFixEventArgs>? IssueFixRequested;

        public ObservableCollection<IssueInfo> ActiveIssues
        {
            get => _activeIssues;
            set
            {
                _activeIssues = value;
                OnPropertyChanged(nameof(ActiveIssues));
                OnPropertyChanged(nameof(HasIssues));
            }
        }

        public bool HasIssues => ActiveIssues.Count > 0;

        public ActiveIssuesDashboard()
        {
            InitializeComponent();

            // Set data context to self for binding
            DataContext = this;

            // Get hardware monitor service
            _hardwareMonitor = EnhancedHardwareMonitorService.Instance;

            // Subscribe to hardware events
            SubscribeToHardwareEvents();

            // Check for initial issues
            CheckForInitialIssues();
        }

        private void SubscribeToHardwareEvents()
        {
            // Subscribe to hardware monitor events
            _hardwareMonitor.HighCpuUsageDetected += HardwareMonitor_HighCpuUsageDetected;
            _hardwareMonitor.HighTemperatureDetected += HardwareMonitor_HighTemperatureDetected;
            _hardwareMonitor.HighMemoryUsageDetected += HardwareMonitor_HighMemoryUsageDetected;
            _hardwareMonitor.LowDiskSpaceDetected += HardwareMonitor_LowDiskSpaceDetected;
            _hardwareMonitor.NetworkDisconnected += HardwareMonitor_NetworkDisconnected;
        }

        private void CheckForInitialIssues()
        {
            // Check current system state for issues
            if (_hardwareMonitor.CpuUsage > _hardwareMonitor.HighCpuThreshold)
            {
                AddIssue("HighCPU", "CPU usage is high",
                    $"CPU usage is at {_hardwareMonitor.CpuUsage:F1}%, which may slow down your system.",
                    "Show Details", IssueInfo.Severity.Medium);
            }

            if (_hardwareMonitor.CpuTemperature > _hardwareMonitor.HighTemperatureThreshold)
            {
                AddIssue("HighTemperature", "CPU temperature is high",
                    $"CPU temperature is at {_hardwareMonitor.CpuTemperature:F1}°C, which is above the recommended limit.",
                    "Show Details", IssueInfo.Severity.Medium);
            }

            if (_hardwareMonitor.MemoryUsage > _hardwareMonitor.HighMemoryThreshold)
            {
                AddIssue("HighMemory", "Memory usage is high",
                    $"Memory usage is at {_hardwareMonitor.MemoryUsage:F1}%, which may slow down your system.",
                    "Fix Now", IssueInfo.Severity.Medium);
            }

            if (_hardwareMonitor.AvailableDiskSpace < _hardwareMonitor.LowDiskSpaceThreshold)
            {
                double gbAvailable = _hardwareMonitor.AvailableDiskSpace / (1024.0 * 1024 * 1024);
                AddIssue("DiskSpace", "Disk space is low",
                    $"You have {gbAvailable:F1} GB of disk space remaining, which is below the recommended minimum.",
                    "Fix Now", IssueInfo.Severity.High);
            }

            if (!_hardwareMonitor.IsNetworkConnected)
            {
                AddIssue("NetworkDisconnected", "Network is disconnected",
                    "Your computer is not connected to any network. Check your network settings or Wi-Fi connection.",
                    "Network Settings", IssueInfo.Severity.High);
            }

            // Check for Windows updates
            CheckForWindowsUpdates();
        }

        private void CheckForWindowsUpdates()
        {
            try
            {
                // In a real application, this would check the Windows Update API
                // For now, we'll simulate by checking registry for pending updates
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update"))
                {
                    if (key != null)
                    {
                        var automaticUpdates = key.GetValue("AUOptions");
                        var needReboot = key.GetValue("RebootRequired");

                        if (needReboot != null && Convert.ToBoolean(needReboot))
                        {
                            AddIssue("WindowsUpdate", "Restart required to complete updates",
                                "Windows has installed updates that require a restart to complete installation.",
                                "Restart Now", IssueInfo.Severity.High);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log(LogLevel.Error, $"Error checking for Windows updates: {ex.Message}");
            }
        }

        #region Hardware Event Handlers

        private void HardwareMonitor_HighCpuUsageDetected(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddIssue("HighCPU", "CPU usage is high",
                    $"CPU usage is at {_hardwareMonitor.CpuUsage:F1}%, which may slow down your system.",
                    "Show Details", IssueInfo.Severity.Medium);
            });
        }

        private void HardwareMonitor_HighTemperatureDetected(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddIssue("HighTemperature", "CPU temperature is high",
                    $"CPU temperature is at {_hardwareMonitor.CpuTemperature:F1}°C, which is above the recommended limit.",
                    "Show Details", IssueInfo.Severity.Medium);
            });
        }

        private void HardwareMonitor_HighMemoryUsageDetected(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddIssue("HighMemory", "Memory usage is high",
                    $"Memory usage is at {_hardwareMonitor.MemoryUsage:F1}%, which may slow down your system.",
                    "Fix Now", IssueInfo.Severity.Medium);
            });
        }

        private void HardwareMonitor_LowDiskSpaceDetected(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                double gbAvailable = _hardwareMonitor.AvailableDiskSpace / (1024.0 * 1024 * 1024);
                AddIssue("DiskSpace", "Disk space is low",
                    $"You have {gbAvailable:F1} GB of disk space remaining, which is below the recommended minimum.",
                    "Fix Now", IssueInfo.Severity.High);
            });
        }

        private void HardwareMonitor_NetworkDisconnected(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddIssue("NetworkDisconnected", "Network is disconnected",
                    "Your computer is not connected to any network. Check your network settings or Wi-Fi connection.",
                    "Network Settings", IssueInfo.Severity.High);
            });
        }

        #endregion

        private void AddIssue(string issueType, string title, string description, string buttonText, IssueInfo.Severity severity)
        {
            // Check if this issue type already exists
            foreach (var issue in ActiveIssues)
            {
                if (issue.FixActionTag == issueType)
                {
                    // Issue already exists, just update its description and timestamp
                    issue.Text = description;
                    issue.Timestamp = DateTime.Now;
                    return;
                }
            }

            // If we get here, the issue doesn't exist yet, so add it
            var newIssue = new IssueInfo
            {
                Icon = GetIconForIssueType(issueType),
                Text = description,
                FixButtonText = buttonText,
                FixActionTag = issueType,
                IssueSeverity = severity,
                Timestamp = DateTime.Now
            };

            // Add to the collection
            ActiveIssues.Add(newIssue);

            // Log the new issue
            _loggingService.Log(LogLevel.Warning, $"New issue detected: {title} - {description}");

            // Update property for empty state visibility
            OnPropertyChanged(nameof(HasIssues));
        }

        private string GetIconForIssueType(string issueType)
        {
            switch (issueType)
            {
                case "HighCPU":
                    return "⚙️";
                case "HighTemperature":
                    return "🌡️";
                case "HighMemory":
                    return "📊";
                case "DiskSpace":
                    return "💾";
                case "NetworkDisconnected":
                    return "🌐";
                case "WindowsUpdate":
                    return "🔄";
                default:
                    return "⚠️";
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FixIssue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is IssueInfo issue)
            {
                // Log the action
                _loggingService.Log(LogLevel.Info, $"User requested to fix issue: {issue.FixActionTag}");

                // Raise the event for the parent to handle
                IssueFixRequested?.Invoke(this, new IssueFixEventArgs(issue));
            }
        }

        public void RemoveIssue(string issueType)
        {
            // Find the issue by type and remove it
            for (int i = ActiveIssues.Count - 1; i >= 0; i--)
            {
                if (ActiveIssues[i].FixActionTag == issueType)
                {
                    ActiveIssues.RemoveAt(i);
                    break;
                }
            }

            // Update property for empty state visibility
            OnPropertyChanged(nameof(HasIssues));
        }
    }

    public class IssueFixEventArgs : EventArgs
    {
        public IssueInfo Issue { get; private set; }

        public IssueFixEventArgs(IssueInfo issue)
        {
            Issue = issue;
        }
    }
}