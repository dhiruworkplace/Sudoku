using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
	public class GameScreen : Screen
	{
		#region Inspector Variables

		[Space]

		[SerializeField] private Text difficultyText	= null;
		[SerializeField] private Text timeText			= null;

		[Space]
        [SerializeField] private Text difficultyText1 = null;
        [SerializeField] private Text timeText1 = null;
		
		#endregion

		#region Unity Methods		

        private void Update()
		{
			timeText.text = GameManager.Instance.ActivePuzzleTimeStr;
			timeText1.text = GameManager.Instance.ActivePuzzleTimeStr;
		}

		#endregion

		#region Public Methods

		public override void Show(bool back, bool immediate)
		{
			base.Show(back, immediate);

			if (GameManager.Instance.ActivePuzzleData != null)
			{
                difficultyText1.text = GameManager.Instance.ActivePuzzleDifficultyStr;
                difficultyText.text = GameManager.Instance.ActivePuzzleDifficultyStr;
			}
		}

		#endregion		
	}
}