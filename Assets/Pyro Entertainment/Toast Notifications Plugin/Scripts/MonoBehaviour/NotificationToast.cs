using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Pyro.NotificationManager;

namespace Pyro
{
    [RequireComponent(typeof(AudioSource))]
    public class NotificationToast : MonoBehaviour
    {
        [SerializeField] Image _notificationIcon;
        [SerializeField] TMP_Text _notificationText;
        [SerializeField] Slider _notificationTimeSlider;

        float _notificationTotalTime;
        float _notificationAnimationTime;
        float _currentTime;

        int _initialLayoutTopPosition;

        RectTransform _rectTransform;
        AudioSource _audioSource;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _audioSource = GetComponent<AudioSource>();
            _currentTime = 0;
        }

        void Update()
        {
            _currentTime += Time.deltaTime;
            float progress = _currentTime / _notificationTotalTime;

            _notificationTimeSlider.value = progress;
        }

        public void Init(string notificationText, NotificationStyle style, float notificationTime, float notificationAnimationTime)
        {
            Debug.Log(style.iconImage);
            if (style.iconImage != null)
            {
                _notificationIcon.sprite = style.iconImage;
                _notificationIcon.color = style.iconColor;
                _notificationIcon.enabled = true;
            }
            else
            {
                _notificationIcon.enabled = false;
            }

            if (style.sound != null)
            {
                _audioSource.clip = style.sound;
                _audioSource.volume = style.soundVolume;
                _audioSource.enabled = true;
            }
            else
            {
                _audioSource.enabled = false;
            }
            
            _notificationText.text = notificationText;

            _notificationTotalTime = notificationTime;
            _notificationAnimationTime = notificationAnimationTime;            
        }

        public void Show(Transform container, VerticalLayoutGroup verticalLayoutGroup)
        {
            //NOTE: This is set to appear on the left upper corner
            _initialLayoutTopPosition = verticalLayoutGroup.padding.top;
            _rectTransform.anchoredPosition = new Vector2(-_rectTransform.rect.width, -_initialLayoutTopPosition);

            verticalLayoutGroup.padding.top = (int)Math.Round(_rectTransform.rect.height) + (int)Math.Round(verticalLayoutGroup.spacing);
            StartCoroutine(ShowNotification(container, verticalLayoutGroup));
        }

        IEnumerator ShowNotification(Transform container, VerticalLayoutGroup verticalLayoutGroup)
        {
            Vector2 targetPos = new Vector2(0f, _rectTransform.anchoredPosition.y);

            float duration = _notificationAnimationTime;
            float t = 0;

            Vector2 start = _rectTransform.anchoredPosition;
            while (t < duration)
            {
                t += Time.deltaTime;
                _rectTransform.anchoredPosition = Vector2.Lerp(start, targetPos, t/duration);
                yield return null;
            }

            if (_audioSource.clip != null)
            {
                _audioSource.Play();
            }

            _rectTransform.anchoredPosition = targetPos;
            verticalLayoutGroup.padding.top = _initialLayoutTopPosition;
            transform.SetParent(container);

            StartCoroutine(HideNotification());
        }

        IEnumerator HideNotification()
        {
            yield return new WaitForSeconds(_notificationTotalTime);

            transform.SetParent(transform.parent.parent);
            Vector2 targetPos = new Vector2(-_rectTransform.rect.width, _rectTransform.anchoredPosition.y);

            float duration = _notificationAnimationTime;
            float t = 0;

            Vector2 start = _rectTransform.anchoredPosition;
            while (t < duration)
            {
                t += Time.deltaTime;
                _rectTransform.anchoredPosition = Vector2.Lerp(start, targetPos, t / duration);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
