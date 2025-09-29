using UnityEngine;
using UnityEngine.UI;

namespace ArtboxGames
{
	[RequireComponent(typeof(Button))]
	public class OpenLinkButton : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private string url = "";

		#endregion

		#region Unity Methods

		private void Start()
		{
			gameObject.GetComponent<Button>().onClick.AddListener(() => { Application.OpenURL(url); });
		}

		#endregion
	}
}
