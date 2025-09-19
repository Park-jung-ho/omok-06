using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using Newtonsoft.Json.Linq;

public class RankingSystem : MonoBehaviour
{
    [SerializeField] private GameObject rankPanel;
    [SerializeField] private GameObject[] panels;

    public void OpenRankPanel()
    {
        rankPanel.SetActive(true);
        StartCoroutine(LoadRanking());
    }

    public void CloseRankPanel()
    {
        rankPanel.SetActive(false);
    }

    private IEnumerator LoadRanking()
    {
        string url = $"{Constants.ServerUrl}/ranking";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var json = www.downloadHandler.text;
                var array = JArray.Parse(json);

                for (int i = 0; i < panels.Length; i++)
                {
                    if (i < array.Count)
                    {
                        var data = array[i];
                        string nickname = data["nickname"]?.ToString();
                        int rank = data["rank"]?.Value<int>() ?? 0;
                        int wins = data["wins"]?.Value<int>() ?? 0;
                        int losses = data["losses"]?.Value<int>() ?? 0;

                        // Panel 안의 TMP_Text 4개 가져오기
                        TMP_Text[] texts = panels[i].GetComponentsInChildren<TMP_Text>();

                        texts[0].text = $"{rank}급";
                        texts[1].text = nickname;
                        texts[2].text = $"{wins}승";
                        texts[3].text = $"{losses}패";

                        panels[i].SetActive(true);
                    }
                    else
                    {
                        panels[i].SetActive(false); // 데이터 없으면 패널 숨기기
                    }
                }
            }
        }
    }
}
