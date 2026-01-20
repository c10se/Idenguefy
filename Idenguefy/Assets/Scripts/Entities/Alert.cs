using System;

namespace Idenguefy.Entities
{
    /// <summary>
    /// Base class representing an alert
    /// Stores:
    /// <list type="number">
    ///     <item><description> ID </description></item>
    ///     <item><description> Message </description></item>
    ///     <item><description> Timestamp </description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Xavier
    /// Version: 1.1
    /// Notes: N/A
    /// </remarks>
    [Serializable]
    public class Alert
    {
        public string AlertID;
        public string Title;
        public string Message;
        //Store as string for JSON compatibility, if not then all alerts will show as "01 Jan 0001, 00:00"
        public string TimestampString;
        public AlertType Type;

        [NonSerialized] 
        private DateTime _timestamp;    //For convenience, no need serialize

        public DateTime Timestamp
        {
            get
            {
                if (_timestamp == default && !string.IsNullOrEmpty(TimestampString))
                    DateTime.TryParse(TimestampString, out _timestamp);
                return _timestamp;
            }
            set
            {
                _timestamp = value;
                TimestampString = _timestamp.ToString("o"); //ISO 8601 format
            }
        }

        public Alert(string alertID, string title, string message, DateTime timestamp, AlertType type)
        {
            AlertID = alertID;
            Title = title;
            Message = message;
            Timestamp = timestamp;
            Type = type;
        }

        // Empty constructor for JsonUtility
        public Alert() { }
    }

    public enum AlertType
    {
        Indoor,
        Outdoor
    }

    ///// <summary>
    ///// Specialized alert triggered when the user or a map pointer is near a dengue cluster
    ///// </summary>
    //public class ProximityAlert : Alert
    //{
    //    public ProximityAlert(string alertID, string message, DateTime timestamp) : base(alertID, message, timestamp) { }
    //}


    ///// <summary>
    ///// Specialized alert triggered when a map pointer tagged with home is near a dengue cluster
    ///// </summary>
    //public class HomeAlert : Alert
    //{
    //    public HomeAlert(string alertID, string message, DateTime timestamp) : base(alertID, message, timestamp) { }
    //}
}



