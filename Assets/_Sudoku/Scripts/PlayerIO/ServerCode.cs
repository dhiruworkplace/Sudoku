using PlayerIOClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ArtboxGames
{
    public class ServerCode : MonoBehaviour
    {
        public static ServerCode Instance;

        public static Client player;

        public static Connection piocon;
        public Dictionary<string, string> JoinedPlayer = new Dictionary<string, string>();
        public bool isAdmin = false;
        public List<string> winners = new List<string>();

        public List<string> botNames;
        public List<int> botTime;

        private const string GameID = "sudoku-ovgqzxguewznrylgk1wdq";
        private const string password = "12345678";

        private const string roomType = "sudoku";

        private List<Message> msgList = new List<Message>(); //  Messsage queue implementation

        private void Awake()
        {
            Instance = this;
        }

        void handlemessage(object sender, Message m)
        {
            msgList.Add(m);
        }

        void FixedUpdate()
        {
            // process message queue
            foreach (Message m in msgList)
            {
                switch (m.Type)
                {
                    case "JoinPlayer":
                        string PlayerId = m.GetString(0);
                        string PlayerName = m.GetString(1);

                        if (!JoinedPlayer.ContainsKey(PlayerId))
                        {
                            JoinedPlayer.Add(PlayerId, PlayerName);
                            if (PlayerWaiting.Instance != null)
                                PlayerWaiting.Instance.SetPlayer(PlayerId, PlayerName);
                        }
                        break;

                    case "PlayerLeft":
                        // remove characters from the scene when they leave
                        //Debug.Log("player left");
                        string PlayerId1 = m.GetString(0);
                        if (JoinedPlayer.ContainsKey(PlayerId1))
                        {
                            JoinedPlayer.Remove(PlayerId1);
                            if (PlayerWaiting.Instance != null)
                                PlayerWaiting.Instance.RemovePlayer(PlayerId1);

                            setWinner(PlayerId1);
                        }
                        break;

                    case "DisconectPlayer":
                        // remove characters from the scene when they leave
                        //Debug.Log("player left");
                        string playerId2 = m.GetString(0);
                        if (JoinedPlayer.ContainsKey(playerId2))
                        {
                            JoinedPlayer.Remove(playerId2);
                            if (PlayerWaiting.Instance != null)
                                PlayerWaiting.Instance.RemovePlayer(playerId2);

                            setWinner(playerId2);
                        }
                        break;

                    case "NotAllowToJoin":
                        string playerId = m.GetString(0);
                        if (playerId == player.ConnectUserId)
                        {
                            if (GameManager.Instance != null && GameManager.Instance.isPrivatePlay && !isAdmin)
                            {
                                PopupManager.Instance.HidePopup("playerwaiting");
                                GameManager.Instance.ShowMessage("Room already started, please create or join another room");
                            }
                            else
                                CreateRoom();
                        }
                        break;

                    case "levelType":
                        if (GameManager.Instance != null)
                            GameManager.Instance.levelType = m.GetString(0);
                        break;

                    case "GameStart":
                        PlayerWaiting.Instance.HideMe();
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.PlayNewGame_Online(m.GetString(0));
                            GameManager.Instance.requiredPlayer = JoinedPlayer.Count;

                            List<PlayerData> data = GameManager.Instance.players;
                            for (int i = 0; i < JoinedPlayer.Count; i++)
                            {
                                if (i >= GameManager.Instance.requiredPlayer)
                                    break;
                                data[i].gameObject.SetActive(true);
                                data[i].name = JoinedPlayer.ElementAt(i).Key;
                                data[i].playerName = JoinedPlayer.ElementAt(i).Value;
                                data[i].mistakes = 0;
                                data[i].fillAmount = 0;
                                data[i].isWinner = false;
                            }
                        }
                        break;

                    case "Winner":
                        SetWinnerData(m.GetString(0));
                        break;

                    case "Mistakes":
                        if (GameManager.Instance != null)
                            GameManager.Instance.players.Find(x => x.name == m.GetString(0)).mistakes = m.GetInt(1);
                        break;

                    case "Progress":
                        if (GameManager.Instance != null)
                            GameManager.Instance.players.Find(x => x.name == m.GetString(0)).fillAmount = m.GetInt(1);
                        break;

                    case "BotPlayers":
                        string botPlayers = m.GetString(0);
                        MakeBotPlayer(botPlayers);
                        break;

                    case "Timer":
                        int timer = m.GetInt(0);
                        //Debug.Log("=== timer : " + timer);
                        break;
                }
            }
            // clear message queue after it's been processed
            msgList.Clear();
        }

        // Create new room
        public void CreateNewRoom(string roomName, string requiredPlayer, string levelType)
        {
            if (player == null)
            {
                GameManager.Instance.ShowMessage("Unable to connect with server, please try again");
                Authentication(SystemInfo.deviceUniqueIdentifier, "", SocialLogin.Instance.Login_Type);
                return;
            }
            player.Multiplayer.CreateJoinRoom(
                roomName,             //Room id. If set to null a random roomid is used
                roomType,                                   //The room type started on the server
                true,                                         //Should the room be visible in the lobby?
                new Dictionary<string, string> {
                { "maxPlayers", requiredPlayer },
                { "currentPlayers", "0" },
                { "requiredPlayer",requiredPlayer},
                { "levelType",levelType},
                { "playType",GameManager.Instance.isPrivatePlay.ToString() }
                },
                new Dictionary<string, string> {
                { "DeviceId" , SystemInfo.deviceUniqueIdentifier }
                },
                delegate (Connection connection)
                {
                    Debug.Log("room created successfull...");
                    piocon = connection;
                    piocon.OnMessage += handlemessage;
                    isAdmin = true;
                    PopupManager.Instance.Show("playerwaiting");
                    PopupManager.Instance.HidePopup("privateparty");
                    PopupManager.Instance.HidePopup("loading");
                    SendJoinPlayer(player.ConnectUserId, SocialLogin.Name);
                },
                delegate (PlayerIOError error)
                {
                    Debug.Log("Error CreateOrJoin Room: " + error.Message);
                    GameManager.Instance.ShowMessage(error.Message);
                }
            );
        }

        // Joining room
        public void JoinRoom(string roomID)
        {
            if (player == null)
            {
                GameManager.Instance.ShowMessage("Unable to connect with server, please try again");
                Authentication(SystemInfo.deviceUniqueIdentifier, "", SocialLogin.Instance.Login_Type);
                return;
            }
            player.Multiplayer.JoinRoom(
                roomID,
                new Dictionary<string, string> {
                { "DeviceId" , SystemInfo.deviceUniqueIdentifier },
                { "requiredPlayer",GameManager.Instance.requiredPlayer.ToString()},
                { "levelType",GameManager.Instance.levelType},
                { "playType",GameManager.Instance.isPrivatePlay.ToString() }
                },
                delegate (Connection connection)
                {
                //Debug.Log ("room joined successfull...");
                piocon = connection;
                    piocon.OnMessage += handlemessage;
                    isAdmin = false;
                    PopupManager.Instance.Show("playerwaiting");
                    PopupManager.Instance.HidePopup("joinroom");
                    PopupManager.Instance.HidePopup("loading");
                    SendJoinPlayer(player.ConnectUserId, SocialLogin.Name);
                },
                delegate (PlayerIOError error)
                {
                    Debug.Log("Error Joining Room: " + error.ToString());
                    GameManager.Instance.ShowMessage(error.Message);
                }
            );
        }

        public void CreateRoom()
        {
            PopupManager.Instance.Show("loading");
            StartCoroutine(checkInternet((isConnected) =>
            {
                if (isConnected)
                {
                    PollRoomList();
                }
                else
                {
                    GameManager.Instance.ShowMessage("No internet connection!");
                }
            }));
        }

        // allready created room list function
        private void PollRoomList()
        {
            Debug.Log("PollRoomList");
            if (player != null)
            {
                player.Multiplayer.ListRooms(roomType,
                    new Dictionary<string, string>
                    {
                    { "requiredPlayer",GameManager.Instance.requiredPlayer.ToString()},
                    { "levelType",GameManager.Instance.levelType},
                    { "playType",GameManager.Instance.isPrivatePlay.ToString() }
                    }
                    , 20, 0, OnRoomList, delegate (PlayerIOError error)
                    {
                        Debug.Log("Error PollRoomList : " + error.ToString());
                        GameManager.Instance.ShowMessage(error.Message);
                    });
            }
            else
            {
                GameManager.Instance.ShowMessage("Unable to connect with server, please try again");
                Authentication(SystemInfo.deviceUniqueIdentifier, "", SocialLogin.Instance.Login_Type);
            }
        }

        // room information
        void OnRoomList(RoomInfo[] rooms)
        {
            int i = 0;
            Debug.Log("Room count : " + rooms.Length.ToString());
            if (rooms.Length == 0)
            {
                CreateNewRoom(player.ConnectUserId, GameManager.Instance.requiredPlayer.ToString(), GameManager.Instance.levelType);
            }
            else
            {
                JoinRoom(rooms[i].Id);
            }
        }

        private void SendJoinPlayer(string playerId, string playerName)
        {
            if (piocon != null && piocon.Connected)
                piocon.Send("JoinPlayer", playerId, playerName);
        }

        public void StartGame(string groupid)
        {
            if (piocon != null && piocon.Connected)
                piocon.Send("GameStart", groupid);
        }

        public void SendDisconnect()
        {
            if (piocon != null && piocon.Connected)
                piocon.Send("DisconectPlayer");
        }

        public void SendWinner(string playerId)
        {
            if (piocon != null && piocon.Connected)
                piocon.Send("Winner", playerId);
        }

        public void SendMistakes(string playerId, int mistakes)
        {
            if (piocon != null && piocon.Connected)
                piocon.Send("Mistakes", playerId, mistakes);
        }

        public void SendProgress(int total)
        {
            if (piocon != null && piocon.Connected)
                piocon.Send("Progress", total);
        }

        public void LeaveRoom()
        {
            if (piocon != null && piocon.Connected)
            {
                piocon.Disconnect();
            }
        }

        public void CreateNewUser(string name, string email, LoginType loginType)
        {
            PlayerIO.Authenticate(
                GameID,                                 //Your game id
                "public",                               //Your SimpleUsers connection id
                new Dictionary<string, string> {        //Authentication arguments
                {"register", "true"},               //Register a user
                {"username", name},                 //Username - required
                {"password", password },             //Password - required
                { "email", email },
                { "gender", loginType.ToString() },
                { "birthdate", loginType.ToString() }
                },
                null,                                   //PlayerInsight segments
                delegate (Client client)
                {
                //Success!
                Debug.Log("user registered success");
                    player = client;
                },
                delegate (PlayerIOError error)
                {
                //Error registering.
                //Check error.Message to find out in what way it failed,
                //if any registration data was missing or invalid, etc.
                Debug.Log("ERROR_NewUser Reg. : " + error.Message);
                //MainScreen.Instance.ShowToastMsg(error.Message, Color.red);
            }
            );
        }

        public void Authentication(string deviceID, string email, LoginType loginType, string accessToken = null)
        {
            var _email = string.IsNullOrEmpty(email) ? deviceID + "@gmail.com" : email;
            email = _email;
            Debug.Log("=== EMAIL :" + email);

            if (accessToken == null)
            {
                PlayerIO.Authenticate(
                    GameID,                                 //Your game id
                    "public",                               //Your SimpleUsers connection id
                    new Dictionary<string, string> {        //Authentication arguments
                {"username", deviceID},             //Username - either this or email, or both                
                {"password", password}              //Password - required
                    },
                    null,                                   //PlayerInsight segments
                    delegate (Client client)
                    {
                    //Success!
                    Debug.Log("authentication success : " + client.ConnectUserId);
                        player = client;
                    },
                    delegate (PlayerIOError error)
                    {
                    //Error authenticating.
                    Debug.Log("ERROR_Authentication : " + error.Message);
                        if (PlayerWaiting.Instance != null)
                        {
                            GameManager.Instance.ShowMessage(error.Message);
                        }
                        if (error.Message.StartsWith("UnknownUser"))
                        {
                            CreateNewUser(deviceID, email, loginType);
                        }
                    }
                );
            }
            else
            {
                FB_Authentication(accessToken, email);
            }
        }

        public void FB_Authentication(string accessToken, string email)
        {
            PlayerIO.Authenticate(
                GameID,            //Your game id
                "publicfb",                               //Your connection id
                new Dictionary<string, string> {        //Authentication arguments                
                {"accessToken", accessToken},  //Access token
                { "email",email},
                },
            null,                                   //PlayerInsight segments
            delegate (Client client)
            {
            //Success!
            player = client;
                Debug.Log("FB authentication success");
            },
            delegate (PlayerIOError error)
            {
            //Error authenticating.
            Debug.Log("FB Auth Error : " + error.Message);
            });
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

        private void SetWinnerData(string playerId)
        {
            if (winners.Contains(playerId))
                return;

            winners.Add(playerId);
            GameManager.Instance.players.Find(x => x.name == playerId).txtMistakes.text = "Winner" + winners.Count;
            GameManager.Instance.players.Find(x => x.name == playerId).isWinner = true;

            if (winners.Count == GameManager.Instance.requiredPlayer - 1)
            {
                if (player.ConnectUserId == playerId)
                {
                    //GameManager.Instance.ActivePuzzleCompleted();
                    //SoundManager.Instance.Play("puzzle-complete");
                }
                else
                {
                    //loose
                    PopupManager.Instance.Show("fail_dialog");
                    GameManager.Instance.players.Find(x => x.name == player.ConnectUserId).txtMistakes.text = "Loose";
                    AdsManager.Instance.ShowInterstitial();
                }
            }
            else
            {
                if (player.ConnectUserId == playerId)
                {
                    GameManager.Instance.ActivePuzzleCompleted();
                    SoundManager.Instance.Play("puzzle-complete");
                }
            }

            //remove joined player on win, when winner is bot player
            if (JoinedPlayer.ContainsKey(playerId) && playerId.StartsWith("bot"))
            {
                JoinedPlayer.Remove(playerId);
            }
        }

        // set winner after player left, if player type is four.
        private void setWinner(string playerId)
        {
            //Debug.Log("okkk : " + JoinedPlayer.Count + "\t winner cnt : " + winners.Count);
            if (JoinedPlayer.Count == 1 && ScreenManager.Instance.CurrentScreenId == "game"
                && winners.Count < GameManager.Instance.requiredPlayer - 1)
            {
                winners.Add(player.ConnectUserId);
                GameManager.Instance.ActivePuzzleCompleted();
                GameManager.Instance.players.Find(x => x.name == player.ConnectUserId).txtMistakes.text = "Winner" + winners.Count;
                //GameManager.Instance.players.Find(x => x.name == playerId).txtMistakes.text = "Looser";
            }
        }

        private IEnumerator IE_MakeBotPlayer(string strPlayers)
        {
            string[] botPlayers = strPlayers.Split(',');
            for (int i = 0; i < botPlayers.Length; i++)
            {
                yield return new WaitForSeconds(1f);
                int index = int.Parse(botPlayers[i]);
                string botName = botNames[index];

                if (!JoinedPlayer.ContainsKey(botName))
                {
                    JoinedPlayer.Add("bot_" + botName, botName);
                    PlayerWaiting.Instance.SetPlayer("bot_" + botName, botName);
                }
            }
            yield return new WaitForSeconds(1f);
            if (isAdmin)
                StartGame(GameManager.Instance.levelType);
        }

        private void MakeBotPlayer(string strPlayers)
        {
            StartCoroutine(IE_MakeBotPlayer(strPlayers));
        }
    }
}