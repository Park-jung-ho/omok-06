using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UserUIData
{
    public Image image;
    public Image blockImage;
    public TMP_Text nickname;
    public TMP_Text rank;
}
public class ReplayPanel : MonoBehaviour
{
    public UserUIData[] userUIData;
    public Button button;


    public void SetBlock(int idx, Sprite image)
    {
        userUIData[idx].blockImage.sprite = image;
    }
}
