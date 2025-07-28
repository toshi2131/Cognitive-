namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using Core;
    using UnityEditor;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using System.Collections;
    using UnityEngine.Networking;

    public class PrototypeTestController : CognitiveTestController
    {
        [SerializeField]
        protected GameObject introPanel;
        [SerializeField]
        protected GameObject scorePanel;
        [SerializeField]
        protected GameObject startPanel;
        [SerializeField]
        protected bool export = false;

        protected bool startButtonClicked = false;
        protected GameObject introDialog;

        protected List<PrototypeGameConfig> exportConfigs = new List<PrototypeGameConfig>();

        public override void ReportScores()
        {
            if (scorePanel != null)
            {
                gameDuration = Time.time - gameDuration;
                Debug.Log("Game Duration: " + gameDuration);
                StartCoroutine(PostScoreToServer());
                scorePanel.SetActive(true);
            }
        }

        public override void ShowIntro()
        {
            if (introPanel != null)
            {
                introPanel.SetActive(true);
            }
        }

        public void OnIntroClosed()
        {
            StartGames();
        }

        public override void StartGames()
        {
            AtGameEnd();
        }
        public override void AtGameEnd()
        {
            if (export && currentGame != null && exportConfigs != null)
            {
                PrototypeGameConfig config = currentGame.ExportConfiguration() as PrototypeGameConfig;
                if (config != null)
                {
                    exportConfigs.Add(config);
                }
            }

            base.AtGameEnd();
        }
        private IEnumerator PostScoreToServer()
        {
            WWWForm form = new WWWForm();
            form.AddField("total_score", totalScore);
            form.AddField("max_score", totalPossibleScore);
            if (token != null)
            {
                form.AddField("token", token);
            } 
            form.AddField("test_duration", gameDuration.ToString());
            using (UnityWebRequest www = UnityWebRequest.Post("http://18.219.193.100:5000/session/total_score", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    Debug.LogError("Save failed: " + www.error);
                else
                    Debug.Log("Score saved!");
            }
        }
        
        public override void setPlayerName(string player_name)
        {
            PlayerName = player_name;
        }
        public void OnScorePanelClosed()
        {
            if (export && exportConfigs != null)
            {
                PrototypeGameManifest manifest = new PrototypeGameManifest(exportConfigs);
                string configExport = JsonConvert.SerializeObject(manifest);
                Debug.Log("Configs: " + configExport);
            }
            scorePanel.SetActive(false);
            startPanel.SetActive(true);
            //Application.Quit();
//#if UNITY_EDITOR
  //          EditorApplication.ExitPlaymode();
//#endif
        }
    }
}