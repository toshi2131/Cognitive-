namespace CognitiveTestEngine.Prototype
{
    using Core;
    using System.Collections.Generic;
    using System.Collections;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// A simple game to yield a series of buttons that mostly have one value, with the digits flipped
    /// for one "oddball", challenging the user to locate the oddball quickly.  This game can be relatively
    /// unforgiving if there are too many buttons and not enough time, but it creates a template that might
    /// be improved on further down the road.
    /// </summary>
    public class PrototypeFindOddballGame : AbstractTestGame, IParentDialog
    {
        [SerializeField]
        private GameObject buttonTray;
        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private int totalButtonCount = 30;
        
        [SerializeField]
        private float findingTime = 2.5f;
        [SerializeField]
        private int iterations = 3;
        
        [SerializeField]
        private string initialInstruction = "We're going to show you a grid of numbers.  Quickly find the one that is different than the others.  Press Ok to begin.";
        [SerializeField]
        private string oddballInstruction = "Quickly find the number that doesn't belong:";
        [SerializeField]
        private GameButton continueButton;

        [SerializeField]
        private bool doIntro = true;

        private GameButton currentGuesser = null;
        private string currentGuess = "";
        private List<GameButton> buttons = new List<GameButton>();

        private int firstDigit;
        private int secondDigit;
        private int uniform;
        private int oddball;

        private CognitiveTestController controller;
        private WaitForSecondsRealtime waitToLook;
        private WaitForEndOfFrame wait = new WaitForEndOfFrame();

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
                if (protoData.instructions != null && protoData.instructions.Count == 2)
                {
                    initialInstruction = protoData.instructions[0];
                    oddballInstruction = protoData.instructions[1];
                }
                else
                {
                    Debug.LogError("Invalid configuration data: instructions");
                    return;
                }

                if (protoData.intParams != null && protoData.intParams.Count == 3)
                {
                    totalButtonCount = protoData.intParams[0];
                    iterations = protoData.intParams[1];
                    float findingTimeRaw = (float)protoData.intParams[2];
                    findingTime = findingTimeRaw / 10f;
                }
                else
                {
                    Debug.LogError("Invalid configuration data: intParams");
                    return;
                }

                if (protoData.boolParams != null && protoData.boolParams.Count == 1)
                {
                    doIntro = protoData.boolParams[0];
                }
                else
                {
                    Debug.LogError("Invalid configuration data: boolParams");
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
                iterations,
                (int) (findingTime * 10f)
            };

            List<string> instructions = new List<string>
            {
                initialInstruction,
                oddballInstruction
            };

            List<bool> bools = new List<bool>
            {
                doIntro
            };

            PrototypeGameConfig config = new PrototypeGameConfig("findOddball", instructions, new List<string>(), ints, bools);
            return config;
        }

        public override void StartGame(CognitiveTestController controller)
        {
            if (buttonTray == null || instructionLabel == null || continueButton == null ||
                buttonPrefab == null || controller == null || findingTime < 2f || totalButtonCount < 5)
            {
                Debug.LogError("Invalid input or initialization for game " + name + ", aborting");
                controller.AtGameEnd();
                return;
            }

            this.controller = controller;
            score = 0;
            maxScore = iterations;
            gameType = "Find Oddball";
            buttonTray.SetActive(false);

            for (int i = 0; i < totalButtonCount; i++)
            {
                GameObject buttonGo = Object.Instantiate(buttonPrefab, buttonTray.transform);
                GameButton button = buttonGo.GetComponent<GameButton>();
                if (button != null)
                {
                    buttons.Add(buttonGo.GetComponent<GameButton>());
                }
                else
                {
                    Debug.LogError(name + ": Button prefab does not have a GameButton component on it, aborting");
                    controller.AtGameEnd();
                    return;
                }
            }

            instructionLabel.gameObject.SetActive(true);
            instructionLabel.text = initialInstruction;
            waitToLook = new WaitForSecondsRealtime(findingTime);
            continueButton.Init(this, "Ok");

            StartCoroutine(MainGameProcess());
        }

        protected IEnumerator MainGameProcess()
        {
            yield return null;
            while (doIntro && string.Equals(currentGuess, ""))
            {
                yield return wait;
            }

            if (doIntro)
            {
                controller.PlayButtonClickSound();
            }
            continueButton.gameObject.SetActive(false);

            instructionLabel.text = oddballInstruction;

            buttonTray.SetActive(false);
            currentGuess = "";
            currentGuesser = null;

            for (int i = 0; i < iterations; i++)
            {
                yield return SubGameProcess();
            }

            controller.AtGameEnd();
        }

        protected IEnumerator SubGameProcess()
        {
            firstDigit = Random.Range(1, 9);
            do
            {
                secondDigit = Random.Range(1, 9);
            } while (secondDigit == firstDigit);

            uniform = (firstDigit * 10) + secondDigit;
            oddball = (secondDigit * 10) + firstDigit;
            int oddballIndex = Random.Range(0, totalButtonCount);

            for (int i = 0; i < totalButtonCount; i++)
            {
                buttons[i].Init(this, i == oddballIndex ? oddball.ToString() : uniform.ToString());
            }

            float time = Time.time;
            buttonTray.SetActive(true);

            while (string.Equals(currentGuess, "") && Time.time < (time + findingTime))
            {
                yield return wait;
            }

            if (string.Equals(currentGuess, oddball.ToString()))
            {
                controller.PlayCoinSound();
                currentGuesser.OnSuccess();
                score++;
            }
            else
            {
                controller.PlayWrongAnswerSound();
                if (currentGuesser != null)
                {
                    currentGuesser.OnFailure();
                }
            }

            currentGuess = "";
            currentGuesser = null;
            buttonTray.SetActive(false);
        }
    }
}
