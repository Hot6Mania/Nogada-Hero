using UnityEngine;
using UnityEngine.EventSystems;

public class Controller : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform background;
    public RectTransform handle;
    public float maxDistance = 80f;

    private Vector2 inputVector = Vector2.zero;

    public Vector2 Direction => inputVector;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos);

        inputVector = pos / maxDistance;
        inputVector = inputVector.magnitude > 1 ? inputVector.normalized : inputVector;

        handle.anchoredPosition = inputVector * maxDistance;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
