using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Wird zur Laufzeit von PetShopHandler auf jede Pet-Karte gesetzt.
// Hover/Select → spielt die Ready-Animation als Sprite-Cycle auf dem Icon-Image.
public class PetCardHoverTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler,       IDeselectHandler
{
    private float        _fps = 8f;
    private Image        _icon;
    private List<Sprite> _frames;
    private Sprite       _original;
    private Coroutine    _anim;

    public void Init(Image icon, List<Sprite> frames)
    {
        _icon     = icon;
        _frames   = frames;
        _original = icon != null ? icon.sprite : null;
    }

    public void OnPointerEnter(PointerEventData _) => Play();
    public void OnPointerExit(PointerEventData _)  => Stop();
    public void OnSelect(BaseEventData _)           => Play();
    public void OnDeselect(BaseEventData _)         => Stop();

    void Play()
    {
        if (_frames == null || _frames.Count == 0 || _icon == null) return;
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate());
    }

    void Stop()
    {
        if (_anim != null) { StopCoroutine(_anim); _anim = null; }
        if (_icon != null) _icon.sprite = _original;
    }

    IEnumerator Animate()
    {
        var wait = new WaitForSeconds(1f / _fps);
        for (int i = 0; ; i++)
        {
            _icon.sprite = _frames[i % _frames.Count];
            yield return wait;
        }
    }
}
