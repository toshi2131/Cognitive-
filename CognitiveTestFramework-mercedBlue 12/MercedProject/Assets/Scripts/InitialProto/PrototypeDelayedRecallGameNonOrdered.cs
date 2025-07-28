namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using System.Collections.Generic;
    using System.Collections;
    using TMPro;


    /// <summary>
    /// A delayed recall game that pulls a smaller list of words from a larger one, and asks the
    /// player to remember them for later.  It goes through two iterations of walking the player
    /// through the list given, with a small score for getting all of them right, and then reports
    /// back the list for later.  Intended to be paired with PrototypeDelayedRecallEndGAmeNonOrdered.
    /// 
    /// This game is built to be repeatable from the manifest of words.  Problems will occur if the
    /// list of words differ between this game and the other one.
    /// </summary>
    public class PrototypeDelayedRecallGameNonOrdered : AbstractTestGame, IParentDialog
    {
        [SerializeField]
        private List<string> possibleWords = new List<string>();
        [SerializeField]
        private int wordsToGuess = 5;
        [SerializeField]
        private int totalButtonCount = 10;

        [SerializeField]
        private GameObject buttonTray;
        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private GameObject instructionTray;
        [SerializeField]
        private List<TextMeshProUGUI> instructionWords = new List<TextMeshProUGUI>();
        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private GameButton continueButton;

        // ELF Note:  these could be done as an array of instructions so that the deserialization is cleaner
        [SerializeField]
        private string initialInstruction = "Please memorize these words.  Click the button when you're ready to proceed.";
        [SerializeField]
        private string playbackInstruction = "Please click on the words you just saw.";
        [SerializeField]
        private string secondPlayInstruction = "Good!  One more time.";
        [SerializeField]
        private string finalInstruction = "Good!  Now remember these for later.  We'll quiz you on it in a moment.";

        private List<GameButton> buttons = new List<GameButton>();
        private List<string> currentPossibleWords = new List<string>();
        private List<string> actualWords = new List<string>();
        private List<string> guessableWords = new List<string>();

        private GameButton currentGuesser = null;
        private string currentGuess = "";

        private WaitForEndOfFrame wait = new WaitForEndOfFrame();
        private CognitiveTestController controller;

        public override List<string> GetDelayedRecallData()
        {
            return actualWords;
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
                if (protoData.instructions != null && protoData.instructions.Count == 4)
                {
                    initialInstruction = protoData.instructions[0];
                    playbackInstruction = protoData.instructions[1];
                    secondPlayInstruction = protoData.instructions[2];
                    finalInstruction = protoData.instructions[3];
                }
                else
                {
                    Debug.LogError("Invalid configuration data: instructions");
                    return;
                }

                if (protoData.intParams != null && protoData.intParams.Count == 2)
                {
                    totalButtonCount = protoData.intParams[0];
                    wordsToGuess = protoData.intParams[1];
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
                totalButtonCount,
                wordsToGuess
            };


            List<string> instructions = new List<string>
            {
                initialInstruction,
                playbackInstruction,
                secondPlayInstruction,
                finalInstruction
            };

            return new PrototypeGameConfig("delayedRecallString", instructions, possibleWords, ints, new List<bool>());
        }

        public override void StartGame(CognitiveTestController controller)
        {
            if (buttonTray == null || instructionTray == null || instructionWords.Count != wordsToGuess || instructionLabel == null ||
                buttonPrefab == null || continueButton == null || controller == null || wordsToGuess >= totalButtonCount)
            {
                Debug.LogError("Invalid input or initialization for game " + name + ", aborting");
                controller.AtGameEnd();
                return;
            }

            this.controller = controller;
            score = 0;
            maxScore = 2;

            currentPossibleWords.Clear();
            actualWords.Clear();

            for (int i = 0; i < totalButtonCount; i++)
            {
                GameObject buttonGo = Object.Instantiate(buttonPrefab, buttonTray.transform); //copy button prefab and place into button tray
                buttons.Add(buttonGo.GetComponent<GameButton>());
            }

            foreach (string element in possibleWords)
            {
                currentPossibleWords.Add(element);
            }

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
            instructionTray.SetActive(true);
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
            foreach(string word in wordsInProgress)
            {
                currentPossibleWords.Remove(word);
                guessableWords.Add(word);
            }

            // Fill out the delta of guessable words
            for(int i = wordsToGuess; i < totalButtonCount; i++)
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

            while(guesses > 0)
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
            if (wordsInProgress.Count == 0)
            {
                score++;
            }
        }
    }
}