using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AbsolutePitchGame
{
    public class GameSceneManager : MonoBehaviour
    {
        private enum GamePhase
        {
            AwaitingPlayback,
            AwaitingAnswer,
            ShowingFeedback
        }

        private static readonly string[] NoteNames =
        {
            "C",
            "C#",
            "D",
            "D#",
            "E",
            "F",
            "F#",
            "G",
            "G#",
            "A",
            "A#",
            "B"
        };

        [Header("UI References")]
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private List<KeyboardButtonUI> noteButtons = new();

        [Header("Gameplay Settings")]
        [SerializeField] private float correctDelay = 1.5f;
        [SerializeField] private float incorrectDelay = 2.0f;
        [SerializeField] private bool repeatUntilCorrect = true;
        [SerializeField] private bool autoReplayAfterMiss = false;

        private int correctNoteIndex;
        private int score;
        private GamePhase currentPhase = GamePhase.AwaitingPlayback;
        private Coroutine feedbackRoutine;

        private void Start()
        {
            RegisterButtons();
            PrepareNextQuestion(true);
        }

        private void OnDisable()
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
                feedbackRoutine = null;
            }
        }

        public void HandlePlayNoteRequest()
        {
            PlayCurrentNote();

            if (currentPhase == GamePhase.AwaitingPlayback)
            {
                currentPhase = GamePhase.AwaitingAnswer;
                SetButtonsInteractable(true);
            }
        }

        public void SubmitAnswer(int noteIndex)
        {
            if (currentPhase != GamePhase.AwaitingAnswer)
            {
                return;
            }

            SetButtonsInteractable(false);

            if (noteIndex == correctNoteIndex)
            {
                HandleCorrectAnswer();
            }
            else
            {
                HandleIncorrectAnswer();
            }
        }

        public string GetNoteLabel(int index)
        {
            if (index < 0 || index >= NoteNames.Length)
            {
                return string.Empty;
            }

            return NoteNames[index];
        }

        private void RegisterButtons()
        {
            foreach (var button in noteButtons)
            {
                button?.BindGameManager(this);
                button?.SetInteractable(false);
            }
        }

        private void PrepareNextQuestion(bool pickNewNote)
        {
            if (pickNewNote)
            {
                correctNoteIndex = Random.Range(0, NoteNames.Length);
            }

            currentPhase = GamePhase.AwaitingPlayback;
            SetButtonsInteractable(false);
            UpdateFeedback("音を再生して、鍵盤を押してください");
        }

        private void PlayCurrentNote()
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("GameSceneManager: AudioManager instance not found.");
                return;
            }

            AudioManager.Instance.PlayNote(correctNoteIndex);
        }

        private void HandleCorrectAnswer()
        {
            currentPhase = GamePhase.ShowingFeedback;
            score++;
            UpdateScore();
            UpdateFeedback("正解！");
            RestartRoutine(AdvanceRoutine(correctDelay, true));
        }

        private void HandleIncorrectAnswer()
        {
            currentPhase = GamePhase.ShowingFeedback;
            UpdateFeedback($"残念！正解は『{NoteNames[correctNoteIndex]}』でした");

            if (repeatUntilCorrect)
            {
                RestartRoutine(RepeatSameQuestionRoutine());
                return;
            }

            RestartRoutine(AdvanceRoutine(incorrectDelay, true));
        }

        private IEnumerator RepeatSameQuestionRoutine()
        {
            yield return new WaitForSeconds(incorrectDelay);

            if (autoReplayAfterMiss)
            {
                PlayCurrentNote();
            }

            currentPhase = GamePhase.AwaitingAnswer;
            SetButtonsInteractable(true);
        }

        private IEnumerator AdvanceRoutine(float waitTime, bool pickNewNote)
        {
            yield return new WaitForSeconds(waitTime);
            PrepareNextQuestion(pickNewNote);
        }

        private void RestartRoutine(IEnumerator routine)
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(routine);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            foreach (var button in noteButtons)
            {
                button?.SetInteractable(interactable);
            }
        }

        private void UpdateFeedback(string message)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackText.text = message;
        }

        private void UpdateScore()
        {
            if (scoreText == null)
            {
                return;
            }

            scoreText.text = $"SCORE: {score}";
        }
    }
}
