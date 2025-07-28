namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using TMPro;
    using System.Collections;

    public class PrototypeScoreDialog : MonoBehaviour, IParentDialog
    {
        [SerializeField]
        private PrototypeTestController controller;
        [SerializeField]
        private GameButton QuitButton;
        [SerializeField]
        private TextMeshProUGUI scoreLabel;
        [SerializeField]
        private TextMeshProUGUI maxScoreLabel;
        [SerializeField]
        private float waitToCloseTime = 0.75f;

        public void Awake()
        {
            QuitButton.Init(this, "Quit");
        }

        public void OnButtonClick(GameObject sender, string text) 
        {

            controller.PlayButtonClickSound();
            Debug.Log("Score Dialog Quit Button Clicked");
            QuitButton.Enable(false);
            StartCoroutine(ScorePanelDelay());
        }
        private IEnumerator ScorePanelDelay()
        {
            yield return new WaitForSeconds(waitToCloseTime);
            gameObject.SetActive(false);
            controller.OnScorePanelClosed();
        }

        public void OnEnable()
        {
            if (controller != null)
            {
                scoreLabel.text = controller.totalScore.ToString();
                maxScoreLabel.text = controller.totalPossibleScore.ToString();
            }
        }
    }
}