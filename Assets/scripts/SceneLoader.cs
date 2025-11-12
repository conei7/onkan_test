using UnityEngine;
using UnityEngine.SceneManagement;

namespace AbsolutePitchGame
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        public void LoadConfiguredScene()
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("SceneLoader: sceneName is not set.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        public void LoadSceneByName(string nameToLoad)
        {
            if (string.IsNullOrWhiteSpace(nameToLoad))
            {
                Debug.LogWarning("SceneLoader: nameToLoad is empty.");
                return;
            }

            SceneManager.LoadScene(nameToLoad);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
