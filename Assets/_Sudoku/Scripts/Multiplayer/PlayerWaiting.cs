using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class PlayerWaiting : MonoBehaviour
    {
        public static PlayerWaiting Instance;

        public GameObject MyProfile;
        public List<GameObject> Opponents;

        [SerializeField] private Text levelType;
        [SerializeField] private Text waitingMsg;
        [SerializeField] private Text roomCode;
        [SerializeField] private GameObject PlayBtn;
        [SerializeField] private GameObject shareBtn;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            Invoke("setMyProfile", 0.2f);
            ResetData();
        }

        private void Update()
        {
            if (levelType != null && GameManager.Instance != null)
                levelType.text = "MODE : " + GameManager.Instance.levelType;
        }

        private void setMyProfile()
        {
            MyProfile.GetComponentInChildren<Text>().text = SocialLogin.Name;
            MyProfile.transform.Find("Image/Picture").GetComponent<Image>().sprite = GameManager.Instance.playerImage.sprite; ;
            levelType.text = "MODE : " + GameManager.Instance.levelType;
            if (GameManager.Instance.isPrivatePlay)
            {
                roomCode.gameObject.SetActive(true);
                roomCode.text = "ROOM CODE : " + GameManager.Instance.roomCode;
                shareBtn.SetActive(true);
            }
            else
            {
                roomCode.gameObject.SetActive(false);
                shareBtn.SetActive(false);
            }
        }

        public void SetPlayer(string playerId, string playerName)
        {
            if (string.IsNullOrEmpty(playerId) || string.IsNullOrEmpty(playerName) || playerId == ServerCode.player.ConnectUserId)
                return;

            int players = ServerCode.Instance.JoinedPlayer.Count;
            if (players >= GameManager.Instance.requiredPlayer && ServerCode.Instance.isAdmin)
            {
                if (GameManager.Instance.isOnlinePlay && GameManager.Instance.isPrivatePlay)
                {
                    PlayBtn.SetActive(true);
                }
            }
            if (players >= 2)
            {
                waitingMsg.text = "";
                Opponents[players - 2].SetActive(true);
                Opponents[players - 2].name = playerId.ToString();
                Opponents[players - 2].GetComponentInChildren<Text>().text = playerName;
            }
        }

        //remove player from player waiting screen.
        public void RemovePlayer(string playerId)
        {
            PlayBtn.SetActive(false);
            int players = ServerCode.Instance.JoinedPlayer.Count;
            if (players < 2)
                waitingMsg.text = "Waiting for opponent player...";

            for (int i = 0; i < Opponents.Count; i++)
            {
                if (Opponents[i].name == playerId)
                {
                    Opponents[i].name = "Player" + i + 1;
                    Opponents[i].GetComponentInChildren<Text>().text = "Player" + i + 1;
                    Opponents[i].SetActive(false);
                }
            }
        }

        public void Play()
        {
            ServerCode.Instance.StartGame(GameManager.Instance.levelType);
        }

        public void HideMe()
        {
            GetComponent<Popup>().Hide(true);
        }

        private void ResetData()
        {
            ServerCode.Instance.JoinedPlayer.Clear();
            ServerCode.Instance.winners.Clear();
            waitingMsg.text = "Waiting for opponent player...";
            roomCode.text = "";

            PlayBtn.SetActive(false);

            for (int i = 0; i < Opponents.Count; i++)
            {
                Opponents[i].name = "Player" + i + 1;
                Opponents[i].GetComponentInChildren<Text>().text = "Player" + i + 1;
                Opponents[i].SetActive(false);
            }
        }

        public void ShareRoomCode()
        {
            NativeShare share = new NativeShare();
            share.SetSubject("Share Room Code").SetTitle("Share").SetText("I want to play Sudoku with you! Please install from Play Store : https://play.google.com/store/apps/details?id=" + Application.identifier + " Start game and go to Private Play and enter Room Code : " + GameManager.Instance.roomCode).Share();
        }
    }
}