namespace CognitiveTestEngine.Core
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An abstract MonoBehaviour that operates as the parent behavior for a single instance of a cognitive
    /// test minigame.  This is abstracted intentionally so that the controller classes can operate off this
    /// interface without needing to care what's actually under the hood.  This generally operates as the "view"
    /// of the model-view-controller design pattern, although there is some interactivity to it as well.
    /// </summary>
    public abstract class AbstractTestGame : MonoBehaviour
    {
        /// <summary>
        /// Method to start the game.  When the game is completed and ready for the player to move on, something
        /// that comes from here should call CognitiveTestController.AtGameEnd().  The controller will generally not do any
        /// further prodding between the start and end point of the game.
        /// </summary>
        /// <param name="controller">The controller object to report back to</param>
        public abstract void StartGame(CognitiveTestController controller);

        /// <summary>
        /// The actual player score that was obtained by playing the game.  This is generally not read until the
        /// end of the game, but if an implementation of CognitiveTestController reads it at more than one point
        /// (e.g. to update UI in progress) then it should be updated as often as needed.
        /// </summary>
        public int score { get; protected set; }

        /// <summary>
        /// The amount a player can score if they play the game perfectly.  This should be scaled so that the
        /// importance of each game within the larger test suite is weighted accordingly, e.g. a simpler game
        /// with a low number of actions should be scored lower than a bigger game.
        /// </summary>
        public int maxScore { get; protected set; }

        public string gameType { get; protected set; }
        /// <summary>
        /// If this game is a delayed recall type game, the data that will need to be recalled at a later time.
        /// This is implemented as a list of strings because the initial example is a word test, but it can be
        /// parsed into other data as needed.  A later game may discharge this data when the other side of the
        /// delayed recall game is done.  Generally, only one such delayed recall should be in effect at a time,
        /// but there's nothing preventing multiple such tests -- however, MoCA and BoCA both only use one per
        /// test session.
        /// </summary>
        /// <returns>If it's the opening game of a delayed recall test, the data to be recalled.  Else, null.</returns>
        public virtual List<string> GetDelayedRecallData()
        {
            return null;
        }

        /// <summary>
        /// If there is a delayed recall game in progress, flag that the other half of the recall has taken place.
        /// </summary>
        /// <returns>True if this game finished a delayed recall test, else false.</returns>
        public virtual bool DischargeRecallData()
        {
            return false;
        }

        /// <summary>
        /// Configure the game with its data.  Should be called by the TestData/builder code in instantiation.
        /// This function should be overridden for any minigame that has been developed past a hard-coded state.
        /// </summary>
        /// <param name="configData">The data with which to configure the game.</param>
        public virtual void Configure(AbstractGameConfig configData)
        {
        }

        /// <summary>
        /// Export sample data that could be used to initiate this minigame.  This is a helper method to enable
        /// initial building of data that can be serialized to a file.  This method need not be overridden in final
        /// production builds.
        /// </summary>
        /// <returns>The configuration data that drove this game that could be used to reproduce its current behavior</returns>
        public virtual AbstractGameConfig ExportConfiguration()
        {
            return null;
        }
    }
}