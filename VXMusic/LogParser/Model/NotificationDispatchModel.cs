using System.Collections.Generic;
using VXMusic.LogParser.Helpers;

namespace VXMusic.LogParser.Models
{
    class NotificationDispatchModel
    {
        public EventType Type { get; set; }
        public bool WasGrouped { get; set; }
        public List<NotificationDispatchModel> GroupedNotifications { get; set; }

        public NotificationDispatchModel()
        {
            WasGrouped = false;
            GroupedNotifications = new List<NotificationDispatchModel>();
        }
    }
}
