namespace CognitiveTestEngine.Core
{
    /// <summary>
    /// An abstract data class containing the configuration data for a single test minigame.  This class
    /// has no abstract methods but should never actually be directly used.  This operates as the "model"
    /// of the model-view-controller design pattern.
    /// </summary>
    public abstract class AbstractGameConfig
    {
        /// <summary>
        /// The type key for the minigame.  This can be used by a TestData builder to instantiate a game.
        /// </summary>
        public string type;

        /// <summary>
        /// Null arg constructor
        /// </summary>
        public AbstractGameConfig()
        {
            type = "";
        }

        /// <summary>
        /// Single arg constructor.
        /// </summary>
        /// <param name="type"></param>
        public AbstractGameConfig(string type)
        {
            this.type = type;
        }
    }
}