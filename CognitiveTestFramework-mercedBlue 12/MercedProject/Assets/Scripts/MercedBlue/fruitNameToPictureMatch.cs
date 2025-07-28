namespace CognitiveTestEngine.Prototype
{
    using Core;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Schema;
    using UnityEngine;
    using UnityEngine.UI;

    public class ItemFalling : AbstractTestGame
    {
        [SerializeField] List<Image> fruitPrefabs; // Store multiple fruit UI prefabs
        [SerializeField] Transform GridTop;
        [SerializeField] Transform GridAnswer;
        [SerializeField] Transform GridBottom;
        [SerializeField] GameButton continueButton;
        [SerializeField] GameObject slotChoicesPrefab;
        [SerializeField] GameObject slotAnswerPrefab;
        [SerializeField] GameObject slotRandomPrefab; // Prefab for the slots

        [SerializeField] private string initialInstruction = "temp instruction";
        [SerializeField] private List<string> allPossibleFruitName = new List<string>();
        [SerializeField] private int totalWordsToGuess = 5;
        [SerializeField] private int totalRandomizedWords = 12;

        [SerializeField] float spawnRate = 0.3f;
        [SerializeField] float duration = .5f;
        [SerializeField] float interval = .1f;


        [SerializeField] private List<string> currentPossibleFruitsNames = new List<string>(); //changes based on what fruit is already used
        [SerializeField] private List<string> correctFruitNames = new List<string>();
        private CognitiveTestController controller;

        public Sprite[] fruitSprites; // Assign all fruit sprites
        [SerializeField] List<Sprite> correctFruitSprites = new List<Sprite>(); // Assign all fruit sprites
        [SerializeField] List<Sprite> currentPossibleFruitSprites = new List<Sprite>();

        public List<Sprite> GetFruitNames()
        {
            return correctFruitSprites;
        }
        public override void Configure(AbstractGameConfig configData)
        {
            PrototypeGameConfig protoData = configData as PrototypeGameConfig;
            if (protoData != null && protoData.instructions.Count == 1)
            {
                initialInstruction = protoData.stringParams[0];
            }
            else
            {
                Debug.LogError("Invalid configuration data: instructions");
                return;
            }

            if (protoData != null && protoData.intParams.Count == 2)
            {
                totalWordsToGuess = protoData.intParams[0];
                totalRandomizedWords = protoData.intParams[1];
            }
            else
            {
                Debug.LogError("Invalid configuration data: totalWordsToGuess");
                return;
            }

            if (protoData != null && protoData.stringParams.Count > 0)
            {
                allPossibleFruitName = protoData.stringParams;
            }
            else
            {
                Debug.LogError("Invalid configuration data: allPossibleFruitName");
                return;
            }
        }
        public override void StartGame(CognitiveTestController controller)
        {
            
            this.controller = controller;
            currentPossibleFruitsNames.Clear();
            foreach (string fruits in allPossibleFruitName) // Assign all possible fruit name to a new temp list
            {
                currentPossibleFruitsNames.Add(fruits);
            }

            for (int i = 0; i < 12; i++) 
            {
                int randomIndex = Random.Range(0, currentPossibleFruitsNames.Count);
                if (currentPossibleFruitsNames[randomIndex] == "X") //if the fruit name is already used
                {
                    i--;
                    Debug.Log("skipped");
                    continue; //skip this iteration
                }
                string tempWord = currentPossibleFruitsNames[randomIndex];
                Sprite tempSprites = fruitSprites[randomIndex]; // Select a random fruit sprite from the list
                currentPossibleFruitSprites.Add(tempSprites); //get the correct fruit sprite
                if (i < 5)
                {      
                    correctFruitSprites.Add(tempSprites); //get the 5 correct fruit sprite
                    correctFruitNames.Add(tempWord); //get the 5 correct fruit name
                }
                Debug.Log("fruit: " + tempWord + "\nIndex: " + randomIndex);
                currentPossibleFruitsNames[randomIndex] = "X"; //change the fruit names so it wont be used again
            }

            if (slotRandomPrefab != null && slotAnswerPrefab != null && slotChoicesPrefab != null) // add all the slots
            {
                for (int i = 0; i < totalWordsToGuess; i++)
                {
                    GameObject slotsRoll = Object.Instantiate(slotRandomPrefab, GridTop.transform);
                    GameObject slotAnswer = Object.Instantiate(slotAnswerPrefab, GridAnswer.transform);
                    Slot slotScript = slotAnswer.GetComponent<Slot>(); 
                    if (slotScript != null)
                    {
                        slotScript.slotIndex = i; 
                    }
                }
                //for (int i = 0; i < totalRandomizedWords; i++)
                //{
         
                //    GameObject slotsOptions = Object.Instantiate(slotChoicesPrefab, GridBottom.transform);
                //}
            }

            /*AssignRandomFruits();*/ // Assign random fruit sprites to the items in the gridBottom transform
            StartCoroutine(MainGameProcess());
        }

        IEnumerator MainGameProcess() //item falling script
        {
            float elapsedTime;
            int index = 0;
            foreach (Transform slot in GridTop)
            {
                elapsedTime = 0f;
                Transform placeholder = null;
                foreach (Transform child in slot)
                {
                    placeholder = slot.Find(child.name);
                }
                while (elapsedTime < duration)
                {
                    Image selectedFruitPrefab = fruitPrefabs[Random.Range(0, fruitPrefabs.Count)]; // Select a random fruit prefab from the list 
                    Image newFruit = Instantiate(selectedFruitPrefab, slot); // Instantiate a random fruit UI Image from the list
                    newFruit.rectTransform.anchoredPosition = new Vector2(0f, 150f); // Set UI position
                    yield return new WaitForSeconds(spawnRate);
                    Destroy(newFruit.gameObject, 1f); // Destroy UI fruit after time
                    elapsedTime += interval;
                }
                
                if (placeholder != null)
                {
                    Image placeholderImage = placeholder.GetComponent<Image>(); // Get the Image component

                    if (placeholderImage != null && fruitPrefabs.Count > 0)
                    {
                        yield return new WaitForSeconds(0.1f);
                        placeholderImage.sprite = correctFruitSprites[index]; // Change the sprite
                  
                    }
                }
                index++;
            }
            
            for (int i = 0; i < totalRandomizedWords; i++)
            {

                GameObject slotsOptions = Object.Instantiate(slotChoicesPrefab, GridBottom.transform);
            }
            AssignRandomFruits(); // Assign random fruit sprites to the items in the gridBottom transform
            while (!AllSlotsFilled()) // Wait until all slots are filled
            {
                yield return null; // Wait for the next frame
            }
            controller.AtGameEnd();
        }
        void AssignRandomFruits() //assign random fruits to the choices
        {
            foreach (Transform slot in GridBottom) // Loop through each slot in gridBottom
            {
                Transform item = slot.Find("Item"); // Find the "Item" inside the slot
                if (item != null) //if there is no item
                {
                    Image fruitImage = item.GetComponent<Image>(); // Get UI Image component
                    if (fruitImage != null && currentPossibleFruitSprites.Count > 0)
                    {
                        int randomIndex = Random.Range(0, currentPossibleFruitSprites.Count);
                        Debug.Log(randomIndex);
                        fruitImage.sprite = currentPossibleFruitSprites[randomIndex]; // Assign a random fruit sprite
                        currentPossibleFruitSprites.RemoveAt(randomIndex); // Remove the assigned sprite from the list
                    }
                }
            }
        }
        bool AllSlotsFilled()
        {
            foreach (Transform slot in GridAnswer.transform)
            {
                if (slot.childCount == 0)
                {
                    return false; // One is still empty
                }
            }
            return true; // All filled
        }
    }
}

//comment all ctrl K ctrl C
//uncomment all ctrl K ctrl U