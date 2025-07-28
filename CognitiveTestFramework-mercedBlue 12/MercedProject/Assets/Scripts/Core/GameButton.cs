namespace CognitiveTestEngine.Core
{
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;

    /// <summary>
    /// Simple button behavior with a callback to its parent dialog.  Designed to be mass
    /// instantiated and transmit just enough information to let the parent dialog
    /// understand which button was pressed.
    /// 
    /// Unlike most of the other sample classes, this one is not abstracted.
    /// </summary>
    public class GameButton : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI label;
        [SerializeField]
        private Image buttonImage;
        [SerializeField]
        private Button button;
        [SerializeField]
        private Color32 neutralColor;

        [SerializeField]
        private Color32 successColor;

        [SerializeField]
        private Color32 failureColor;

        private IParentDialog parentGame = null;
        private CardData cardValue = null;

        /// <summary>
        /// Initialize the button and parent it to the game or other dialog that needs to know
        /// when it is clicked.  The labelText is intended for a text button, but it can also be
        /// used as a key for a graphic of some sort that might be pulled out of an atlas in a
        /// subclass.
        /// 
        /// If a button using this behavior is not interacting properly with its parent, it is probably because this
        /// method was not called.
        /// </summary>
        /// <param name="game">The parent of the button to be notified when pressed</param>
        /// <param name="labelText">Text for the button label or (if a graphical button subclass)
        /// a key to the graphic to be shown</param>
        public void Init(IParentDialog game, string labelText)
        {
            parentGame = game;
            label.text = labelText;
            if (buttonImage == null)
            {
                buttonImage = gameObject.GetComponent<Image>();
            }

            if (button == null)
            {
                button = gameObject.GetComponent<Button>();
            }

            // Intentionally not an else or else if
            if (button != null)
            {
                button.enabled = true;
            }
        }

        /// <summary>
        /// Should be attached to the button in the editor to be called when clicked.
        /// </summary>
        public void OnClick()
        {
            if (parentGame != null)
            {
                parentGame.OnButtonClick(gameObject, label.text);
            }
        }
        public void addCard(CardData card)
        {
            buttonImage.sprite = card.cardImage;
            cardValue = card;
        }

        public void useBack()
        {
            buttonImage.sprite = cardValue.backImage;
        }

        public void useFront()
        {
            buttonImage.sprite = cardValue.cardImage;
        }

        public Sprite getImage()
        {
            return buttonImage.sprite;
        }

        public CardData getCard()
        {
            return cardValue;
        }
        /// <summary>
        /// A call for the parent game or dialog to be executed when this was the right button to press
        /// in a game.
        /// </summary>
        public void OnSuccess()
        {
            if (buttonImage != null)
            {
                buttonImage.color = successColor;
            }

            if (button != null)
            {
                button.enabled = false;
            }
        }

        /// <summary>
        /// A call for the parent game or dialog to be executed when this was the wrong button to press
        /// in a game.
        /// </summary>
        public void OnFailure()
        {
            if (buttonImage != null)
            {
                buttonImage.color = failureColor;
            }

            if (button != null)
            {
                button.enabled = false;
            }
        }

        /// <summary>
        /// Enable and disable function.
        /// </summary>
        /// <param name="toEnable">True if the button should be enabled, else false.</param>
        public void Enable(bool toEnable)
        {
            button.enabled = toEnable;
        }

        /// <summary>
        /// Automatically called from Unity whenever this button is enabled, either by being made
        /// visible or enabled from the Enable() call.
        /// </summary>
        public void OnEnable()
        {
            if (buttonImage == null)
            {
                buttonImage = gameObject.GetComponent<Image>();
            }

            if (buttonImage != null)
            {
                buttonImage.color = neutralColor;
            }

            if (button != null)
            {
                button.enabled = true;
            }
        }
    }
}