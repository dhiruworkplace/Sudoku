using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
    public class Statistics : MonoBehaviour
    {
        public static Statistics Instance;

        private int _totalplayed;
        private int _totalwin;
        private int _totalonline;

        public int TotalPlayed
        {
            get { return _totalplayed; }
            set
            {
                _totalplayed = value;
                PlayerPrefs.SetInt("totalPlayed", value);
                PlayerPrefs.Save();
            }
        }

        public int TotalWin
        {
            get { return _totalwin; }
            set
            {
                _totalwin = value;
                PlayerPrefs.SetInt("totalWin", value);
                PlayerPrefs.Save();
            }
        }

        public int TotalOnline
        {
            get { return _totalonline; }
            set
            {
                _totalonline = value;
                PlayerPrefs.SetInt("totalOnline", value);
                PlayerPrefs.Save();
            }
        }

        public Text txtTotalPlayed;
        public Text txtTotalWin;
        public Text txtTotalLoose;
        public Text txtWinStreak;
        public Text txtPractice;
        public Text txtOnline;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (!PlayerPrefs.HasKey("totalPlayed"))
                PlayerPrefs.SetInt("totalPlayed", 0);
            else
                TotalPlayed = PlayerPrefs.GetInt("totalPlayed");

            if (!PlayerPrefs.HasKey("totalWin"))
                PlayerPrefs.SetInt("totalWin", 0);
            else
                TotalWin = PlayerPrefs.GetInt("totalWin");

            if (!PlayerPrefs.HasKey("totalOnline"))
                PlayerPrefs.SetInt("totalOnline", 0);
            else
                TotalOnline = PlayerPrefs.GetInt("totalOnline");

            PlayerPrefs.Save();
        }

        public void ShowData()
        {
            txtTotalPlayed.text = _totalplayed.ToString("00");
            txtTotalWin.text = _totalwin.ToString("00");
            int totalLoose = _totalplayed - TotalWin;
            txtTotalLoose.text = totalLoose.ToString("00");
            float winStreak = ((float)_totalwin / (float)_totalplayed);
            winStreak = winStreak >= 0f ? winStreak : 0.0f;
            txtWinStreak.text = (winStreak * 100).ToString("F2") + " %";
            int totalPractice = _totalplayed - _totalonline;
            txtPractice.text = totalPractice.ToString("00");
            txtOnline.text = _totalonline.ToString("00");
        }
    }
}