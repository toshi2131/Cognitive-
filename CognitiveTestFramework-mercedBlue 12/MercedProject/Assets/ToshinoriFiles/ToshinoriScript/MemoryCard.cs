using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace CognitiveTestEngine.Core
{
    public class MemoryCard : AbstractTestGame
    {
        [Header("Card Setup")]
        [SerializeField] private Sprite backgroundCardImage;
        [SerializeField] private GameObject cardGrid; // Assign the parent GameObject in the Inspector

        private List<Button> cardButtonList = new List<Button>();
        [SerializeField] private Sprite[] pictures;
        public List<Sprite> cardButtons = new List<Sprite>();

        private bool firstGuess, secondGuess;
        private int correctGuessesCount;
        private int gameGuesses = 4; // Set the number of PAIRS you want

        private int firstGuessIndex, secondGuessIndex;
        private Sprite firstGuessSprite; // Store the Sprite directly
        private Sprite secondGuessSprite;

        private CognitiveTestController controller;
        private UIManager uiManager;

        [Header("UI")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Button continueButton;

        public override void StartGame(CognitiveTestController controller)
        {
            this.controller = controller;
            uiManager = Object.FindFirstObjectByType<UIManager>();

            Global2.guessesCount = 0;
            correctGuessesCount = 0;

            if (titlePanel != null)
            {
                titlePanel.SetActive(true);
            }
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }

            cardButtonList.Clear();
            if (cardGrid != null)
            {
                Button[] btns = cardGrid.GetComponentsInChildren<Button>();
                foreach (Button btn in btns)
                {
                    btn.image.sprite = backgroundCardImage;
                    btn.interactable = true;
                    btn.image.color = Color.white;
                    cardButtonList.Add(btn);
                }

                if (cardButtonList.Count > gameGuesses * 2)
                {
                    cardButtonList = cardButtonList.GetRange(0, gameGuesses * 2);
                }
                else if (cardButtonList.Count < gameGuesses * 2)
                {
                    Debug.LogError("Error: Found only " + cardButtonList.Count + " buttons in the CardGrid. Expected " + (gameGuesses * 2) + ".");
                    return;
                }
            }
            else
            {
                Debug.LogError("Error: cardGrid GameObject is not assigned in the Inspector!");
                return;
            }

            cardButtons.Clear();
            if (pictures != null && pictures.Length >= gameGuesses)
            {
                for (int i = 0; i < gameGuesses; i++)
                {
                    cardButtons.Add(pictures[i]);
                    cardButtons.Add(pictures[i]);
                }
            }
            else
            {
                Debug.LogError("Error: Not enough pictures assigned in the Inspector for " + gameGuesses + " pairs.");
                return;
            }

            Shuffle(cardButtons);

            if (cardButtonList.Count != cardButtons.Count)
            {
                Debug.LogError("Error: Mismatch in the number of card buttons and card images!");
                return;
            }

            for (int i = 0; i < cardButtonList.Count; i++)
            {
                int capturedIndex = i;
                cardButtonList[i].name = i.ToString();
                cardButtonList[i].onClick.RemoveAllListeners();
                cardButtonList[i].onClick.AddListener(() => CardFlipped(capturedIndex));
            }

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void CardFlipped(int index)
        {
            if (!firstGuess)
            {
                firstGuess = true;
                firstGuessIndex = index;
                firstGuessSprite = cardButtons[index];
                cardButtonList[index].image.sprite = firstGuessSprite;
            }
            else if (!secondGuess && index != firstGuessIndex)
            {
                secondGuess = true;
                secondGuessIndex = index;
                secondGuessSprite = cardButtons[index];
                cardButtonList[index].image.sprite = secondGuessSprite;

                Global2.guessesCount++;
                StartCoroutine(DealWithMatchingCards());
            }
        }

        private IEnumerator DealWithMatchingCards()
        {
            yield return new WaitForSeconds(1f);

            if (firstGuessSprite == secondGuessSprite)
            {
                if (controller != null)
                {
                    controller.PlayCoinSound();
                }
                cardButtonList[firstGuessIndex].interactable = false;
                cardButtonList[secondGuessIndex].interactable = false;

                cardButtonList[firstGuessIndex].image.color = new Color(0, 0, 0, 0);
                cardButtonList[secondGuessIndex].image.color = new Color(0, 0, 0, 0);

                correctGuessesCount++;

                if (correctGuessesCount == gameGuesses)
                {
                    if (controller != null)
                    {
                        controller.PlayCoinSound();
                    }
                    ShowWinPanel();
                }
            }
            else
            {
                if (controller != null)
                {
                    controller.PlayWrongAnswerSound();
                }

                cardButtonList[firstGuessIndex].image.sprite = backgroundCardImage;
                cardButtonList[secondGuessIndex].image.sprite = backgroundCardImage;
            }

            firstGuess = false;
            secondGuess = false;
        }

        private void ShowWinPanel()
        {
            if (winPanel != null)
                winPanel.SetActive(true);

            int maxScore = 100;
            int score = Mathf.Max(10, maxScore - (Global2.guessesCount - gameGuesses) * 10);
            this.score = score;
            this.maxScore = maxScore;

            if (uiManager != null)
                uiManager.ShowWinMessage();
        }

        private void OnContinueClicked()
        {
            if (controller != null)
            {
                controller.PlayButtonClickSound();
                controller.AtGameEnd();
            }
        }

        private void Shuffle(List<Sprite> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int rand = Random.Range(i, list.Count);
                Sprite temp = list[i];
                list[i] = list[rand];
                list[rand] = temp;
            }
        }
    }
}
