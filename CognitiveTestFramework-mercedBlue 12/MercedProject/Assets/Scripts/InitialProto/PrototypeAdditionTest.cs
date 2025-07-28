namespace CognitiveTestEngine.Prototype
{
    using Core;
    using System.Collections.Generic;
    using System.Collections;
    using TMPro;
    using UnityEngine;

    /// <summary>
    /// Simple addition test, loosely based on the BoCA addition testing.  This is pretty basic, and only handles
    /// addition and not subtraction, and does not do the BoCA standard of testing a progression of two single
    /// digit numbers, then a single and double digit number then two double digit numbers while making sure there
    /// is always a "carry" value to be calculated.  Can potentially be improved to model that, or to be more
    /// interesting or instructive in its UI flow.
    /// </summary>
    public class PrototypeAdditionTest : AbstractTestGame, IParentDialog
    {
        [SerializeField]
        private GameObject buttonTray;
        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private TextMeshProUGUI addendLabel;
        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private int totalButtonCount = 30;
        [SerializeField]
        private int minimumSum = 40;
        [SerializeField]
        private int minimumAddend = 15;
        [SerializeField]
        private int maximumAddend = 40;
        [SerializeField]
        private float addendLookTime = 2;
        [SerializeField]
        private float addendDisappearTime = 0.5f;
        [SerializeField]
        private int iterations = 3;
        [SerializeField]
        private GameButton continueButton;

        // ELF Note:  these could be done as an array of instructions so that the deserialization is cleaner
        [SerializeField]
        private string initialInstruction = "We're going to play an addition game. Don't use paper, just your head!  Click Start to continue.";
        [SerializeField]
        private string addingInstruction = "Please add the following two numbers in your head:";

        private GameButton currentGuesser = null;
        private string currentGuess = "";
        private List<GameButton> buttons = new List<GameButton>();

        private int sum;

        private CognitiveTestController controller;
        private WaitForSecondsRealtime waitAddend;
        private WaitForSecondsRealtime waitDisappear;
        private WaitForEndOfFrame waitGeneral = new WaitForEndOfFrame();

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
                    addingInstruction = protoData.instructions[1];
                }
                else
                {
                    Debug.LogError("Invalid configuration data: instructions");
                    return;
                }

                if (protoData.intParams != null && protoData.intParams.Count == 7)
                {
                    totalButtonCount = protoData.intParams[0];
                    minimumSum = protoData.intParams[1];
                    minimumAddend = protoData.intParams[2];
                    maximumAddend = protoData.intParams[3];
                    iterations = protoData.intParams[4];
                    float addendLookTimeRaw = (float)protoData.intParams[5];
                    addendLookTime = addendLookTimeRaw / 10f;
                    float addendDisappearTimeRaw = (float)protoData.intParams[6];
                    addendDisappearTime = addendDisappearTimeRaw / 10f;
                }
                else
                {
                    Debug.LogError("Invalid configuration data: intParams");
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
                minimumSum,
                minimumAddend,
                maximumAddend,
                iterations,
                (int) (addendLookTime * 10f),
                (int) (addendDisappearTime * 10f)
            };

            List<string> instructions = new List<string>
            {
                initialInstruction,
                addingInstruction
            };

            return new PrototypeGameConfig("addingProto", instructions, new List<string>(), ints, new List<bool>());
        }

        public override void StartGame(CognitiveTestController controller)
        {
            if (buttonTray == null || instructionLabel == null || addendLabel == null ||
                buttonPrefab == null || controller == null || (maximumAddend * 2 < minimumSum) || (minimumAddend * 2 > (minimumSum + totalButtonCount)))
            {
                Debug.LogError("Invalid input or initialization for game " + name + ", aborting");
                controller.AtGameEnd();
                return;
            }

            this.controller = controller;
            score = 0;
            maxScore = iterations;
            gameType = "Addition Test";

            buttonTray.SetActive(false);

            for (int i = 0; i < totalButtonCount; i++)
            {
                GameObject buttonGo = Object.Instantiate(buttonPrefab, buttonTray.transform);
                GameButton button = buttonGo.GetComponent<GameButton>();
                if (button != null)
                {
                    button.Init(this, (minimumSum + i).ToString());
                    buttons.Add(buttonGo.GetComponent<GameButton>());
                }
                else
                {
                    Debug.LogError(name + ": Button prefab does not have a GameButton component on it, aborting");
                    
                    controller.AtGameEnd();
                    return;
                }
            }

            addendLabel.gameObject.SetActive(false);
            instructionLabel.gameObject.SetActive(true);
            instructionLabel.text = initialInstruction;
            waitAddend = new WaitForSecondsRealtime(addendLookTime);
            waitDisappear = new WaitForSecondsRealtime(addendDisappearTime);
            continueButton.Init(this, "Ok");

            StartCoroutine(MainGameProcess());
        }

        protected IEnumerator MainGameProcess()
        {
            yield return null;
            while (string.Equals(currentGuess, ""))
            {
                yield return waitGeneral;
            }

            controller.PlayButtonClickSound();

            buttonTray.SetActive(false);
            continueButton.gameObject.SetActive(false);
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
            instructionLabel.text = addingInstruction;
            sum = Random.Range(minimumSum, minimumSum + totalButtonCount - 1);
            int firstAddend = Random.Range(minimumAddend, maximumAddend);
            int secondAddend = sum - firstAddend;

            addendLabel.text = firstAddend.ToString();
            addendLabel.gameObject.SetActive(true);
            yield return waitAddend;
            addendLabel.text = "plus";
            yield return waitDisappear;
            addendLabel.text = secondAddend.ToString();
            yield return waitAddend;
            addendLabel.gameObject.SetActive(false);
            buttonTray.SetActive(true);

            while (string.Equals(currentGuess, ""))
            {
                yield return waitGeneral;
            }

            int enteredSum = int.Parse(currentGuess);
            if (enteredSum == sum)
            {
                controller.PlayCoinSound();
                currentGuesser.OnSuccess();
                score++;
            }
            else
            {
                controller.PlayWrongAnswerSound();
                currentGuesser.OnFailure();
            }

            yield return waitDisappear;
            currentGuess = "";
            currentGuesser = null;
            buttonTray.SetActive(false);
        }
    }
}
