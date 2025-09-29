using Newtonsoft.Json;
using OneSignalSDK;
using UnityEngine;
using Unity.RemoteConfig;
using System.Collections.Generic;

namespace ArtboxGames
{
    public class RemoteConfig : MonoBehaviour
    {
        public static RemoteConfig instance;

        [SerializeField] private GameObject appUpdate;
        [SerializeField] private GameObject appMaintenance;
        [SerializeField] private UnityEngine.UI.Text message;

        public bool adsEnable = true;
        public int adsOnClick = 6;
        public struct userAttributes { }
        public struct appAttributes { }

        private string environmentName = "production";
        private string environmentId = "9e1be158-a822-4714-972c-34211abb041b";

#if UNITY_DEVELOPMENT_BUILD
        private string environmentName = "development";
        private string environmentId = "f41cdbb9-dede-44f3-80c5-fa65adc4ca0f";
#endif
        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(this);
        }

        // Start is called before the first frame update
        void Start()
        {
            OneSignal.Default.Initialize("36b42b2f-8e24-45d1-adfb-f52756a2bf0f");

            ConfigManager.FetchCompleted += SetRemoteData;
            ConfigManager.SetEnvironmentID(environmentId);
            ConfigManager.FetchConfigs(new userAttributes(), new appAttributes());
        }

        private void SetRemoteData(ConfigResponse response)
        {
            switch (response.requestOrigin)
            {
                case ConfigOrigin.Default:
                    Debug.Log("Default values will be returned");
                    break;
                case ConfigOrigin.Cached:
                    Debug.Log("Cached values loaded");
                    break;
                case ConfigOrigin.Remote:
                    Debug.Log("Remote Values changed");
                    //Debug.Log("===== RemoteConfigService.Instance.appConfig fetched: " + ConfigManager.appConfig.config.ToString());

                    adsEnable = ConfigManager.appConfig.GetBool("adsEnable");
                    adsOnClick = ConfigManager.appConfig.GetInt("adsOnClick");
                    float appVersion = ConfigManager.appConfig.GetFloat("appVersion");
                    string jsonString = ConfigManager.appConfig.GetJson("appMaintenance");

                    Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);

                    if (data["status"].ToString().Equals("true"))
                    {
                        message.text = data["message"].ToString();
                        appMaintenance.SetActive(true);
                        Invoke(nameof(PauseApp), 0.25f);
                    }
                    else if (appVersion > float.Parse(Application.version))
                    {
                        appUpdate.SetActive(true);
                        Invoke(nameof(PauseApp), 0.25f);
                    }
                    break;
            }
        }

        private void PauseApp()
        {
            Time.timeScale = 0f;
        }

        public void UpdateNow()
        {
            Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
            CloseApp();
        }

        public void NotNow()
        {
            appUpdate.SetActive(false);
            //SoundManager.Instance?.Play("btn-click");     
            Time.timeScale = 1f;
        }

        public void MoreGame()
        {
            Application.OpenURL("https://play.google.com/store/apps/developer?id=Artbox+Infotech");
            CloseApp();
        }

        public void CloseApp()
        {
            Application.Quit();
        }
    }
}