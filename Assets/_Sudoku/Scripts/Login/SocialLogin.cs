using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArtboxGames
{
    public class SocialLogin : MonoBehaviour
    {
        public static SocialLogin Instance;
        public static string Name;
        public static string imagePath;

        void Awake()
        {
            if (Instance)
                Destroy(gameObject);
            else
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
        }

        private void Start()
        {
            PlayAsGuest();
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else
            {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }

        IEnumerator LoadYourAsyncScene(string sceneName)
        {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        public void PlayAsGuest()
        {
            if (!PlayerPrefs.HasKey("playerName"))
            {
                string playerName = "Guest" + UnityEngine.Random.Range(10000, 99999);
                PlayerPrefs.SetString("playerName", playerName);
                PlayerPrefs.Save();
                Name = playerName;
            }
            else
            {
                Name = PlayerPrefs.GetString("playerName");
            }
            imagePath = "";
            StartCoroutine(LoadYourAsyncScene("Main"));
            PlayerIOAuthentication();
        }

        private void PlayerIOAuthentication()
        {
            StartCoroutine(checkInternet((isConnected) =>
            {
                if (isConnected)
                    ServerCode.Instance.Authentication(SystemInfo.deviceUniqueIdentifier, "");
            }));
        }

        public IEnumerator checkInternet(Action<bool> action)
        {
            WWW www = new WWW("http://google.com");
            yield return www;
            if (www.error != null)
            {
                action(false);
            }
            else
            {
                action(true);
            }
        }
    }
}