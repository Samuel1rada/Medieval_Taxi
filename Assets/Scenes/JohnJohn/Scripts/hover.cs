using UnityEngine;
using UnityEngine.EventSystems;

public class hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
}
