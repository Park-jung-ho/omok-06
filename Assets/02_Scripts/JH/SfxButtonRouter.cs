using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SfxButtonRouter : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance?.PlaySFX("click");
    }
}
