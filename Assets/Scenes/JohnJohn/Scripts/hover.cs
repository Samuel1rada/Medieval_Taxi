using UnityEngine;
using UnityEngine.EventSystems;

public class hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public GameObject hoverObject;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverObject.SetActive(true);
        Debug.Log("Pointer entered: " + hoverObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverObject.SetActive(false);
        Debug.Log("Pointer exited: " + hoverObject.name);
    }

    public void OnSelect(BaseEventData eventData)
    {
        hoverObject.SetActive(true);
        Debug.Log("UI element selected (controller): " + hoverObject.name);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        hoverObject.SetActive(false);
        Debug.Log("UI element deselected (controller): " + hoverObject.name);
    }
}
