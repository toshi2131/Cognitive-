namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;

    public class databaseManager : MonoBehaviour, IParentDialog
    {
        [SerializeField]
        private PrototypeTestController controller;
        [SerializeField]
        private GameButton saveButton;

        public void Awake()
        {
            saveButton.Init(this, "Save result");
        }

        public void OnButtonClick(GameObject sender, string text)
        {
            controller.PlayButtonClickSound();    
        }
    }
}