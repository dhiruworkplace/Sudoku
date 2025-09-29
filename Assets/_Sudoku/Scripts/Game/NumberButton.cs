using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
	public class NumberButton : ClickableListItem
	{
		#region Inspector Variables

		[SerializeField] private Text numberText = null;
		public int number = 0;

		#endregion

		#region Member Variables

		private CanvasGroup cg;

		#endregion

		#region Public Methods

		public void Setup(int number)
		{
			numberText.text = number.ToString();
			this.number = number;
			SetVisible(true);
		}

		public void SetVisible(bool isVisible)
		{
			if (cg == null)
			{
				cg = gameObject.GetComponent<CanvasGroup>();

				if (cg == null)
				{
					cg = gameObject.AddComponent<CanvasGroup>();
				}
			}

			cg.alpha			= isVisible ? 1 : 0;
			cg.interactable	= isVisible;
			cg.blocksRaycasts	= isVisible;
		}

        public void PointerDown() {
            transform.localScale = Vector3.one * 0.9f;
        }

        public void PointerUp() {
            transform.localScale = Vector3.one;
        }

        #endregion
    }
}
