using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Image = UnityEngine.UI.Image;


/// <summary>
/// Simple jigsaw game. There is a limited selection of possible puzzles but the pieces are placed randomly.
/// The goal is to test spatial reasoning and visual matching. 
/// </summary>

namespace CognitiveTestEngine.Core { 
    public class JigsawGame : AbstractTestGame, IParentDialog
    {
        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private GameButton startButton;
        [SerializeField]
        private GameButton continueButton;
        [SerializeField]
        private RectTransform puzzleContainer;
        [SerializeField]
        private GameObject piecePrefab;
        [SerializeField]
        private int rows = 4, cols = 4;
        [SerializeField]
        private Sprite[] puzzleSprites;
        [SerializeField]
        private AudioSource correctSound;

        private CognitiveTestController controller;
        private List<JigsawPiece> pieces = new List<JigsawPiece>();
        private int piecesPlaced = 0;
        private GameObject backgroundObject;
        private float startTime;
           

        public override void StartGame(CognitiveTestController controller)
        {
            this.controller = controller;
            score = 0;
            maxScore = 5;

            instructionLabel.text = "Complete the jigsaw puzzle for the following picture:";
            instructionLabel.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(false);

            startButton.Init(this, "Start");
            startButton.gameObject.SetActive(true);

            var chosen = puzzleSprites[Random.Range(0, puzzleSprites.Length)];
            SetupPuzzle(chosen);
        }

        private void SetupPuzzle(Sprite fullImage)
        {
            foreach (Transform child in puzzleContainer)
            {
                 Destroy(child.gameObject);
            }

            pieces.Clear();
            piecesPlaced = 0;

            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(puzzleContainer, false);
            backgroundObject = bgGO;

            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImg = bgGO.GetComponent<Image>();
            bgImg.sprite = fullImage;
            bgImg.preserveAspect = true;
            bgGO.transform.SetAsFirstSibling();

            float w = puzzleContainer.rect.width / cols;
            float h = puzzleContainer.rect.height / rows;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var go = Instantiate(piecePrefab, puzzleContainer);
                    go.SetActive(false);

                    var img = go.GetComponent<Image>();
                    int x = (int)(fullImage.texture.width * (c / (float)cols));
                    int y = (int)(fullImage.texture.height * (r / (float)rows));
                    int tw = fullImage.texture.width / cols;
                    int th = fullImage.texture.height / rows;

                    var sub = Sprite.Create(
                        fullImage.texture,
                        new Rect(x, y, tw, th),
                        new Vector2(0.5f, 0.5f),
                        fullImage.pixelsPerUnit);
                    img.sprite = sub;

                    var piece = go.GetComponent<JigsawPiece>();
                    piece.correctAnchoredPos = new Vector2(
                        c * w + w / 2 - puzzleContainer.rect.width / 2,
                        r * h + h / 2 - puzzleContainer.rect.height / 2);
                    piece.rect.sizeDelta = new Vector2(w, h);

                    pieces.Add(piece);
                }
            }
        }
        
        public void OnButtonClick(GameObject sender, string text)
        {
            controller.PlayButtonClickSound();

            if (text == "Start")
            {
                startTime = Time.time;

                startButton.gameObject.SetActive(false);
                instructionLabel.gameObject.SetActive(false);

                if (backgroundObject != null)
                {
                    backgroundObject.SetActive(false);
                }

                foreach (var p in pieces)
                {
                    p.gameObject.SetActive(true);
                    p.rect.anchoredPosition = new Vector2(Random.Range(-puzzleContainer.rect.width / 2f, puzzleContainer.rect.width / 2f), Random.Range(-puzzleContainer.rect.height / 2f, puzzleContainer.rect.height / 2f));
                }
            }
            else if (text == "Continue")
            {
                controller.AtGameEnd();
            }
        }

        public void OnPiecePlacedCorrectly()
        {
            correctSound.Play();
            piecesPlaced++;

            if (piecesPlaced == pieces.Count)
            {
                float elapsed = Time.time - startTime;
                score = CalculateTimeScore(elapsed);

                instructionLabel.text = "Completed!";
                instructionLabel.gameObject.SetActive(true);
                continueButton.Init(this, "Continue");
                continueButton.gameObject.SetActive(true);
            }
        }
        

        // scoring done by calculating how much time is taken to complete the puzzle.
        // They should take 1.5 minutes. Max 5 pts. 
        private int CalculateTimeScore(float elapsedSeconds)
        {
            if (elapsedSeconds <= 90f) return 5;
            if (elapsedSeconds <= 150f)
            {
                float t = (elapsedSeconds - 90f) / 60f;
                float val = Mathf.Lerp(5f, 3f, t);
                return Mathf.RoundToInt(val);
            }
            if (elapsedSeconds <= 210f)
            {
                float t = (elapsedSeconds - 150f) / 60f;
                float val = Mathf.Lerp(3f, 0f, t);
                return Mathf.RoundToInt(val);
            }
            return 0;
        }
    }
}
