using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveTestEngine.Core
{
    public class CrossWord : AbstractTestGame, IParentDialog
    {

        [SerializeField]
        private TextMeshProUGUI instructionLabel;
        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private GameButton continueButton;

        [SerializeField] private Transform[] LetterDrops;
        [SerializeField] private TextMeshProUGUI[] DropsText;

        [SerializeField] private TextMeshProUGUI[] DropHints;


        [SerializeField] private Transform[] LetterDrags;
        [SerializeField] private TextMeshProUGUI[] DragsText;

        [SerializeField] private TextMeshProUGUI Hints;

        public static bool crossBool;
        public static int PuzzleNum;



        private string currentGuess = "";
        private GameButton currentGuesser = null;
        private CognitiveTestController controller;
        private WaitForEndOfFrame waitGeneral = new WaitForEndOfFrame();



        public void OnButtonClick(GameObject sender, string text)
        {
            currentGuess = text;
            currentGuesser = sender.GetComponent<GameButton>();
        }

        public override void StartGame(CognitiveTestController controller)
        {
            PuzzleNum = 0;
            crossBool = true;
            score = 0;


            for (int i = 0; i < 20; i++)
            {
                LetterDrops[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < 6; i++)
            {
                LetterDrags[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < 20; i++)
            {
                DropHints[i].gameObject.SetActive(true);
            }

            continueButton.Init(this, "Continue");

            continueButton.gameObject.SetActive(true);

            PuzzleNum = PuzzlePicker();

            PuzzleInitializer(PuzzleNum);


            this.controller = controller;
            instructionLabel.gameObject.SetActive(true);
            Hints.gameObject.SetActive(true);

            StartCoroutine(MainGameProcess());
        }

        public int PuzzlePicker()
        {
            //pick the puzzle randomly 0-5
            int Puzzle = Random.Range(0, 6);
            return Puzzle;
        }

        public void Loader(int zone, int dragged, int Puzzle)
        {
            DropsText[zone].text = DragsText[dragged].text;
            CheckResults(Puzzle);
        }

        public void PuzzleInitializer(int Puzzle)
        {
            if (Puzzle == 0)
            {
                int[] activeDropIndices = { 4, 5, 6, 7, 8, 9, 11, 14, 16 };
                foreach (int index in activeDropIndices)
                {
                    LetterDrops[index].gameObject.SetActive(true);
                    DropZone dz = LetterDrops[index].GetComponent<DropZone>();
                    if (dz != null)
                    {
                        dz.dropZoneId = index;
                        dz.c = this;
                    }
                }


                for (int i = 0; i < 6; i++)
                {
                    LetterDrags[i].gameObject.SetActive(true);
                    Drag dragScript = LetterDrags[i].GetComponent<Drag>();
                    if (dragScript != null)
                    {
                        dragScript.dragId = i;
                    }

                    if (i < DragsText.Length)
                    {
                        if (i == 0) DragsText[i].text = $"E";
                        else if (i == 1) DragsText[i].text = $"S";
                        else if (i == 2) DragsText[i].text = $"L";
                        else if (i == 3) DragsText[i].text = $"P";
                        else if (i == 4) DragsText[i].text = $"O";
                        else if (i == 5) DragsText[i].text = $"G";
                    }

                }


                Hints.text = $"1 Down:\nLiquid used to stiffen hair\n2 Across:\nIncline/Decline\n3 Down:\nPiece of Wood";

                DropHints[4].text = $"1";
                DropHints[5].text = $"2";
                DropHints[6].text = $"3";

            }

            else if (Puzzle == 1)
            {
                int[] activeDropIndices = { 0, 1, 2, 3, 4, 7, 9, 12, 14, 17, 18, 19 };

                foreach (int index in activeDropIndices)
                {
                    LetterDrops[index].gameObject.SetActive(true);
                    DropZone dz = LetterDrops[index].GetComponent<DropZone>();
                    if (dz != null)
                    {
                        dz.dropZoneId = index;
                        dz.c = this;
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    LetterDrags[i].gameObject.SetActive(true);
                    Drag dragScript = LetterDrags[i].GetComponent<Drag>();
                    if (dragScript != null)
                    {
                        dragScript.dragId = i;
                    }

                    if (i < DragsText.Length)
                    {
                        if (i == 0) DragsText[i].text = $"A";
                        else if (i == 1) DragsText[i].text = $"E";
                        else if (i == 2) DragsText[i].text = $"L";
                        else if (i == 3) DragsText[i].text = $"T";
                        else if (i == 4) DragsText[i].text = $"S";
                    }
                }

                Hints.text = $"1 Across:\nStories\n2 Down:\nFewer\n3 Down:\nAn Exchange\n4 Across:\nView";
                DropHints[0].text = $"1";
                DropHints[2].text = $"2";
                DropHints[4].text = $"3";
                DropHints[17].text = $"4";

            }

            else if (Puzzle == 2)
            {
                int[] activeDropIndices = { 0, 1, 2, 3, 5, 7, 10, 11, 12, 13, 17 };

                foreach (int index in activeDropIndices)
                {
                    LetterDrops[index].gameObject.SetActive(true);
                    DropZone dz = LetterDrops[index].GetComponent<DropZone>();
                    if (dz != null)
                    {
                        dz.dropZoneId = index;
                        dz.c = this;
                    }
                }

                for (int i = 0; i < 6; i++)
                {
                    LetterDrags[i].gameObject.SetActive(true);
                    Drag dragScript = LetterDrags[i].GetComponent<Drag>();
                    if (dragScript != null)
                    {
                        dragScript.dragId = i;
                    }

                    if (i < DragsText.Length)
                    {
                        if (i == 0) DragsText[i].text = $"O";
                        else if (i == 1) DragsText[i].text = $"E";
                        else if (i == 2) DragsText[i].text = $"D";
                        else if (i == 3) DragsText[i].text = $"R";
                        else if (i == 4) DragsText[i].text = $"N";
                        else if (i == 5) DragsText[i].text = $"T";
                    }
                }

                Hints.text = $"1 Across:\nMount(Past Tense)\n1 Down:\nA Color\n2 Down:\nStriking Metal Will Leave a ____\n3 Across:\nComplete";
                DropHints[0].text = $"1";
                DropHints[2].text = $"2";
                DropHints[10].text = $"3";
            }

            else if (Puzzle == 3)
            {
                int[] activeDropIndices = { 0, 1, 2, 3, 5, 8, 10, 11, 12, 13, 15 };

                foreach (int index in activeDropIndices)
                {
                    LetterDrops[index].gameObject.SetActive(true);
                    DropZone dz = LetterDrops[index].GetComponent<DropZone>();
                    if (dz != null)
                    {
                        dz.dropZoneId = index;
                        dz.c = this;
                    }
                }

                for (int i = 0; i < 6; i++)
                {
                    LetterDrags[i].gameObject.SetActive(true);
                    Drag dragScript = LetterDrags[i].GetComponent<Drag>();
                    if (dragScript != null)
                    {
                        dragScript.dragId = i;
                    }

                    if (i < DragsText.Length)
                    {
                        if (i == 0) DragsText[i].text = $"T";
                        else if (i == 1) DragsText[i].text = $"S";
                        else if (i == 2) DragsText[i].text = $"O";
                        else if (i == 3) DragsText[i].text = $"N";
                        else if (i == 4) DragsText[i].text = $"P";
                        else if (i == 5) DragsText[i].text = $"E";
                    }
                }

                Hints.text = $"1 Across:\nAn Act Of Movement\n1 Down:\nCome To an End\n2 Down:\nA Writing Utensil\n3 Across:\nUnclose";
                DropHints[0].text = $"1";
                DropHints[3].text = $"2";
                DropHints[10].text = $"3";

            }

            else if (Puzzle == 4)
            {
                int[] activeDropIndices = { 0, 1, 2, 3, 4, 5, 7, 9, 10, 12, 14, 15, 17, 19 };

                foreach (int index in activeDropIndices)
                {
                    LetterDrops[index].gameObject.SetActive(true);
                    DropZone dz = LetterDrops[index].GetComponent<DropZone>();
                    if (dz != null)
                    {
                        dz.dropZoneId = index;
                        dz.c = this;
                    }
                }

                for (int i = 0; i < 6; i++)
                {
                    LetterDrags[i].gameObject.SetActive(true);
                    Drag dragScript = LetterDrags[i].GetComponent<Drag>();
                    if (dragScript != null)
                    {
                        dragScript.dragId = i;
                    }

                    if (i < DragsText.Length)
                    {
                        if (i == 0) DragsText[i].text = $"C";
                        else if (i == 1) DragsText[i].text = $"B";
                        else if (i == 2) DragsText[i].text = $"T";
                        else if (i == 3) DragsText[i].text = $"S";
                        else if (i == 4) DragsText[i].text = $"A";
                        else if (i == 5) DragsText[i].text = $"I";
                    }
                }

                Hints.text = $"1 Across:\nFundamental\n1 Down:\nPieces\n2 Down:\nBroken Skin Leaves A ____\n3 Down:\nFeline (Plural)";
                DropHints[0].text = $"1";
                DropHints[2].text = $"2";
                DropHints[4].text = $"3";
            }

            else if (Puzzle == 5)
            {
                int[] activeDropIndices = { 2, 5, 6, 7, 8, 9, 12, 15, 16, 17, 18, 19 };

                foreach (int index in activeDropIndices)
                {
                    LetterDrops[index].gameObject.SetActive(true);
                    DropZone dz = LetterDrops[index].GetComponent<DropZone>();
                    if (dz != null)
                    {
                        dz.dropZoneId = index;
                        dz.c = this;
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    LetterDrags[i].gameObject.SetActive(true);
                    Drag dragScript = LetterDrags[i].GetComponent<Drag>();
                    if (dragScript != null)
                    {
                        dragScript.dragId = i;
                    }

                    if (i < DragsText.Length)
                    {
                        if (i == 0) DragsText[i].text = $"N";
                        else if (i == 1) DragsText[i].text = $"T";
                        else if (i == 2) DragsText[i].text = $"S";
                        else if (i == 3) DragsText[i].text = $"E";
                        else if (i == 4) DragsText[i].text = $"C";
                    }
                }

                Hints.text = $"1 Down:\nMeshed Material Used For Fishing (Plural)\n2 Across:\nSmell\n3 Across:\nExams";
                DropHints[2].text = $"1";
                DropHints[5].text = $"2";
                DropHints[15].text = $"3";

            }

            else if (Puzzle == 6)
            {
                //new puzzle
            }

            else if (Puzzle == 7)
            {
                //new puzzle
            }

        }

        public void CheckResults(int Puzzle)
        {
            if (Puzzle == 0)
            {
                score = 0;
                maxScore = 9;
                if (DropsText[4].text == DragsText[5].text)
                {
                    score++;
                }
                if (DropsText[5].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[6].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[7].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[8].text == DragsText[3].text)
                {
                    score++;
                }
                if (DropsText[9].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[11].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[14].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[16].text == DragsText[5].text)
                {
                    score++;
                }
                Debug.Log("Score is: " + score);
            }

            else if (Puzzle == 1)
            {
                score = 0;
                maxScore = 12;
                if (DropsText[0].text == DragsText[3].text)
                {
                    score++;
                }
                if (DropsText[1].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[2].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[3].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[4].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[7].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[9].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[12].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[14].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[17].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[18].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[19].text == DragsText[1].text)
                {
                    score++;
                }
                Debug.Log("Score is: " + score);
            }

            else if (Puzzle == 2)
            {
                score = 0;
                maxScore = 11;
                if (DropsText[0].text == DragsText[3].text)
                {
                    score++;
                }
                if (DropsText[1].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[2].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[3].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[5].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[7].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[10].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[11].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[12].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[13].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[17].text == DragsText[5].text)
                {
                    score++;
                }
                Debug.Log("Score is: " + score);
            }
            else if (Puzzle == 3)
            {
                score = 0;
                maxScore = 11;
                if (DropsText[0].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[1].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[2].text == DragsText[5].text)
                {
                    score++;
                }
                if (DropsText[3].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[5].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[8].text == DragsText[5].text)
                {
                    score++;
                }
                if (DropsText[10].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[11].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[12].text == DragsText[5].text)
                {
                    score++;
                }
                if (DropsText[13].text == DragsText[3].text)
                {
                    score++;
                }
                if (DropsText[15].text == DragsText[4].text)
                {
                    score++;
                }
                Debug.Log("Score is: " + score);
            }
            else if (Puzzle == 4)
            {
                score = 0;
                maxScore = 14;
                if (DropsText[0].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[1].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[2].text == DragsText[3].text)
                {
                    score++;
                }
                if (DropsText[3].text == DragsText[5].text)
                {
                    score++;
                }
                if (DropsText[4].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[5].text == DragsText[5].text)
                {
                    score++;
                }
                if (DropsText[7].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[9].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[10].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[12].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[14].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[15].text == DragsText[3].text)
                {
                    score++;
                }
                if (DropsText[17].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[19].text == DragsText[3].text)
                {
                    score++;
                }
                Debug.Log("Score is: " + score);
            }

            else if (Puzzle == 5)
            {
                score = 0;
                maxScore = 12;
                if (DropsText[2].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[5].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[6].text == DragsText[4].text)
                {
                    score++;
                }
                if (DropsText[7].text == DragsText[3].text)
                {
                    score++;
                }
                if (DropsText[8].text == DragsText[0].text)
                {
                    score++;
                }
                if (DropsText[9].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[12].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[15].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[16].text == DragsText[3].text)
                {
                    score++;
                }
                if (DropsText[17].text == DragsText[2].text)
                {
                    score++;
                }
                if (DropsText[18].text == DragsText[1].text)
                {
                    score++;
                }
                if (DropsText[19].text == DragsText[2].text)
                {
                    score++;
                }
                Debug.Log("Score is: " + score);
            }
            else if (Puzzle == 6)
            {

            }
            else if (Puzzle == 7)
            {

            }

        }




        protected IEnumerator MainGameProcess()
        {
            yield return null;
            while (string.Equals(currentGuess, ""))
            {
                yield return waitGeneral;
            }

            crossBool = false;


            continueButton.gameObject.SetActive(false);
            currentGuess = "";
            currentGuesser = null;

            controller.AtGameEnd();

        }
    }
}
