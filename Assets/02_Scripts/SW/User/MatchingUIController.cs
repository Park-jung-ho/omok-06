using UnityEngine;

public class MatchingUIController : MonoBehaviour
{
    public void OnClickMulti()
    {
        MatchingManager.Instance.OnClickMultiPlay();
    }

    public void OnClickCancel()
    {
        MatchingManager.Instance.CancelMatching();
    }
}
