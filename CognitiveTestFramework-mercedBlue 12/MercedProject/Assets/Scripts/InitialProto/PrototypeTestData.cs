namespace CognitiveTestEngine.Prototype
{
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Core;

    [Serializable]
    public class PrototypeGameMapping
    {
        public string type;
        public GameObject prefab;

        public PrototypeGameMapping(string type, GameObject go)
        {
            this.type = type;
            this.prefab = go;
        }
    }

    [Serializable]
    public class PrototypeGameConfig : AbstractGameConfig
    {
        public List<string> instructions;
        public List<string> stringParams;
        public List<int> intParams;
        public List<bool> boolParams;

        public PrototypeGameConfig(string s, List<string> instructions, List<string> strings, List<int> ints, List<bool> bools)
        {
            this.type = s;
            this.instructions = instructions;
            stringParams = strings;
            intParams = ints;
            boolParams = bools;
        }
    }

    public class PrototypeGameManifest
    {
        public List<PrototypeGameConfig> games;
        public PrototypeGameManifest(List<PrototypeGameConfig> configs)
        {
            games = configs;
        }
    }

    public class PrototypeTestData : AbstractCognitiveTestData
    {
        [SerializeField]
        public List<PrototypeGameMapping> prefabMap;

        [SerializeField]
        private List<GameObject> gamePrefabs = new List<GameObject>();
        [SerializeField]
        private string dataUrl;

        [SerializeField]
        private List<PrototypeGameConfig> configs;
        private Dictionary<string, GameObject> prefabInternalMap;

        private int index = 0;

        public override GameObject GetNextGameView(Transform anchor)
        {
            GameObject nextPrefab = null;

            if (configs != null)
            {
                // We have JSON data to use, use that.
                while(nextPrefab == null && index < configs.Count)
                {
                    PrototypeGameConfig nextConfig = configs[index];
                    if (nextConfig != null && prefabInternalMap.ContainsKey(nextConfig.type))
                    {
                        nextPrefab = UnityEngine.Object.Instantiate(prefabInternalMap[nextConfig.type], anchor);
                        AbstractTestGame testGame = nextPrefab.GetComponent<AbstractTestGame>();
                        if (testGame != null)
                        {
                            testGame.Configure(nextConfig);
                        }
                        else
                        {
                            nextPrefab = null;
                        }
                    }
                    index++;
                }
            }
            else
            {
                // We don't have JSON data to use, so don't.
                while (gamePrefabs != null && index < gamePrefabs.Count && nextPrefab == null)
                {
                    if (gamePrefabs[index] != null)
                    {
                        nextPrefab = UnityEngine.Object.Instantiate(gamePrefabs[index], anchor);
                    }
                    index++;
                }
            }

            return nextPrefab;
        }

        public override IEnumerator PopulateGames()
        {
            yield return null;

            if (!string.IsNullOrEmpty(dataUrl))
            {
                TextAsset textData = (TextAsset)Resources.Load(dataUrl);
                if (textData != null)
                {
                    string txt = textData.text;
                    try
                    {
                        PrototypeGameManifest manifest = JsonConvert.DeserializeObject<PrototypeGameManifest>(txt);
                        if (manifest != null)
                        {
                            configs = manifest.games;
                            prefabInternalMap = new Dictionary<string, GameObject>();
                            foreach(PrototypeGameMapping map in prefabMap)
                            {
                                prefabInternalMap.Add(map.type, map.prefab);
                            }
                        }
                    }
                    catch
                    {
                        Debug.LogError("Couldn't get manifest data, using fallback");
                        configs = null;
                    }
                }
                else
                {
                    Debug.LogError("Couldn't load dataUrl, using fallback");
                    configs = null;
                }
            }
            else
            {
                Debug.Log("No dataUrl detected, using fallback");
                configs = null;
            }
        }
    }
}