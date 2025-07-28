namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using UnityEngine.UI;
    using System.Collections;
    using UnityEngine.Networking;
    using TMPro;
    using System;

    public class UserDialog : MonoBehaviour, IParentDialog
    {
        [SerializeField]
        private PrototypeTestController controller;
        [SerializeField]
        private GameButton BackButton;
        [SerializeField]
        private GameObject RowContentPrefab;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private GameObject StartPanel;
        [SerializeField]
        private GameObject UserPanel;

        public void OnEnable()
        {
            BackButton.Init(this, "Back");
            StartCoroutine(UserInfo());
        }


        public void OnButtonClick(GameObject sender, string text)
        {
            controller.PlayButtonClickSound();
            UserPanel.SetActive(false);
            StartPanel.SetActive(true);
        }
        IEnumerator UserInfo()
        {
            UnityWebRequest request = UnityWebRequest.Get("http://18.219.193.100:5000/scores");
            request.SetRequestHeader("Authorization", "Bearer " + controller.PlayerName);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + request.error);
            }
            else
            {
                int count = 1;
                string json = "{\"results\":" + request.downloadHandler.text + "}";
                UserResultsList resultList = JsonUtility.FromJson<UserResultsList>(json);
                foreach (UserResults result in resultList.results)
                {
                    if (!string.IsNullOrEmpty(result.total_score) && !string.IsNullOrEmpty(result.max_score) && !string.IsNullOrEmpty(result.date_created) && !string.IsNullOrEmpty(result.test_duration))
                    {
                        GameObject row = Instantiate(RowContentPrefab, content);
                        row.transform.Find("Index").GetComponent<TextMeshProUGUI>().text = count.ToString();
                        row.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = result.total_score + "/" + result.max_score;
                        DateTime dateTime = DateTime.Parse(result.date_created);
                        string formattedDate = dateTime.ToString("dd MMM yyyy");
                        row.transform.Find("DateTaken").GetComponent<TextMeshProUGUI>().text = formattedDate;

                        float duration = float.Parse(result.test_duration);
                        int minutes = Mathf.FloorToInt(duration / 60f);
                        int seconds = Mathf.FloorToInt(duration % 60f);
                        row.transform.Find("TestDuration").GetComponent<TextMeshProUGUI>().text = $"{minutes}m {seconds}s";

                        count++;
                    }
                }
            }
        }
        [System.Serializable]
        public class UserResults
        {
            public string total_score;
            public string date_created;
            public string max_score;
            public string test_duration;
        }
        [System.Serializable]
        public class UserResultsList
        {
            public UserResults[] results;
        }
    }
}