using System;
using System.Collections.Generic;
using System.Text;
#nullable disable

namespace SysMax2._1Services
{
    /// 
    /// Service that provides helpful assistance messages for non-IT users
    /// 
    public class AssistantService
    {
        private readonly Dictionary<string, string> assistantMessages;

        public AssistantService()
        {
            // Initialize assistant messages
            assistantMessages = new Dictionary<string, string>
            {
                // General system messages
                ["Welcome"] = "Welcome to SysMax! I'm your system assistant. I'll help you understand what's happening with your computer and provide suggestions when issues are detected.",

                ["SystemOverview"] = "This overview shows you the key metrics of your system at a glance. Green indicators are good, yellow mean attention might be needed, and red indicates issues that should be addressed.",

                // CPU-related messages
                ["HighCPU"] = "Your CPU (the brain of your computer) is working very hard right now. This could be because of a demanding program or many programs running at once. If your computer feels slow, try closing some applications.",

                ["CPUTemperature"] = "Your CPU temperature is higher than normal. This might make your computer run slower to protect itself. Make sure your computer has proper ventilation and isn't covered or in direct sunlight.",

                // Memory-related messages
                ["HighMemory"] = "Your computer's memory (RAM) is heavily used right now. This could make your computer feel sluggish. Try closing some applications or browser tabs to free up memory.",

                ["LowMemory"] = "You're running low on available memory. This will slow down your computer significantly. Try closing some programs, especially those you're not actively using.",

                // Storage-related messages
                ["LowDiskSpace"] = "Your hard drive is running out of space. This can slow down your computer and prevent updates from installing. Try removing unused programs, emptying the recycle bin, or using Disk Cleanup.",

                ["DiskHealth"] = "One of your storage drives may have health issues. It's a good idea to back up your important files soon, just to be safe.",

                // Network-related messages
                ["NetworkDisconnected"] = "Your network connection appears to be down. Check that your Wi-Fi is turned on, you're within range of your router, and that other devices can connect to the internet.",

                ["WeakWiFi"] = "Your Wi-Fi signal is weak. Try moving closer to your router, or consider using a wired connection or Wi-Fi extender for better performance.",

                ["IPNotConfigured"] = "Your IP address isn't properly configured, which means your computer can't connect to the internet. Try restarting your router or using the 'Repair Connection' option in the Network tools.",

                // Update-related messages
                ["UpdatesAvailable"] = "Important updates are available for your computer. These updates often fix security issues and improve performance. It's a good idea to install them when you're not in the middle of important work.",

                ["UpdatesRequired"] = "Your computer needs critical updates that help protect it from security threats. I recommend installing these updates as soon as possible.",

                // Optimization-related messages
                ["StartupPrograms"] = "You have many programs starting automatically with Windows. This can make your computer boot slower. Consider disabling ones you don't need to start automatically.",

                ["TempFiles"] = "Your computer has accumulated temporary files that are taking up space. Running the Disk Cleanup tool can help remove these files safely.",

                ["Fragmentation"] = "Your hard drive might benefit from defragmentation, which reorganizes files to make them load faster. This only applies to traditional hard drives, not SSDs (solid state drives).",

                // Hardware-related messages
                ["BatteryLow"] = "Your battery is running low. Consider plugging in your laptop soon to avoid losing your work.",

                ["BatteryHealth"] = "Your battery's overall health has declined. It may not hold a charge as long as when it was new. This is normal as batteries age.",

                // Security-related messages
                ["AntivirusDisabled"] = "Your antivirus protection appears to be disabled. This leaves your computer vulnerable to threats. Consider enabling it for better security.",

                ["FirewallDisabled"] = "Your firewall is turned off. The firewall helps protect your computer from unauthorized access. It's recommended to keep it enabled.",

                // Performance-related messages
                ["HighDiskActivity"] = "Your disk is very active right now. This might be caused by background processes like updates or backups. Your computer might feel slower until this activity completes.",

                ["GPUHighUsage"] = "Your graphics card is working hard right now. This is normal when playing games or editing videos, but might indicate a problem if you're not doing graphics-intensive tasks.",

                // Tool explanations
                ["DiskCleanupTool"] = "Disk Cleanup is a safe tool that removes temporary files, system files that are no longer needed, and empties the Recycle Bin to free up disk space.",

                ["DefragTool"] = "Defragmentation reorganizes files on traditional hard drives to make them load faster. It's not necessary for SSDs (solid state drives), which work differently.",

                ["NetworkDiagnostics"] = "Network Diagnostics checks your internet connection and attempts to identify and fix common problems automatically.",

                ["SystemScan"] = "A System Scan checks various components of your computer for issues and provides recommendations for fixing them.",

                // Error explanations
                ["BlueScreen"] = "A blue screen error (also called BSOD) happens when Windows encounters a critical problem and needs to restart. If this happens often, it might indicate hardware problems or driver issues.",

                ["ProgramNotResponding"] = "When a program stops responding or 'freezes', it might be processing a complex task or encountered a problem. You can often close it using Task Manager if needed.",

                ["SlowComputer"] = "A slow computer can be caused by many factors: running too many programs at once, low disk space, old hardware, or malware. The System Scan can help identify specific causes.",

                // Maintenance advice
                ["RegularMaintenance"] = "Regular maintenance like removing unused programs, keeping Windows updated, and backing up important files can help keep your computer running smoothly.",

                ["BackupReminder"] = "Remember to back up your important files regularly. You can use Windows Backup, cloud storage, or an external drive to keep copies of your files safe.",

                // Specific tool outputs
                ["PingExplanation"] = "Ping measures how quickly your computer can communicate with a server on the internet. Lower numbers (measured in milliseconds) are better. High ping times can cause delays in online games and video calls.",

                ["DNSExplanation"] = "DNS (Domain Name System) converts website names to IP addresses that computers use to find websites. Problems with DNS can prevent you from accessing websites even when your internet connection is working.",

                ["TraceRouteExplanation"] = "Traceroute shows the path that information takes to reach a website. This can help identify where connection problems might be occurring between your computer and the website.",

                ["SpeedTestExplanation"] = "A speed test measures your internet connection's download speed (how quickly you can receive data), upload speed (how quickly you can send data), and latency (how quickly you get a response). Higher download/upload speeds and lower latency are better."
            };
        }

        /// <summary>
        /// Gets an assistant message by key
        /// </summary>
        /// <param name="key">The message key</param>
        /// <returns>The assistant message, or a default message if the key is not found</returns>
        public string GetMessage(string key)
        {
            if (assistantMessages.TryGetValue(key, out string message))
            {
                return message;
            }

            return "I'm here to help you understand your system better. What would you like to know?";
        }

        /// <summary>
        /// Generates context-aware messages based on system state
        /// </summary>
        /// <param name="cpuUsage">Current CPU usage percentage</param>
        /// <param name="memoryUsage">Current memory usage percentage</param>
        /// <param name="diskSpace">Available disk space percentage</param>
        /// <param name="isNetworkConnected">Whether the network is connected</param>
        /// <returns>A relevant assistant message based on the current system state</returns>
        public string GetContextAwareMessage(int cpuUsage, int memoryUsage, int diskSpace, bool isNetworkConnected)
        {
            // Check for issues in priority order (most critical first)
            if (!isNetworkConnected)
            {
                return GetMessage("NetworkDisconnected");
            }

            if (diskSpace < 10)
            {
                return GetMessage("LowDiskSpace");
            }

            if (memoryUsage > 90)
            {
                return GetMessage("LowMemory");
            }

            if (cpuUsage > 90)
            {
                return GetMessage("HighCPU");
            }

            if (memoryUsage > 80)
            {
                return GetMessage("HighMemory");
            }

            if (diskSpace < 15)
            {
                return GetMessage("LowDiskSpace");
            }

            // If no issues, return a general message
            return GetMessage("SystemOverview");
        }

        /// <summary>
        /// Gets a detailed explanation for a specific diagnostic tool
        /// </summary>
        /// <param name="toolName">The name of the diagnostic tool</param>
        /// <returns>An explanation of what the tool does and how to interpret results</returns>
        public string GetToolExplanation(string toolName)
        {
            switch (toolName.ToLower())
            {
                case "diskcleanup":
                    return GetMessage("DiskCleanupTool");

                case "defrag":
                    return GetMessage("DefragTool");

                case "networkdiagnostics":
                    return GetMessage("NetworkDiagnostics");

                case "systemscan":
                    return GetMessage("SystemScan");

                case "ping":
                    return GetMessage("PingExplanation");

                case "dns":
                    return GetMessage("DNSExplanation");

                case "traceroute":
                    return GetMessage("TraceRouteExplanation");

                case "speedtest":
                    return GetMessage("SpeedTestExplanation");

                default:
                    return $"This tool helps diagnose and possibly fix issues with your computer's {toolName.ToLower()} system.";
            }
        }

        /// <summary>
        /// Provides an explanation for a specific error
        /// </summary>
        /// <param name="errorType">The type of error encountered</param>
        /// <returns>A user-friendly explanation of the error</returns>
        public string ExplainError(string errorType)
        {
            switch (errorType.ToLower())
            {
                case "bluescreen":
                    return GetMessage("BlueScreen");

                case "notresponding":
                    return GetMessage("ProgramNotResponding");

                case "slow":
                    return GetMessage("SlowComputer");

                case "ipconfig":
                    return GetMessage("IPNotConfigured");

                default:
                    return $"This error might be temporary. If it continues to happen, running a System Scan might help identify the cause.";
            }
        }
    }
}