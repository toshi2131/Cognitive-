using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace CognitiveTestEngine.Core
{
    public class SequenceMemory : AbstractTestGame
    {
        [Header("Grid Settings")]
        [SerializeField] private GameObject boxPrefab;
        [SerializeField] private Transform gridParent;
        [SerializeField] private int maxLevel = 5;
        [SerializeField] private float flashDuration = 0.5f;
        [SerializeField] private float flashDelay = 0.3f;

        [Header("Colors")]
        [SerializeField] private Color defaultColor = new Color(0.13f, 0.91f, 0.35f, 1f);
        [SerializeField] private Color flashColor = Color.white;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI levelIndicator;
        [SerializeField] private TextMeshProUGUI instructionLabel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;

        private List<Button> buttons = new List<Button>();
        private List<int> sequence = new List<int>();
        private List<int> input = new List<int>();
        private bool inputEnabled = false;
        private int currentLevel = 1;
        private Button continueButton;
        private CognitiveTestController controller;

        public override void StartGame(CognitiveTestController controller)
        {
            this.controller = controller;
            InitializeGame();
        }

        private void InitializeGame()
        {
            currentLevel = 1;
            sequence.Clear();
            input.Clear();
            SetupGrid();
            SetupUI();
            StartCoroutine(PlaySequence());
        }

        private void SetupGrid()
        {
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);
            buttons.Clear();

            for (int i = 0; i < 9; i++)
            {
                GameObject box = Instantiate(boxPrefab, gridParent);

                RectTransform rt = box.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Button btn = box.GetComponent<Button>();
                btn.name = $"Box_{i}";
                btn.image.color = defaultColor;
                int index = i;
                btn.onClick.AddListener(() => OnBoxClicked(index));
                buttons.Add(btn);
            }
        }

        private void SetupUI()
        {
            gameOverPanel?.SetActive(false);

            continueButton = gameOverPanel?.GetComponentInChildren<Button>();
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinueClicked);
                continueButton.gameObject.SetActive(false);
            }

            if (instructionLabel != null)
                instructionLabel.text = "Repeat the pattern";

            UpdateLevelText();
        }

        private void UpdateLevelText()
        {
            if (levelIndicator != null)
                levelIndicator.text = $"Level {currentLevel}";
        }

        private IEnumerator PlaySequence()
        {
            inputEnabled = false;
            input.Clear();
            UpdateLevelText();

            if (instructionLabel != null)
                instructionLabel.text = "Watch closely...";

            if (sequence.Count < currentLevel)
                sequence.Add(Random.Range(0, 9));

            yield return new WaitForSeconds(0.75f);

            foreach (int index in sequence)
            {
                controller?.PlayButtonClickSound();
                yield return StartCoroutine(FlashButton(index));
                yield return new WaitForSeconds(flashDelay);
            }

            if (instructionLabel != null)
                instructionLabel.text = "Repeat the pattern";

            inputEnabled = true;
        }

        private IEnumerator FlashButton(int index)
        {
            if (index < 0 || index >= buttons.Count) yield break;

            var btn = buttons[index];
            btn.image.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            btn.image.color = defaultColor;
        }

        private void OnBoxClicked(int index)
        {
            if (!inputEnabled) return;

            controller?.PlayButtonClickSound();
            StartCoroutine(FlashClick(index));

            if (index != sequence[input.Count])
            {
                HandleGameOver();
                return;
            }

            input.Add(index);

            if (input.Count == sequence.Count)
                StartCoroutine(NextLevel());
        }

        private IEnumerator FlashClick(int index)
        {
            var btn = buttons[index];
            btn.image.color = flashColor;
            yield return new WaitForSeconds(0.2f);
            btn.image.color = defaultColor;
        }

        private IEnumerator NextLevel()
        {
            inputEnabled = false;
            controller?.PlayCoinSound();
            yield return new WaitForSeconds(0.8f);

            currentLevel++;
            if (currentLevel > maxLevel)
            {
                HandleGameOver(true);
            }
            else
            {
                StartCoroutine(PlaySequence());
            }
        }

        private void HandleGameOver(bool completed = false)
        {
            inputEnabled = false;
            controller?.PlayWrongAnswerSound();

            score = Mathf.RoundToInt(((float)(currentLevel - 1) / maxLevel) * 10);
            maxScore = 10;

            gameOverText.text = completed
                ? $"You completed all {maxLevel} levels!"
                : $"You reached Level {currentLevel}!";

            gameOverPanel?.SetActive(true);
            continueButton?.gameObject.SetActive(true);
        }

        private void OnContinueClicked()
        {
            controller?.PlayButtonClickSound();
            controller?.AtGameEnd();
        }

        private void OnDestroy()
        {
            continueButton?.onClick.RemoveAllListeners();
        }
    }
}
