using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
	public class MainScreen : Screen
	{
		#region Inspector Variables

		//[Space]

		//[SerializeField] private GameObject	continueButton			= null;
		//[SerializeField] private Text		continueDifficultyText	= null;
		//[SerializeField] private Text		continueTimeText		= null;

		#endregion

		#region Public Methods

		public override void Show(bool back, bool immediate)
		{
			base.Show(back, immediate);

			//continueButton.SetActive(GameManager.Instance.ActivePuzzleData != null);
            /*
			if (GameManager.Instance.ActivePuzzleData != null)
			{
				continueDifficultyText.text	= "Difficulty: " + GameManager.Instance.ActivePuzzleDifficultyStr;
				continueTimeText.text		= "Time: " +GameManager.Instance.ActivePuzzleTimeStr;
			}*/
		}

        #endregion

        public void Share() {
            NativeShare share = new NativeShare();
            share.SetSubject("Share").SetTitle("Share").SetText("I realy enjoy this amazing game, try it now : https://play.google.com/store/apps/details?id=" + Application.identifier).Share();
        }
	}
}
