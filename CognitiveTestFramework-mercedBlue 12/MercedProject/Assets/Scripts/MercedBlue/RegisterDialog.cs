namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using UnityEngine.UI;
    using System.Collections;
    using UnityEngine.Networking;
    using TMPro;
    public class RegisterDialog : MonoBehaviour, IParentDialog
    {
        [SerializeField]
        private PrototypeTestController controller;
        [SerializeField]
        private TMP_InputField nameField;
        [SerializeField]
        private TMP_InputField passwordField;
        [SerializeField]
        private GameButton RegisterButton;
        [SerializeField]
        private GameObject introPanel;
        [SerializeField]
        private GameObject registerPanel;

        public void Awake()
        {
            RegisterButton.Init(this, "Create an Account");
        }


        public void OnButtonClick(GameObject sender, string text)
        {
            controller.PlayButtonClickSound();
            StartCoroutine(Register());
        }

        IEnumerator Register()
        {
            WWWForm form = new WWWForm();
            form.AddField("player_name", nameField.text);
            form.AddField("password", passwordField.text);
            using (UnityWebRequest www = UnityWebRequest.Post("http://18.219.193.100:5000/auth/register", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    Debug.Log("Save failed: " + www.error);
                else
                    Debug.Log("User registered");
                    registerPanel.SetActive(false);
                    introPanel.SetActive(true);

            }
        }
    }
}