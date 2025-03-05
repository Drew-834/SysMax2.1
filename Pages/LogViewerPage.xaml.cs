using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;
using System.Threading.Tasks;
using SysMax2._1.Services;
using SysMax2._1.Models;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for LogViewerPage.xaml
    /// </summary>
    public partial class LogViewerPage : Page
    {
        private MainWindow mainWindow;
        private LoggingService loggingService = LoggingService.Instance;
        private string currentLogText = "";

        public LogViewerPage()
        {
            InitializeComponent();

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Load logs on page load
            LoadLogs();

            // Show the assistant with helpful information
            if (mainWindow != null)
            {
                mainWindow.ShowAssistantMessage("This page shows system logs that can help diagnose issues. You can filter logs by type, search for specific text, or export logs to share with IT support.");
            }
        }

        private async void LoadLogs()
        {
            try
            {
                // Get logs based on selected time range
                string logs = "";
                if (TimeRangeComboBox.SelectedIndex == 0) // Recent logs
                {
                    logs = loggingService.GetRecentLogs();
                }
                else
                {
                    logs = await loggingService.GetAllLogs();

                    // Apply time filter if not "All logs"
                    if (TimeRangeComboBox.SelectedIndex < 4)
                    {
                        logs = FilterLogsByTime(logs);
                    }
                }

                // Apply level filters
                logs = FilterLogsByLevel(logs);

                // Apply search filter if any
                if (!string.IsNullOrEmpty(SearchBox.Text) && SearchBox.Text != "Search logs...")
                {
                    logs = FilterLogsBySearch(logs, SearchBox.Text);
                }

                // Store the current filtered logs
                currentLogText = logs;

                // Update the text box
                LogTextBox.Text = currentLogText;

                // Scroll to the end
                LogTextBox.ScrollToEnd();
            }
            catch (Exception ex)
            {
                LogTextBox.Text = $"Error loading logs: {ex.Message}";
            }
        }

        private string FilterLogsByTime(string logs)
        {
            StringBuilder filteredLogs = new StringBuilder();
            string[] logLines = logs.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            DateTime startDate = DateTime.Now.Date;

            switch (TimeRangeComboBox.SelectedIndex)
            {
                case 1: // Today
                    startDate = DateTime.Now.Date;
                    break;
                case 2: // Yesterday
                    startDate = DateTime.Now.Date.AddDays(-1);
                    break;
                case 3: // Last 7 days
                    startDate = DateTime.Now.Date.AddDays(-7);
                    break;
            }

            foreach (string line in logLines)
            {
                try
                {
                    // Extract date from log line [YYYY-MM-DD HH:mm:ss]
                    int dateStartIndex = line.IndexOf('[') + 1;
                    int dateEndIndex = line.IndexOf(']', dateStartIndex);
                    if (dateStartIndex > 0 && dateEndIndex > dateStartIndex)
                    {
                        string dateStr = line.Substring(dateStartIndex, dateEndIndex - dateStartIndex);
                        if (DateTime.TryParse(dateStr, out DateTime logDate))
                        {
                            if (TimeRangeComboBox.SelectedIndex == 2) // Yesterday
                            {
                                if (logDate.Date == startDate)
                                {
                                    filteredLogs.AppendLine(line);
                                }
                            }
                            else
                            {
                                if (logDate.Date >= startDate)
                                {
                                    filteredLogs.AppendLine(line);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // If parsing fails, include the line anyway
                    filteredLogs.AppendLine(line);
                }
            }

            return filteredLogs.ToString();
        }

        private string FilterLogsByLevel(string logs)
        {
            StringBuilder filteredLogs = new StringBuilder();
            string[] logLines = logs.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            bool showInfo = InfoFilter.IsChecked ?? true;
            bool showWarning = WarningFilter.IsChecked ?? true;
            bool showError = ErrorFilter.IsChecked ?? true;
            bool showCritical = CriticalFilter.IsChecked ?? true;

            foreach (string line in logLines)
            {
                if (showInfo && line.Contains("[Info]"))
                {
                    filteredLogs.AppendLine(line);
                }
                else if (showWarning && line.Contains("[Warning]"))
                {
                    filteredLogs.AppendLine(line);
                }
                else if (showError && line.Contains("[Error]"))
                {
                    filteredLogs.AppendLine(line);
                }
                else if (showCritical && line.Contains("[Critical]"))
                {
                    filteredLogs.AppendLine(line);
                }
            }

            return filteredLogs.ToString();
        }

        private string FilterLogsBySearch(string logs, string searchText)
        {
            StringBuilder filteredLogs = new StringBuilder();
            string[] logLines = logs.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in logLines)
            {
                if (line.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    filteredLogs.AppendLine(line);
                }
            }

            return filteredLogs.ToString();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();

            // Log the action
            loggingService.Log(LogLevel.Info, "User refreshed log view");
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"SysMax_Logs_Export_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Export either the current filtered view or all logs
                    string logsToExport = LogTextBox.Text;

                    if (string.IsNullOrWhiteSpace(logsToExport))
                    {
                        logsToExport = await loggingService.GetAllLogs();
                    }

                    await File.WriteAllTextAsync(saveFileDialog.FileName, logsToExport);

                    // Log the export action
                    loggingService.Log(LogLevel.Info, $"Logs exported to {saveFileDialog.FileName}");

                    MessageBox.Show("Logs exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error exporting logs: {ex.Message}");
                MessageBox.Show($"Error exporting logs: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadLogs();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text != "Search logs...")
            {
                LoadLogs();
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search logs...")
            {
                SearchBox.Text = string.Empty;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Search logs...";
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(LogTextBox.Text))
                {
                    Clipboard.SetText(LogTextBox.Text);

                    // Log the action
                    loggingService.Log(LogLevel.Info, "User copied logs to clipboard");

                    MessageBox.Show("Logs copied to clipboard!", "Copy Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                loggingService.Log(LogLevel.Error, $"Error copying to clipboard: {ex.Message}");
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();

            // Log the action
            loggingService.Log(LogLevel.Info, "User cleared log display");
        }
    }
}