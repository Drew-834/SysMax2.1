﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Linq;

using SysMax2._1.Pages;
using SysMax2._1.Models;


#nullable disable

public static class LogicalTreeHelperExtensions
{
    public static IEnumerable<T> FindLogicalChildren<T>(this DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
            {
                if (rawChild is DependencyObject child)
                {
                    if (child is T tChild)
                        yield return tChild;

                    foreach (T childOfChild in child.FindLogicalChildren<T>())
                        yield return childOfChild;
                }
            }
        }
    }
}
namespace SysMax2._1
{
    public partial class MainWindow : Window
    {
        // Current active page/view
        private string currentPage = "Overview";

        // User mode
        private string currentUserMode = "Basic";

        // Simple timer to update system info in status bar
        private DispatcherTimer statusUpdateTimer;

        // Assistant service for helpful messages
        private AssistantService assistantService;

        // Flag to remember if assistant panel is open
        private bool isAssistantPanelOpen = false;



        public MainWindow()
        {
            InitializeComponent();

            // Initialize status update timer
            InitializeStatusTimer();

            // Initialize the assistant service
            assistantService = new AssistantService();

            // Default to Overview page on startup
            NavigateToPage("Overview");

            // Start with assistant button visible but panel hidden
            AssistantPanel.Visibility = Visibility.Collapsed;
            AssistantContainer.Visibility = Visibility.Visible;
            isAssistantPanelOpen = false;

            // Set default status
            UpdateStatus("Application started. System ready.");
        }

        private void InitializeStatusTimer()
        {
            statusUpdateTimer = new DispatcherTimer();
            statusUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
            statusUpdateTimer.Start();
        }

        private void StatusUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Update CPU and RAM usage in status bar
            try
            {
                // In a real implementation, use LibreHardwareMonitorLib to get actual values
                // This is a simplified placeholder
                var process = Process.GetCurrentProcess();
                var cpuUsage = new Random().Next(1, 100); // Placeholder
                var memoryUsageInMB = process.WorkingSet64 / 1024 / 1024;
                var totalMemoryInMB = (long)(new PerformanceCounter("Memory", "Available MBytes").NextValue() * 1.5);
                var memoryPercentage = (int)(memoryUsageInMB * 100 / totalMemoryInMB);

                // Update UI
                
                // TODO -------------------------------------------------------
                //CPUUsageStatus.Text = $"CPU: {cpuUsage}%";
                //MemoryUsageStatus.Text = $"RAM: {memoryPercentage}%";

                //// Color coding for high usage
                //CPUUsageStatus.Foreground = cpuUsage > 80 ? new SolidColorBrush(Colors.OrangeRed) :
                //                          (cpuUsage > 60 ? new SolidColorBrush(Colors.Orange) :
                //                          (SolidColorBrush)Resources["MutedTextColor"]);

                //MemoryUsageStatus.Foreground = memoryPercentage > 80 ? new SolidColorBrush(Colors.OrangeRed) :
                //                             (memoryPercentage > 60 ? new SolidColorBrush(Colors.Orange) :
                //                             (SolidColorBrush)Resources["MutedTextColor"]);

                // Check for concerning system states and update assistant in Basic mode
                if (currentUserMode == "Basic" && isAssistantPanelOpen)
                {
                    // Only show auto-updates when user is in the Overview page
                    if (currentPage == "Overview")
                    {
                        // Check for high resource usage
                        if (cpuUsage > 90 && memoryPercentage > 80)
                        {
                            // Both CPU and memory are very high
                            UpdateAssistantMessage(assistantService.GetMessage("HighCPU") + "\n\n" +
                                                  assistantService.GetMessage("HighMemory"));
                        }
                        else if (cpuUsage > 90)
                        {
                            // Just CPU is very high
                            UpdateAssistantMessage(assistantService.GetMessage("HighCPU"));
                        }
                        else if (memoryPercentage > 90)
                        {
                            // Just memory is very high
                            UpdateAssistantMessage(assistantService.GetMessage("LowMemory"));
                        }
                        else if (memoryPercentage > 80)
                        {
                            // Memory is moderately high
                            UpdateAssistantMessage(assistantService.GetMessage("HighMemory"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                Debug.WriteLine($"Error updating status: {ex.Message}");
            }
        }

        private void Navigation_Click(object sender, RoutedEventArgs e)
        {
            // Get the name of the clicked navigation item
            if (sender is Button button)
            {
                string pageName = button.Name.Replace("Nav", "");
                NavigateToPage(pageName);
            }
        }

        private void UserToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                string userMode = toggleButton.Tag.ToString();

                // Set the appropriate toggle states
                BasicUserToggle.IsChecked = (userMode == "Basic");
                ITProToggle.IsChecked = (userMode == "Pro");

                // Store the current user mode
                currentUserMode = userMode;

                // Update interface for the selected user mode
                ApplyUserModeInterface(userMode);

                // Update status text
                UpdateStatus($"Switched to {userMode} user mode");
            }
        }

        private void ApplyUserModeInterface(string userMode)
        {
            // In a real implementation, show/hide elements based on the user mode
            // Here we're just showing different behaviors based on mode

            // For this example:
            // - Basic mode: Show assistant, simpler UI
            // - Pro mode: Hide assistant by default, more detailed UI

            if (userMode == "Basic")
            {
                // Basic mode - show assistant
                AssistantContainer.Visibility = Visibility.Visible;

                // If we're on the Overview page, switch to the Basic version
                if (currentPage == "Overview")
                {
                    NavigateToPage("Overview");
                }

                // Update assistant message
                if (isAssistantPanelOpen)
                {
                    UpdateAssistantMessage("I'm here to help! The Basic mode shows you simplified information about your computer's health.");
                }
            }
            else if (userMode == "Pro")
            {
                // Pro mode - hide assistant by default
                CloseAssistantPanel();

                // If we're on the Overview page, switch to the Pro version
                if (currentPage == "Overview")
                {
                    NavigateToPage("Overview");
                }
            }
        }

        private void AssistantButton_Click(object sender, RoutedEventArgs e)
        {
            if (isAssistantPanelOpen)
            {
                CloseAssistantPanel();
            }
            else
            {
                OpenAssistantPanel();
            }
        }

        private void CloseAssistant_Click(object sender, RoutedEventArgs e)
        {
            CloseAssistantPanel();
        }

        private void OpenAssistantPanel()
        {
            // Show the assistant panel with animation
            AssistantPanel.Visibility = Visibility.Visible;

            // Create and start the animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = (Duration)Resources["TransitionDuration"]
            };

            AssistantPanel.BeginAnimation(OpacityProperty, animation);

            // Update the assistant message based on current page and system state
            UpdateAssistantBasedOnContext();

            isAssistantPanelOpen = true;
        }

        private void CloseAssistantPanel()
        {
            // Hide the assistant panel with animation
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = (Duration)Resources["TransitionDuration"]
            };

            animation.Completed += (s, e) => AssistantPanel.Visibility = Visibility.Collapsed;

            AssistantPanel.BeginAnimation(OpacityProperty, animation);

            isAssistantPanelOpen = false;
        }

        private void UpdateAssistantMessage(string message)
        {
            // Update the assistant message text with animation
            // First fade out
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150)
            };

            fadeOut.Completed += (s, e) =>
            {
                // Change text and fade back in
                AssistantMessage.Text = message;

                DoubleAnimation fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(150)
                };

                AssistantMessage.BeginAnimation(OpacityProperty, fadeIn);
            };

            AssistantMessage.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void UpdateAssistantBasedOnContext()
        {
            // Get system metrics (in a real app, these would be real values)
            Random random = new Random();
            int cpuUsage = int.Parse(CPUUsageStatus.Text.Replace("CPU: ", "").Replace("%", ""));
            int memoryUsage = int.Parse(MemoryUsageStatus.Text.Replace("RAM: ", "").Replace("%", ""));
            int diskSpace = 100 - random.Next(70, 95); // Simulate available disk space
            bool isNetworkConnected = true; // Assume connected

            // If we're on a specific page, show relevant info for that page
            if (currentPage == "CPU" || currentPage == "Memory" || currentPage == "Storage" ||
                currentPage == "Network" || currentPage == "Diagnostics" || currentPage == "Optimization")
            {
                UpdateAssistantMessage(assistantService.GetMessage(currentPage));
            }
            else
            {
                // Otherwise show context-aware help based on system state
                string contextMessage = assistantService.GetContextAwareMessage(
                    cpuUsage, memoryUsage, diskSpace, isNetworkConnected);

                UpdateAssistantMessage(contextMessage);
            }
        }

        /// <summary>
        /// Public method that can be called from other pages to show specific assistant messages
        /// </summary>
        /// <param name="message">The message to display</param>
        public void ShowAssistantMessage(string message)
        {
            // Make sure assistant is visible in Basic mode
            if (currentUserMode == "Basic")
            {
                AssistantContainer.Visibility = Visibility.Visible;

                // Open the panel if it's not already open
                if (!isAssistantPanelOpen)
                {
                    OpenAssistantPanel();
                }

                // Update the message
                UpdateAssistantMessage(message);
            }
        }

        private Page CreatePlaceholderPage(string pageName)
        {
            // Create a simple page with appropriate content
            Page page = new Page();

            // Create the root grid
            Grid grid = new Grid();
            grid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E1E"));

            // Create a stackpanel for content
            StackPanel content = new StackPanel();
            content.VerticalAlignment = VerticalAlignment.Center;
            content.HorizontalAlignment = HorizontalAlignment.Center;

            // Add title
            TextBlock title = new TextBlock();
            title.Text = $"{pageName} Page";
            title.FontSize = 28;
            title.Foreground = new SolidColorBrush(Colors.White);
            title.Margin = new Thickness(0, 0, 0, 20);
            title.HorizontalAlignment = HorizontalAlignment.Center;
            content.Children.Add(title);

            // Add message
            TextBlock message = new TextBlock();
            message.Text = $"This {pageName.ToLower()} functionality is coming soon.\nCheck back in a future update!";
            message.FontSize = 16;
            message.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));
            message.TextAlignment = TextAlignment.Center;
            content.Children.Add(message);

            // Add icon (could be customized based on page type)
            string iconEmoji = "🔍"; // Default

            switch (pageName)
            {
                case "CPU":
                    iconEmoji = "⚙️";
                    break;
                case "Memory":
                    iconEmoji = "📊";
                    break;
                case "Storage":
                    iconEmoji = "💾";
                    break;
                case "Diagnostics":
                    iconEmoji = "🔧";
                    break;
                case "Optimization":
                    iconEmoji = "⚡";
                    break;
                case "Settings":
                    iconEmoji = "⚙️";
                    break;
                case "Help":
                    iconEmoji = "❓";
                    break;
            }

            TextBlock icon = new TextBlock();
            icon.Text = iconEmoji;
            icon.FontSize = 72;
            icon.Margin = new Thickness(0, 30, 0, 30);
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            content.Children.Add(icon);

            // Add to grid
            grid.Children.Add(content);

            // Set as page content
            page.Content = grid;

            return page;
        }

        public void UpdateStatus(string message)
        {
            if (StatusText != null)
            {
                StatusText.Text = message;
            }
        }

        // Method to navigate to specific pages
        public void NavigateToPage(string pageName)
        {
            // Store the current page
            currentPage = pageName;

            // Reset all navigation buttons' styles
            // Find all buttons that start with "Nav" and reset their styles
            foreach (var navButton in this.FindLogicalChildren<Button>()
                                     .Where(b => b.Name != null && b.Name.StartsWith("Nav")))
            {
                navButton.Background = Brushes.Transparent;
                navButton.Foreground = (SolidColorBrush)Resources["MutedTextColor"];
            }

            // Highlight the selected navigation item
            if (FindName($"Nav{pageName}") is Button selectedButton)
            {
                selectedButton.Background = new SolidColorBrush(Color.FromArgb(50, 52, 152, 219));
                selectedButton.Foreground = (SolidColorBrush)Resources["LightTextColor"];
            }

            // Update status text
            UpdateStatus($"Viewing {pageName} page");

            // Update the assistant message if the panel is open
            if (isAssistantPanelOpen)
            {
                UpdateAssistantMessage(assistantService.GetMessage(pageName));
            }

            // Navigate to the appropriate page
            Page newPage = null;

            switch (pageName)
            {
                case "Overview":
                    // Create the appropriate version of the Overview page based on user mode
                    if (currentUserMode == "Basic")
                    {
                        newPage = new Pages.SystemOverviewBasicPage();
                    }
                    else
                    {
                        newPage = new Pages.SystemOverviewPage();
                    }
                    break;

                case "SystemInfo":
                    // Create System Information Page
                    newPage = new Pages.SystemInfoPage();
                    break;

                case "Logs":
                    // Create Log Viewer Page
                    newPage = new Pages.LogViewerPage();
                    break;

                case "Issues":
                    // Create Issue Details Page with default issue type
                    newPage = new Pages.IssueDetailsPage("DiskSpace");
                    break;

                case "Settings":
                    // Create ApplicationSettingsPage
                    newPage = new Pages.ApplicationSettingsPage();
                    break;

                case "Help":
                    // Create HelpSupportPage
                    newPage = new Pages.HelpSupportPage();
                    break;

                case "Network":
                case "CPU":
                case "Memory":
                case "Storage":
                case "Diagnostics":
                case "Optimization":
                    // For now, use placeholders for these pages
                    newPage = CreatePlaceholderPage(pageName);
                    break;

                default:
                    // Default to Overview if unknown page
                    if (currentUserMode == "Basic")
                    {
                        newPage = new Pages.SystemOverviewBasicPage();
                    }
                    else
                    {
                        newPage = new Pages.SystemOverviewPage();
                    }
                    break;
            }

            // Apply fade transition
            if (newPage != null)
            {
                // Fade out current content if there is any
                if (MainContentFrame.Content != null)
                {
                    DoubleAnimation fadeOut = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(150)
                    };

                    fadeOut.Completed += (s, e) =>
                    {
                        // Navigate to new page
                        MainContentFrame.Navigate(newPage);

                        // Fade in new content
                        DoubleAnimation fadeIn = new DoubleAnimation
                        {
                            From = 0,
                            To = 1,
                            Duration = TimeSpan.FromMilliseconds(200)
                        };

                        MainContentFrame.BeginAnimation(OpacityProperty, fadeIn);
                    };

                    MainContentFrame.BeginAnimation(OpacityProperty, fadeOut);
                }
                else
                {
                    // No current content, just navigate and fade in
                    MainContentFrame.Navigate(newPage);

                    DoubleAnimation fadeIn = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(200)
                    };

                    MainContentFrame.BeginAnimation(OpacityProperty, fadeIn);
                }
            }
        }
    


protected override void OnClosing(CancelEventArgs e)
        {
            // Clean up resources when closing
            statusUpdateTimer.Stop();
            base.OnClosing(e);
        }
    }
}