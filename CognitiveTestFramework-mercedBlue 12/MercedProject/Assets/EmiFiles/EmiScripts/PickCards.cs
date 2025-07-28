namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using UnityEngine.UI;
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
    ///

    public class PickCards : AbstractTestGame, IParentDialog
    {
        [SerializeField]
        private List<CardData> possibleCards = new List<CardData>();
        [SerializeField]
        private int cardsToGuess = 5;
        [SerializeField]
        private int totalButtonCount = 10;

        [SerializeField]
        private GameObject buttonTray;
        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private GameObject instructionTray;
        [SerializeField]
        private List<CardData> instructionCards = new List<CardData>();

        // Trying Out :
        //[SerializeField]
        //private List<GameButton> instructionCards = new List<GameButton>();

        public float flipDuration = 0.5f;

        private Image cardImage;
        private bool isFlipped = false;

        // -------------------------------------------------------------------

        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private GameButton continueButton;

        // ELF Note:  these could be done as an array of instructions so that the deserialization is cleaner
        [SerializeField]
        private string initialInstruction = "Please memorize these cards.  Click the button when you're ready to proceed.";
        [SerializeField]
        private string playbackInstruction = "Please click on the cards you just saw.";
        [SerializeField]
        private string secondPlayInstruction = "Great Job!  Let's try that one more time.";
        [SerializeField]
        private string finalInstruction = "Great Job! Click to continue:";

        private List<GameButton> buttons = new List<GameButton>();
        private List<CardData> currentPossibleCards = new List<CardData>();
        private List<CardData> actualCards = new List<CardData>();
        private List<CardData> guessableCards = new List<CardData>();

        private GameButton currentGuesser = null;
        private string currentGuess = "";

        private WaitForEndOfFrame wait = new WaitForEndOfFrame();
        private CognitiveTestController controller;

        //public override List<CardData> GetDelayedRecallData()
        //{
        //    return actualCards;
        //}

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
                    cardsToGuess = protoData.intParams[1];
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
                cardsToGuess
            };


            List<string> instructions = new List<string>
            {
                initialInstruction,
                playbackInstruction,
                secondPlayInstruction,
                finalInstruction
            };

            return new PrototypeGameConfig("pickCards", instructions, new List<string>(), ints, new List<bool>());
        }

        public override void StartGame(CognitiveTestController controller)
        {

            if (instructionCards.Count != cardsToGuess)
            {
                Debug.LogError("Invalid input or initialization for game " + name + ", aborting: 0");
                controller.AtGameEnd();
                return;
            }
            if (buttonTray == null || instructionTray == null || instructionCards.Count != cardsToGuess || instructionLabel == null ||
                buttonPrefab == null || continueButton == null || controller == null)
            {
                Debug.LogError("Invalid input or initialization for game " + name + ", aborting");
                controller.AtGameEnd();
                return;
            }

            this.controller = controller;
            score = 0;
            maxScore = 2;

            currentPossibleCards.Clear();
            actualCards.Clear();

            for (int i = 0; i < totalButtonCount; i++)
            {
                GameObject buttonGo = Object.Instantiate(buttonPrefab, buttonTray.transform);
                buttons.Add(buttonGo.GetComponent<GameButton>());
            }

            foreach (CardData element in possibleCards)
            {
                currentPossibleCards.Add(element);
            }

            for (int i = 0; i < cardsToGuess; i++)
            {
                int newIndex = Random.Range(0, currentPossibleCards.Count);
                CardData newCard = currentPossibleCards[newIndex];
                //Debug.LogError(newCard);
                actualCards.Add(newCard);

                // OG Part:
                instructionCards[i] = newCard;
                //Debug.LogError(instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetType());
                instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite = newCard.cardImage;

                //// Testing Part:
                //instructionCards[i].Init(this, "");
                //instructionCards[i].addCard(newCard);
                //instructionCards[i].useBack();

                currentPossibleCards.Remove(newCard);
            }

            instructionLabel.text = initialInstruction;
            currentGuess = "";
            instructionTray.SetActive(true);


            chooseCards();


            //continueButton.Init(this, "Ok");
            //continueButton.gameObject.SetActive(true);
            //StartCoroutine(MainGameProcess());
        }

        private void chooseCards()
        {
            //int choose = 5;

            //while (choose > 0)
            //{
            //    Debug.LogError(choose);
            //    currentGuess = "";
            //    currentGuesser = null;

            //    while (currentGuesser == null)
            //    {
            //        yield return wait;
            //    }

            //    choose -= 1;

            //    for (int i = 0; i < cardsToGuess; i++)
            //    {
            //        if (currentGuesser == instructionCards[i])
            //        {
            //            StartCoroutine(FlipAnimation(180f, i));
            //        }
            //    }

            //    currentGuesser.useFront();
            //}


            //continueButton.gameObject.SetActive(false);

            //yield return Update();

            continueButton.Init(this, "Ok");
            continueButton.gameObject.SetActive(true);

            StartCoroutine(MainGameProcess());
        }

        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    for (int i = 0; i < cardsToGuess; i++)
            //    {
            //        if (!isFlipped)
            //        {

            //            StartCoroutine(FlipAnimation(180f, i));
            //            isFlipped = true;
            //        }
            //        else
            //        {
            //            StartCoroutine(FlipAnimation(0f, i));
            //            isFlipped = false;
            //        }
            //    }

            //}
            //Debug.LogError("Called Update!");

            if (Input.GetMouseButtonDown(0))
            {
                //Debug.LogError("Hit the left-click");
                //// Convert mouse position to world space
                //Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                ////RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

                ////// If the ray hits an object, flip the clicked object
                ////if (hit.collider != null)
                ////{
                ////    Debug.LogError("We hit the collider");
                ////    SpriteRenderer spriteRenderer = hit.collider.GetComponent<SpriteRenderer>();

                ////    // If the object has a SpriteRenderer, flip it
                ////    if (spriteRenderer != null)
                ////    {
                ////        Debug.LogError("SpriteRender: ", spriteRenderer);
                ////        spriteRenderer.flipX = !spriteRenderer.flipX; // Toggle flipX
                ////    }
                ////}

                //// Raycast to check if it hits any collider
                //RaycastHit3D hit = Physics3D.Raycast(mousePosition, Vector2.zero);

                //Debug.LogError(hit);
                //Debug.LogError(hit.collider);
                //Debug.LogError(hit.rigidbody);

                //if (hit.collider != null)
                //{

                //    Debug.LogError("We hit the collider");
                //    //    // Do something with the clicked object
                //    //    GameObject clickedObject = hit.collider.gameObject;

                //    //    if (clickedObject.GetComponentAtIndex(2).GetComponent<Image>().sprite == )

                //    //    //// Example: Change the color of the clicked object
                //    //    //clickedObject.GetComponent<SpriteRenderer>().color = Color.green;

                //    //    //// Log the object's name
                //    //    //Debug.Log("You clicked on: " + clickedObject.name);
                //    //}
                //}

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //RaycastHit hit;

                //Debug.LogError(ray);

                //if (Physics.Raycast(ray, out RaycastHit hit))
                ////if (hit.collider != null)
                //{

                //    Debug.LogError("hit" + hit.transform);
                //    if (hit.transform == transform)
                //    {
                //        Debug.Log("Touched or clicked on: " + gameObject.name);

                //        //// Example: Change material if provided
                //        //if (materials.Length > 0)
                //        //{
                //        //    currentMatIndex = (currentMatIndex + 1) % materials.Length;
                //        //    objRenderer.material = materials[currentMatIndex];
                //        //}
                //    }
                //}

                for(int i = 0; i < cardsToGuess; i++)
                {
                    //GameObject g = instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject;

                    //Debug.LogError("X position");
                    //Debug.LogError(ray.origin[0]);
                    //Debug.LogError(g.transform.position.x);
                    //Debug.LogError(g.transform.position.x + 150);

                    //Debug.LogError("Y position");
                    //Debug.LogError(ray.origin[1]);
                    //Debug.LogError(g.transform.position.y);
                    //Debug.LogError(g.transform.position.y + 80);

                    //if ((g.transform.position.x < ray.origin[0]) && (ray.origin[0] < g.transform.position.x + 150)
                    //    && (g.transform.position.y < ray.origin[1]) && (ray.origin[1] < g.transform.position.y + 80))
                    //{
                    //    Debug.LogError("Made it here");
                    //    if (instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite == instructionCards[i].cardImage)
                    //    {
                    //        instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite = instructionCards[i].backImage;
                    //    }
                    //    else
                    //    {
                    //        instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite = instructionCards[i].cardImage;
                    //    }
                    //}

                    //if (instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite == instructionCards[i].cardImage)
                    //{
                    //    instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite = instructionCards[i].backImage;
                    //}


                }

            }


            //continueButton.Init(this, "Ok");
            //continueButton.gameObject.SetActive(true);

            //StartCoroutine(MainGameProcess());

        }

        private IEnumerator FlipAnimation(float targetRotation, int i)
        {
            float elapsedTime = 0f;
            float startRotation = transform.eulerAngles.y;

            while (elapsedTime < flipDuration)
            {
                elapsedTime += Time.deltaTime;
                float newRotation = Mathf.Lerp(startRotation, targetRotation, elapsedTime / flipDuration);
                //transform.eulerAngles = new Vector3(transform.eulerAngles.x, newRotation, transform.eulerAngles.z);
                if (Mathf.Abs(newRotation - 90) < 1)
                {
                    if (instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite == instructionCards[i].cardImage)
                    {
                        instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite = instructionCards[i].backImage;
                    }
                    else
                    {
                        instructionTray.GetComponentAtIndex(1).transform.GetChild(i).gameObject.GetComponentAtIndex(2).GetComponent<Image>().sprite = instructionCards[i].cardImage;
                    }
                }
                yield return null;
            }
            //transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetRotation, transform.eulerAngles.z);
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
            List<CardData> cardAnswer = new List<CardData>(actualCards);

            //foreach (CardData card in cardsInProgress)
            //{

            //    Debug.LogError(card);
            //}

            //List<Sprite> cardAnswer = new List<Sprite>();
            //foreach (CardData card in cardsInProgress)
            //{
            //    cardAnswer.Add(card.cardImage);
            //}

            instructionLabel.text = instruction;
            yield return null;

            currentPossibleCards.Clear();
            currentPossibleCards.AddRange(possibleCards);
            guessableCards.Clear();

            // Get our five actual words
            foreach (CardData card in cardAnswer)
            {
                currentPossibleCards.Remove(card);
                guessableCards.Add(card);
            }

            // Fill out the delta of guessable words
            for (int i = cardsToGuess; i < totalButtonCount; i++)
            {
                int newIndex = Random.Range(0, currentPossibleCards.Count);
                CardData newCard = currentPossibleCards[newIndex];
                guessableCards.Add(newCard);
                currentPossibleCards.Remove(newCard);
            }

            // Now shuffle that field onto the buttons.  Each time we run this subloop, we should have
            // the actual words shuffled in with (totalButtons - actualWords) others to allow the user
            // to have a different field to guess from.
            for (int j = 0; j < totalButtonCount; j++)
            {
                int guessIndex = Random.Range(0, guessableCards.Count);
                CardData guessCard = guessableCards[guessIndex];
                buttons[j].Init(this, "");
                buttons[j].addCard(guessCard);
                //buttons[j].GetComponent < Image > = guessCard.cardImage;
                guessableCards.Remove(guessCard);
            }

            int guesses = cardsToGuess;
            buttonTray.SetActive(true);

            while (guesses > 0)
            {
                //Debug.LogError(guesses);
                currentGuess = "";
                currentGuesser = null;

                while (currentGuesser == null)
                {
                    yield return wait;
                }

                guesses -= 1;
                if (cardAnswer.Contains(currentGuesser.getCard()))
                {
                    cardAnswer.Remove(currentGuesser.getCard());
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
            if (cardAnswer.Count == 0)
            {
                score++;
            }
        }
    }
}