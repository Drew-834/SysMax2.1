using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for HelpSupportPage.xaml
    /// </summary>
    public partial class HelpSupportPage : Page
    {
        private MainWindow mainWindow;
        private Border currentlyOpenFaq = null;

        public HelpSupportPage()
        {
            InitializeComponent();

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;
        }

        private void SearchHelpBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchHelpBox.Text == "Search for help topics...")
            {
                SearchHelpBox.Text = string.Empty;
            }
        }

        private void SearchHelpBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchHelpBox.Text))
            {
                SearchHelpBox.Text = "Search for help topics...";
            }
        }

        private void ContactSupportButton_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would open a contact form or email
            MessageBox.Show("This would open a contact form in a real application.", "Contact Support",
                MessageBoxButton.OK, MessageBoxImage.Information);

            // Show assistant message
            if (mainWindow != null)
            {
                mainWindow.ShowAssistantMessage("Need help from our support team? You can email support@sysmax.com or call our helpdesk at 1-800-555-0123 during business hours.");
            }
        }

        private void HelpTopic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string topic = button.Tag?.ToString() ?? "";
                ShowHelpForTopic(topic);
            }
        }

        private void ShowHelpForTopic(string topic)
        {
            // In a real application, this would display help content for the selected topic
            // For this demo, we'll just show an assistant message

            if (mainWindow != null)
            {
                switch (topic)
                {
                    case "CPU":
                        mainWindow.ShowAssistantMessage("CPU usage shows how hard your computer's processor is working. High CPU usage (above 80%) for extended periods can cause your computer to slow down or overheat.");
                        break;

                    case "Memory":
                        mainWindow.ShowAssistantMessage("Memory (RAM) issues often manifest as system slowdowns. If you're running low on memory, try closing unused applications or browser tabs to free up resources.");
                        break;

                    case "DiskSpace":
                        mainWindow.ShowAssistantMessage("Low disk space can slow down your computer and prevent updates from installing. Try removing unused programs, emptying the recycle bin, or using the Disk Cleanup tool.");
                        break;

                    case "Network":
                        mainWindow.ShowAssistantMessage("Network problems can be caused by issues with your router, internet service provider, or network adapter. Try restarting your router and checking your connection settings.");
                        break;

                    case "Updates":
                        mainWindow.ShowAssistantMessage("Windows Updates are important for security and performance. You can check for updates by going to Settings > Windows Update or using the quick action button in the Overview page.");
                        break;

                    case "SystemScan":
                        mainWindow.ShowAssistantMessage("System scans check various components of your computer for issues. Running a scan from the Overview page can help identify problems and suggest solutions.");
                        break;

                    default:
                        mainWindow.ShowAssistantMessage("I'm here to help! What would you like to know about this topic?");
                        break;
                }
            }
        }

        private void ToggleFaqItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string contentName)
            {
                var contentBorder = FindName(contentName) as Border;

                if (contentBorder != null)
                {
                    // If we have another FAQ open, close it first
                    if (currentlyOpenFaq != null && currentlyOpenFaq != contentBorder)
                    {
                        AnimateFaqCollapse(currentlyOpenFaq);
                    }

                    // Toggle the clicked FAQ item
                    if (contentBorder.Visibility == Visibility.Visible)
                    {
                        AnimateFaqCollapse(contentBorder);
                        currentlyOpenFaq = null;
                    }
                    else
                    {
                        AnimateFaqExpand(contentBorder);
                        currentlyOpenFaq = contentBorder;
                    }
                }
            }
        }

        private void AnimateFaqExpand(Border contentBorder)
        {
            // First, measure the content to get its natural height
            contentBorder.Visibility = Visibility.Visible;
            contentBorder.Measure(new Size(contentBorder.ActualWidth, double.PositiveInfinity));
            double targetHeight = contentBorder.DesiredSize.Height;

            // Start with height 0
            contentBorder.Height = 0;

            // Animate to the measured height
            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                From = 0,
                To = targetHeight, 
                Duration = TimeSpan.FromMilliseconds(250)
            };

            // After animation completes, set Height to Auto by clearing the local value
            heightAnimation.Completed += (s, e) => contentBorder.ClearValue(FrameworkElement.HeightProperty);

            contentBorder.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
        }

        private void AnimateFaqCollapse(Border contentBorder)
        {
            // Store the current height
            double currentHeight = contentBorder.ActualHeight;

            // Set a fixed height so we can animate it
            contentBorder.Height = currentHeight;

            // Animate collapse
            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                From = currentHeight,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            heightAnimation.Completed += (s, e) => contentBorder.Visibility = Visibility.Collapsed;

            contentBorder.BeginAnimation(FrameworkElement.HeightProperty, heightAnimation);
        }

        private void WatchVideo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string videoId = button.Tag?.ToString() ?? "";

                // In a real application, this would play a video or open a browser
                MessageBox.Show($"This would play the '{videoId}' tutorial video in a real application.",
                    "Video Tutorial", MessageBoxButton.OK, MessageBoxImage.Information);

                // Show assistant message depending on the video
                if (mainWindow != null)
                {
                    switch (videoId)
                    {
                        case "GettingStarted":
                            mainWindow.ShowAssistantMessage("The 'Getting Started' video introduces you to SysMax's main features and shows you how to navigate the interface.");
                            break;

                        case "Optimization":
                            mainWindow.ShowAssistantMessage("The 'Optimization' tutorial explains how to improve your system's performance by managing startup programs, cleaning disk space, and adjusting settings.");
                            break;

                        case "Troubleshooting":
                            mainWindow.ShowAssistantMessage("The 'Troubleshooting' video covers common issues like high CPU usage, memory problems, and network connectivity, with step-by-step solutions.");
                            break;

                        default:
                            mainWindow.ShowAssistantMessage("This video tutorial will guide you through using the selected feature.");
                            break;
                    }
                }
            }
        }
    }
}