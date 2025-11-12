using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AbsolutePitchGame
{
    public class ModeSelectSceneManager : MonoBehaviour
    {
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private string singleNoteSceneName = "GameScene";
        [SerializeField] private Button singleNoteButton;
        [SerializeField] private List<Button> futureModeButtons = new();

        private void Awake()
        {
            ApplyFutureModeLock();
        }

        public void StartSingleNoteMode()
        {
            if (sceneLoader == null)
            {
                Debug.LogWarning("ModeSelectSceneManager: sceneLoader is not assigned.");
                return;
            }

            sceneLoader.LoadSceneByName(singleNoteSceneName);
        }

        private void ApplyFutureModeLock()
        {
            foreach (var button in futureModeButtons)
            {
                if (button == null)
                {
                    continue;
                }

                button.interactable = false;
                var canvasGroup = button.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
                }

                canvasGroup.alpha = 0.5f;
            }

            if (singleNoteButton != null)
            {
                singleNoteButton.interactable = true;
            }
        }
    }
}
