using System;

namespace SysMax2._1.Models
{
    /// <summary>
    /// Represents a system issue or notification
    /// </summary>
    public class IssueInfo
    {

        // The icon to display for this issue (emoji or character)
        public string Icon { get; set; } = "";


        // The text description of the issue
        public string Text { get; set; } = "";


        // The text to display on the fix button
        public string FixButtonText { get; set; } = "Fix Now";


        // The tag to identify the fix action type
        public string FixActionTag { get; set; } = "";

        
        // The severity level of the issue
        public Severity IssueSeverity { get; set; } = Severity.Medium;

        
        // The timestamp when the issue was detected
       
        public DateTime Timestamp { get; set; } = DateTime.Now;

       
        // Severity levels for issues
       
        public enum Severity
        {
            Low,
            Medium,
            High,
            Critical
        }
    }
}