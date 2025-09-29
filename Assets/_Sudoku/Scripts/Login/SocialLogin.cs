using Facebook.Unity;
using Google;
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
        private LoginType _loginType;

        public LoginType Login_Type {
            get {
                return _loginType;
            }
            set {
                _loginType = value;
                if (_loginType == LoginType.None)
                    PlayerPrefs.SetInt("login", 0);
                else if (_loginType == LoginType.Facebook)
                    PlayerPrefs.SetInt("login", 1);
                else if (_loginType == LoginType.Google)
                    PlayerPrefs.SetInt("login", 2);
                else if (_loginType == LoginType.Apple)
                    PlayerPrefs.SetInt("login", 3);
                else if (_loginType == LoginType.Guest)
                    PlayerPrefs.SetInt("login", 4);

                PlayerPrefs.Save();
            }
        }

        private string webClientId = "680894183327-m6e59aiu5au5qg3a76b3ad5rqtsfsmgb.apps.googleusercontent.com";

        private GoogleSignInConfiguration configuration;

        private loginCallBack l_callback;
        private string result;

        void Awake() {
            if (Instance)
                Destroy(gameObject);
            else {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }

            if (!PlayerPrefs.HasKey("login")) {
                PlayerPrefs.SetInt("login", 0);
                PlayerPrefs.Save();
            }
            else {
                int login = PlayerPrefs.GetInt("login");
                if (login == 0)
                    Login_Type = LoginType.None;
                else if (login == 1)
                    Login_Type = LoginType.Facebook;
                else if (login == 2)
                    Login_Type = LoginType.Google;
                else if (login == 3)
                    Login_Type = LoginType.Apple;
                else if (login == 4)
                    Login_Type = LoginType.Guest;
            }

            if (!FB.IsInitialized) {
                // Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }

            l_callback = loginCallBack.None;

#if UNITY_IOS
        webClientId = "883191117179-sairemhcr15eh9a10b8oascojtfjgtk0.apps.googleusercontent.com";
#endif
            configuration = new GoogleSignInConfiguration {
                WebClientId = webClientId,
                RequestEmail = true,
                RequestIdToken = true
            };
        }

        private void Start() {
            if (_loginType == LoginType.Google) {
                OnSignInSilently();
            }
            else if (_loginType == LoginType.Facebook) {
#if UNITY_EDITOR
                Name = "editor test";
                Login_Type = LoginType.Facebook;
                ServerCode.Instance.Authentication(SystemInfo.deviceUniqueIdentifier, "", LoginType.Guest);
                StartCoroutine(LoadYourAsyncScene("Main"));
#endif
            }
            else if (_loginType == LoginType.Apple) {
                //AppleLogin.Instance.SetupLoginMenu();
            }
            else if (_loginType == LoginType.Guest) {
                Name = PlayerPrefs.GetString("playerName");
                StartCoroutine(LoadYourAsyncScene("Main"));
                PlayerIOAuthentication();
            }
            else if (_loginType == LoginType.None) {
                StartCoroutine(LoadYourAsyncScene("Login"));
            }
        }

        private void InitCallback() {
            if (FB.IsInitialized) {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK     
                if (FB.IsLoggedIn && _loginType == LoginType.Facebook) {
                    if (LoginManager.Instance != null)
                        LoginManager.Instance.LoadingPage.SetActive(true);
                    GetPlayerData();
                }
            }
            else {
                if (LoginManager.Instance != null)
                    LoginManager.Instance.ShowMessage("Failed to Initialize the Facebook SDK");
            }
        }

        private void OnHideUnity(bool isGameShown) {
            if (!isGameShown) {
                // Pause the game - we will need to hide
                Time.timeScale = 0;
            }
            else {
                // Resume the game - we're getting focus again
                Time.timeScale = 1;
            }
        }

        public void LoginWithFacebook() {
            var perms = new List<string>() { "public_profile", "email" };
            FB.LogInWithReadPermissions(perms, AuthCallback);
        }

        private void AuthCallback(ILoginResult result) {
            if (FB.IsLoggedIn) {
                // AccessToken class will have session details
                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                // Print current access token's User ID
                Debug.Log(aToken.UserId);
                // Print current access token's granted permissions
                foreach (string perm in aToken.Permissions) {
                    Debug.Log(perm);
                }

                GetPlayerData();
            }
            else {
                if (LoginManager.Instance != null)
                    LoginManager.Instance.ShowMessage("User cancelled login");
            }
        }

        private void GetPlayerData() {
            FB.API("/me?fields=first_name,last_name,email", HttpMethod.GET, LoginCallback);
        }

        void LoginCallback(IGraphResult result) {
            if (result.Error != null) {
                Debug.Log("Error Response:\n" + result.Error);
                SceneManager.LoadScene("Login");
                Login_Type = LoginType.None;
            }
            else if (!FB.IsLoggedIn) {
                if (LoginManager.Instance != null)
                    LoginManager.Instance.ShowMessage("User cancelled login");
                SceneManager.LoadScene("Login");
            }
            else {
                IDictionary dict = Facebook.MiniJSON.Json.Deserialize(result.RawResult) as IDictionary;
                string userID = dict["id"].ToString();
                imagePath = "https" + "://graph.facebook.com/" + userID + "/picture?type=large";
                Name = dict["first_name"].ToString() + " " + dict["last_name"].ToString();
                string email = ""; //string.IsNullOrEmpty(dict["email"].ToString()) ? "" : dict["email"].ToString();
                Login_Type = LoginType.Facebook;
                ServerCode.Instance.Authentication(AccessToken.CurrentAccessToken.UserId, email, LoginType.Facebook, AccessToken.CurrentAccessToken.TokenString);
                StartCoroutine(LoadYourAsyncScene("Main"));
            }
        }

        IEnumerator LoadYourAsyncScene(string sceneName) {
            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone) {
                yield return null;
            }
        }
        public void OnSignIn() {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            AddStatusText("Calling SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
              OnAuthenticationFinished);
        }

        public void OnSignOut() {
            AddStatusText("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
            SceneManager.LoadScene("Login");
        }

        public void OnDisconnect() {
            AddStatusText("Calling Disconnect");
            GoogleSignIn.DefaultInstance.Disconnect();
        }

        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task) {
            if (task.IsFaulted) {
                using (IEnumerator<System.Exception> enumerator =
                        task.Exception.InnerExceptions.GetEnumerator()) {
                    if (enumerator.MoveNext()) {
                        GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                        AddStatusText("Got Error: " + error.Status + " " + error.Message);
                        l_callback = loginCallBack.Cancel;
                    }
                    else {
                        AddStatusText("Got Unexpected Exception?!?" + task.Exception);
                        l_callback = loginCallBack.Failed;
                    }
                }
            }
            else if (task.IsCanceled) {
                AddStatusText("Canceled");
                l_callback = loginCallBack.Cancel;
            }
            else {
                AddStatusText("Welcome: " + task.Result.DisplayName + "!");
                l_callback = loginCallBack.Success;
                result += task.Result.DisplayName + "," + task.Result.Email + "," + task.Result.UserId;
                imagePath = task.Result.ImageUrl.ToString();
                ServerCode.Instance.Authentication(task.Result.UserId, task.Result.Email, LoginType.Google);
            }
        }

        public void OnSignInSilently() {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            AddStatusText("Calling SignIn Silently");

            GoogleSignIn.DefaultInstance.SignInSilently()
                  .ContinueWith(OnAuthenticationFinished);
        }

        public void OnGamesSignIn() {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = true;
            GoogleSignIn.Configuration.RequestIdToken = false;

            AddStatusText("Calling Games SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
              OnAuthenticationFinished);
        }

        private List<string> messages = new List<string>();
        void AddStatusText(string text) {
            if (messages.Count == 5) {
                messages.RemoveAt(0);
            }
            messages.Add(text);
            string txt = "";
            foreach (string s in messages) {
                txt += "\n" + s;
            }
            //Debug.Log("=== Message : " + txt);
        }

        private enum loginCallBack
        {
            None,
            Cancel,
            Success,
            Failed
        }

        private void Update() {
            if (l_callback == loginCallBack.Cancel) {
                if (LoginManager.Instance != null) {
                    LoginManager.Instance.ShowMessage("User cancelled login");
                    LoginManager.Instance.LoadingPage.SetActive(false);
                }
                Login_Type = LoginType.None;
            }
            else if (l_callback == loginCallBack.Success) {
                var data = result.Split(',');
                Login_Type = LoginType.Google;
                Name = data[0];
                StartCoroutine(LoadYourAsyncScene("Main"));
            }
            else if (l_callback == loginCallBack.Failed) {
                if (LoginManager.Instance != null) {
                    LoginManager.Instance.ShowMessage("Login failed!");
                    LoginManager.Instance.LoadingPage.SetActive(false);
                }
                Login_Type = LoginType.None;
            }
            l_callback = loginCallBack.None;
            result = "";
        }

        public void PlayAsGuest() {
            if (!PlayerPrefs.HasKey("playerName")) {
                string playerName = "Guest" + UnityEngine.Random.Range(10000, 99999);
                PlayerPrefs.SetString("playerName", playerName);
                PlayerPrefs.Save();
                Name = playerName;
            }
            else {
                Name = PlayerPrefs.GetString("playerName");
            }
            imagePath = "";
            Login_Type = LoginType.Guest;
            StartCoroutine(LoadYourAsyncScene("Main"));
            PlayerIOAuthentication();
        }

        private void PlayerIOAuthentication() {
            StartCoroutine(checkInternet((isConnected) => {
                if (isConnected)
                    ServerCode.Instance.Authentication(SystemInfo.deviceUniqueIdentifier, "", LoginType.Guest);
            }));
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
    }
}