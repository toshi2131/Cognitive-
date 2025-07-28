using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CognitiveTestEngine.Core
{
    public class AddButtons2 : MonoBehaviour
    {
        [SerializeField] private Transform gameBoard;
        [SerializeField] private GameObject cardButton;

        private void Awake()
        {
            for(int i = 0; i < 8; i++)
            {
                GameObject button = Instantiate(cardButton);
                button.name = $"{i}";
                button.transform.SetParent(gameBoard, false);
            }
        }
    }
}