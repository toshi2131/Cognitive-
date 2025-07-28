using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace CognitiveTestEngine.Core
{
    public class PyramidStand : AbstractTestGame, IParentDialog
    {
        [SerializeField] private TextMeshProUGUI instructionLabel;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform[] blocks;

        [SerializeField] private TextMeshProUGUI WinText;
        [SerializeField] private TextMeshProUGUI LoseText;

        [SerializeField] private GameButton continueButton;

        public static bool winBool;
        public static bool loseBool;
        public static bool done = false;
        public static bool pyramidBool = false;

        private string currentGuess = "";
        private GameButton currentGuesser = null;
        private CognitiveTestController controller;
        private WaitForEndOfFrame waitGeneral = new WaitForEndOfFrame();

        public void OnButtonClick(GameObject sender, string text)
        {
            currentGuess = text;
            currentGuesser = sender.GetComponent<GameButton>();
        }


        public override void StartGame(CognitiveTestController controller)
        {
            pyramidBool = true;
            DropZone.Slots.slotsOccupied = 0;
            score = 0;
            maxScore = 1;
            winBool = false;
            loseBool = false;


            continueButton.Init(this, "Continue");

            continueButton.gameObject.SetActive(false);

            this.controller = controller;
            instructionLabel.gameObject.SetActive(true);

            StartCoroutine(MainGameProcess());
        }



        public void CheckResults()
        {
                
            if (Mathf.Abs(blocks[0].position.y) < Mathf.Abs(blocks[1].position.y) &&
                Mathf.Abs(blocks[1].position.y) < Mathf.Abs(blocks[2].position.y) &&
                Mathf.Abs(blocks[2].position.y) < Mathf.Abs(blocks[3].position.y) &&
                Mathf.Abs(blocks[3].position.y) < Mathf.Abs(blocks[4].position.y))
            {
                winBool = true;
                loseBool = false;
            }
            else
            {
                loseBool = true;
                winBool = false;
            }

            if (winBool == true)
            {
                controller.PlayCoinSound();
                WinText.gameObject.SetActive(true);
                continueButton.gameObject.SetActive(true);
                score++;
            }

            else
            {
                LoseText.gameObject.SetActive(true);
                continueButton.gameObject.SetActive(true);
                controller.PlayWrongAnswerSound();
            }

        }

        
        
        //end the game when Continue button is pressed
        protected IEnumerator MainGameProcess()
        {

            yield return null;
            while (string.Equals(currentGuess, ""))
            {
                yield return waitGeneral;
            }

            pyramidBool = false;
            DropZone.Slots.slotsOccupied = 0;
            continueButton.gameObject.SetActive(false);
            currentGuess = "";
            currentGuesser = null;

            controller.AtGameEnd();
        }
    }
}