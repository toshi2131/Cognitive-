namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using UnityEngine.UI;
    using System.Collections;
    using UnityEngine.Networking;
    using NUnit.Framework.Internal;

    public class StartDialog : MonoBehaviour, IParentDialog
    {
        [SerializeField]
        private PrototypeTestController controller;
        [SerializeField]
        private GameButton StartButton;
        [SerializeField]
        private GameButton HistoryButton;
        [SerializeField]
        private GameObject startPanel;
        [SerializeField]
        private GameObject userPanel;
        public void Awake()
        {
            StartButton.Init(this, "START");
            HistoryButton.Init(this, "History");
        }


        public void OnButtonClick(GameObject sender, string text)
        {
            if (sender.name == "StartButton")
            {
                controller.PlayButtonClickSound();
                StartCoroutine(StartGame());
            }
            if (sender.name == "HistoryButton")
            {
                controller.PlayButtonClickSound();
                startPanel.SetActive(false);
                userPanel.SetActive(true);
            }
        }
        IEnumerator StartGame()
        {
            WWWForm form = new WWWForm();
            form.AddField("player_name", controller.PlayerName);
            form.AddField("token", controller.token);
            using (UnityWebRequest www = UnityWebRequest.Post("http://18.219.193.100:5000/session", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Session failed: " + www.error);

                }
                else
                {
                    SessionResponse response = JsonUtility.FromJson<SessionResponse>(www.downloadHandler.text); //retrieve the session ID from the server and set it as the session ID in the controller in the current game
                    controller.sessionID = response.session_id;
                    Debug.Log("Session created");
                    Debug.Log("Session ID: " + controller.sessionID);

                    startPanel.SetActive(false);
                    controller.startTimer();
                    controller.OnIntroClosed();
                }
            }
        }
        [System.Serializable]
        public class SessionResponse
        {
            public int session_id;
        }
    }
}