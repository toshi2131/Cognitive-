namespace CognitiveTestEngine.Core
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.Networking;
    using UnityEngine.UIElements;

    /// <summary>
    /// Abstract implementation of a controller class that operates the top level of the test game.
    /// This is the "controller" of the model-view-controller design pattern.  This handles basic
    /// functionality for operating the flow of a single test session, but functional implementations
    /// should handle any extra behaviors such as further UI, help UI, windowing, ambient sound, or whatever.
    /// </summary>
    public abstract class CognitiveTestController : MonoBehaviour
    {
        /// <summary>
        /// The total score accumulated by the player in this session
        /// </summary>
        public int totalScore { get; set; }
        /// <summary>
        /// The total possible score that could be achieved by the player if they played the entire session perfectly
        /// </summary>
        public int totalPossibleScore { get; set; }
        public string PlayerName { get; set; }
        public string gameType { get; set; }    
        public int sessionID { get; set; }
        public string token { get; set; }
        public float gameDuration { get; set; }
        /// <summary>
        /// The data interface and builder that will provide games to play in the session
        /// </summary>
        public AbstractCognitiveTestData testDataSource;

        /// <summary>
        /// The most recent data being tracked for a delayed recall game.
        /// </summary>
        public List<string> lastDelayedRecallData { get; protected set; }

        /// <summary>
        /// The anchor transform to attach games to.
        /// </summary>
        public RectTransform anchor;

        /// <summary>
        /// Sounds to play when the player clicks things or gets right or wrong answers.  This is provided here
        /// so that there is a common interface for the basic bells and whistles.  The coin and wrong answer sounds
        /// are generally intended for positive and negative in-game feedback, while the button click is intended to
        /// be a mundane sound played when non-game UI is interacted with.
        /// </summary>
        [SerializeField]
        protected AudioSource coinSound;
        [SerializeField]
        protected AudioSource wrongAnswerSound;
        [SerializeField]
        protected AudioSource buttonClickSound;
        
        /// <summary>
        /// The current game in progress.
        /// </summary>
        protected AbstractTestGame currentGame;

        /// <summary>
        /// Start method to run the game
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator Start()
        {
            totalScore = 0;
            totalPossibleScore = 0;

            if (testDataSource != null)
            {
                yield return testDataSource.PopulateGames();
            }
            ShowIntro();
        }

        /// <summary>
        /// Callback to be used by minigames to indicate that the player has completed playing them, and
        /// final scores (actual and possible) are available to be tallied.  Cleans up the current game and
        /// starts the next.  If the last game has been played, starts the final reporting process.
        /// </summary>
        public virtual void AtGameEnd()
        {
            if (currentGame != null)
            {
                CleanupGame(currentGame);
                currentGame = null;
            }

            currentGame = PopulateNewGame();

            if (currentGame != null)
            {
                currentGame.StartGame(this);
            }
            else
            {
                ReportScores();
            }
        }

        /// <summary>
        /// Tally the score for a game and clean up after it to make room for the next one.
        /// </summary>
        /// <param name="game">The game to be scored and cleaned</param>
        public virtual void CleanupGame(AbstractTestGame game)
        {
            totalScore += game.score;
            totalPossibleScore += game.maxScore;
            if (game.gameType != null)
            {
                StartCoroutine(SaveGameData(game.gameType, game.score)); 
            }
            else
            {
                gameType = "Unknown";
            }
            

            List<string> recallData = game.GetDelayedRecallData();
            if (recallData != null && recallData.Count > 0)
            {
                lastDelayedRecallData = recallData;
            }

            if (game.DischargeRecallData())
            {
                lastDelayedRecallData.Clear();
            }

            Object.Destroy(game.gameObject);
        }

        public virtual IEnumerator SaveGameData(string gameType, int score)
        {

            WWWForm form = new WWWForm();
            form.AddField("gameType", gameType);
            form.AddField("sessionID", sessionID);
            form.AddField("score", score);
            using (UnityWebRequest www = UnityWebRequest.Post("http://18.219.193.100:5000/session/games", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    Debug.Log("Save failed: " + www.error);
                else
                    Debug.Log("Game data saved");
            }
        }
        /// <summary>
        /// Get the next game to be played from the data/builder object.  When this method returns,
        /// any new games available should be attached to the anchor point and ready to go.
        /// 
        /// Intentionally uses the abstract level so that it remains implementation-agnostic at the game level.
        /// </summary>
        /// <returns>The AbstractTestGame object for the new game if there is another to be played, null if the player
        /// has completed the last game of the session.</returns>
        public virtual AbstractTestGame PopulateNewGame()
        {
            AbstractTestGame game = null;

            if (testDataSource != null)
            {
                while (true)
                {
                    GameObject newViewObject = testDataSource.GetNextGameView(anchor);
                    if (newViewObject != null)
                    {
                        game = newViewObject.GetComponent<AbstractTestGame>();
                        if (game == null)
                        {
                            Object.DestroyImmediate(newViewObject);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        game = null;
                        break;
                    }
                }
            }

            return game;
        }

        /// <summary>
        /// Show any introductory UI that serves as initial logo, tutorial, welcome messaging, etc.
        /// All implementations of this function should call StartGames() when all the introductory
        /// UI process has been completed.
        /// </summary>
        public abstract void ShowIntro();

        /// <summary>
        /// Start the main game session.  All iteration points through the game sequence from here will
        /// generally through the AtGameEnd() callback, although some implementations of this method might
        /// have more sophisticated flows as needed.
        /// </summary>
        public abstract void StartGames();
        
        /// <summary>
        /// Call to be made when all games in the session have been completed, and it is time to report any
        /// scoring to everyone who needs to receive it (whether simply showing the player how they did, and/or
        /// sending any data to any scientific or medical staff that may be monitoring the test.
        /// </summary>
        public abstract void ReportScores();

        public abstract void setPlayerName(string name);
        /// <summary>
        /// Play a "coin sound" indicating that the player has made a successful in-game action.  This base method simply
        /// places one sound, but if more complex sound interactions (e.g. calling from a range of several sounds
        /// instead of just one) are desired, this method may be overridden.  One possible override could be to rotate
        /// the sound between a random list that is exposed in the implementing class, and rotate the sound over to
        /// the next one in line after one is played.
        /// </summary>
        public virtual void PlayCoinSound()
        {
            if (coinSound != null)
            {
                coinSound.Play();
            }
        }

        /// <summary>
        /// Play a sound indicating that the player has made a wrong choice during the game.  As with the coin,
        /// this plays a single action but can be overridden if more complex sound behavior is desired.
        /// </summary>
        public virtual void PlayWrongAnswerSound()
        {
            if (wrongAnswerSound != null)
            {
                wrongAnswerSound.Play();
            }
        }

        /// <summary>
        /// Play a generic clicking sound indicating that the player has clicked a button.  This should be
        /// reserved for non-game actions so that they don't seem "dead" or otherwise silent, and should
        /// generally not be used for anything that the player is doing right or wrong in the game.  Should
        /// be as neutral a sound as possible.
        /// </summary>
        public virtual void PlayButtonClickSound()
        {
            if (buttonClickSound != null)
            {
                buttonClickSound.Play();
            }
        }

        public void startTimer()
        {
            gameDuration = Time.time;
        }
    }
}