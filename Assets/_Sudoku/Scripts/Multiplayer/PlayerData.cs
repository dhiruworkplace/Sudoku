using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class PlayerData : MonoBehaviour
    {
        public string playerName;
        public int mistakes;
        public int fillAmount;

        public Text txtName;
        public Text txtMistakes;
        public Image fillImage;
        public Image playerImage;

        public bool isWinner = false;

        private int time = 0;

        ServerCode serverCode;

        private void OnEnable()
        {
            Invoke("setBotPlayerData", 0.5f);
        }

        private void Start()
        {
            serverCode = ServerCode.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            if (!serverCode.winners.Contains(name))
            {
                txtName.text = playerName;
                txtMistakes.text = "Mistakes : " + mistakes.ToString();
                if (!name.StartsWith("bot"))
                    fillImage.fillAmount = (float)fillAmount / 81f;
            }
        }

        private void setBotPlayerData()
        {
            if (name.StartsWith("bot"))
            {
                fillImage.fillAmount = (float)GameManager.Instance.players[0].fillAmount / 81f;
                time = serverCode.botTime[UnityEngine.Random.Range(0, serverCode.botTime.Count)];
                Debug.Log("=== time : " + time);
                InvokeRepeating("ManageProgress", 0f, 1f);
            }
            else
            {
                if (ServerCode.player.ConnectUserId == name)
                    playerImage.sprite = GameManager.Instance.playerImage.sprite;
            }
        }

        private void ManageProgress()
        {
            float val = (1f / (float)time);
            fillImage.fillAmount += val;

            if (fillImage.fillAmount >= 1f && serverCode.winners.Count < GameManager.Instance.requiredPlayer - 1
                && !isWinner)
            {
                CancelInvoke("ManageProgress");
                serverCode.SendWinner(name);
            }
            if (isWinner)
            {
                CancelInvoke("ManageProgress");
                fillImage.fillAmount = 1f;
            }

            // send mistakes
            int randomVal = UnityEngine.Random.Range(0, 6);
            if (randomVal == 0)
            {
                mistakes++;
                serverCode.SendMistakes(name, mistakes);
            }
        }

        private void OnDisable()
        {
            CancelInvoke("ManageProgress");
        }
    }
}