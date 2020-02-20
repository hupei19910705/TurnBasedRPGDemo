using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameRocker : ScrollRect
{
    private float _radius = 0f;

    protected override void Start()
    {
        base.Start();
        _radius = (transform as RectTransform).sizeDelta.x * 0.5f;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        var contentPos = content.anchoredPosition;
        if (contentPos.magnitude > _radius)
        {
            contentPos = contentPos.normalized * _radius;
            SetContentAnchoredPosition(contentPos);
        }
    }
}
