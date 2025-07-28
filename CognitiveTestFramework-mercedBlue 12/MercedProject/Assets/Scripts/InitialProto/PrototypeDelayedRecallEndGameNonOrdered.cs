namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using System.Collections.Generic;
    using System.Collections;
    using TMPro;

    /// <summary>
    /// The other half of the pairing with PrototypeDelayedRecallGameNonOrdered.  This game will
    /// gather the words that were chosen to be rememebered from the earlier game, and asks the player
    /// to remember them again later.  The number of buttons shown here will generally be greater than
    /// the earlier practice run, and getting each word correct will be more important.
    /// 
    /// Problems may occur if the data for the possible words in this game differs from the other one.
    /// </summary>
    public class PrototypeDelayedRecallEndGameNonOrdered : AbstractTestGame, IParentDialog
    {
        [SerializeField]
        private List<string> possibleWords = new List<string>();
        [SerializeField]
        private int wordsToGuess = 5;
        [SerializeField]
        private int totalButtonCount = 20;

        [SerializeField]
        private GameObject buttonTray;
        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private GameButton continueButton;

        [SerializeField]
        private string initialInstruction = "It's time to quiz on the words we gave you earlier.  Click Start to continue.";
        [SerializeField]
        private string playbackInstruction = "Please click on each of the words from earlier.";
        [SerializeField]
        private string finalInstruction = "Good work!  Press continue to move on.";

        private List<GameButton> buttons = new List<GameButton>();
        private List<string> currentPossibleWords = new List<string>();
        private List<string> actualWords = new List<string>();
        private List<string> guessableWords = new List<string>();

        private GameButton currentGuesser = null;
        private string currentGuess = "";

        private WaitForEndOfFrame wait = new WaitForEndOfFrame();
        private CognitiveTestController controller;

        public override bool DischargeRecallData()
        {
            return true;
        }

        public void OnButtonClick(GameObject sender, string text)
        {
            currentGuess = text;
            currentGuesser = sender.GetComponent<GameButton>();
        }

        public override void Configure(AbstractGameConfig configData)
        {
            PrototypeGameConfig protoData = configData as PrototypeGameConfig;
            if (protoData != null)
            {
                if (protoData.instructions != null && protoData.instructions.Count == 3)
                {
                    initialInstruction = protoData.instructions[0];
                    playbackInstruction = protoData.instructions[1];
                    finalInstruction = protoData.instructions[2];
                }
                else
                {
                    Debug.LogError("Invalid configuration data: instructions");
                    return;
                }

                if (protoData.intParams != null && protoData.intParams.Count == 1)
                {
                    totalButtonCount = protoData.intParams[0];
                }
                else
                {
                    Debug.LogError("Invalid configuration data: intParams");
                    return;
                }

                if (protoData.stringParams != null && protoData.stringParams.Count > 0)
                {
                    possibleWords = protoData.stringParams;
                }
                else
                {
                    Debug.LogError("Invalid configuration data: stringParams");
                    return;
                }
            }
            else
            {
                Debug.LogError("Invalid configuration data: total");
            }
        }

        public override AbstractGameConfig ExportConfiguration()
        {
            List<int> ints = new List<int>
            {
                totalButtonCount
            };


            List<string> instructions = new List<string>
            {
                initialInstruction,
                playbackInstruction,
                finalInstruction
            };


            return new PrototypeGameConfig("delayedRecallStringEnd", instructions, possibleWords, ints, new List<bool>());
        }

        public override void StartGame(CognitiveTestController controller)
        {
            if (buttonTray == null || instructionLabel == null ||
                buttonPrefab == null || continueButton == null || controller == null || wordsToGuess >= totalButtonCount)
            {
                Debug.LogError("Invalid input or initialization for game " + name + ", aborting");
                controller.AtGameEnd();
                return;
            }

            this.controller = controller;
            score = 0;

            currentPossibleWords.Clear();
            actualWords.AddRange(controller.lastDelayedRecallData);
            wordsToGuess = actualWords.Count;
            maxScore = wordsToGuess;

            for (int i = 0; i < totalButtonCount; i++)
            {
                GameObject buttonGo = Object.Instantiate(buttonPrefab, buttonTray.transform);
                buttons.Add(buttonGo.GetComponent<GameButton>());
            }

            foreach (string element in possibleWords)
            {
                currentPossibleWords.Add(element);
            }

            instructionLabel.text = initialInstruction;
            currentGuess = "";
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

            buttonTray.SetActive(false);
            continueButton.gameObject.SetActive(false);
            currentGuess = "";
            currentGuesser = null;

            yield return SubGameProcess(playbackInstruction);

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

            currentPossibleWords.Clear();
            currentPossibleWords.AddRange(possibleWords);
            guessableWords.Clear();

            // Get our five actual words
            foreach (string word in wordsInProgress)
            {
                if (currentPossibleWords.Contains(word))
                {
                    currentPossibleWords.Remove(word);
                }
                guessableWords.Add(word);
            }

            // Fill out the delta of guessable words
            for (int i = wordsToGuess; i < totalButtonCount; i++)
            {
                int newIndex = Random.Range(0, currentPossibleWords.Count);
                string newWord = currentPossibleWords[newIndex];
                guessableWords.Add(newWord);
                currentPossibleWords.Remove(newWord);
            }

            // Now shuffle that field onto the buttons.  Each time we run this subloop, we should have
            // the actual words shuffled in with (totalButtons - actualWords) others to allow the user
            // to have a different field to guess from.
            for (int j = 0; j < totalButtonCount; j++)
            {
                int guessIndex = Random.Range(0, guessableWords.Count);
                string guessWord = guessableWords[guessIndex];
                buttons[j].Init(this, guessWord);
                guessableWords.Remove(guessWord);
            }

            int guesses = wordsToGuess;
            buttonTray.SetActive(true);

            while (guesses > 0)
            {
                currentGuess = "";
                currentGuesser = null;

                while (string.Equals(currentGuess, ""))
                {
                    yield return wait;
                }

                guesses -= 1;
                if (wordsInProgress.Contains(currentGuess))
                {
                    wordsInProgress.Remove(currentGuess);
                    controller.PlayCoinSound();
                    currentGuesser.OnSuccess();
                }
                else
                {
                    controller.PlayWrongAnswerSound();
                    currentGuesser.OnFailure();
                }
            }

            currentGuess = "";
            currentGuesser = null;
            buttonTray.SetActive(false);
            score = wordsToGuess - wordsInProgress.Count;
        }
    }
}