namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using System.Collections.Generic;
    using System.Collections;
    using TMPro;

    /// <summary>
    /// An initial version of a delayed recall word game, but this one is built to also care about the word
    /// order.  Since that's considerably harder than the non-ordered version (which is what MoCA and BoCA did),
    /// this version was abandoned prior to data driven functionality being added and mostly exists as an example
    /// if some other game where ordering matters is desired to be made.
    /// 
    /// Is meant to be paired with PrototypeDelayedRecallEndGame.
    /// </summary>
    public class PrototypeDelayedRecallGame : AbstractTestGame, IParentDialog
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
        private GameObject instructionTray;
        [SerializeField]
        private List<TextMeshProUGUI> instructionWords = new List<TextMeshProUGUI>();
        [SerializeField]
        private List<GameButton> buttons = new List<GameButton>();
        [SerializeField]
        private GameButton continueButton;

        [SerializeField]
        private string initialInstruction = "Please memorize these words in order.  Click the button when you're ready to proceed.";
        [SerializeField]
        private string playbackInstruction = "Please click on the buttons in the order of the words we just showed you.";
        [SerializeField]
        private string secondPlayInstruction = "Good!  One more time.";
        [SerializeField]
        private string finalInstruction = "Good!  Now remember these for later.  We'll quiz you on it in a moment.";

        private List<string> currentPossibleWords = new List<string>();
        private List<string> actualWords = new List<string>();
        private int wordsToGuess;
        private GameObject currentGuesser = null;
        private string currentGuess = "";

        private WaitForEndOfFrame wait = new WaitForEndOfFrame();
        private CognitiveTestController controller;

        public override List<string> GetDelayedRecallData()
        {
            return actualWords;
        }

        public void OnButtonClick(GameObject sender, string text)
        {
            currentGuesser = sender;
            currentGuess = text;
        }

        public override void StartGame(CognitiveTestController controller)
        {
            if (buttonTray == null || instructionTray == null || instructionWords.Count == 0 || instructionLabel == null ||
                buttons.Count == 0 || possibleWords.Count == 0 || continueButton == null || controller == null)
            {
                Debug.LogError("Invalid input or initialization for game " + name + ", aborting");
                controller.AtGameEnd();
                return;
            }

            this.controller = controller;
            score = 0;
            maxScore = 0;

            currentPossibleWords.Clear();
            actualWords.Clear();
           
            foreach (string element in possibleWords)
            {
                currentPossibleWords.Add(element);
            }

            wordsToGuess = buttons.Count;

            for (int i = 0; i < wordsToGuess; i++)
            {
                int newIndex = Random.Range(0, currentPossibleWords.Count);
                string newWord = currentPossibleWords[newIndex];
                actualWords.Add(newWord);
                instructionWords[i].text = newWord;
                currentPossibleWords.Remove(newWord);
            }

            instructionLabel.text = initialInstruction;
            currentGuess = "";
            instructionTray.SetActive(true);
            continueButton.Init(this, "Ok");
            continueButton.gameObject.SetActive(true);
            StartCoroutine(MainGameProcess());
        }

        protected private IEnumerator MainGameProcess()
        {
            yield return null;
            while (string.Equals(currentGuess, ""))
            {
                yield return wait;
            }

            controller.PlayButtonClickSound();

            instructionTray.SetActive(false);
            buttonTray.SetActive(false);
            continueButton.gameObject.SetActive(false);
            currentGuess = "";
            currentGuesser = null;

            yield return SubGameProcess(playbackInstruction);
            yield return SubGameProcess(secondPlayInstruction);

            continueButton.Init(this, "Ok");
            continueButton.gameObject.SetActive(true);
            instructionLabel.text = finalInstruction;
            while (string.Equals(currentGuess, ""))
            {
                yield return wait;
            }
            controller.PlayButtonClickSound();

            controller.AtGameEnd();
        }

        protected private IEnumerator SubGameProcess(string instruction)
        {
            List<string> wordsInProgress = new List<string>(actualWords);

            instructionLabel.text = instruction;
            yield return null;
            for (int i = 0; i < wordsToGuess; i++)
            {
                int wordIndex = Random.Range(0, wordsInProgress.Count);
                buttons[i].Init(this, wordsInProgress[wordIndex]);
                buttons[i].gameObject.SetActive(true);
                wordsInProgress.Remove(wordsInProgress[wordIndex]);
            }

            wordsInProgress.AddRange(actualWords);
            buttonTray.SetActive(true);

            while(wordsInProgress.Count > 0)
            {
                currentGuess = "";
                currentGuesser = null;

                while(string.Equals(currentGuess, ""))
                {
                    yield return wait;
                }

                if (string.Equals(currentGuess, wordsInProgress[0]))
                {
                    controller.PlayCoinSound();
                    currentGuesser.SetActive(false);
                    wordsInProgress.RemoveAt(0);
                }
                else
                {
                    controller.PlayWrongAnswerSound();
                }
            }

            currentGuess = "";
            currentGuesser = null;
            buttonTray.SetActive(false);
        }
    }
}