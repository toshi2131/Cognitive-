namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using System.Collections.Generic;
    using TMPro;
    using System.Collections;

    /// <summary>
    /// The latter half of the ORDERED delayed recall word game.  As neither MoCA nor BoCA prioritize word
    /// order, this test was also abandoned early in prototyping.
    /// 
    /// This is still potentially useful as an example of an iterative, progressively easier multiple choice
    /// quiz if that's desired for some other game instance.
    /// </summary>
    public class PrototypeDelayedRecallEndGame : AbstractTestGame, IParentDialog
    {
        // ELF Production version will populate these words from data instead of hard coding in editor,
        // so that the initial game and end game can both be populated from the same data.
        [SerializeField]
        private List<string> possibleWords = new List<string>();

        [SerializeField]
        private GameObject buttonTray;
        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private List<GameButton> buttons = new List<GameButton>();
        [SerializeField]
        private GameButton continueButton;

        [SerializeField]
        private string initialInstruction = "It's time to quiz on the words we gave you earlier.  Click Start to continue.";
        [SerializeField]
        private string firstTryInstruction1 = "Please click on the ";
        [SerializeField]
        private string firstTryInstruction2 = " word of the sequence given earlier.";
        [SerializeField]
        private string secondTryInstruction = "No, that's not it.  Try now.";
        [SerializeField]
        private string thirdTryInstruction = "I'm sorry, that's not it either.  How about now?";
        [SerializeField]
        private string finalInstruction = "Phew, good work!  Press continue to move on.";

        private List<string> actualWords = new List<string>();
        private int wordsToGuess;
        private GameObject currentGuesser = null;
        private string currentGuess = "";
        private List<string> currentPossibleWords = new List<string>();

        private WaitForEndOfFrame wait = new WaitForEndOfFrame();
        private CognitiveTestController controller;
        private int scoreFromWord = 0;
        private int firstTryButtons = 0;
        private int secondTryButtons = 0;
        private int thirdTryButtons = 0;

        private List<string> orderWords = new List<string>
        {
            "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th"
        };

        public override bool DischargeRecallData()
        {
            return true;
        }

        public void OnButtonClick(GameObject sender, string text)
        {
            currentGuesser = sender;
            currentGuess = text;
        }

        public override void StartGame(CognitiveTestController controller)
        {
            if (buttonTray == null || instructionLabel == null ||
                buttons.Count == 0 || possibleWords.Count == 0 || controller == null)
            {
                Debug.LogError("Invalid input or initialization for game " + name + ", aborting");
                controller.AtGameEnd();
                return;
            }

            this.controller = controller;
            score = 0;
            firstTryButtons = buttons.Count;
            secondTryButtons = firstTryButtons / 2;
            thirdTryButtons = secondTryButtons / 2;

            actualWords.AddRange(controller.lastDelayedRecallData);
            maxScore = actualWords.Count * 3;
            currentPossibleWords.Clear();
            currentPossibleWords.AddRange(possibleWords);

            instructionLabel.text = initialInstruction;
            currentGuess = "";
            continueButton.Init(this, "Ok");
            continueButton.gameObject.SetActive(true);
            buttonTray.SetActive(false);
            StartCoroutine(MainGameProcess());
        }

        protected private IEnumerator MainGameProcess()
        {
            List<string> wordsInProgress = new List<string>(actualWords);

            yield return null;
            yield return null;
            while (string.Equals(currentGuess, ""))
            {
                yield return wait;
            }

            controller.PlayButtonClickSound();

            continueButton.gameObject.SetActive(false);
            currentGuess = "";
            currentGuesser = null;

            while(wordsInProgress.Count > 0)
            {
                string currentWord = wordsInProgress[0];
                yield return SubGameProcess(currentWord, actualWords.Count - wordsInProgress.Count);

                score += scoreFromWord;
                wordsInProgress.RemoveAt(0);
            }

            buttonTray.SetActive(false);

            continueButton.gameObject.SetActive(true);
            instructionLabel.text = finalInstruction;
            while (string.Equals(currentGuess, ""))
            {
                yield return wait;
            }
            controller.PlayButtonClickSound();

            controller.AtGameEnd();
        }

        protected IEnumerator SubGameProcess(string word, int wordIndex)
        {
            scoreFromWord = 0;
            yield return SubGamePass(word, firstTryButtons, firstTryInstruction1 + orderWords[wordIndex] + firstTryInstruction2, 3);

            if (scoreFromWord > 0)
            {
                yield break;
            }

            yield return SubGamePass(word, secondTryButtons, secondTryInstruction, 2);

            if (scoreFromWord > 0)
            {
                yield break;
            }

            yield return SubGamePass(word, thirdTryButtons, thirdTryInstruction, 1);
        }

        protected IEnumerator SubGamePass(string word, int choices, string instructions, int possibleScore)
        {
            int whereToPutWord = Random.Range(0, choices);
            currentPossibleWords.Clear();
            currentPossibleWords.AddRange(possibleWords);
            currentPossibleWords.Remove(word);
            buttonTray.SetActive(false);
            instructionLabel.text = instructions;

            currentGuess = "";
            currentGuesser = null;

            for (int i = 0; i < buttons.Count; i++)
            {
                if (i < choices)
                {
                    string currentWord;
                    if (i == whereToPutWord)
                    {
                        currentWord = word;
                    }
                    else
                    {
                        int index = Random.Range(0, currentPossibleWords.Count);
                        currentWord = currentPossibleWords[index];
                        currentPossibleWords.Remove(currentWord);
                    }

                    buttons[i].Init(this, currentWord);
                    buttons[i].gameObject.SetActive(true);
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }

            buttonTray.SetActive(true);

            while (string.Equals(currentGuess, ""))
            {
                yield return wait;
            }

            if (string.Equals(currentGuess, word))
            {
                controller.PlayCoinSound();
                scoreFromWord = possibleScore;
            }
            else
            {
                controller.PlayWrongAnswerSound();
            }
        }
    }
}