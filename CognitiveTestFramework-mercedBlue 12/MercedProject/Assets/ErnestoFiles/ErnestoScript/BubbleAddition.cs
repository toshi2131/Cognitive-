using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveTestEngine.Core
{
    public class BubbleAddition : AbstractTestGame, IParentDialog
    {

        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private GameButton continueButton;
        [SerializeField]
        private GameObject SpawnZones;
        [SerializeField]
        private Transform AnswerBubble;

        [SerializeField] private Transform[] bubbles;
        [SerializeField] private TextMeshProUGUI[] AdditionText;
        [SerializeField] private TextMeshProUGUI AnswerText;

        public static bool bubbleBool;
        public static bool winBool;

        private string currentGuess = "";
        private GameButton currentGuesser = null;
        private CognitiveTestController controller;
        private WaitForEndOfFrame waitGeneral = new WaitForEndOfFrame();

        public List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

        public class WinBubble
        {
            public static int Winner = 0;
        }


        public void OnButtonClick(GameObject sender, string text)
        {
            currentGuess = text;
            currentGuesser = sender.GetComponent<GameButton>();
        }

        public override void StartGame(CognitiveTestController controller)
        {

            bubbleBool = true;
            DropZone.Slots.slotsOccupied = 0;
            score = 0;
            maxScore = 1;
            int answerText = Random.Range(2, 50);

            continueButton.Init(this, "Continue");

            continueButton.gameObject.SetActive(false);

            for (int i = 0; i < 8; i++)
            {
                bubbles[i].gameObject.SetActive(false);
            }

            List<int> pickedNumbers = BubblePicker();


            if (pickedNumbers != null)
            {
                Debug.Log("Picked numbers: " + string.Join(", ", pickedNumbers));
            }

            int bubbleValue = 0;

            for (int i = 0; i < 5; i++)
            {
                bubbleValue = pickedNumbers[i];
                Debug.Log("activating bubble with value: " + bubbleValue);


                if (bubbleValue >= 1 && bubbleValue <= bubbles.Length)
                {
                    bubbles[bubbleValue - 1].gameObject.SetActive(true);
                    int x = Random.Range(1, 26);
                    int y = Random.Range(1, 26);

                    while (x + y == answerText)
                    {
                        x = Random.Range(1, 26);
                    }

                    AdditionText[bubbleValue - 1].text = $"{x} + {y}";

                }
                else
                {
                    Debug.LogError("Picked number is out of the expected range for bubbles: " + bubbleValue);
                }

            }

            WinBubble.Winner = pickedNumbers[4] - 1;
            Debug.Log("Winning bubble: " + WinBubble.Winner);

            int a = Random.Range(0, (answerText + 1));
            Debug.Log("a Text set to: " + a);
            int b = answerText - a;
            Debug.Log("a Text set to: " + b);

            AdditionText[pickedNumbers[4] - 1].text = $"{a} + {b}";

            AnswerText.text = $"{answerText}";
            Debug.Log("Answer Text set to: " + AnswerText.text);

            
            this.controller = controller;
            instructionLabel.gameObject.SetActive(true);

            StartCoroutine(MainGameProcess());
        }

        public List<int> BubblePicker()
        {

            List<int> selectedNumbers = new List<int>();
            List<int> availableNumbers = numbers.ToList();

            for (int i = 0; i < 5; i++)
            {
                int randomIndex = Random.Range(0, availableNumbers.Count);
                selectedNumbers.Add(availableNumbers[randomIndex]);
                availableNumbers.RemoveAt(randomIndex);
            }

            return selectedNumbers;
        }



        public void CheckResults(int Won)
        {
            float tolerance = 0.1f;
            if (Mathf.Abs(bubbles[Won].position.y - AnswerBubble.position.y) <= tolerance && Mathf.Abs(bubbles[Won].position.x - AnswerBubble.position.x) <= tolerance)
            {
                winBool = true;
                
            }
            else
            {
                winBool = false;
            }

            if(winBool == true)
            {
                controller.PlayCoinSound();
                //WinText.gameObject.SetActive(true);
                continueButton.gameObject.SetActive(true);
                bubbles[Won].gameObject.SetActive(false);
                score++;
            }
            else
            {
                //LoseText.gameObject.SetActive(true);
                continueButton.gameObject.SetActive(true);
                for(int i = 0; i < 5; i++)
                {
                    if(Mathf.Abs(bubbles[i].position.y - AnswerBubble.position.y) <= tolerance && Mathf.Abs(bubbles[i].position.x - AnswerBubble.position.x) <= tolerance)
                    {
                        bubbles[i].gameObject.SetActive(false);
                    }
                }

                controller.PlayWrongAnswerSound();
            }
        }

        protected IEnumerator MainGameProcess()
        {
            yield return null;
            while (string.Equals(currentGuess, ""))
            {
                yield return waitGeneral;
            }
            WinBubble.Winner = 0;
            DropZone.Slots.slotsOccupied = 0;
            bubbleBool = false;

            continueButton.gameObject.SetActive(false);
            currentGuess = "";
            currentGuesser = null;

            controller.AtGameEnd();
            
        }
    }
}
