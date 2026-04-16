using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro
{
    public class NotificationManager : MonoBehaviour
    {
        [Serializable]
        public struct NotificationType
        {
            public string name;
            public NotificationStyle style;
        }

        [Serializable]
        public struct NotificationStyle
        {
            public Sprite iconImage;
            public Color iconColor;
            public AudioClip sound;
            [Range(0, 1)]
            public float soundVolume;
        }

        public static NotificationManager Instance { get; private set; }

        [SerializeField] GameObject _notificationToastBlueprint;

        [SerializeField] Transform _notificationsContainer;

        public List<NotificationType> NotificationTypes { get { return _notificationTypes; } }
        [SerializeField] List<NotificationType> _notificationTypes;

        [Header("Time Settings")]
        [Space]

        [Tooltip("In seconds")]
        [SerializeField] float _notificationDisplayTime;

        [Tooltip("In seconds")]
        [SerializeField] float _notificationIntervalDisplayTime;

        [Tooltip("In seconds")]
        [SerializeField] float _notificationAnimationTime;

        private Queue<NotificationData> _notifications;
        private VerticalLayoutGroup _verticalLayoutGroup;

        public List<NotificationData> GetNotifications() {
            return _notifications.ToList();
        }

        public void SetNotifications(List<NotificationData> value)
        {
            _notifications = new Queue<NotificationData>(value);
        }

        public NotificationType? GetNotificationType(string name)
        {
            var type = _notificationTypes.Find((notification) => notification.name == name);
            return type;
        }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _notifications = new Queue<NotificationData>();
            _verticalLayoutGroup = _notificationsContainer.gameObject.GetComponent<VerticalLayoutGroup>();
        }

        private IEnumerator RunTriggerAllNotifications()
        {
            while (_notifications.Count > 0)
            {
                TriggerNotification();
                yield return new WaitForSeconds(_notificationIntervalDisplayTime);
            }
        }

        public void AddNotification(string text, string typeName)
        {
            NotificationType? type = _notificationTypes.Find(t => t.name == typeName);
            if (type == null)
                throw new Exception($"[Pyro.Notification] Error: Notification type does not exist. Add {typeName} type to NotificationTypes list in the NotificationManager Script.");

            NotificationData notification = new NotificationData(text, type.Value.style);
            _notifications.Enqueue(notification);
        }

        public void TriggerNotification()
        {
            if (_notifications.TryDequeue(out NotificationData notification)) {
                GameObject notificationInstance = Instantiate(_notificationToastBlueprint, transform);
                if (notificationInstance.TryGetComponent<NotificationToast>(out NotificationToast notificationToast))
                {
                    notificationToast.Init(notification.NotificationText, notification.NotificationStyle, _notificationDisplayTime, _notificationAnimationTime);
                    notificationToast.Show(_notificationsContainer, _verticalLayoutGroup);
                }
            }
        }

        public void TriggerAllNotifications()
        {
            StartCoroutine(RunTriggerAllNotifications());
        }
    }
}

