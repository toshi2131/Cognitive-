using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace CognitiveTestEngine.Core
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI winText;

        public void ShowWinMessage()
        {
            winText.text = $"It took you {Global2.guessesCount} guesses to complete the game.";
        }
    }
}
