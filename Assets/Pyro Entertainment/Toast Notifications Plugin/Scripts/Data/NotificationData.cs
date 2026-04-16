using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Pyro.NotificationManager;

namespace Pyro
{
    public class NotificationData
    {
        public NotificationStyle NotificationStyle { get { return _notificationStyle; } }
        public string NotificationText { get { return _notificationText; } }

        private NotificationStyle _notificationStyle;
        private string _notificationText;

        public NotificationData(string notificationText, NotificationStyle NotificationStyle)
        {
            _notificationStyle = NotificationStyle;
            _notificationText = notificationText;
        }
    }
}
