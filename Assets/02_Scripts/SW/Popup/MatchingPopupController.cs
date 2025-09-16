using TMPro;
using UnityEngine;

public class MatchingPopupController : MonoBehaviour
{
    private static MatchingPopupController instance;

    [SerializeField] private GameObject popupPrefab; // 인스펙터에서 드래그해서 할당
    private static GameObject popupInstance;
    private static TMP_Text countdownText;

    private void Awake()
    {
        instance = this;
    }

    // 팝업 열기
    public static void OpenPopup()
    {
        if (popupInstance != null) return;

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas 없음 → 팝업 생성 실패");
            return;
        }

        if (instance == null || instance.popupPrefab == null)
        {
            Debug.LogError("MatchingPopupController 프리팹이 할당되지 않음");
            return;
        }

        popupInstance = Object.Instantiate(instance.popupPrefab, canvas.transform);

        countdownText = popupInstance.transform.Find("CountdownText").GetComponent<TMP_Text>();
        countdownText.text = "9";
    }

    // 카운트다운 업데이트
    public static void UpdateCountdown(int timeLeft)
    {
        if (countdownText != null)
        {
            countdownText.text = timeLeft.ToString();
        }
        else
        {
            Debug.LogWarning("[UpdateCountdown] countdownText가 null");
        }
    }

    // 팝업 닫기
    public static void ClosePopup()
    {
        if (popupInstance != null)
        {
            Object.Destroy(popupInstance);
            popupInstance = null;
            countdownText = null;
        }
    }
}
