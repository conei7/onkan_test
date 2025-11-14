using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace AbsolutePitchGame
{
    public class GameSceneManager : MonoBehaviour
    {
        private enum GamePhase
        {
            AwaitingPlayback,
            AwaitingAnswer
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
    [SerializeField] private float nextQuestionDelay = 1.0f;
    [SerializeField] private float sessionDuration = 30.0f;

    private int correctNoteIndex;
    private int totalQuestions;
    private int correctAnswers;
    private GamePhase currentPhase = GamePhase.AwaitingPlayback;
    private Coroutine autoAdvanceRoutine;
    private Coroutine sessionCountdownRoutine;
    private float remainingTime;
    private bool sessionActive;

        private void Start()
        {
            // If inspector list was not populated, try to auto-discover KeyboardButtonUI in the scene
            if (noteButtons == null || noteButtons.Count == 0)
            {
                var found = FindObjectsOfType<KeyboardButtonUI>();
                if (found != null && found.Length > 0)
                {
                    // Sort by GameObject name to get deterministic order if developer named them consistently
                    var ordered = found.OrderBy(f => f.gameObject.name).ToList();
                    noteButtons = ordered;
                    Debug.Log($"GameSceneManager: auto-discovered {ordered.Count} KeyboardButtonUI objects and assigned to noteButtons.");
                }
                else
                {
                    Debug.LogWarning("GameSceneManager: no KeyboardButtonUI found in scene. Please assign noteButtons in inspector.");
                }
            }

            RegisterButtons();
            StartSession();
        }

        private void OnDisable()
        {
            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
                autoAdvanceRoutine = null;
            }

            if (sessionCountdownRoutine != null)
            {
                StopCoroutine(sessionCountdownRoutine);
                sessionCountdownRoutine = null;
            }
        }

        public void HandlePlayNoteRequest()
        {
            if (!sessionActive)
            {
                return;
            }

#if UNITY_EDITOR
            Debug.Log($"HandlePlayNoteRequest called - phase={currentPhase} correctNoteIndex={correctNoteIndex}");
#endif
            PlayCurrentNote();

            if (currentPhase == GamePhase.AwaitingPlayback)
            {
                currentPhase = GamePhase.AwaitingAnswer;
                SetButtonsInteractable(true);
            }
        }

        public void SubmitAnswer(int noteIndex)
        {
#if UNITY_EDITOR
            Debug.Log($"SubmitAnswer called - noteIndex={noteIndex} phase={currentPhase} correctNoteIndex={correctNoteIndex}");
#endif
            if (!sessionActive)
            {
                return;
            }

            if (currentPhase != GamePhase.AwaitingAnswer)
            {
                return;
            }

            SetButtonsInteractable(false);

            totalQuestions++;
            var isCorrect = noteIndex == correctNoteIndex;
            if (isCorrect)
            {
                correctAnswers++;
            }
            UpdateScore();

            if (isCorrect)
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

        private void PrepareNextQuestion(bool pickNewNote, bool updatePrompt = true)
        {
            if (pickNewNote)
            {
                correctNoteIndex = Random.Range(0, NoteNames.Length);
            }

            currentPhase = GamePhase.AwaitingPlayback;
            SetButtonsInteractable(false);
            if (updatePrompt)
            {
                UpdateFeedback("Listen and choose a note");
            }
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopNotePlayback();
            }

            UpdateFeedback("Correct! Next note is coming...");
            ScheduleNextQuestion(true);
        }

        private void HandleIncorrectAnswer()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopNotePlayback();
            }

            UpdateFeedback($"Almost! The answer was {NoteNames[correctNoteIndex]}. Next note is coming...");
            ScheduleNextQuestion(true);
        }

        private void ScheduleNextQuestion(bool pickNewNote)
        {
            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
            }

            autoAdvanceRoutine = StartCoroutine(AutoAdvanceRoutine(pickNewNote));
        }

        private IEnumerator AutoAdvanceRoutine(bool pickNewNote)
        {
            yield return new WaitForSeconds(nextQuestionDelay);
            autoAdvanceRoutine = null;
            PrepareNextQuestion(pickNewNote);
            HandlePlayNoteRequest();
        }

        private void StartSession()
        {
            remainingTime = sessionDuration;
            sessionActive = true;
            totalQuestions = 0;
            correctAnswers = 0;
            UpdateScore();

            if (sessionCountdownRoutine != null)
            {
                StopCoroutine(sessionCountdownRoutine);
            }

            sessionCountdownRoutine = StartCoroutine(SessionCountdownRoutine());
            PrepareNextQuestion(true);
        }

        private IEnumerator SessionCountdownRoutine()
        {
            while (remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                UpdateScore();
                yield return null;
            }

            remainingTime = 0f;
            UpdateScore();
            EndSession();
        }

        private void EndSession()
        {
            sessionActive = false;
            SetButtonsInteractable(false);

            if (autoAdvanceRoutine != null)
            {
                StopCoroutine(autoAdvanceRoutine);
                autoAdvanceRoutine = null;
            }

            UpdateFeedback($"Time's up! Score: {correctAnswers}/{totalQuestions}");
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

            scoreText.text = $"SCORE: {correctAnswers}/{totalQuestions}  TIME: {Mathf.CeilToInt(Mathf.Max(remainingTime, 0f))}s";
        }
    }
}
