using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class LoginManager : MonoBehaviour
    {
        public static LoginManager Instance;
        public GameObject LoadingPage;
        public GameObject MessageDialog;
        public Text message;

        public GameObject buttonsForAndroid;

        private void Start() {
            Instance = this;

#if UNITY_ANDROID
            buttonsForAndroid.SetActive(true);
#elif UNITY_IOS
    appleLoginBtn.SetActive(false);
#endif
        }

        public void LoginWithApple() {
            if (AllowAppleLogin()) {
                LoadingPage.SetActive(true);
                StartCoroutine(checkInternet((isConnected) => {
                    if (isConnected) {
                        //AppleLogin.Instance.LoginWithApple();
                    }
                    else {
                        ShowMessage("No internet connection!");
                    }
                }));
            }
            else {
                ShowMessage("Please update your OS or try another login");
            }
        }

        public void LoginWithGuest() {
            LoadingPage.SetActive(true);
            SocialLogin.Instance.PlayAsGuest();
        }

        public void LoginWithFacebook() {
            LoadingPage.SetActive(true);
            StartCoroutine(checkInternet((isConnected) => {
                if (isConnected) {
                    SocialLogin.Instance.LoginWithFacebook();
                }
                else {
                    ShowMessage("No internet connection!");
                }
            }));
        }

        public void LoginWithGoogle() {
            if (Application.platform == RuntimePlatform.WindowsEditor)
                return;
            LoadingPage.SetActive(true);
            StartCoroutine(checkInternet((isConnected) => {
                if (isConnected) {
                    SocialLogin.Instance.OnSignIn();
                }
                else {
                    ShowMessage("No internet connection!");
                }
            }));
        }

        public void ShowMessage(string msg) {
            LoadingPage.SetActive(false);
            PopupManager.Instance.Show("info");
            message.text = msg;
        }

        public IEnumerator checkInternet(Action<bool> action) {
            WWW www = new WWW("http://google.com");
            yield return www;
            if (www.error != null) {
                action(false);
            }
            else {
                action(true);
            }
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Application.Quit();
            }
        }

        // allow vibration to specific range of devices.
        private bool AllowAppleLogin() {
            try {
                string OperationSystemStr = SystemInfo.operatingSystem;
                var OS_Version = OperationSystemStr.Split(' ');
                Debug.Log("=== OS_Version : " + OS_Version);
                if (float.Parse(OS_Version[1].Substring(0, 2)) >= 13f) {
                    return true;
                }
                //Debug.Log ("=== Not allowed vibration feature");
                return false;
            }
            catch (Exception e) {
                Debug.Log("===> ERROR of apple version code : " + e.Message);
                return false;
            }
        }
    }
}

public enum LoginType
{
    None,
    Facebook,
    Google,
    Apple,
    Guest
}