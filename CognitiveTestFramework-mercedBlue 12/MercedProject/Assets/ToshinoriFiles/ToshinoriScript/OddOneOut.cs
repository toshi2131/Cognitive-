using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace CognitiveTestEngine.Core
{
    public class OddOneOutGame : AbstractTestGame
    {
        [Header("Card Setup")]
        [SerializeField] private List<Sprite> cardOptions; // 0: Diamond, 1: Heart, 2: Club, 3: Spade
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform gridParent;

        [Header("UI")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button continueButton;

        private List<Button> cardButtons = new List<Button>();
        private int oddCardIndex;
        private Sprite commonCard;
        private Sprite oddCard;

        private CognitiveTestController controller;
        private UIManager uiManager;

        public override void StartGame(CognitiveTestController controller)
        {
            this.controller = controller;

            if (titlePanel != null) titlePanel.SetActive(true);
            if (continueButton != null) continueButton.gameObject.SetActive(false);
            if (messageText != null) messageText.text = "";

            SpawnGrid();

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void SpawnGrid()
        {
            cardButtons.Clear();

            foreach (Transform child in gridParent)
            {
                Destroy(child.gameObject);
            }

            if (cardOptions == null || cardOptions.Count < 4)
            {
                Debug.LogError("cardOptions must contain 4 sprites: 0=Diamond, 1=Heart, 2=Club, 3=Spade.");
                return;
            }

            // Randomly pick red pair (0-1) or black pair (2-3)
            bool useRedPair = Random.Range(0, 2) == 0;
            int first = useRedPair ? 0 : 2;
            int second = first + 1;

            // Randomly choose which of the pair is common vs odd
            if (Random.value < 0.5f)
            {
                commonCard = cardOptions[first];
                oddCard = cardOptions[second];
            }
            else
            {
                commonCard = cardOptions[second];
                oddCard = cardOptions[first];
            }

            oddCardIndex = Random.Range(0, 28);

            for (int i = 0; i < 28; i++)
            {
                GameObject card = Instantiate(cardPrefab, gridParent);
                Button btn = card.GetComponent<Button>();
                cardButtons.Add(btn);

                Sprite sprite = (i == oddCardIndex) ? oddCard : commonCard;
                btn.image.sprite = sprite;
                btn.name = i.ToString();

                int capturedIndex = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCardClicked(capturedIndex));
            }
        }

        private void OnCardClicked(int index)
        {
            if (controller != null) controller.PlayButtonClickSound();

            if (index == oddCardIndex)
            {
                if (controller != null) controller.PlayCoinSound();

                if (messageText != null)
                    messageText.text = "Found The Odd One Out!";

                if (continueButton != null)
                    continueButton.gameObject.SetActive(true);

                this.score = 10;
                this.maxScore = 10;
            }
            else
            {
                if (controller != null) controller.PlayWrongAnswerSound();
            }
        }

        private void OnContinueClicked()
        {
            if (controller != null)
            {
                controller.PlayButtonClickSound();
                controller.AtGameEnd();
            }
        }
    }
}
