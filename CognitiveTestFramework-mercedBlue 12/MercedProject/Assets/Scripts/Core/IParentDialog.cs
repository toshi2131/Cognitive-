namespace CognitiveTestEngine.Core
{
    using UnityEngine;

    /// <summary>
    /// Basic interface for a parent dialog that should be notified when a button or other UI
    /// has been clicked.  Designed to be used with the GameButton class, but is actually
    /// agnostic to it if another UI MonoBehaviour wishes to use it.  This interface should
    /// be implemented by any parent game or dialog that wishes to receive callbacks when
    /// UI is interacted with by a player.
    /// 
    /// Inheritance interfaces to this may enable other behaviors, or it could be ignored
    /// altogether if other methods are desired.  It's here as a convenience method.
    /// </summary>
    public interface IParentDialog
    {
        /// <summary>
        /// Method to be called on the parent dialog when a button has been clicked.
        /// </summary>
        /// <param name="sender">The game object that sent the click</param>
        /// <param name="text">Any other identifying text information that may make the click
        /// more quickly useful to the parent (i.e. to save the trouble of doing callbacks to
        /// the UI that sent it to figure out what the parent should do with the click)</param>
        public void OnButtonClick(GameObject sender, string text);
    }
}