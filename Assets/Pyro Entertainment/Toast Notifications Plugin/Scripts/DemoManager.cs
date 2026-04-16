using Pyro;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pyro
{
    public class DemoManager : MonoBehaviour
    {
        [SerializeField] Button _addNotificationBt;
        [SerializeField] Button _triggerNotificationBt;
        [SerializeField] Button _triggerAllNotificationBt;

        [SerializeField] TMP_Dropdown _notificationTypesDropdown;

        [SerializeField] TMP_InputField _notificationTextInput;

        // Start is called before the first frame update
        void Start()
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            NotificationManager.Instance.NotificationTypes.ForEach(type => options.Add(new TMP_Dropdown.OptionData(type.name, type.style.iconImage, Color.white)));
            _notificationTypesDropdown.AddOptions(options);

            _addNotificationBt.onClick.AddListener(() =>
            {
                NotificationManager.Instance.AddNotification(_notificationTextInput.text, _notificationTypesDropdown.options[_notificationTypesDropdown.value].text);
                _notificationTextInput.text = string.Empty;
            });

            _triggerNotificationBt.onClick.AddListener(() =>
            {
                NotificationManager.Instance.TriggerNotification();
            });

            _triggerAllNotificationBt.onClick.AddListener(() =>
            {
                NotificationManager.Instance.TriggerAllNotifications();
            });
        }
    }
}
