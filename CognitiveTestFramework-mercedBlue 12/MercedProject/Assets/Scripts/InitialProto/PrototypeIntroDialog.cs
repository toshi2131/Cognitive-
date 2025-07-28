namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using UnityEngine.UI;
    using System.Collections;
    using UnityEngine.Networking;
    using TMPro;
    using static CognitiveTestEngine.Prototype.StartDialog;

    public class PrototypeIntroDialog : MonoBehaviour, IParentDialog
    {
        [SerializeField]
        private PrototypeTestController controller;
        [SerializeField] 
        private TMP_InputField nameField;
        [SerializeField] 
        private TMP_InputField passwordField;
        [SerializeField]
        private GameButton LoginButton;
        [SerializeField]
        private GameButton GuestButton;
        [SerializeField]
        private GameButton RegisterButton;
        [SerializeField]
        private GameObject introPanel;
        [SerializeField]
        private GameObject registerPanel;
        [SerializeField]
        private GameObject startPanel;
        public void Awake()
        {
            LoginButton.Init(this, "Login");
            GuestButton.Init(this, "Continue as Guest");
            RegisterButton.Init(this, "Create an Account");
        }

        public void OnButtonClick(GameObject sender, string text)
        {
            if (sender.name == "LoginButton")
            {
                controller.PlayButtonClickSound();
                StartCoroutine(Login());
            }
            if (sender.name == "RegisterButton")
            {
                controller.PlayButtonClickSound();
                introPanel.SetActive(false);
                registerPanel.SetActive(true);
            }
            if (sender.name == "GuestButton")
            {
                controller.PlayButtonClickSound();
                introPanel.SetActive(false); //gameObject is IntroPanel
                controller.OnIntroClosed();
            }
        }

        IEnumerator Login()
        {
            WWWForm form = new WWWForm();
            form.AddField("player_name", nameField.text);
            form.AddField("password", passwordField.text);

            using (UnityWebRequest www = UnityWebRequest.Post("http://18.219.193.100:5000/auth/login", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Network error: " + www.error);
                }
                else
                {
                    Debug.Log("Server response: " + www.downloadHandler.text);

                    if (www.responseCode == 200)
                    {
                        SessionResponse response = JsonUtility.FromJson<SessionResponse>(www.downloadHandler.text); //retrieve the session ID from the server and set it as the session ID in the controller in the current game
                        controller.token = response.token;
                        controller.sessionID = response.session_id;
                        Debug.Log("Token created");
                        Debug.Log("Token: " + controller.token);
                        Debug.Log("Session ID: " + controller.sessionID);
                        Debug.Log("Logged in!");
                        controller.setPlayerName(nameField.text);
                        introPanel.SetActive(false);
                        startPanel.SetActive(true);
                    }
                    else
                    {
                        Debug.LogWarning("Login failed. Code: " + www.responseCode);
                    }
                }
            }
        }
        [System.Serializable]
        public class SessionResponse
        {
            public string token;
            public int session_id;
        }
    }
}