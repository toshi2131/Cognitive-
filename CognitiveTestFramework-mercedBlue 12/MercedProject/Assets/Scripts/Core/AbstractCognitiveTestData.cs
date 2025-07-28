namespace CognitiveTestEngine.Core
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Abstract class for a data manifest processor that yields a series of fully configured games
    /// that can be played in order.  This can be lazily implemented as a hard coded list or generated
    /// from either internal or external configuration data, the abstraction allows any level of sophistication
    /// to this by design.
    /// 
    /// This operates, at minimum, as a builder design pattern, but is generally intended to also encapsulate
    /// the data handling as well.
    /// </summary>
    public abstract class AbstractCognitiveTestData : MonoBehaviour
    {
        /// <summary>
        /// Spawn the next game view and attach it to the anchor.  If there is data that needs to be
        /// used to configure the game, do this before returning it to the caller.
        /// </summary>
        /// <param name="anchor">The transform to attach the game object to</param>
        /// <returns>The raw gameobject being spawned</returns>
        public abstract GameObject GetNextGameView(Transform anchor);

        // Method to populate all the games.  Will return once all data is ready to go.
        // This can potentially go to an external file as well.
        public abstract IEnumerator PopulateGames();
    }
}