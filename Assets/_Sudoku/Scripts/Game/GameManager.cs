using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class GameManager : SaveableManager<GameManager> {
        #region Inspector Variables

        [Header("Data")]
        [SerializeField] private List<PuzzleGroupData> puzzleGroups = null;

        [Header("Values")]
        [SerializeField] private int hintsPerCompletedPuzzle = 1;
        [SerializeField] private int numLevelsBetweenAds = 3;

        public int mistakesLimit = 3;
        public int mistakesCount;
        public Text mistakesText;
        public Text mistakesText1;
        public int requiredPlayer = 2;
        public bool isOnlinePlay { get; set; }
        public bool isPrivatePlay { get; set; }
        public List<PlayerData> players;
        public string levelType = "beginner";
        public string roomCode = "";

        [Space]
        [SerializeField] private GameObject topPanel_Practice = null;
        [SerializeField] private GameObject topPanel_Online = null;

        [Header("Game Settings")]
        [SerializeField] private bool noteDetection = true;
        [SerializeField] private bool keepScreenOn = true;
        [SerializeField] private bool numberFirstInput = true;
        [SerializeField] private bool mistakeLimit = true;
        [SerializeField] private bool sound = true;
        [SerializeField] private bool vibrate = true;
        [SerializeField] private bool highlightConnectedCell = true;
        [SerializeField] private bool highlightSameNumbers = true;

        [Header("Components")]
        [SerializeField] private PuzzleBoard puzzleBoard = null;

        [Header("Player Profile")]
        public Text txtPlayerName;
        public Text message;
        public Image playerImage;

        [Header("Pause")]
        [SerializeField] private GameObject PauseImg;
        [SerializeField] private GameObject PlayImg;
        [SerializeField] private GameObject PauseImg1;
        [SerializeField] private GameObject PlayImg1;
        [Space(5)]
        public InputField inputRoomCode;

        private string characters = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";

        #endregion

        #region Properties

        public override string SaveId { get { return "game_manager"; } }
        public bool IsPaused
        {
            get { return _isPause; }
            set
            {
                _isPause = value;
                if (value)
                {
                    PlayImg.SetActive(true);
                    PauseImg.SetActive(false);
                    PlayImg1.SetActive(true);
                    PauseImg1.SetActive(false);
                }
                else
                {
                    PlayImg.SetActive(false);
                    PauseImg.SetActive(true);
                    PlayImg1.SetActive(false);
                    PauseImg1.SetActive(true);
                }
            }
        }

        public PuzzleData ActivePuzzleData { get; private set; }
        public int NumLevelsTillAdd { get; private set; }

        public List<PuzzleGroupData> PuzzleGroupDatas { get { return puzzleGroups; } }
        public bool NoteDetection { get { return noteDetection; } }
        public bool KeepScreenOn { get { return keepScreenOn; } }
        public bool NumberFirstInput { get { return numberFirstInput; } }
        public bool MistakeLimit { get { return mistakeLimit; } }
        public bool Sound { get { return sound; } }
        public bool Vibrate { get { return vibrate; } }
        public bool HighlightConnectedCell { get { return highlightConnectedCell; } }
        public bool HighlightSameNumbers { get { return highlightSameNumbers; } }

        public int HintsPerCompletedPuzzle { get { return hintsPerCompletedPuzzle; } }

        [Space]
        public int firstInputNum = 0;

        public System.Action<string> OnGameSettingChanged { get; set; }

        private bool _isPause;

        public string ActivePuzzleDifficultyStr
        {
            get
            {
                if (ActivePuzzleData == null) return "";

                PuzzleGroupData puzzleGroupData = GetPuzzleGroup(ActivePuzzleData.groupId);

                if (puzzleGroupData == null) return "";

                return puzzleGroupData.displayName;
            }
        }

        public string ActivePuzzleTimeStr
        {
            get
            {
                if (ActivePuzzleData == null) return "00:00";

                return Utilities.FormatTimer(ActivePuzzleData.elapsedTime);
            }
        }

        #endregion

        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();

            puzzleBoard.Initialize();

            for (int i = 0; i < puzzleGroups.Count; i++)
            {
                puzzleGroups[i].Load();
            }

            InitSave();
            SocialLogin.Name = string.IsNullOrEmpty(SocialLogin.Name) ? "Guest" : SocialLogin.Name;
            txtPlayerName.text = SocialLogin.Name;
        }

        private void Start() {
            StartCoroutine(LoadImage(SocialLogin.imagePath, playerImage, null));
            levelType = "beginner";
        }

        private void Update()
        {
            if (!IsPaused && ActivePuzzleData != null && ScreenManager.Instance.CurrentScreenId == "game")
            {
                ActivePuzzleData.elapsedTime += Time.deltaTime;
            }
        }

        #endregion

        #region Public Methods

        public void PlayNewGame(int groupIndex)
        {
            // Make sure the groupIndex is within the bounds of puzzleGroups
            if (groupIndex >= 0 && groupIndex < puzzleGroups.Count)
            {
                PlayNewGame(puzzleGroups[groupIndex]);

                return;
            }

            Debug.LogErrorFormat("[GameManager] PlayNewGame(int groupIndex) : The given groupIndex ({0}) is out of bounds for the puzzleGroups of size {1} \"{0}\"", groupIndex, puzzleGroups.Count);
        }

        public void PlayNewGame(string groupId)
        {
            if (isOnlinePlay)
            {
                levelType = groupId;
                ServerCode.Instance.CreateRoom();                              
                return;
            }
            // Get the PuzzleGroupData for the given groupId
            for (int i = 0; i < puzzleGroups.Count; i++)
            {
                PuzzleGroupData puzzleGroupData = puzzleGroups[i];

                if (groupId == puzzleGroupData.groupId)
                {
                    PlayNewGame(puzzleGroupData);

                    return;
                }
            }

            Debug.LogErrorFormat("[GameManager] PlayNewGame(string groupId) : Could not find a PuzzleGroupData with the given id \"{0}\"", groupId);
        }   

        public void PlayNewGame_Online(string groupId)
        {
            // Get the PuzzleGroupData for the given groupId
            for (int i = 0; i < puzzleGroups.Count; i++)
            {
                PuzzleGroupData puzzleGroupData = puzzleGroups[i];

                if (groupId == puzzleGroupData.groupId)
                {
                    PlayNewGame(puzzleGroupData);

                    return;
                }
            }
            Debug.LogErrorFormat("[GameManager] PlayNewGame(string groupId) : Could not find a PuzzleGroupData with the given id \"{0}\"", groupId);
        }

        public void ContinueActiveGame()
        {
            PlayGame(ActivePuzzleData);
        }

        public void SetGameSetting(string setting, bool value)
        {
            switch (setting)
            {
                case "1":
                    noteDetection = value;
                    break;
                case "2":
                    keepScreenOn = value;
                    break;
                case "3":
                    numberFirstInput = value;
                    break;
                case "4":
                    mistakeLimit = value;
                    break;
                case "5":
                    sound = value;
                    break;
                case "6":
                    vibrate = value;
                    break;
                case "7":
                    highlightConnectedCell = value;
                    break;
                case "8":
                    highlightSameNumbers = value;
                    break;
            }
            MakeGameSettings();
            if (OnGameSettingChanged != null && !MistakeLimit)
            {
                OnGameSettingChanged(setting);
            }
        }

        public void ActivePuzzleCompleted()
        {
            // Get the PuzzleGroupData for the puzzle
            PuzzleGroupData puzzleGroup = GetPuzzleGroup(ActivePuzzleData.groupId);
            float elapsedTime = ActivePuzzleData.elapsedTime;

            // Set the puzzle data to null now so the game can't be continued
            ActivePuzzleData = null;

            puzzleGroup.PuzzlesCompleted += 1;
            puzzleGroup.TotalTime += elapsedTime;

            bool newBest = false;

            if (puzzleGroup.MinTime == 0 || elapsedTime < puzzleGroup.MinTime)
            {
                newBest = true;
                puzzleGroup.MinTime = elapsedTime;
            }

            // Award the player their hint
            //CurrencyManager.Instance.Give("hints", hintsPerCompletedPuzzle);

            object[] popupData =
            {
                puzzleGroup.displayName,
                elapsedTime,
                puzzleGroup.MinTime,
                newBest
            };

            // Show the puzzle complete popup
            PopupManager.Instance.Show("puzzle_complete", popupData, (bool cancelled, object[] outData) =>
            {
                // If the popup was closed without the cancelled flag being set then the player selected New Game
                if (!cancelled && puzzleGroup != null)
                {
                    if (isOnlinePlay)
                        ScreenManager.Instance.Back();
                    else
                        PlayNewGame(puzzleGroup);
                }
                // Else go back to the main screen
                else
                {
                    ScreenManager.Instance.Back();
                }
            });
            AdsManager.Instance.ShowInterstitial();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a new PuzzleData from the given PuzzleGroupData and sets up the game to play it
        /// </summary>
        private void PlayNewGame(PuzzleGroupData puzzleGroupData)
        {
            // Get a puzzle that has not yet been played by the user
            PuzzleData puzzleData = puzzleGroupData.GetPuzzle();

            // Play the game using the new puzzle data
            PlayGame(puzzleData);
        }

        /// <summary>
        /// Starts the game using the given PuzzleData
        /// </summary>
        private void PlayGame(PuzzleData puzzleData)
        {
            mistakesCount = 0;
            firstInputNum = 0;

            // Set the active puzzle dat
            ActivePuzzleData = puzzleData;

            // Setup the puzzle board to display the numbers
            puzzleBoard.Setup(puzzleData);

            // Show the game screen
            ScreenManager.Instance.Show("game");

            NumLevelsTillAdd++;

            if (NumLevelsTillAdd > numLevelsBetweenAds)
            {
                //if (MobileAdsManager.Instance.ShowInterstitialAd(null))
                {
                    //NumLevelsTillAdd = 0;
                }
            }
            MakeGameSettings();

            if (isOnlinePlay)
            {
                topPanel_Online.SetActive(true);
                topPanel_Practice.SetActive(false);
            }
            else
            {
                topPanel_Online.SetActive(false);
                topPanel_Practice.SetActive(true);
            }
            Statistics.Instance.TotalPlayed++;
            if (isOnlinePlay)
                Statistics.Instance.TotalOnline++;
        }

        /// <summary>
        /// Gets the puzzle group with the given id
        /// </summary>
        private PuzzleGroupData GetPuzzleGroup(string id)
        {
            for (int i = 0; i < puzzleGroups.Count; i++)
            {
                PuzzleGroupData puzzleGroup = puzzleGroups[i];

                if (id == puzzleGroup.groupId)
                {
                    return puzzleGroup;
                }
            }

            return null;
        }

        #endregion

        #region Save Methods

        public override Dictionary<string, object> Save()
        {
            Dictionary<string, object> saveData = new Dictionary<string, object>();

            // Save the active puzzle if there is one
            if (ActivePuzzleData != null)
            {
                saveData["activePuzzle"] = ActivePuzzleData.Save();
            }

            // Save all the puzzle groups data
            List<object> savedPuzzleGroups = new List<object>();

            for (int i = 0; i < puzzleGroups.Count; i++)
            {
                PuzzleGroupData puzzleGroup = puzzleGroups[i];
                Dictionary<string, object> savedPuzzleGroup = new Dictionary<string, object>();

                savedPuzzleGroup["id"] = puzzleGroup.groupId;
                savedPuzzleGroup["data"] = puzzleGroup.Save();

                savedPuzzleGroups.Add(savedPuzzleGroup);
            }

            saveData["savedPuzzleGroups"] = savedPuzzleGroups;

            // Save the game settings			
            saveData["noteDetection"] = noteDetection;
            saveData["keepScreenOn"] = keepScreenOn;
            saveData["numberFirstInput"] = numberFirstInput;
            saveData["mistakeLimit"] = mistakeLimit;
            saveData["sound"] = sound;
            saveData["vibrate"] = vibrate;
            saveData["highlightConnectedCell"] = highlightConnectedCell;
            saveData["highlightSameNumbers"] = highlightSameNumbers;

            saveData["NumLevelsTillAdd"] = NumLevelsTillAdd;

            return saveData;
        }

        protected override void LoadSaveData(bool exists, JSONNode saveData)
        {
            if (!exists)
            {
                return;
            }

            // If there is a saved active puzzle load it
            if (saveData.AsObject.HasKey("activePuzzle"))
            {
                ActivePuzzleData = new PuzzleData(saveData["activePuzzle"]);
            }

            // Load the saved group data
            JSONArray savedPuzzleGroups = saveData["savedPuzzleGroups"].AsArray;

            for (int i = 0; i < savedPuzzleGroups.Count; i++)
            {
                JSONNode savedPuzzleGroup = savedPuzzleGroups[i];
                PuzzleGroupData puzzleGroup = GetPuzzleGroup(savedPuzzleGroup["id"].Value);

                if (puzzleGroup != null)
                {
                    puzzleGroup.Load(savedPuzzleGroup["data"]);
                }
            }

            // Load the game settings
            noteDetection = saveData["noteDetection"].AsBool;
            keepScreenOn = saveData["keepScreenOn"].AsBool;
            numberFirstInput = saveData["numberFirstInput"].AsBool;
            mistakeLimit = saveData["mistakeLimit"].AsBool;
            sound = saveData["sound"].AsBool;
            vibrate = saveData["vibrate"].AsBool;
            highlightConnectedCell = saveData["highlightConnectedCell"].AsBool;
            highlightSameNumbers = saveData["highlightSameNumbers"].AsBool;

            NumLevelsTillAdd = saveData["NumLevelsTillAdd"].AsInt;

            if (GameManager.Instance.KeepScreenOn)
            { UnityEngine.Screen.sleepTimeout = -1; }
            else
            { UnityEngine.Screen.sleepTimeout = 0; }
        }

        #endregion

        public void StartNewGame()
        {
            if (isOnlinePlay)
                ScreenManager.Instance.Back();
            else
            {
                PlayNewGame(ActivePuzzleData.groupId);
                puzzleBoard.Undo();
            }
        }

        public void Reset()
        {
            if (!isOnlinePlay)
            {
                PlayNewGame(ActivePuzzleData.groupId);
                puzzleBoard.Undo();
            }
        }

        private void MakeGameSettings()
        {
            if (noteDetection)
            { }
            if (sound) { SoundManager.Instance.setBackgroundMusic(true); }
            else { SoundManager.Instance.setBackgroundMusic(false); }

            if (keepScreenOn)
            { UnityEngine.Screen.sleepTimeout = -1; }
            else
            { UnityEngine.Screen.sleepTimeout = 0; }

            if (mistakeLimit)
            {
                mistakesText1.text = mistakesCount.ToString() + "/" + mistakesLimit.ToString();
                mistakesText.text = mistakesCount.ToString() + "/" + mistakesLimit.ToString();
            }
            else {
                mistakesText1.text = mistakesCount.ToString();
                mistakesText.text = mistakesCount.ToString();
            }

            if (highlightConnectedCell)
            {
                puzzleBoard.SetCellsSelected(puzzleBoard.selectedCellRow, puzzleBoard.selectedCellCol, true);
            }
            else
            {
                puzzleBoard.SetAllCellsUnselected(true);
            }
            int row = puzzleBoard.selectedCellRow;
            int col = puzzleBoard.selectedCellCol;

            if (puzzleBoard.activePuzzleData == null || puzzleBoard.activeCells == null)
                return;

            PuzzleBoardCell cell = puzzleBoard.activeCells[puzzleBoard.selectedCellRow][puzzleBoard.selectedCellCol];
            if (HighlightSameNumbers)
            {
                puzzleBoard.HighlightCells(cell.Number);
            }
            else
            {
                puzzleBoard.HighlightCells(cell.Number, false);
                PuzzleData.CellType cellType = puzzleBoard.activePuzzleData.GetCellType(row, col);
                if (cellType != PuzzleData.CellType.Blank)
                {
                    cell.SetHighlighted();
                }
            }
        }

        public void Logout()
        {
            if (SocialLogin.Instance.Login_Type == LoginType.Facebook)
            {
                FB.LogOut();
            }
            else if (SocialLogin.Instance.Login_Type == LoginType.Google)
            {
                SocialLogin.Instance.OnSignOut();
            }
            else if (SocialLogin.Instance.Login_Type == LoginType.Apple)
            {
                //AppleLogin.Instance.OnLogout();
            }
            SocialLogin.Instance.Login_Type = LoginType.None;
            SocialLogin.Name = "Guest";
            SceneManager.LoadScene("Login");
            SocialLogin.imagePath = "";
        }

        // used to select player(2 or four player)
        public void OnToggleChanged(int player)
        {
            if (player == 2)
                requiredPlayer = 2;
            else if (player == 3)
                requiredPlayer = 3;
            else if (player == 4)
                requiredPlayer = 4;
        }

        public void OnToggleChanged_Difficulty(string difficulty)
        {
            levelType = difficulty;
        }

        public void Back_PlayerWaiting()
        {
            if (ServerCode.piocon != null && ServerCode.piocon.Connected)
            {
                ServerCode.Instance.SendDisconnect();
                ServerCode.piocon.Disconnect();
            }
        }

        public void CreateRoom()
        {
            PopupManager.Instance.Show("loading");
            StartCoroutine(checkInternet((isConnected) =>
            {
                if (isConnected) {
                    ServerCode.Instance.isAdmin = true;
                    roomCode = GenerateRoomId();
                    ServerCode.Instance.CreateNewRoom(roomCode, requiredPlayer.ToString(), levelType);
                }
                else
                    ShowMessage("No internet connection!");
            }));            
        }

        public void JoinRoom(Text roomId) {            
            if (string.IsNullOrEmpty(roomId.text.Trim()))
                ShowMessage("Please enter room id");
            else {
                PopupManager.Instance.Show("loading");
                StartCoroutine(checkInternet((isConnected) => {
                    if (isConnected) {
                        roomCode = roomId.text;
                        ServerCode.Instance.JoinRoom(roomId.text.Trim());
                        roomId.text = "";
                    }
                    else
                        ShowMessage("No internet connection!");
                }));
            }
        }

        public void ShowMessage(string msg)
        {
            PopupManager.Instance.HidePopup("loading");
            PopupManager.Instance.Show("info");
            message.text = msg;
        }

        public IEnumerator checkInternet(System.Action<bool> action) {
            WWW www = new WWW("http://google.com");
            yield return www;
            if (www.error != null) {
                action(false);
            }
            else {
                action(true);
            }
        }

        private string GenerateRoomId() {
            string roomId = "";
            for (int i = 0; i < 6; i++) {
                int a = Random.Range(0, characters.Length);
                roomId = roomId + characters[a];
            }
            Debug.Log("=== room id : " + roomId);
            roomCode = roomId;
            return roomId;
        }

        public IEnumerator LoadImage(string path, Image _userImage, Image loading) {
            //loading.enabled = true;
            int login = PlayerPrefs.GetInt("login");
            int h_w = 200;
            if (login == 2)
                h_w = 90;
              
            WWW url = new WWW(path);
            Texture2D textFb2 = new Texture2D(h_w, h_w, TextureFormat.RGBA32, false); //TextureFormat must be DXT5

            yield return url;
            Debug.Log("GetUserImage : " + url);

            float width = (float)textFb2.width;
            float height = (float)textFb2.height;
            Rect rect = new Rect(0, 0, width, height);
            //loading.enabled = false;

            if (url.error == null) {
                _userImage.sprite = Sprite.Create(textFb2, rect, new Vector2(0, 0), 1);
                url.LoadImageIntoTexture(textFb2);
            }
        }

        public void WatchVideo() {
            PopupManager.Instance.Show("loading");
            StartCoroutine(checkInternet((isConnected) => {
                if (isConnected) {
                    if (AdsManager.Instance.ShowRewardVideo()) {
                        PopupManager.Instance.HidePopup("watchvideo");                        
                    }
                    else {
                        ShowMessage("No video to serve, Please try after few minutes");
                    }
                }
                else {
                    ShowMessage("No internet connection!");
                }
            }));           
        }
    }
}