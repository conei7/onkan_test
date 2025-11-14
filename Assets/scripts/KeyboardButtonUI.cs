using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AbsolutePitchGame
{
    [RequireComponent(typeof(Button))]
    public class KeyboardButtonUI : MonoBehaviour
    {
        [SerializeField] private int noteIndex;
        [SerializeField] private Button button;
        [SerializeField] private List<KeyCode> keyBindings = new();

        private GameSceneManager gameManager;

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            button?.onClick.AddListener(NotifyGameManager);
        }

        private void OnDisable()
        {
            button?.onClick.RemoveListener(NotifyGameManager);
        }

        private void Update()
        {
            if (gameManager == null || button == null || !button.interactable)
            {
                return;
            }

            foreach (var key in keyBindings)
            {
                if (Input.GetKeyDown(key))
                {
                    NotifyGameManager();
                    break;
                }
            }
        }

        public void BindGameManager(GameSceneManager manager)
        {
            gameManager = manager;
        }

        public void SetInteractable(bool interactable)
        {
            if (button == null)
            {
                return;
            }

            button.interactable = interactable;
        }

        private void NotifyGameManager()
        {
            Debug.Log($"KeyboardButtonUI.NotifyGameManager noteIndex={noteIndex}, gameManager={(gameManager==null?"null":"ok")}");
            gameManager?.SubmitAnswer(noteIndex);
        }
    }
}
